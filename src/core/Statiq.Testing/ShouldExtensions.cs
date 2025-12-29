using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shouldly;

namespace Statiq.Common
{
    public static class ShouldExtensions
    {
        public static TDocument ShouldHaveSingleWithSource<TDocument>(this IEnumerable<TDocument> documents, NormalizedPath sourcePath, string customMessage = null)
            where TDocument : class, IDocument
        {
            documents.ShouldNotBeNull(customMessage);
            IEnumerable<TDocument> matches = documents.Where(x => x.Source.Equals(sourcePath));
            return matches.ShouldHaveSingleItem(customMessage);
        }

        public static TDocument ShouldHaveSingleWithDestination<TDocument>(this IEnumerable<TDocument> documents, NormalizedPath destinationPath, string customMessage = null)
            where TDocument : class, IDocument
        {
            documents.ShouldNotBeNull(customMessage);
            IEnumerable<TDocument> matches = documents.Where(x => x.Destination.Equals(destinationPath));
            return matches.ShouldHaveSingleItem(customMessage);
        }

        public static void ShouldNotHaveSource<TDocument>(this IEnumerable<TDocument> documents, NormalizedPath sourcePath, string customMessage = null)
            where TDocument : class, IDocument
        {
            documents.ShouldNotBeNull(customMessage);
            IEnumerable<TDocument> matches = documents.Where(x => x.Destination.Equals(sourcePath));
            matches.ShouldBeEmpty(customMessage);
        }

        public static void ShouldNotHaveDestination<TDocument>(this IEnumerable<TDocument> documents, NormalizedPath destinationPath, string customMessage = null)
            where TDocument : class, IDocument
        {
            documents.ShouldNotBeNull(customMessage);
            IEnumerable<TDocument> matches = documents.Where(x => x.Destination.Equals(destinationPath));
            matches.ShouldBeEmpty(customMessage);
        }
    }
}
