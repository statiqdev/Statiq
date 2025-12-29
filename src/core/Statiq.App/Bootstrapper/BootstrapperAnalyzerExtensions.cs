using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.App
{
    public static class BootstrapperAnalyzerExtensions
    {
        public static TBootstrapper ConfigureAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper, Action<IAnalyzerCollection> analyzersAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureEngine(x => analyzersAction.ThrowIfNull(nameof(analyzersAction))(x.Analyzers));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, string name, IAnalyzer analyzer)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, analyzer));

        // Below extensions match up with IAnalyzerCollectionExtensions

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, IAnalyzer analyzer)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(analyzer));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, Type analyzerType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(analyzerType));

        public static Bootstrapper AddAnalyzer<TAnalyzer>(this Bootstrapper bootstrapper)
            where TAnalyzer : IAnalyzer =>
            bootstrapper.ConfigureAnalyzers(x => x.Add<TAnalyzer>());

        public static Bootstrapper AddAnalyzer<TAnalyzer>(this Bootstrapper bootstrapper, string name)
            where TAnalyzer : IAnalyzer =>
            bootstrapper.ConfigureAnalyzers(x => x.Add<TAnalyzer>(name));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, string name, Type analyzerType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, analyzerType));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, string name, LogLevel logLevel, IAnalyzer analyzer)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, logLevel, analyzer));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, LogLevel logLevel, IAnalyzer analyzer)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(logLevel, analyzer));

        public static TBootstrapper AddAnalyzer<TBootstrapper>(this TBootstrapper bootstrapper, LogLevel logLevel, Type analyzerType)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(logLevel, analyzerType));

        public static Bootstrapper AddAnalyzer<TAnalyzer>(this Bootstrapper bootstrapper, LogLevel logLevel)
            where TAnalyzer : IAnalyzer =>
            bootstrapper.ConfigureAnalyzers(x => x.Add<TAnalyzer>(logLevel));

        public static Bootstrapper AddAnalyzer<TAnalyzer>(this Bootstrapper bootstrapper, string name, LogLevel logLevel)
            where TAnalyzer : IAnalyzer =>
            bootstrapper.ConfigureAnalyzers(x => x.Add<TAnalyzer>(name, logLevel));

        // DelegateAnalyzer

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            IEnumerable<KeyValuePair<string, Phase>> pipelinePhases,
            Func<IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, logLevel, pipelinePhases, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            IEnumerable<KeyValuePair<string, Phase>> pipelinePhases,
            Action<IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, logLevel, pipelinePhases, analyzeAction));

        public static TBootstrapper AnalyzeDocument<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            IEnumerable<KeyValuePair<string, Phase>> pipelinePhases,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.AddDocument(name, logLevel, pipelinePhases, analyzeFunc));

        public static TBootstrapper AnalyzeDocument<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            IEnumerable<KeyValuePair<string, Phase>> pipelinePhases,
            Action<IDocument, IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.AddDocument(name, logLevel, pipelinePhases, analyzeAction));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Func<IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, logLevel, pipeline, phase, analyzeFunc));

        public static TBootstrapper Analyze<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Action<IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.Add(name, logLevel, pipeline, phase, analyzeAction));

        public static TBootstrapper AnalyzeDocument<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Func<IDocument, IAnalyzerContext, Task> analyzeFunc)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.AddDocument(name, logLevel, pipeline, phase, analyzeFunc));

        public static TBootstrapper AnalyzeDocument<TBootstrapper>(
            this TBootstrapper bootstrapper,
            string name,
            LogLevel logLevel,
            string pipeline,
            Phase phase,
            Action<IDocument, IAnalyzerContext> analyzeAction)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.ConfigureAnalyzers(x => x.AddDocument(name, logLevel, pipeline, phase, analyzeAction));

        // Reflection

        /// <summary>
        /// Adds all analyzers that implement <see cref="IAnalyzer"/> from the specified assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <param name="assembly">The assembly to add analyzers from.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper, Assembly assembly)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            assembly.ThrowIfNull(nameof(assembly));
            foreach (Type analyzerType in bootstrapper.ClassCatalog.GetTypesAssignableTo<IAnalyzer>()
                .Where(x => x.Assembly.Equals(assembly) && x.GetConstructor(Type.EmptyTypes) is object))
            {
                bootstrapper.AddAnalyzer(analyzerType);
            }
            return bootstrapper;
        }

        /// <summary>
        /// Adds all analyzers that implement <see cref="IAnalyzer"/> from the entry assembly.
        /// </summary>
        /// <param name="bootstrapper">The bootstrapper.</param>
        /// <returns>The current bootstrapper.</returns>
        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper)
            where TBootstrapper : IBootstrapper =>
            bootstrapper.AddAnalyzers(Assembly.GetEntryAssembly());

        public static Bootstrapper AddAnalyzers<TParent>(this Bootstrapper bootstrapper) => bootstrapper.AddAnalyzers(typeof(TParent));

        public static TBootstrapper AddAnalyzers<TBootstrapper>(this TBootstrapper bootstrapper, Type parentType)
            where TBootstrapper : IBootstrapper
        {
            bootstrapper.ThrowIfNull(nameof(bootstrapper));
            parentType.ThrowIfNull(nameof(parentType));
            foreach (Type analyzerType in parentType.GetNestedTypes()
                .Where(x => typeof(IAnalyzer).IsAssignableFrom(x) && x.GetConstructor(Type.EmptyTypes) is object))
            {
                bootstrapper.AddAnalyzer(analyzerType);
            }
            return bootstrapper;
        }
    }
}
