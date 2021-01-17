using Microsoft.AspNetCore.Components;

namespace Abeer.Client.UISdk.Shared
{
    public interface ICollapsePannelItem
    {
        int Index { get; set; }
        string Title { get; set; }
        RenderFragment ChildContent { get; }
    }
}