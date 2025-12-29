using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Spectre.Console.Cli;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.App
{
    [Description("Tests a globbing pattern against a sample path.")]
    public class GlobTestCommand : Command<GlobTestCommandSettings>
    {
        public override int Execute(
            CommandContext context,
            GlobTestCommandSettings settings,
            CancellationToken cancellationToken = default)
        {
            // Make sure path is absolute
            NormalizedPath path = new NormalizedPath(settings.Path);
            if (path.IsRelative)
            {
                path = NormalizedPath.AbsoluteRoot / path;
            }

            // Test the pattern
            TestFileProvider fileProvider = new TestFileProvider
            {
                { path }
            };
            IDirectory directory = fileProvider.GetDirectory("/");
            IFile[] matches = Globber.GetFiles(directory, new[] { settings.Pattern }).ToArray();
            if (matches.Length > 0)
            {
                Console.WriteLine("The path DOES match the pattern");
            }
            else
            {
                Console.WriteLine("The path DOES NOT match the pattern");
            }

            return 0;
        }
    }
}