using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class GroupDocumentsFixture : BaseFixture
    {
        public class ExecuteTests : GroupDocumentsFixture
        {
            [Test]
            public async Task SetsCorrectMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                GroupDocuments groupByMany = new GroupDocuments(Config.FromDocument(d => new[] { d.GetInt("A") % 3, 3 }));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.GetInt(Keys.GroupKey));
                        return d;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(count, groupByMany, gatherData);

                // Then
                Assert.That(groupKey, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
            }

            [Test]
            public async Task SetsDocumentsInMetadata()
            {
                // Given
                List<IList<string>> content = new List<IList<string>>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                GroupDocuments groupByMany = new GroupDocuments(Config.FromDocument(d => new[] { d.GetInt("A") % 3, 3 }));
                OrderDocuments orderBy = new OrderDocuments(Config.FromDocument<int>(Keys.GroupKey));
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(async d =>
                    {
                        List<string> groupContent = await d
                            .GetDocuments(Keys.Children)
                            .ToAsyncEnumerable()
                            .SelectAwait(async x => await x.GetContentStringAsync())
                            .ToListAsync();
                        content.Add(groupContent.ToList());
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(count, groupByMany, orderBy, gatherData);

                // Then
                Assert.That(content.Count, Is.EqualTo(4));
                Assert.That(content[0], Is.EquivalentTo(new[] { "3", "6" }));
                Assert.That(content[1], Is.EquivalentTo(new[] { "1", "4", "7" }));
                Assert.That(content[2], Is.EquivalentTo(new[] { "2", "5", "8" }));
                Assert.That(content[3], Is.EquivalentTo(new[] { "1", "2", "3", "4", "5", "6", "7", "8" }));
            }

            [Test]
            public async Task GroupByMetadataKey()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                SetMetadata meta = new SetMetadata("GroupMetadata", Config.FromDocument(d => new object[] { d.GetInt("A") % 3, 3 }));
                GroupDocuments groupByMany = new GroupDocuments("GroupMetadata");
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.GetInt(Keys.GroupKey));
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(count, meta, groupByMany, gatherData);

                // Then
                Assert.That(groupKey, Is.EquivalentTo(new[] { 0, 1, 2, 3 }));
            }

            [Test]
            public async Task GroupByMetadataKeyWithMissingMetadata()
            {
                // Given
                List<int> groupKey = new List<int>();
                CountModule count = new CountModule("A")
                {
                    AdditionalOutputs = 7,
                    EnsureInputDocument = true
                };
                ForEachDocument meta = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        int groupMetadata = d.GetInt("A") % 3;
                        return groupMetadata == 0 ? d : d.Clone(new MetadataItems { { "GroupMetadata", new object[] { groupMetadata, 3 } } });
                    })).ForEachDocument();
                GroupDocuments groupByMany = new GroupDocuments("GroupMetadata");
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.GetInt(Keys.GroupKey));
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(count, meta, groupByMany, gatherData);

                // Then
                Assert.That(groupKey, Is.EquivalentTo(new[] { 1, 2, 3 }));
            }

            [Test]
            public async Task DefaultComparerIsCaseSensitive()
            {
                // Given
                List<object> groupKey = new List<object>();
                ExecuteConfig meta = new ExecuteConfig(Config.FromContext(
                    c => new IDocument[]
                    {
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "A", "b" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "B" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "C" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "c" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { 1 } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "1" } } })
                    }));
                GroupDocuments groupByMany = new GroupDocuments("Tag");
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get(Keys.GroupKey));
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(meta, groupByMany, gatherData);

                // Then
                Assert.That(groupKey, Is.EquivalentTo(new object[] { "A", "B", "b", "C", "c", 1, "1" }));
            }

            [Test]
            public async Task CaseInsensitiveStringComparer()
            {
                // Given
                List<object> groupKey = new List<object>();
                ExecuteConfig meta = new ExecuteConfig(Config.FromContext(
                    c => new IDocument[]
                    {
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "A", "b" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "B" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "C" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "c" } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { 1 } } }),
                        c.CreateDocument(new MetadataItems { { "Tag", new object[] { "1" } } })
                    }));
                GroupDocuments groupByMany = new GroupDocuments("Tag").WithComparer(StringComparer.OrdinalIgnoreCase);
                ForEachDocument gatherData = new ExecuteConfig(
                    Config.FromDocument(d =>
                    {
                        groupKey.Add(d.Get(Keys.GroupKey));
                        return (object)null;
                    })).ForEachDocument();

                // When
                IReadOnlyList<IDocument> results = await ExecuteAsync(meta, groupByMany, gatherData);

                // Then
                Assert.That(groupKey, Is.EquivalentTo(new object[] { "A", "b", "C", 1 }));
            }
        }
    }
}
