using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Abeer.Data.UnitOfworks;
using Abeer.Server.Hubs;
using Abeer.Shared;
using System.Text;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.IO;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _UnitOfWork;
        public IHubContext<SynchroHub> HubContext { get; }
        public IConfiguration Configuration { get; }

        private static readonly Random rdm = new Random();

        public CardController(FunctionalUnitOfWork onlineWalletUnitOfWork, IHubContext<SynchroHub> hubContext, IConfiguration configuration)
        {
            _UnitOfWork = onlineWalletUnitOfWork;
            HubContext = hubContext;
            Configuration = configuration;
        }

        // GET: api/Cards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Batch>>> GetBatches()
        {
            return Ok(await _UnitOfWork.CardRepository.GetBatches());
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
                var cardNumberPattern = string.Concat(DateTime.UtcNow.ToString("yyyMMddHHmmss"), rdm.Next(100, 999));

                var batch = await _UnitOfWork.CardRepository.AddBatch(new Batch
                {
                    Id = Guid.NewGuid(),
                    CardType = Card.CardType,
                    Quantity = Card.Quantity,
                    CardStartNumber = string.Concat(cardNumberPattern, (1).ToString().PadRight(3, '0')),
                    CardLastNumber = string.Concat(cardNumberPattern, (Card.Quantity).ToString().PadRight(3, '0'))
                }, User.NameIdentifier());

                using (MemoryStream ms = new MemoryStream())
                {
                    var sw = new StreamWriter(ms, Encoding.UTF8);
                    
                    List<Card> cards = new List<Card>();

                    for (int i = 0; i < Card.Quantity; i++)
                    {
                        if(i > 0 && i % 100 == 0)
                        {
                            cards = (await _UnitOfWork.CardRepository
                                .AddRange(batch, cards, User.NameIdentifier())).ToList();
                            await HubContext.Clients.All.SendAsync("Card.Insert", cards);
                            cards = new List<Card>();
                        }

                        cards.Add(GenerateCard(i, batch, sw, Card.CardType, cardNumberPattern, User.NameIdentifier()));
                    }

                    sw.Flush();
                    ms.Seek(0, SeekOrigin.Begin);

                    batch.CsvFileContent = ms.ToArray();
                    ms.Close();
                }

                await _UnitOfWork.CardRepository.UpdateBatch(batch);

                return Ok(await _UnitOfWork.CardRepository.GetCards());
            }
        }

        private Card GenerateCard(int i, Batch batch, StreamWriter sw, string cardType, string cardNumberPattern, string userId)
        {
            var cardNumber = string.Concat(cardNumberPattern, (i++).ToString().PadRight(3, '0'));
            var line = ($"{Configuration["Service:FrontOffice:Url"].TrimEnd('/')}/ViewProfile/{cardNumber}");
            sw.WriteLine(line);

            return new Card
            {
                Id = Guid.NewGuid(),
                CardType = cardType,
                CreatorId = userId,
                GeneratedBy = userId,
                GeneratedDate = DateTime.UtcNow,
                IsGenerated = true,
                CardNumber = cardNumber,
                PinCode = rdm.Next(10000, 99999).ToString(),
                Quantity = 1,
                BatchId = batch.Id
            };
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

            return File(Card.Batch?.CsvFileContent, "text/csv", $"data_{Card.CardNumber}_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + ".csv"}");
        }
    }
}
