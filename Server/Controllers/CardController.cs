using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Abeer.Data.UnitOfworks;
using Abeer.Server.Hubs;
using Abeer.Shared;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _UnitOfWork;
        public IHubContext<SynchroHub> HubContext { get; }
        private static readonly Random rdm = new Random();

        public CardController(FunctionalUnitOfWork onlineWalletUnitOfWork, IHubContext<SynchroHub> hubContext)
        {
            _UnitOfWork = onlineWalletUnitOfWork;
            HubContext = hubContext;
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Card>>> GetCards()
        {
            return Ok(await _UnitOfWork.CardRepository.GetCards());
        }

        // GET: api/Cards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Card>> GetCard(Guid id)
        {
            var card = await _UnitOfWork.CardRepository.GetCard(id);

            if (card == null)
            {
                return NotFound();
            }

            return card;
        }

        // PUT: api/Cards/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCard(Guid id, Card Card)
        {
            if (id != Card.Id)
            {
                return BadRequest();
            }

            await _UnitOfWork.CardRepository.Update(Card);

            await _UnitOfWork.CardRepository.AddStatus(new CardStatu { Card = Card, StatusDate = DateTime.UtcNow, Status = CardStatus.Updated, UserId = User.NameIdentifier() });

            await HubContext.Clients.All.SendAsync("Card.Update", Card);

            return NoContent();
        }

        [HttpPut("sell/{id}")]
        public async Task<IActionResult> SellCard(Guid id, Card Card)
        {
            if (id != Card.Id)
            {
                return BadRequest();
            }

            var current = await _UnitOfWork.CardRepository.FirstOrDefault(c => c.Id == id);

            current.IsSold = true;
            current.SoldDate = DateTime.UtcNow;
            current.SoldBy = User.NameIdentifier();

            await _UnitOfWork.CardRepository.Update(current);

            await _UnitOfWork.CardRepository.AddStatus(new CardStatu { Card = current, StatusDate = DateTime.UtcNow, Status = CardStatus.Sold, UserId = User.NameIdentifier() });

            await HubContext.Clients.All.SendAsync("Card.Sold", current);

            return NoContent();
        }

        // POST: api/Cards
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Card>>> PostCard(Card Card)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                for (int i = 0; i < Card.Quantity; i++)
                {
                    await AddCard(new Card
                    {
                        CardType = Card.CardType,
                        CreatorId = User.NameIdentifier(),
                        GeneratedBy = User.NameIdentifier(),
                        GeneratedDate = DateTime.UtcNow,
                        Icon = Card.Icon,
                        IsGenerated = true,
                        CardNumber = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100, 999)),
                        PinCode = rdm.Next(10000, 99999).ToString(),
                        Quantity = 1,
                        Value = Card.Value
                    });
                }
                return Ok(await _UnitOfWork.CardRepository.GetCards());
            }
        }

        private async Task<Card> AddCard(Card Card)
        {
            Card = await _UnitOfWork.CardRepository.Add(Card, User.NameIdentifier());
            await HubContext.Clients.All.SendAsync("Card.Insert", Card);
            return Card;
        }

        // DELETE: api/Cards/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<Card>> DeleteCard(Guid id)
        {
            var Card = await _UnitOfWork.CardRepository.Find(id);

            if (Card == null)
            {
                return NotFound();
            }

            _UnitOfWork.CardRepository.Remove(Card);

            return Card;
        }

        [HttpGet("GetCsvFile/{id}")]
        public async Task<IActionResult> GetCsvFile(Guid id)
        {
            var Card = await _UnitOfWork.CardRepository.Find(id);

            if (Card == null)
                return NotFound();

            return File(Card.CsvFileContent, "text/csv", $"data_{Card.CardNumber}_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + ".csv"}");
        }
    }
}
