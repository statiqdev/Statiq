using System;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Razor.Hosting;

namespace Statiq.Razor
{
    internal abstract class CompilationResult : IDisposable
    {
        /// <summary>
        /// Loads the assembly and gets the compiled item.
        /// </summary>
        public abstract RazorCompiledItem GetCompiledItem();

        public IRazorPage GetPage(string relativePath)
        {
            RazorCompiledItem compiledItem = GetCompiledItem();
            IRazorPage page = (IRazorPage)Activator.CreateInstance(compiledItem.Type);
            page.Path = relativePath;
            return page;
        }

        public abstract void Dispose();
    }
}