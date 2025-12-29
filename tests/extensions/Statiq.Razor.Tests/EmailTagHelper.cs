using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Statiq.Razor.Tests
{
    public class EmailTagHelper : TagHelper
    {
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "a";    // Replaces <email> with <a> tag
            string address = (await output.GetChildContentAsync()).GetContent() + "@foo.com";
            output.Attributes.SetAttribute("href", "mailto:" + address);
            output.Content.SetContent(address);
        }
    }
}