using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using Abeer.Shared.Technical;
using Abeer.Shared.ViewModels;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Abeer.Ads.Data;
using Abeer.DataFileReader;

namespace Abeer.Ads.ApiFeatures
{
    [Route("api/bo/[controller]")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly AdsUnitOfWork _adsUnitOfWork;
        private readonly ILogger<ExportController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ExportController(AdsUnitOfWork referentialUnitOfWork, ILogger<ExportController> logger, IServiceProvider serviceProvider)
        {
            _adsUnitOfWork = referentialUnitOfWork;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpPost()]
        public async Task<ActionResult<ExportResultViewModel>> ExportReferential(ExportOptionsViewModel options)
        {
            IList<ExchangeDataRow> models = new List<ExchangeDataRow>();
            var exportResult = new ExportResultViewModel();

            if (options.IncludeFamily)
                await ExportFamilies(models, options, exportResult);

            if (options.IncludeCategory)
                await ExportCategories(models, options, exportResult);

            using var memo = new MemoryStream();

            switch (options.FileType)
            {
                case ".xlsx":
                    {
                        using var xlsxWriter = new XlsWriter<ExchangeDataRow>(options, models, memo);
                        xlsxWriter.Write();
                        exportResult.FileMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                        break;
                    }
                case ".csv":
                    {
                        using var stmWriter = new StreamWriter(memo);
                        using var csvWriter = new CsvWriter<ExchangeDataRow>(options, models, stmWriter);
                        exportResult.FileMimeType = "text/csv";
                        csvWriter.Write();
                        stmWriter.Close();
                        break;
                    }
                case ".xml":
                    {
                        using var stmWriter = new StreamWriter(memo);
                        using var xmlWriter = new XmlWriter<ExchangeDataRow>(options, models, stmWriter);
                        exportResult.FileMimeType = "application/xml";
                        xmlWriter.Write();
                        stmWriter.Close();
                        break;
                    }
            }

            exportResult.FileContent = memo.ToArray();
            return Ok(exportResult);
        }

        private async Task ExportCategories(IList<ExchangeDataRow> models, ExportOptionsViewModel options, ExportResultViewModel exportResult)
        {
            List<AdsCategoryViewModel> categories = new List<AdsCategoryViewModel>();

            if (!string.IsNullOrEmpty(options.Category))
            {
                categories.AddRange(await _adsUnitOfWork.CategoriesRepository.FilterByIds(options.Category.Split(';').Select(i => Guid.Parse(i)).ToList()));
            }
            else
            {
                if (!string.IsNullOrEmpty(options.Family))
                    categories.AddRange(await _adsUnitOfWork.CategoriesRepository.GetByFamily(Guid.Parse(options.Family)));
                else
                    categories.AddRange(await _adsUnitOfWork.CategoriesRepository.GetAll());
            }

            exportResult.CategoriesExported = categories.Count;

            foreach (var category in categories)
                ParseCategory(category, models);
        }

        private async Task ExportFamilies(IList<ExchangeDataRow> models, ExportOptionsViewModel options, ExportResultViewModel exportResult)
        {
            List<AdsFamilyViewModel> families = new List<AdsFamilyViewModel>();

            if (!string.IsNullOrEmpty(options.Family))
            {
                var family = await _adsUnitOfWork.FamiliesRepository.Get(Guid.Parse(options.Family));
                families.Add(family);
            }
            else
            {
                families.AddRange(await _adsUnitOfWork.FamiliesRepository.GetAll());
            }

            exportResult.FamiliesExported = families.Count;

            foreach (var family in families)
                await ParseFamily(family, models);
        }

        private void ParseCategory(AdsCategoryViewModel category, IList<ExchangeDataRow> models)
        {
            models.Add(new ExchangeDataRow
            {
                Type = "Category",
                FamilyId = category.FamilyId.ToString(),
                CategoryId = category.CategoryId.ToString(),
                Code = category.Code,
                Label = category.Label,
                MetaTitle = category.MetaTitle,
                MetaDescription = category.MetaDescription,
                MetaKeywords = category.MetaKeywords
            });
        }

        private async Task ParseFamily(AdsFamilyViewModel family, IList<ExchangeDataRow> models)
        {
            models.Add(new ExchangeDataRow
            {
                Type = "Family",
                FamilyId = family.FamilyId.ToString(),
                Code = family.Code,
                Label = family.Label,
                LabelSearch = family.LabelSearch,
                UrlApi = family.UrlApi,
                MetaTitle = family.MetaTitle,
                MetaDescription = family.MetaDescription,
                MetaKeywords = family.MetaKeywords,
                PurchaseLabelRule = family.PurchaseLabelRule
            });

            var attributes = await _adsUnitOfWork.FamilyAttributesRepository.GetByFamily(family.FamilyId);

            if (attributes != null && attributes.Any())
            {
                foreach (var attribute in attributes)
                {
                    models.Add(new ExchangeDataRow
                    {
                        Type = "Attribute",
                        FamilyId = family.FamilyId.ToString(),
                        FamilyAttributeId = attribute.FamilyAttributeId.ToString(),
                        Code = attribute.Code,
                        Label = attribute.Label,
                        IsRequired = attribute.IsRequired.ToString(),
                        IsSearchable = attribute.IsSearchable.ToString(),
                        AttributeType = attribute.Type
                    });
                }
            }
        }
    }
}