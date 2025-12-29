using System;
using System.Collections.Generic;
using System.Globalization;

namespace Statiq.Common
{
    /// <summary>
    /// Common metadata keys for modules in the core library.
    /// </summary>
    public static class Keys
    {
        // Settings

        /// <summary>
        /// The host to use when generating links.
        /// </summary>
        /// <type cref="string" />
        public const string Host = nameof(Host);

        /// <summary>
        /// Indicates if generated links should use HTTPS instead of HTTP as the scheme.
        /// </summary>
        /// <type cref="bool" />
        public const string LinksUseHttps = nameof(LinksUseHttps);

        /// <summary>
        /// The default root path to use when generating links
        /// (for example, <c>"/virtual/directory"</c>).
        /// Note that you may also want to use the <c>--virtual-dir</c>
        /// argument on the command line when using this setting so that
        /// the preview server serves the site at the same path as the generated links
        /// (for example, <c>--virtual-dir "/virtual/directory"</c>).
        /// </summary>
        /// <type cref="string" />
        public const string LinkRoot = nameof(LinkRoot);

        /// <summary>
        /// Indicates whether to hide index pages (as defined by <see cref="IndexFileName"/>) by default when generating links.
        /// </summary>
        /// <type cref="bool" />
        public const string LinkHideIndexPages = nameof(LinkHideIndexPages);

        /// <summary>
        /// Indicates whether to hide <see cref="PageFileExtensions"/> (usually ".html" and ".htm" extensions) by default when generating links.
        /// </summary>
        /// <type cref="bool" />
        public const string LinkHideExtensions = nameof(LinkHideExtensions);

        /// <summary>
        /// Indicates that links should always be rendered in lowercase.
        /// </summary>
        /// <type cref="bool" />
        public const string LinkLowercase = nameof(LinkLowercase);

        /// <summary>
        /// Indicates that a trailing slash should be appended when hiding a page due to <see cref="LinkHideIndexPages"/>.
        /// </summary>
        /// <type cref="bool" />
        public const string LinkHiddenPageTrailingSlash = nameof(LinkHiddenPageTrailingSlash);

        /// <summary>
        /// Set to <c>true</c> to fail generation on all warnings and errors. The generation will finish but
        /// an exception will be thrown if any warnings or errors occurred during generation.
        /// </summary>
        public const string Strict = nameof(Strict);

        /// <summary>
        /// Indicates whether caching should be used.
        /// </summary>
        /// <type cref="bool" />
        public const string UseCache = nameof(UseCache);

        /// <summary>
        /// The index file name to use for link generation, tree creation, etc. (defaults to "index.html").
        /// </summary>
        /// <type cref="string" />
        public const string IndexFileName = nameof(IndexFileName);

        /// <summary>
        /// The file extensions of "pages" (used by <see cref="IExecutionState.OutputPages"/>
        /// to filter output documents (defaults to "htm" and "html").
        /// </summary>
        /// <type cref="T:byte[]" />
        public const string PageFileExtensions = nameof(PageFileExtensions);

        /// <summary>
        /// Indicates whether to clean the output path on each execution.
        /// </summary>
        /// <type cref="bool" />
        [Obsolete("Use CleanMode instead")]
        public const string CleanOutputPath = nameof(CleanOutputPath);

        /// <summary>
        /// Indicates how to clean the output path on each execution.
        /// </summary>
        /// <type cref="CleanMode" />
        public const string CleanMode = nameof(CleanMode);

        /// <summary>
        /// Allows you to set an alternate date/time that the engine will use as the current date/time.
        /// </summary>
        /// <remarks>
        /// Note that this can result in unexpected behavior when using write times or other variable
        /// date/time values for documents (as Statiq Web does). For example, if you set this value
        /// to a date in the past, any time you edit a file, the write time for that file will be set
        /// to the current time and it will no longer be considered before your set
        /// <see cref="CurrentDateTime"/> value. When using this setting, you should also set explicit
        /// dates for all content where applicable (such as "Published" in Statiq Web).
        /// </remarks>
        /// <type cref="DateTime" />
        public const string CurrentDateTime = nameof(CurrentDateTime);

