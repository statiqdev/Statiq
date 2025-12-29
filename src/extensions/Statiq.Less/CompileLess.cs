using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using dotless.Core;
using dotless.Core.configuration;
using dotless.Core.Importers;
using Microsoft.Extensions.Logging;
using Statiq.Common;

namespace Statiq.Less
{
    /// <summary>
    /// Compiles Less CSS files to CSS stylesheets.
    /// </summary>
    /// <remarks>
    /// The content of the input document is compiled to CSS and the content of the output document contains the compiled CSS stylesheet.
    /// </remarks>
    /// <example>
    /// This is a pipeline that compiles two Less CSS files, one for Bootstrap (which contains a lot of includes) and a second for custom CSS.
    /// <code>
    /// Pipelines.Add("Less",
    ///     ReadFiles("master.less"),
    ///     Concat(ReadFiles("bootstrap.less")),
    ///     Less(),
    ///     WriteFiles(".css")
    /// );
    /// </code>
    /// </example>
    /// <category name="Templates" />
    public class CompileLess : Module
    {
        private Config<NormalizedPath> _inputPath = Config.FromDocument(doc => doc.Source);

        /// <summary>
        /// Specifies a delegate that should be used to get the input path for each
        /// input document. This allows the Sass processor to search the right
        /// file system and paths for include files. By default, the value of
        /// <see cref="IDocument.Source"/> is used for the input document path.
        /// </summary>
        /// <param name="inputPath">A delegate that should return a <see cref="NormalizedPath"/>.</param>
        /// <returns>The current instance.</returns>
        public CompileLess WithInputPath(Config<NormalizedPath> inputPath)
        {
            _inputPath = inputPath.ThrowIfNull(nameof(inputPath));
            return this;
        }

        /// <inheritdoc />
        protected override Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            DotlessConfiguration config = DotlessConfiguration.GetDefault();

            // config.Logger = typeof(LessLogger);
            EngineFactory engineFactory = new EngineFactory(config);
            FileSystemReader fileSystemReader = new FileSystemReader(context.FileSystem);
            return context.Inputs.ParallelSelectAsync(ProcessLessAsync);

            async Task<IDocument> ProcessLessAsync(IDocument input)
            {
                context.LogDebug("Processing Less for {0}", input.ToSafeDisplayString());

                // This is a hack to get to the underlying engine
                ParameterDecorator parameterDecorator = (ParameterDecorator)engineFactory.GetEngine();
                CacheDecorator cacheDecorator = (CacheDecorator)parameterDecorator.Underlying;
                LessEngine engine = (LessEngine)cacheDecorator.Underlying;
                engine.Logger = new LessLogger(context);
                ((Importer)engine.Parser.Importer).FileReader = fileSystemReader;

                // Less conversion
                NormalizedPath path = await _inputPath.GetValueAsync(input, context);
                if (!path.IsNull)
                {
                    engine.CurrentDirectory = path.Parent.FullPath;
                }
                else
                {
                    engine.CurrentDirectory = string.Empty;
                    path = new NormalizedPath(Path.GetRandomFileName());
                    context.LogWarning($"No input path found for document {input.ToSafeDisplayString()}, using {path.FileName.FullPath}");
                }
                string content = engine.TransformToCss(await input.GetContentStringAsync(), path.FileName.FullPath);

                // Process the result
                NormalizedPath cssPath = path.GetRelativeInputPath().ChangeExtension("css");
                return input.Clone(
                    cssPath,
                    context.GetContentProvider(content, MediaTypes.Css));
            }
        }
    }
}