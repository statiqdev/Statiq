using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Testing
{
    public class CountModule : Module
    {
        public int AdditionalOutputs { get; set; } // Controls how many additional outputs are spawned
        public string ValueKey { get; set; } // This is the key used for storing the value in the metadata
        public int Value { get; set; } // This is incremented on every call and output and added to the metadata
        public int ExecuteCount { get; set; }
        public int InputCount { get; set; }
        public int OutputCount { get; set; }
        public bool CloneSource { get; set; } // Indicates whether the clone call should output a source
        public bool EnsureInputDocument { get; set; }

        public CountModule(string valueKey)
        {
            ValueKey = valueKey;
        }

        /// <inheritdoc />
        protected override async Task<IEnumerable<IDocument>> ExecuteContextAsync(IExecutionContext context)
        {
            int sourceCount = 0;
            ExecuteCount++;
            List<IDocument> results = new List<IDocument>();

            // Add an initial document if there isn't already one
            ImmutableArray<IDocument> inputs = context.Inputs;
            if (inputs.Length == 0 && EnsureInputDocument)
            {
                inputs = ImmutableArray.Create(context.CreateDocument());
            }

            foreach (IDocument input in inputs)
            {
                string inputContent = await input.GetContentStringAsync();
                InputCount++;
                for (int c = 0; c < AdditionalOutputs + 1; c++)
                {
                    OutputCount++;
                    Value++;
                    if (CloneSource)
                    {
                        results.Add(
                            input.Clone(
                                new NormalizedPath(ValueKey + sourceCount++, PathKind.Absolute),
                                null,
                                new Dictionary<string, object> { { ValueKey, Value } },
                                context.GetContentProvider(inputContent is null ? Value.ToString() : inputContent + Value)));
                    }
                    else
                    {
                        results.Add(
                            input.Clone(
                                new Dictionary<string, object> { { ValueKey, Value } },
                                context.GetContentProvider(inputContent is null ? Value.ToString() : inputContent + Value)));
                    }
                }
            }
            return results;
        }
    }
}
