using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using Shouldly;
using Statiq.Core;
using Statiq.Testing;

namespace Statiq.Common.Tests.Settings
{
    [TestFixture]
    public class SettingsFixture : BaseFixture
    {
        public class BuildConfigurationObjectTests : SettingsFixture
        {
            [Test]
            public void ConstructsObjectGraph()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();

                // When
                object graph = Common.Settings.BuildConfigurationObject(configuration, null);

                // Then
                IDictionary<string, object> root = graph.ShouldBeAssignableTo<IDictionary<string, object>>();
                root.ShouldContainKeyAndValue("key0", "value0");
                root.ShouldContainKey("section0");
                root.ShouldContainKey("section1");
                root.ShouldContainKey("section2");
                IDictionary<string, object> dictionary = root["section0"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("key1", "value1");
                dictionary.ShouldContainKey("key2");
                IList<object> list = dictionary["key2"].ShouldBeAssignableTo<IList<object>>();
                list.Count.ShouldBe(3);
                list[0].ShouldBe("=> \"arr1\"");
                list[1].ShouldBe("arr2");
                dictionary = list[2].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("foo1", "bar1");
                dictionary.ShouldContainKeyAndValue("foo2", "=> \"bar2\"");
                dictionary = root["section1"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKeyAndValue("key3", "3");
                dictionary.ShouldContainKeyAndValue("key4", "value4");
                dictionary = root["section2"].ShouldBeAssignableTo<IDictionary<string, object>>();
                dictionary.ShouldContainKey("subsection0");
                IDictionary<string, object> subsection = dictionary["subsection0"].ShouldBeAssignableTo<IDictionary<string, object>>();
                subsection.ShouldContainKeyAndValue("key5", "=> 1 + 2");
                subsection.ShouldContainKeyAndValue("key6", "value6");
                dictionary.ShouldContainKey("subsection1");
                subsection = dictionary["subsection1"].ShouldBeAssignableTo<IDictionary<string, object>>();
                subsection.ShouldContainKeyAndValue("key7", "value7");
                subsection.ShouldContainKeyAndValue("key8", "value8");
            }
        }

        public class GetMetadataTests : SettingsFixture
        {
            [Test]
            public void GetsNestedMetadata()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                object result = settings.GetMetadata("section2").GetMetadata("subsection1")["key7"];

                // Then
                result.ShouldBe("value7");
            }
        }

        public class TryGetValueTests : SettingsFixture
        {
            [Test]
            public void GetsNormalValue()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC {1+2} XYZ");
            }

