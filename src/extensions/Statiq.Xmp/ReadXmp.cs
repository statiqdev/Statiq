using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using MetadataExtractor;
using MetadataExtractor.Formats.Xmp;
using Microsoft.Extensions.Logging;
using Statiq.Common;
using XmpCore;

namespace Statiq.Xmp
{
    /// <summary>
    /// Reads XMP data from the input documents and adds it to the document metadata.
    /// </summary>
    /// <remarks>
    /// The <see cref="IDocument.Source"/> of each input document will be used to
    /// locate any sidecar files.
    /// </remarks>
    /// <category name="Metadata" />
    public class ReadXmp : Module
    {
        private readonly bool _skipElementOnMissingData;
        private readonly bool _errorOnDoubleKeys = true;
        private readonly bool _delocalizing = true;
        private readonly bool _flatten = true;
        private readonly List<XmpSearchEntry> _toSearch = new List<XmpSearchEntry>();

        private readonly Dictionary<string, string> _namespaceAlias =
            new Dictionary<string, string>
            {
                { "dc", "http://purl.org/dc/elements/1.1/" },
                { "xmpRights", "http://ns.adobe.com/xap/1.0/rights/" },
                { "cc", "http://creativecommons.org/ns#" },
                { "xmp", "http://ns.adobe.com/xap/1.0/" },
                { "xml", "http://www.w3.org/XML/1998/namespace" },
            };

        /// <summary>
        /// Reads XMP data from the input documents and adds it to the document metadata with the specified options.
        /// </summary>
        /// <param name="skipElementOnMissingMandatoryData">If mandatory data is missing, the element will be skipped.</param>
        /// <param name="errorsOnDoubleKeys">If <c>true</c> (the default), an error will be produced if the XML metadata would overwrite existing
        /// document metadata. If <c>false</c>, the XMP metadata overrides the existing metadata.</param>
        /// <param name="delocalizing">If <c>true</c> (the default), when multiple elements with different languages are present, the local language will be used to choose the correct element.</param>
        /// <param name="flatten">If <c>true</c> (the default), when an array has only one element the output metadata is reduced to the single element.</param>
        public ReadXmp(bool skipElementOnMissingMandatoryData = false, bool errorsOnDoubleKeys = true, bool delocalizing = true, bool flatten = true)
        {
            _skipElementOnMissingData = skipElementOnMissingMandatoryData;
            _errorOnDoubleKeys = errorsOnDoubleKeys;
            _delocalizing = delocalizing;
            _flatten = flatten;
        }

        /// <summary>
        /// Specifies an XML element to find in the XMP data along with the metadata key that will be used to set it in the document.
        /// </summary>
        /// <param name="xmpPath">The tag name of the XMP element including the namespace prefix.</param>
        /// <param name="targetMetadata">The metadata key where the value should be added to the document.</param>
        /// <param name="isMandatory">Specifies that the input should contain the XMP metadata.</param>
        /// <returns>The current module instance.</returns>
        public ReadXmp WithMetadata(string xmpPath, string targetMetadata, bool isMandatory = false)
        {
            _toSearch.Add(new XmpSearchEntry(this, isMandatory, targetMetadata, xmpPath));
            return this;
        }

        /// <summary>
        /// Adds or overrides a namespace for resolving namespace prefixes of the <c>xmpPath</c>
        /// specified in <see cref="WithMetadata(string, string, bool)"/>. Several default namespaces are predefined:
        /// <list>
        /// <item>
        /// <term>dc - </term>
        /// <description>http://purl.org/dc/elements/1.1/</description>
        /// </item>
        /// <item>
        /// <term>xmpRights - </term>
        /// <description>http://ns.adobe.com/xap/1.0/rights/</description>
        /// </item>
        /// <item>
        /// <term>cc - </term>
        /// <description>http://creativecommons.org/ns#</description>
        /// </item>
        /// <item>
        /// <term>xmp - </term>
        /// <description>http://ns.adobe.com/xap/1.0/</description>
        /// </item>
        /// <item>
        /// <term>xml - </term>
        /// <description>http://www.w3.org/XML/1998/namespace</description>
        /// </item>
        /// </list>
        /// </summary>
        /// <param name="xmlNamespace">The namespace to define.</param>
        /// <param name="alias">The namespace alias.</param>
        /// <returns>The current module instance.</returns>
        public ReadXmp WithNamespace(string xmlNamespace, string alias)
        {
            _namespaceAlias[alias] = xmlNamespace;
            return this;
        }

