using Microsoft.AspNetCore.Components;

namespace Abeer.UI_Ads.Components
{
    public partial class FormAd
    {
        [Parameter]
        public Microsoft.Extensions.Localization.IStringLocalizer Loc { get; set; }
        [Parameter]
        public Abeer.Shared.Functional.AdModel Ad { get; set; }
        [Parameter]
        public bool FormHasError { get; set; }
        [Parameter]
        public string FormError { get; set; }
        [Parameter]
        public bool Disabled { get; set; }

        private void AssignImageUrl1(string imgUrl) => Ad.ImageUrl1 = imgUrl;
        private void AssignImageUrl2(string imgUrl) => Ad.ImageUrl2 = imgUrl;
        private void AssignImageUrl3(string imgUrl) => Ad.ImageUrl3 = imgUrl;
        private void AssignImageUrl4(string imgUrl) => Ad.ImageUrl4 = imgUrl;
    }
}
