using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Control
{
    [TestFixture]
    public class ExtractFrontMatterFixture : BaseFixture
    {
        public class ExecuteTests : ExtractFrontMatterFixture
        {
            [Test]
            public async Task DefaultCtorSplitsAtDashes()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(@"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(@"Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithDelimiterTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"
---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithoutDelimiterTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"
FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task DelimiterWithNewLineTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBeEmpty();
            }

            [Test]
            public async Task DelimiterOnLastLineTreatsAsFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBeEmpty();
            }

            [Test]
            public async Task DelimiterFollowedByJunkIsNotFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---foo")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBeNull();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---foo");
            }

            [Test]
            public async Task DelimiterPrecededByJunkIsNotFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
foo---")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBeNull();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
foo---");
            }

            [Test]
            public async Task DashStringDoesNotSplitAtNonmatchingDashes()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter("-", new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task MatchingStringSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
ABC
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter("ABC", new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithTrailingSpacesSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!!  
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithLeadingSpacesDoesNotSplit()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
  !!!!
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
  !!!!
Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithRepeatedDelimiterWithExtraLinesSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                     new TestDocument(@"FM1
FM2

!!!!

Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2

");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"
Content1
Content2");
            }

            [Test]
            public async Task SingleCharWithSingleDelimiterSplitsAtCorrectLocation()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter('!', new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task MultipleInputDocumentsResultsInMultipleOutputs()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"AA
-
XX"),
                    new TestDocument(@"BB
-
YY")
                };
                string frontMatterContent = string.Empty;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent += await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(2);
                frontMatterContent.ShouldBe(
                    @"AA
BB
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe("XX");
                (await documents.Skip(1).First().GetContentStringAsync()).ShouldBe("YY");
            }

            [Test]
            public async Task DefaultCtorIgnoresDelimiterOnFirstLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task DoesNotMatchDelimiterAtEndOfLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1---
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1---
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task MatchIsLazy()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
---
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
---
Content2");
            }

            [Test]
            public async Task NoIgnoreDelimiterOnFirstLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).IgnoreDelimiterOnFirstLine(false);

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(string.Empty);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task MatchesWhenNoIgnoreDelimiterOnFirstLine()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).IgnoreDelimiterOnFirstLine(false);

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task IgnoreSettingIgnoredWhenStartDelimiter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })))
                    .RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterNotFound()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                })))
                    .RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task EmptyFirstLineWithStartDelimiterShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"
+++
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"
+++
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task NoEndDelimiter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++
FM1
FM2
+++
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"+++
FM1
FM2
+++
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsStringShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"123
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter("123");

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedStringShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"123123
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter("123");

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"123123
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsCharShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedCharShouldMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++
FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
");
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task StartDelimiterAsRepeatedCharWithExtraCharsShouldNotMatch()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"+++---
FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(x =>
                {
                    executed = true;
                    return new[] { x };
                }))).RequireStartDelimiter('+');

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"+++---
FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task PreservesFrontMatter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(new ExecuteConfig(Config.FromDocument(async x =>
                {
                    frontMatterContent = await x.GetContentStringAsync();
                    return new[] { x };
                }))).PreserveFrontMatter();

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task ProvidedRegexDoesNotUseDefaultDelimiter()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
---
Content1
Content2")
                };
                bool executed = false;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new string[] { "\\A(?:^\\r*\\!+[^\\S\\n]*$\\r?\\n)?(.*?)(?:^\\r*\\!+[^\\S\\n]*$\\r?\\n)" },
                    new ExecuteConfig(Config.FromDocument(x =>
                    {
                        executed = true;
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                executed.ShouldBeFalse();
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"FM1
FM2
---
Content1
Content2");
            }

            [Test]
            public async Task ProvidedRegexStringMatches()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new string[] { @"\A(?:^\r*\!+[^\S\n]*$\r?\n)?(.*?)(?:^\r*\!+[^\S\n]*$\r?\n)" },
                    new ExecuteConfig(Config.FromDocument(async x =>
                    {
                        frontMatterContent = await x.GetContentStringAsync();
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task ProvidedRegexMatches()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new Regex[]
                    {
                        new Regex(
                            @"\A(?:^\r*\!+[^\S\n]*$\r?\n)?(.*?)(?:^\r*\!+[^\S\n]*$\r?\n)",
                            RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.Multiline)
                    },
                    new ExecuteConfig(Config.FromDocument(async x =>
                    {
                        frontMatterContent = await x.GetContentStringAsync();
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task RegexUsesGroupName()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new string[] { @"\A(?:^\r*\!+[^\S\n]*$\r?\n)?(.*?)(?'frontmatter'^\r*\!+[^\S\n]*$\r?\n)" },
                    new ExecuteConfig(Config.FromDocument(async x =>
                    {
                        frontMatterContent = await x.GetContentStringAsync();
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"!!!
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task MulipleRegexStringMatches()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
+++
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new string[]
                    {
                        @"\A(?:^\r*\!+[^\S\n]*$\r?\n)?(.*?)(?:^\r*\!+[^\S\n]*$\r?\n)",
                        @"\A(?:^\r*\++[^\S\n]*$\r?\n)?(.*?)(?:^\r*\++[^\S\n]*$\r?\n)"
                    },
                    new ExecuteConfig(Config.FromDocument(async x =>
                    {
                        frontMatterContent = await x.GetContentStringAsync();
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"Content1
Content2");
            }

            [Test]
            public async Task FirstRegexStringMatches()
            {
                // Given
                TestExecutionContext context = new TestExecutionContext();
                TestDocument[] inputs =
                {
                    new TestDocument(@"FM1
FM2
!!!
+++
Content1
Content2")
                };
                string frontMatterContent = null;
                ExtractFrontMatter frontMatter = new ExtractFrontMatter(
                    new string[]
                    {
                        @"\A(?:^\r*\!+[^\S\n]*$\r?\n)?(.*?)(?:^\r*\!+[^\S\n]*$\r?\n)",
                        @"\A(?:^\r*\++[^\S\n]*$\r?\n)?(.*?)(?:^\r*\++[^\S\n]*$\r?\n)"
                    },
                    new ExecuteConfig(Config.FromDocument(async x =>
                    {
                        frontMatterContent = await x.GetContentStringAsync();
                        return new[] { x };
                    })));

                // When
                IEnumerable<IDocument> documents = await ExecuteAsync(inputs, context, frontMatter);

                // Then
                documents.Count().ShouldBe(1);
                frontMatterContent.ShouldBe(
                    @"FM1
FM2
",
                    frontMatterContent);
                (await documents.First().GetContentStringAsync()).ShouldBe(
                    @"+++
Content1
Content2");
            }

            [Test]
            public void ThrowsForDocumentConfigStringRegex()
            {
                // Given, When, Then
                Should.Throw<ArgumentException>(() => new ExtractFrontMatter(
                    Config.FromDocument<IEnumerable<string>>((doc, ctx) => new string[] { "abc" })));
            }

            [Test]
            public void ThrowsForDocumentConfigRegex()
            {
                // Given, When, Then
                Should.Throw<ArgumentException>(() => new ExtractFrontMatter(
                    Config.FromDocument<IEnumerable<Regex>>((doc, ctx) => new Regex[] { new Regex("abc") })));
            }
        }
    }
}