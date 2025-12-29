using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Common.Tests.Scripting
{
    [TestFixture]
    public class IScriptHelperTestFixture : BaseFixture
    {
        public class TryGetScriptStringTests : IScriptHelperTestFixture
        {
            [TestCase("foo", null, null)]
            [TestCase("=foo", null, null)]
            [TestCase("=>foo", true, "foo")]
            [TestCase("->foo", false, "foo")]
            [TestCase(" =foo", null, null)]
            [TestCase(" =>foo", true, "foo")]
            [TestCase(" ->foo", false, "foo")]
            [TestCase("= >foo", null, null)]
            [TestCase("- >foo", null, null)]
            [TestCase("=> foo", true, " foo")]
            [TestCase("-> foo", false, " foo")]
            [TestCase(" => foo", true, " foo")]
            [TestCase(" -> foo", false, " foo")]
            [TestCase("bar=>foo", null, null)]
            [TestCase("bar->foo", null, null)]
            [TestCase("  => foo ", true, " foo ")]
            [TestCase("  -> foo ", false, " foo ")]
            [TestCase(" = > foo", null, null)]
            [TestCase(" - > foo", null, null)]
            public void GetsScriptString(string input, bool? expected, string expectedScript)
            {
                // Given, When
                bool? result = IScriptHelper.TryGetScriptString(input, out string resultScript);

                // Then
                result.ShouldBe(expected);
                resultScript.ShouldBe(expectedScript);
            }
        }
    }
}