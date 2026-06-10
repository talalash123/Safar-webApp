using System;
using System.Collections.Generic;
using System.Linq;

namespace SafarWebApp.Services
{
	public class PassengerProfile
	{
		public string Name { get; set; }
		public int Age { get; set; }
		public string Gender { get; set; }
		public bool IsWithFamily { get; set; }
	}

	public class BookedSeatInfo
	{
		public string SeatNumber { get; set; }
		public int PassengerAge { get; set; }
		public string PassengerGender { get; set; }
		public bool IsFamilyOccupied { get; set; }
	}

	public class SeatingArrangementService
	{
		public string SuggestBestSeat(PassengerProfile newPassenger, List<string> availableSeats, List<BookedSeatInfo> currentlyBooked)
		{
			if (!availableSeats.Any()) return null;

			// Rule 1: Senior Citizens (Age > 60) -> Try to find adjacent seats with other seniors
			if (newPassenger.Age >= 60)
			{
				var neighboringSeniors = currentlyBooked.Where(b => b.PassengerAge >= 60).Select(b => b.SeatNumber);
				foreach (var seat in availableSeats)
				{
					if (IsNearToAny(seat, neighboringSeniors)) return seat;
				}
			}

			// Rule 2: Solo Females -> Group near other females, strictly avoid isolated seats next to men
			if (newPassenger.Gender.Equals("Female", System.StringComparison.OrdinalIgnoreCase) && !newPassenger.IsWithFamily)
			{
				var femaleSeats = currentlyBooked.Where(b => b.PassengerGender.Equals("Female", System.StringComparison.OrdinalIgnoreCase)).Select(b => b.SeatNumber);
				foreach (var seat in availableSeats)
				{
					if (IsNearToAny(seat, femaleSeats)) return seat;
				}
			}

			// Rule 3: Families -> Group near other family designated areas
			if (newPassenger.IsWithFamily)
			{
				var familySeats = currentlyBooked.Where(b => b.IsFamilyOccupied).Select(b => b.SeatNumber);
				foreach (var seat in availableSeats)
				{
					if (IsNearToAny(seat, familySeats)) return seat;
				}
			}

			// Fallback: Default to the first available seat
			return availableSeats.First();
		}

		private bool IsNearToAny(string seat, IEnumerable<string> targetSeats)
		{
			// Simple string matching heuristic assuming format like "1A", "1B"
			// Extracts numerical row to check proximity
			if (int.TryParse(new string(seat.Where(char.IsDigit).ToArray()), out int currentRow))
			{
				foreach (var target in targetSeats)
				{
					if (int.TryParse(new string(target.Where(char.IsDigit).ToArray()), out int targetRow))
					{
						if (Math.Abs(currentRow - targetRow) <= 1) return true; // Adjacent or same row
					}
				}
			}
			return false;
		}
	}
}