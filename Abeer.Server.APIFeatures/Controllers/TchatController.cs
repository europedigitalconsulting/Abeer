using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using System.Collections.Generic;
using Abeer.Shared.ViewModels;

namespace Abeer.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TchatController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _functionalUnitOfWork;
        public TchatController(FunctionalUnitOfWork functionalUnitOfWork)
        {
            _functionalUnitOfWork = functionalUnitOfWork;
        }
        [HttpGet("{contactId}")]
        public async Task<ActionResult<List<Message>>> Getsync(Guid contactId)
        {
            var ListMessage = await _functionalUnitOfWork.MessageRepository.GetMessages(Guid.Parse(User.NameIdentifier()), contactId);
            return Ok(ListMessage);
        }
        [HttpPost]
        public async Task<IActionResult> PostAsync(TchatViewModel model)
        {
            var contact = await _functionalUnitOfWork.ContactRepository.GetContact(model.ContactId, User.NameIdentifier());

            if (contact == null)
                return BadRequest();

            Message msg = new Message
            {
                UserIdFrom = Guid.Parse(User.NameIdentifier()),
                UserIdTo = Guid.Parse(contact.UserId),
                DateSent = DateTime.Now,
                Text = model.Text,
            };

            msg = await _functionalUnitOfWork.MessageRepository.Add(msg);
            return Ok(msg);
        }
    }
}
