using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Contents
{
    [TestFixture]
    public class ProcessShortcodesFixture : BaseFixture
    {
        public class ExecuteTests : ProcessShortcodesFixture
        {
            [Test]
            public async Task ProcessesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123Foo456");
            }

            [Test]
            public async Task ProcessesNestedShortcodeInResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("Nested");
                context.Shortcodes.Add<NestedShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABCFooXYZ456");
            }

            [Test]
            public async Task ProcessesNestedShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Foo");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Foo ?>ABC<?# Bar /?>XYZ<?#/ Foo ?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABCFooXYZ456");
            }

            [Test]
            public async Task DoesNotProcessNestedRawShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Raw");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Raw ?>ABC<?# Bar /?>XYZ<?#/ Raw ?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123ABC<?# Bar /?>XYZ456");
            }

            [Test]
            public async Task DoesNotProcessDirectlyNestedRawShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<RawShortcode>("Raw");
                context.Shortcodes.Add<TestShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Raw ?><?# Bar /?><?#/ Raw ?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123<?# Bar /?>456");
            }

            [Test]
            public async Task ShortcodeSupportsNullResult()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<TestShortcode>("S1");
                context.Shortcodes.Add<NullResultShortcode>("S2");
                TestDocument document = new TestDocument("123<?# S1 /?>456<?# S2 /?>789<?# S1 /?>");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123Foo456789Foo");
            }

            [Test]
            public async Task DisposesShortcode()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<DisposableShortcode>("Bar");
                TestDocument document = new TestDocument("123<?# Bar /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                DisposableShortcode.Disposed.ShouldBeTrue();
            }

            [Test]
            public async Task ShortcodesCanReadMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<ReadsMetadataShortcode>("S1");
                TestDocument document = new TestDocument("123<?# S1 /?>456")
                {
                    { "Foo", "abc" }
                };
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123abc456");
            }

            [Test]
            public async Task ShortcodesCanSetNestedMetadata()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<AddsNestedMetadataShortcode>("S1");
                context.Shortcodes.Add<ReadsMetadataShortcode>("S2");
                TestDocument document = new TestDocument("12<?# S2 /?>34<?# S1 ?>56<?# S2 /?>78<?#/ S1 ?>90<?# S2 /?>12")
                {
                    { "Foo", "abc" }
                };
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("12abc3456xyz7890abc12");
            }

            [Test]
            public async Task ShortcodesPersistState()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<IncrementingShortcode>("S");
                TestDocument document = new TestDocument("ab<?# S /?>cd<?# S /?>ef<?# S /?>gh");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("ab20cd21ef22gh");
            }

            [Test]
            public async Task MultipleShortcodeResultDocuments()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                context.Shortcodes.Add<MultiShortcode>("S");
                TestDocument document = new TestDocument("123<?# S /?>456");
                ProcessShortcodes module = new ProcessShortcodes();

                // When
                TestDocument result = await ExecuteAsync(document, context, module).SingleAsync();

                // Then
                result.Content.ShouldBe("123aaaBBB456");
            }
        }

        public class TestShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { "Foo" });
        }

        public class NestedShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { "ABC<?# Nested /?>XYZ" });
        }

        public class RawShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { content });
        }

        public class NullResultShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(null);
        }

        public class DisposableShortcode : IShortcode, IDisposable
        {
            public static bool Disposed { get; set; }

            public DisposableShortcode()
            {
                // Make sure it resets
                Disposed = false;
            }

            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { "Foo" });

            public void Dispose() =>
                Disposed = true;
        }

        public class AddsNestedMetadataShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new[]
                {
                    new ShortcodeResult(
                        content,
                        new MetadataItems
                        {
                            { "Foo", "xyz" }
                        })
                });
        }

        public class ReadsMetadataShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { document.GetString("Foo") });
        }

        public class IncrementingShortcode : IShortcode
        {
            private int _value = 20;

            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { _value++.ToString() });
        }

        public class MultiShortcode : IShortcode
        {
            public Task<IEnumerable<ShortcodeResult>> ExecuteAsync(KeyValuePair<string, string>[] args, string content, IDocument document, IExecutionContext context) =>
                Task.FromResult<IEnumerable<ShortcodeResult>>(new ShortcodeResult[] { "aaa", "BBB" });
        }
    }
}
