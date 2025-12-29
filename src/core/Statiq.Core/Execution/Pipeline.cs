using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A base pipeline class.
    /// </summary>
    /// <remarks>
    /// Use the derived constructor to initialize the phases and other properties.
    /// </remarks>
    public class Pipeline : IPipeline
    {
        private ModuleList _inputModules;
        private ModuleList _processModules;
        private ModuleList _transformModules;
        private ModuleList _outputModules;

        /// <inheritdoc/>
        public ModuleList InputModules
        {
            get => _inputModules ?? (_inputModules = new ModuleList());
            protected set => _inputModules = value;
        }

        /// <inheritdoc/>
        public ModuleList ProcessModules
        {
            get => _processModules ?? (_processModules = new ModuleList());
            protected set => _processModules = value;
        }

        /// <inheritdoc/>
        public ModuleList PostProcessModules
        {
            get => _transformModules ?? (_transformModules = new ModuleList());
            protected set => _transformModules = value;
        }

        /// <inheritdoc/>
        public ModuleList OutputModules
        {
            get => _outputModules ?? (_outputModules = new ModuleList());
            protected set => _outputModules = value;
        }

        /// <inheritdoc/>
        public HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public HashSet<string> DependencyOf { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public bool Isolated { get; set; }

        /// <inheritdoc/>
        public bool Deployment { get; set; }

        /// <inheritdoc/>
        public bool PostProcessHasDependencies { get; set; }

        /// <inheritdoc/>
        public ExecutionPolicy ExecutionPolicy { get; set; }

        /// <inheritdoc/>
        IReadOnlyCollection<string> IReadOnlyPipeline.Dependencies => Dependencies;

        /// <inheritdoc/>
        IReadOnlyCollection<string> IReadOnlyPipeline.DependencyOf => DependencyOf;
    }
}