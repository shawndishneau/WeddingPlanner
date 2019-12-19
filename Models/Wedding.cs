using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WeddingPlanner.Models
{
    public class Wedding
    {
        [Key]
        public int WeddingId { get; set; }

        [Required(ErrorMessage="You need to have two people to be married")]
        public string WedderOne { get; set; }

        [Required(ErrorMessage="You need to have two people to be married")]
        public string WedderTwo { get; set; }

        [Required(ErrorMessage="Must be a future date")]
        [FutureDate(ErrorMessage="Must be a future date")]
        public DateTime WeddingDate { get; set; }

        [Required(ErrorMessage="You must enter an address")]
        public string WeddingAddress { get; set; }

        [Required(ErrorMessage="Please insert a city name")]
        [MinLength(3, ErrorMessage="Minimum length for a city name is 3 characters")]
        public string WeddingCity { get; set;}

        [Required(ErrorMessage="Please insert a state abbreviation")]
        [MaxLength(3, ErrorMessage="Please use state abbreviations for a state")]
        [MinLength(2, ErrorMessage="Please use state abbreviations for a state")]
        public string WeddingState { get; set;}

        [Required(ErrorMessage="Please insert a Zip Code")]
        [Range(0, 99999, ErrorMessage="A Zip Code needs to have 5 numbers")]
        public int? WeddingZipCode { get; set;}
        public int CreatorId { get; set; }
        public User Creator { get; set; }
        // *************************************************************************************************
        // used in .cshtml files and controller files
        public List<Association> GuestsAttending { get; set; }
        // *************************************************************************************************
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;
    }



    // This deletes the files if they are in the past as in the wedding has already happened
    public class FutureDateAttribute : ValidationAttribute {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime) value < DateTime.Now) 
            {
                return new ValidationResult ("The date entered is not a future date.");
            }
            return ValidationResult.Success;
        }
    }
}