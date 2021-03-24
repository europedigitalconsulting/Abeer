using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Abeer.Ads.Shared;
using Abeer.Shared.Technical;
using Abeer.Ads.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Abeer.Ads.ApiFeatures
{
    [Route("api/bo/[controller]")]
    [ApiController]
    public class FamiliesController : ControllerBase
    {
        private readonly AdsUnitOfWork _adsUnitOfWork;

        public FamiliesController(AdsUnitOfWork adsUnitOfWork)
        {
            _adsUnitOfWork = adsUnitOfWork;
        }

        [HttpGet]
        public async Task<ActionResult<IList<AdsFamilyViewModel>>> GetAll()
        {
            var familyViewModels = await _adsUnitOfWork.FamiliesRepository.GetAll();
            return Ok(familyViewModels);
        }

        [HttpPost]
        public async Task<ActionResult<AdsFamilyViewModel>> Create(AdsFamilyViewModel familyView)
        {
            if (familyView == null)
                return BadRequest();

            if (familyView.FamilyId == Guid.Empty)
            {
                familyView.FamilyId = Guid.NewGuid();

                foreach (var item in familyView.Attributes)
                {
                    item.FamilyId = familyView.FamilyId;
                    item.FamilyAttributeId = Guid.NewGuid();
                }
            }

            if (!ModelState.IsValid)
                return BadRequest();

            var entity = await _adsUnitOfWork.FamiliesRepository.Add(familyView);

            return Created($"{familyView.FamilyId}", familyView);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<AdsFamilyViewModel>> Update(Guid id, AdsFamilyViewModel familyView)
        {
            if (familyView == null)
                return BadRequest();

            if (familyView.FamilyId != id)
                return BadRequest();

            if (familyView.Attributes.Any(a => a.FamilyAttributeId == Guid.Empty))
            {
                foreach (var familyAttribute in familyView.Attributes.Where(a => a.FamilyAttributeId == Guid.Empty))
                {
                    familyAttribute.FamilyId = id;
                    familyAttribute.FamilyAttributeId = Guid.NewGuid();
                }
            }

            var vm = await _adsUnitOfWork.FamiliesRepository.Update(familyView);

            foreach (var attribute in familyView.Attributes)
            {
                if (vm.Attributes.Any(a => a.FamilyAttributeId == attribute.FamilyAttributeId))
                {
                    await _adsUnitOfWork.FamilyAttributesRepository.Update(attribute);
                }
                else
                {
                    await _adsUnitOfWork.FamilyAttributesRepository.Add(attribute);
                }
            }

            var attributes = await _adsUnitOfWork.FamilyAttributesRepository.GetByFamily(familyView.FamilyId);

            foreach (var attribute in attributes.Except(familyView.Attributes, model => model.FamilyAttributeId))
            {
                await _adsUnitOfWork.FamilyAttributesRepository.Remove(attribute.FamilyAttributeId);
            }

            var result = await _adsUnitOfWork.FamiliesRepository.Get(familyView.FamilyId);

            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<bool>> Delete(Guid id)
        {
            await _adsUnitOfWork.FamiliesRepository.Remove(id);
            return Ok();
        }

        [HttpDelete("RemoveAttributes/{id}")]
        public async Task<ActionResult<bool>> RemoveAttributes(Guid id)
        {
            await _adsUnitOfWork.FamilyAttributesRepository.Remove(id);
            return Ok();
        }

        [HttpPost("AddAttributes")]
        public async Task<ActionResult<bool>> AddAttributes(AdsFamilyAttributeViewModel model)
        {
            model = await _adsUnitOfWork.FamilyAttributesRepository.Add(model);
            return Ok(model);
        }
    }
}
