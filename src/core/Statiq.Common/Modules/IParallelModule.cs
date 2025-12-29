namespace Statiq.Common
{
    public interface IParallelModule
    {
        /// <summary>
        /// Indicates whether documents will be
        /// processed by this module in parallel.
        /// </summary>
        bool Parallel { get; set; }
    }
}
