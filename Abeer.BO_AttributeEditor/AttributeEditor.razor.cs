using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using Abeer.AttributeEditor;

namespace Abeer.AttributeEditor
{
    public partial class AttributeEditor
    {
        [Parameter] public List<EditAttributeViewModel> ListAttributeEdit { get; set; }
        public EditAttributeViewModel NewAttributes { get; set; } = new EditAttributeViewModel();

        public void AddAttribute()
        {
            switch (string.IsNullOrEmpty(NewAttributes.Label))
            {
                case false when !string.IsNullOrEmpty(NewAttributes.Type):
                    ListAttributeEdit.Add(NewAttributes);
                    NewAttributes = new EditAttributeViewModel();
                    break;
            }
        }
        private async Task DeleteAttribute(EditAttributeViewModel item)
        {
            ListAttributeEdit.Remove(item);
            await InvokeAsync(StateHasChanged);
        }

    }
}
