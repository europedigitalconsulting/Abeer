using Microsoft.AspNetCore.Components;

using System.Collections.Generic;
using System.Threading.Tasks;

namespace CryptoCoin.UI.Components.CollapsePanel.lib
{
    public partial class CollapsePannel: ComponentBase
    {
        [Parameter] public RenderFragment ChildContent { get; set; }
        internal List<CollapsePannelItem> Items { get; set; } = new List<CollapsePannelItem>();
        internal CollapsePannelItem ActivePannel { get; set; }
        internal int AddPannel(CollapsePannelItem collapsePannelItem)
        {
            collapsePannelItem.Index = Items.Count;
            Items.Add(collapsePannelItem);
            return collapsePannelItem.Index;
        }
        internal void RemovePannel(CollapsePannelItem collapsePannelItem)
        {
            Items.Remove(collapsePannelItem);
        }

        internal void SetActivateTab(CollapsePannelItem collapsePannelItem)
        {
            ActivePannel = collapsePannelItem;
        }
    }
}
