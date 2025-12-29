using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Queries HTML content of the input documents and adds a metadata value that contains it's headings.
    /// </summary>
    /// <remarks>
    /// A new document is created for each heading, all of which are placed into a <c>IReadOnlyList&lt;IDocument&gt;</c>
    /// in the metadata of each input document. The new heading documents contain metadata with the level of the heading,
    /// the children of the heading (the following headings with one deeper level) and optionally the heading content, which
    /// is also set as the content of each document. The output of this module is the input documents with the additional
    /// metadata value containing the documents that present each heading.
    /// </remarks>
    /// <metadata cref="Keys.Headings" usage="Output"/>
    /// <metadata cref="Keys.Level" usage="Output"/>
    /// <metadata cref="Keys.HeadingId" usage="Output"/>
    /// <metadata cref="Keys.Children" usage="Output">
    /// The child heading documents of the current heading document.
    /// </metadata>
    /// <category name="Metadata" />
    public class GatherHeadings : ParallelConfigModule<int>
    {
        private bool _nesting;
        private bool _withNestedElements;
        private string _metadataKey = Keys.Headings;
        private string _levelKey = Keys.Level;
        private string _idKey = Keys.HeadingId;
        private string _childrenKey = Keys.Children;
        private string _headingKey;

        public GatherHeadings()
            : this(1)
        {
        }

        public GatherHeadings(Config<int> level)
            : base(level, true)
        {
        }

        /// <summary>
        /// Includes nested HTML elements in the heading content (the default is <c>false</c>).
        /// </summary>
        /// <param name="nestedElements"><c>true</c> to include nested elements, <c>false</c> otherwise.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithNestedElements(bool nestedElements = true)
        {
            _withNestedElements = true;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the level.
        /// </summary>
        /// <param name="levelKey">The key to use for the level.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithLevelKey(string levelKey)
        {
            _levelKey = levelKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the heading
        /// <c>id</c> attribute (if it has one).
        /// </summary>
        /// <param name="idKey">The key to use for the <c>id</c>.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithIdKey(string idKey)
        {
            _idKey = idKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use in the heading documents to store the children
        /// of a given heading. In other words, the metadata for this key will
        /// contain all the headings following the one in the document with a
        /// level one deeper than the current heading.
        /// </summary>
        /// <param name="childrenKey">The key to use for children.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithChildrenKey(string childrenKey)
        {
            _childrenKey = childrenKey;
            return this;
        }

        /// <summary>
        /// Sets the key to use for storing the heading content in the heading documents.
        /// The default is <c>null</c> which means only store the heading content in the
        /// content of the heading document. Setting this can be useful when you want
        /// to use the heading documents in downstream modules, setting their content
        /// to something else while maintaining the heading content in metadata.
        /// </summary>
        /// <param name="headingKey">The key to use for the heading content.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithHeadingKey(string headingKey)
        {
            _headingKey = headingKey;
            return this;
        }

        /// <summary>
        /// Controls whether the heading documents are nested. If nesting is
        /// used, only the level 1 headings will be in the root set of documents.
        /// The rest of the heading documents will only be accessible via the
        /// metadata of the root heading documents.
        /// </summary>
        /// <param name="nesting"><c>true</c> to turn on nesting.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithNesting(bool nesting = true)
        {
            _nesting = true;
            return this;
        }

        /// <summary>
        /// Allows you to specify an alternate metadata key for the heading documents.
        /// </summary>
        /// <param name="metadataKey">The metadata key to store the heading documents in.</param>
        /// <returns>The current module instance.</returns>
        public GatherHeadings WithMetadataKey(string metadataKey)
        {
            _metadataKey = metadataKey;
            return this;
        }

        protected override async Task<IEnumerable<Common.IDocument>> ExecuteConfigAsync(Common.IDocument input, IExecutionContext context, int value)
        {
            // Return the original document if no metadata key
            if (string.IsNullOrWhiteSpace(_metadataKey))
            {
                return input.Yield();
            }

            // Parse the HTML content
            IHtmlDocument htmlDocument = await input.ParseHtmlAsync(false);
            if (htmlDocument is null)
            {
                return input.Yield();
            }

            // Validate the level
            if (value < 1)
            {
                throw new ArgumentException("Heading level cannot be less than 1");
            }
            if (value > 6)
            {
                throw new ArgumentException("Heading level cannot be greater than 6");
            }

            // Evaluate the query and create the holding nodes
            Heading previousHeading = null;
            List<Heading> headings = htmlDocument
                .QuerySelectorAll(GetHeadingQuery(value))
                .Select(x =>
                {
                    previousHeading = new Heading
                    {
                        Element = x,
                        Previous = previousHeading,
                        Level = int.Parse(x.NodeName.Substring(1))
                    };
                    return previousHeading;
                })
                .ToList();

            // Build the tree from the bottom-up
            for (int level = value; level >= 1; level--)
            {
                int currentLevel = level;
                foreach (Heading heading in headings.Where(x => x.Level == currentLevel))
                {
                    // Get the parent
                    Heading parent = null;
                    if (currentLevel > 1)
                    {
                        parent = heading.Previous;
                        while (parent is object && parent.Level >= currentLevel)
                        {
                            parent = parent.Previous;
                        }
                    }

                    // Create the document
                    MetadataItems metadata = new MetadataItems();
                    string content = _withNestedElements
                        ? heading.Element.TextContent
                        : string.Join(
                            string.Empty,
                            heading.Element.ChildNodes
                                .Select(x =>
                                {
                                    if (x is IText text)
                                    {
                                        return text.Text;
                                    }
                                    if (x is IHtmlAnchorElement anchor)
                                    {
                                        return string.Join(
                                            string.Empty,
                                            anchor.ChildNodes.OfType<IText>().Select(t => t.Text));
                                    }
                                    return null;
                                })
                                .Where(x => !x.IsNullOrEmpty()))
                            .Trim();
                    if (_levelKey is object)
                    {
                        metadata.Add(_levelKey, heading.Level);
                    }
                    if (_idKey is object && heading.Element.HasAttribute("id"))
                    {
                        metadata.Add(_idKey, heading.Element.GetAttribute("id"));
                    }
                    if (_headingKey is object)
                    {
                        metadata.Add(_headingKey, content);
                    }
                    if (_childrenKey is object)
                    {
                        metadata.Add(_childrenKey, heading.Children.AsReadOnly());
                    }

                    heading.Document = context.CreateDocument(metadata, content);

                    // Add to parent
                    parent?.Children.Add(heading.Document);
                }
            }

            return input
                .Clone(new MetadataItems
                {
                    {
                        _metadataKey,
                        _nesting
                            ? headings
                                .Where(x => x.Level == headings.Min(y => y.Level))
                                .Select(x => x.Document)
                                .ToArray()
                            : headings
                                .Select(x => x.Document)
                                .ToArray()
                    }
                })
                .Yield();
        }

        public static string GetHeadingQuery(int level)
        {
            StringBuilder query = new StringBuilder();
            for (int l = 1; l <= level; l++)
            {
                if (l > 1)
                {
                    query.Append(",");
                }
                query.Append("h");
                query.Append(l);
            }
            return query.ToString();
        }

        private class Heading
        {
            public IElement Element { get; set; }
            public Heading Previous { get; set; }
            public int Level { get; set; }
            public Common.IDocument Document { get; set; }
            public List<Common.IDocument> Children { get; } = new List<Common.IDocument>();
        }
    }
}