        /// <summary>
        /// Indicates the culture to use for reading and interpreting dates as input.
        /// </summary>
        /// <type cref="string" />
        /// <type cref="CultureInfo" />
        public const string DateTimeInputCulture = nameof(DateTimeInputCulture);

        /// <summary>
        /// Indicates the culture to use for displaying dates in output.
        /// </summary>
        /// <type cref="string" />
        /// <type cref="CultureInfo" />
        public const string DateTimeDisplayCulture = nameof(DateTimeDisplayCulture);

        /// <summary>
        /// Sets a semantic version range of Statiq Framework that must be used.
        /// </summary>
        /// <type cref="string" />
        public const string MinimumStatiqFrameworkVersion = nameof(MinimumStatiqFrameworkVersion);

        /// <summary>
        /// Specifies analyzers and log levels as "[analyzer]=[log level]" (log level is optional, "All" to set all analyzers).
        /// </summary>
        /// <type cref="string" />
        public const string Analyzers = nameof(Analyzers);

        /// <summary>
        /// The log level at which failures should occur (defaults to LogLevel.Error).
        /// </summary>
        /// <type cref="string" />
        public const string FailureLogLevel = nameof(FailureLogLevel);

        /// <summary>
        /// Usually an error will be generated if the destination of a document falls outside the output folder.
        /// Setting this to <c>true</c> will ignore such errors and should be used when files need to be written
        /// to arbitrary locations on the file system.
        /// </summary>
        public const string IgnoreExternalDestinations = nameof(IgnoreExternalDestinations);

        // Document

        /// <summary>
        /// All the children of this node.
        /// </summary>
        /// <type cref="T:IReadOnlyCollection{IDocument}" />
        public const string Children = nameof(Children);

        // SetDestination

        /// <summary>
        /// The extension to use when setting the destination of a document.
        /// </summary>
        /// <type cref="string" />
        public const string DestinationExtension = nameof(DestinationExtension);

        /// <summary>
        /// The file name to use when setting the destination of a document.
        /// The destination will be set to the given file name at the same
        /// relative path.
        /// </summary>
        /// <type cref="NormalizedPath" />
        public const string DestinationFileName = nameof(DestinationFileName);

        /// <summary>
        /// The path to use when setting the destination of a document.
        /// The specified path can be either relative to the output path or absolute.
        /// </summary>
        /// <type cref="NormalizedPath" />
        public const string DestinationPath = nameof(DestinationPath);

        // GroupDocuments

        /// <summary>
        /// The key for the current group.
        /// </summary>
        /// <type cref="object" />
        public const string GroupKey = nameof(GroupKey);

        // AddIndexes

        /// <summary>
        /// The one-based index of the current document relative to other documents in the pipeline.
        /// </summary>
        /// <type cref="int" />
        public const string Index = nameof(Index);

        // OrderDocuments

        /// <summary>
        /// A loose ordering key used by <c>OrderDocuments</c>.
        /// </summary>
        /// <type cref="int" />
        public const string Order = nameof(Order);

        // GenerateSitemap

        /// <summary>
        /// Contains a document-specific sitemap item for use when generating a sitemap.
        /// </summary>
        /// <type cref="SitemapItem" />
        public const string SitemapItem = nameof(SitemapItem);

        // ReadWeb

        /// <summary>
        /// The URI where the document was downloaded from.
        /// </summary>
        /// <type cref="Uri" />
        public const string SourceUri = nameof(SourceUri);

        /// <summary>
        /// The web headers of the document.
        /// </summary>
        /// <type cref="T:Dictionary{string, string}" />
        public const string SourceHeaders = nameof(SourceHeaders);

        // CreateTree

        /// <summary>
        /// The path that represents this node in the tree.
        /// </summary>
        /// <type cref="Array" />
        public const string TreePath = nameof(TreePath);

