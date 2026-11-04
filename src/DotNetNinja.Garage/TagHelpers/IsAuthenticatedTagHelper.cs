using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace DotNetNinja.Garage.TagHelpers;

[HtmlTargetElement("*", Attributes = "is-authenticated")]
public class IsAuthenticatedTagHelper : TagHelper
{
    [HtmlAttributeName("is-authenticated")]
    public bool IsAuthenticated { get; set; } = true;

    [ViewContext]
    [HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext.HttpContext.User.Identity?.IsAuthenticated != IsAuthenticated)
        {
            output.SuppressOutput();
        }
    }
}