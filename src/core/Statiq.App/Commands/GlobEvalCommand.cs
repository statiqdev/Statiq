using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    [Description("Evaluates a globbing pattern against an existing path.")]
    public class GlobEvalCommand : Command<GlobEvalCommandSettings>
    {
        public override int Execute(CommandContext context, GlobEvalCommandSettings settings,
            CancellationToken cancellationToken = default)
        {
            // Make sure path is absolute
            NormalizedPath path = new NormalizedPath(settings.Path);
            if (path.IsRelative)
            {
                path = new NormalizedPath(Directory.GetCurrentDirectory()) / path;
            }

            FileSystem fileSystem = new FileSystem();
            LocalFileProvider fileProvider = new LocalFileProvider(fileSystem);
            fileSystem.FileProvider = fileProvider;
            IDirectory directory = fileProvider.GetDirectory(path);
            foreach (IFile match in (IEnumerable<IFile>) Globber.GetFiles(directory, new[] { settings.Pattern })
                         .ToArray())
            {
                Console.WriteLine(match.Path.FullPath);
            }

            return 0;
        }

        private class FileSystem : IFileSystem
        {
            public IFileProvider FileProvider { get; set; }
            IFileProvider IReadOnlyFileSystem.FileProvider => FileProvider;

            public NormalizedPath RootPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public PathCollection InputPaths => throw new NotImplementedException();
            public IDictionary<NormalizedPath, NormalizedPath> InputPathMappings => throw new NotImplementedException();
            public PathCollection ExcludedPaths => throw new NotImplementedException();
            public NormalizedPath OutputPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public NormalizedPath TempPath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            public NormalizedPath CachePath { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
            NormalizedPath IReadOnlyFileSystem.RootPath => throw new NotImplementedException();
            IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.InputPaths => throw new NotImplementedException();
            IReadOnlyDictionary<NormalizedPath, NormalizedPath> IReadOnlyFileSystem.InputPathMappings => throw new NotImplementedException();
            IReadOnlyList<NormalizedPath> IReadOnlyFileSystem.ExcludedPaths => throw new NotImplementedException();
            NormalizedPath IReadOnlyFileSystem.OutputPath => throw new NotImplementedException();
            NormalizedPath IReadOnlyFileSystem.TempPath => throw new NotImplementedException();
            NormalizedPath IReadOnlyFileSystem.CachePath => throw new NotImplementedException();
            IFileWriteTracker IReadOnlyFileSystem.WriteTracker => throw new NotImplementedException();
        }
    }
}
