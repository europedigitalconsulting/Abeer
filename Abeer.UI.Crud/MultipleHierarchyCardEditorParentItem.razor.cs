using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;

namespace Abeer.UI.Crud
{
    public partial class MultipleHierarchyCardEditorParentItem<TItem, TParent>
        where TParent : class, new()
        where TItem : class, new()
    {
        [Parameter]
        public string Label { get; set; }

        [Parameter]
        public Func<bool> IsActive { get; set; }

        [Parameter]
        public string Value { get; set; }

        [Parameter]
        public int TotalItemCount { get; set; }

        [CascadingParameter]
        public MultipleHierarchyCardEditor<TItem> Parent { get; set; }

        public  MultipleHierarchyCardEditorInfo Info { get; private set; }

        [Parameter]
        public Func<int, int, CancellationToken, Task<IEnumerable<TParent>>> GetItems { get; set; }

        [Parameter]
        public RenderFragment<TParent> Template { get; set; }

        [Parameter]
        public RenderFragment EmptyTemplate { get; set; }
        
        protected override Task OnInitializedAsync()
        {
            Info = new MultipleHierarchyCardEditorInfo
            {
                Label = Label
            };

            Console.WriteLine($"{Label} Initialized");
            
            Parent.AddItem(Info);
            return base.OnInitializedAsync();
        }

        private async ValueTask<ItemsProviderResult<TParent>> LoadItems(ItemsProviderRequest request)
        {
            var numItem = Math.Min(request.Count, TotalItemCount - request.StartIndex);
            var items = await GetItems(request.StartIndex, numItem, request.CancellationToken);
            return new ItemsProviderResult<TParent>(items, TotalItemCount);
        }

        public void SetCount(int count)
        {
            TotalItemCount = count;
            StateHasChanged();
        }
    }
}
