using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NUnit.Framework;
using Shouldly;
using Statiq.Common;
using Statiq.Testing;

namespace Statiq.Razor.Tests
{
    [TestFixture]
    [NonParallelizable]
    public class IHtmlHelperExtensionsFixture : BaseFixture
    {
        public class DocumentLinkTests : IHtmlHelperExtensionsFixture
        {
            [Test]
            public void ThrowsForNullHtmlHelper()
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath("/a/b/c"));

                // When
                Should.Throw<ArgumentNullException>(() => ((IHtmlHelper)null).DocumentLink(document));
            }

            [Test]
            public void ThrowsForNullDocument()
            {
                // Given
                IHtmlHelper htmlHelper = new TestHtmlHelper();

                // When
                Should.Throw<ArgumentNullException>(() => htmlHelper.DocumentLink(null));
            }

            [Test]
            public void GetsDocumentLink()
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath("a/b/c"));
                IHtmlHelper htmlHelper = new TestHtmlHelper();

                // When
                IHtmlContent result = htmlHelper.DocumentLink(document);

                // Then
                using (StringWriter writer = new StringWriter())
                {
                    result.WriteTo(writer, HtmlEncoder.Default);
                    writer.ToString().ShouldBe(@"<a href=""/a/b/c"">C</a>");
                }
            }

            [Test]
            public void CustomLinkText()
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath("a/b/c"));
                IHtmlHelper htmlHelper = new TestHtmlHelper();

                // When
                IHtmlContent result = htmlHelper.DocumentLink(document, "Foo");

                // Then
                using (StringWriter writer = new StringWriter())
                {
                    result.WriteTo(writer, HtmlEncoder.Default);
                    writer.ToString().ShouldBe(@"<a href=""/a/b/c"">Foo</a>");
                }
            }

            [Test]
            public void QueryAndFragmentWithoutLinkText()
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath("a/b/c"));
                IHtmlHelper htmlHelper = new TestHtmlHelper();

                // When
                IHtmlContent result = htmlHelper.DocumentLink(document, "abc=123#xyz", (string)null);

                // Then
                using (StringWriter writer = new StringWriter())
                {
                    result.WriteTo(writer, HtmlEncoder.Default);
                    writer.ToString().ShouldBe(@"<a href=""/a/b/c?abc=123#xyz"">C</a>");
                }
            }

            [Test]
            public void QueryAndFragmentWithLinkText()
            {
                // Given
                TestDocument document = new TestDocument(new NormalizedPath("a/b/c"));
                IHtmlHelper htmlHelper = new TestHtmlHelper();

                // When
                IHtmlContent result = htmlHelper.DocumentLink(document, "abc=123#xyz", "Foo");

                // Then
                using (StringWriter writer = new StringWriter())
                {
                    result.WriteTo(writer, HtmlEncoder.Default);
                    writer.ToString().ShouldBe(@"<a href=""/a/b/c?abc=123#xyz"">Foo</a>");
                }
            }
        }

        public class CachedPartialTests : IHtmlHelperExtensionsFixture
        {
            [TestCase(true)]
            [TestCase(false)] // Sanity check
            public async Task CachedPartialShouldRenderTheSameContent(bool cached)
            {
                // Given
                TestExecutionContext context = RenderRazorFixture.GetExecutionContext();
                context.FileSystem.FileProvider = new TestFileProvider
                {
                    {
                        "/input/_Partial.cshtml",
                        "<div>GUID: @Guid.NewGuid().ToString()</div>"
                    },
                };
                TestDocument document1 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test1.cshtml",
                    $@"<p>z1</p>
@Html.{(cached ? "CachedPartial" : "Partial")}(""_Partial"")
<p>After</p>");
                TestDocument document2 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test2.cshtml",
                    $@"<p>z2</p>
@Html.{(cached ? "CachedPartial" : "Partial")}(""_Partial"")
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { document1, document2 }, context, razor);

                // Then
                string content1 = results.Single(x => x.Source.FileNameWithoutExtension == "Test1").Content.Replace("z1", string.Empty);
                string content2 = results.Single(x => x.Source.FileNameWithoutExtension == "Test2").Content.Replace("z2", string.Empty);
                if (cached)
                {
                    content1.ShouldBe(content2);
                }
                else
                {
                    content1.ShouldNotBe(content2);
                }
            }

            // Note that the same string value will intern to the same object
            [TestCase("Foo", "Foo", true)] // Same model
            [TestCase("z1", "z2", false)] // Different model
            public async Task CachedPartialWithModel(
                string model1,
                string model2,
                bool shouldEqual)
            {
                // Given
                TestExecutionContext context = RenderRazorFixture.GetExecutionContext();
                context.Settings.Add("Foo", "Bar");
                context.FileSystem.FileProvider = new TestFileProvider
                {
                    {
                        "/input/_Partial.cshtml",
                        @"@model string
                        <div>@Model</div>
                        <div>GUID: @Guid.NewGuid().ToString()</div>"
                    },
                };
                TestDocument document1 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test1.cshtml",
                    $@"<p>z1</p>
@Html.CachedPartial(""_Partial"", ""{model1}"")
<p>After</p>");
                TestDocument document2 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test2.cshtml",
                    $@"<p>z2</p>
@Html.CachedPartial(""_Partial"", ""{model2}"")
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { document1, document2 }, context, razor);

                // Then
                string content1 = results.Single(x => x.Source.FileNameWithoutExtension == "Test1").Content.Replace("z1", string.Empty);
                string content2 = results.Single(x => x.Source.FileNameWithoutExtension == "Test2").Content.Replace("z2", string.Empty);
                if (shouldEqual)
                {
                    content1.ShouldBe(content2);
                }
                else
                {
                    content1.ShouldNotBe(content2);
                }
            }

            // Note that the same string value will intern to the same object
            [TestCase("Foo", "Bar", "Foo", "Bar", true)] // Same model, same cache key
            [TestCase("Foo", "z1", "Foo", "z2", false)] // Same model, different cache key
            [TestCase("z1", "Bar", "z2", "Bar", true)] // Different model, same cache key
            [TestCase("z1", "z1", "z2", "z2", false)] // Different model, different cache key
            [TestCase(null, "Bar", null, "Bar", true)] // No model, same cache key
            [TestCase(null, "z1", null, "z2", false)] // No model, different cache key
            public async Task CachedPartialWithModelAndCacheKey(
                string model1,
                string cacheKey1,
                string model2,
                string cacheKey2,
                bool shouldEqual)
            {
                // Given
                TestExecutionContext context = RenderRazorFixture.GetExecutionContext();
                context.Settings.Add("Foo", "Bar");
                context.FileSystem.FileProvider = new TestFileProvider
                {
                    {
                        "/input/_Partial.cshtml",
                        @"@model string
                        <div>@Model</div>
                        <div>GUID: @Guid.NewGuid().ToString()</div>"
                    },
                };
                TestDocument document1 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test1.cshtml",
                    $@"<p>z1</p>
@Html.CachedPartial(""_Partial"", ""{model1}"", ""{cacheKey1}"")
<p>After</p>");
                TestDocument document2 = RenderRazorFixture.GetDocument(
                    "/input/RootRelativePartial/Test2.cshtml",
                    $@"<p>z2</p>
@Html.CachedPartial(""_Partial"", ""{model2}"", ""{cacheKey2}"")
<p>After</p>");
                RenderRazor razor = new RenderRazor();

                // When
                ImmutableArray<TestDocument> results = await ExecuteAsync(new[] { document1, document2 }, context, razor);

                // Then
                string content1 = results.Single(x => x.Source.FileNameWithoutExtension == "Test1").Content.Replace("z1", string.Empty);
                string content2 = results.Single(x => x.Source.FileNameWithoutExtension == "Test2").Content.Replace("z2", string.Empty);
                if (shouldEqual)
                {
                    content1.ShouldBe(content2);
                }
                else
                {
                    content1.ShouldNotBe(content2);
                }
            }
        }

        private class TestHtmlHelper : IHtmlHelper
        {
            public Html5DateRenderingMode Html5DateRenderingMode { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public string IdAttributeDotReplacement => throw new NotImplementedException();

            public IModelMetadataProvider MetadataProvider => throw new NotImplementedException();

            public ITempDataDictionary TempData => throw new NotImplementedException();

            public UrlEncoder UrlEncoder => throw new NotImplementedException();

            public dynamic ViewBag => throw new NotImplementedException();

            public Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext => throw new NotImplementedException();

            public ViewDataDictionary ViewData => throw new NotImplementedException();

            public IHtmlContent ActionLink(string linkText, string actionName, string controllerName, string protocol, string hostname, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent AntiForgeryToken() => throw new NotImplementedException();

            public MvcForm BeginForm(string actionName, string controllerName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();

            public MvcForm BeginRouteForm(string routeName, object routeValues, FormMethod method, bool? antiforgery, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent CheckBox(string expression, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent Display(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();

            public string DisplayName(string expression) => throw new NotImplementedException();

            public string DisplayText(string expression) => throw new NotImplementedException();

            public IHtmlContent DropDownList(string expression, IEnumerable<SelectListItem> selectList, string optionLabel, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent Editor(string expression, string templateName, string htmlFieldName, object additionalViewData) => throw new NotImplementedException();

            public string Encode(object value) => throw new NotImplementedException();

            public string Encode(string value) => throw new NotImplementedException();

            public void EndForm() => throw new NotImplementedException();

            public string FormatValue(object value, string format) => throw new NotImplementedException();

            public string GenerateIdFromName(string fullName) => throw new NotImplementedException();

            public IEnumerable<SelectListItem> GetEnumSelectList(Type enumType) => throw new NotImplementedException();

            public IEnumerable<SelectListItem> GetEnumSelectList<TEnum>()
                where TEnum : struct =>
                throw new NotImplementedException();

            public IHtmlContent Hidden(string expression, object value, object htmlAttributes) => throw new NotImplementedException();

            public string Id(string expression) => throw new NotImplementedException();

            public IHtmlContent Label(string expression, string labelText, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent ListBox(string expression, IEnumerable<SelectListItem> selectList, object htmlAttributes) => throw new NotImplementedException();

            public string Name(string expression) => throw new NotImplementedException();

            public Task<IHtmlContent> PartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();

            public IHtmlContent Password(string expression, object value, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent RadioButton(string expression, object value, bool? isChecked, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent Raw(object value) => throw new NotImplementedException();

            public IHtmlContent Raw(string value) => throw new NotImplementedException();

            public Task RenderPartialAsync(string partialViewName, object model, ViewDataDictionary viewData) => throw new NotImplementedException();

            public IHtmlContent RouteLink(string linkText, string routeName, string protocol, string hostName, string fragment, object routeValues, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent TextArea(string expression, string value, int rows, int columns, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent TextBox(string expression, object value, string format, object htmlAttributes) => throw new NotImplementedException();

            public IHtmlContent ValidationMessage(string expression, string message, object htmlAttributes, string tag) => throw new NotImplementedException();

            public IHtmlContent ValidationSummary(bool excludePropertyErrors, string message, object htmlAttributes, string tag) => throw new NotImplementedException();

            public string Value(string expression, string format) => throw new NotImplementedException();
        }
    }
}