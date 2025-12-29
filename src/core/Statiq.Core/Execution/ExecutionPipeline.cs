using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// A base pipeline that runs code for each execution phase.
    /// </summary>
    public abstract class ExecutionPipeline : Common.Module, IPipeline
    {
        protected ExecutionPipeline()
        {
            Dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            DependencyOf = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (GetType()
                .GetMethod(nameof(ExecuteInputPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                ((IPipeline)this).InputModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteProcessPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                ((IPipeline)this).ProcessModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteTransformPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                ((IPipeline)this).PostProcessModules.Add(this);
            }
            if (GetType()
                .GetMethod(nameof(ExecuteOutputPhaseAsync), BindingFlags.Instance | BindingFlags.NonPublic)
                .DeclaringType != typeof(ExecutionPipeline))
            {
                ((IPipeline)this).OutputModules.Add(this);
            }
        }

        /// <inheritdoc/>
        ModuleList IPipeline.InputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        ModuleList IPipeline.ProcessModules { get; } = new ModuleList();

        /// <inheritdoc/>
        ModuleList IPipeline.PostProcessModules { get; } = new ModuleList();

        /// <inheritdoc/>
        ModuleList IPipeline.OutputModules { get; } = new ModuleList();

        /// <inheritdoc/>
        public virtual HashSet<string> Dependencies { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        public virtual HashSet<string> DependencyOf { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        /// <inheritdoc/>
        IReadOnlyCollection<string> IReadOnlyPipeline.Dependencies => Dependencies;

        /// <inheritdoc/>
        IReadOnlyCollection<string> IReadOnlyPipeline.DependencyOf => DependencyOf;

        /// <inheritdoc/>
        public virtual bool Isolated { get; set; }

        /// <inheritdoc/>
        public virtual bool Deployment { get; set; }

        /// <inheritdoc/>
        public virtual bool PostProcessHasDependencies { get; set; }

        /// <inheritdoc/>
        public virtual ExecutionPolicy ExecutionPolicy { get; set; }

        protected sealed override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context) =>
            context.Phase switch
            {
                Phase.Input => ExecuteInputPhaseAsync(context),
                Phase.Process => ExecuteProcessPhaseAsync(context),
                Phase.PostProcess => ExecuteTransformPhaseAsync(context),
                Phase.Output => ExecuteOutputPhaseAsync(context),
                _ => base.ExecuteContextAsync(context),
            };

        /// <inheritdoc />
        // Unused, prevent overriding in derived classes
        protected sealed override Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context) =>
            throw new NotSupportedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteInputPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteProcessPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteTransformPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();

        protected virtual Task<IEnumerable<IDocument>> ExecuteOutputPhaseAsync(IExecutionContext context) =>
            throw new NotImplementedException();
    }
}