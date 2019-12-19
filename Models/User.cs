using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required(ErrorMessage="Please insert a first name")]
        [MinLength(3, ErrorMessage="Minimum length for a first name is 3 characters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage="Please insert a last name")]
        [MinLength(3, ErrorMessage="Minimum length for a last name is 3 characters")]
        public string LastName { get; set; }

        [Required(ErrorMessage="Please insert an Email address")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage="Please insert a password")]
        [MinLengthAttribute(8, ErrorMessage="Password must be 8 characters of longer!")]
        public string Password { get; set; }

        [Required(ErrorMessage="Please confirm your password")]
        [DataType(DataType.Password)]
        [MinLength(8, ErrorMessage="Password must be 8 characters or longer!")]
        [Compare("Password")]
        [NotMapped]
        public string ConfirmPassword { get; set; }
        // **********************************************************************************************
        public List<Association> WeddingsAttending { get; set; }
        public List<Wedding> WeddingsCreated { get; set; }
        // *************************************************************************************************

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }


    public class LoginUser
    {
        [Required(ErrorMessage="Please provide a registered Email address")]
        public string LoginEmail { get; set; }

        [Required(ErrorMessage="Please provide a correct password")]
        public string LoginPassword { get; set; }
    }
}