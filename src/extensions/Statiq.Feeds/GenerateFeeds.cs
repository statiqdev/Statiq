using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Html;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using Statiq.Feeds.Syndication;
using IDocument = Statiq.Common.IDocument;

namespace Statiq.Feeds
{
    /// <summary>
    /// Generates syndication feeds including RSS, Atom, and RDF.
    /// </summary>
    /// <remarks>
    /// Each input document represents an item in the feed, up to the specified maximum number of
    /// documents. Note that documents will be sorted in descending order of their published date
    /// to correspond with conventional feed behavior. If you want to use a different ordering,
    /// sort the documents before using this module and call <see cref="PreserveOrdering(bool)"/>.
    /// You should also configure the "Host" setting since that's used to create absolute links.
    /// This module outputs a document for each of the selected feed types. Input documents
    /// are not output by this module.
    /// </remarks>
    /// <metadata cref="FeedKeys.Title" usage="Input"/>
    /// <metadata cref="FeedKeys.Description" usage="Input"/>
    /// <metadata cref="FeedKeys.Author" usage="Input"/>
    /// <metadata cref="FeedKeys.Image" usage="Input"/>
    /// <metadata cref="FeedKeys.Copyright" usage="Input"/>
    /// <metadata cref="FeedKeys.Excerpt" usage="Input"/>
    /// <metadata cref="FeedKeys.Published" usage="Input"/>
    /// <metadata cref="FeedKeys.Updated" usage="Input"/>
    /// <metadata cref="FeedKeys.Content" usage="Input"/>
    /// <category name="Content" />
    public class GenerateFeeds : Module
    {
        /// <summary>
        /// The default path for RSS files.
        /// </summary>
        public static readonly NormalizedPath DefaultRssPath = new NormalizedPath("feed.rss");

        /// <summary>
        /// The default path for Atom files.
        /// </summary>
        public static readonly NormalizedPath DefaultAtomPath = new NormalizedPath("feed.atom");

        /// <summary>
        /// The default path for RDF files.
        /// </summary>
        public static readonly NormalizedPath DefaultRdfPath = null;

        private int _maximumItems = 20;
        private bool _preserveOrdering = false;
        private bool _absolutizeLinks = true;
        private NormalizedPath _rssPath = DefaultRssPath;
        private NormalizedPath _atomPath = DefaultAtomPath;
        private NormalizedPath _rdfPath = DefaultRdfPath;

        private string _feedId;
        private string _feedTitle;
        private string _feedDescription;
        private string _feedAuthor;
        private DateTime? _feedPublished;
        private DateTime? _feedUpdated;
        private Uri _feedLink;
        private Uri _feedImageLink;
        private string _feedCopyright;

        private Config<string> _itemId = Config.FromDocument((doc, ctx) => TypeHelper.Convert<string>(ctx.GetLink(doc, true)));
        private Config<string> _itemTitle = Config.FromDocument(doc => doc.GetString(FeedKeys.Title));
        private Config<string> _itemDescription = Config.FromDocument(doc => doc.GetString(FeedKeys.Description) ?? doc.GetString(FeedKeys.Excerpt));
        private Config<string> _itemAuthor = Config.FromDocument(doc => doc.GetString(FeedKeys.Author));
        private Config<DateTime?> _itemPublished = Config.FromDocument(doc => doc.Get<DateTime?>(FeedKeys.Published));
        private Config<DateTime?> _itemUpdated = Config.FromDocument(doc => doc.Get<DateTime?>(FeedKeys.Updated));
        private Config<Uri> _itemLink = Config.FromDocument((doc, ctx) => TypeHelper.Convert<Uri>(ctx.GetLink(doc, true)));
        private Config<Uri> _itemImageLink = Config.FromDocument((doc, ctx) => TypeHelper.Convert<Uri>(ctx.GetLink(doc, FeedKeys.Image, true)));
        private Config<string> _itemContent = Config.FromDocument(async doc => doc.GetString(FeedKeys.Content) ?? await doc.GetContentStringAsync());
        private Config<Uri> _itemThreadLink = null;
        private Config<int> _itemThreadCount = null;
        private Config<DateTime?> _itemThreadUpdated = null;

        /// <summary>
        /// Sets how many items the feed will contain. The default value is 20.
        /// Note that documents will be used in the order in which they are input
        /// into this module, so a <c>OrderBy</c> module or similar should be used
        /// to order the documents prior to this module. Use a value of 0 to include
        /// all input documents.
        /// </summary>
        /// <param name="maximumItems">The maximum number of items.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds MaximumItems(int maximumItems)
        {
            _maximumItems = maximumItems;
            return this;
        }

