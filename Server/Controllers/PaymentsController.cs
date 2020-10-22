using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Abeer.Shared;
using Abeer.Data.UnitOfworks;
using Microsoft.AspNetCore.Authorization;

namespace Abeer.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly FunctionalUnitOfWork _context;

        public PaymentsController(FunctionalUnitOfWork context)
        {
            _context = context;
        }

        // GET: api/Payments
        [HttpGet("{id}")]
        public async Task<ActionResult<IList<Payment>>> GetPayment(Guid transactionId)
        {
            return Ok(await _context.PaymentRepository.GetPayments(transactionId));
        }

        // GET: api/Payments/5
        [HttpGet("{tid}/{id}")]
        public async Task<ActionResult<Payment>> GetPayment(Guid tid, Guid id)
        {
            var payment = await _context.PaymentRepository.GetPayment(p=>p.TransactionId == tid && p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            return payment;
        }

        // PUT: api/Payments/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPut("{tid}/{id}")]
        public async Task<IActionResult> PutPayment(Guid tid, Guid id, Payment payment)
        {
            if (id != payment.Id)
            {
                return BadRequest();
            }

            await _context.PaymentRepository.Update(payment);

            return NoContent();
        }

        // POST: api/Payments
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [HttpPost("{tid}")]
        public async Task<ActionResult<Payment>> PostPayment(Guid tid, Payment payment)
        {
            if (payment.TransactionId != Guid.Empty && payment.TransactionId != tid)
                return BadRequest();

            await _context.PaymentRepository.Add(payment);

            return CreatedAtAction("GetPayment", new { id = payment.Id }, payment);
        }

        // DELETE: api/Payments/5
        [HttpDelete("{tid}/{id}")]
        public async Task<ActionResult<Payment>> DeletePayment(Guid tid, Guid id)
        {
            var payment = await _context.PaymentRepository.GetPayment(p=>p.TransactionId == tid && p.Id == id);

            if (payment == null)
            {
                return NotFound();
            }

            await _context.PaymentRepository.Remove(payment);

            return payment;
        }
    }
}
