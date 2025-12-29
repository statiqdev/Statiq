using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Returns pipeline outputs for use within modules during execution.
    /// </summary>
    internal class PhaseOutputs : IPipelineOutputs
    {
        private readonly IReadOnlyDictionary<string, PhaseResult[]> _phaseResults;
        private readonly PipelinePhase _currentPhase;
        private readonly IPipelineCollection _pipelines;

        // Cache the dependency calculation
        private readonly Lazy<Dictionary<string, DocumentList<IDocument>>> _cachedDependencyOutputs;

        public PhaseOutputs(IReadOnlyDictionary<string, PhaseResult[]> phaseResults, PipelinePhase currentPhase, IPipelineCollection pipelines)
        {
            _phaseResults = phaseResults.ThrowIfNull(nameof(phaseResults));
            _currentPhase = currentPhase.ThrowIfNull(nameof(currentPhase));
            _pipelines = pipelines.ThrowIfNull(nameof(pipelines));

            _cachedDependencyOutputs = new Lazy<Dictionary<string, DocumentList<IDocument>>>(
                () => GetOutputsFromDependencies(), LazyThreadSafetyMode.ExecutionAndPublication);
        }

        public DocumentList<IDocument> FromPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);
            ValidatePipeline(pipelineName);

            return _cachedDependencyOutputs.Value.TryGetValue(pipelineName, out DocumentList<IDocument> outputs)
                ? outputs : new DocumentList<IDocument>(Array.Empty<IDocument>());
        }

        public IReadOnlyDictionary<string, DocumentList<IDocument>> ByPipeline()
        {
            ValidateCurrent();

            return _cachedDependencyOutputs.Value;
        }

        public DocumentList<IDocument> ExceptPipeline(string pipelineName)
        {
            ValidateCurrent();
            ValidateArguments(pipelineName);

            return _cachedDependencyOutputs.Value
                .Where(x => !x.Key.Equals(pipelineName, StringComparison.OrdinalIgnoreCase))
                .SelectMany(x => x.Value)
                .ToDocumentList();
        }

        public IEnumerator<IDocument> GetEnumerator()
        {
            ValidateCurrent();

            return _cachedDependencyOutputs.Value.SelectMany(x => x.Value).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        private Dictionary<string, DocumentList<IDocument>> GetOutputsFromDependencies()
        {
            Dictionary<string, DocumentList<IDocument>> outputs = new Dictionary<string, DocumentList<IDocument>>(StringComparer.OrdinalIgnoreCase);

            // If we're in a deployment pipeline, add outputs from all non-deployment output phases
            // Not all non-deployment pipelines may be executing though, so do a check to be sure we actually have outputs
            if (_currentPhase.Pipeline.Deployment)
            {
                outputs.AddRange(_pipelines.AsEnumerable()
                    .Where(x => !x.Value.Deployment && _phaseResults.ContainsKey(x.Key))
                    .Select(x => KeyValuePair.Create(x.Key, GetOutputs(_phaseResults[x.Key], Phase.Output))));
            }

            // If we're in the process phase, get outputs from the process phase only from dependencies
            // Only add from same deployment value pipelines (if a deployment pipeline, non-deployment phases were added above)
            if (_currentPhase.Phase == Phase.Process)
            {
                HashSet<string> transitiveProcessDependencies = GatherPipelineDependencies(_currentPhase, Phase.Process);
                outputs.AddRange(_phaseResults
                    .Where(x => _pipelines[x.Key].Deployment == _currentPhase.Pipeline.Deployment)
                    .Where(x => transitiveProcessDependencies.Contains(x.Key))
                    .Select(x => KeyValuePair.Create(x.Key, GetOutputs(x.Value, Phase.Process))));
            }

            // If we're in the post-process phase, get outputs from all process phases including this one
            // Only add from same deployment value pipelines (if a deployment pipeline, non-deployment phases were added above)
            if (_currentPhase.Phase == Phase.PostProcess)
            {
                outputs.AddRange(_phaseResults
                    .Where(x => _pipelines[x.Key].Deployment == _currentPhase.Pipeline.Deployment)
                    .Select(x => KeyValuePair.Create(x.Key, GetOutputs(x.Value, Phase.Process))));

                // Also add other post-process phases if the post-process dependencies flag is set
                // Need to AddOrReplace existing outputs since we've already added some for process phases
                if (_currentPhase.Pipeline.PostProcessHasDependencies)
                {
                    HashSet<string> transitivePostProcessDependencies = GatherPipelineDependencies(_currentPhase, Phase.PostProcess);
                    outputs.AddOrReplaceRange(_phaseResults
                        .Where(x => _pipelines[x.Key].Deployment == _currentPhase.Pipeline.Deployment)
                        .Where(x => transitivePostProcessDependencies.Contains(x.Key))
                        .Select(x => KeyValuePair.Create(x.Key, GetOutputs(x.Value, Phase.PostProcess))));
                }
            }

            // If we're in the output phase non-deployment outputs were added above
            // (which should only happen if this is a deployment pipeline, otherwise we would have thrown during checks)
            // Deployment pipelines can't access outputs from other deployment pipelines either

            return outputs;
        }

        // Crawl up the phases looking for one with outputs if the one specified below doesn't have any
        private static DocumentList<IDocument> GetOutputs(PhaseResult[] phaseResults, Phase phase)
        {
            int p = (int)phase;
            while (p >= 0 && phaseResults[p] is null)
            {
                p--;
            }
            return p == -1 ? DocumentList<IDocument>.Empty : phaseResults[p].Outputs.ToDocumentList();
        }

        private HashSet<string> GatherPipelineDependencies(
            PipelinePhase currentPhase,
            Phase dependentPhase,
            HashSet<string> transientDependencies = null)
        {
            transientDependencies ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (PipelinePhase dependency in currentPhase.Dependencies.Where(x => x.Phase == dependentPhase))
            {
                transientDependencies.Add(dependency.PipelineName);
                GatherPipelineDependencies(dependency, dependentPhase, transientDependencies);
            }
            return transientDependencies;
        }

        /// <summary>
        /// Validates the pipeline name argument.
        /// </summary>
        private void ValidateArguments(string pipelineName)
        {
            if (string.IsNullOrEmpty(pipelineName))
            {
                throw new ArgumentException(nameof(pipelineName));
            }
            if (!_pipelines.ContainsKey(pipelineName))
            {
                throw new KeyNotFoundException($"The pipeline {pipelineName} could not be found");
            }
        }

        /// <summary>
        /// Validates the requested pipeline.
        /// </summary>
        private void ValidatePipeline(string pipelineName)
        {
            // Make sure it's a real pipeline
            if (!_pipelines.ContainsKey(pipelineName))
            {
                throw new KeyNotFoundException($"The pipeline {pipelineName} could not be found");
            }

            // Make sure the pipeline isn't isolated
            if (_pipelines[pipelineName].Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents in isolated pipeline {pipelineName}");
            }

            // If this is a deployment pipeline and the request pipeline is not, we can access anything
            if (_currentPhase.Pipeline.Deployment && !_pipelines[pipelineName].Deployment)
            {
                return;
            }

            // Make sure this pipeline isn't deployment (which it is if we're in the output phase) and we're asking for a non-deployment output phase
            if ((_currentPhase.Phase == Phase.Input || _currentPhase.Phase == Phase.Output) && _pipelines[pipelineName].Deployment)
            {
                throw new InvalidOperationException($"Cannot access documents in the {_currentPhase} phase from another deployment pipeline {pipelineName}");
            }

            // Make sure we're not accessing our own documents or documents from a non-dependency while in the process phase
            if (_currentPhase.Phase == Phase.Process)
            {
                if (pipelineName.Equals(_currentPhase.PipelineName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Cannot access documents from currently executing pipeline {pipelineName} while in the {nameof(Phase.Process)} phase");
                }
                if (!GatherPipelineDependencies(_currentPhase, Phase.Process).Contains(pipelineName))
                {
                    throw new InvalidOperationException($"Cannot access documents from pipeline {pipelineName} without a dependency while in the {nameof(Phase.Process)} phase");
                }
            }
        }

        /// <summary>
        /// Validates the current pipeline and phase.
        /// </summary>
        private void ValidateCurrent()
        {
            // An isolated pipeline cannot access outputs
            if (_currentPhase.Pipeline.Isolated)
            {
                throw new InvalidOperationException($"Cannot access documents from inside isolated pipeline {_currentPhase.PipelineName}");
            }

            // Outputs cannot be accessed from the input or output phases (unless a deployment pipeline)
            if ((_currentPhase.Phase == Phase.Input || _currentPhase.Phase == Phase.Output) && !_currentPhase.Pipeline.Deployment)
            {
                throw new InvalidOperationException($"Cannot access document outputs during the {_currentPhase.Phase} phase");
            }
        }
    }
}