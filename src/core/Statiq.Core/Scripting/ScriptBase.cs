using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    public abstract class ScriptBase
    {
        protected ScriptBase(IMetadata metadata, IExecutionState executionState, IExecutionContext executionContext)
        {
            Metadata = metadata;
            ExecutionState = executionState;
            Context = executionContext;
        }

        public IExecutionState ExecutionState { get; }

        public IMetadata Metadata { get; }

        public IExecutionContext Context { get; }

#pragma warning disable SA1300 // Element should begin with upper-case letter
        public IExecutionContext ctx => Context;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        public IDocument Document => Metadata as IDocument ?? throw new InvalidOperationException("Script object is not a document");

#pragma warning disable SA1300 // Element should begin with upper-case letter
        public IDocument doc => Document;
#pragma warning restore SA1300 // Element should begin with upper-case letter

        public abstract Task<object> EvaluateAsync();

        // IDocument pass-throughs (these will throw if not a document)

        public NormalizedPath Source => Document.Source;

        public NormalizedPath Destination => Document.Destination;

        public IContentProvider ContentProvider => Document.ContentProvider;

        // Manually implement IExecutionContext pass-throughs since we don't
        // want to automatically proxy everything in IExecutionContext

        public string PipelineName => Context.PipelineName;

        public IReadOnlyPipeline Pipeline => Context.Pipeline;

        public Phase Phase => Context.Phase;

        public IExecutionContext Parent => Context.Parent;

        public IModule Module => Context.Module;

        public ImmutableArray<IDocument> Inputs => Context.Inputs;
    }
}