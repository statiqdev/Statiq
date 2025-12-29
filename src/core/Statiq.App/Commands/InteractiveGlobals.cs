using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Statiq.Common;
using Statiq.Core;

namespace Statiq.App
{
    /// <summary>
    /// This is the set of globals for the REPL. You can add additional methods to the
    /// REPL by creating extension methods.
    /// </summary>
    public class InteractiveGlobals
    {
        private readonly Action _triggerExecution;
        private readonly Action _triggerExit;

        public InteractiveGlobals(IEngine engine, Action triggerExecution, Action triggerExit)
        {
            Engine = engine;
            _triggerExecution = triggerExecution;
            _triggerExit = triggerExit;
        }

        [Description("The  engine.")]
        public IEngine Engine { get; }

        [Description("The service provider.")]
        public IServiceProvider Services => Engine.Services;

        [Description("A collection of output pages from all pipelines.")]
        public FilteredDocumentList<IDocument> OutputPages => Engine.OutputPages;

        [Description("All outputs from all pipelines.")]
        public IPipelineOutputs Outputs => Engine.Outputs;

        [Description("The settings collection.")]
        public IReadOnlySettings Settings => Engine.Settings;

        [Description("The file system.")]
        public IReadOnlyFileSystem FileSystem => Engine.FileSystem;

        [Description("A collection of all pipelines.")]
        public IReadOnlyPipelineCollection Pipelines => Engine.Pipelines;

        [Description("Triggers another execution.")]
        public void Execute() => _triggerExecution();

        [Description("Exits the application.")]
        public void Exit() => _triggerExit();

        [Description("Exits the application.")]
        public void Quit() => Exit();

        [Description("Prints the global methods and properties.")]
        public void Help() => Help(GetType());

        [Description("Reflects over an object to print its methods and properties (optionally filtering to members that contain a string).")]
        public void Help(object obj, string filter = null)
        {
            if (obj is null)
            {
                Console.WriteLine();
                Console.WriteLine("Object is null.");
                Console.WriteLine();
            }
            else
            {
                Help(obj.GetType(), filter);
            }
        }

        [Description("Reflects over a type to print its methods and properties (optionally filtering to members that contain a string).")]
        public void Help(Type type, string filter = null)
        {
            // Methods
            List<MethodInfo> methodInfos = ReflectionHelper.GetCallableMethods(type)
                .Concat(ReflectionHelper.GetExtensionMethods(type, Engine.ClassCatalog.Values))
                .Where(m => filter is null || m.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (methodInfos.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("METHODS");
                Console.WriteLine();
                foreach (MethodInfo methodInfo in methodInfos)
                {
                    string helpText = "- " + ReflectionHelper.GetMethodSignature(methodInfo);
                    DescriptionAttribute description = methodInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (description is object)
                    {
                        helpText += " // " + description.Description;
                    }
                    Console.WriteLine(helpText);
                }
            }

            // Properties
            List<PropertyInfo> propertyInfos = ReflectionHelper.GetCallableProperties(type)
                .Where(p => filter is null || p.Name.Contains(filter, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (propertyInfos.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("PROPERTIES");
                Console.WriteLine();
                foreach (PropertyInfo propertyInfo in propertyInfos)
                {
                    string helpText = "- " + ReflectionHelper.GetPropertySignature(propertyInfo, false, true);
                    DescriptionAttribute description = propertyInfo.GetCustomAttribute<DescriptionAttribute>();
                    if (description is object)
                    {
                        helpText += " // " + description.Description;
                    }
                    Console.WriteLine(helpText);
                }
            }

            Console.WriteLine();
        }
    }
}
