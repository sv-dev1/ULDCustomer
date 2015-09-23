using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace ULDCustomer.Models.ViewModel
{
    public class CardDetail
    {
        //[Required]
        public string accountType { get; set; }
        [Required]
        public string cardNumber { get; set; }
        //[Required]
        public string cardType { get; set; }
        [Required]
        public string verificationValue { get; set; }
        [Required]
        public int month { get; set; }
        [Required]
        public int year { get; set; }
       [Required]
        public string firstName { get; set; }
        [Required]
        public string lastName { get; set; }

        //[Required]
        public string fullName { get; set; }

        public string token { get; set; }

        public string title { get; set; }

        public string description { get; set; }

        public string chargeFrequency { get; set; }

        public string price { get; set; }

        public BillingAddress BillingInfo { get; set; }

        ////newly implemented
        public int CustomerId { get; set; }
    }
    public class BillingAddress
    {
        public string Country { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
    }
}