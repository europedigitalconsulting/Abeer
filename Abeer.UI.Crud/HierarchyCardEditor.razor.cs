using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Abeer.UI.Crud
{
    public partial class HierarchyCardEditor<TItem, TParent> : ComponentBase 
        where TItem : class, new()
        where TParent : class
    {
        private bool _isModalVisible = false;
        private string _mode = "Insert";
        private ImportModal _importModal;

        public TItem Item { get; set; } = new TItem();

        [Parameter]
        public IStringLocalizer StringLocalizer { get; set; }

        [Parameter]
        public  IList<TParent> Parents { get; set; }
        [Parameter]
        public TParent Parent { get; set; }
        [Parameter]
        public IList<TItem> Items { get; set; }

        [Parameter]
        public string ModalDialogSize { get; set; }
        [Parameter]
        public EventCallback<TItem> Delete { get; set; }
        [Parameter]
        public EventCallback<TItem> Insert { get; set; }
        [Parameter]
        public EventCallback<TItem> Update { get; set; }
        [Parameter]
        public EventCallback<TParent> ParentSelected { get; set; }
        [Parameter]
        public EventCallback<TItem> PreDelete { get; set; }
        [Parameter]
        public EventCallback<TItem> PreInsert { get; set; }
        [Parameter]
        public EventCallback<TItem> PreUpdate { get; set; }


        [Parameter]
        public EventCallback<string> SearchButtonClick { get; set; }
        [Parameter]
        public RenderFragment<TItem> FormTemplate { get; set; }
        [Parameter]
        public RenderFragment<TItem> RepeaterItemTemplate { get; set; }
        [Parameter]
        public RenderFragment<TParent> RepeaterParentItemTemplate { get; set; }
        [Parameter]
        public RenderFragment<TItem> CustomButtonsPart { get; set; }
        [Parameter]
        public RenderFragment EmptyTemplate { get; set; }

        [Parameter]
        public Func<bool> IsAddedAllowed { get; set; }

        [Parameter]
        public Func<bool> IsEditAllowed { get; set; }

        [Parameter]
        public Func<bool> IsDeleteAllowed { get; set; }

        [Parameter]
        public bool IsImportEnabled { get; set; }

        [Parameter]
        public string ImportPostFileUrl { get; set; }

        [Parameter]
        public EventCallback GotoHome { get; set; }
        [Parameter]
        public string HomeUrl { get; set; }

        public string ParentLabel { get; set; }
        public string ParentUrl { get; set; }

        string TitleDialog { get; set; }

        string IconDialog { get; set; }
        string DialogAction { get; set; }


        string SearchTerm { get; set; }

        public async Task ShowInsert()
        {
            Item = new TItem();
            _mode = "Insert";
            _isModalVisible = true;
            TitleDialog = StringLocalizer["InsertTitle"];
            IconDialog = "fas fa-plus-square";
            DialogAction = StringLocalizer["Insert"];

            await PreInsert.InvokeAsync(Item);

            StateHasChanged();
        }

        public async Task ShowImport()
        {
            await _importModal.ShowDialog();
        }

        public async Task ShowEdit(TItem item)
        {
            Item = item;
            await PreUpdate.InvokeAsync(item);
            _mode = "Update";
            _isModalVisible = true;
            TitleDialog = StringLocalizer["EditTitle"];
            IconDialog = "fas fa-edit";
            DialogAction = StringLocalizer["Edit"];
            StateHasChanged();
        }

        public void CloseDialog()
        {
            _isModalVisible = false;
        }

        public async Task StateHasChange()
        {
            await InvokeAsync(StateHasChange);
        }

        public async  Task ShowDelete(TItem item)
        {
            Item = item;
            _mode = "Delete";
            _isModalVisible = true;
            TitleDialog = StringLocalizer["DeleteTitle"];
            IconDialog = "fas fa-trash";
            DialogAction = StringLocalizer["Delete"];
            await PreDelete.InvokeAsync(item);
            StateHasChanged();
        }

        async Task Save()
        {
            _isModalVisible = false;
            await Run();
            await InvokeAsync(StateHasChange);
        }

        Task Run()
        {
            switch (_mode)
            {
                case "Insert": return Insert.InvokeAsync(Item);
                case "Update": return Update.InvokeAsync(Item);
                case "Delete": return Delete.InvokeAsync(Item);
            }

            return Task.CompletedTask;
        }

        async Task Cancel()
        {
            _isModalVisible = false;
        }

        public async Task SelectParent(TParent parent)
        {
            Parent = parent;

            if(ParentSelected.HasDelegate)
                await ParentSelected.InvokeAsync(parent);
        }
    }
}