        protected override async Task<IEnumerable<IDocument>> ExecuteInputAsync(IDocument input, IExecutionContext context)
        {
            XmpDirectory xmpDirectory;
            try
            {
                using (Stream stream = input.GetContentStream())
                {
                    xmpDirectory = ImageMetadataReader.ReadMetadata(stream).OfType<XmpDirectory>().FirstOrDefault();
                }
            }
            catch (Exception)
            {
                xmpDirectory = null;
            }
            if (xmpDirectory is null)
            {
                // Try to read sidecarfile
                if (!input.Source.IsNull)
                {
                    IFile sidecarFile = context.FileSystem.GetInputFile(input.Source.AppendExtension(".xmp"));
                    if (sidecarFile.Exists)
                    {
                        MemoryStream xmpBytes = context.MemoryStreamFactory.GetStream();
                        using (Stream xmpStream = sidecarFile.OpenRead())
                        {
                            await xmpStream.CopyToAsync(xmpBytes);
                        }
                        xmpDirectory = new XmpReader().Extract(xmpBytes.ToArray());
                    }
                }
            }
            if (xmpDirectory is null)
            {
                if (_toSearch.Any(y => y.IsMandatory))
                {
                    context.LogWarning($"File doe not contain Metadata or sidecar file ({input.ToSafeDisplayString()})");
                    if (_skipElementOnMissingData)
                    {
                        return null;
                    }
                }
                return input.Yield();
            }

            Dictionary<string, object> newValues = new Dictionary<string, object>();

            TreeDirectory hierarchicalDirectory = TreeDirectory.GetHierarchicalDirectory(xmpDirectory);

            foreach (XmpSearchEntry search in _toSearch)
            {
                try
                {
                    TreeDirectory metadata = hierarchicalDirectory.Childrean.Find(y => search.PathWithoutNamespacePrefix == y.ElementName && search.Namespace == y.ElementNameSpace);

                    if (metadata is null)
                    {
                        if (search.IsMandatory)
                        {
                            context.LogError($"Metadata does not Contain {search.XmpPath} ({input.ToSafeDisplayString()})");
                            if (_skipElementOnMissingData)
                            {
                                return null;
                            }
                        }
                        continue;
                    }
                    object value = GetObjectFromMetadata(metadata, hierarchicalDirectory);
                    if (newValues.ContainsKey(search.MetadataKey) && _errorOnDoubleKeys)
                    {
                        context.LogError($"This Module tries to write same Key multiple times {search.MetadataKey} ({input.ToSafeDisplayString()})");
                    }
                    else
                    {
                        newValues[search.MetadataKey] = value;
                    }
                }
                catch (Exception e)
                {
                    context.LogError($"An exception occurred : {e} {e.Message}");
                    if (search.IsMandatory && _skipElementOnMissingData)
                    {
                        return null;
                    }
                }
            }
            return newValues.Count > 0 ? input.Clone(newValues).Yield() : input.Yield();
        }

        [System.Diagnostics.DebuggerDisplay("{ElementName}: {ElementValue} [{ElementArrayIndex}] ({ElementNameSpace})")]
        private class TreeDirectory
        {
            public string ElementName
            {
                get
                {
                    string path = Element?.Path;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return null;
                    }
                    string pathWithouParent = !string.IsNullOrWhiteSpace(Parent?.Element?.Path)
                        ? path.Substring(Parent.Element.Path.Length).TrimStart('/')
                        : path.TrimStart('/');
                    string pathWithoutNamespace = Regex.Replace(pathWithouParent, "^[^:]+:(?<tag>[^/]+)(/.*)?$", "${tag}");
                    return Regex.IsMatch(pathWithoutNamespace, @"\[\d+\]") ? null : pathWithoutNamespace;
                }
            }

            public int ElementArrayIndex
            {
                get
                {
                    string path = Element?.Path;
                    if (string.IsNullOrWhiteSpace(path))
                    {
                        return -1;
                    }

                    string pathWithouParent;
                    if (!string.IsNullOrWhiteSpace(Parent?.Element?.Path))
                    {
                        pathWithouParent = path.Substring(Parent.Element.Path.Length).TrimStart('/');
                    }
                    else
                    {
                        pathWithouParent = path.TrimStart('/');
                    }

                    string pathWithoutNamespace = Regex.Replace(pathWithouParent, "^[^:]+:(?<tag>[^/]+)(/.*)?$", "${tag}");

                    if (Regex.IsMatch(pathWithoutNamespace, @"\[\d+\]"))
                    {
                        return int.Parse(Regex.Replace(pathWithoutNamespace, @"\[(?<index>\d+)\]", "${index}"));
                    }

                    return -1;
                }
            }

            public bool IsArrayElement => ElementArrayIndex != -1;
            public string ElementNameSpace => Element?.Namespace;
            public string ElementValue => Element?.Value;
            public IXmpPropertyInfo Element { get; }
            public List<TreeDirectory> Childrean { get; } = new List<TreeDirectory>();

            private TreeDirectory Parent { get; set; }

            private TreeDirectory()
            {
            }

            private TreeDirectory(IXmpPropertyInfo x)
            {
                Element = x;
            }

