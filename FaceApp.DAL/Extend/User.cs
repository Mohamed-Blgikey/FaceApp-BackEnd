using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FaceApp.DAL.Extend
{
    public class User:IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? FullName { get; set; }
        public bool Gender { get; set; } //1 man            0 woman
        public bool IsActive { get; set; }
        public DateTime LastActive { get; set; }
        public DateTime BirthDate { get; set; }
        public DateTime DateCreated { get; set; }

        public string? City { get; set; }


        public string? Country { get; set; }

    }
}
