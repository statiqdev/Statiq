using System;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.IO
{
    [TestFixture]
    public class RelativePathResolverFixture : BaseFixture
    {
        public class ResolveTests : RelativePathResolverFixture
        {
            [WindowsTestCase("C:/A/B/C", "C:/A/B/C", "")]
            [WindowsTestCase("C:/", "C:/", "")]
            [WindowsTestCase("C:/", "C:", "")]
            [WindowsTestCase("C:", "C:/", "")]
            [WindowsTestCase("C:", "C:", "")]
            [WindowsTestCase("C:/A/B/C", "C:/A/D/E", "../../D/E")]
            [WindowsTestCase("C:/A/B/C", "C:/", "../../..")]
            [WindowsTestCase("C:/A/B/C", "C:", "../../..")]
            [WindowsTestCase("C:/A/B/C/D/E/F", "C:/A/B/C", "../../..")]
            [WindowsTestCase("C:/A/B/C", "C:/A/B/C/D/E/F", "D/E/F")]
            [WindowsTestCase("C:/A/B/C", "W:/X/Y/Z", "W:/X/Y/Z")]
            [WindowsTestCase("C:/A/B/C", "D:/A/B/C", "D:/A/B/C")]
            [WindowsTestCase("C:/A/B", "D:/E/", "D:/E")]
            [WindowsTestCase("C:/", "B:/", "B:/")]
            [WindowsTestCase("C:/", "B:", "B:/")]
            [WindowsTestCase("C:", "B:/", "B:/")]
            [WindowsTestCase("C:", "B:", "B:/")]
            [WindowsTestCase("C:/", "/", "/")]
            [WindowsTestCase("C:", "/", "/")]

            // Absolute
            [TestCase("/C/A/B/C", "/C/A/B/C", "")]
            [TestCase("/C/", "/C/", "")]
            [TestCase("/C/A/B/C", "/C/A/D/E", "../../D/E")]
            [TestCase("/C/A/B/C", "/C/", "../../..")]
            [TestCase("/C/A/B/C/D/E/F", "/C/A/B/C", "../../..")]
            [TestCase("/C/A/B/C", "/C/A/B/C/D/E/F", "D/E/F")]
            [TestCase("/C/A/B/C", "/W/X/Y/Z", "/W/X/Y/Z")]
            [TestCase("/C/A/B/C", "/D/A/B/C", "/D/A/B/C")]
            [TestCase("/C/A/B", "/D/E/", "/D/E")]
            [TestCase("/C/", "/B/", "/B")]
            [TestCase("/", "/A/B", "A/B")]
            [TestCase("/C/A/B", "/", "/")]

            // Relative
            [TestCase("C/A/B/C", "C/A/B/C", "")]
            [TestCase("C/", "C/", "")]
            [TestCase("C/A/B/C", "C/A/D/E", "../../D/E")]
            [TestCase("C/A/B/C", "C/", "../../..")]
            [TestCase("C/A/B/C/D/E/F", "C/A/B/C", "../../..")]
            [TestCase("C/A/B/C", "C/A/B/C/D/E/F", "D/E/F")]
            [TestCase("C/A/B/C", "W/X/Y/Z", "W/X/Y/Z")]
            [TestCase("C/A/B/C", "D/A/B/C", "D/A/B/C")]
            [TestCase("C/A/B", "D/E/", "D/E")]
            [TestCase("C/", "B/", "B")]
            [TestCase("C/A/B", "", "C/A/B")]
            [TestCase("", "C/A/B", "C/A/B")]
            [TestCase("C/A/B", ".", ".")]
            [TestCase(".", "C/A/B", "C/A/B")]
            [TestCase("C/A/B", "C/.", "../..")]
            [TestCase("C/.", "C/A/B", "A/B")]
            [TestCase("", "", "")]
            [TestCase(".", "", ".")]
            [TestCase("", ".", ".")]
            [TestCase(".", ".", ".")]

            public void ShouldReturnRelativePathWithDirectoryPath(string source, string target, string expected)
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath(source);
                NormalizedPath targetPath = new NormalizedPath(target);

                // When
                NormalizedPath relativePath = RelativePathResolver.Resolve(sourcePath, targetPath);

                // Then
                ((string)relativePath).ShouldBe(expected);
                if (targetPath.IsAbsolute)
                {
                    sourcePath.Combine(relativePath).ShouldBe(targetPath);
                }
            }

            [WindowsTestCase("C:/A/B/C", "C:/A/B/C/hello.txt", "hello.txt")]
            [WindowsTestCase("C:/", "C:/hello.txt", "hello.txt")]
            [WindowsTestCase("C:", "C:/hello.txt", "hello.txt")]
            [WindowsTestCase("C:/A/B/C", "C:/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [WindowsTestCase("C:/A/B/C", "C:/hello.txt", "../../../hello.txt")]
            [WindowsTestCase("C:/A/B/C/D/E/F", "C:/A/B/C/hello.txt", "../../../hello.txt")]
            [WindowsTestCase("C:/A/B/C", "C:/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [WindowsTestCase("C:/A/B/C", "W:/X/Y/Z/hello.txt", "W:/X/Y/Z/hello.txt")]
            [WindowsTestCase("C:/A/B/C", "D:/A/B/C/hello.txt", "D:/A/B/C/hello.txt")]
            [WindowsTestCase("C:/A/B", "D:/E/hello.txt", "D:/E/hello.txt")]
            [WindowsTestCase("C:/", "B:/hello.txt", "B:/hello.txt")]
            [WindowsTestCase("C:", "B:/hello.txt", "B:/hello.txt")]

            // Absolute
            [TestCase("/C/A/B/C", "/C/A/B/C/hello.txt", "hello.txt")]
            [TestCase("/C/", "/C/hello.txt", "hello.txt")]
            [TestCase("/C/A/B/C", "/C/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [TestCase("/C/A/B/C", "/C/hello.txt", "../../../hello.txt")]
            [TestCase("/C/A/B/C/D/E/F", "/C/A/B/C/hello.txt", "../../../hello.txt")]
            [TestCase("/C/A/B/C", "/C/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [TestCase("/C/A/B/C", "/W/X/Y/Z/hello.txt", "/W/X/Y/Z/hello.txt")]
            [TestCase("/C/A/B/C", "/D/A/B/C/hello.txt", "/D/A/B/C/hello.txt")]
            [TestCase("/C/A/B", "/D/E/hello.txt", "/D/E/hello.txt")]
            [TestCase("/C/", "/B/hello.txt", "/B/hello.txt")]

            // Relative
            [TestCase("C/A/B/C", "C/A/B/C/hello.txt", "hello.txt")]
            [TestCase("C/", "C/hello.txt", "hello.txt")]
            [TestCase("C/A/B/C", "C/A/D/E/hello.txt", "../../D/E/hello.txt")]
            [TestCase("C/A/B/C", "C/hello.txt", "../../../hello.txt")]
            [TestCase("C/A/B/C/D/E/F", "C/A/B/C/hello.txt", "../../../hello.txt")]
            [TestCase("C/A/B/C", "C/A/B/C/D/E/F/hello.txt", "D/E/F/hello.txt")]
            [TestCase("C/A/B/C", "W/X/Y/Z/hello.txt", "W/X/Y/Z/hello.txt")]
            [TestCase("C/A/B/C", "D/A/B/C/hello.txt", "D/A/B/C/hello.txt")]
            [TestCase("C/A/B", "D/E/hello.txt", "D/E/hello.txt")]
            [TestCase("C/", "B/hello.txt", "B/hello.txt")]
            public void ShouldReturnRelativePathWithFilePath(string source, string target, string expected)
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath(source);
                NormalizedPath targetPath = new NormalizedPath(target);

                // When
                NormalizedPath relativePath = RelativePathResolver.Resolve(sourcePath, targetPath);

                // Then
                expected.ShouldBe((string)relativePath);
                if (targetPath.IsAbsolute)
                {
                    sourcePath.Combine(relativePath).ShouldBe(targetPath);
                }
            }

            [Test]
            public void ShouldThrowIfSourceIsNullWithDirectoryPath()
            {
                // Given
                NormalizedPath targetPath = new NormalizedPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(null, targetPath));
            }

            [Test]
            public void ShouldThrowIfTargetIsNullWithDirectoryPath()
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(sourcePath, (NormalizedPath)null));
            }

            [Test]
            [TestCase("/A/B", "A/B")]
            [TestCase("A/B", "/A/B")]
            public void ShouldThrowIfNotBothSameAbsoluteWithDirectoryPath(string source, string target)
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath(source);
                NormalizedPath targetPath = new NormalizedPath(target);

                // When, Then
                Assert.Throws<ArgumentException>(() => RelativePathResolver.Resolve(sourcePath, targetPath));
            }

            [Test]
            public void ShouldThrowIfSourceIsNullWithFilePath()
            {
                // Given
                NormalizedPath targetPath = new NormalizedPath("/A/hello.txt");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(null, targetPath));
            }

            [Test]
            public void ShouldThrowIfTargetIsNullWithFilePath()
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath("/A");

                // When, Then
                Assert.Throws<ArgumentNullException>(() => RelativePathResolver.Resolve(sourcePath, (NormalizedPath)null));
            }

            [Test]
            [TestCase("/A/B", "A/B/hello.txt")]
            [TestCase("A/B", "/A/B/hello.txt")]
            public void ShouldThrowIfNotBothSameAbsoluteWithFilePath(string source, string target)
            {
                // Given
                NormalizedPath sourcePath = new NormalizedPath(source);
                NormalizedPath targetPath = new NormalizedPath(target);

                // When, Then
                Assert.Throws<ArgumentException>(() => RelativePathResolver.Resolve(sourcePath, targetPath));
            }
        }
    }
}
