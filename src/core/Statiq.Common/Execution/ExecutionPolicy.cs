namespace Statiq.Common
{
    /// <summary>
    /// Indicates when the pipeline is executed.
    /// </summary>
    public enum ExecutionPolicy
    {
        /// <summary>
        /// The pipeline is normally executed (I.e., it has a deployment policy equivalent to
        /// <see cref="Normal"/>) unless <see cref="IPipeline.Deployment"/> is <c>true</c> in
        /// which case the pipeline is manually executed (I.e., it has a deployment policy
        /// equivalent to <see cref="Manual"/>).
        /// </summary>
        Default,

        /// <summary>
        /// The pipeline is normally executed.
        /// </summary>
        Normal,

        /// <summary>
        /// The pipeline is not normally executed unless explicitly specified
        /// or as a dependency of an executing pipeline.
        /// </summary>
        Manual,

        /// <summary>
        /// The pipeline is always executed, regardless of explicitly specified pipelines.
        /// </summary>
        Always
    }
}
