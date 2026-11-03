using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetNinja.Garage.TagHelpers;

[HtmlTargetElement("copyright")]
public class CopyrightTagHelper: TagHelper
{
    public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        // get contents of the tag helper and set it as the content of the output
        var content = await output.GetChildContentAsync();
        var html = content.GetContent();
        output.Content.SetHtmlContent($"&copy; Copyright {DateTime.Now.Year} {html}");
        await base.ProcessAsync(context, output);
    }
}