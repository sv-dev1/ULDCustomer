using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ULDCustomer.Models.ULDQuestionService;
using ULDCustomer.Models;
using ULDCustomer.ViewModel;

namespace ULDCustomer.Controllers
{
    public class HomeController : Controller
    {
        ULDQuestionService _obj = new ULDQuestionService();
        public ActionResult Index()
        {
            int ID = 1;

            //Insert into ULDSurvey first time

            vmULDQuestion model=new vmULDQuestion();
            ULDSurvey _uldservey = new ULDSurvey();
             model.question= _obj.GetQuestionByID(ID);
            // model.SurveyID = _obj.AddServey();
            List<Answer> answer = new List<Answer>();
            answer.Add(new Answer { AnswerId = 1, AnswerText = "Answer A" });
            answer.Add(new Answer { AnswerId = 2, AnswerText = "Answer B" });
            answer.Add(new Answer { AnswerId = 3, AnswerText = "Answer C" });
            answer.Add(new Answer { AnswerId = 4, AnswerText = "Answer D" });
            model.Answers = answer;
            ViewBag.ID = ID;
            
            return View(model);
        }

        [HttpPost]
        public ActionResult Index(vmULDQuestion model)
        {
            try
            {
                //ID++;
                //string s = Request.Form["choices"].ToString();

                if (model.AnswerId == 1)
                {
                    model.answerText = "A";
                }
                else if (model.AnswerId == 2)
                {
                    model.answerText = "B";
                }
                else if (model.AnswerId == 3)
                {
                    model.answerText = "C";
                }
                else
                {
                    model.answerText = "D";
                }
                int res = _obj.AddQuestion(model);

                if (model.ULDQuestionID < 4)
                {
                    int QuestionID = model.ULDQuestionID + 1;
                    model.question = _obj.GetQuestionByID(QuestionID);
                    ViewBag.ID = QuestionID;
                    List<Answer> answer = new List<Answer>();
                    answer.Add(new Answer { AnswerId = 1, AnswerText = "Answer A" });
                    answer.Add(new Answer { AnswerId = 2, AnswerText = "Answer B" });
                    answer.Add(new Answer { AnswerId = 3, AnswerText = "Answer C" });
                    answer.Add(new Answer { AnswerId = 4, AnswerText = "Answer D" });
                    model.Answers = answer;
                    return View(model);
                }
                return RedirectToAction("Index","PinPayment");
            }
            catch (Exception ex)
            { 

            }
            return View();            
            
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
