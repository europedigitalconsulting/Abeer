
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

namespace Abeer.Client.Pages
{
    public partial class Users : ComponentBase
    {
        bool showModal = false;
        ApplicationUser current = new ApplicationUser();
        string Mode = "Create";
        bool ModalShareProfileVisible = false;

        public UserForm UserForm { get; set; }
        public string TitleDialog { get; set; }
        public List<ApplicationUser> Data { get; set; } = new List<ApplicationUser>();
        protected string SearchTerm { get; set; }
        protected List<ApplicationUser> Items = new List<ApplicationUser>();

        public int NbOfUsers { get; set; }
        public int NbOfUsersOnline { get; set; }

        [Inject]
        public HttpClient HttpClient { get; set; }

        [Inject]private NavigationManager navigationManager { get; set; }

        [CascadingParameter]
        private Task<AuthenticationState> authenticationStateTask { get; set; }

        protected override async Task OnParametersSetAsync()
        {
            var authenticateSate = await authenticationStateTask;
            
            Console.WriteLine("User claims");

            if (!authenticateSate.User.Identity.IsAuthenticated || !authenticateSate.User.HasClaim(ClaimTypes.Role, "admin"))
            {
                Console.WriteLine("User is not admin, redirect to login");
                navigationManager.NavigateTo("/authentication/Login", true);
            }

            var response = await HttpClient.GetAsync("/api/Users", HttpCompletionOption.ResponseContentRead);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();

            Data = JsonConvert.DeserializeObject<List<ApplicationUser>>(json);
            Items = Data.ToList();
            
            NbOfUsers = Data.Count;
            NbOfUsersOnline = Data.Count(u => u.IsOnline);

            await base.OnParametersSetAsync();

        }
        public void SearchButtonClick()
        {
            Console.WriteLine($"start search User {SearchTerm}");
            var found = Data?.Where(c => c.DisplayName?.Contains(SearchTerm) == true || c.Email?.Contains(SearchTerm) == true)?.ToList();
            Items = found;
            Console.WriteLine($"{found.Count} Users found");
            StateHasChanged();
        }

        async Task ToggleModalCreateUser()
        {
            showModal = !showModal;
        }

        async Task ToggleModalShareProfile()
        {
            ModalShareProfileVisible = !ModalShareProfileVisible;
        }

        static Random rdm = new Random();

        async Task ShowInsertUser()
        {
            current = new ApplicationUser();
            current.PinCode = rdm.Next(10000, 99999);
            Mode = "Create";
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        async Task ShowEditUser(ApplicationUser User)
        {
            current = User;
            Mode = "Update";
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        async Task ShowDeleteUser(ApplicationUser User)
        {
            current = User;
            Mode = "Delete";
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        async Task Insert()
        {
            var response = await HttpClient.PostAsJsonAsync<ApplicationUser>("api/Users", current);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"insert result : {json}");
            var User = JsonConvert.DeserializeObject<ApplicationUser>(json);
            Data.Add(User);
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        async Task Update()
        {
            var response = await HttpClient.PutAsJsonAsync<ApplicationUser>($"api/Users/{current.Id}", current);
            response.EnsureSuccessStatusCode();
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        async Task ValidateUser(ApplicationUser user)
        {
            var response = await HttpClient.PutAsJsonAsync<ApplicationUser>($"api/Users/Validate/{user.Id}", user);
            response.EnsureSuccessStatusCode();
            await InvokeAsync(StateHasChanged);
        }

        async Task LockUser(ApplicationUser user)
        {
            var response = await HttpClient.PutAsJsonAsync<ApplicationUser>($"api/Users/Lock/{user.Id}", user);
            response.EnsureSuccessStatusCode();
            await InvokeAsync(StateHasChanged);
        }

        async Task UnLockUser(ApplicationUser user)
        {
            var response = await HttpClient.PutAsJsonAsync<ApplicationUser>($"api/Users/UnLock/{user.Id}", user);
            response.EnsureSuccessStatusCode();
            await InvokeAsync(StateHasChanged);
        }

        async Task Delete()
        {
            var response = await HttpClient.DeleteAsync($"api/Users/{current.Id}");
            response.EnsureSuccessStatusCode();
            Data.Remove(current);
            Items.Remove(current);
            await ToggleModalCreateUser().ConfigureAwait(false);
            await InvokeAsync(StateHasChanged);
        }

        public string ProfileUrl { get; private set; }

        Task ShareProfile(ApplicationUser user)
        {
            ProfileUrl = navigationManager.ToAbsoluteUri($"/viewProfile/{user.PinCode}").ToString();
            return ToggleModalShareProfile();
        }

        Task SaveForm()
        {
            return Mode switch
            {
                "Insert" => Insert(),
                "Update" => Update(),
                "Delete" => Delete(),
                _ => throw new NotImplementedException(),
            };
        }
    }
}