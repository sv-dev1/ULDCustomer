using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ULDCustomer.ViewModel
{
    public class vmULDQuestion
    {
        public int ULDQuestionID { get; set; }
        public string question { get; set; }
        public int AnswerId { get; set; }
        public int SurveyID { get; set; }
        public string answerText { get; set; }
        public List<Answer> Answers { get; set; }

        
    }
    public class Answer
    {
        public int  AnswerId { get; set; }
        public string AnswerText { get; set; }
    }
    public enum EnumAnswer
    {
        A = 1,
        B = 2,
        C = 3,
        D = 4
    }

}