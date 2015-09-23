using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ULDCustomer.ViewModel;

namespace ULDCustomer.Models.ULDQuestionService
{
    public class ULDQuestionService
    {
        ULDEntities _db = new ULDEntities();
        public string GetQuestionByID(int ID)
        {   
            var Question="";
            try
            {
              Question = _db.uld_SurveyQuestionGet(ID).FirstOrDefault();               
              return Question;
            }
            catch (Exception ex)
            {
 
            }
            return Question;
        }

        public int AddQuestion(vmULDQuestion obj)
        {
            int res=0;

            try
            {
                res = _db.uld_SurveyAnswerInsert(obj.ULDQuestionID, obj.SurveyID, obj.answerText);                             
                return res;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }
        public int AddServey()
        {
            decimal? DecSurveyID = 0;
            int SurveyID = 0;
            try
            {
                DecSurveyID = _db.uld_SurveyCreate().FirstOrDefault();                
                SurveyID = Convert.ToInt32(DecSurveyID);
                return SurveyID;
            }
            catch (Exception ex)
            {

            }
            return 0;
        }
        
    }
}