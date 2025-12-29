using System.Collections.Generic;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Util
{
    [TestFixture]
    public class ArgumentSplitterFixture : BaseFixture
    {
        public class SplitTests : ArgumentSplitterFixture
        {
            // From http://stackoverflow.com/a/298990/807064
            [TestCase("", new string[] { })]
            [TestCase("a", new[] { "a" })]
            [TestCase(" abc ", new[] { "abc" })]
            [TestCase("a b ", new[] { "a", "b" })]
            [TestCase("a b \"c d\"", new[] { "a", "b", "c d" })]
            [TestCase(
                @"/src:""C:\tmp\Some Folder\Sub Folder"" /users:""abcdefg@hijkl.com"" tasks:""SomeTask,Some Other Task"" -someParam",
                new[] { @"/src:""C:\tmp\Some Folder\Sub Folder""", @"/users:""abcdefg@hijkl.com""", @"tasks:""SomeTask,Some Other Task""", "-someParam" })]
            public void ShouldSplitExceptInQuotes(string arguments, string[] expected)
            {
                // Given, When
                IEnumerable<string> actual = ArgumentSplitter.Split(arguments);

                // Then
                actual.ShouldBe(expected);
            }
        }
    }
}
