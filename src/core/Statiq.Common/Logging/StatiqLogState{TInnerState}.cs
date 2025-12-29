namespace Statiq.Common
{
    public class StatiqLogState<TInnerState> : StatiqLogState
    {
        public StatiqLogState(TInnerState innerState)
        {
            InnerState = innerState;
        }

        public TInnerState InnerState { get; }
    }
}
