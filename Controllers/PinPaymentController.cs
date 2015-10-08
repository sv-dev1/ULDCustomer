using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Xml;
using System.Xml.Serialization;
using ULDCustomer.Models;
using ULDCustomer.Models.ViewModel;
using ULDCustomer.Models.Utilities;
using System.Configuration;
using System.Web.UI.WebControls;
using System.Web.Routing;


namespace ULDCustomer.Controllers
{
    //[RequireHttps]

    public class PinPaymentController : Controller
    {
        #region
        //Variable declaration
        WebRequest myReq = null;
        CredentialCache mycache = null;
        Stream requestStream = null;
        WebResponse wr = null;
        Stream receiveStream = null;
        StreamReader reader = null;
        List<Plans> planlist = new List<Plans>();
        XmlSerializer serializer = null;
        StringReader rdr = null;
        subscriptionplans _subscriptionplans = null;
        XmlDocument xmlDoc = null;
        string xml = "";
        string site = "";
        string url = "";
        byte[] bytes;
        string credentials = ConfigurationManager.AppSettings["apiToken"].ToString();
        #endregion
        #region
        //Action methods
        public ActionResult Index()
        {

            //return View("Plans", GetPlans());
            return View("Plans", DBCommon.GetPlans());

        }


        public ActionResult xposeSubscriber(string url, string xml, string method)
        {
            try
            {
                myReq = WebRequest.Create(url);
                mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                myReq.ContentType = "Application/json;";
                if (method.ToLower() == "post")
                {
                    byte[] bytes;
                    myReq.ContentType = "text/xml; encoding='utf-8'";
                    bytes = System.Text.Encoding.ASCII.GetBytes(xml);
                    myReq.ContentLength = bytes.Length;
                    myReq.Method = method;
                    requestStream = myReq.GetRequestStream();
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                }
                wr = myReq.GetResponse();
                receiveStream = wr.GetResponseStream();
                reader = new StreamReader(receiveStream, Encoding.UTF8);
                xmlDoc = new XmlDocument();
                xmlDoc.Load(wr.GetResponseStream());
                serializer = new XmlSerializer(typeof(subscriptionplans));
                rdr = new StringReader(xmlDoc.InnerXml);
                _subscriptionplans = (subscriptionplans)serializer.Deserialize(rdr);
                foreach (var item in _subscriptionplans.subscriptionplan)
                {
                    Plans plans = new Plans();
                    plans.Amount = item.amount.Value;
                    plans.Name = item.name;
                    plans.Type = item.plantype;
                    plans.Id = item.id.Value;
                    plans.ServiceLevel = item.featurelevel;
                    planlist.Add(plans);
                }
                string content = reader.ReadToEnd();

                return View("Plans", planlist);
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        //[Route("{id}")]
        public ActionResult CreateSubscriber(string id, int SurveyID)
        {
            try
            {

                List<Plans> plans = DBCommon.GetPlans();
                var f = plans.FirstOrDefault(x => x.Name == id);
                SiteSubscriber obj = new SiteSubscriber();
                obj.SubscriptionId = f.Id.ToString();
                obj.ULDSurveyID = SurveyID;
                //obj.ServiceLevel = f.ServiceLevel.ToString();
                return View("CreateSubscriber", obj);
            }
            catch
            {
                return null;
            }
        }
        [HttpPost]
        //[Route("{id}")]
        public ActionResult CreateSubscriber(SiteSubscriber obj)
        {
            if (ModelState.IsValid)
            {
                Guid guid = new Guid();
                Random rnmd = new Random();
                ULDEntities db = new ULDEntities();
                if (ModelState.IsValid)
                {
                    ULDCustomer.Models.ULDCustomer obtbl = new ULDCustomer.Models.ULDCustomer();
                    CustomerModel model = new CustomerModel();
                    int cutomerid = model.AddCustomer(obj);
                    xml = "<subscriber><customer-id>" + cutomerid + "</customer-id><screen-name>" + obj.FirstName + obj.LastName + "</screen-name></subscriber>";
                    site = ConfigurationManager.AppSettings["apiUrl"].ToString();
                    url = string.Format("https://subs.pinpayments.com/api/v4/{0}/subscribers.xml", site);
                    CreateSubscriberApi(url, xml, "Post");
                    CardDetail obj1 = new CardDetail();

                    obj1.token = GenrateInvoice(obj.SubscriptionId, cutomerid.ToString(), obj.FirstName, obj.Email);
                    obj1.firstName = obj.FirstName;
                    obj1.lastName = obj.LastName;
                    obj1.fullName = obj.FirstName + " " + obj.LastName;
                    var planDetail = DBCommon.GetPlanDetailByProductID(obj.SubscriptionId);
                    obj1.title = planDetail.Rows[0]["title"].ToString();
                    obj1.CustomerId = cutomerid;
                    obj1.description = planDetail.Rows[0]["description"].ToString();                    
                    obj1.price = planDetail.Rows[0]["price"].ToString();
                    ViewBag.year = DBCommon.BindYear();
                    ViewBag.month = DBCommon.BindMonth();
                    TempData["CardDetail"] = obj1;
                    TempData["email"] = obj.Email;
                    TempData["PrimaryEmail"] = obj.Email;
                    //   return RedirectToAction("AddCardDetail", new { id = cutomerid });
                    return RedirectToAction("AddCardDetail");
                }
                return View("CreateSubscriber");
            }
            else
            {
                return View(obj);
            }

        }

        public ActionResult AddCardDetail()
        {

            CardDetail tmpObj = (CardDetail)TempData["CardDetail"];
            TempData["CardDetail"] = tmpObj;

            if (ModelState.IsValid)
            {
                CardDetail obj = new CardDetail();
                obj.token = tmpObj.token;
                obj.firstName = tmpObj.firstName;
                obj.lastName = tmpObj.lastName;
                obj.fullName = tmpObj.firstName + " " + tmpObj.lastName;
                obj.title = tmpObj.title;
                obj.description = tmpObj.description;
                obj.chargeFrequency = tmpObj.chargeFrequency;
                obj.CustomerId = tmpObj.CustomerId;
                obj.price = tmpObj.price;
                ViewBag.year = DBCommon.BindYear();
                ViewBag.month = DBCommon.BindMonth();
                ViewBag.CountryList = GetCountryList();
                //ViewBag.CustomerId = id;
                //obj.CustomerId = id;   
                ViewBag.id = obj.CustomerId;
                ViewBag.email = TempData["email"];
                ViewBag.name = obj.fullName;
                ViewBag.date = DateTime.Now;
                return View(obj);
            }
            else
            {
                return View();
            }

        }

        [HttpPost]
        public ActionResult AddCardDetail(CardDetail obj)
        {
            if (ModelState.IsValid)
            {
                obj.accountType = "credit-card";
                if (obj.cardNumber.StartsWith("4"))
                {
                    obj.cardType = "visa";
                }
                else if (obj.cardNumber.StartsWith("51") || obj.cardNumber.StartsWith("52") || obj.cardNumber.StartsWith("53") || obj.cardNumber.StartsWith("54") || obj.cardNumber.StartsWith("55"))
                {
                    obj.cardType = "master";
                }
                else if (obj.cardNumber.StartsWith("6011") || obj.cardNumber.StartsWith("65"))
                {
                    obj.cardType = "discover";
                }
                else if (obj.cardNumber.StartsWith("34") || obj.cardNumber.StartsWith("37"))
                {
                    obj.cardType = "american_express";
                }
                try
                {
                    //xml = "<payment>  <account-type>" + obj.accountType + "</account-type><credit-card><number>" + obj.cardNumber + "</number><card-type>" + obj.cardType.Trim() + "</card-type><verification-value>" + obj.verificationValue + "</verification-value><month>" + obj.month + "</month><year>" + obj.year + "</year> <first-name>" + obj.firstName + "</first-name><last-name>" + obj.lastName + "</last-name><address1>" + obj.BillingInfo.Address + "</address1><city>" + obj.BillingInfo.City + "</city><state>" + obj.BillingInfo.State + "</state><zip>" + obj.BillingInfo.Zip + "</zip><country>" + obj.BillingInfo.Country + "</country><phone-number>480-225-8217</phone-number></credit-card></payment>";
                    //xml = "<payment><account-type>credit-card</account-type><credit-card><number>" + obj.cardNumber + "</number><card-type>" + Request.Form["CardType"].ToString() + "</card-type><verification-Value>" + obj.verificationValue + "</verification-Value><month>" + obj.month + "</month><year>" + obj.year + "</year><first-name>" + obj.firstName + "</first-name><last-name>" + obj.lastName + "</last-name> <address1>" + obj.BillingInfo.Address + "</address1><city>" + obj.BillingInfo.City + "</city><state>" + obj.BillingInfo.State + "</state><zip>" + obj.BillingInfo.Zip + "</zip><country>"+obj.BillingInfo.Country+"</country></credit-card></payment>";
                    xml = "<payment>  <account-type>credit-card</account-type><credit-card><number>4222222222222</number><card-type>visa</card-type><verification-value>234</verification-value><month>1</month><year>2018</year> <first-name>" + obj.firstName + "</first-name><last-name>" + obj.lastName + "</last-name></credit-card></payment>";
                    site = ConfigurationManager.AppSettings["apiUrl"].ToString();
                    url = string.Format("https://subs.pinpayments.com/api/v4/{0}/invoices/", site);
                    string ErrorResponse = Payment(url + obj.token + "/pay.xml", xml, "PUT", obj.token);
                    var statuscode = ErrorResponse.Split('-')[0];
                    string message = ErrorResponse.Split('-')[1];
                    switch (statuscode)
                    {
                        case "404":
                            ModelState.AddModelError("PaymentError", message);
                            //ViewBag.PlanTitle = obj.title;
                            //ViewBag.description = obj.description;
                            //ViewBag.ChargeFrequency = obj.chargeFrequency;
                            //ViewBag.Price = obj.price;
                            ViewBag.year = DBCommon.BindYear();
                            ViewBag.month = DBCommon.BindMonth();
                            ViewBag.CountryList = GetCountryList();
                            return View(obj);
                        case "422":
                            ModelState.AddModelError("PaymentError", message);
                            //ViewBag.PlanTitle = obj.title;
                            //ViewBag.description = obj.description;
                            //ViewBag.ChargeFrequency = obj.chargeFrequency;
                            //ViewBag.Price = obj.price;
                            ViewBag.year = DBCommon.BindYear();
                            ViewBag.month = DBCommon.BindMonth();
                            ViewBag.CountryList = GetCountryList();
                            return View(obj);
                        case "403":
                            ModelState.AddModelError("PaymentError", message);
                            //ViewBag.PlanTitle = obj.title;
                            //ViewBag.description = obj.description;
                            //ViewBag.ChargeFrequency = obj.chargeFrequency;
                            //ViewBag.Price = obj.price;
                            ViewBag.year = DBCommon.BindYear();
                            ViewBag.month = DBCommon.BindMonth();
                            ViewBag.CountryList = GetCountryList();
                            return View(obj);
                        case "503":
                            ModelState.AddModelError("PaymentError", message);
                            //ViewBag.PlanTitle = obj.title;
                            //ViewBag.description = obj.description;
                            //ViewBag.ChargeFrequency = obj.chargeFrequency;
                            //ViewBag.Price = obj.price;
                            ViewBag.year = DBCommon.BindYear();
                            ViewBag.month = DBCommon.BindMonth();
                            ViewBag.CountryList = GetCountryList();
                            return View(obj);
                        case "NotFound":
                            ModelState.AddModelError("PaymentError", "Plan is not exists in database");
                            //ViewBag.PlanTitle = obj.title;
                            //ViewBag.description = obj.description;
                            //ViewBag.ChargeFrequency = obj.chargeFrequency;
                            //ViewBag.Price = obj.price;
                            ViewBag.year = DBCommon.BindYear();
                            ViewBag.month = DBCommon.BindMonth();
                            ViewBag.CountryList = GetCountryList();
                            message = "Plan is not exists in database";
                            return View(obj);
                    }
                    if (statuscode == "404" || statuscode == "422" || statuscode == "422" || statuscode == "403")
                    {
                        ModelState.AddModelError("PaymentError", message);
                    }

                    CustomerModel model = new CustomerModel();
                    model.ValidateCustomerID(obj.CustomerId);
                    ViewBag.id = obj.CustomerId;
                    ViewBag.email = TempData["PrimaryEmail"];
                    return RedirectToAction("ThankYou");
                }
                catch (WebException ex)
                {
                    return View("PaymentFail");
                }
            }
            else
            {
                ViewBag.year = DBCommon.BindYear();
                ViewBag.month = DBCommon.BindMonth();
                //ViewBag.PlanTitle = obj.title;
                //ViewBag.description = obj.description;
                //ViewBag.ChargeFrequency = obj.chargeFrequency;
                //ViewBag.Price = obj.price;
                ViewBag.CountryList = GetCountryList();
                return View();
            }
        }
        public ActionResult IsEmailExist(string Email)
        {
            CustomerModel cust = new CustomerModel();
            return Json(cust.IsEmailExist(Email), JsonRequestBehavior.AllowGet);
        }

        public ActionResult ThankYou()
        {
            return View();
        }

        public SelectList GetCountryList()
        {
            SelectList slSound;
            var listItems = new List<Country>();
            listItems.Add(new Country { Name = "Argentina", Value = "AR" });
            listItems.Add(new Country { Name = "Australia", Value = "AU" });
            listItems.Add(new Country { Name = "Austria", Value = "AT" });
            listItems.Add(new Country { Name = "Belarus", Value = "BY" });
            listItems.Add(new Country { Name = "Belgium", Value = "BE" });
            listItems.Add(new Country { Name = "Bosnia and Herzegovina", Value = "BA" });
            listItems.Add(new Country { Name = "Brazil", Value = "BR" });
            listItems.Add(new Country { Name = "Bulgaria", Value = "BG" });
            listItems.Add(new Country { Name = "Canada", Value = "CA" });
            listItems.Add(new Country { Name = "Chile", Value = "CL" });
            listItems.Add(new Country { Name = "China", Value = "CN" });
            listItems.Add(new Country { Name = "Colombia", Value = "CO" });
            listItems.Add(new Country { Name = "Costa Rica", Value = "CR" });
            listItems.Add(new Country { Name = "Croatia", Value = "HR" });
            listItems.Add(new Country { Name = "Cuba", Value = "CU" });
            listItems.Add(new Country { Name = "Cyprus", Value = "CY" });
            listItems.Add(new Country { Name = "Czech Republic", Value = "CZ" });
            listItems.Add(new Country { Name = "Denmark", Value = "DK" });
            listItems.Add(new Country { Name = "Dominican Republic", Value = "DO" });
            listItems.Add(new Country { Name = "Egypt", Value = "EG" });
            listItems.Add(new Country { Name = "Estonia", Value = "EE" });
            listItems.Add(new Country { Name = "Finland", Value = "FI" });
            listItems.Add(new Country { Name = "France", Value = "FR" });
            listItems.Add(new Country { Name = "Georgia", Value = "GE" });
            listItems.Add(new Country { Name = "Germany", Value = "DE" });
            listItems.Add(new Country { Name = "Gibraltar", Value = "GI" });
            listItems.Add(new Country { Name = "Greece", Value = "GR" });
            listItems.Add(new Country { Name = "Hong Kong S.A.R., China", Value = "HK" });
            listItems.Add(new Country { Name = "Hungary", Value = "HU" });
            listItems.Add(new Country { Name = "Iceland", Value = "IS" });
            listItems.Add(new Country { Name = "India", Value = "IN" });
            listItems.Add(new Country { Name = "Indonesia", Value = "ID" });
            listItems.Add(new Country { Name = "Iran", Value = "IR" });
            listItems.Add(new Country { Name = "Iraq", Value = "IQ" });
            listItems.Add(new Country { Name = "Ireland", Value = "IE" });
            listItems.Add(new Country { Name = "Israel", Value = "IL" });
            listItems.Add(new Country { Name = "Italy", Value = "IT" });
            listItems.Add(new Country { Name = "Jamaica", Value = "JM" });
            listItems.Add(new Country { Name = "Japan", Value = "JP" });
            listItems.Add(new Country { Name = "Kazakhstan", Value = "KZ" });
            listItems.Add(new Country { Name = "Kuwait", Value = "KW" });
            listItems.Add(new Country { Name = "Kyrgyzstan", Value = "KG" });
            listItems.Add(new Country { Name = "Laos", Value = "LA" });
            listItems.Add(new Country { Name = "Latvia", Value = "LV" });
            listItems.Add(new Country { Name = "Lebanon", Value = "LB" });
            listItems.Add(new Country { Name = "Lithuania", Value = "LT" });
            listItems.Add(new Country { Name = "Luxembourg", Value = "LU" });
            listItems.Add(new Country { Name = "Macedonia", Value = "MK" });
            listItems.Add(new Country { Name = "Malaysia", Value = "MY" });
            listItems.Add(new Country { Name = "Malta", Value = "MT" });
            listItems.Add(new Country { Name = "Mexico", Value = "MX" });
            listItems.Add(new Country { Name = "Moldova", Value = "MD" });
            listItems.Add(new Country { Name = "Monaco", Value = "MC" });
            listItems.Add(new Country { Name = "Montenegro", Value = "ME" });
            listItems.Add(new Country { Name = "Morocco", Value = "MA" });
            listItems.Add(new Country { Name = "Netherlands", Value = "NL" });
            listItems.Add(new Country { Name = "New Zealand", Value = "NZ" });
            listItems.Add(new Country { Name = "Nicaragua", Value = "NI" });
            listItems.Add(new Country { Name = "North Korea", Value = "KP" });
            listItems.Add(new Country { Name = "Norway", Value = "NO" });
            listItems.Add(new Country { Name = "Pakistan", Value = "PK" });
            listItems.Add(new Country { Name = "Palestinian Territory", Value = "PS" });
            listItems.Add(new Country { Name = "Peru", Value = "PE" });
            listItems.Add(new Country { Name = "Philippines", Value = "PH" });
            listItems.Add(new Country { Name = "Poland", Value = "PL" });
            listItems.Add(new Country { Name = "Portugal", Value = "PT" });
            listItems.Add(new Country { Name = "Puerto Rico", Value = "PR" });
            listItems.Add(new Country { Name = "Qatar", Value = "QA" });
            listItems.Add(new Country { Name = "Romania", Value = "RO" });
            listItems.Add(new Country { Name = "Russia", Value = "RU" });
            listItems.Add(new Country { Name = "Saudi Arabia", Value = "SA" });
            listItems.Add(new Country { Name = "Serbia", Value = "RS" });
            listItems.Add(new Country { Name = "Singapore", Value = "SG" });
            listItems.Add(new Country { Name = " Slovakia ", Value = "SK" });
            listItems.Add(new Country { Name = " Slovenia ", Value = "SI" });
            listItems.Add(new Country { Name = " South Africa ", Value = "ZA" });
            listItems.Add(new Country { Name = " South Korea ", Value = "KR" });
            listItems.Add(new Country { Name = " Spain ", Value = "ES" });
            listItems.Add(new Country { Name = " Sri Lanka ", Value = "LK" });
            listItems.Add(new Country { Name = " Sweden ", Value = "SE" });
            listItems.Add(new Country { Name = " Switzerland ", Value = "CH" });
            listItems.Add(new Country { Name = " Taiwan ", Value = "TW" });
            listItems.Add(new Country { Name = " Thailand ", Value = "TH" });
            listItems.Add(new Country { Name = " Tunisia ", Value = "TN" });
            listItems.Add(new Country { Name = " Turkey ", Value = "TR" });
            listItems.Add(new Country { Name = " Ukraine ", Value = "UA" });
            listItems.Add(new Country { Name = " United Arab Emirates ", Value = "AE" });
            listItems.Add(new Country { Name = " United Kingdom ", Value = "GB" });
            listItems.Add(new Country { Name = " USA ", Value = "US" });
            listItems.Add(new Country { Name = " Uzbekistan ", Value = "UZ" });
            listItems.Add(new Country { Name = " Vietnam", Value = "VN" });
            slSound = new SelectList(listItems, "Value", "Name", "US");
            return slSound;
        }
        public class Country
        {
            public string Name { get; set; }
            public string Value { get; set; }
        }
        #endregion
        #region
        //api methods   
        public string CreateSubscriberApi(string url, string xml, string method)
        {
            try
            {
                myReq = WebRequest.Create(url);
                mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                myReq.ContentType = "text/xml; encoding='utf-8'";
                bytes = System.Text.Encoding.ASCII.GetBytes(xml);
                myReq.ContentLength = bytes.Length;
                myReq.Method = method;
                requestStream = myReq.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                wr = myReq.GetResponse();
                receiveStream = wr.GetResponseStream();
                reader = new StreamReader(receiveStream, Encoding.UTF8);
                return reader.ReadToEnd(); ;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string Payment(string url, string xml, string method, string token)
        {
            try
            {
                myReq = WebRequest.Create(url);
                mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                myReq.ContentType = "text/xml; encoding='utf-8'";
                bytes = System.Text.Encoding.ASCII.GetBytes(xml);
                myReq.ContentLength = bytes.Length;
                myReq.Method = method;

                requestStream = myReq.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                wr = (HttpWebResponse)myReq.GetResponse();
                wr = myReq.GetResponse();
                receiveStream = wr.GetResponseStream();

                reader = new StreamReader(receiveStream, Encoding.UTF8);
                return reader.ReadToEnd();
            }
            catch (WebException we)
            {
                Stream dataStream = we.Response.GetResponseStream();
                XmlDocument locRequest = new XmlDocument();
                if (dataStream.Length > 0)
                {
                    locRequest.Load(dataStream);
                }
                //var resp = new StreamReader(we.Response.GetResponseStream()).ReadToEnd();
                HttpStatusCode statuscode = new HttpStatusCode();
                statuscode = ((HttpWebResponse)we.Response).StatusCode;
                var ErrorMessage = locRequest.InnerText.ToString();

                return statuscode.ToString() + "-" + ErrorMessage;
            }
        }
        public string GenrateInvoice(string subsciptionid, string customerId, string screenName, string email)
        {
            try
            {
                xml = "<invoice><subscription-plan-id>" + subsciptionid + "</subscription-plan-id><subscriber><customer-id>" + customerId + "</customer-id><screen-name>" + screenName + "</screen-name><email>" + email + "</email></subscriber></invoice>";
                site = ConfigurationManager.AppSettings["apiUrl"].ToString();
                string url = string.Format("https://subs.pinpayments.com/api/v4/{0}/invoices.xml", site);
                myReq = WebRequest.Create(url);
                mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                myReq.ContentType = "text/xml; encoding='utf-8'";
                bytes = System.Text.Encoding.ASCII.GetBytes(xml);
                myReq.ContentLength = bytes.Length;
                myReq.Method = "POST";
                requestStream = myReq.GetRequestStream();
                requestStream.Write(bytes, 0, bytes.Length);
                requestStream.Close();
                wr = myReq.GetResponse();
                receiveStream = wr.GetResponseStream();
                reader = new StreamReader(receiveStream, Encoding.UTF8);
                serializer = new XmlSerializer(typeof(invoice));
                rdr = new StringReader(reader.ReadToEnd());
                invoice _invoice = (invoice)serializer.Deserialize(rdr);
                return _invoice.token;
            }
            catch (Exception ex)
            {
                return ex.Message;

            }
        }
        public IList<Plans> GetPlans()
        {
            try
            {
                string site = ConfigurationManager.AppSettings["apiUrl"].ToString();
                string url = string.Format("https://subs.pinpayments.com/api/v4/{0}/subscription_plans.xml", site);
                myReq = WebRequest.Create(url);
                mycache = new CredentialCache();
                myReq.Headers["Authorization"] = "Basic " + Convert.ToBase64String(Encoding.ASCII.GetBytes(credentials));
                myReq.ContentType = "Application/json;";
                myReq.Method = "Get";
                wr = myReq.GetResponse();
                receiveStream = wr.GetResponseStream();
                reader = new StreamReader(receiveStream, Encoding.UTF8);
                xmlDoc = new XmlDocument();
                xmlDoc.Load(wr.GetResponseStream());
                serializer = new XmlSerializer(typeof(subscriptionplans));
                rdr = new StringReader(xmlDoc.InnerXml);
                _subscriptionplans = new subscriptionplans();
                using (XmlReader reade = new XmlTextReader(rdr))
                {

                    var subscriptionplanssubscriptionplans = new XmlSerializer(typeof(subscriptionplans));
                    _subscriptionplans = subscriptionplanssubscriptionplans.Deserialize(reade) as subscriptionplans;
                    List<Plans> planlist = new List<Plans>();
                    foreach (var item in _subscriptionplans.subscriptionplan)
                    {
                        Plans plans = new Plans();
                        plans.Amount = item.amount.Value;
                        plans.Name = item.name;
                        plans.Type = item.plantype;
                        plans.Id = item.id.Value;
                        plans.ServiceLevel = item.featurelevel;
                        planlist.Add(plans);
                    }
                    return planlist;



                }
            }
            catch (Exception ex)
            {

                return null;
            }


        }
        #endregion
    }
}
