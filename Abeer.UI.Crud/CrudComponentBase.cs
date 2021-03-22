using Abeer.Shared;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Shared.Technical;

namespace Abeer.UI.Crud
{
    public abstract class CrudComponentBase<TItem> : ComponentBase
        where TItem:class, new()
    {
        public CardEditor<TItem> CardEditor { get; set; }
        
        protected IList<TItem> Items;
        protected IList<TItem> Data;
        protected ClaimsPrincipal User;

        [Inject] public HttpClient HttpClient { get; set; }

        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

        protected abstract string ApiUrl { get; }
        protected virtual bool ShowHierarchy { get => false; }

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

        protected virtual string ImportPostFileUrl => $"{ApiUrl}/import";
        protected  virtual Func<TItem, string> ParentDisplayText { get; set; }
        protected TItem Parent { get; set; }

        protected virtual IList<TItem> FilterHierarchy()
        {
            return Data;
        }

        protected override async Task OnInitializedAsync()
        {
            var authenticateSate = await AuthenticationStateTask;
            User = authenticateSate.User;
            var getResponse = await HttpClient.GetAsync(ApiUrl);
            getResponse.EnsureSuccessStatusCode();
            var json = await getResponse.Content.ReadAsStringAsync();
            Console.Write($"get api {ApiUrl} result {json}");
            Data = JsonConvert.DeserializeObject<List<TItem>>(json);

            Items = ShowHierarchy switch
            {
                true => FilterHierarchy(),
                _ => Data.ToList()
            };

            await InvokeAsync(StateHasChanged);
            await base.OnInitializedAsync();
        }

        protected async Task Insert(TItem item)
        {
            if (Parent != null)
                SetParent(item);

            PreInsert(item);

            var postResponse = await HttpClient.PostAsJsonAsync(ApiUrl, item);
            postResponse.EnsureSuccessStatusCode();
            var json = await postResponse.Content.ReadAsStringAsync();
            CardEditor.CloseDialog();

            Console.WriteLine(json);

            var inserted = JsonConvert.DeserializeObject<TItem>(json);
            //Console.WriteLine("deserialized successfully");

            Data.Add(inserted);
            Items.Add(inserted);
            await InvokeAsync(StateHasChanged);

            //Console.WriteLine("added successfully");
        }

        protected async Task Update(TItem item)
        {

            PreUpdate(item);

            var putResponse = await HttpClient.PutAsJsonAsync($"{ApiUrl}/{GetId(item)}", item);
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
            var deleteResponse = await HttpClient.DeleteAsync($"{ApiUrl}/{GetId(item)}");

            deleteResponse.EnsureSuccessStatusCode();
            //Console.WriteLine("get delete response successfully");

            Data.Remove(item);
            Items.Remove(item);

            //Console.WriteLine("deleted locally successfully");

            CardEditor.CloseDialog();

            await InvokeAsync(StateHasChanged);
            //Console.WriteLine("delete successfully");
        }

        protected async Task Search(string searchTerm)
        {
            SearchTerm = searchTerm; 
            if (string.IsNullOrEmpty(SearchTerm))
            {
                Items = Data.ToList();
            }
            else
            {
                if(Parent != null)
                { 
                    Items = FilterHierarchy()?.Where(m => IsMatch(m)).ToList();
                }

                else
                    Items = Data.Where(m => IsMatch(m)).ToList();
            } 
            await InvokeAsync(StateHasChanged);
        }

        protected abstract bool IsMatch(TItem m);

        protected string SearchTerm { get; set; }


        public bool IsAddedAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Insert");
        public bool IsEditAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Edit");
        public bool IsDeleteAllowed() => User?.Identity.IsAuthenticated == true && IsAllowed(User, "Delete");
    }
}
