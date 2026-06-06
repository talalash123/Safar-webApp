using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;

namespace Safar.Pages.Customer
{
    public class TicketConfirmationModel : PageModel
    {
        private readonly IMongoCollection<Booking> _bookingCollection;

        public Booking TargetTicket { get; set; } = new Booking();

        public TicketConfirmationModel(IMongoDatabase database)
        {
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public IActionResult OnGet(string ticketId)
        {
            if (string.IsNullOrEmpty(ticketId))
            {
                return RedirectToPage("/Customer/Index");
            }

            // Extract the freshly saved booking document from MongoDB via Unique Document ID parameters
            TargetTicket = _bookingCollection.Find(b => b.Id == ticketId).FirstOrDefault();

            if (TargetTicket == null)
            {
                return RedirectToPage("/Customer/Index");
            }

            return Page();
        }
    }
}