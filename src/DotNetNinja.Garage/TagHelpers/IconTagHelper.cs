using System.ComponentModel;
using System.Reflection;
using DotNetNinja.Garage.Constants;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetNinja.Garage.TagHelpers;

[HtmlTargetElement("icon")]
public class IconTagHelper: TagHelper
{
    [HtmlAttributeName("key")]
    public Icons Key { get; set; } = Icons.None;

    [HtmlAttributeName("name")]
    public string Name { get; set; } = string.Empty;

    public override Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
    {
        if (Key != Icons.None)
        {
            var enumName = Key.ToString();
            Name = typeof(Icons)
                .GetField(enumName)?
                .GetCustomAttribute<DescriptionAttribute>()?
                .Description ?? enumName;
        }

        if(string.IsNullOrWhiteSpace(Name))
        {
            // handle the case where Name is still null or whitespace
            output.TagName = null;
            output.SuppressOutput();
        }
        else
        {
            // handle the case where Name is not null or whitespace
            output.TagName = "i";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"bi bi-{Name}");
        }

        return base.ProcessAsync(context, output);
    }
}
