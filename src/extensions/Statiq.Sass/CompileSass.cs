using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SharpScss;
using Statiq.Common;

namespace Statiq.Sass
{
    /// <summary>
    /// Compiles Sass CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The content of the input document is compiled to CSS and the content
    /// of the output document contains the compiled CSS stylesheet.
    /// </para>
    /// <para>
    /// Any metadata prefixed with "Sass_" or "Sass-" (case-insensitive) will be
    /// injected into the beginning of the Sass content as variables. Any spaces will
    /// be converted to "-" and the "Sass_" or "Sass-" prefix will be removed for the
    /// Sass variable name. Note that because these are injected at the top of the Sass
    /// content, and because the order of metadata is undefined, variables defined this
    /// way should not use other variables in their values. Using the <c>!default</c>
    /// syntax in Sass files where metadata variables might override the ones in the
    /// Sass file is one technique for working with metadata-defined Sass variables.
    /// </para>
    /// </remarks>
    /// <category name="Templates" />
    public class CompileSass : ParallelModule
    {
        private readonly List<NormalizedPath> _includePaths = new List<NormalizedPath>();
        private Config<NormalizedPath> _inputPath = Config.FromDocument(DefaultInputPath);
        private Func<string, string> _importPathFunc = null;
        private bool _includeSourceComments = false;
        private ScssOutputStyle _outputStyle = ScssOutputStyle.Compact;
        private bool _generateSourceMap = false;

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the <see cref="IDocument.Source"/>
        /// value is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="NormalizedPath"/>.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithInputPath(Config<NormalizedPath> inputPath)
        {
            _inputPath = inputPath.ThrowIfNull(nameof(inputPath));
            return this;
        }

        /// <summary>
        /// Adds a list of paths to search while processing includes.
        /// </summary>
        /// <param name="paths">The paths to include.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithIncludePaths(params NormalizedPath[] paths)
        {
            _includePaths.AddRange(paths);
            return this;
        }

        /// <summary>
        /// A delegate that processes the path in <c>@import</c> statements.
        /// </summary>
        /// <param name="importPathFunc">A delegate that should return the correct import path for a given import.</param>
        /// <returns>The current instance.</returns>
        public CompileSass WithImportPath(Func<string, string> importPathFunc)
        {
            _importPathFunc = importPathFunc;
            return this;
        }

        /// <summary>
        /// Sets whether the source comments are included (by default they are not).
        /// </summary>
        /// <param name="includeSourceComments"><c>true</c> to include source comments.</param>
        /// <returns>The current instance.</returns>
        public CompileSass IncludeSourceComments(bool includeSourceComments = true)
        {
            _includeSourceComments = includeSourceComments;
            return this;
        }

        /// <summary>
        /// Sets the output style to compact.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithCompactOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compact;
            return this;
        }

        /// <summary>
        /// Sets the output style to expanded.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithExpandedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Expanded;
            return this;
        }

        /// <summary>
        /// Sets the output style to compressed.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithCompressedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Compressed;
            return this;
        }

        /// <summary>
        /// Sets the output style to nested.
        /// </summary>
        /// <returns>The current instance.</returns>
        public CompileSass WithNestedOutputStyle()
        {
            _outputStyle = ScssOutputStyle.Nested;
            return this;
        }

        /// <summary>
        /// Specifies whether a source map should be generated (the default
        /// behavior is <c>false</c>).
        /// </summary>
        /// <param name="generateSourceMap"><c>true</c> to generate a source map.</param>
        /// <returns>The current instance.</returns>
        public CompileSass GenerateSourceMap(bool generateSourceMap = true)
        {
            _generateSourceMap = generateSourceMap;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            context.LogDebug($"Processing Sass for {input.ToSafeDisplayString()}");

            NormalizedPath inputPath = await _inputPath.GetValueAsync(input, context);
            if (inputPath.IsNull || inputPath.IsRelative)
            {
                inputPath = context.FileSystem.GetInputFile(new NormalizedPath(Path.GetRandomFileName())).Path;
                context.LogWarning($"No input path found for document {input.ToSafeDisplayString()}, using {inputPath.FileName.FullPath}");
            }

            string content = await input.GetContentStringAsync();

            // Inject any "Sass_" metadata as variables
            string[] variables = input.Keys
                .Where(x => !x.IsNullOrWhiteSpace()
                    && (x.StartsWith("sass_", StringComparison.OrdinalIgnoreCase) || x.StartsWith("sass-", StringComparison.OrdinalIgnoreCase)))
                .Select(x => (x.Remove(0, 5).Replace(" ", "-"), input.GetString(x)))
                .Where(x => !x.Item1.IsNullOrWhiteSpace() && !x.Item2.IsNullOrWhiteSpace())
                .Select(x => $"${x.Item1}: {x.Item2};")
                .ToArray();
            if (variables.Length > 0)
            {
                content = $"{string.Join(Environment.NewLine, variables)}{Environment.NewLine}{content}";
            }

            // Sass conversion
            FileImporter importer = new FileImporter(context.FileSystem, _importPathFunc);
            ScssOptions options = new ScssOptions
            {
                OutputStyle = _outputStyle,
                GenerateSourceMap = _generateSourceMap,
                SourceComments = _includeSourceComments,
                InputFile = inputPath.FullPath,
                TryImport = importer.TryImport
            };
            IEnumerable<string> includePaths = _includePaths
                .Where(x => !x.IsNull)
                .Select(x =>
                {
                    if (x.IsAbsolute)
                    {
                        return x.FullPath;
                    }
                    NormalizedPath containingInputPath = context.FileSystem.GetContainingInputPath(x);
                    return containingInputPath.IsNull ? null : containingInputPath.Combine(x).FullPath;
                })
                .Where(x => x is object);
            options.IncludePaths.AddRange(includePaths);
            ScssResult result = Scss.ConvertToCss(content, options);

            // Process the result
            NormalizedPath relativeDirectory = context.FileSystem.GetContainingInputPath(inputPath);
            NormalizedPath relativePath = relativeDirectory.IsNull ? NormalizedPath.Null : relativeDirectory.GetRelativePath(inputPath);
            if (relativePath.IsNull)
            {
                relativePath = inputPath.FileName;
            }

            NormalizedPath cssPath = relativePath.ChangeExtension("css");
            IDocument cssDocument = input.Clone(
                cssPath,
                context.GetContentProvider(result.Css ?? string.Empty, MediaTypes.Css));

            // Generate a source map if requested
            if (_generateSourceMap && result.SourceMap is object)
            {
                NormalizedPath sourceMapPath = relativePath.ChangeExtension("map");
                IDocument sourceMapDocument = input.Clone(
                    sourceMapPath,
                    context.GetContentProvider(result.SourceMap));
                return new[] { cssDocument, sourceMapDocument };
            }

            return cssDocument.Yield();
        }

        private static NormalizedPath DefaultInputPath(IDocument document, IExecutionContext context)
        {
            if (!document.Source.IsNull)
            {
                IFile inputFile = context.FileSystem.GetInputFile(document.Source);
                return inputFile.Exists ? inputFile.Path : null;
            }
            return null;
        }
    }
}