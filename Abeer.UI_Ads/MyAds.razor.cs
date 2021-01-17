using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Abeer.UI_Ads
{
    public partial class MyAds
    {
        private List<Abeer.Shared.Functional.AdModel> Ads = new List<Abeer.Shared.Functional.AdModel>();
        private bool ModalEditAdVisible;
        private Abeer.Shared.Functional.AdModel Current = new Abeer.Shared.Functional.AdModel();
        private bool UpdateHasError;
        private bool ModalDeleteAdVisible;
        private bool DeleteHasError;

        protected override async Task OnInitializedAsync()
        {
            var getAds = await HttpClient.GetAsync("/api/Ads");
            getAds.EnsureSuccessStatusCode();
            var json = await getAds.Content.ReadAsStringAsync();
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdModel>>(json);
            await base.OnParametersSetAsync();
        }

        private void OpenEditModal(Abeer.Shared.Functional.AdModel ad)
        {
            Current = ad;
            ModalEditAdVisible = true;
        }

        private void OpenDeleteModal(Abeer.Shared.Functional.AdModel adModel)
        {
            Current = adModel;
            ModalDeleteAdVisible = true;
        }

        private async Task Update()
        {
            var update = await HttpClient.PutAsJsonAsync<Abeer.Shared.Functional.AdModel>("/api/Ads", Current);
            update.EnsureSuccessStatusCode();
            ModalEditAdVisible = false;
            await InvokeAsync(StateHasChanged);
        }

        private async Task Delete()
        {
            var update = await HttpClient.DeleteAsync($"/api/Ads/{Current.Id}");
            update.EnsureSuccessStatusCode();
            ModalDeleteAdVisible = false;
            Ads.Remove(Current);
            await InvokeAsync(StateHasChanged);
        }

        private async Task ViewNotValid()
        {
            var getAds = await HttpClient.GetAsync("/api/Ads/notvalid");
            getAds.EnsureSuccessStatusCode();
            var json = await getAds.Content.ReadAsStringAsync();
            Ads = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Abeer.Shared.Functional.AdModel>>(json);
            await InvokeAsync(StateHasChanged);
        }

        private async Task GotoAll()
        {
            NavigationManager.NavigateTo(NavigationManager.ToAbsoluteUri("ads/list").ToString(), true);
            await InvokeAsync(StateHasChanged);
        }
    }
}
