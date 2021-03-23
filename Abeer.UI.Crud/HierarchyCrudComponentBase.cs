using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Shared.Technical;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Newtonsoft.Json;

namespace Abeer.UI.Crud
{
    public abstract class HierarchyCrudComponentBase<TItem, TParent> : ComponentBase
        where TItem : class, new()
        where TParent : class
    {
        public HierarchyCardEditor<TItem, TParent> CardEditor { get; set; }

        protected IList<TItem> Items { get; set; }
        protected IList<TItem> Data { get; set; }
        protected IList<TParent> Parents { get; set; }

        protected ClaimsPrincipal User;

        [Inject] public HttpClient HttpClient { get; set; }

        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected abstract Func<TParent, string> ApiGetUrl { get; }
        protected  abstract  string ApiBaseUrl { get; }
        protected  abstract  string ParentApiUrl { get;  }
        protected  abstract  string ParentUrl { get; }
        protected virtual bool ShowHierarchy { get => true; }

        protected virtual void LoadInit()
        {

        }

        protected virtual void PreInsert(TItem item) { }
        protected virtual void PreUpdate(TItem item) { }
        protected virtual void PreDelete(TItem item) { }

        protected abstract Guid GetId(TItem item);
        protected abstract bool IsEquals(TItem i, TItem m);
        protected abstract bool IsAllowed(ClaimsPrincipal user, string rule);
        protected virtual void SetParent(TItem item)
        {

        }

        protected virtual string ImportPostFileUrl => $"{ApiBaseUrl}/import";
        protected virtual Func<TParent, string> ParentDisplayText { get; set; }
        protected TParent Parent { get; set; }
        
        protected virtual IList<TItem> FilterHierarchy()
        {
            return Data;
        }

        protected override async Task OnInitializedAsync()
        {
            var authenticateSate = await AuthenticationStateTask;
            User = authenticateSate.User;
            
            var getResponse = await HttpClient.GetAsync(ParentApiUrl);
            getResponse.EnsureSuccessStatusCode();

            var json = await getResponse.Content.ReadAsStringAsync();
            Console.WriteLine($"get parent {json}");

            Parents = JsonConvert.DeserializeObject<List<TParent>>(json);

            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }

        protected async Task Insert(TItem item)
        {
            if (Parent != null)
                SetParent(item);

            PreInsert(item);

            var postResponse = await HttpClient.PostAsJsonAsync(ApiBaseUrl, item);
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            CardEditor.CloseDialog();

            Console.WriteLine(json);

            var inserted = JsonConvert.DeserializeObject<TItem>(json);

            Data.Add(inserted);
            Items.Add(inserted);
            
            await InvokeAsync(StateHasChanged);
        }

        protected async Task Update(TItem item)
        {

            PreUpdate(item);

            var putResponse = await HttpClient.PutAsJsonAsync($"{ApiBaseUrl}/{GetId(item)}", item);
            putResponse.EnsureSuccessStatusCode();
            //Console.WriteLine("get put response successfully");

            var json = await putResponse.Content.ReadAsStringAsync();
            var updated = JsonConvert.DeserializeObject<TItem>(json);
            //Console.WriteLine("deserialized successfully");

            Data[Data.FindIndex(i => IsEquals(item, i))] = updated;
            Items[Items.FindIndex(i => IsEquals(item, i))] = updated;

            //Console.WriteLine("updated locally successfully");

            CardEditor.CloseDialog();

            await InvokeAsync(StateHasChanged);
            //Console.WriteLine("updated successfully");
        }

        protected async Task Delete(TItem item)
        {
            var deleteResponse = await HttpClient.DeleteAsync($"{ApiBaseUrl}/{GetId(item)}");

            deleteResponse.EnsureSuccessStatusCode();
            //Console.WriteLine("get delete response successfully");

            Data.Remove(item);
            Items.Remove(item);

            //Console.WriteLine("deleted locally successfully");

            CardEditor.CloseDialog();

            await InvokeAsync(StateHasChanged);
            //Console.WriteLine("delete successfully");
        }

        protected async Task Search()
        {
            Filter();
            await InvokeAsync(StateHasChanged);
        }

        private void Filter()
        {
            if (string.IsNullOrEmpty(SearchTerm))
            {
                Items = Data.ToList();
            }
            else
            {
                if (Parent != null)
                {
                    Items = FilterHierarchy()?.Where(m => IsMatch(m)).ToList();
                }

                else
                    Items = Data.Where(m => IsMatch(m)).ToList();
            }
        }

        protected abstract bool IsMatch(TItem m);

        protected string SearchTerm { get; set; }

        protected async Task ParentSelected(TParent parent)
        {
            Parent = parent;
            CardEditor.ParentLabel = ParentDisplayText(parent);
            CardEditor.ParentUrl = ParentUrl;

            var url = ApiGetUrl(parent);

            Console.WriteLine($"get url {url}");
            var getResponse = await HttpClient.GetAsync(url);
            getResponse.EnsureSuccessStatusCode();

            var json = await getResponse.Content.ReadAsStringAsync();
            Data = JsonConvert.DeserializeObject<List<TItem>>(json);

            Filter();
            CardEditor.Items = Items;
        }

        public bool IsAddedAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Insert");
        public bool IsEditAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Edit");
        public bool IsDeleteAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Delete");
    }
}