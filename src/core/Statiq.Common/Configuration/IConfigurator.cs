namespace Statiq.Common
{
    public interface IConfigurator<TConfigurable>
        where TConfigurable : IConfigurable
    {
        void Configure(TConfigurable configurable);
    }
}