            [Test]
            public void GetsOverridenValue()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""ABC {1+2} XYZ""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration);
                settings["foo"] = 123;
                settings = settings.WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(123);
            }

            [Test]
            public void EvaluatesExpression()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {1+2} XYZ\""""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesReturnStatement()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> return $\""ABC {1+2} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesMultipleStatements()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> { int x = 1 + 2; return $\""ABC {x} XYZ\""; }""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesMultipleStatementsWithoutBraces()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> int x = 1 + 2; return $\""ABC {x} XYZ\"";""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesIntExpression()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> 1 + 2""
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe(3);
            }

            [Test]
            public void CanAccessOtherValuesInExpression()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""foo"": ""=> $\""ABC {Get(\""bar\"")} XYZ\"""",
  ""bar"": 3
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                bool result = settings.TryGetValue("foo", out object value);

                // Then
                result.ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class IndexerTests : SettingsFixture
        {
            [Test]
            public void EvaluatesLateAddedSettingExpression()
            {
                // Given
                IConfigurationRoot configuration = new ConfigurationRoot(Array.Empty<IConfigurationProvider>());
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                settings.Add("Foo", @"=> $""ABC {1+2} XYZ""");

                // Then
                settings.TryGetValue("foo", out object value).ShouldBeTrue();
                value.ShouldBe("ABC 3 XYZ");
            }
        }

        public class IConfigurationTests : SettingsFixture
        {
            [TestCase("missing", "missing", "missing", null, 0)]
            [TestCase("missing:missing2", "missing2", "missing:missing2", null, 0)]
            [TestCase("section0:missing", "missing", "section0:missing", null, 0)]
            [TestCase("key0", "key0", "key0", "value0", 0)]
            [TestCase("section0", "section0", "section0", null, 2)]
            [TestCase("section0:key1", "key1", "section0:key1", "value1", 0)]
            [TestCase("section0:key2:0", "0", "section0:key2:0", "arr1", 0)]
            [TestCase("section0:key2:1", "1", "section0:key2:1", "arr2", 0)]
            [TestCase("section0:key2:1000", "1000", "section0:key2:1000", null, 0)]
            [TestCase("section0:key2:1:foo2", "foo2", "section0:key2:1:foo2", null, 0)]
            [TestCase("section0:key2:2:foo2", "foo2", "section0:key2:2:foo2", "bar2", 0)]
            [TestCase("section0:key2:20:foo2", "foo2", "section0:key2:20:foo2", null, 0)]
            [TestCase("section2:subsection0:key5", "key5", "section2:subsection0:key5", "3", 0)]
            public void GetsConfigurationSection(
                string key,
                string expectedKey,
                string expectedPath,
                string expectedValue,
                int expectedChildrenCount)
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settings.GetSection(key);

                // Then
                section.Key.ShouldBe(expectedKey);
                section.Path.ShouldBe(expectedPath);
                section.Value.ShouldBe(expectedValue);
                section.GetChildren().Count().ShouldBe(expectedChildrenCount);
            }

            [Test]
            public void GetsValueAddedToSettings()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);
                settings.Add("newkey", "newvalue");

                // When
                IConfigurationSection section = ((IConfiguration)settings).GetSection("newkey");

                // Then
                section.Key.ShouldBe("newkey");
                section.Path.ShouldBe("newkey");
                section.Value.ShouldBe("newvalue");
                section.GetChildren().Count().ShouldBe(0);
            }

            [Test]
            public void GetsListAddedToSettings()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                Common.Settings settings = new Common.Settings(configuration).WithExecutionState(context);
                settings.Add("newkey", new List<int> { 2, 4, 6, 8 });

                // When
                IConfigurationSection section = ((IConfiguration)settings).GetSection("newkey:1");

                // Then
                section.Key.ShouldBe("1");
                section.Path.ShouldBe("newkey:1");
                section.Value.ShouldBe("4");
                section.GetChildren().Count().ShouldBe(0);
            }

            [Test]
            public void GetsNestedSection()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration();
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settings = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settings.GetSection("section0");
                IConfigurationSection result = section.GetSection("key1");

                // Then
                section.Key.ShouldBe("section0");
                section.Path.ShouldBe("section0");
                section.Value.ShouldBeNull();
                result.Key.ShouldBe("key1");
                result.Path.ShouldBe("section0:key1");
                result.Value.ShouldBe("value1");
            }

            [Test]
            public void EvaluatesExpressionInNestedSection()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settingsConfiguration = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settingsConfiguration.GetSection("section0:foo");

                // Then
                section.Key.ShouldBe("foo");
                section.Path.ShouldBe("section0:foo");
                section.Value.ShouldBe("ABC 3 XYZ");
            }

            [Test]
            public void EvaluatesExpressionThroughNestedSection()
            {
                // Given
                IConfigurationRoot configuration = GetConfiguration(@"
{
  ""section0"": {
    ""foo"": ""=> $\""ABC {1+2} XYZ\""""
  }
}");
                TestExecutionContext context = new TestExecutionContext();
                context.ScriptHelper = new ScriptHelper(context);
                IConfiguration settingsConfiguration = new Common.Settings(configuration).WithExecutionState(context);

                // When
                IConfigurationSection section = settingsConfiguration.GetSection("section0");
                IConfigurationSection result = section.GetSection("foo");

                // Then
                section.Key.ShouldBe("section0");
                section.Path.ShouldBe("section0");
                section.Value.ShouldBeNull();
                result.Key.ShouldBe("foo");
                result.Path.ShouldBe("section0:foo");
                result.Value.ShouldBe("ABC 3 XYZ");
            }
        }

        private static IConfigurationRoot GetConfiguration(string json = Json)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(json);
            writer.Flush();
            stream.Position = 0;
            return new ConfigurationBuilder().AddJsonStream(stream).Build();
        }

        private const string Json = @"{
  ""key0"": ""value0"",
  ""section0"": {
    ""key1"": ""value1"",
    ""key2"": [
      ""=> \""arr1\"""",
      ""arr2"",
      {
        ""foo1"": ""bar1"",
        ""foo2"": ""=> \""bar2\""""
      }
    ]
  },
  ""section1"": {
    ""key3"": ""3"",
    ""key4"": ""value4""
  },
  ""section2"": {
    ""subsection0"" : {
      ""key5"": ""=> 1 + 2"",
      ""key6"": ""value6""
    },
    ""subsection1"" : {
      ""key7"": ""value7"",
      ""key8"": ""value8""
    }
  }
}";
    }
}
