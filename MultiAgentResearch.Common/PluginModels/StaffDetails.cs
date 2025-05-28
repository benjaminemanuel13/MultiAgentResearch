using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MultiAgentResearch.Common
{
    public class StaffDetails
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }

        public StaffDetails(string name, string position, string department, string email, string phoneNumber)
        {
            Name = name;
            Position = position;
            Department = department;
            Email = email;
            PhoneNumber = phoneNumber;
        }
        public override string ToString()
        {
            return $"{Name}, {Position}, {Department}, {Email}, {PhoneNumber}";
        }
    }
}
