using Microsoft.AspNetCore.Components;

namespace Abeer.Client.Shared
{
    public interface ICollapsePannelItem
    {
        int Index { get; set; }
        string Title { get; set; }
        RenderFragment ChildContent { get; }
    }
}