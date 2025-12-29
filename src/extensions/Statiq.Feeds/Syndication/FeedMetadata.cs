using System;

namespace Statiq.Feeds.Syndication
{
    public abstract class FeedMetadata : IFeedMetadata
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime? Published { get; set; }
        public DateTime? Updated { get; set; }
        public Uri Link { get; set; }
        public Uri ImageLink { get; set; }
    }
}