using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Sets the document destination.
    /// </summary>
    /// <remarks>
    /// This module is typically used before <see cref="WriteFiles"/> to set the
    /// <see cref="IDocument.Destination"/> prior to writing the document.
    /// If an extension is provided, this module will change the extension of the
    /// destination path for each input document.
    /// If the metadata keys <c>DestinationPath</c>, <c>DestinationFileName</c>,
    /// or <c>DestinationExtension</c> are set (in that order of precedence), the value
    /// will be used instead of the specified extension. For example, if you have a bunch
    /// of Razor .cshtml files that need to be rendered to .html files but one of them
    /// should be output as a .xml file instead, define the <c>DestinationExtension</c>
    /// metadata value in the front matter of the page.
    /// If a delegate is provided, it takes precedence over the metadata values (but can
    /// use them if needed).
    /// </remarks>
    /// <metadata cref="Keys.DestinationPath" usage="Input" />
    /// <metadata cref="Keys.DestinationFileName" usage="Input" />
    /// <metadata cref="Keys.DestinationExtension" usage="Input" />
    /// <category name="Input/Output" />
    public class SetDestination : ParallelConfigModule<NormalizedPath>
    {
        /// <summary>
        /// Sets the destination of input documents according to the metadata values for
        /// <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// (in that order of precedence). If none of those metadata values are set, the
        /// destination will remain unchanged.
        /// </summary>
        public SetDestination()
            : base(Config.FromDocument(GetCurrentDestinationFromMetadata), true)
        {
        }

        /// <summary>
        /// Changes the destination extension of input documents to the default page extension
        /// defined in the setting <see cref="Keys.PageFileExtensions"/> (which defaults to ".html").
        /// If <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// metadata values are set, those will take precedence.
        /// </summary>
        /// <param name="pageFileExtension">
        /// <c>true</c> to change the extension to the default page file extension,
        /// <c>false</c> to only set the destination from metadata values.
        /// </param>
        public SetDestination(bool pageFileExtension)
            : base(
                pageFileExtension
                    ? Config.FromDocument((doc, ctx) =>
                    {
                        string ext = ctx.Settings.GetPageFileExtensions()[0];
                        NormalizedPath path = GetCurrentDestinationFromMetadata(doc);
                        if (path.IsNull)
                        {
                            path = doc.Destination.IsNull ? NormalizedPath.Null : doc.Destination.ChangeExtension(ext);
                        }
                        return path;
                    })
                    : Config.FromDocument(GetCurrentDestinationFromMetadata),
                true)
        {
        }

        /// <summary>
        /// Changes the destination extension of input documents to the specified extension.
        /// If <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// metadata values are set, those will take precedence.
        /// </summary>
        /// <param name="pathOrExtension">
        /// The path or extension to set the destination to.
        /// If the value starts with a "." then it will be treated as an extension and the existing destination path will be changed (if there is one).
        /// If the value does not start with a "." then it will be treated as a path and the destination will be set to the value.
        /// Use <see cref="SetDestination(Config{NormalizedPath}, bool)"/> for more control.
        /// </param>
        public SetDestination(string pathOrExtension)
            : base(
                pathOrExtension.ThrowIfNull(nameof(pathOrExtension)).StartsWith('.') && new NormalizedPath(pathOrExtension).Segments.Length <= 1
                    ? Config.FromDocument(doc =>
                    {
                        NormalizedPath path = GetCurrentDestinationFromMetadata(doc);
                        if (path.IsNull)
                        {
                            path = doc.Destination.IsNull ? NormalizedPath.Null : doc.Destination.ChangeExtension(pathOrExtension);
                        }
                        return path;
                    })
                    : Config.FromValue<NormalizedPath>(pathOrExtension),
                true)
        {
        }

        /// <summary>
        /// Changes the destination of input documents to that of the delegate.
        /// </summary>
        /// <param name="destination">A delegate that returns a <see cref="NormalizedPath"/> with the desired destination path.</param>
        /// <param name="ignoreMetadata">
        /// If <c>true</c> existing <c>DestinationPath</c>, <c>DestinationFileName</c>, and <c>DestinationExtension</c>
        /// metadata values will be ignored and the provided config will take precedence, otherwise if <c>false</c>
        /// and if <c>DestinationPath</c>, <c>DestinationFileName</c>, or <c>DestinationExtension</c>
        /// metadata values are set, those will take precedence.
        /// </param>
        public SetDestination(Config<NormalizedPath> destination, bool ignoreMetadata = false)
            : base(
                Config.FromDocument(
                    async (doc, ctx) =>
                    {
                        NormalizedPath path = ignoreMetadata ? NormalizedPath.Null : GetCurrentDestinationFromMetadata(doc);
                        if (path.IsNull)
                        {
                            path = await destination.GetValueAsync(doc, ctx);
                        }
                        return path;
                    }),
                true)
        {
            destination.ThrowIfNull(nameof(destination));
        }

        public static NormalizedPath GetCurrentDestinationFromMetadata(IDocument doc)
        {
            NormalizedPath path = doc.GetPath(Keys.DestinationPath);
            if (!path.IsNull)
            {
                return path;
            }
            path = doc.GetPath(Keys.DestinationFileName);
            if (!path.IsNull)
            {
                return doc.Destination.IsNull ? path : doc.Destination.ChangeFileName(path);
            }
            string extension = doc.GetString(Keys.DestinationExtension);
            if (!string.IsNullOrEmpty(extension) && !doc.Destination.IsNull)
            {
                return doc.Destination.ChangeExtension(extension);
            }
            return null;
        }

        protected override Task<IEnumerable<IDocument>> ExecuteConfigAsync(
            IDocument input,
            IExecutionContext context,
            NormalizedPath value)
        {
            if (value.IsNull)
            {
                return Task.FromResult(input.Yield());
            }

            context.LogDebug(
                $"Setting destination of {input.Source.ToDisplayString("null")} to {value.ToDisplayString("null")}");
            return Task.FromResult(input.Clone(value).Yield());
        }
    }
}