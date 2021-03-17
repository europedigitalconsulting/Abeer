using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Microsoft.AspNetCore.Authorization;
using Abeer.Data.UnitOfworks;
using System;
using Abeer.Services;
using System.Collections.Generic;
using Abeer.Shared.ViewModels;
using System.Linq;

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
            foreach (var item in ListMessage.Where(x => x.DateReceived == null).ToList())
            {
                item.DateReceived = DateTime.Now;
                await _functionalUnitOfWork.MessageRepository.Update(item);
            }
            
            return Ok(ListMessage);
        }
        [HttpGet("GetMessageUnread")]
        public async Task<ActionResult<List<string>>> GetMessageUnread()
        {
            var listMsg = await _functionalUnitOfWork.MessageRepository.GetMessageUnread(Guid.Parse(User.NameIdentifier()));
            var result = listMsg.Select(x => x.UserIdFrom).Distinct();

            return Ok(result);
        }
        [HttpPut]
        public async Task<IActionResult> PutAsync(Message model)
        {
            var message = await _functionalUnitOfWork.MessageRepository.FirstOrDefault(x => x.Id == model.Id);

            if (message == null)
                return BadRequest();

            message.DateReceived = DateTime.Now; 

            await _functionalUnitOfWork.MessageRepository.Update(message);
            return Ok(message);
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