        /// <summary>
        /// Gets set on documents that were created as a placeholder for tree roots.
        /// </summary>
        /// <type cref="bool" />
        public const string TreePlaceholder = nameof(TreePlaceholder);

        // AddTitle

        /// <summary>
        /// The calculated title of the document.
        /// </summary>
        /// <type cref="string" />
        public const string Title = nameof(Title);

        // PaginateDocuments

        public const string Next = nameof(Next);

        public const string Previous = nameof(Previous);

        public const string TotalPages = nameof(TotalPages);

        public const string TotalItems = nameof(TotalItems);

        // GenerateRedirects

        /// <summary>
        /// The path(s) where the document should be redirected from.
        /// </summary>
        /// <type cref="NormalizedPath" />
        public const string RedirectFrom = nameof(RedirectFrom);

        /// <summary>
        /// Added to a document created as the source of redirection and contains the path where the redirection is to.
        /// You can check if a document contains this key to see if it's the source of a redirection.
        /// </summary>
        /// <type cref="NormalizedPath" />
        public const string RedirectTo = nameof(RedirectTo);

        /// <summary>
        /// Replaces the default body of meta-refresh redirect HTML files with the specified body
        /// (it will be included raw so don't escape HTML).
        /// </summary>
        public const string RedirectBody = nameof(RedirectBody);

        /// <summary>
        /// Entirely disables cache modules, both during runtime and the persistent cache.
        /// </summary>
        public const string DisableCache = nameof(DisableCache);

        /// <summary>
        /// Resets the cache when set (you generally won't set this as an initial setting, otherwise
        /// the cache will always be reinitialized.
        /// </summary>
        public const string ResetCache = nameof(ResetCache);

        /// <summary>
        /// Indicates that the current metadata object, document, or metadata values should
        /// be excluded from script evaluation typically triggered by a <c>=></c> prefix.
        /// If this value is <c>true</c> no metadata values will be evaluated. If it's a
        /// <see cref="IEnumerable{String}"/> then the indicated metadata values will be
        /// excluded from evaluation.
        /// </summary>
        public const string ExcludeFromEvaluation = nameof(ExcludeFromEvaluation);

        /// <summary>
        /// Holds the current value when enumerating values as documents using the EnumerateValues module.
        /// </summary>
        public const string Current = nameof(Current);

        /// <summary>
        /// The default key used by the EnumerateValues module to locate values to enumerate
        /// within a document.
        /// </summary>
        public const string Enumerate = nameof(Enumerate);

        /// <summary>
        /// Used to indicate that the original input should be output along with the enumerated values
        /// when using the EnumerateValues module.
        /// </summary>
        public const string EnumerateWithInput = nameof(EnumerateWithInput);

        // HTML (these are mirrored in an HtmlKeys class for backwards compatibility)

        /// <summary>
        /// Contains the content of the first result from the query
        /// selector (unless an alternate metadata key is specified).
        /// </summary>
        /// <type cref="string" />
        public const string Excerpt = nameof(Excerpt);

        /// <summary>
        /// Contains the outer HTML of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type cref="string" />
        public const string OuterHtml = nameof(OuterHtml);

        /// <summary>
        /// Contains the inner HTML of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type cref="string" />
        public const string InnerHtml = nameof(InnerHtml);

        /// <summary>
        /// Contains the text content of the query result (unless an alternate metadata key is specified).
        /// </summary>
        /// <type cref="string" />
        public const string TextContent = nameof(TextContent);

        /// <summary>
        /// Documents that represent the headings in each input document.
        /// </summary>
        /// <type cref="T:IReadOnlyList{IDocument}" />
        public const string Headings = nameof(Headings);

        /// <summary>
        /// The value of the <c>id</c> attribute of the current heading document
        /// if the heading contains one.
        /// </summary>
        /// <type cref="string" />
        public const string HeadingId = nameof(HeadingId);

        /// <summary>
        /// The level of the heading of the current heading document.
        /// </summary>
        /// <type cref="int" />
        public const string Level = nameof(Level);
    }
}