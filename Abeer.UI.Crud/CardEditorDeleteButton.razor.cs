using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Abeer.UI.Crud
{
    public partial class CardEditorDeleteButton<TItem> : ComponentBase where TItem : class, new()
    {
        [Parameter]
        public CardEditor<TItem> CardEditor { get; set; }

        [Parameter]
        public bool IsDeletable { get; set; }

        [Parameter]
        public TItem Item { get; set; }

        void DeleteButtonClicked(MouseEventArgs mouseEventArgs)
        {
            CardEditor?.ShowDelete(Item);
        }
    }
}
