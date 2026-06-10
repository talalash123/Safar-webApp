using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;

namespace Safar.Pages.Customer
{
    public class PassengerInfoModel : PageModel
    {
        [BindProperty(SupportsGet = true)]
        public string Id { get; set; } // Train ID

        [BindProperty(SupportsGet = true)]
        public string Src { get; set; } // Source station

        [BindProperty(SupportsGet = true)]
        public string Dest { get; set; } // Destination station

        [BindProperty(SupportsGet = true)]
        public string Date { get; set; } // Travel date string

        [BindProperty(SupportsGet = true)]
        public string Class { get; set; } // Class: Economy, Business, Executive

        [BindProperty(SupportsGet = true)]
        public decimal Fare { get; set; } // Ticket Fare per seat

        // Passenger Form Data
        [BindProperty]
        public string LeadName { get; set; }

        [BindProperty]
        public string LeadPhone { get; set; }

        [BindProperty]
        public string LeadCNIC { get; set; }

        [BindProperty]
        public int PassengerCount { get; set; } = 1;

        [BindProperty]
        public int LeadAge { get; set; } = 30;

        [BindProperty]
        public string LeadGender { get; set; } = "Male";

        [BindProperty]
        public bool IsWithFamily { get; set; }

        // Form post variables for list of passengers
        [BindProperty]
        public List<string> PassengerNames { get; set; } = new List<string>();

        [BindProperty]
        public List<int> PassengerAges { get; set; } = new List<int>();

        [BindProperty]
        public List<string> PassengerGenders { get; set; } = new List<string>();

        public void OnGet()
        {
            // Initializes values from search query strings
        }

        public IActionResult OnPost()
        {
            if (string.IsNullOrEmpty(LeadName) || string.IsNullOrEmpty(LeadPhone) || string.IsNullOrEmpty(LeadCNIC))
            {
                return Page();
            }

            // Create serialized passenger manifest list to pass cleanly to next steps
            var manifestList = new List<PassengerManifestItem>();
            
            // Add lead passenger as first manifest item
            manifestList.Add(new PassengerManifestItem 
            { 
                Name = LeadName, 
                Age = LeadAge, 
                Gender = LeadGender 
            });

            // Add other passengers
            for (int i = 0; i < PassengerCount - 1; i++)
            {
                string pName = PassengerNames.Count > i ? PassengerNames[i] : $"Passenger {i + 2}";
                int pAge = PassengerAges.Count > i ? PassengerAges[i] : 30;
                string pGender = PassengerGenders.Count > i ? PassengerGenders[i] : "Male";

                manifestList.Add(new PassengerManifestItem
                {
                    Name = pName,
                    Age = pAge,
                    Gender = pGender
                });
            }

            string serializedManifest = JsonSerializer.Serialize(manifestList);

            // Redirect to SelectSeats carrying all passenger inputs in query parameters
            return RedirectToPage("./SelectSeats", new
            {
                id = Id,
                src = Src,
                dest = Dest,
                date = Date,
                @class = Class,
                fare = Fare,
                passengerCount = PassengerCount,
                leadName = LeadName,
                leadPhone = LeadPhone,
                leadCNIC = LeadCNIC,
                leadAge = LeadAge,
                leadGender = LeadGender,
                isWithFamily = IsWithFamily,
                passengerManifest = serializedManifest
            });
        }
    }

    public class PassengerManifestItem
    {
        public string Name { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
    }
}
