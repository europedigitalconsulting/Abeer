using Microsoft.AspNetCore.Components;

namespace CryptoCoin.UI.Components.CollapsePanel.lib
{
    public interface ICollapsePannelItem
    {
        int Index { get; set; }
        string Title { get; set; }
        RenderFragment ChildContent { get; }
    }
}
