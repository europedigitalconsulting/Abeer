using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using Abeer.Shared.ViewModels;
using AutoMapper;
using Abeer.Shared.Functional;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class QrcodeController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        private readonly IMapper _mapper;
        public QrcodeController(FunctionalUnitOfWork functionalUnitOfWork, IMapper mapper)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetAsync()
        { 
          var list =   await _functionalUnitOfWork.QrCodeRepository.Get(Guid.Parse(User.NameIdentifier()));

            return Ok(list);
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(QrCode entity)
        {
            if (entity == null)
                return BadRequest();

            if (entity.OwnerId.ToString() != User.NameIdentifier())
                return BadRequest();

            entity = await _functionalUnitOfWork.QrCodeRepository.Add(entity);


            return Ok(entity);
        }
        [HttpDelete("{userId}/{qrcodeId}")]
        public async Task<IActionResult> Delete(string userId, Guid qrcodeId)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(qrcodeId.ToString()))
                return BadRequest();
            if (userId != User.NameIdentifier())
                return BadRequest();

            await _functionalUnitOfWork.QrCodeRepository.Delete(qrcodeId);

            return Ok();
        }
    }
}
