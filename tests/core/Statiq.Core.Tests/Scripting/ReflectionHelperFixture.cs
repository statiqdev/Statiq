using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Scripting
{
    [TestFixture]
    public class ReflectionHelperFixture : BaseFixture
    {
        public class GetCallSignatureTests : ReflectionHelperFixture
        {
            [Test]
            public void GetsClassCallSignatures()
            {
                // Given, When
                string[] callSignatures = ReflectionHelper.GetCallSignatures(typeof(TestClass), "Through").ToArray();

                // Then
                callSignatures.ShouldBe(
                    new[]
                    {
                        "public double Dubs() => Through.Dubs();",
                        "public System.Single Floats(int f) => Through.Floats(f);",
                        "public string BBB => Through.BBB;",
                        "public int ExtA(int abc) => Through.ExtA(abc);",
                        "public string ExtB(int def) => Through.ExtB(def);"
                    },
                    true);
            }

            [Test]
            public void GetsInterfaceCallSignatures()
            {
                // Given, When
                string[] callSignatures = ReflectionHelper.GetCallSignatures(typeof(IFoo), "Through").ToArray();

                // Then
                callSignatures.ShouldBe(
                    new[]
                    {
                        "public double Dubs() => Through.Dubs();",
                        "public System.Single Floats(int f) => Through.Floats(f);",
                        "public string Bar(int foo) => Through.Bar(foo);",
                        "public int Foo(string bar, int qwerty = 2) => Through.Foo(bar, qwerty);",
                        "public System.DateTime GetDate(int buzz) => Through.GetDate(buzz);",
                        "public int AAA => Through.AAA;",
                        "public string BBB => Through.BBB;",
                        "public int ExtC(int ghi) => Through.ExtC(ghi);",
                        "public string ExtD(int jkl) => Through.ExtD(jkl);"
                    },
                    true);
            }
        }

        public class TestBase
        {
            public string BBB => "bbb";
        }

        public class TestClass : TestBase
        {
            public double Dubs() => 0;

            public float Floats(int f) => 1.1f;

            private string Str() => "str";

#pragma warning disable SA1400 // Access modifier should be declared
            int AAA { get; set; }
#pragma warning restore SA1400 // Access modifier should be declared

            private bool CCC => false;
        }

        public interface IBase
        {
            double Dubs();

            float Floats(int f) => 1.1f;

            int AAA { get; }

            public string BBB => "bbb";
        }

        public partial interface IFoo
        {
            public string Bar(int foo) => Foo(foo.ToString()).ToString();
        }

        public partial interface IFoo : IBase
        {
            int Foo(string bar, int qwerty = 2);

            DateTime GetDate(int buzz);
        }
    }

#pragma warning disable SA1402 // File may only contain a single type
    public static class TestClassExtensions
    {
        public static int ExtA(this ReflectionHelperFixture.TestClass testClass, int abc) => 1;
    }

    public static class TestBaseExtensions
    {
        public static string ExtB(this ReflectionHelperFixture.TestBase testBase, int def) => "2";
    }

    public static class IBaseExtensions
    {
        public static int ExtC(this ReflectionHelperFixture.IBase bse, int ghi) => 3;
    }

    public static class IFooExtensions
    {
        public static string ExtD(this ReflectionHelperFixture.IFoo foo, int jkl) => "4";
    }
#pragma warning restore SA1402 // File may only contain a single type
}
