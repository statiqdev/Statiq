# 1.0.0-beta.72

- Added a `DocumentSourceComparer` class that implements `IEqualityComparer<IDocument>` and can be used to compare documents by source path.
- Added a `IEnumerable<IDocument>.ContainsBySource()` extension method to check if a collection of documents contains a document with a given source path.
- Added an improved warning message and early exit out of recursive settings expansion. 
- Added a `MediaTypes.IsMediaType()` method to help determine if a given path matches specified media type(s).

# 1.0.0-beta.71

- Modified the behavior of computed metadata values to cache the value for a given document when using the `=>` prefix. The previous behavior that evaluates a computed value every time it's accessed can still be used by prefixing with `->` instead. In theory this change shouldn't result in any differences in behavior since documents are immutable in the first place (so caching wouldn't be any different from re-evaluating), but if you have computed metadata values that consider state outside the document (such as something like `DateTime.Now`), you'll need to switch those to use the `->` prefix instead.
- Updated JavaScriptEngineSwitcher.Core and JavaScriptEngineSwitcher.Jint.
- Updated `highlight.js` used in `Statiq.Highlight` (#269).

# 1.0.0-beta.70

- Updated Markdig to 0.31.0 to fix an upstream bug when using alt text on images (#267).
- Updated Spectre.Console (#247, thanks @devlead).
- Updated JavaScriptEngineSwitcher (#246, thanks @devlead).
- Updated Microsoft.Data.SqlClient (#248, thanks @devlead).
- Updated System.Linq.Async (#249, thanks @devlead).
- Updated YamlDotNet (#263, thanks @NikoMix).
- Updated NetEscapades.Configuration.Yaml in reaction to update to YamlDotNet.
- Ensures that the `GenerateFeeds` module always sets a feed title, even if one isn't provided.
- Added a warning to `GenerateFeeds` if a feed link isn't set, either through the `Host` setting or otherwise.

# 1.0.0-beta.69

- Added a configurator for the `IEngineManager` and a corresponding bootstrapper `ConfigureEngineManager()` extension method to allow customizing the engine manager used in most commands just prior to it executing the engine (useful for last-minute pipeline customization and some other niche use cases).

# 1.0.0-beta.68

- Improved the `HttpClient.SendWithRetryAsync()` extension to log retries at the information level since they may indicate other problems, and to retry during internal `HttpClient` timeouts.
- Improved exception logging to the console by making sure internal cancellations and timeout exceptions are logged (previously they were silent, which could create the impression nothing went wrong).

# 1.0.0-beta.67

- Fixed a bug that still resulted in file cleaning even when `CleanMode.None` is set (I.e. `--noclean`).
- `RenderMarkdown` no longer escapes `@` characters inside `mailto` links (#254).
- Added a `EscapeAtInMarkdown` setting that can control `@` escaping in Markdown files by the `RenderMarkdown` module on a file-by-file basis (#254).

# 1.0.0-beta.66

- Fixed a bug in `ExtractFrontMatter` to ensure front matter is extracted when the delimiter is on the last line of the file. 

# 1.0.0-beta.65

- Added a new pipeline `PostProcessHasDependencies` property that indicates the post-process phase of a pipeline should depend on the post-process phase(s) of the pipeline dependencies. This is helpful in certain situations where you need a pipeline to run after other post-process phases from dependencies.
- Added a `GenerateRedirects.AlwaysCreateAdditionalOutput()` method to allow creating additional redirect files even if no redirects are specified (I.e. if redirects are also being generated from another source).
- Added additional overloads to `GenerateRedirects.WithAdditionalOutput()` that can accept the execution context in the delegate and/or return a `Task`.

# 1.0.0-beta.64

- Fixed `ElseIf` when `Predicate.RequiresDocument` is false in `ExecuteIf` module (#255, #256, thanks @nils-a).
- Adds the ability to configure a specific date/time as "current". All code and themes should now use `IExecutionState.GetCurrentDateTime()` instead of `DateTime.Now`, etc.
- Removes console background color for better compatibility with different console themes (#257).
- Outputs the configured log level on execution.

# 1.0.0-beta.63

- Fixed generation of document source paths for symbols in `AnalyzeCSharp` to use the symbol ID and generate shorter names so very long symbols don't create paths that are too long (#244).

# 1.0.0-beta.62

- Changed `Statiq.App.props` to `Statiq.App.targets` to resolve some import ordering bugs.
- Added a `Keys.IgnoreExternalDestinations` setting to ignore when document destinations are not relative to the output path since some use cases may require writing documents elsewhere.

# 1.0.0-beta.61

- Updated the referenced version of ImageSharp in the Statiq.Images library (#242, thanks @olevett).
- Fixed some `HtmlKeys` copy-paste errors (#240, thanks @pascalberger).

# 1.0.0-beta.60

- Added support for surfacing tuple information in the `AnalyzeCSharp` module.

# 1.0.0-beta.59

- Added support for regular expressions to the `ExtractFrontMatter` module (#231).
- Added `IPipelineOutputs.ExceptPipelines()` extension method to get outputs from all but a set of specified pipelines.
- `IPipelineOutputs.ExceptPipeline()` now returns documents in natural order like `IPipelineOutputs.FromPipeline()` does.
- Added `AnalyzeCSharp.IncludeEmptyNamespaces()` option to control whether recursively empty namespaces are output.

# 1.0.0-beta.58

- Updated Buildalyzer in `Statiq.CodeAnalysis` to 4.1.0 which removes some dependencies on .NET Framework and resolves some package resolution problems.

# 1.0.0-beta.57

- Added support for "raw" code fences to the `RenderMarkdown` module that bypasses markdown processing.
- Fixed a bug when instantiating a `ShortcodeResult` from the bootstrapper due to lack of availability of an execution context (since the engine hasn't been created yet).
- Added `WithNestedElements()` to the `GatherHeadings` module to control whether nested HTML element content is included in the heading text (the default is now that it is not, other than links).
- Fixed a bug with certain modules double-encoding character references (https://github.com/statiqdev/Statiq.Web/issues/981).
- Added a new `StartProcessArgument` class and `WithArgument()`/`WithArguments()` methods to the `StartProcess` module that makes it easier to define multiple arguments inside a single config delegate.

# 1.0.0-beta.56

- Added `DocumentList<TDocument>.GetDestination(NormalizedPath destinationPath)`, `.GetSource(NormalizedPath sourcePath)`, and `.GetRelativeSource(NormalizedPath sourcePath)` to better provide single document results from a document list.
- Added `IPipelineOutputs.GetDestination(NormalizedPath destinationPath)`, `.GetSource(NormalizedPath sourcePath)`, and `.GetRelativeSource(NormalizedPath sourcePath)` to better provide single document results from pipeline output collections.
- Added `IDocument.Timestamp` that holds a timestamp of document instantiation and is useful for ordering documents based on "most recent" semantics.
- Uses the new `IDocument.Timestamp` property to order certain document results like the `PipelineOutputs` enumerator, `PipelineOutputs.ExceptPipeline()`, `IEnumerable<IDocument>.FilterDestinations()`, and `IEnumerable<IDocument>.FilterSources()` in descending creation order (in other words, document results that otherwise don't have a natural order are ordered by timestamp now) (#226).
- Fixed a bug when multiple modules call `IEngine.GetJavaScriptEnginePool()` for the first time concurrently.
- Added `CodeAnalysisKeys.Implements` to the documents produced by the `AnalyzeCSharp` module to represent the interface member(s) being implemented by properties, events, and methods.
- Added the ability to inject Sass variables via metadata by prefixing with "Sass_" which should make providing theme customizations a lot easier.

# 1.0.0-beta.55

- Added a new `MarkdownExtensions` metadata key that can add Markdown (I.e. Markdig) extension types per-document or per-execution using metadata/settings (#222).
- Removed the recently added `PathCollection.RemoveDefault()` method and instead allow optionally specifying whether paths that get added to a `PathCollection` are removable (the default is `true`).
- Removed the recently added notion of "initial settings" due to being confusing and instead moved settings initialization up in the order of bootstrapper operations (this _shouldn't_ result in a breaking change, but be on the lookout for problems).

# 1.0.0-beta.54

- Added a new `RetryModules` module that provides simple retry behavior for it's child modules (it essentially wraps module execution with [Polly](http://www.thepollyproject.org/)).
- Added a new `PathCollection.RemoveDefault()` method that only removes the default paths, and used it to clear only default paths when alternate input paths are specified on the command line.

# 1.0.0-beta.53

- **Breaking change:** `IFileSystem` must now be injected and passed to `BaseCommand` when creating custom commands that derive from it.
- **Breaking change:** `IEngine.FileSystem` is now a `IReadOnlyFileSystem` instead of `IFileSystem` (I.e. the file system can no longer be edited one the engine has been created).
- **Breaking change:** The file system can no longer be configured through `Bootstrapper.ConfigureEngine()` and must be done through `Bootstrapper.ConfigureFileSystem()` or one of the other more specific file system bootstrapper extensions like `Bootstrapper.SetRootPath()`.
- Changed the `Statiq.Razor` Razor compilers to use collectible assemblies in a custom load context for better memory usage and caching behavior. This isn't a breaking change but was somewhat risky so keep an eye out for any problems related to Razor compilation and please open issues if you notice any regressions.
- Updated `Markdig` in `Statiq.Markdown` to 0.26.0.
- Fixed a number of inconsistencies with the way the `@` symbol was being escaped in Markdown documents (using `\@` should now work reliably, see https://github.com/statiqdev/Discussions/discussions/109).
- `Engine` now accepts a `IReadOnlyFileSystem` as a constructor argument (a new file system will be created if one is not provided).
- Added bootstrapper support for "initial" settings which are set before other parts of the bootstrapper (like the file system or services). Use these new initial settings bootstrapper extensions when you need to add a setting very early in the bootstrapper process just after reading configuration files (for most use cases the existing settings extensions continue to be preferred).
- Updated `Buildalyzer` in `Statiq.CodeAnalysis` to 3.2.6.
- Updated `Microsoft.CodeAnalysis.CSharp` in `Statiq.Core` to 3.11.0.
- Improved diagnostic logging of compilation message in `Statiq.Razor`.
- Made the output ordering of the `ReadFiles` module deterministic which will help certain caching scenarios where cache codes are generated by combining output files including order.
- Added a `CacheDocuments.WithoutSourceMapping()` extension to toggle a new mode for the `CacheDocuments` module where all inputs are mapped to all outputs and the entire cache is invalidated when one input changes.
- Changed dynamic Razor assemblies to use the path and file name in their assembly names instead of a random file name (#220).
- Added a warning when a Razor partial or view uses a `@model` directive that could be overridden by a `@inherits` directive in an import.

# 1.0.0-beta.52

- Made it easier to override certain interfaces used by the engine by registering alternate implementations in the service collection passed to the engine. Alternate implementations of the following can now be provided: `IApplicationState`, `IMemoryStreamFactory`, `ILinkGenerator`, `INamespacesCollection`, `IScriptHelper`, `IDocumentFactory`, and `IFileCleaner`. In addition, the default implementations of these interfaces have been refactored to make deriving from them easier (I.e. marked interface members as `virtual`).
- Refactored the default `LinkGenerator` to use strings instead of `NormalizedPath` which could result in poor results in some edge cases like trailing slashes (#218).
- Added toggle for keeping trailing slash when hiding a page in `LinkGenerator` and related extensions, also exposed via a new `Keys.LinkHiddenPageTrailingSlash` setting (#218).
- Updated Spectre.Console reference in Statiq.App to 0.43.0.

# 1.0.0-beta.51

- Fixed a bug with backwards compatability of the `HtmlKeys` file not being in the original namespace (#973).

# 1.0.0-beta.50

- **Breaking change:** Removed `Statiq.Html` extension and moved all functionality into either `Statiq.Common` (helpers and utilities) or `Statiq.Core` (modules). All modules are still available through `Statiq.Core` and you should remove references to `Statiq.Html`.
- Added a reference to `AngleSharp` in `Statiq.Common`.
- Added all keys from `HtmlKeys` to `Keys` in `Statiq.Common` but kept `HtmlKeys` and marked it obsolete for backwards-compatibility.
- Changed the behavior of HTML parsing and formatting (I.e. both reading and writing) to preserve any originally encoded character references (#213).
- Added a new `IExecutionContext.GetContentProvider()` extension that accepts an AngleSharp `IMarkupFormattable` (such as `IHtmlDocument`).
- Added `WithAutoHighlightUnspecifiedLanguage()` to the `HighlightCode` module to avoid time-consuming generation-time code highlighting when the language is unknown (#210, thanks @Turnerj).
- Changed the `HighlightShortcode` module to add a `<pre>` element when there are new lines in the content or when specified using a new `AddPre` shortcode argument (#215).

# 1.0.0-beta.49

- **Breaking change:** The `LinkGenerator` class is no longer static and now needs to be accessed through a new `IExecutionState.LinkGenerator` or `IExecutionContext.LinkGenerator` property.
- Added the `cache` directory to the excluded list in `Statiq.App.props`.
- Fixed a bug with `DocumentFileProvider` and documents with a null `Destination`.
- Fixed a bug in the Razor engine when run under .NET 6 RC runtimes (#204, thanks @phil-scott-78).
- Updated several dependencies (#199, #201, #202, thanks @devlead).
- Added the ability to cache Razor partials using new `CachedPartial()` and `CachedPartialAsync()` HTML helpers. (#205)

# 1.0.0-beta.48

- Added `GenerateLunrIndex.WithClientName` method to allow setting the name of the client object in the generated JavaScript file.
- Fixed a bug in `GenerateLunrIndex` when no documents contain a given search field.
- Added `GenerateLunrIndex.WithStemming` methods to control stemming behavior and changed default to no stemming.
- Added support for typeahead style searching to the generated client JavaScript search file by default (I.e. automatically adds trailing wildcards).

# 1.0.0-beta.47

- **Breaking change:** Renamed the erroneous `Statiq.SearchIndex` namespace in the `Statiq.Lunr` package to `Statiq.Lunr`.
- **Breaking change:** Completely rewrote the `GenerateLunrIndex` module to build and output a search index at generation time and add a bunch of new features (#192).
- Fixed a bug with the reflected namespace collection when dealing with objects in the global namespace (#191).
- Fixed a bug when getting outputs from a valid pipeline that didn't produce any (it now returns an empty collection instead of throwing and exception) (#172).
- Fixed a bug in the `GenerateSiteMap` module when using the `LinkRoot` setting that included the link root twice (#158, #193, thanks @kkato233).
- Added support for named strongly-typed pipelines (#173).

# 1.0.0-beta.46

- Fixed a bug in `MirrorResources` to prevent it from mirroring links with "rel" values that don't specify artifacts (#190).
- Fixed a bug in `MirrorResources` to prevent it from mirroring resources from the current host when `Host` is specified (#190).
- Added a `ReadExcel` module to Statiq.Tables that can read an Excel file into metadata as a `IReadOnlyList<IReadOnlyList<string>>`.
- Added a `ReadCsv` module to Statiq.Tables that can read a CSV file into metadata as a `IReadOnlyList<IReadOnlyList<string>>`.

# 1.0.0-beta.45

- Fixed several bugs related to caching in the `CopyFiles` module (#189).

# 1.0.0-beta.44

- **Breaking change:** Fixed a bug with Razor layouts and partials and explicit model types. Previously all layouts and partials were assuming
  the model type was an `IDocument`. This meant that extension methods and other `IDocument` oriented functionality worked in a layout or partial
  when using the `@Model` property to access the document, but it also meant they didn't work for alternate models. Layouts and partials are
  supposed to generally handle all model types (since different views can call them), so now the model is `dynamic` for layouts and partials
  unless explicitly specified. This has the side-effect of making some `@Model` access like `IDocument` extension methods that used to work fail.
  If you see "does not contain a definition" error messages in Razor compilation, try changing `@Model` property access to `@Document` in your layouts
  and partials, or using an explicit `@model` directive at the top of the layout pr partial file to explicitly specify the model type for that layout or partial.
- Fixed a bug when changing the `CleanMode` setting via the bootstrapper.
- Improved compilation failure exception messages for Razor layouts and partials.

# 1.0.0-beta.43

- Fixed a bug with using statements not being added to Razor layouts and partials (#953).

# 1.0.0-beta.42

- **Breaking change:** Deleted `IFile.GetCacheHashCodeAsync()` and replaced it with an implementation of `ICacheCode`.
- **Breaking change:** Renamed `IContentProvider.GetCacheHashCodeAsync()` and replaced it with an implementation of `ICacheCode`.
- **Breaking change:** Renamed `IDocument.GetCacheHashCodeAsync()` and replaced it with an implementation of `ICacheCode`.
- Added a new "cache" folder that contains caching artifacts to improve performance, deleting it won't harm anything but it should be left if possible to improve initial generation performance.
  Also note that the "cache" folder likely shouldn't be committed to a repository, though it's designed to support that scenario when warranted (I.e. relative paths, etc.).
- Compiled Razor assemblies (including layouts and partials) are now cached to disk which dramatically improved initial generation performance by not recompiling files that haven't changed.
- Added `IFile.ReadAllBytesAsync()` and `IFile.WriteAllBytesAsync()`.
- Added `IFile.WriteFromAsync()`, `IFile.AppendFromAsync()`, and `IFile.CopyToAsync()` extension methods to more easily copy a file from/to a stream.
- Added `IReadOnlyFileSystem.CachePath` and related extensions, methods, etc. to provide a path where cache files should be stored and set to "cache" by default (which should be excluded in `.gitignore`).
- The write tracking data is now cached in a file so if the output folder has not changed and the content is the same, files don't need to be written even on the first execution.
- Added an `ICacheCode` interface to provide a standard deterministic `.GetCacheCodeAsync()` method and implemented it in `IDocument`, `IContentProvider`, and `IFile`.
- Changed the console output encoding and the `ProcessLauncher` child process console encodings to UTF-8 so emoji and other Unicode characters will render correctly from child processes.
- Fixed a bug with the `--help` CLI option and command description escaping (#186).
- Added ability to set the temp and cache paths via the CLI.

# 1.0.0-beta.41

- Added support for the `PATH` and `PATHEXT` environment variables to the `ProcessLauncher` so it can resolve file names more like `cmd.exe`.
- Added better error messages when process launching fails.
- Added `CleanMode.Unwritten` that will wait to clean the output folder _after_ each execution and tries to avoid writing duplicate files to the file system (this is the new default).
- Added `IFile.GetCacheHashCodeAsync()` which can be used to get a hash code representing the current state of a file.
- Refactored `FileContent.GetCacheHashCodeAsync()` to use the new `IFile.GetCacheHashCodeAsync()` instead of attempting to read the entire file content.

# 1.0.0-beta.40

- Several optimizations to file IO and the `WriteFiles` and `CopyFiles` modules.

# 1.0.0-beta.39

- Fixed a bug with link generation for non http/https links in Markdown (#179, #184, thanks @JoshClose).
- Fixed a bug with the `ConsoleListener` on MacOS that prevented signaling from the main thread (used for the `preview` command in Statiq Web) (#182, #183, thanks @devlead).
- Fixed a bug with the CLI `--help` output that caused the app to crash (#180, #181, thanks @matkoch).
- Added caching for `IEnumerable<TDocument>.GetDestinationTree()` and `IEnumerable<TDocument>.GetSourceTree()` which results in a significant performance improvement in some cases for large sites.
- Improved logging messages related to timing.
- Added `IFile` extensions to serialize and deserialize JSON.
- Added `IFile` extensions to serialize and deserialize YAML.
- Added `NormalizedPath.ThrowIfRealtive()` and `NormalizedPath.ThrowIfAbsolute()` helper methods.
- Added a `IDirectory.MoveTo()` method.
- Fixed a bug that caused an input phase overload of an `ExecutionPipeline` not to execute.

# 1.0.0-beta.38

- Fixed a bug when both a debugger was attached and the log level was set to debug by removing the `DebugLogger` (#176).
- Added a `RedirectTo` metadata value to documents output from `GenerateRedirects`.
- Fixed a regression with the `DocumentLink()` HTML helper in Razor (#177).

# 1.0.0-beta.37

- Updated Buildalyzer to the most recent version and resolves Roslyn version incompatibilities in the `AnalyzeCSharp` module (#174, thanks @mholo65).
- Added an optional `makeAbsolute` parameter to `LinkGenerator.GetLink()` that allows keeping links as relative (#170).
- Fixed fragment support in the Markdown link rewriter (#170, #175, thanks @JoshClose).
- Fixed `LinkGenerator` behavior when using query and/or fragment components (#170).
- Added `makeAbsolute` parameters to `IExecutionState.GetLink()` extension methods and others as appropriate (#170).

# 1.0.0-beta.36

- Removed the `UrlResolutionTagHelper` from Razor processing so that `~/` links don't get processed (#170).
- Added support for `~/` link resolution to Markdown files, note that this is an interim feature and will be removed again in favor of a new module that handles `~/` links for all template engines soon (#170, #171, thanks @JoshClose).

# 1.0.0-beta.35

- Added better error logging of Razor rendering failures.
- Changed the default Razor model type to `IDocument` from `dynamic` if no explicit model or base type is specified.

# 1.0.0-beta.33

- Added ability to "map" input folders to subfolders in the virtual folder hierarchy (so not every input folder has to be at the root now).
- Added the `GenerateJson` module back in (it got dropped a while ago when Statiq.Json was merged with Statiq.Core).
- Added a utility `LoggerFactoryLoggerWrapper` class that can wrap the context (or any other `ILogger`) and provide it as an `ILoggerFactory` that always logs to the underlying logger.
- Fixed a bug with the generated doctype element in redirects (#160, #161, thanks @gep13).
- Fixed a bug with virtual input directories when getting a parent directory from a file produced by one (it wasn't getting the original virtual directory).

# 1.0.0-beta.32

- **Breaking change:** Refactored the `IExecutionContext.GetContentProviderAsync(string)` extension (and overloads) to be non-async and unified related extensions that get content providers.
- **Breaking change:** Refactored `IDocument.CloseAsync(string)` and related extensions to be non-async and unified related clone methods into a single set of overloads.
- **Breaking change:** Removed the `UseStringContentFiles` option because it's no longer relevant with the new string and stream processing.
- Updated the Razor engine from 3.0.0 to 3.1.10.
- Updated the Roslyn libraries from 3.4.0 to 3.8.0.
- Changed the precedence of environment variables to overwrite configuration from settings files to match expected ASP.NET Core conventions (#154).
- Fixed some bugs with the `ProcessLauncher` on Linux and Mac by ensuring the entire process tree is killed (#156).
- Added some performance caching for `IExecutionProcess.OutputPages`.
- Refactored `NormalizedPath` to reduce use of strings.
- Added some performance enhancements to caching of AngleSharp HTML documents.

# 1.0.0-beta.31

- Added some additional retry policies to file operations to avoid file lock exceptions under certain conditions (#151).
- Added ability to customize the body of meta-refresh redirect HTML files using the `RedirectBody` key (#153).
- Added `.fhtml` as a media type to mean HTML fragments (needed so that we can treat full HTML and HTML fragments differently when applying layouts in Statiq Web).

# 1.0.0-beta.30

- Added a new `CleanMode` enumeration and corresponding setting to control which output files are cleaned on execution (#152).
- Added a `--clean-mode` command-line option to control the cleaning mode.
- Changed the default cleaning behavior to `CleanMode.Self` which only cleans files written during execution and not those written by external processes, for example.
- Added the `IServiceProvider` to the `PipelineBuilder` so services are available when building pipelines (#150).
- Added new `IReadOnlyFileSystem.GetRootPath()` extensions.
- Added `ProcessLauncher.WaitForRunningProcesses()` that allows waiting until all running processes have exited.
- Added a `IBootstrapper.Command` property that can get the command that was run (provided it inherits from `BaseCommand<TSettings>`).

# 1.0.0-beta.29

- Added a new `interactive` command that provides a REPL (read-eval-print prompt) after execution, useful for inspecting the state of the engine and debugging the generation.

# 1.0.0-beta.28

- Added a `.PreserveFrontMatter()` configuration method to the `ExtractFrontMatter` module that preserves the front matter content.
- Fixed a bug with gathered headings where HTML was included in the heading text (#142).
- Added support for specifying `ViewData` in the `RenderRazor` module (#145, #146, thanks @alanta).
- Added a new `BeforeDeployment` event that gets raised before any deployment pipelines are run (or at the end of execution if there are no deployment pipelines).
- Added a new `ProcessLauncher` utility class to `Statiq.Common` that does what the `StartProcess` module does, but in a way that can be used outside of the module since it's such a general use case.

# 1.0.0-beta.27

- Fixed a bug with deployment pipelines getting document outputs from non-deployment pipelines.
- Fixed a bug where deployment pipeline input phases were starting with outputs from non-deployment pipelines (input phases should always start empty).
- Improved exception logging when in a module to include pipeline, phase, and module name.

# 1.0.0-beta.26

- Fixed an unnecessary pipeline dependency check for deployment pipelines that prevented setting up dependency chains involving deployment pipelines and only dependent non-deployment pipelines.

# 1.0.0-beta.25

- **Breaking change:** Removed the `Statiq.Html.ValidateLinks` module in favor of analyzers in Statiq Web (which have already been improved beyond the module).
  If you still need access to this module directly, copy the code from a previous version into your own project and reference the `Statiq.Html` package and it should continue to work.
- Updated ImageSharp in `Statiq.Images` to stable version 1.0.1 (thanks @JimBobSquarePants, #138).
- Improved logging, _all_ error and warning messages are now output on Azure Pipelines and GitHub Actions as checks.

# 1.0.0-beta.24

- Added `StartProcess.LogErrors()` to configure whether standard error output is logged as debug or error messages.
- Added `IContentProvider.GetCacheHashCodeAsync()` to get and cache a hash code for a given content provider.
- Updated AngleSharp to 0.14.0 (#135, #136, thanks @alanta).
- Added `RenderMarkdown.WithMarkdownDocumentKey(string markdownDocumentKey)` to indicate where the `MarkdownDocument` should be saved, or not at all.
- Removed `DocumentAnalyzer` and `SyncDocumentAnalyzer` in favor of combining into alternate overloads in `Analyzer` and `SyncAnalyzer` similar to how `Module` handles it
  (this lets analyzers provide functionality before documents are processed individually).
- Renamed `IInitializer` to `IBootstrapperInitializer`.
- Added `IEngineInitializer` which will be instantiated and called when an engine first starts up (for example, lets you hook engine events from extensions).
- Moved engine event classes like `BeforeEngineExecution` to `Statiq.Common` so they can be subscribed by extensions using a `IEngineInitializer`.
- Removed documents from the `IAnalyzer` methods, added `IExecutionContext` as a base interface for `IAnalyzerContext`, and analyzer documents now passed as `IAnalyzerContext.Inputs` similar to modules.
- Added `IAnalyzer.BeforeEngineExecutionAsync(IEngine engine, Guid executionId)` to get called one-per-instance for each analyzer prior to each engine execution (for resetting caches, etc.).
- Changed analyzer pipeline and phase specification to be a collection of `KeyValuePair<string, Phase>` instead of distinct pipeline and phase arrays.
- Changed deployment pipelines so that none of their phases are executed until all phases of non-deployment pipelines are completed (I.e. deployment pipelines now don't start executing until all non-deployment pipelines are done).
- Non-deployment pipelines can no longer have a dependency on deployment pipelines (this didn't really make sense before, but now it's enforced).

# 1.0.0-beta.23

- Removed a debugging break statement that snuck into the last release.

# 1.0.0-beta.22

- Fixed a bug with the `ValidateLinks` module when using a `<base>` element on the page.
- Added `IExecutionState.LogBuildServerWarning()` and `IExecutionState.LogBuildServerError()` extensions to log messages to the console in a format build servers can recognize for build checks (GitHub Actions and Azure Pipelines).
- Fixed bugs with relative link validation (#128, #134, thanks @mholo65).
- Added a `StartProcess.HideArguments()` method to hide arguments when logging process commands.
- Fixed several bugs related to cancellation and error codes.
- Fixed a bug with `NormalizedPath.GetTitle()` when the file name contains multiple extensions (#130, #131, thanks @devlead).
- Added support for analyzers (#104).
- Added `Analyzer`, `SyncAnalyzer`, `DocumentAnalyzer`, and `SyncDocumentAnalyzer` base analyzer classes (#104).
- Added `Bootstrapper.Analyze()` and `Bootstrapper.AnalyzeDocument()` for defining delegate-based analyzers (#104).
- Fixed a bug in the console logger when logging with `LogLevel.None`.
- Added a `--failure-log-level <LEVEL>` CLI command to fail generation and return a non-zero exit code if any log messages exceed the provided threshold (I.e. `--failure-log-level Warning`) (#101).
- Added a `Bootstrapper.SetFailureLogLevel()` extension to set the failure log level from code (#101).
- The `RenderMarkdown` module now adds `MarkdownDocument` metadata that contains the Markdig `MarkdownDocument` created during processing.
- Added Scriban (Liquid) support via new `RenderScriban` module (#120, thanks @mholo65).
- Added `ExecuteDestinations` and `ExecuteSources` modules to filter documents by destination or source and then execute modules on the filtered documents.

# 1.0.0-beta.21

- Added some optimizations for async file I/O.
- Added overloads to `FilterSources` and `FilterDestinations` modules that accept `Config<IEnumerable<string>>` for the patterns.
- Fixed `IDirectory.GetFiles()` and `IDirectory.GetDirectories()` to make sure excluded paths are excluded from the results.
- Creating a `ReadFiles` module without any patterns now returns all files.
- Added `ExtractFrontMatter.RequireStartDelimiter()` to require the front matter to start with a specified delimiter on the first line (for example, `/*` as the opening of a comment block).
- Added `IMetadata.ToJson()` utility extensions to serialize an `IMetadata` object to JSON.
- Fixed a bug where the `EvaluateScript` module would reset the document media type for content return values.
- Added a new `glob eval` command to evaluate a globbing pattern against a specified directory and report all the matches.
- Added a new `glob test` command to test a specified path against a globbing pattern to see if it matches.
- Removes subresource attributes in `MirrorResource` (#127).
- Updated Spectre.Cli CLI library for better console help messages.
- Added a new `ReadApi` module for generally reading from an API client (#126, thanks @unchase).
- Added `IBootstrapper.ModifyPipeline()` to make it easier to modify an existing pipeline via the bootstrapper.
- Added collection initializer and list support to the `ExecuteBranch` module.

# 1.0.0-beta.20

- **Breaking change:** Removed the `IDocument.GetParent()`, `IDocument.HasChildren()`, `IDocument.GetDescendants()`, and `IDocument.GetDescendantsAndSelf()` extension methods
  (`IDocument.GetChildren()` still remains since it's fetching actual metadata values). Instead, the appropriate tree concept can now be accessed via an implementation
  of the `IDocumentTree<TDocument>` interface. This change may break navigation, so `Outputs` or `OutputPages` should be considered for generating navigation instead.
- Added an `OutputPages` property to the engine and execution context to make it easier to filter outputs by "pages" (by default, documents with a destination path ending in ".htm" or ".html").
  This is what you should use to generate navigation going forward.
- Added `Outputs.Get()` and `OutputPages.Get()` to support getting a single document from those collections by destination path, which is faster than globbing.
- Added a `IDocumentTree<TDocument>` interface to encapsulate different kinds of document tree traversal logic.
- Added a `DocumentMetadataTree<TDocument>` implementation to represent document trees as the result of metadata containing child documents.
- Added a `DocumentPathTree<TDocument>` implementation to represent document trees as the result of file paths.
- Added a `IEnumerable<TDocument>.AsMetadataTree()` extension method to get a `DocumentMetadataTree<TDocument>` instance that creates a tree from document metadata containing child documents.
- Added a `IEnumerable<TDocument>.AsDestinationTree()` extension method to get a `DocumentPathTree<TDocument>` instance that creates a tree from document destination paths.
- Added a `IEnumerable<TDocument>.AsSourceTree()` extension method to get a `DocumentPathTree<TDocument>` instance that creates a tree from document source paths.
- Added a new `FilteredDocumentList<TDocument>` return type for `IEnumerable<TDocument>.FilterDestinations()` and `IEnumerable<TDocument>.FilterSources()` calls
  (including `Outputs[string[] patterns]`) which implements the new `IDocumentTree<TDocument>` and lets you treat the resulting filtered documents as a tree from the filter return.
- Added the `IDocumentTree<TDocument>` interface to `IPipelineOutputs` with default implementations that operate on document destination paths. This means you can call
  methods like `Outputs.GetChildren(doc)` to get all the children across all pipelines of the given document, etc.
- Added a new `IndexFileName` setting to control the default file name of index files (defaults to `index.html`).
- Added a new `PageFileExtensions` setting to control the default file extensions of "pages" for things like `OutputPages` filtering and link generation (defaults to ".html" and ".htm").
- Added a new constructor to the `SetDestination` module that will change the destination of documents to the first value of the `PageFileExtensions` setting (default of ".html").
- Added a new `MinimumStatiqFrameworkVersion` key to perform a check for the minimum allowed version of Statiq Framework. If this is set to something higher than the current version
  of Statiq Framework, an error will be logged and execution will stop. Any setting that starts will this key will be considered, so it's recommended the use of this key be
  suffixed with a unique identifier to avoid conflicts between components (for example `MinimumStatiqFrameworkVersion-MySite`).
- Refactored settings and configuration implementations. You shouldn't notice anything usage-wise, but keep an eye out for anything that doesn't work as expected

# 1.0.0-beta.19

- Lots of under-the-hood refactoring to make things faster.
- Fix for invalid URIs in the `ValidateLinks` module (#119).

# 1.0.0-beta.18

- **Breaking change:** Changed the IPipelineOutputs` indexer to filter documents by destination path from all pipelines instead of get documents from a specified pipeline.
  To use the previous behavior, call `IPipelineOutputs.FromPipeline(string)`. This will make the more common case (finding a document or documents among all pipelines) easier.
  To refactor your code to match the old behavior, do a string search for "Outputs[" and replace with `Outputs.FromPipeline()`.
- **Breaking change:** Changed the `DocumentList<TDocument>` indexer to return all matching documents instead of the first matching document. To refactor your code
  to match the old behavior add a `.FirstOrDefault()` after the call to the indexer.
- **Breaking change:** Removed the `CompileScript` module in favor of global script caching (so using the `EvaluateScript` module will also cache the script compilation, removing the need for a separate cached assembly).
- **Breaking change:** Removed all but a single string-based evaluation method from `IScriptHelper` to promote global script compilation caching.
- **Breaking change:** Removed global metadata properties from scripted documents, metadata, and shortcodes due to performance penalty and inability to cache across documents.
  Uses of global properties that refer to other metadata will have to be replaced with `Get()` variants. For example, a scripted metadata value `=> Foo` should become `=> Get("Foo")`.
- **Breaking change:** Renamed the `AbsolutizeLinks` module to `MakeLinksAbsolute` for clarity and to match with the new `MakeLinksRootRelative` module.
- Added a new `MakeLinksRootRelative` module.
- Added a `IEnumerable<IDocument>.ContainsById(IDocument)` extension method to determine if a sequence contains an equivalent document by ID.
- Added a new `ConcurrentCache<TKey, TValue>` helper class that uses `Lazy<T>`, which improves performance of internals by avoiding duplicate value factory evaluation.
- Script compilations are now globally cached, dramatically improving performance of scripted documents, metadata, and shortcodes.
- Fixed some bugs with the `CacheDocuments` module and document hash code generation.
- Added a `IComparer<T>.ToConvertingComparer()` extension method that converts a typed comparer into a `IComparer<object>` that performs type conversions.
- Added a `IEqualityComparer<T>.ToConvertingComparer()` extension method that converts a typed comparer into a `IComparer<object>` that performs type conversions.
- Added a `RemoveTreePlaceholders` module to remove tree placeholder documents without flattening.
- The `MergeMetadata` and `MergeDocuments` modules no longer merge settings (since they're inherited by the document regardless).
- Added `IMetadata.WithoutSettings()` to return filtered metadata without any settings values.
- Added the key being requested to `IMetadataValue.Get()` so that metadata values can use it if needed.
- Added recursive metadata expansion detection of `IMetadataValue` metadata values (it will now throw an error so you know which key is recursively expanding).
- Added a `RenderSectionOrPartial(string sectionName, string partialName)` helper to the base Razor page.
- Added feed metadata to the output documents from `GenerateFeeds`.
- Added a new `AddRtlSupport` module in `Statiq.Html` that automatically adds RTL attributes to HTML elements (#113, #15, thanks @encrypt0r).
- Added a `IEnumerable<IDocument>.RemoveTreePlaceholders()` extension method.
- Added an option to remove tree placeholder documents in the `FlattenTree` module and the `IEnumerable<IDocument>.Flatten()` extension methods.
- Added `settings` as a default settings file name (with support for JSON, YAML, or XML formats).
- Added support for `appsettings` and `statiq` YAML (`.yml` and `.yaml`) and XML (`.xml`) configuration files.
- Added containing types to the symbol ID for nested symbols in the `AnalyzeCSharp` module (#116).
- Added a message about using a higher log level when an error occurs (#115).
- Fixed a bug on engine reruns (I.e. the Statiq.Web preview command).

# 1.0.0-beta.17

- Made `RenderRazor.WithLayout()` take precedence over an available `_ViewStart.cshtml` file (by ignoring the `_ViewStart.cshtml` file entirely which previously took precedence in this case).
- Fixed some bugs with input-relative path finding for Razor layouts and partials (#102).

# 1.0.0-beta.16

- Added `NormalizedPath.ContainsDescendantOrSelf()`.
- Added `NormalizedPath.ContainsChildOrSelf()`.
- Fixed several bugs with the implementation of `NormalizedPath.GetRelativeOutputPath()`.
- Added a `IReadOnlyFileSystem.GetRelativeOutputPath()` extension.
- Added a `IReadOnlyFileSystem.GetRelativeInputPath()` extension.
- Added `IFileSystem.ExcludedPaths` to indicate input paths that should be excluded from processing.
- Added `.GetInstance(Type)` and `.GetInstance<TType>()` methods to the `ClassCatalog`.
- Fixed a bug where custom commands were being added twice (#103).
- Fixed a bug in the `GenerateRedirects` module for original files ending in `.htm` (#105).
- Updated `Spectre.Cli` to version 0.35.0 with better internal command registration (#110, thanks @patriksvensson).
- Removed restriction on only using `IDocument` models in Razor (#108, #23, thanks @alanta).

# 1.0.0-beta.15

- Added the new `Statiq.App.props` file to a `buildTransitive` folder in the package so it flows transitively to consumers.

# 1.0.0-beta.14

- Fixed a bug with async context-based `Config` delegates not indicating that they don't require a document.
- Added a `IReadOnlyPipelineCollection IExecutionState.ExecutingPipelines` property that provides the currently executing pipelines.
- Added a new `ThrowExceptionIf` module that throws an exception if a condition is true.
- Added a `Config<bool>.IsFalse()` extension method.
- Added a `Config.ContainsAnySettings()` extension method.
- Added `IReadOnlyDictionary<K, V>.ContainsAnyKeys()` extension methods.
- Fixed a bug regarding disposal of a content stream in the `StartProcess` module.
- Fixed a bug that required feed items to have URI IDs when the specification indicates they can also be arbitrary strings.
- Added a props file to the Statiq.App package to automatically set the default output exclusion and input content MSBuild props.
- Fixed a bug with the `OrderDocuments` module sorting by string value for default keys.
- Changed `GatherHeadings` to take a `Config<int>` for specifying the level.

# 1.0.0-beta.13

- Removed the `StreamContent` content provider in favor of the new `MemoryContent` content provider (should also provide a nice performance boost since there's no more stream locking).
- Added `IExecutionContext.GetContentProvider()` extensions that take a `byte[]` buffer.
- Added new `MemoryContent` content provider that wraps a `byte[]` buffer.
- Changed `IEngine.Settings` to be mutable, settings can now be added directly to the engine after instantiation.
- Fixed a bug with case-insensitive settings from the command line.
- Fixed a bug with computed metadata settings from the command line.

# 1.0.0-beta.12

- Added document flattening by default when using the `DocumentFileProvider`.
- Added document flattening by default when using `IEnumerable<TDocument>.FilterSources()` and `IEnumerable<TDocument>.FilterDestinations()` extensions.
- Added a `IReadOnlyDictionary<TKey, TValue>.ContainsKeys()` extension utility method.
- Added multiple key test to the `ExecuteIf` module.

# 1.0.0-beta.11

- Fixed the `ProcessSidecarFile` module to pass the media type to child modules.
- Added `IBootstrapper.RunTestAsync()` extensions to assist with testing entire bootstrappers.
- Added `AsDependencyOf()` methods to `PipelineBuilder`.
- Added a collection initializer to `TestFileProvider` to make adding test files easier.
- Added full `NormalizedPath` support to `TestFileProvider` internals.

# 1.0.0-beta.10

- Better engine and pipeline exception handling and log messages.
- Added a new `--debug` CLI flag to launch a debugger and attach it.

# 1.0.0-beta.9

- Fix not to reuse a `HttpRequest` during retry, changed the `HttpClient.SendWithRetryAsync()` and related methods to take a factory instead of a single request (#98).
- Fix for a `PipelineBuilder` with no actions to return an empty pipeline.
- Added `IBootstrapper.AddDeploymentPipeline()` overloads.

# 1.0.0-beta.8

- Added `IReadOnlyDictionary<string, Type>` implementation to `ClassCatalog`.
- Optimized `ScriptMetadataValue` metadata caching for big performance gains in certain scenarios.
- Ensures that document metadata can't override the properties from `IDocument` (`Id`, `Source`, `Destination`, and `ContentProvider`).
- Removed the `ApplyDirectoryMetadata` module in favor of a more specific/powerful capability in Statiq Web (it never really made sense in Framework anyway).
- Fixed default `GenerateSitemap` document destination to "sitemap.xml".

# 1.0.0-beta.7

- Added a `IDocument.GetParent()` overload that uses the current execution context and doesn't require passing in a set of candidate documents.
- The `GenerateFeeds` module now replaces relative links in item descriptions and content.
- Added a new `AbsolutizeLinks` module to `Statiq.Html` to turn all links in a document to absolute.

# 1.0.0-beta.6

- Refactored the `ValidateLinks` module to accept `Config<bool>` settings.
- Changed shortcode semantics to only pass new metadata down (metadata is no longer merged up to host document, even though no existing shortcodes did that anyway).
- Refactored shortcodes to return a new `ShortcodeResult` object that includes content and nested metadata.
- Consolidated shortcode base types to only return variations of `ShortcodeResult` (instead of `Stream` and `string` versions as well). 
- Added the `ILogger` interface to `IDocument` along with default implementations to support easier document logging with a document prefix (usually the source path).

# 1.0.0-beta.5

- Added a `Markdown` shortcode to render Markdown content anywhere (for example, in another templating language like Razor).

# 1.0.0-beta.4

- Added Handlebars media types.
- Added `WithInputDocument()` to the `EnumerateValues` module to include the original input document in the enumeration.
- Fixed some edge-case bugs in `LazyDocumentMetadataValue`.
- Added `<?#^ ... /?>` as special syntax for the `Include` shortcode with the "^" character.
- Added support for HTTP/HTTPS schemes to the `Include` shortcode for including web content.
- Added `HttpClient.SendWithRetryAsync()` extension methods to send an HTTP request with a retry policy when a `HttpClient` is available.
- Added `IExecutionState.SendHttpRequestWithRetryAsync()` methods to send an HTTP request with a retry policy without needing an `HttpClient`.
- Removed the `ProcessIncludes` module in favor of the `Include` shortcode.

# 1.0.0-beta.3

- The `OrderDocuments` module will now attempt to convert incompatible values.
- Improved document path error messages to include path.
- Added support to `IDocument.ToDocument()` extensions to return or clone the original object if it's an `IDocument`.

# 1.0.0-beta.2

- Changed the `GenerateFeeds` module to order by descending publish date to meet feed conventions and adds a `GenerateFeeds.PreserveOrdering(bool)` method to revert to old behavior (#92).
- Fixed a bug with the `OptimizeFileName` stripping the path when optimizing the destination file name (#93).
- Added support for casting the dynamic object returned from `IDocument.AsDynamic()` back to an `IDocument`.
- Added a new `IReadOnlyPipelineCollection` object and exposed it via `IExecutionState.Pipelines` to provide the current set of read-only pipelines during execution.
- Added `IPipeline.GetAllDependencies()` extension methods to get the full set of `IPipeline.DependencyOf` and `IPipeline.Dependencies` for a given pipeline.
- Added `IPipeline.DependencyOf` to allow specifying which pipelines a given pipeline is a dependency of (the reverse of `IPipeline.Dependencies`).
- Tweaked the way `NormalizedPath.OptimizeFileName()` handles dashes (it no longer removes them and does a better job of collapsing them).
- Fixed `object.ToDocument()` and `object.ToDocuments()` extensions to construct the `ObjectDocument<T>` from the actual type of the object.
- Added `IDocument.AsDynamic()` (moved from the `RenderHandlebars` module, thanks @mholo65).

# 1.0.0-beta.1

- Added Statiq.Handlebars and a `RenderHandlebars` module (#67, #90, thanks @mholo65).
- Refactored the `OptimizeFileName` module and added some extra configuration methods.
- Added `NormalizedPath.OptimizeFileName()` as both an instance method and static helper to clean up and optimize file names for the web.
- Moved `ForEachDocument` and `ForAllDocuments` modules to Statiq.Common so they can be used as base module classes for extensions.
- Fixed a bug with the `SetDestination` module when a string-based path is used that starts with a "." but isn't an extension.
- Fixed an unusual edge-case bug when evaluating scripts with assemblies that have the same simple name (I.e. LINQPad queries).
- Added `IHtmlHelper.DocumentLink()` HTML helper extensions to Statiq.Razor.
- Removed "theme" from the set of default input paths (added by default only in Statiq Web).
- Changed `CommandUtilities` in Statiq.App public.
- Moved the serve command to Statiq Web.
- Moved the preview command to Statiq Web.
- Moved Statiq.Hosting to Statiq Web as Statiq.Web.Hosting.
- Moved Statiq.Aws to Statiq Web as Statiq.Web.Aws.
- Moved Statiq.Azure to Statiq Web as Statiq.Web.Azure.
- Moved Statiq.Netlify to Statiq Web as Statiq.Web.Netlify.
- Moved Statiq.GitHub to Statiq Web as Statiq.Web.GitHub.
- Moved `ActionFileSystemWatcher` to Statiq.Common and made it public.
- Moved `InterlockedBool` to Statiq.Common and made it public.
- Added `ShortcodeHelper` static class to Statiq.Common and moved shortcode argument parsing helper method there. 
- Moved HTML-based shortcodes to Statiq.Web.
- Removed the need to pass `IExecutionContext` to a bunch of different extension methods that can rely on `IExecutionContext.Current`.
- Added `IExecutionContext.HasCurrent` to check if a current execution context is available.
- Changed `IExecutionContext.Current` to throw if no execution context is available.
- Added `IDocument` extensions to clone documents from string or `Stream` content.
- Added `IExecutionContext` extensions to create documents from string or `Stream` content.
- Added `DocumentShortcode` and `SyncDocumentShortcode` as base classes for single document-based shortcodes.
- Added `ContentShortcode` and `SyncContentShortcode` as base classes for simple string-based shortcodes.
- Renamed `IMetadata.GetDocumentList()` to `IMetadata.GetDocuments()` and added a new `IMetadata.GetDocumentList()` that returns a `DocumentList<IDocument>`.
- Changed `IPipelineOutputs` and `IEnumerable<IDocument>` extensions to return `DocumentList<TDocument>`.
- Added `DocumentList<TDocument>` which wraps a set of documents, eliminating nulls, and provides an indexer for filtering destinations.
- Added `IEnumerable<IDocument>.FirstOrDefaultSource()` and `IEnumerable<IDocument>.FirstOrDefaultDestination()` extensions.

# 1.0.0-alpha.29

- Moved a bunch of `IBootstrapper` extensions to Statiq.Common so they're available from extension libraries in an `IInitializer`.
- Renamed `IConfigurableBootstrapper` to `IBootstrapper`.
- Added a new `IInitializer` interface that can be used for library/module initialization (but only when using the `Bootstrapper`).
- Refactored the `RenderRazor` module to use the built-in service collection when possible.
- Added a new `IServiceCollection.AddRazor()` extension to register Razor services out-of-band.
- Refactored the `ClassCatalog` to `Statiq.Common` and exposed it via the `IExecutionState` interface.
- Added `.WithSource()` to the `PaginateDocuments` and `GroupDocuments` modules.
- Added a new `Keys.Order` key and made the `OrderDocuments` module support it.
- Added a `keepExisting` parameter to the `GenerateExcerpt` module.
- Removed some ambiguous `IShortcodeColletion.Add()` extensions.
- Added a bunch of `Bootstrapper.AddShortcode()` extensions.
- Added a new `ForAllDocuments` module that can act as a parent module to arbitrary child modules.
- Added a new `If` shortcode (#789).
- Added a new `ForEach` shortcode (#789).
- Added a `TypeHelper.TryConvert()` method that takes a target `Type`.
- Added support for "script strings" to metadata get extensions (if the key starts with "=>" it will be treated as a script and evaluated instead of getting the metadata value directly).
- Refactored `IShortcode` to return multiple shortcode result documents and concatenates their content.
- Modified `CreateTree` sort delegate to include the `IExecutionContext` and to sort by input document order by default (instead of path/file name).
- Added a `IDocument.IdEquals()` extension method.
- Added a `IDocument.GetLink()` extension method that calls `IExecutionContext.GetLink()`.
- Added a `IDocument.HasChildren()` extension method.
- Added an empty constructor to `OrderDocuments` that orders documents by the `Keys.Index` value and then the file name by default.
- Added a `IConfig.Cast<TValue>()` convenience extension method.

# 1.0.0-alpha.28

- Added `ctx` and `doc` shorthand properties to the scripted metadata script host.
- Ensured that scripted metadata uses a strongly-typed property in the script host for metadata properties like `Source` and `Destination`.
- Added ".yml" to file extensions mapped to the "text/yaml" media type.
- Added ability to include all inputs in generated feeds from `GenerateFeeds` by setting maximum items to 0
- Refactored Statiq.Hosting usage of Newtonsoft.Json to System.Text.Json.
- Moved the `ParseJson` module into Statiq.Core, refactored it to use System.Text.Json, and removed the `Statiq.Json` extension library.
- Renamed `IMetadata.GetNestedMetadata()` to `IMetadata.GetMetadata()`.
- Added `TypeHelper.RegisterTypeConverter()` methods to register type converters at runtime.
- Added a type converter that can convert `IMetadata` to `IDocument`.

# 1.0.0-alpha.27

- Added support to `MergeDocuments` and `MergeMetadata` for keeping existing metadata when merging.
- Added a new `MergeContent` module that merges content from child modules to input documents.
- Added an overload to the `SetContent` module that accepts a full `IContentProvider`.
- Fixed a small bug with `MetadataDictionary` initialization when items content `IMetadataValue` or `Config<T>` values.
- Added several new document metadata outputs to the `Paginate` module include `Previous` and `Next`.
- Added a new `LazyDocumentMetadataValue` that can be used to lazily find a given document as a metadata value (I.e. after cloning).
- Renamed the "Transform" phase to "PostProcess" to better reflect the intended semantics and make it easier to explain.

# 1.0.0-alpha.26

- Added a phase timeline graphic to the execution summary output.
- Added some `Span<char>.Replace()` extension methods.
- Added `NormalizedPath.ReplaceInvalidFileNameChars()` and `NormalizedPath.ReplaceInvalidPathChars()` static methods.
- Added `IMetadata.GetRawEnumerable()` and an `IMetadata.GetRawEnumerable()` extension method to make enumerating raw key-value pairs easier.
- Added a new `ExecuteBranch` module that can execute multiple branches of modules.
- Added config overrides to the modules that operate on document sets.
- Fixes a bug with `MirrorResources` and files that contain "index" in their name.
- Combines `FilePath` and `DirectoryPath` into a single consolidated `NormalizedPath (#79).
- Fixes a race condition in `Process.WaitForExit(int)` calls (thanks @duncanawoods).

# 1.0.0-alpha.25

- Added support for object-based settings to the bootstrapper (as opposed to just initial string-based configuration).
- Added support for `IConfig` metadata values.
- Added `LastWriteTime` and `CreationTime` to `IFileSystemEntry`.
- Fixed a race condition in `ScriptMetadataValue`.
- Changed behavior of `SetDestination` when using a config value to make `DestinationPath`, `DestinationFileName`, and `DestinationExtension` take precedence (with an option to override).
- Added a `Context` property for the current `IExecutionContext` to the available script properties (I.e., for use in metadata value scripts via "=>" notation).

# 1.0.0-alpha.24

- Added new `ProcessHtml` module for more flexible processing of DOM nodes.
- Added new `IEnumerable<IDocument>.Flatten()` extension to flatten document trees.
- Added new `IEnumerable<IDocument>.FilterSources()` and `IEnumerable<IDocument>.FilterDestinations()` extension methods.
- Added a new `FilterDestinations` module to filter documents by destination path.
- Added a `Config.ContainsSettings(params string[] keys)` config factory to return whether the settings contain all the specified keys.
- Refactored some methods from `IExecutionContext` into `IExecutionState` and moved implementation to `Engine`.
- Added new `EnumerateValues` module that will clone or create documents for each item in an enumeration.
- Added `Keys.ExcludeFromEvaluation` that can exclude all or some metadata values from automatic script evaluation.
- Added `IMetadata.GetNestedMetadata()` to get a nested metadata value (not called `IMetadata.GetMetadata()` to avoid conflicts with the old previous method of that name).
- Renamed `IMetadata.GetMetadata()` to `IMetadata.FilterMetadata()` which now returns a `FilteredMetadata` instance.
- Added new `FilteredMetadata` class to filter underlying metadata items by key(s).
- Refactored a bunch of default interface implementations back to extension methods (turns out default interface implementations are a little awkward to maintain).
- Moved scripting support and the `CompileScript` and `EvaluateScript` modules to `Statiq.Core`.
- Added `Microsoft.CodeAnalysis` to `Statiq.Core`.
- Metadata and configuration settings that are a string starting with "=>" are now considered "scripted" and the content to the right of the arrow will be lazily evaluated as C# code - this is a **big deal** and the use cases will become apparent over time (I have lots of big ideas around this feature).
- Removed the `InterpolateMetadata` module in favor of more robust built-in scripted metadata.
- Removed the `Statiq.CodeAnalysis.IDocumentExtensions.Interpolate()` extension method in favor of more robust built-in scripted metadata.
- Changed `CancellationTokenSource` uses inside the engine to `CancellationToken` since the engine does not itself cancel execution.
- Surfaced the `CancellationToken` for a given execution through the `IExecutionState`.
- Added a check to ensure the engine is only performing one execution at a time (the outer execution loop is not concurrently safe).
- Major refactoring involving engine and execution context interfaces, added a new `IExecutionState` interface that essentially represents a run-time engine.
- Added a new `DocumentIdComparer` to compare documents by ID (#69, thanks @mholo65).
- Removed `IParallelModule.WithSequentialExecution()` and standardized on `.WithParallelExecution(false)` instead to make default behavior of running in parallel clearer (I.e., you have to turn it off).
- Added additional configuration methods to `CacheDocuments` providing more control over when to invalidate cached documents (#78).
- Added `IReadOnlyPipeline` for runtime access to pipeline data without changing modules.
- Added `IExecutionContext.Pipeline` to get the currently executing pipeline from the execution context.
- Removed `IBootstrapper` and refactored to the one true `Bootstrapper`.
- Added a `BootstrapperFactory` available via `Bootstrapper.Factory` to create bootstrappers (this will make specialized creation extensions easier to discover).

# 1.0.0-alpha.23

- Changing target for all projects to .NET Core 3.1 LTS.
- New `Eval` shortcode (#37, #68, thanks @ProH4Ck).
- Fixes the `CacheDocuments` module and excludes `IDocument.Id` from hash calculation (#74, thanks @mholo65).

# 1.0.0-alpha.22

- Fixes a bug with `MirrorResources` and relative links (#72, thanks @dafergu2).
- The `PhaseOutputs` collection now returns output documents from the most recent available phase or an empty result set if no phases were defined.
- Adds .wasm as a supported media type (required for WebAssembly streaming).

# 1.0.0-alpha.21

- Fixes a bug with parallel modules when they return a null enumerable.
- Adds `AnalyzeCSharp.WithCompilationAssemblyName()` to set the name of the module compilation (#71).
- Adds a metadata key "Compilation" to `AnalyzeCSharp` output documents to get the Roslyn `Compilation` from the module (#71).
- Adds `IConfig.EnsureNonNull()` and `IConfig.EnsureNonDocument()` extensions to simplify config parameter checks.
- Refactors many of the configuration methods in `AnalyzeCSharp` to take configs instead of atomic values.
- Ensures namespace documents from `AnalyzeCSharp` contain the "ContainingAssembly" metadata (#70).
- Build script support for non-Windows platforms via a new `build.sh` (#65, thanks @khalidabuhakmeh).
- Adds a `Config<TValue>.MakeEnumerable()` extension to transform a config into an enumerable value.
- Adds a common `MultiConfigModuleBase` for `MultiConfigModule` and `ParallelMultiConfigModule`.
- Adds `CombineConfig` helper methods to `MultiConfigModuleBase` to help with combining config values during configuration.
- Refactors several of the `StartProcess` configuration methods to take configs.

# 1.0.0-alpha.20

- Split `DefaultFeatures.Commands` into `DefaultFeatures.BuildCommands`, `DefaultFeatures.HostingCommands`, and `DefaultFeatures.CustomCommands` for finer control.
- Renamed `DefaultsToAdd` to `DefaultFeatures`.
- Adds a new `ThrowException` module that can be used to throw exceptions based on a config value.
- Renames `BuildCommand` to `PipelinesCommand`.
- Refactors default commands by renaming `build` to `pipelines` and accepting pipelines to execute as an argument (moving the root path to an option).
- Added `IBootstrapper.AddCommands<TParent>()` to add all nested class commands of a parent type.
- Added `IBootstrapper.AddPipelines<TParent>()` to add all nested class pipelines of a parent type.
- Added `Bootstrapper.CreateDefaultWithout()` and `IBootstrapper.AddDefaultsWithout()` to create a default bootstrapper without specific components.
- Renamed `IBootstrapper.AddBuildCommand()` methods to `IBootstrapper.AddPipelineCommand()`.

# 1.0.0-alpha.19

- Added `StartProcess.WithArgument()` methods to add arguments to the module using a fluent interface.
- Added `Config<TValue>.CombineWith()` extensions for combining two configs.
- Added `Config<TValue>.Transform()` extensions for transforming from one value to another.
- Made `IExecutionContext` (re)implement `IMetadata` through `Settings`.
- Added `ToString()` overloads to `IFileSystemEntry` (can't believe those weren't already there).
- Added a `Statiq.Netlify` extension with a `DeployNetlifySite` module.
- `IDocument` now implements `IContentProviderFactory`.
- Added some additional `IContentProvider` overloads to the `DeployAppService` module.

# 1.0.0-alpha.18

- Updated several low-risk package versions.
- Adds option names to all command option values (#64, thanks @patriksvensson).
- New `serve` command for serving arbitrary folders with a local preview server (#55, #60, thanks @duracellko).
- Fix for regression when there are dependencies on pipelines that aren't executing.

# 1.0.0-alpha.17

- Small -- prefix in console/logs to help indicate when a pipeline phase is finished.
- Set longer timeouts for App Service uploads.
- Added support for uploading a zip file to Azure App Service (useful for artifact-based release processes).

# 1.0.0-alpha.16

- Fixes a bug in dependency ordering where deployment pipeline output phases were being run too early.

# 1.0.0-alpha.15

- Added `SetMediaType` module to set the media type without changing the content.
- Changed the semantics of `IDocument.ContentProvider` so that it's never null.
- Added `IContentProvider.Length` to get the content length without needing to get the stream.
- Renamed `AddMetadata` to `SetMetadata` to better match other module naming conventions and to reflect the metadata key being set might already exist.
- Renamed `ReplaceContent` to `SetContent` to better match other module naming conventions.
- Removed the pre/post render flag from `ProcessShortcodes` and uses a default delimiter of `<?# ... ?>` (the forthcoming site generator will need to define the alternate delimiter for pre-transform shortcodes directly).
- Added overloads of methods that create content providers to set the media type.
- Updated all built-in modules to set the media type whenever appropriate.
- Added `IContentProvider.MediaType` to surface the media type of content.
- Added a `MediaTypes` static class that contains an exhaustive set of media type (MIME) mappings by file extension.
- Added a `DeploySearchIndex` module that deploys an Azure search index from input document metadata.
- Added a `deploy` command that executes deployment pipelines.
- Added a new `ExecutionPolicy.Normal` policy and changed `ExecutionPolicy.Default` to specify different behavior depending on if the pipeline is a deployment pipeline.

# 1.0.0-alpha.14

- The string overload of `SetDestination` now takes either an extension or a path distinguished by a preceding dot.
- Updated Statiq.Razor to 3.0 libraries.
- Updated Statiq.Hosting to 3.0 libraries.

# 1.0.0-alpha.13

- Fixes `ExecuteIf` to work when there are no input documents and the config doesn't require one.
- Fixes bug calculating command name for generic command types.
- Refactors `CreateDocuments` and adds additional config-based overloads.

# 1.0.0-alpha.12

- Changed `IBoostrapper.ConfigureSettings()` to use a new `IConfigurationSettings` object that exposes the settings and the underlying `IConfiguration`
- Renamed the execution-time `IConfigurationSettings` to `IReadOnlyConfigurationSettings` and introduced a new mutable `IConfigurationSettings` to use in the bootstrapper.
- Several bug fixes related to settings and configuration.

# 1.0.0-alpha.11

- Changed `IBootstrapper.AddSettings()` calls to run after other configuration.
- Adds a single pattern overload to `ReadFiles`.
- Some refactoring of the base `Pipeline` class (most importantly to remove the `Dependencies` setter in favor of adding to the existing hash set).
- Adds `IReadOnlyApplicationState.IsCommand()` to determine the current command.

# 1.0.0-alpha.10

- Added extensions for CLI `IConfigurator` to allow more flexible direct configuration (such as command branches).
- Removed configuration/settings debug output on startup as it could leak secrets via environment variables or other configuration providers.
- Fix for `GenerateJson` so it executes when no input documents are provided.
- Fix for preview command when no output is generated and the output directory does not exist.
- Fix for clearing content with empty string in `ReplaceContent`.
- Renamed `EngineSettings` to `ConfigurationSettings` to reflect a broader use than just the engine.

# 1.0.0-alpha.9

- Added `IBootstrapper.AddDelegateCommand()` fluent methods to configure delegate-based commands.
- Added `IBootstrapper.AddBuildCommand()` fluent methods to configure simple commands that build specified pipelines.
- Refactored the base commands to allow consumers to derive from `EngineCommand`.
- Added a new `IEngineManager` interface to expose the engine manager to commands that derive from `EngineCommand`.
- Refactored `IEngine.Settings` and `IExecutionContext.Settings` to use a `IConfiguration` as the backing store and present it as metadata.
- Lazily creates type-based pipelines using the DI container so they can have injected services (#59).
- Adds `INamedPipeline` to allow pipeline instances to provide names.
- Changes `Module.AfterExecution()` and `Module.AfterExecutionAsync()` to pass a new `ExecutionOutputs` class instead of by ref (which doesn't work with async).
- Some tweaks to the `MirrorResources` retry policy.

# 1.0.0-alpha.8

- Adds `==` overloads to `NormalizedPath`.
- Adds a special `RenderSection()` to `StatiqRazorPage` that renders default content if the section is not defined.
- Renamed `IDocument.GetStream()` to `IDocument.GetContentStream()`.
- Renamed `IDocument.GetStringAsync()` to `IDocument.GetContentStringAsync()`.
- Renamed `IDocument.GetBytesAsync()` to `IDocument.GetContentBytesAsync()`.
- Added `IEngine.SerialExecution` and `--serial` CLI argument to run pipelines and modules in serial (#58).

# 1.0.0-alpha.7

- Adds support for deployment pipelines (`IPipeline.Deployment`) which run their output phase only after other output phases (#57).
- Fixes a bug when specifying a setting on the CLI and the bootstrapper.
- Adds `StartProcess.WithErrorExitCode()` to define a custom function for determining if the process existed in error.
- Adds new `-d`/`--defaults` and a flag to the engine to indicate if default pipelines should be run independent of specified pipelines.
- Renames `SimpleBuildCommand` to `CustomBuildCommand` and adds support for the default pipelines flag.

# 1.0.0-alpha.6

- Adds back a `ExecuteModules` module that works like the old `Branch` module used to by dropping any output documents from the child modules.
- Tweaks to the placeholder factory in `CreateTree`.
- Fix for the JavaScript engine getting reset on execution.
- No longer strips "Pipeline" from the end of pipeline classes for the pipeline name since `nameof` is often used to refer to pipelines.

# 1.0.0-alpha.5

- Adds `StartProcess.KeepContent()` to prevent replacing document content with process output.
- Adds `StartProcess.OnlyOnce()` to only execute the process once.
- Renamed `PipelineTrigger` to `ExecutionPolicy`.
- Adds type-based methods for adding pipelines.
- All `IPipeline` implementations from the entry assembly are added by the bootstrapper by default.
- All `ICommand` implementations from the entry assembly are added by the bootstrapper by default.
- Adds ability to specify which defaults to add to the bootstrapper.
- Made `EngineManager` public so it can be used by custom commands.
- Adds a new `SimpleBuildCommand` base command to make creating new pipeline-specific build commands easier.
- Adds `AddPipelines()` and `AddCommands()` methods to add pipelines and commands from the entry or a given assembly.
- Adds `MultiConfigModule`, `ParallelMultiConfigModule`, `SyncMultiConfigModule`, and `ParallelSyncMultiConfigModule` base classes for modules that use multiple `Config<T>` values.
- Adds `ExecutionPipeline` base pipeline for use when a custom pipeline that runs code for each phase is needed.
- Adds new `ZipDirectory` module.
- Renames the `Statiq.AmazonWebServices` extension library to `Statiq.Aws`.
- Adds `Statiq.Azure` extension library.
- Adds new `DeployAppService` module.

# 1.0.0-alpha.4

- Updated to .NET Core 3.0 final.
- Isolated pipelines can now be dependencies of other pipelines (but output documents still can't be accessed).
- Renames delegate-based `IBootstrapper.AddSettings()` overload to `IBoostrapper.ConfigureSettings()`.
- Renames `IBootstrapper.AddServices()` to `IBoostrapper.ConfigureServices()`.
- Adds `IBootstrapper.ConfigureEngine()`.
- Adds a `StartProcess` module to start a process and create a document from it's output or run it in the background.
- The bootstrapper now adds environment variables to the settings by default with ALL_CAPS keys.
- Any setting with an ALL_CAPS key is masked during debug output on startup.
- Ongoing console logging improvements.
- Added trigger conditions to pipelines to include always running or manually running.
- Added a `-p`/`--pipeline` CLI argument to indicate which pipelines to execute.

# 1.0.0-alpha.3

- New execution summary table logged after execution.
- New console logger with better output.
- More refactoring of base `Module` before/after methods.

# 1.0.0-alpha.2

- Renamed the GitHub project/repo to "Statiq.Framework" to match forthcoming "Statiq.Web" and to distinguish between primary code repos (prefixed by "Statiq.") and themes, etc. Also note the upcoming Statiq app will be known as Statiq Web from now on (as opposed to Statiq Framework).
- The engine now returns a `IPipelineOutputs` with the result documents from each pipeline.
- Adds global events `BeforeModuleExecution` and `AfterModuleExecution` with ability to override outputs.
- Adds a new global event mechanism via `IEventCollection`, `IReadOnlyEventCollection`, `IEngine.Events`, and `IExecutionContext.Events`.
- Refactored the base module classes to include a before/after execution method, made the execution methods `protected`, and renamed the execution methods for clarity.
- Added property setters with null checks to `Pipeline` so it works better as a base class and you can define the phase modules directly as properties.
- Cleaned up `ModuleList` methods to remove overload ambiguity between `params IModule[]` and `IEnumerable<IModule>`.
- Added an implicit operator from `IModule[]` to `ModuleList`.
- Raw application arguments as well as application input are now surfaced through a new `IReadOnlyApplicationState` object in the `IExecutionContext`, taking the place of the `ApplicationInput` property.
- Adds a bunch of `Config.FromSettings()` methods that get values from a `IReadOnlySettings`.
- "Pipeline" is now trimmed from the end of type names when types are added as a pipeline to a pipeline collection.

# 1.0.0-alpha.1

- Statiq Framework is comprehensive "reboot" of Wyam.