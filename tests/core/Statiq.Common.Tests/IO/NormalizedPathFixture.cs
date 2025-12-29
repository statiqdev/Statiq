using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class NormalizedPathFixture : BaseFixture
    {
        public class ConstructorTests : NormalizedPathFixture
        {
            [TestCase("hello/temp/test/../world", "hello/temp/world")]
            [TestCase("../hello/temp/test/../world", "../hello/temp/world")]
            [TestCase("../hello/world", "../hello/world")]
            [TestCase("hello/temp/test/../../world", "hello/world")]
            [TestCase("hello/temp/../temp2/../world", "hello/world")]
            [TestCase("/hello/temp/test/../../world", "/hello/world")]
            [TestCase("/hello/../../../../../../temp", "/../../../../../temp")]
            [TestCase("/hello/../../foo/../../../../temp", "/../../../../temp")]
            [TestCase(".", ".")]
            [TestCase("..", "..")]
            [TestCase("/..", "/..")]
            [TestCase("/", "/")]
            [TestCase("/.", "/")]
            [TestCase("/", "/")]
            [TestCase("./.././foo", "../foo")]
            [TestCase("../.././foo", "../../foo")]
            [TestCase("./a", "a")]
            [TestCase("./..", "..")]
            [TestCase("a/./b", "a/b")]
            [TestCase("/a/./b", "/a/b")]
            [TestCase("//a/./b", "/a/b")]
            [TestCase("a/.././b", "b")]
            [TestCase("/a/.././b", "/b")]
            [TestCase("a/b/..", "a")]
            [TestCase("/a/b/..", "/a")]
            [TestCase("a/b/.", "a/b")]
            [TestCase("/a/b/.", "/a/b")]
            [TestCase("/./a/b", "/a/b")]
            [TestCase("/././a/b", "/a/b")]
            [TestCase("/a/b/c/../d/baz.txt", "/a/b/d/baz.txt")]
            [TestCase("../d/baz.txt", "../d/baz.txt")]
            [TestCase("../a/b/c/../d/baz.txt", "../a/b/d/baz.txt")]
            [TestCase("/a/b/c/../d", "/a/b/d")]
            [TestCase("foo", "foo")]
            [WindowsTestCase("c:/hello/temp/test/../../world", "c:/hello/world")]
            [WindowsTestCase("c:/../../../../../../temp", "c:/../../../../../../temp")]
            [WindowsTestCase("c:/../../foo/../../../../temp", "c:/../../../../../temp")]
            [WindowsTestCase("c:/a/b/c/../d/baz.txt", "c:/a/b/d/baz.txt")]
            [WindowsTestCase("c:/a/b/c/../d", "c:/a/b/d")]
            public void ShouldCollapsePath(string fullPath, string expectedFullPath)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(fullPath);

                // Then
                path.FullPath.ShouldBe(expectedFullPath);
                path.Segments.ToStrings().ShouldBe(expectedFullPath.Split('/', StringSplitOptions.RemoveEmptyEntries));
            }

            [Test]
            public void DefaultIsNull()
            {
                // Given, When
                NormalizedPath path = default;

                // Then
                path.IsNull.ShouldBeTrue();
            }

            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given, When, Then
                Should.Throw<ArgumentNullException>(() => new NormalizedPath(null));
            }

            [Test]
            public void EmptyPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(string.Empty);

                // Then
                path.FullPath.ShouldBe(string.Empty);
                path.Segments.ShouldBeEmpty();
                path.IsAbsolute.ShouldBeFalse();
                path.IsNullOrEmpty.ShouldBeTrue();
            }

            [Test]
            public void WhitespacePath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(" ");

                // Then
                path.FullPath.ShouldBe(" ");
                path.IsNullOrEmpty.ShouldBeFalse();
                path.Segments.Length.ShouldBe(1);
                path.Segments[0].ToString().ShouldBe(" ");
                path.IsAbsolute.ShouldBeFalse();
            }

            [Test]
            public void BackslashPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("/");

                // Then
                path.FullPath.ShouldBe("/");
                path.Segments.ShouldBeEmpty();
                path.IsAbsolute.ShouldBeTrue();
            }

            [Test]
            public void ForwardSlashPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("\\");

                // Then
                path.FullPath.ShouldBe("/");
                path.Segments.ShouldBeEmpty();
                path.IsAbsolute.ShouldBeTrue();
            }

            [Test]
            public void TrimsWhitespace()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("\t ");

                // Then
                path.FullPath.ShouldBe(" ");
                path.IsNullOrEmpty.ShouldBeFalse();
            }

            [Test]
            public void CurrentDirectoryReturnsDot()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("./");

                // Then
                path.FullPath.ShouldBe(".");
            }

            [Test]
            public void ShouldNormalizePathSeparators()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("shaders\\basic");

                // Then
                path.FullPath.ShouldBe("shaders/basic");
            }

            [Test]
            public void ShouldTrimWhiteSpaceFromPathAndLeaveSpaces()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("\t\r\nshaders/basic ");

                // Then
                path.FullPath.ShouldBe("shaders/basic ");
            }

            [Test]
            public void ShouldNotRemoveWhiteSpaceWithinPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("my awesome shaders/basic");

                // Then
                path.FullPath.ShouldBe("my awesome shaders/basic");
            }

            [TestCase("/Hello/World/", "/Hello/World")]
            [TestCase("\\Hello\\World\\", "/Hello/World")]
            [TestCase("file.txt/", "file.txt")]
            [TestCase("file.txt\\", "file.txt")]
            [TestCase("Temp/file.txt/", "Temp/file.txt")]
            [TestCase("Temp\\file.txt\\", "Temp/file.txt")]
            public void ShouldRemoveTrailingSlashes(string value, string expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveSingleTrailingSlash(string value)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe("/");
            }

            [TestCase("./Hello/World/", "Hello/World")]
            [TestCase(".\\Hello/World/", "Hello/World")]
            [TestCase("./file.txt", "file.txt")]
            [TestCase("./Temp/file.txt", "Temp/file.txt")]
            public void ShouldRemoveRelativePrefix(string value, string expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [TestCase("\\")]
            [TestCase("/")]
            public void ShouldNotRemoveOnlyRelativePart(string value)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(value);

                // Then
                path.FullPath.ShouldBe("/");
            }
        }

        public class FullPathTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldReturnFullPath()
            {
                // Given, When
                const string expected = "shaders/basic";
                NormalizedPath path = new NormalizedPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForInferedAbsolutePath()
            {
                // Given, When
                const string expected = "/shaders/basic";
                NormalizedPath path = new NormalizedPath(expected);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ShouldReturnFullPathForExplicitAbsolutePath()
            {
                // Given, When
                const string expected = "shaders/basic";
                NormalizedPath path = new NormalizedPath(expected, PathKind.Absolute);

                // Then
                path.FullPath.ShouldBe(expected);
            }

            [WindowsTest]
            public void ShouldNotPrependSlashForRootedPath()
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("C:/shaders/basic");

                // Then
                path.FullPath.ShouldBe("C:\\shaders/basic");
            }
        }

        public class RootTests : NormalizedPathFixture
        {
            [TestCase(@"\a\b\c", "/")]
            [TestCase("/a/b/c", "/")]
            [TestCase("a/b/c", "")]
            [TestCase(@"a\b\c", "")]
            [TestCase("foo.txt", "")]
            [TestCase("foo", "")]
            [WindowsTestCase(@"c:\\a\\b\\c", "c:/")]
            [WindowsTestCase("c:/a/b/c", "c:/")]
            public void ShouldReturnRootPath(string fullPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath root = path.Root;

                // Then
                root.FullPath.ShouldBe(expected);
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
            [WindowsTestCase(@"c:\\a\\b\\c")]
            [WindowsTestCase("c:/a/b/c")]
            public void ShouldReturnEmptyRootForExplicitRelativePath(string fullPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath, PathKind.Relative);

                // When
                NormalizedPath root = path.Root;

                // Then
                root.FullPath.ShouldBeEmpty();
            }
        }

        public class IsRelativeTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders", true)]
            [TestCase("assets/shaders/basic.frag", true)]
            [TestCase("/assets/shaders", false)]
            [TestCase("/assets/shaders/basic.frag", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelative(string fullPath, bool expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(fullPath);

                // Then
                path.IsRelative.ShouldBe(expected);
            }

            [WindowsTestCase("c:/assets/shaders", false)]
            [WindowsTestCase("c:/assets/shaders/basic.frag", false)]
            [WindowsTestCase("c:/", false)]
            [WindowsTestCase("c:", false)]
            public void ShouldReturnWhetherOrNotAPathIsRelativeOnWindows(string fullPath, bool expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(fullPath);

                // Then
                path.IsRelative.ShouldBe(expected);
            }
        }

        public class ToStringTests : NormalizedPathFixture
        {
            [TestCase("temp/hello", "temp/hello")]
            [TestCase("/temp/hello", "/temp/hello")]
            [WindowsTestCase("c:/temp/hello", "c:/temp/hello")]
            public void ShouldReturnStringRepresentation(string path, string expected)
            {
                // Given, When
                NormalizedPath testPath = new NormalizedPath(path);

                // Then
                testPath.ToString().ShouldBe(expected);
            }
        }

        public class SegmentsTests : NormalizedPathFixture
        {
            [TestCase("Hello/World")]
            [TestCase("/Hello/World")]
            [TestCase("/Hello/World/")]
            [TestCase("./Hello/World/")]
            public void ShouldReturnSegmentsOfPath(string pathName)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(pathName);

                // Then
                path.Segments.Length.ShouldBe(2);
                path.Segments[0].ToString().ShouldBe("Hello");
                path.Segments[1].ToString().ShouldBe("World");
            }
        }

        public class EqualsTests : NormalizedPathFixture
        {
            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SameAssetInstanceIsEqual(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath("shaders/basic.vert");

                // Then
                path.Equals(path, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void PathsAreInequalIfAnyIsNull(StringComparison comparisonType)
            {
                // Given, When
                bool result = new NormalizedPath("test.txt").Equals(null, comparisonType);

                // Then
                result.ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SamePathsAreEqual(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                first.Equals(second, comparisonType).ShouldBeTrue();
                second.Equals(first, comparisonType).ShouldBeTrue();
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsAreNotEqual(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                first.Equals(second, comparisonType).ShouldBeFalse();
                second.Equals(first, comparisonType).ShouldBeFalse();
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("SHADERS/BASIC.VERT");

                // When
                bool firstResult = first.Equals(second, comparisonType);
                bool secondResult = second.Equals(first, comparisonType);

                // Then
                firstResult.ShouldBe(expected);
                secondResult.ShouldBe(expected);
            }
        }

        public class GetHashCodeTests : NormalizedPathFixture
        {
            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void SamePathsGetSameHashCode(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                first.GetHashCode(comparisonType).ShouldBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal)]
            [TestCase(StringComparison.OrdinalIgnoreCase)]
            public void DifferentPathsGetDifferentHashCodes(StringComparison comparisonType)
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                first.GetHashCode(comparisonType).ShouldNotBe(second.GetHashCode(comparisonType));
            }

            [TestCase(StringComparison.Ordinal, false)]
            [TestCase(StringComparison.OrdinalIgnoreCase, true)]
            public void SamePathsButDifferentCasingFollowComparison(StringComparison comparisonType, bool expected)
            {
                // Given
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("SHADERS/BASIC.VERT");

                // When
                bool result = first.GetHashCode(comparisonType).Equals(second.GetHashCode(comparisonType));

                // Then
                result.ShouldBe(expected);
            }
        }

        public class EqualityOperatorTests : NormalizedPathFixture
        {
            [Test]
            public void PathsAreInequalIfAnyIsNull()
            {
                // Given, When
                NormalizedPath result = new NormalizedPath("test.txt");

                // Then
                result.IsNull.ShouldBeFalse();
            }

            [Test]
            public void SamePathsAreEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.vert");

                // Then
                (first == second).ShouldBeTrue();
                (second == first).ShouldBeTrue();
            }

            [Test]
            public void DifferentPathsAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                NormalizedPath second = new NormalizedPath("shaders/basic.frag");

                // Then
                (first == second).ShouldBeFalse();
                (second == first).ShouldBeFalse();
            }

            [Test]
            public void StringPathsAreEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                const string second = "shaders/basic.vert";

                // Then
                (first == second).ShouldBeTrue();
            }

            [Test]
            public void DifferentStringPathIsNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                const string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void AbsoluteStringAndPathAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("shaders/basic.vert");
                const string second = "/shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void StringAndAbsolutePathAreNotEqual()
            {
                // Given, When
                NormalizedPath first = new NormalizedPath("/shaders/basic.vert");
                const string second = "shaders/basic.frag";

                // Then
                (first == second).ShouldBeFalse();
            }

            [Test]
            public void BothNullAreEqual()
            {
                // Given, When
                NormalizedPath first = null;

                // Then
                first.IsNull.ShouldBeTrue();
            }
        }
        public class HasExtensionTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders/basic.txt", true)]
            [TestCase("assets/shaders/basic", false)]
            [TestCase("assets/shaders/basic/", false)]
            public void CanSeeIfAPathHasAnExtension(string fullPath, bool expected)
            {
                // Given, When
                NormalizedPath path = new NormalizedPath(fullPath);

                // Then
                Assert.That(path.HasExtension, Is.EqualTo(expected));
            }
        }

        public class ExtensionTests : NormalizedPathFixture
        {
            [TestCase("assets/shaders/basic.frag", ".frag")]
            [TestCase("assets/shaders/basic.frag/test.vert", ".vert")]
            [TestCase("assets/shaders/basic", "")]
            [TestCase("assets/shaders/basic.frag/test", "")]
            public void CanGetExtension(string fullPath, string expected)
            {
                // Given
                NormalizedPath result = new NormalizedPath(fullPath);

                // When
                string extension = result.Extension;

                // Then
                Assert.That(extension, Is.EqualTo(expected));
            }
        }

        public class DirectoryTests : NormalizedPathFixture
        {
            [Test]
            public void CanGetDirectoryForFilePath()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                NormalizedPath directory = path.Parent;

                // Then
                Assert.That(directory.FullPath, Is.EqualTo("temp"));
            }

            [Test]
            public void CanGetDirectoryForFilePathInRoot()
            {
                // Given
                NormalizedPath path = new NormalizedPath("hello.txt");

                // When
                NormalizedPath directory = path.Parent;

                // Then
                directory.FullPath.ShouldBeEmpty();
            }
        }

        public class RootRelativeTests : NormalizedPathFixture
        {
            [TestCase(@"\a\b\c", "a/b/c")]
            [TestCase("/a/b/c", "a/b/c")]
            [TestCase("a/b/c", "a/b/c")]
            [TestCase(@"a\b\c", "a/b/c")]
            [TestCase("foo.txt", "foo.txt")]
            [TestCase("foo", "foo")]
            [WindowsTestCase(@"c:\\a\\b\\c", "a/b/c")]
            [WindowsTestCase("c:/a/b/c", "a/b/c")]
            public void ShouldReturnRootRelativePath(string fullPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath rootRelative = path.RootRelative;

                // Then
                Assert.That(rootRelative.FullPath, Is.EqualTo(expected));
            }

            [TestCase(@"\a\b\c")]
            [TestCase("/a/b/c")]
            [TestCase("a/b/c")]
            [TestCase(@"a\b\c")]
            [TestCase("foo.txt")]
            [TestCase("foo")]
            [WindowsTestCase(@"c:\a\b\c")]
            [WindowsTestCase("c:/a/b/c")]
            public void ShouldReturnSelfForExplicitRelativePath(string fullPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath, PathKind.Relative);

                // When
                NormalizedPath rootRelative = path.RootRelative;

                // Then
                Assert.That(rootRelative.FullPath, Is.EqualTo(path.FullPath));
            }
        }

        public class ChangeExtensionTests : NormalizedPathFixture
        {
            [TestCase("temp/hello.txt", ".dat", "temp/hello.dat")]
            [TestCase("temp/hello.txt", "dat", "temp/hello.dat")]
            [TestCase("temp/hello.txt", ".txt", "temp/hello.txt")]
            [TestCase("temp/hello.txt", "txt", "temp/hello.txt")]
            [TestCase("temp/hello.txt", "", "temp/hello.")]
            [TestCase("temp/hello.txt", null, "temp/hello")]
            [TestCase("temp/hello.txt", ".", "temp/hello.")]
            [TestCase("temp/hello.txt.foo", ".dat", "temp/hello.txt.dat")]
            [TestCase("temp/hello.txt.foo", "dat", "temp/hello.txt.dat")]
            [TestCase("temp/hello.txt.foo", ".txt", "temp/hello.txt.txt")]
            [TestCase("temp/hello.txt.foo", "txt", "temp/hello.txt.txt")]
            [TestCase("temp/hello.txt.foo", "", "temp/hello.txt.")]
            [TestCase("temp/hello.txt.foo", null, "temp/hello.txt")]
            [TestCase("temp/hello.txt.foo", ".", "temp/hello.txt.")]
            public void ShouldChangeExtension(string path, string extension, string expected)
            {
                // Given
                NormalizedPath normalized = new NormalizedPath(path);

                // When
                normalized = normalized.ChangeExtension(extension);

                // Then
                Assert.That(normalized.ToString(), Is.EqualTo(expected));
            }

            [TestCase("foo")]
            [TestCase(".foo")]
            public void AddsExtensionToEmptyPath(string extension)
            {
                // Given
                NormalizedPath path = NormalizedPath.Empty;

                // When
                path = path.ChangeExtension(extension);

                // Then
                path.FullPath.ShouldBe(".foo");
            }
        }

        public class AppendExtensionTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfExtensionIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.AppendExtension(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }

            [TestCase("dat", "temp/hello.txt.dat")]
            [TestCase(".dat", "temp/hello.txt.dat")]
            public void CanAppendExtensionToPath(string extension, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                path = path.AppendExtension(extension);

                // Then
                Assert.That(path.ToString(), Is.EqualTo(expected));
            }
        }

        public class InsertSuffixTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfSuffixIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.InsertSuffix(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }

            [TestCase("temp/hello.txt", "123", "temp/hello123.txt")]
            [TestCase("/hello.txt", "123", "/hello123.txt")]
            [TestCase("temp/hello", "123", "temp/hello123")]
            [TestCase("temp/hello.txt.dat", "123", "temp/hello.txt123.dat")]
            public void CanInsertSuffixToPath(string path, string suffix, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                filePath = filePath.InsertSuffix(suffix);

                // Then
                Assert.That(filePath.FullPath, Is.EqualTo(expected));
            }
        }

        public class InserPrefixTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPRefixIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("temp/hello.txt");

                // When
                TestDelegate test = () => path.InsertPrefix(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }

            [TestCase("temp/hello.txt", "123", "temp/123hello.txt")]
            [TestCase("/hello.txt", "123", "/123hello.txt")]
            [TestCase("hello.txt", "123", "123hello.txt")]
            [TestCase("temp/hello", "123", "temp/123hello")]
            [TestCase("temp/hello.txt.dat", "123", "temp/123hello.txt.dat")]
            public void CanInsertPrefixToPath(string path, string prefix, string expected)
            {
                // Given
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                filePath = filePath.InsertPrefix(prefix);

                // Then
                Assert.That(filePath.FullPath, Is.EqualTo(expected));
            }
        }

        public class FileNameTests : NormalizedPathFixture
        {
            [TestCase("/input/test.txt", "test.txt")]
            [TestCase("input/test.txt", "test.txt")]
            [TestCase("test.txt", "test.txt")]
            [TestCase("/test.txt", "test.txt")]
            [TestCase("/input/test", "test")]
            [TestCase("input/test", "test")]
            [TestCase("test", "test")]
            [TestCase("/test", "test")]
            [TestCase("/input/test.txt.foo", "test.txt.foo")]
            [TestCase("input/test.txt.foo", "test.txt.foo")]
            [TestCase("test.txt.foo", "test.txt.foo")]
            [TestCase("/test.txt.foo", "test.txt.foo")]
            [TestCase("", "")]
            [TestCase("/", "")]
            [TestCase("test.", "test.")]
            [TestCase("/input/test.", "test.")]
            [TestCase("input/test.", "test.")]
            [TestCase("/test.", "test.")]
            public void GetsFileName(string fullPath, string expected)
            {
                // Given
                NormalizedPath normalizedPath = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = normalizedPath.FileName;

                // Then
                result.FullPath.ShouldBe(expected);
            }

            [Test]
            public void ResultIsRelative()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/foo/bar.txt");

                // When
                NormalizedPath result = path.FileName;

                // Then
                path.IsAbsolute.ShouldBeTrue();
                result.IsAbsolute.ShouldBeFalse();
            }
        }

        public class FileNameWithoutExtensionTests : NormalizedPathFixture
        {
            [TestCase("/input/test.txt", "test")]
            [TestCase("input/test.txt", "test")]
            [TestCase("test.txt", "test")]
            [TestCase("/test.txt", "test")]
            [TestCase("/input/test", "test")]
            [TestCase("input/test", "test")]
            [TestCase("test", "test")]
            [TestCase("/test", "test")]
            [TestCase("/input/test.txt.foo", "test.txt")]
            [TestCase("input/test.txt.foo", "test.txt")]
            [TestCase("test.txt.foo", "test.txt")]
            [TestCase("/test.txt.foo", "test.txt")]
            [TestCase("", "")]
            [TestCase("/", "")]
            [TestCase("test.", "test")]
            [TestCase("/input/test.", "test")]
            [TestCase("input/test.", "test")]
            [TestCase("/test.", "test")]
            public void ShouldReturnFilenameWithoutExtensionFromPath(string fullPath, string expected)
            {
                // Given
                NormalizedPath normalizedPath = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = normalizedPath.FileNameWithoutExtension;

                // Then
                result.FullPath.ShouldBe(expected);
            }

            [TestCase("/input/.test")]
            [TestCase(".test")]
            public void ShouldReturnNullIfOnlyExtension(string fullPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = path.FileNameWithoutExtension;

                // Then
                result.FullPath.ShouldBeEmpty();
            }

            [Test]
            public void ResultIsRelative()
            {
                // Given
                NormalizedPath path = new NormalizedPath("/foo/bar.txt");

                // When
                NormalizedPath result = path.FileNameWithoutExtension;

                // Then
                path.IsAbsolute.ShouldBeTrue();
                result.IsAbsolute.ShouldBeFalse();
            }
        }

        public class ChangeFileNameTests : NormalizedPathFixture
        {
            [TestCase("/input/test.txt", "foo.bar", "/input/foo.bar")]
            [TestCase("/input/test", "foo.bar", "/input/foo.bar")]
            [TestCase("input/test.txt", "foo.bar", "input/foo.bar")]
            [TestCase("input/test", "foo.bar", "input/foo.bar")]
            [TestCase("/test.txt", "foo.bar", "/foo.bar")]
            [TestCase("/test", "foo.bar", "/foo.bar")]
            [TestCase("test.txt", "foo.bar", "foo.bar")]
            [TestCase("test", "foo.bar", "foo.bar")]
            [TestCase("/input/test.txt", "foo", "/input/foo")]
            [TestCase("/input/test", "foo", "/input/foo")]
            [TestCase("input/test.txt", "foo", "input/foo")]
            [TestCase("input/test", "foo", "input/foo")]
            [TestCase("/test.txt", "foo", "/foo")]
            [TestCase("/test", "foo", "/foo")]
            [TestCase("test.txt", "foo", "foo")]
            [TestCase("test", "foo", "foo")]
            public void ShouldChangeFileName(string fullPath, string fileName, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(fullPath);

                // When
                NormalizedPath result = path.ChangeFileName(fileName);

                // Then
                result.FullPath.ShouldBe(expected);
            }
        }

        public class NameTests : NormalizedPathFixture
        {
            [TestCase("/a/b", "b")]
            [TestCase("/a/b/", "b")]
            [TestCase("/a/b/../c", "c")]
            [TestCase("/a/b/..", "a")]
            [TestCase("/a", "a")]
            [TestCase("/", "/")]
            [WindowsTestCase("C:/", "C:")]
            [WindowsTestCase("C:", "C:")]
            [WindowsTestCase("C:/Data", "Data")]
            [WindowsTestCase("C:/Data/Work", "Work")]
            [WindowsTestCase("C:/Data/Work/file.txt", "file.txt")]
            public void ShouldReturnDirectoryName(string directoryPath, string name)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                string result = path.Name;

                // Then
                Assert.That(result, Is.EqualTo(name));
            }
        }

        public class ParentTests : NormalizedPathFixture
        {
            [TestCase("/a/b", "/a")]
            [TestCase("/a/b/", "/a")]
            [TestCase("/a/b/../c", "/a")]
            [TestCase("/a", "/")]
            [WindowsTestCase("C:/a/b", "C:/a")]
            [WindowsTestCase("C:/a", "C:/")]
            public void ReturnsParent(string directoryPath, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                NormalizedPath parent = path.Parent;

                // Then
                Assert.That(parent.FullPath, Is.EqualTo(expected));
            }

            [TestCase(".")]
            [TestCase("a")]
            [TestCase("")]
            public void RelativeRootDirectoryReturnsEmptyParent(string directoryPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                NormalizedPath parent = path.Parent;

                // Then
                parent.FullPath.ShouldBeEmpty();
            }

            [TestCase("/")]
            [WindowsTestCase("C:")]
            public void AbsoluteRootDirectoryReturnsNull(string directoryPath)
            {
                // Given
                NormalizedPath path = new NormalizedPath(directoryPath);

                // When
                NormalizedPath parent = path.Parent;

                // Then
                parent.FullPath.ShouldBeNull();
            }
        }

        public class GetFilePathTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.GetFilePath(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }

            [WindowsTestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/", "c:/simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/", "c:/test/simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "test/simple.frag", "c:/simple.frag")]
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "/test/simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "/test/simple.frag", "/assets/shaders/simple.frag")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(first);

                // When
                NormalizedPath result = path.GetFilePath(new NormalizedPath(second));

                // Then
                Assert.That(result.FullPath, Is.EqualTo(expected));
            }
        }

        public class CombineFileTests : NormalizedPathFixture
        {
            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }

            [WindowsTestCase("c:/assets/shaders/", "simple.frag", "c:/assets/shaders/simple.frag")]
            [WindowsTestCase("c:/", "simple.frag", "c:/simple.frag")]
            [WindowsTestCase("c:/assets/shaders/", "test/simple.frag", "c:/assets/shaders/test/simple.frag")]
            [WindowsTestCase("c:/", "test/simple.frag", "c:/test/simple.frag")]
            [WindowsTestCase("c:/", "c:/test/simple.frag", "c:/test/simple.frag")]
            [TestCase("assets/shaders", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("assets/shaders/", "simple.frag", "assets/shaders/simple.frag")]
            [TestCase("/assets/shaders/", "simple.frag", "/assets/shaders/simple.frag")]
            [TestCase("assets/shaders", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("assets/shaders/", "test/simple.frag", "assets/shaders/test/simple.frag")]
            [TestCase("/assets/shaders/", "test/simple.frag", "/assets/shaders/test/simple.frag")]
            [TestCase("assets", "/other/asset.txt", "/other/asset.txt")]
            [TestCase(".", "asset.txt", "asset.txt")]
            [TestCase(".", "other/asset.txt", "other/asset.txt")]
            [TestCase(".", "/other/asset.txt", "/other/asset.txt")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath path = new NormalizedPath(first);

                // When
                NormalizedPath result = path.Combine(new NormalizedPath(second));

                // Then
                Assert.That(result.FullPath, Is.EqualTo(expected));
            }
        }

        public class CombineTests : NormalizedPathFixture
        {
            [WindowsTestCase("c:/assets/shaders/", "simple", "c:/assets/shaders/simple")]
            [WindowsTestCase("c:/", "simple", "c:/simple")]
            [WindowsTestCase("c:/assets/shaders/", "c:/simple", "c:/simple")]
            [TestCase("assets/shaders", "simple", "assets/shaders/simple")]
            [TestCase("assets/shaders/", "simple", "assets/shaders/simple")]
            [TestCase("/assets/shaders/", "simple", "/assets/shaders/simple")]
            [TestCase("assets", "/other/assets", "/other/assets")]
            [TestCase("assets", "", "assets")]
            [TestCase("", "other", "other")]
            [TestCase("foo/assets", "", "foo/assets")]
            [TestCase("", "foo/other", "foo/other")]
            [TestCase("/foo/assets", "", "/foo/assets")]
            [TestCase("", "/foo/other", "/foo/other")]
            [TestCase("assets", "/", "/")]
            [TestCase("/", "other", "/other")]
            [TestCase("foo/assets", "/", "/")]
            [TestCase("/", "foo/other", "/foo/other")]
            [TestCase("/foo/assets", "/", "/")]
            [TestCase("/", "/foo/other", "/foo/other")]
            public void ShouldCombinePaths(string first, string second, string expected)
            {
                // Given
                NormalizedPath firstPath = new NormalizedPath(first);
                NormalizedPath secondPath = new NormalizedPath(second);

                // When
                NormalizedPath result = firstPath.Combine(secondPath);

                // Then
                result.FullPath.ShouldBe(expected);
                result.FullPath.ShouldBe(System.IO.Path.Combine(first, second).Replace("\\", "/"));
            }

            [Test]
            public void ShouldCombineMultiplePaths()
            {
                // Given
                NormalizedPath first = "a";
                NormalizedPath second = "b";
                NormalizedPath third = "c";
                NormalizedPath fourth = "d";
                NormalizedPath fifth = "e";

                // When
                NormalizedPath result = NormalizedPath.Combine(first, second, third, fourth, fifth);

                // Then
                result.FullPath.ShouldBe("a/b/c/d/e");
            }

            [Test]
            public void ShouldThrowIfPathIsNull()
            {
                // Given
                NormalizedPath path = new NormalizedPath("assets");

                // When
                TestDelegate test = () => path.Combine(null);

                // Then
                Assert.That(test, Throws.TypeOf<ArgumentNullException>());
            }
        }

        public class ContainsChildTests : NormalizedPathFixture
        {
            [TestCase("", "", false)]
            [TestCase("", "test.txt", true)]
            [TestCase("", "/test.txt", false)]
            [TestCase("/", "/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", false)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChild(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("", "", false)]
            [TestCase("", "a", true)]
            [TestCase("", "/a", false)]
            [TestCase("/", "/a", true)]
            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", false)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", false)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChild(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class ContainsChildOrSelfTests : NormalizedPathFixture
        {
            [TestCase("", "", true)]
            [TestCase("", "test.txt", true)]
            [TestCase("", "/test.txt", false)]
            [TestCase("/", "/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", false)]
            [TestCase("/a/b/c/test.txt", "/a/b/c/test.txt", true)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChildOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("", "", true)]
            [TestCase("", "a", true)]
            [TestCase("", "/a", false)]
            [TestCase("/", "/a", true)]
            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", true)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", false)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsChildOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class IsSiblingTests : NormalizedPathFixture
        {
            [TestCase("", "/test.txt", false)]
            [TestCase("/a/b/c", "/a/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", false)]
            [TestCase("/a/b/c/test.txt", "/a/b/c/test.txt", false)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.IsSibling(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("", "/a", false)]
            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", false)]
            [TestCase("/a/b/c", "/a/b/e", true)]
            [TestCase("/a/b/c", "/a/b/c/d", false)]
            [TestCase("/a/b/c", "/a/b/c/d/e", false)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.IsSibling(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class IsSiblingOrSelfTests : NormalizedPathFixture
        {
            [TestCase("", "/test.txt", false)]
            [TestCase("/a/b/c", "/a/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", false)]
            [TestCase("/a/b/c/text.txt", "/a/b/c/test.txt", true)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.IsSiblingOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("", "/a", false)]
            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", true)]
            [TestCase("/a/b/c", "/a/b/e", true)]
            [TestCase("/a/b/c", "/a/b/c/d", false)]
            [TestCase("/a/b/c", "/a/b/c/d/e", false)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.IsSiblingOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class ContainsDescendantTests : NormalizedPathFixture
        {
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", true)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendant(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", false)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", true)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendant(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class ContainsDescendantOrSelfTests : NormalizedPathFixture
        {
            [TestCase("/a/b/c", "/a/b/test.txt", false)]
            [TestCase("/a/b/c", "/a/b/c/test.txt", true)]
            [TestCase("/a/b/c", "/a/b/c/d/test.txt", true)]
            [TestCase("/a/b/c/test.txt", "/a/b/c/test.txt", true)]
            public void ShouldCheckFilePath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendantOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }

            [TestCase("/a/b/c", "/a/b", false)]
            [TestCase("/a/b/c", "/a/b/c", true)]
            [TestCase("/a/b/c", "/a/b/c/d", true)]
            [TestCase("/a/b/c", "/a/b/c/d/e", true)]
            public void ShouldCheckDirectoryPath(string directory, string path, bool expected)
            {
                // Given
                NormalizedPath directoryPath = new NormalizedPath(directory);
                NormalizedPath filePath = new NormalizedPath(path);

                // When
                bool result = directoryPath.ContainsDescendantOrSelf(filePath);

                // Then
                result.ShouldBe(expected);
            }
        }

        public class OptimizeFileNameTests : NormalizedPathFixture
        {
            [TestCase(
                "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789._~:?#[]@!$&'()*+,;=",
                "abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789")]
            [TestCase("Děku.jemeविकीвики_движка", "děku.jemeविकीвикидвижка")]
            [TestCase(
                "this is my title - and some \t\t\t\t\n   clever; (piece) of text here: [ok].",
                "this-is-my-title-and-some-clever-piece-of-text-here-ok")]
            [TestCase(
                "this is my title?!! science and #firstworldproblems :* :sadface=true",
                "this-is-my-title-science-and-firstworldproblems-sadfacetrue")]
            [TestCase(
                "one_two_three__four__five and a six__seven__eight_nine______ten",
                "onetwothreefourfive-and-a-sixseveneightnineten")]
            public void FileNameIsConvertedCorrectly(string input, string output)
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName(input);

                // Then
                result.ShouldBe(output);
            }

            [Test]
            public void FileNameShouldBeLowercase()
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName("FileName With MiXeD CapS");

                // Then
                result.ShouldBe("filename-with-mixed-caps");
            }

            [Test]
            public void CanChangeReservedCharacters()
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName(
                    "this-is_a-.net-tag",
                    reservedChars: NormalizedPath.OptimizeFileNameReservedChars.Replace("_", string.Empty));

                // Then
                result.ShouldBe("this-is_a-.net-tag");
            }

            [Test]
            public void DoesNotTrimDot()
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName("this_is_a_.", trimDot: false);

                // Then
                result.ShouldBe("thisisa.");
            }

            [TestCase(null)]
            [TestCase("")]
            [TestCase(" ")]
            public void IgnoresNullOrWhiteSpaceStrings(string input)
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName(input);

                // Then
                result.ShouldBeEmpty();
            }

            [Test]
            public void PreservesExtension()
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName("myfile.html");

                // Then
                result.ShouldBe("myfile.html");
            }

            [Test]
            public void TrimWhitespace()
            {
                // Given, When
                string result = NormalizedPath.OptimizeFileName("   myfile.html   ");

                // Then
                result.ShouldBe("myfile.html");
            }

            [Test]
            public void OptimizesInstance()
            {
                // Given
                NormalizedPath path = new NormalizedPath("a/b/c/ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789_~:?#[]@!$&'()*+,;=.html");

                // When
                NormalizedPath result = path.OptimizeFileName();

                // Then
                result.FullPath.ShouldBe("a/b/c/abcdefghijklmnopqrstuvwxyzabcdefghijklmnopqrstuvwxyz0123456789.html");
            }

            [TestCase("2020.09.29.1.html", "2020.09.29.1")]
            [TestCase("Statiq.Framework.sln.DotSettings", "Statiq.Framework.Sln")]
            [TestCase("README.md", "README")]
            [TestCase("FOO", "FOO")]
            [TestCase("FOO/index.md", "FOO")]
            public void GetTitle(string input, string expect)
            {
                // Given
                NormalizedPath path = new NormalizedPath(input);

                // When
                string title = path.GetTitle();

                // Then
                title.ShouldBe(expect);
            }
        }
    }
}
