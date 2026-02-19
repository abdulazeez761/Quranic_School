using System;
using System.Collections.Generic;

namespace Hafiz.DTOs
{
    public class ParentDto
    {
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string SecondName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int ChildrenCount { get; set; }
    }
}
