using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Statiq.Common;

namespace Statiq.Core
{
    /// <summary>
    /// Flattens a tree structure.
    /// </summary>
    /// <remarks>
    /// This module will either get all descendants of all input documents from
    /// a given metadata key (<see cref="Keys.Children"/> by default) or all
    /// descendants from all metadata if a <c>null</c> key is specified. The
    /// output also includes the initial input documents in both cases.
    /// </remarks>
    /// <remarks>
    /// The documents will be returned in no particular order and only distinct
    /// documents will be returned (I.e., if a document exists as a
    /// child of more than one parent, it will only appear once in the result set).
    /// </remarks>
    /// <metadata cref="Keys.Children" usage="Input"/>
    /// <category name="Metadata" />
    public class FlattenTree : SyncModule
    {
        private readonly string _childrenKey = Keys.Children;
        private readonly string _treePlaceholderKey;

        /// <summary>
        /// Creates a new flatten module that uses the <see cref="Keys.Children"/> key
        /// to find child documents and does not remove tree placeholder documents.
        /// </summary>
        public FlattenTree()
        {
        }

        /// <summary>
        /// Creates a new flatten module with the specified key for child documents
        /// and does not remove tree placeholder documents.
        /// Specify <c>null</c> to flatten all descendant documents regardless of key.
        /// </summary>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten all documents.</param>
        public FlattenTree(string childrenKey)
        {
            _childrenKey = childrenKey;
        }

        /// <summary>
        /// Creates a new flatten module with the specified key for child documents.
        /// Specify <c>null</c> to flatten all descendant documents regardless of key.
        /// </summary>
        /// <param name="removeTreePlaceholders"><c>true</c> to filter out documents with the <see cref="Keys.TreePlaceholder"/> metadata.</param>
        public FlattenTree(bool removeTreePlaceholders)
        {
            _treePlaceholderKey = removeTreePlaceholders ? Keys.TreePlaceholder : null;
        }

        /// <summary>
        /// Creates a new flatten module with the specified key for child documents.
        /// Specify <c>null</c> to flatten all descendant documents regardless of key.
        /// </summary>
        /// <param name="removeTreePlaceholders"><c>true</c> to filter out documents with the <see cref="Keys.TreePlaceholder"/> metadata.</param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten all documents.</param>
        public FlattenTree(bool removeTreePlaceholders, string childrenKey)
        {
            _treePlaceholderKey = removeTreePlaceholders ? Keys.TreePlaceholder : null;
            _childrenKey = childrenKey;
        }

        /// <summary>
        /// Creates a new flatten module with the specified key for child documents.
        /// Specify <c>null</c> to flatten all descendant documents regardless of key.
        /// </summary>
        /// <param name="treePlaceholderKey">
        /// The metadata key that identifies placeholder documents (<see cref="Keys.TreePlaceholder"/> by default).
        /// If <c>null</c>, tree placeholders will not be removed.
        /// </param>
        /// <param name="childrenKey">The metadata key that contains the children or <c>null</c> to flatten all documents.</param>
        public FlattenTree(string treePlaceholderKey, string childrenKey)
        {
            _treePlaceholderKey = treePlaceholderKey;
            _childrenKey = childrenKey;
        }

        protected override IEnumerable<IDocument> ExecuteContext(IExecutionContext context) =>
            context.Inputs.Flatten(_treePlaceholderKey, _childrenKey);
    }
}