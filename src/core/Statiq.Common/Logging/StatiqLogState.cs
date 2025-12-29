namespace Statiq.Common
{
    public class StatiqLogState
    {
        public IDocument Document { get; set; }

        public bool LogToBuildServer { get; set; } = true;
    }
}
