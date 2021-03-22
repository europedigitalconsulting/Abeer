using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.EntityFrameworkCore.Internal;

namespace Abeer.UI.Crud
{
    public partial class MultipleHierarchyCardEditor<TItem> : ComponentBase
    where TItem : class, new()
    {
        [Parameter]
        public IStringLocalizer StringLocalizer { get; set; }

        [Parameter]
        public string HomeUrl { get; set; }

        public List<MultipleHierarchyCardEditorInfo> ParentTypes { get; set; } = new List<MultipleHierarchyCardEditorInfo>();

        [Parameter]
        public RenderFragment ChildContent { get; set; }

        [Parameter]
        public Func<bool> IsItemDisplayed { get; set; }

        [Parameter]
        public RenderFragment<TItem> ItemTemplate { get; set; }

        [Parameter]
        public RenderFragment<TItem> FormTemplate { get; set; }

        [Parameter]
        public RenderFragment NotFoundItemTemplate { get; set; }
        [Parameter]
        public RenderFragment BreadCrumbTemplate { get; set; }

        [Parameter]
        public string LabelItem { get; set; }

        [Parameter]
        public string ModalDialogSize { get; set; }

        public int Count => Items?.Count ?? 0;
        public List<TItem> Items { get; set; }

        private string TitleDialog { get; set; }
        private string IconDialog { get; set; }
        private string DialogAction { get; set; }

        public TItem Item { get; set; }

        public string Mode { get; set; }

        [Parameter]
        public Action<TItem> PreInsert { get; set; }
        [Parameter]
        public Action<TItem> PreUpdate { get; set; }
        [Parameter]
        public Action<TItem> PreDelete { get; set; }
        [Parameter]
        public EventCallback<TItem> Insert { get; set; }
        [Parameter]
        public EventCallback<TItem> Update { get; set; }

        [Parameter]
        public EventCallback<TItem> Delete { get; set; }

        public async Task AddItem()
        {
            Item = new TItem();
            _isModalVisible = true;

            if (PreInsert != null)
                PreInsert(Item);

            Mode = "Insert";
            TitleDialog = StringLocalizer["Insert"];
            DialogAction = StringLocalizer["Save"];
            IconDialog = "fas fa-plus-square";
            await InvokeAsync(StateHasChanged);
        }

        public async Task OpenEdit(TItem item)
        {
            Item = item;
            _isModalVisible = true;

            if (PreUpdate != null)
                PreUpdate(Item);

            Mode = "Update";
            TitleDialog = StringLocalizer["Update"];
            DialogAction = StringLocalizer["Save"];
            IconDialog = "fas fa-edit";

            await InvokeAsync(StateHasChanged);
        }

        public async Task OpenDelete(TItem item)
        {
            Item = item;
            _isModalVisible = true;

            if (PreDelete != null)
                PreDelete(Item);

            Mode = "Delete";
            TitleDialog = StringLocalizer["Update"];
            DialogAction = StringLocalizer["Save"];
            IconDialog = "fas fa-trash";

            await InvokeAsync(StateHasChanged);
        }

        private MultipleHierarchyCardEditorInfo MultipleHierarchyCardEditorInfo
            => ParentTypes?.FirstOrDefault(p => string.IsNullOrEmpty(p.Value));

        private bool _isModalVisible;

        public void AddItem(MultipleHierarchyCardEditorInfo multipleHierarchyCardEditorInfo)
        {
            Console.WriteLine($"Add parent type {multipleHierarchyCardEditorInfo.Label}");

            ParentTypes.Add(new MultipleHierarchyCardEditorInfo
            {
                Label = multipleHierarchyCardEditorInfo.Label
            });
        }

        private Virtualize<TItem> virtualizeData;

        private async Task Save()
        {
            switch (Mode)
            {
                case "Insert":
                    {
                        await Insert.InvokeAsync(Item);
                        await virtualizeData.RefreshDataAsync();
                        _isModalVisible = false;
                        await InvokeAsync(StateHasChanged);
                        break;
                    }
                case "Update":
                    {
                        await Update.InvokeAsync(Item);
                        await virtualizeData.RefreshDataAsync();
                        _isModalVisible = false;
                        await InvokeAsync(StateHasChanged);
                        break;
                    }
                case "Delete":
                    {

                        await Delete.InvokeAsync(Item);
                        Items.Remove(Item);
                        await virtualizeData.RefreshDataAsync();
                        _isModalVisible = false;
                        await InvokeAsync(StateHasChanged);
                        break;
                    }
            }
        }

        public async Task Refresh()
        {
            await virtualizeData.RefreshDataAsync();
        }
    }
}
