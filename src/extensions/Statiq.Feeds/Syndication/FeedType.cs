using System;
using Statiq.Common;
using Statiq.Feeds.Syndication.Atom;
using Statiq.Feeds.Syndication.Rdf;
using Statiq.Feeds.Syndication.Rss;

namespace Statiq.Feeds.Syndication
{
    public class FeedType
    {
        public static readonly FeedType Rdf = new FeedType(
            source => new RdfFeed(source),
            MediaTypes.Get(".rdf") ?? MediaTypes.Xml);

        public static readonly FeedType Rss = new FeedType(
            source => new RssFeed(source),
            MediaTypes.Get(".rss") ?? MediaTypes.Xml);

        public static readonly FeedType Atom = new FeedType(
            source => new AtomFeed(source),
            MediaTypes.Get(".atom") ?? MediaTypes.Xml);

        private readonly Func<IFeed, IFeed> _feedFactory;

        private FeedType(Func<IFeed, IFeed> feedFactory, string mediaType)
        {
            _feedFactory = feedFactory;
            MediaType = mediaType;
        }

        public string MediaType { get; }

        public IFeed GetFeed(IFeed source) => _feedFactory(source);
    }
}