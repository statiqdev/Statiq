using System;
using System.Reflection;
using Microsoft.Data.SqlClient;
using NUnit.Framework;
using Shouldly;
using Statiq.Testing;

namespace Statiq.Core.Tests.Modules.Metadata
{
    [TestFixture]
    public class ReadSqlFixture : BaseFixture
    {
        public class ConstructorTests : ReadSqlFixture
        {
            [Test]
            public void ThrowsForEmptyConnectionString()
            {
                Assert.Throws<ArgumentException>(() => new ReadSql(string.Empty, "SELECT 1"));
            }

            [Test]
            public void ThrowsForEmptyQuery()
            {
                Assert.Throws<ArgumentException>(() => new ReadSql("Server=.;Database=tempdb;Trusted_Connection=True;", string.Empty));
            }
        }

        public class AzureExtensionTests : ReadSqlFixture
        {
            [Test]
            public void AzureExtensionAssemblyIsAvailable()
            {
                Assembly assembly = Assembly.Load("Microsoft.Data.SqlClient.Extensions.Azure");
                assembly.ShouldNotBeNull();
            }

            [Test]
            public void ParsesActiveDirectoryAuthenticationConnectionString()
            {
                SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    DataSource = "example.database.windows.net",
                    InitialCatalog = "Example",
                    Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault
                };

                builder.Authentication.ShouldBe(SqlAuthenticationMethod.ActiveDirectoryDefault);
                using SqlConnection connection = new SqlConnection(builder.ConnectionString);
                connection.ConnectionString.ShouldContain("ActiveDirectoryDefault");
            }
        }
    }
}
