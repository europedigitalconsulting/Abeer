using System.Security.Claims;

namespace Abeer.Shared.Security
{
    public static class ClaimNames
    {
        public static string Title => "title";
        public static string Address => "address";
        public static string City => "city";
        public static string Country => "country";
        public static string DisplayDescription = "displaydescription";
        public static string DescriptionVideo => "descriptionVideo";
        public static string DescriptionVideoCover => "descriptionVideoCover";
        public static string VideoProfileUrl => "videoProfileUrl";
        public static string VideProfileCoverUrl => "videProfileCoverUrl";
        public static string Description => "description";
        public static string DigitCode => "pinDigit";
        public static string DisplayName => "displayName";
        public static string Email => ClaimTypes.Email;
        public static string FirstName => ClaimTypes.GivenName;
        public static string Id => ClaimTypes.NameIdentifier;
        public static string PhoneNumber => ClaimTypes.HomePhone;
        public static string IsUnlimited => "isunlimited";

        public static string IsAdmin => "admin";
        public static string IsManager => "manager";
        public static string IsOnline => "isonline";
        public static string IsOperator => "operator";
        public static string LastLogin => "lastlogin";
        public static string LastName => ClaimTypes.Surname;
        public static string NumberOfView = "numberOfView";
        public static string PhotoUrl => "photoUrl";
        public static string IsReadOnly => "isreadonly";
        public static string PinCode => "pinCode";
        public static string SubscriptionStart => "subscriptionStart";
        public static string SubscriptionEnd => "subscriptionEnd";
        public static string Subscription => "subscription";
        public static string Ultimate => "ultimate";
        public static string IsPayable => "isPayable";
        public static string IsFree => "isfree";
    }
}
