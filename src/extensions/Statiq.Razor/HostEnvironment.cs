using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using IFileProvider = Microsoft.Extensions.FileProviders.IFileProvider;

namespace Statiq.Razor
{
    internal class HostEnvironment : IWebHostEnvironment
    {
        public HostEnvironment(IFileProvider fileProvider)
        {
            EnvironmentName = "Statiq";

            // This gets used to load dependencies and is passed to Assembly.Load()
            ApplicationName = typeof(HostEnvironment).Assembly.FullName;

            WebRootPath = ((FileSystemFileProvider)fileProvider).StatiqFileSystem.RootPath.FullPath;
            WebRootFileProvider = fileProvider;
            ContentRootPath = WebRootPath;
            ContentRootFileProvider = WebRootFileProvider;
        }

        public string EnvironmentName { get; set; }
        public string ApplicationName { get; set; }
        public string WebRootPath { get; set; }
        public IFileProvider WebRootFileProvider { get; set; }
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; }
    }
}