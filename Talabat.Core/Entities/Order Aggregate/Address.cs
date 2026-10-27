using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Talabat.Core.Entities.Order_Aggregate
{
	// Not Mapped
	// Address & Order 1 to 1 Total l 2
   public class Address
    {
		public Address()
		{
			
		}
		public Address(string firstName, string lastName, string street, string city, string country, string phoneNumber = "")
		{
			FirstName = firstName;
			LastName = lastName;
			Street = street;
			City = city;
			Country = country;
			PhoneNumber = phoneNumber;
		}

		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string Street { get; set; }
		public string City { get; set; }
		public string Country { get; set; }
		public string PhoneNumber { get; set; } = string.Empty;
	}
}
