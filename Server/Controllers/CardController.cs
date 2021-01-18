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
using Abeer.Shared.ViewModels;
using System.Data;
using ClosedXML.Excel;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CardController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _UnitOfWork;
        private readonly IConfiguration _configuration;
        public IHubContext<SynchroHub> HubContext { get; }

        public CardController(FunctionalUnitOfWork onlineWalletUnitOfWork, IHubContext<SynchroHub> hubContext, IConfiguration configuration)
        {
            _UnitOfWork = onlineWalletUnitOfWork;
            _configuration = configuration;
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

        [HttpGet("Statistics")]
        public async Task<ActionResult<CardStatisticsViewModel>> GetStatistics()
        {
            var cards = (await _UnitOfWork.CardRepository.GetCards()).ToList();

            var data = new CardStatisticsViewModel
            {
                NbOfCard = cards.Count,
                NbOfCardUsed = cards.Count(t => t.IsUsed)
            };

            data.NbOfCardAvailable = data.NbOfCard - data.NbOfCardUsed;

            return data;
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
            _UnitOfWork.SaveChanges();

            await _UnitOfWork.CardRepository.AddStatus(new CardStatu
            {
                Card = Card, StatusDate = DateTime.UtcNow, Status = CardStatus.Updated, UserId = User.NameIdentifier()
            });
            _UnitOfWork.SaveChanges();

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

            Card.IsSold = true;
            Card.SoldDate = DateTime.UtcNow;
            Card.SoldBy = User.NameIdentifier();

            await _UnitOfWork.CardRepository.Update(Card);
            _UnitOfWork.SaveChanges();

            await _UnitOfWork.CardRepository.AddStatus(new CardStatu
                {Card = Card, StatusDate = DateTime.UtcNow, Status = CardStatus.Sold, UserId = User.NameIdentifier()});
            _UnitOfWork.SaveChanges();

            await HubContext.Clients.All.SendAsync("Card.Sold", Card);

            return NoContent();
        }

        // POST: api/Cards
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost]
        public async Task<ActionResult<Card>> PostCard(Card Card)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                Card = await _UnitOfWork.CardRepository.Add(Card, User.NameIdentifier());
                await HubContext.Clients.All.SendAsync("Card.Insert", Card);

                return CreatedAtAction("GetCard", new {id = Card.Id}, Card);
            }
        }

        private Random rdm = new Random();

        [HttpPost("generate")]
        public async Task<ActionResult<IList<Card>>> Generate(Batch batch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            else
            {
                batch.Id = batch.Id == Guid.Empty ? Guid.NewGuid() : batch.Id;

                batch = await _UnitOfWork.CardRepository.AddBatch(batch, User.NameIdentifier());
                await HubContext.Clients.All.SendAsync("Batch.Insert", batch);

                string left = DateTime.UtcNow.ToString("yyyMMddHHmmss");
                string right = "100000";

                if (!string.IsNullOrEmpty(batch.CardStartNumber))
                {
                    right = batch.CardStartNumber.Substring(batch.CardStartNumber.Length - 6);
                    left = batch.CardStartNumber.Substring(0, batch.CardStartNumber.Length - 6);
                }

                var dt = new DataTable("Cards");
                
                var result = new List<Card>();

                dt.Columns.Add("Id");
                dt.Columns.Add("CardNumber");
                dt.Columns.Add("PinCode");
                dt.Columns.Add("ProfileUrl");

                for (int i = 0; i < batch.Quantity; i++)
                {
                    var carindex = long.Parse(right) + i;
                    var cardNumber = string.Concat(left, carindex);

                    var dr = dt.NewRow();

                    var card = new Card
                    {
                        Id = Guid.NewGuid(),
                        Batch = batch, BatchId =  batch.Id, 
                        Value = 0, Quantity = 1, 
                        CardNumber = cardNumber.ToString(), 
                        GeneratedBy = User.NameIdentifier(), 
                        PinCode = rdm.Next(10000, 99999).ToString(), 
                        CardType = batch.CardType, 
                        IsGenerated = true, CreatorId = User.NameIdentifier(), 
                    };

                    card = await _UnitOfWork.CardRepository.Add(card, User.NameIdentifier());
                    result.Add(card);

                    dr["Id"] = card.Id;
                    dr["CardNumber"] = card.CardNumber;
                    dr["PinCode"] = card.PinCode;
                    dr["ProfileUrl"] = $"{_configuration["Service:FrontOffice:Url"]}/viewprofile/{cardNumber}";

                    dt.Rows.Add(dr);
                }

                using var wb = new XLWorkbook();
                wb.AddWorksheet(dt);

                await using var stream = new MemoryStream();
                wb.SaveAs(stream);
                stream.Flush();

                var content = stream.ToArray();
                batch.CsvFileContent = content;
                await _UnitOfWork.CardRepository.UpdateBatch(batch);

                return CreatedAtAction("GetBatch", new { id = batch.Id }, new GeneratedBatchViewModel
                {
                    Batch = batch,
                    Cards = result
                });
            }
        }

        [HttpGet("GetBatch")]
        public async Task<ActionResult<Batch>> GetBatch(Guid id)
        {
            var batch = await _UnitOfWork.CardRepository.FindBatch(id);
            return Ok(batch);
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
            _UnitOfWork.SaveChanges();

            return Card;
        }

        [HttpGet("GetCsvFile/{id}")]
        public async Task<IActionResult> GetCsvFile(Guid id)
        {
            var Card = await _UnitOfWork.CardRepository.Find(id);

            if (Card == null)
                return NotFound();

            return File(Card.CsvFileContent, "text/csv",
                $"data_{Card.CardNumber}_{DateTime.UtcNow.ToString("yyyMMddHHmmss") + ".csv"}");
        }
    }
}