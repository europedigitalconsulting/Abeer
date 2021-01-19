using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Abeer.Shared.Functional;

namespace Abeer.Shared.ViewModels
{
    public class ListAdViewModel
    {
        public Guid Id { get; set; }
        public string ImageUrl1 { get; set; }
        public string OrderNumber { get; set; }
        public string Title { get; set; }
    }

    public class AdViewModel
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public string Currency { get; set; }
        public string Url1 { get; set; }
        public string Url2 { get; set; }
        public string Url3 { get; set; }
        public string Url4 { get; set; }
        public string ImageUrl1 { get; set; }
        public string ImageUrl2 { get; set; }
        public string ImageUrl3 { get; set; }
        public string ImageUrl4 { get; set; }
        public string OwnerId { get; set; }
        public int ViewCount { get; set; }
        public string PaymentInformation { get; set; }
        public DateTime StartDisplayTime { get; set; }
        public DateTime? EndDisplayTime { get; set; }
        public bool IsValid { get; set; }
        public DateTime ValidateDate { get; set; }
        public DateTime CreateDate { get; set; }
        public Guid AdPriceId { get; set; }
        public AdPrice AdPrice { get; set; }
        public string Country { get; set; }
        public  ViewApplicationUser Owner { get; set; }
        public  List<ListAdViewModel> OtherAds { get; set; }

        public static implicit operator AdViewModel(AdModel model)
        {
            return new AdViewModel
            {
                Currency = model.Currency,
                AdPrice = model.AdPrice,
                AdPriceId = model.AdPriceId,
                Country = model.Country,
                CreateDate = model.CreateDate,
                Description = model.Description,
                EndDisplayTime = model.EndDisplayTime,
                Id = model.Id,
                Title = model.Title,
                Price = model.Price,
                IsValid = model.IsValid,
                ImageUrl1 = model.ImageUrl1,
                ImageUrl2 = model.ImageUrl2,
                ImageUrl3 = model.ImageUrl3,
                ImageUrl4 = model.ImageUrl4,
                OwnerId = model.OwnerId,
                PaymentInformation = model.PaymentInformation,
                Url1 = model.Url1,
                Url2 = model.Url2,
                Url3 = model.Url3,
                Url4 = model.Url4,
                ViewCount = model.ViewCount,
                StartDisplayTime = model.StartDisplayTime,
                OrderNumber = model.OrderNumber,
                ValidateDate = model.ValidateDate
            };
        }

        public static explicit operator AdModel(AdViewModel model)
        {
            return new AdModel
            {
                Currency = model.Currency,
                AdPrice = model.AdPrice,
                AdPriceId = model.AdPriceId,
                Country = model.Country,
                CreateDate = model.CreateDate,
                Description = model.Description,
                EndDisplayTime = model.EndDisplayTime,
                Id = model.Id,
                Title = model.Title,
                Price = model.Price,
                IsValid = model.IsValid,
                ImageUrl1 = model.ImageUrl1,
                ImageUrl2 = model.ImageUrl2,
                ImageUrl3 = model.ImageUrl3,
                ImageUrl4 = model.ImageUrl4,
                OwnerId = model.OwnerId,
                PaymentInformation = model.PaymentInformation,
                Url1 = model.Url1,
                Url2 = model.Url2,
                Url3 = model.Url3,
                Url4 = model.Url4,
                ViewCount = model.ViewCount,
                StartDisplayTime = model.StartDisplayTime,
                OrderNumber = model.OrderNumber,
                ValidateDate = model.ValidateDate
            };
        }
    }
}
