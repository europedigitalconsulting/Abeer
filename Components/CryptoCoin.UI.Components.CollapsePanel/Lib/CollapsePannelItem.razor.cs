using Microsoft.AspNetCore.Components;

using System;
using System.Collections.Generic;
using System.Text;

namespace CryptoCoin.UI.Components.CollapsePanel.lib
{
    public partial class CollapsePannelItem : ComponentBase, ICollapsePannelItem, IDisposable
    {
        public int Index { get; set; }

        [Parameter]
        public string Title { get; set; }

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [CascadingParameter]
        private CollapsePannel Parent { get; set; }

        protected bool Visibled;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            if (Parent == null)
                throw new ArgumentNullException(nameof(Parent), "CollapsePannelItem must exist within a CollapsePannel");

            var index = Parent.AddPannel(this);
            Index = index;

            if (Parent.ActivePannel == null)
                Parent.ActivePannel = this;
        }

        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            Visibled = Parent.ActivePannel?.Index.Equals(this.Index) == true;
            Console.WriteLine($"{Index} : {Visibled}");
        }

        public void Dispose()
        {
            Parent.RemovePannel(this);
        }

        internal void Activate()
        {
            Parent.SetActivateTab(this);
            Visibled = true;
        }

    }
}
