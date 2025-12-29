using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using YamlDotNet.RepresentationModel;
using Statiq.Yaml.Dynamic;
using Statiq.Common;

namespace Statiq.Yaml
{
    /// <summary>
    /// Parses YAML content for each input document and stores the result in it's metadata.
    /// </summary>
    /// <remarks>
    /// Parses the content for each input document and then stores a dynamic object
    /// representing the first YAML document in metadata with the specified key. If no key is specified,
    /// then the dynamic object is not added. You can also flatten the YAML to add top-level pairs directly
    /// to the document metadata.
    /// </remarks>
    /// <category name="Metadata" />
    public class ParseYaml : ParallelSyncModule
    {
        private readonly bool _flatten;
        private readonly string _key;

        /// <summary>
        /// The content of the input document is parsed as YAML. All root-level scalars are added to the input document's
        /// metadata. Any more complex YAML structures are ignored. This is best for simple key-value YAML documents.
        /// </summary>
        public ParseYaml()
        {
            _flatten = true;
        }

        /// <summary>
        /// The content of the input document is parsed as YAML. A dynamic object representing the first YAML
        /// document is set as the value for the given metadata key. See YamlDotNet.Dynamic for more details
        /// about the dynamic YAML object. If flatten is true, all root-level scalars are also added
        /// to the input document's metadata.
        /// </summary>
        /// <param name="key">The metadata key in which to set the dynamic YAML object.</param>
        /// <param name="flatten">If set to <c>true</c>, all root-level scalars are also added to the input document's metadata.</param>
        public ParseYaml(string key, bool flatten = false)
        {
            _key = key;
            _flatten = flatten;
        }

        protected override IEnumerable<IDocument> ExecuteInput(IDocument input, IExecutionContext context)
        {
            List<Dictionary<string, object>> documentMetadata = new List<Dictionary<string, object>>();
            using (TextReader contentReader = new StreamReader(input.GetContentStream()))
            {
                YamlStream yamlStream = new YamlStream();
                yamlStream.Load(contentReader);
                foreach (YamlDocument document in yamlStream.Documents)
                {
                    // If this is a sequence, get a document for each item
                    if (document.RootNode is YamlSequenceNode rootSequence)
                    {
                        documentMetadata.AddRange(rootSequence.Children.Select(x => GetDocumentMetadata(x, context)));
                    }
                    else
                    {
                        // Otherwise, just get a single set of metadata
                        documentMetadata.Add(GetDocumentMetadata(document.RootNode, context));
                    }
                }
            }
            if (documentMetadata.Count == 0 && _flatten)
            {
                return input.Yield();
            }
            return documentMetadata.Select(metadata => input.Clone(metadata));
        }

        private Dictionary<string, object> GetDocumentMetadata(YamlNode node, IExecutionContext context)
        {
            Dictionary<string, object> metadata = new Dictionary<string, object>();

            // Get the dynamic representation
            if (!string.IsNullOrEmpty(_key))
            {
                metadata[_key] = new DynamicYaml(node);
            }

            // Also get the flat metadata if requested
            if (_flatten)
            {
                if (!(node is YamlMappingNode mappingNode))
                {
                    throw new InvalidOperationException("Cannot flatten YAML content that doesn't have a mapping node at the root (or within a root sequence).");
                }

                // Map scalar-to-scalar children
                foreach (KeyValuePair<YamlNode, YamlNode> child in
                    mappingNode.Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlScalarNode))
                {
                    metadata[((YamlScalarNode)child.Key).Value] = ((YamlScalarNode)child.Value).Value;
                }

                // Map simple sequences
                foreach (KeyValuePair<YamlNode, YamlNode> child in
                    mappingNode.Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlSequenceNode && ((YamlSequenceNode)y.Value).All(z => z is YamlScalarNode)))
                {
                    metadata[((YamlScalarNode)child.Key).Value] = ((YamlSequenceNode)child.Value).Select(a => ((YamlScalarNode)a).Value).ToArray();
                }

                // Map object sequences
                foreach (KeyValuePair<YamlNode, YamlNode> child in
                    mappingNode.Children.Where(y => y.Key is YamlScalarNode && y.Value is YamlSequenceNode && ((YamlSequenceNode)y.Value).All(z => z is YamlMappingNode)))
                {
                    metadata[((YamlScalarNode)child.Key).Value] = ((YamlSequenceNode)child.Value).Select(a => context.CreateDocument(GetDocumentMetadata(a, context))).ToArray();
                }

                // Note: No support for mixed sequences of YamlMappingNode and YamlScalarNode together
            }

            return metadata;
        }
    }
}