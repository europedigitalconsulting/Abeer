using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using Abeer.Shared.Technical;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abeer.Services;
using Abeer.DataFileReader;
using Abeer.Ads.Data;

namespace Abeer.Ads.ApiFeatures
{
    [Route("api/bo/[controller]")]
    [ApiController]
    public class ImportController : ControllerBase
    {
        private readonly AdsUnitOfWork _adsUnitOfWork;
        private readonly ILogger<ImportController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ImportController(AdsUnitOfWork referentialUnitOfWork, ILogger<ImportController> logger, IServiceProvider serviceProvider)
        {
            _adsUnitOfWork = referentialUnitOfWork;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpPost()]
        public async Task<ActionResult<AdsImportResultViewModel>> ImportReferential(ImportOptionsViewModel options)
        {
            var extension = System.IO.Path.GetExtension(options.FileName);

            if (string.IsNullOrEmpty(extension))
                return BadRequest("extension");

            IList<ExchangeDataRow> models = new
                List<ExchangeDataRow>();

            var result = new AdsImportResultViewModel();

            switch (extension)
            {
                case ".xlsx":
                case ".xls":
                    {
                        using var xlsReader = new XlsReader<ExchangeDataRow>(options);
                        models = xlsReader.Read();
                        break;
                    }
                case ".xml":
                    {
                        var xmlReader = new XmlReader<ExchangeDataRow>(options);
                        models = xmlReader.Read();
                        break;
                    }
                case ".csv":
                case ".txt":
                    {
                        var csvReader = new CsvReader<ExchangeDataRow>(options);
                        models = csvReader.Read();
                        break;
                    }
            }

            await StartProcessImportProduct(options, models, result);

            return Ok(result);
        }

        private async Task StartProcessImportProduct(ImportOptionsViewModel options, IList<ExchangeDataRow> models, AdsImportResultViewModel importResult)
        {
            if (options.Reset)
            {
                var duo = _serviceProvider.GetRequiredService<AdsUnitOfWork>();

                await duo.CategoriesRepository.RemoveAll((await duo.CategoriesRepository.GetAll())?.Select(t => t.CategoryId).ToList());
                await duo.FamiliesRepository.RemoveAll((await duo.FamiliesRepository.GetAll())?.Select(f => f.FamilyId).ToList());
            }

            foreach (var item in models)
            {
                if (string.IsNullOrEmpty(item.Type))
                    continue;

                switch (item.Type.ToLower())
                {
                    case "family":
                        await ImportFamily(item, importResult);
                        break;
                    case "attribute":
                        await ImportAttribute(item);
                        break;
                    case "category":
                        await ImportCategory(item, importResult);
                        break;
                    default:
                        _logger.LogWarning($"type {item.Type} not supported");
                        break;
                }
            }
        }

        private void ConvertInt(Guid productTypeId,
            Func<string> getValue, Action<int> setValue)
        {
            var current = getValue();

            if (!int.TryParse(current, out var value))
                _logger.LogWarning($"can not parse name {current} for product type {productTypeId}");
            else
                setValue(value);
        }

        private async Task<AdsCategoryViewModel> ImportCategory(ExchangeDataRow item, AdsImportResultViewModel importResult)
        {
            if (!Guid.TryParse(item.FamilyId, out var familyId))
                _logger.LogWarning($"ParentId:{item.FamilyId} is not a guid");

            if (!Guid.TryParse(item.CategoryId, out var categoryId))
                _logger.LogWarning($"CategoryId:{item.CategoryId} is not a guid");

            var family = await ImportFamily(item, importResult);

            var current = await _adsUnitOfWork.
                CategoriesRepository.Get(categoryId);

            if (current == null)
            {
                _logger.LogInformation($"Create new category with id {categoryId} code {item.Code} label {item.Label}");

                current = new AdsCategoryViewModel()
                {
                    CategoryId = categoryId,
                    Code = item.Code,
                    Label = item.Label,
                    FamilyId = family.FamilyId,
                    MetaDescription = item.MetaDescription,
                    MetaKeywords = item.MetaKeywords,
                    MetaTitle = item.MetaTitle,
                    PictureUrl = item.PictureUrl
                };

                importResult.CategoriesAdded += 1;
                await _adsUnitOfWork.CategoriesRepository.Add(current);
            }
            else if (item.Type.Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Update Category with id {categoryId} code {current.Code} label {current.Label}");

                current.CategoryId = categoryId;
                current.Code = item.Code;
                current.Label = item.Label;
                current.FamilyId = familyId;
                current.MetaDescription = item.MetaDescription;
                current.MetaKeywords = item.MetaKeywords;
                current.MetaTitle = item.MetaTitle;
                current.PictureUrl = item.PictureUrl;

                importResult.CategoriesUpdated += 1;
                await _adsUnitOfWork.CategoriesRepository.Update(current);
            }

            return current;
        }

