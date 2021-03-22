using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI.Crud
{
    public partial class CardEditorEditButton<TItem> : ComponentBase where TItem : class, new()
    {
        [Parameter]
        public CardEditor<TItem> CardEditor { get; set; }

        [Parameter]
        public bool IsEditable { get; set; }

        [Parameter]
        public TItem Item { get; set; }

        void EditButtonClicked(MouseEventArgs mouseEventArgs)
        {
            CardEditor?.ShowEdit(Item);
        }
    }
}