            internal static TreeDirectory GetHierarchicalDirectory(XmpDirectory directories)
            {
                TreeDirectory root = new TreeDirectory();

                TreeDirectory[] treeNodes = directories.XmpMeta.Properties.Where(x => x.Path is object).Select(x => new TreeDirectory(x)).ToArray();

                var possibleChildrean = treeNodes.Select(x => new
                {
                    Element = x,
                    PossibleChildrean = treeNodes.Where(y => y.Element.Path != x.Element.Path && y.Element.Path.StartsWith(x.Element.Path)).ToArray()
                }).ToArray();
                var childOfRoot = possibleChildrean.Where(x => !possibleChildrean.Any(y => y.PossibleChildrean.Contains(x.Element))).ToArray();

                root.Childrean.AddRange(childOfRoot.Select(x => x.Element));
                foreach (var child in childOfRoot)
                {
                    child.Element.Parent = root;
                }

                foreach (var node in possibleChildrean)
                {
                    TreeDirectory[] childOfNode = node.PossibleChildrean.Where(x => !possibleChildrean.Any(y => node.PossibleChildrean.Contains(y.Element) && y.PossibleChildrean.Contains(x))).ToArray();

                    node.Element.Childrean.AddRange(childOfNode);
                    foreach (TreeDirectory child in childOfNode)
                    {
                        child.Parent = node.Element;
                    }
                }

                return root;
            }
        }

        private object GetObjectFromMetadata(TreeDirectory metadata, TreeDirectory hirachciDirectory)
        {
            if (metadata.Element.Options.IsArray)
            {
                IOrderedEnumerable<TreeDirectory> arreyElemnts = metadata.Childrean.Where(x => x.IsArrayElement).OrderBy(x => x.ElementArrayIndex);
                object[] array = arreyElemnts.Select(y => GetObjectFromMetadata(y, hirachciDirectory)).ToArray();
                if (_delocalizing && array.All(x => x is LocalizedString))
                {
                    CultureInfo systemCulture = System.Globalization.CultureInfo.CurrentCulture;
                    LocalizedString matchingString = null;
                    do
                    {
                        matchingString = array.OfType<LocalizedString>().FirstOrDefault(x => x.Culture.Equals(systemCulture));
                        if (systemCulture.Parent.Equals(systemCulture))
                        {
                            // We are at the Culture Root. so break or run for ever.
                            break;
                        }
                        systemCulture = systemCulture.Parent;
                    }
                    while (matchingString is null);

                    if (matchingString is object)
                    {
                        return matchingString.Value;
                    }
                }
                if (_flatten && array.Length == 1)
                {
                    return array[0];
                }

                return array;
            }
            else if (metadata.Element.Options.IsStruct)
            {
                IDictionary<string, object> obj = new System.Dynamic.ExpandoObject();
                List<TreeDirectory> properties = metadata.Childrean; // directories.XmpMeta.Properties.Where(x => x.Path is object && x.Path.StartsWith(metadata.Path))

                foreach (TreeDirectory prop in properties)
                {
                    obj.Add(prop.ElementName, GetObjectFromMetadata(prop, hirachciDirectory));
                }
                return obj;
            }
            else if (metadata.Element.Options.IsSimple)
            {
                // xml:lang, de

                if (metadata.Element.Options.HasLanguage)
                {
                    TreeDirectory langMetadata = metadata.Childrean.Single(x => x.ElementName == "lang" && x.ElementNameSpace == "http://www.w3.org/XML/1998/namespace");
                    System.Globalization.CultureInfo culture;
                    if (langMetadata.ElementValue == "x-default")
                    {
                        culture = CultureInfo.InvariantCulture;
                    }
                    else
                    {
                        culture = System.Globalization.CultureInfo.GetCultureInfo(langMetadata.ElementValue);
                    }

                    return new LocalizedString() { Culture = culture, Value = metadata.ElementValue };
                }

                return metadata.ElementValue;
            }
            else
            {
                throw new NotSupportedException($"Option {metadata.Element.Options.GetOptionsString()} not supported.");
            }
        }

        private class LocalizedString
        {
            public string Value { get; set; }
            public System.Globalization.CultureInfo Culture { get; set; }

            public override string ToString()
            {
                return Value;
            }

            public static implicit operator string(LocalizedString localizedString)
            {
                return localizedString.Value;
            }
        }

        private class XmpSearchEntry
        {
            private readonly ReadXmp _parent;

            public XmpSearchEntry(ReadXmp parent, bool isMandatory, string targetMetadata, string xmpPath)
            {
                _parent = parent;
                IsMandatory = isMandatory;
                MetadataKey = targetMetadata;
                XmpPath = xmpPath;
                string alias = Regex.Replace(XmpPath, "^(?<ns>[^:]+):(?<name>.+)$", "${ns}");
                if (!_parent._namespaceAlias.ContainsKey(alias))
                {
                    throw new ArgumentException($"Namespace alias {alias} unknown.", nameof(xmpPath));
                }
            }

            public string XmpPath { get; }

            public string PathWithoutNamespacePrefix => Regex.Replace(XmpPath, "^(?<ns>[^:]+):(?<name>.+)$", "${name}");

            public string Namespace => _parent._namespaceAlias[Regex.Replace(XmpPath, "^(?<ns>[^:]+):(?<name>.+)$", "${ns}")];

            public string MetadataKey { get; }
            public bool IsMandatory { get; }
        }
    }
}