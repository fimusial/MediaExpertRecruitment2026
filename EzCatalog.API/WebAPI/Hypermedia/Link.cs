using System.ComponentModel;

namespace EzCatalog.WebAPI.Hypermedia;

[Description("A hypermedia link to a related resource or an action that can be performed on it.")]
public record Link(
    [property: Description("Absolute URI of the target resource.")] string Href,
    [property: Description("HTTP method to use when following the link.")] string Method)
{
}
