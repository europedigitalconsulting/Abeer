using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Threading.Tasks;
using Abeer.Shared;
using Abeer.Shared.ClientHub;
using Abeer.Shared.EventNotification;
using Abeer.Shared.Functional;
using Abeer.Shared.StateContainer;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Newtonsoft.Json;

namespace Abeer.UI_Tchat
{
    public partial class Tchat : ComponentBase
    {
        [CascadingParameter] private Task<AuthenticationState> authenticationStateTask { get; set; }
        [Parameter] public NotificationClient NotifyClient { get; set; }
        [Inject] private HttpClient httpClient { get; set; }
        [Inject] private StateTchatContainer StateTchatContainer { get; set; }
        private bool ModalChat { get; set; }
        private string MsgText { get; set; }
        private ClaimsPrincipal User;
        [Inject] private IJSRuntime jsRuntime { get; set; }

        protected override async Task OnInitializedAsync()
        {
            var authenticateSate = await authenticationStateTask;

            User = authenticateSate.User; 

            await base.OnInitializedAsync();
        }
        protected async Task OpenChat(ViewContact item)
        {
            var response = await httpClient.GetAsync($"/api/Tchat/{item.UserId}");
            response.EnsureSuccessStatusCode();
            StateTchatContainer.ListMessage.Clear();
            var json = await response.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(json))
                StateTchatContainer.ListMessage.AddRange(JsonConvert.DeserializeObject<List<Message>>(json));

            ModalChat = true;
            StateTchatContainer.ContactSelected = item;
            var tmp = StateTchatContainer.MyContacts.FirstOrDefault(x => x.UserId == item.UserId);
            tmp.HasNewMsg = false;
             
            StateTchatContainer.SetModalTchat(true);

            await ScrollToElementId("msg_card_body");
        }
        protected async Task SendMessage()
        {
            if (!string.IsNullOrEmpty(MsgText))
            {
                TchatViewModel vm = new TchatViewModel();
                vm.Text = MsgText;
                vm.ContactId = StateTchatContainer.ContactSelected.UserId;

                var response = await httpClient.PostAsJsonAsync("/api/Tchat", vm);
                response.EnsureSuccessStatusCode();

                var json = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrEmpty(json))
                {
                    var msg = JsonConvert.DeserializeObject<Message>(json);
                    StateTchatContainer.ListMessage.Add(msg);
                    await NotifyClient.SendMessage(MsgText, StateTchatContainer.ContactSelected.UserId);
                    MsgText = string.Empty;
                    StateHasChanged();
                }
            }
        } 
        protected async Task CloseModalTchat()
        {
            StateTchatContainer.SetModalTchat(false);
            MsgText = string.Empty;
            await InvokeAsync(StateHasChanged);
        }
        protected async Task CloseContact()
        {
            StateTchatContainer.SetModalChatContactOpen(!StateTchatContainer.ModalChatContactOpen);
            //StateTchatContainer.SetHasMessage(StateTchatContainer.MyContacts.Any(x => x.HasNewMsg));

            await NotifyClient.CloseModalContactTchat();
        }
        private async Task ScrollToElementId(string elementId)
        {
            await jsRuntime.InvokeVoidAsync("scrollToElementId", elementId);
        }
    }
}
