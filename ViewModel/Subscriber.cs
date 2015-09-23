using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ULDCustomer.Models.ViewModel
{
    public class SiteSubscriber
    {
        public string CustmerId { get; set; }
        [Required]
    
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }

        [EmailAddress]
        [Required]
        [Remote("IsEmailExist", "PinPayment", ErrorMessage = "Email address already in use.<a href='https://client.mailmonitorapp.com/Forgot_Password' target='_blank'> Click here to retrieve password</a>")]
        public string Email { get; set; }

        [Display(Name = "Promo Code")]
        public string PromoCode { get; set; }


        [Display(Name = "Service Level")]
        public string ServiceLevel { get; set; }


        [Display(Name = "Campany Name")]
        public string Company { get; set; }

        public string SubscriptionId { get; set; }

        [Display(Name = "ULDSurvey ID")]
        public int ULDSurveyID { get; set; }
    }
}