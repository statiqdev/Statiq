using System.IO;
using System.Threading.Tasks;
using Scriban;
using Scriban.Parsing;
using Scriban.Runtime;
using Statiq.Common;

namespace Statiq.Scriban
{
    public class TemplateLoader : ITemplateLoader
    {
        private readonly IReadOnlyFileSystem _fileSystem;

        public TemplateLoader(IReadOnlyFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public string GetPath(TemplateContext context, SourceSpan callerSpan, string templateName)
        {
            NormalizedPath currentPath = new NormalizedPath(context.CurrentSourceFile);
            NormalizedPath templatePath = currentPath.Parent.Combine(templateName);

            return templatePath.FullPath;
        }

        public string Load(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            IFile templateFile = _fileSystem.GetFile(templatePath);

            if (templateFile is null)
            {
                return null;
            }

            using Stream fileStream = templateFile.OpenRead();
            using StreamReader reader = new StreamReader(fileStream);

            return reader.ReadToEnd();
        }

        public async ValueTask<string> LoadAsync(TemplateContext context, SourceSpan callerSpan, string templatePath)
        {
            IFile templateFile = _fileSystem.GetFile(templatePath);

            if (templateFile is null)
            {
                return null;
            }

            return await templateFile.ReadAllTextAsync();
        }
    }
}