        private async Task ImportAttribute(ExchangeDataRow item)
        {
            if (!Guid.TryParse(item.FamilyAttributeId, out var familyAttributeId))
                _logger.LogWarning($"Id:{item.FamilyAttributeId} is not a guid");

            if (!Guid.TryParse(item.FamilyId, out var familyId))
                _logger.LogWarning($"ParentId:{item.FamilyId} is not a guid");

            var attributes = await _adsUnitOfWork
                .FamilyAttributesRepository
                .GetByFamily(familyId);

            var current = attributes.FirstOrDefault(a => a.FamilyAttributeId == familyAttributeId);

            if (current == null)
            {
                _logger.LogInformation($"Create new attribute for family {familyId}  with id {familyAttributeId} code {item.Code} label {item.Label}");

                current = new AdsFamilyAttributeViewModel()
                {
                    Code = item.Code,
                    FamilyId = familyId,
                    FamilyAttributeId = familyAttributeId,
                    Label = item.Label,
                    IsRequired = bool.Parse(item.IsRequired),
                    IsSearchable = bool.Parse(item.IsSearchable),
                    Type = item.AttributeType
                };

                await _adsUnitOfWork.FamilyAttributesRepository.Add(current);
            }
            else if (item.Type.Equals("category", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Update attribute {familyAttributeId} for family with id {familyId} code {current.Code} label {current.Label}");

                current.Code = item.Code;
                current.Label = item.Label;
                current.IsRequired = bool.Parse(item.IsRequired);
                current.IsSearchable = bool.Parse(item.IsSearchable);
                current.Type = item.AttributeType;

                await _adsUnitOfWork.FamilyAttributesRepository.Update(current);
            }
        }

        private async Task<AdsFamilyViewModel> ImportFamily(ExchangeDataRow item, AdsImportResultViewModel importResult)
        {
            if (!Guid.TryParse(item.FamilyId, out var familyId))
                _logger.LogWarning($"ParentId:{item.FamilyId} is not a guid");

            var current = await _adsUnitOfWork.FamiliesRepository.Get(familyId);

            if (current == null)
            {
                _logger.LogInformation($"Create new family with id {familyId} code {item.Code} label {item.Label}");

                current = new AdsFamilyViewModel()
                {
                    Code = item.Code,
                    FamilyId = familyId,
                    Label = item.Label,
                    LabelSearch = item.LabelSearch,
                    HeaderAuthorize = item.HeaderAuthorize,
                    MetaDescription = item.MetaDescription,
                    MetaKeywords = item.MetaKeywords,
                    MetaTitle = item.MetaTitle,
                    PictureUrl = item.PictureUrl,
                    UrlApi = item.UrlApi,
                    PurchaseLabelRule = item.PurchaseLabelRule
                };

                importResult.FamiliesAdded += 1;
                await _adsUnitOfWork.FamiliesRepository.Add(current);
            }
            else if (item.Type.Equals("family", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogInformation($"Update family with id {familyId} code {current.Code} label {current.Label}");

                bool hasChanged = false;

                if (current.Code != item.Code)
                {
                    hasChanged = true;
                    current.Code = item.Code;
                }

                if (current.Label != item.Label)
                {
                    hasChanged = true;
                    current.Label = item.Label;
                }

                if (current.LabelSearch != item.LabelSearch)
                {
                    hasChanged = true;
                    current.LabelSearch = item.LabelSearch;
                }

                if (current.HeaderAuthorize != item.HeaderAuthorize)
                {
                    hasChanged = true;
                    current.HeaderAuthorize = item.HeaderAuthorize;
                }

                if (current.MetaDescription != item.MetaDescription)
                {
                    hasChanged = true;
                    current.MetaDescription = item.MetaDescription;
                }

                if (current.MetaKeywords != item.MetaKeywords)
                {
                    hasChanged = true;
                    current.MetaKeywords = item.MetaKeywords;
                }

                if (current.MetaTitle != item.MetaTitle)
                {
                    hasChanged = true;
                    current.MetaTitle = item.MetaTitle;
                }

                if (current.PictureUrl != item.PictureUrl)
                {
                    hasChanged = true;
                    current.PictureUrl = item.PictureUrl;
                }

                if (current.UrlApi != item.UrlApi)
                {
                    hasChanged = true;
                    current.UrlApi = item.UrlApi;
                }

                if (current.PurchaseLabelRule != item.PurchaseLabelRule)
                {
                    hasChanged = true;
                    current.PurchaseLabelRule = item.PurchaseLabelRule;
                }

                if (hasChanged)
                {
                    importResult.FamiliesUpdated += 1;
                    await _adsUnitOfWork.FamiliesRepository.Update(current);
                }
            }

            return current;
        }
    }
}