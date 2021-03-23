using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Abeer.Ads.Shared;
using Abeer.Shared.ViewModels;
using Abeer.Ads.Data;
using Abeer.DataFileReader;
using Abeer.Services;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared.Data;

namespace Abeer.Ads.ApiFeatures
{
    [Route("api/bo/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AdsUnitOfWork _consumableUnitOfWork;

        public CategoriesController(AdsUnitOfWork consumableUnitOfWork)
        {
            _consumableUnitOfWork = consumableUnitOfWork;
        }
        
        [HttpGet]
        public async Task<ActionResult<IList<AdsCategoryViewModel>>> Getall()
        {
            var list = await _consumableUnitOfWork.CategoriesRepository.GetAll();
            return Ok(list.OrderBy(x => x.FamilyId));
        }

        [HttpGet("byfamily/{familyId}")]
        public async Task<ActionResult<IList<AdsCategoryViewModel>>> GetByFamily(Guid familyId)
        {
            var list = await _consumableUnitOfWork.CategoriesRepository.GetByFamily(familyId);
            return Ok(list.OrderBy(x => x.FamilyId));
        }


        [HttpGet("filter/{familyId}")]
        public async Task<ActionResult<IList<AdsCategoryViewModel>>> FilterByFamily(string familyId)
        {
            var familiesId = familyId.Split(';').Select(i => Guid.Parse(i)).ToList();
            var list = await _consumableUnitOfWork.CategoriesRepository.FilterByFamilies(familiesId);
            return Ok(list.OrderBy(x => x.FamilyId));
        }

        [HttpPost]
        public async Task<ActionResult<AdsCategoryViewModel>> Create(AdsCategoryViewModel categoryView)
        {
            if (!ModelState.IsValid)
                return BadRequest();

            var inserted = await _consumableUnitOfWork.CategoriesRepository.Add(categoryView);
            return Created($"{inserted.CategoryId}", inserted);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdsCategoryViewModel>> Update(Guid id, AdsCategoryViewModel categoryView)
        {
            if (categoryView == null)
                return BadRequest();

            if (categoryView.CategoryId != id)
                return BadRequest();

            var entity = await _consumableUnitOfWork.CategoriesRepository.Update(categoryView);

            return Ok(categoryView);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(Guid id)
        {
            await _consumableUnitOfWork.CategoriesRepository.Remove(id);
            return Ok();
        }

        [HttpPost("import")]
        public async Task Import(ImportOptionsViewModel importOptions)
        {
            var extension = System.IO.Path.GetExtension(importOptions.FileName);

            IList<AdsCategoryViewModel> models = new
                List<AdsCategoryViewModel>();

            if (extension.Equals(".xlsx") || extension.Equals(".xls"))
            {
                using var xlsReader = new XlsReader<AdsCategoryViewModel>(importOptions);
                models = xlsReader.Read();
            }
            else if (extension.Equals(".xml"))
            {
                var xmlReader = new XmlReader<AdsCategoryViewModel>(importOptions);
                models = xmlReader.Read();
                ImportSubCategories(models, xmlReader);
            }

            // await _consumableUnitOfWork.CategoriesRepository.Clear();
            // await _consumableUnitOfWork.CategoriesRepository.BulkInsertAsync(models);
        }

        private void ImportSubCategories(IList<AdsCategoryViewModel> models, XmlReader<AdsCategoryViewModel> xmlReader)
        {
            var parents = models.ToArray();

            foreach (var parent in parents)
            {
                var node = xmlReader.SelectNode($"/Categories/Category[Id={parent.CategoryId}]");

                if (node != null)
                {
                    var subCategories = node.SelectNodes("SubCategories/Category");

                    if (subCategories != null && subCategories.Count > 0)
                    {
                        ImportSubCategories(models, xmlReader, subCategories);
                    }
                }
            }
        }

        private static void ImportSubCategories(IList<AdsCategoryViewModel> models, XmlReader<AdsCategoryViewModel> xmlReader, XmlNodeList nodes)
        {
            foreach (XmlNode xSubCategory in nodes)
            {
                var subCategory = xmlReader.ParseNode(xSubCategory);

                models.Add(subCategory);

                var subCategories = xSubCategory.SelectNodes("SubCategories/Category");

                if (subCategories != null && subCategories.Count > 0)
                {
                    ImportSubCategories(models, xmlReader, subCategories);
                }
            }
        }
    }
}
