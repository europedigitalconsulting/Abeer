using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Shared.Functional;
using Microsoft.AspNetCore.Components;

namespace Abeer.UI_Tchat
{
    public partial class Tchat : ComponentBase
    {
        [Parameter] public EventCallback ModalContactChat { get; set; }
        private bool ModalChat { get; set; }
        protected override async Task OnInitializedAsync()
        {
            Console.WriteLine("ok");
            await base.OnInitializedAsync();
        }
        protected async Task OpenChat()
        {
            ModalChat = true;
            StateHasChanged();
        }
        protected async Task CloseChat()
        {
            ModalChat = false;
            StateHasChanged();
        }
        protected async Task CloseContact()
        {
            await ModalContactChat.InvokeAsync();
        }
    }
}
