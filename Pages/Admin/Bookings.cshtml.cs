using Microsoft.AspNetCore.Mvc.RazorPages;
using MongoDB.Driver;
using Safar.Models;
using System.Collections.Generic;
using System.Linq;

namespace Safar.Pages.Admin
{
    public class BookingsModel : PageModel
    {
        private readonly IMongoCollection<Booking> _bookingCollection;

        public List<Booking> BookingsList { get; set; } = new List<Booking>();

        public BookingsModel(IMongoDatabase database)
        {
            _bookingCollection = database.GetCollection<Booking>("Bookings");
        }

        public void OnGet()
        {
            // Reverse chronological order: Taake sab se latest booking sab se upar dikhe
            BookingsList = _bookingCollection.Find(b => true)
                                             .SortByDescending(b => b.BookingTimestamp)
                                             .ToList();
        }
    }
}