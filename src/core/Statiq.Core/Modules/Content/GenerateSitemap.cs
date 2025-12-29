using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Generates a site map from the input documents.
    /// </summary>
    /// <remarks>
    /// This module generates a site map from the input documents. The output document contains the site map XML as it's content.
    /// You can supply a location for the each item in the site map as a <c>string</c> (with an optional function to format it
    /// into an absolute HTML path) or you can supply a <c>SitemapItem</c> for more control. You can also specify the
    /// <c>Hostname</c> metadata key (as a <c>string</c>) for each input document, which will be prepended to all locations.
    /// </remarks>
    /// <metadata cref="Keys.SitemapItem" usage="Input" />
    /// <category name="Content" />
    public class GenerateSitemap : Module
    {
        private static readonly string[] ChangeFrequencies = { "always", "hourly", "daily", "weekly", "monthly", "yearly", "never" };

        private readonly Config<object> _sitemapItemOrLocation;
        private readonly Func<string, string> _locationFormatter;

        /// <summary>
        /// Creates a site map using the metadata key <c>SitemapItem</c> which should contain either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information. If the key <c>SitemapItem</c> is not found or does not contain the correct type of object,
        /// a link to the document will be used.
        /// </summary>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the <c>SitemapItem</c> metadata key.</param>
        public GenerateSitemap(Func<string, string> locationFormatter = null)
            : this(Config.FromDocument(Keys.SitemapItem), locationFormatter)
        {
        }

        /// <summary>
        /// Creates a site map using the specified metadata key which should contain either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information. If the metadata key is not found or does not contain the correct type of object,
        /// a link to the document will be used.
        /// </summary>
        /// <param name="sitemapItemOrLocationMetadataKey">A metadata key that contains either a <c>SitemapItem</c> or
        /// a <c>string</c> location for each input document.</param>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the specified metadata key.</param>
        public GenerateSitemap(string sitemapItemOrLocationMetadataKey, Func<string, string> locationFormatter = null)
            : this(Config.FromDocument(sitemapItemOrLocationMetadataKey), locationFormatter)
        {
            if (string.IsNullOrEmpty(sitemapItemOrLocationMetadataKey))
            {
                throw new ArgumentException("Argument is null or empty", nameof(sitemapItemOrLocationMetadataKey));
            }
        }

        /// <summary>
        /// Creates a site map using the specified delegate which should return either a <c>string</c> that
        /// contains the location for each input document or a <c>SitemapItem</c> instance with the location
        /// and other information. If the delegate returns <c>null</c> or does not return the correct type of object,
        /// a link to the document will be used.
        /// </summary>
        /// <param name="sitemapItemOrLocation">A delegate that either returns a <c>SitemapItem</c> instance or a <c>string</c>
        /// with the desired item location. If the delegate returns <c>null</c>, the input document is not added to the site map.</param>
        /// <param name="locationFormatter">A location formatter that will be applied to the location of each input after
        /// getting the value of the specified metadata key.</param>
        public GenerateSitemap(Config<object> sitemapItemOrLocation, Func<string, string> locationFormatter = null)
        {
            _sitemapItemOrLocation = sitemapItemOrLocation.ThrowIfNull(nameof(sitemapItemOrLocation));
            _locationFormatter = locationFormatter;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            HashSet<string> locations = new HashSet<string>(); // Remove duplicate locations
            List<string> content = new List<string>();
            content.Add("<?xml version=\"1.0\" encoding=\"UTF-8\"?><urlset xmlns=\"http://www.sitemaps.org/schemas/sitemap/0.9\">");
            foreach (IDocument input in context.Inputs)
            {
                (string formattedLocation, SitemapItem item) = await GetSitemapItemAsync(input, context);
                AddSitemapItemContent(content, formattedLocation, item, locations);
            }
            content.Add("</urlset>");

            // Always output the site map document, even if it's empty
            return context.CreateDocument("sitemap.xml", context.GetContentProvider(() => new StringItemStream(content), MediaTypes.Xml)).Yield();
        }

        private void AddSitemapItemContent(List<string> content, string formattedLocation, SitemapItem sitemapItem, HashSet<string> locations)
        {
            if (!formattedLocation.IsNullOrWhiteSpace() && locations.Add(formattedLocation))
            {
                content.Add($"<url><loc>{formattedLocation}</loc>");
                if (sitemapItem.LastModUtc.HasValue)
                {
                    content.Add($"<lastmod>{sitemapItem.LastModUtc.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")}</lastmod>");
                }
                if (sitemapItem.ChangeFrequency.HasValue)
                {
                    content.Add($"<changefreq>{ChangeFrequencies[(int)sitemapItem.ChangeFrequency.Value]}</changefreq>");
                }
                if (sitemapItem.Priority.HasValue)
                {
                    content.Add($"<priority>{sitemapItem.Priority.Value}</priority>");
                }
                content.Add("</url>");
            }
        }

        private async Task<(string FormattedLocation, SitemapItem Item)> GetSitemapItemAsync(IDocument input, IExecutionContext context)
        {
            // Try to get a SitemapItem
            object delegateResult = await _sitemapItemOrLocation.GetValueAsync(input, context);
            SitemapItem sitemapItem = delegateResult as SitemapItem
                ?? new SitemapItem((delegateResult as string) ?? context.GetLink(input));

            // Add a site map entry if we got an item and valid location
            string location = null;
            if (!string.IsNullOrWhiteSpace(sitemapItem?.Location))
            {
                location = sitemapItem.Location;

                // Apply the location formatter if there is one
                if (_locationFormatter is object)
                {
                    location = _locationFormatter(location);
                }

                // Apply the host name if defined (and the location formatter didn't already set a host name)
                if (!string.IsNullOrWhiteSpace(location)
                    && !location.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                    && !location.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
                {
                    location = context.GetLink(new NormalizedPath(GetLocationWithoutLinkRoot(location, context)), true);
                }
            }
            return (location, sitemapItem);
        }

        private string GetLocationWithoutLinkRoot(string location, IExecutionContext context)
        {
            NormalizedPath root = context.Settings.GetPath(Keys.LinkRoot);
            string rootLink = root.IsNull ? string.Empty : root.FullPath;
            if (rootLink.EndsWith(NormalizedPath.Slash))
            {
                rootLink = rootLink.Substring(0, rootLink.Length - 1);
            }
            if (rootLink.Length > 0 && !rootLink.StartsWith(NormalizedPath.Slash))
            {
                rootLink = NormalizedPath.Slash + rootLink;
            }

            if (rootLink?.Length > 0 && location.StartsWith(rootLink))
            {
                return location.Substring(rootLink.Length);
            }
            else
            {
                return location;
            }
        }
    }
}