        /// <summary>
        /// By default documents are sorted in descending publish order. Use this
        /// method to preserve their input document ordering instead.
        /// </summary>
        /// <param name="preserveOrdering"><c>true</c> to preserve input ordering, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds PreserveOrdering(bool preserveOrdering = true)
        {
            _preserveOrdering = true;
            return this;
        }

        /// <summary>
        /// Indicates whether relative links in the description and content should be changed to absolute (the default is <c>true</c>).
        /// </summary>
        /// <param name="absolutizeLinks">Whether relative links should be changed to absolute.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds AbsolutizeLinks(bool absolutizeLinks = true)
        {
            _absolutizeLinks = absolutizeLinks;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RSS file. The default behavior is to generate a RSS feed with
        /// a path of "feed.rss".
        /// </summary>
        /// <param name="rssPath">The RSS path. If the value is <c>null</c> no RSS file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRssPath(in NormalizedPath rssPath)
        {
            _rssPath = rssPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated Atom file. The default behavior is to generate a RSS feed with
        /// a path of "feed.atom".
        /// </summary>
        /// <param name="atomPath">The Atom path. If the value is <c>null</c> no Atom file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithAtomPath(in NormalizedPath atomPath)
        {
            _atomPath = atomPath;
            return this;
        }

        /// <summary>
        /// Sets the path to the generated RDF file. The default behavior is not to generate a RDF feed.
        /// </summary>
        /// <param name="rdfPath">The RDF path. If the value is <c>null</c> no RDF file will be generated.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithRdfPath(in NormalizedPath rdfPath)
        {
            _rdfPath = rdfPath;
            return this;
        }

        /// <summary>
        /// Sets the feed identifier. The default value is a link to the site.
        /// </summary>
        /// <param name="feedId">The feed identifier.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedId(string feedId)
        {
            _feedId = feedId;
            return this;
        }

        /// <summary>
        /// Sets the feed title. The default value is the value for the "Title" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedTitle">The feed title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedTitle(string feedTitle)
        {
            _feedTitle = feedTitle;
            return this;
        }

        /// <summary>
        /// Sets the feed description. The default value is the value for the "Description" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedDescription">The feed description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedDescription(string feedDescription)
        {
            _feedDescription = feedDescription;
            return this;
        }

        /// <summary>
        /// Sets the feed author. The default value is the value for the "Author" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedAuthor">The feed author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedAuthor(string feedAuthor)
        {
            _feedAuthor = feedAuthor;
            return this;
        }

        /// <summary>
        /// Sets the feed published time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedPublished">The feed published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedPublished(DateTime? feedPublished)
        {
            _feedPublished = feedPublished;
            return this;
        }

        /// <summary>
        /// Sets the feed updated time. The default value is the current UTC time.
        /// </summary>
        /// <param name="feedUpdated">The feed updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedUpdated(DateTime? feedUpdated)
        {
            _feedUpdated = feedUpdated;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the site link.
        /// </summary>
        /// <param name="feedLink">The feed link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedLink(Uri feedLink)
        {
            _feedLink = feedLink;
            return this;
        }

        /// <summary>
        /// Sets the feed image link. The default value is the value for the "Image" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedImageLink">The feed image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedImageLink(Uri feedImageLink)
        {
            _feedImageLink = feedImageLink;
            return this;
        }

        /// <summary>
        /// Sets the feed copyright. The default value is the value for the "Copyright" key
        /// in the global metadata.
        /// </summary>
        /// <param name="feedCopyright">The feed copyright.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithFeedCopyright(string feedCopyright)
        {
            _feedCopyright = feedCopyright;
            return this;
        }

        /// <summary>
        /// Sets the item identifier. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemId">The item identifier (usually a link, but can be other values).</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemId(Config<string> itemId)
        {
            _itemId = itemId;
            return this;
        }

        /// <summary>
        /// Sets the item title. The default value is the value for the "Title" key
        /// in the input document.
        /// </summary>
        /// <param name="itemTitle">The item title.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemTitle(Config<string> itemTitle)
        {
            _itemTitle = itemTitle;
            return this;
        }

        /// <summary>
        /// Sets the item description. The default value is the value for the "Description" key
        /// in the input document.
        /// </summary>
        /// <param name="itemDescription">The item description.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemDescription(Config<string> itemDescription)
        {
            _itemDescription = itemDescription;
            return this;
        }

        /// <summary>
        /// Sets the item author. The default value is the value for the "Author" key
        /// in the input document.
        /// </summary>
        /// <param name="itemAuthor">The item author.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemAuthor(Config<string> itemAuthor)
        {
            _itemAuthor = itemAuthor;
            return this;
        }

        /// <summary>
        /// Sets the item published time. The default value is the value for the "Published" key
        /// in the input document.
        /// </summary>
        /// <param name="itemPublished">The item published time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemPublished(Config<DateTime?> itemPublished)
        {
            _itemPublished = itemPublished;
            return this;
        }

        /// <summary>
        /// Sets the item updated time. The default value is the value for the "Updated" key
        /// in the input document.
        /// </summary>
        /// <param name="itemUpdated">The item updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemUpdated(Config<DateTime?> itemUpdated)
        {
            _itemUpdated = itemUpdated;
            return this;
        }

        /// <summary>
        /// Sets the item link. The default value is the absolute link to the input document.
        /// </summary>
        /// <param name="itemLink">The item link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemLink(Config<Uri> itemLink)
        {
            _itemLink = itemLink;
            return this;
        }

        /// <summary>
        /// Sets the item image link. The default value is the value for the "Image" key
        /// in the input document.
        /// </summary>
        /// <param name="itemImageLink">The item image link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemImageLink(Config<Uri> itemImageLink)
        {
            _itemImageLink = itemImageLink;
            return this;
        }

        /// <summary>
        /// Sets the content of the item. The default value is the value for the "Content" key
        /// in the input document. If that is undefined, the current document content will be used.
        /// </summary>
        /// <param name="itemContent">The content of the item.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemContent(Config<string> itemContent)
        {
            _itemContent = itemContent;
            return this;
        }

        /// <summary>
        /// Sets the item thread link. By default, no thread link is specified.
        /// </summary>
        /// <param name="itemThreadLink">The item thread link.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadLink(Config<Uri> itemThreadLink)
        {
            _itemThreadLink = itemThreadLink;
            return this;
        }

        /// <summary>
        /// Sets the item thread count. By default, no thread count is specified.
        /// </summary>
        /// <param name="itemThreadCount">The item thread count.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadCount(Config<int> itemThreadCount)
        {
            _itemThreadCount = itemThreadCount;
            return this;
        }

        /// <summary>
        /// Sets the item thread updated. By default, no thread updated time is specified.
        /// </summary>
        /// <param name="itemThreadUpdated">The item thread updated time.</param>
        /// <returns>The current module instance.</returns>
        public GenerateFeeds WithItemThreadUpdated(Config<DateTime?> itemThreadUpdated)
        {
            _itemThreadUpdated = itemThreadUpdated;
            return this;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            // Get the feed
            Feed feed = new Feed
            {
                Id = _feedId ?? context.GetLink(),
                Title = _feedTitle ?? context.Settings.GetString(FeedKeys.Title),
                Description = _feedDescription ?? context.Settings.GetString(FeedKeys.Description),
                Author = _feedAuthor ?? context.Settings.GetString(FeedKeys.Author),
                Published = _feedPublished ?? context.GetCurrentDateTime().ToUniversalTime(),
                Updated = _feedUpdated ?? context.GetCurrentDateTime().ToUniversalTime(),
                Link = _feedLink ?? TypeHelper.Convert<Uri>(context.GetLink()),
                ImageLink = _feedImageLink ?? TypeHelper.Convert<Uri>(context.GetLink(context.Settings, FeedKeys.Image, true)),
                Copyright = _feedCopyright ?? context.Settings.GetString(FeedKeys.Copyright) ?? context.GetCurrentDateTime().ToUniversalTime().Year.ToString()
            };

            // Make sure the title is set to something
            if (feed.Title.IsNullOrEmpty())
            {
                feed.Title = "Feed";
            }

            // Copy the feed metadata to document metadata for the eventual outputs
            MetadataItems metadata = new MetadataItems
            {
                { FeedKeys.Title, feed.Title },
                { FeedKeys.Description, feed.Description },
                { FeedKeys.Author, feed.Author },
                { FeedKeys.Published, feed.Published },
                { FeedKeys.Updated, feed.Updated },
                { FeedKeys.Image, feed.ImageLink },
                { FeedKeys.Copyright, feed.Copyright }
            };

            // Display a warning if no host is specified
            if (!feed.Link.IsAbsoluteUri)
            {
                context.LogWarning("Feed is missing absolute link and will not validate according to the W3C criteria, "
                    + "consider specifying a feed link directly or by using the Host setting");
            }

            // Sort the items and take the maximum items
            List<(IDocument, DateTime?)> items = new List<(IDocument, DateTime?)>();
            if (_preserveOrdering)
            {
                // Preserve the input order, take first so we don't calculate unnecessary published dates
                foreach (IDocument input in _maximumItems <= 0 ? context.Inputs : context.Inputs.Take(_maximumItems))
                {
                    items.Add((input, await _itemPublished.GetValueAsync(input, context)));
                }
            }
            else
            {
                // Order by published date then take the maximum items
                foreach (IDocument input in context.Inputs)
                {
                    items.Add((input, await _itemPublished.GetValueAsync(input, context)));
                }
                IEnumerable<(IDocument, DateTime?)> orderedItems = items.OrderByDescending(x => x.Item2);
                items = _maximumItems <= 0 ? orderedItems.ToList() : orderedItems.Take(_maximumItems).ToList();
            }

            // Add items
            foreach ((IDocument, DateTime?) item in items)
            {
                feed.Items.Add(new FeedItem
                {
                    Id = await _itemId.GetValueAsync(item.Item1, context),
                    Title = await _itemTitle.GetValueAsync(item.Item1, context),
                    Description = await _itemDescription.GetValueAsync(item.Item1, context),
                    Author = await _itemAuthor.GetValueAsync(item.Item1, context),
                    Published = item.Item2,
                    Updated = await _itemUpdated.GetValueAsync(item.Item1, context),
                    Link = await _itemLink.GetValueAsync(item.Item1, context),
                    ImageLink = await _itemImageLink.GetValueAsync(item.Item1, context),
                    Content = await _itemContent.GetValueAsync(item.Item1, context),
                    ThreadLink = await _itemThreadLink.GetValueAsync(item.Item1, context),
                    ThreadCount = await _itemThreadCount.GetValueAsync(item.Item1, context),
                    ThreadUpdated = await _itemThreadUpdated.GetValueAsync(item.Item1, context)
                });
            }

            // Make description and content links absolute
            if (_absolutizeLinks)
            {
                HtmlParser parser = new HtmlParser();
                HtmlMarkupFormatter formatter = new HtmlMarkupFormatter();
                foreach (FeedItem feedItem in feed.Items)
                {
                    string description = MakeLinksAbsolute(parser, formatter, context, feedItem.Description);
                    if (description is object)
                    {
                        feedItem.Description = description;
                    }

                    string content = MakeLinksAbsolute(parser, formatter, context, feedItem.Content);
                    if (content is object)
                    {
                        feedItem.Content = content;
                    }
                }
            }

            // Generate the feeds
            return new[]
            {
                GenerateFeed(FeedType.Rss, feed, metadata, _rssPath, context),
                GenerateFeed(FeedType.Atom, feed, metadata, _atomPath, context),
                GenerateFeed(FeedType.Rdf, feed, metadata, _rdfPath, context)
            }.Where(x => x is object);
        }

        private static string MakeLinksAbsolute(HtmlParser parser, HtmlMarkupFormatter formatter, IExecutionContext context, string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                IHtmlDocument dom = parser.ParseDocument(string.Empty);
                INodeList nodes = parser.ParseFragment(value, dom.Body);
                IEnumerable<IElement> elements = nodes.SelectMany(x => x.Descendents<IElement>().Where(y => y.HasAttribute("href") || y.HasAttribute("src")));
                bool replaced = false;
                foreach (IElement element in elements)
                {
                    replaced = MakeLinkAbsolute(element, "href", context) || replaced;
                    replaced = MakeLinkAbsolute(element, "src", context) || replaced;
                }
                if (replaced)
                {
                    using (StringWriter writer = new StringWriter())
                    {
                        nodes.ToHtml(writer, formatter);
                        return writer.ToString();
                    }
                }
            }
            return null;
        }

        private static bool MakeLinkAbsolute(IElement element, string attribute, IExecutionContext context)
        {
            string value = element.GetAttribute(attribute);
            if (!string.IsNullOrEmpty(value)
                && Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out Uri uri)
                && !uri.IsAbsoluteUri)
            {
                element.SetAttribute(attribute, context.GetLink(value, true));
                return true;
            }
            return false;
        }

        private static IDocument GenerateFeed(FeedType feedType, Feed feed, MetadataItems metadata, NormalizedPath path, IExecutionContext context)
        {
            // Get the output path
            if (path.IsNull)
            {
                return null;
            }
            if (!path.IsRelative)
            {
                throw new ArgumentException("The feed output path must be relative");
            }

            // Generate the feed and document
            using (Stream contentStream = context.GetContentStream())
            {
                FeedSerializer.SerializeXml(feedType, feed, contentStream);
                return context.CreateDocument(path, metadata, context.GetContentProvider(contentStream, feedType.MediaType));
            }
        }
    }
}