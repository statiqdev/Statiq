using System.Collections.Generic;
using System.Threading.Tasks;
using Statiq.Common;
using WebMarkupMin.Core;

namespace Statiq.Minification
{
    /// <summary>
    /// Minifies the CSS content.
    /// </summary>
    /// <remarks>
    /// This module takes the CSS content and uses minification to reduce the output.
    /// </remarks>
    /// <example>
    /// <code>
    /// Pipelines.Add("CSS",
    ///     ReadFiles("*.css"),
    ///     MinifyCss(),
    ///     WriteFiles(".css")
    /// );
    /// </code>
    /// </example>
    /// <category name="Content" />
    public class MinifyCss : MinifierBase, IModule
    {
        private bool _isInlineCode;

        /// <summary>
        /// Minifies the CSS content.
        /// </summary>
        /// <param name="isInlineCode">
        /// Boolean to specify whether the content has inline CSS code. Default value is <c>false</c>.
        /// </param>
        public MinifyCss(bool isInlineCode = false)
        {
            // https://github.com/Taritsyn/WebMarkupMin/wiki/Built-in-CSS-minifiers
            _isInlineCode = isInlineCode;
        }

        /// <summary>
        /// Flag for whether the content has inline CSS code.
        /// </summary>
        /// <param name="isInlineCode">Default value is <c>true</c>.</param>
        /// <returns>The current instance.</returns>
        public MinifyCss IsInlineCode(bool isInlineCode = true)
        {
            _isInlineCode = isInlineCode;
            return this;
        }

        /// <inheritdoc />
        public async Task<IEnumerable<IDocument>> ExecuteAsync(IExecutionContext context)
        {
            KristensenCssMinifier minifier = new KristensenCssMinifier();

            return await MinifyAsync(context, (x) => minifier.Minify(x, _isInlineCode), "CSS");
        }
    }
}