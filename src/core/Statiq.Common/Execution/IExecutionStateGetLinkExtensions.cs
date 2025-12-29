namespace Statiq.Common
{
    public static class IExecutionStateGetLinkExtensions
    {
        /// <summary>
        /// Gets a link for the root of the site using the host and root path specified in the settings.
        /// </summary>
        /// <param name="executionState">The execution context.</param>
        /// <returns>A link for the root of the site.</returns>
        public static string GetLink(this IExecutionState executionState) =>
            executionState.GetLink(
                null,
                executionState.Settings.GetString(Keys.Host),
                executionState.Settings.GetString(Keys.LinkRoot),
                executionState.Settings.GetBool(Keys.LinksUseHttps),
                false,
                false);

        /// <summary>
        /// Gets a link for the specified document using the document destination.
        /// This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <remarks>
        /// To add a query and/or fragment to the document link, use <see cref="IDocumentGetLinkExtensions.GetLink(IDocument, string, bool)"/>.
        /// </remarks>
        /// <param name="executionState">The execution context.</param>
        /// <param name="document">The document to generate a link for.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            IDocument document,
            bool includeHost = false) =>
            document.Destination.IsNull ? null : executionState.GetLink(document.Destination.FullPath, includeHost);

        /// <summary>
        /// Gets a link for the specified metadata using the specified metadata value and the default settings from the
        /// configuration. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="metadata">The metadata or document to generate a link for.</param>
        /// <param name="key">The key at which a <see cref="NormalizedPath"/> can be found for generating the link.</param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            IMetadata metadata,
            string key,
            bool includeHost = false) =>
            executionState.GetLink(metadata, key, null, includeHost);

        /// <summary>
        /// Gets a link for the specified metadata using the specified metadata value and the default settings from the
        /// configuration. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="metadata">The metadata or document to generate a link for.</param>
        /// <param name="key">The key at which a <see cref="NormalizedPath"/> can be found for generating the link.</param>
        /// <param name="queryAndFragment">
        /// Appends a query and/or fragment to the URL from the metadata value. If a value is provided for this parameter
        /// and it does not start with "?" or "#" then it will be assumed a query and a "?" will be prefixed.
        /// </param>
        /// <param name="includeHost">
        /// If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            IMetadata metadata,
            string key,
            string queryAndFragment,
            bool includeHost = false)
        {
            if (metadata?.ContainsKey(key) == true)
            {
                // Return the actual URI if it's absolute
                string path = metadata.GetString(key);
                return path is null
                    ? null
                    : executionState.GetLink(executionState.LinkGenerator.AddQueryAndFragment(path, queryAndFragment), includeHost);
            }
            return null;
        }

        /// <summary>
        /// Converts the specified path into a string appropriate for use as a link using default settings from the
        /// configuration. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            bool includeHost = false) =>
            executionState.GetLink(path.FullPath, includeHost);

        /// <summary>
        /// Converts the specified path into a string appropriate for use as a link using default settings from the
        /// configuration. This version should be used inside modules to ensure
        /// consistent link generation. Note that you can optionally include the host or not depending
        /// on if you want to generate host-specific links. By default, the host is not included so that
        /// sites work the same on any server including the preview server.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="includeHost">If set to <c>true</c> the host configured in the output settings will
        /// be included in the link, otherwise the host will be omitted and only the root path will be included (default).</param>
        /// <returns>
        /// A string representation of the path suitable for a web link.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            bool includeHost = false)
        {
            // Return the actual URI if it's absolute
            if (path is object && executionState.LinkGenerator.TryGetAbsoluteHttpUri(path, out string absoluteUri))
            {
                return absoluteUri;
            }

            return executionState.GetLink(
                path,
                includeHost ? executionState.Settings.GetString(Keys.Host) : null,
                executionState.Settings.GetString(Keys.LinkRoot),
                executionState.Settings.GetBool(Keys.LinksUseHttps),
                executionState.Settings.GetBool(Keys.LinkHideIndexPages),
                executionState.Settings.GetBool(Keys.LinkHideExtensions),
                executionState.Settings.GetBool(Keys.LinkLowercase));
        }

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions) =>
            executionState.GetLink(path.FullPath, host, root, useHttps, hideIndexPages, hideExtensions);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions) =>
            executionState.GetLink(path, host, root.FullPath, useHttps, hideIndexPages, hideExtensions);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions) =>
            executionState.GetLink(path.FullPath, host, root.FullPath, useHttps, hideIndexPages, hideExtensions);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions) =>
            executionState.GetLink(
                path,
                host,
                root,
                useHttps,
                hideIndexPages,
                hideExtensions,
                executionState.Settings.GetBool(Keys.LinkLowercase));

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase) =>
            executionState.GetLink(path.FullPath, host, root, useHttps, hideIndexPages, hideExtensions, lowercase);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase) =>
            executionState.GetLink(path, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase) =>
            executionState.GetLink(path.FullPath, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase) =>
            executionState.GetLink(
                path,
                host,
                root,
                useHttps,
                hideIndexPages,
                hideExtensions,
                lowercase,
                true);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute) =>
            executionState.GetLink(path.FullPath, host, root, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute) =>
            executionState.GetLink(path, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute) =>
            executionState.GetLink(path.FullPath, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute) =>
            executionState.GetLink(
                path,
                host,
                root,
                useHttps,
                hideIndexPages,
                hideExtensions,
                lowercase,
                makeAbsolute,
                executionState.Settings.GetBool(Keys.LinkHiddenPageTrailingSlash));

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <param name="hiddenPageTrailingSlash">
        /// Indicates that a trailing slash should be appended when hiding a page due to <paramref name="hideIndexPages" />.
        /// Setting to <c>false</c> means that hiding a page will result in the parent path without a trailing slash.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash) =>
            executionState.GetLink(path.FullPath, host, root, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute, hiddenPageTrailingSlash);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <param name="hiddenPageTrailingSlash">
        /// Indicates that a trailing slash should be appended when hiding a page due to <paramref name="hideIndexPages" />.
        /// Setting to <c>false</c> means that hiding a page will result in the parent path without a trailing slash.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash) =>
            executionState.GetLink(path, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute, hiddenPageTrailingSlash);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <param name="hiddenPageTrailingSlash">
        /// Indicates that a trailing slash should be appended when hiding a page due to <paramref name="hideIndexPages" />.
        /// Setting to <c>false</c> means that hiding a page will result in the parent path without a trailing slash.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            in NormalizedPath path,
            string host,
            in NormalizedPath root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash) =>
            executionState.GetLink(path.FullPath, host, root.FullPath, useHttps, hideIndexPages, hideExtensions, lowercase, makeAbsolute, hiddenPageTrailingSlash);

        /// <summary>
        /// Converts the path into a string appropriate for use as a link, overriding one or more
        /// settings from the configuration.
        /// </summary>
        /// <param name="executionState">The execution state.</param>
        /// <param name="path">The path to generate a link for.</param>
        /// <param name="host">The host to use for the link.</param>
        /// <param name="root">The root of the link. The value of this parameter is prepended to the path.</param>
        /// <param name="useHttps">If set to <c>true</c>, HTTPS will be used as the scheme for the link.</param>
        /// <param name="hideIndexPages">If set to <c>true</c>, index files will be hidden.</param>
        /// <param name="hideExtensions">If set to <c>true</c>, extensions will be hidden.</param>
        /// <param name="lowercase">If set to <c>true</c>, links will be rendered in all lowercase.</param>
        /// <param name="makeAbsolute">
        /// If <paramref name="path"/> is relative, setting this to <c>true</c> (the default value) will assume the path relative from the root of the site
        /// and make it absolute by prepending a slash and <paramref name="root"/> to the path. Otherwise, <c>false</c> will leave relative paths as relative
        /// and won't prepend a slash (but <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/> will have no effect).
        /// If <paramref name="path"/> is absolute, this value has no effect and <paramref name="host"/>, <paramref name="useHttps"/>, and <paramref name="root"/>
        /// will be applied as appropriate.
        /// </param>
        /// <param name="hiddenPageTrailingSlash">
        /// Indicates that a trailing slash should be appended when hiding a page due to <paramref name="hideIndexPages" />.
        /// Setting to <c>false</c> means that hiding a page will result in the parent path without a trailing slash.
        /// </param>
        /// <returns>
        /// A string representation of the path suitable for a web link with the specified
        /// root and hidden file name or extension.
        /// </returns>
        public static string GetLink(
            this IExecutionState executionState,
            string path,
            string host,
            string root,
            bool useHttps,
            bool hideIndexPages,
            bool hideExtensions,
            bool lowercase,
            bool makeAbsolute,
            bool hiddenPageTrailingSlash) =>
            executionState.LinkGenerator.GetLink(
                path,
                host,
                root,
                useHttps ? "https" : null,
                hideIndexPages ? new[] { executionState.Settings.GetIndexFileName() } : null,
                hideExtensions ? executionState.Settings.GetPageFileExtensions() : null,
                lowercase,
                makeAbsolute,
                hiddenPageTrailingSlash);
    }
}