using ULDCustomer.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data;
using ULDCustomer.Models.ViewModel;

namespace ULDCustomer.Models.Utilities
{
    public static class DBCommon
    {
        public static SelectList BindYear()
        {
            List<Year> userServerModel = new List<Year>();
            //DDAcType.Add(new AccountType { ID = 0, NameEnglish = "Please select" });


            SelectList slSound;

            Year[] m = new Year[16] {new Year(){ExpiryYear ="2015"},new Year(){ExpiryYear ="2016"},
            new Year(){ExpiryYear ="2017"},new Year(){ExpiryYear ="2018"},new Year(){ExpiryYear ="2019"},
            new Year(){ExpiryYear ="2020"},new Year(){ExpiryYear ="2021"},new Year(){ExpiryYear ="2022"},
            new Year(){ExpiryYear ="2023"},new Year(){ExpiryYear ="2024"},new Year(){ExpiryYear ="2025"},
            new Year(){ExpiryYear ="2026"},new Year(){ExpiryYear ="2027"},new Year(){ExpiryYear ="2028"},
            new Year(){ExpiryYear ="2029"},new Year(){ExpiryYear ="2030"}
            };

            userServerModel.AddRange(m);


            //slDialStatus = new SelectList(dialStatusModel, "ID", "DialStatus");
            slSound = new SelectList(userServerModel, "ExpiryYear", "ExpiryYear");


            return slSound;
        }

        public static SelectList BindMonth()
        {
            List<Month> userServerModel = new List<Month>();
            //DDAcType.Add(new AccountType { ID = 0, NameEnglish = "Please select" });


            SelectList slSound;

            Month[] m = new Month[12]{new Month(){ExpiryMonth="1"},
           new Month(){ExpiryMonth="2"},new Month(){ExpiryMonth="3"},new Month(){ExpiryMonth="4"},
           new Month(){ExpiryMonth="5"},new Month(){ExpiryMonth="6"},new Month(){ExpiryMonth="7"},new Month(){ExpiryMonth="8"},
           new Month(){ExpiryMonth="9"},new Month(){ExpiryMonth="10"},new Month(){ExpiryMonth="11"},new Month(){ExpiryMonth="12"}};



            userServerModel.AddRange(m);


            //slDialStatus = new SelectList(dialStatusModel, "ID", "DialStatus");
            slSound = new SelectList(userServerModel, "ExpiryMonth", "ExpiryMonth");


            return slSound;
        }

        public static List<Plans> GetPlans()
        {
            Plans plan = new Plans();
            List<Plans> plans = new List<Plans>();
            string connectionString = ConfigurationManager.ConnectionStrings["con"].ToString();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_GetPlans", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        SqlDataAdapter adp = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        adp.Fill(dt);
                        if (dt.Rows.Count > 0 && dt != null)
                        {
                            for (int i = 0; i <= dt.Rows.Count - 1; i++)
                            {
                                plan = new Plans();
                                plan.Id = (int)dt.Rows[i]["ULDProductID"];
                                plan.Amount = (decimal)dt.Rows[i]["price"];
                                plan.Name = dt.Rows[i]["url"].ToString();                                
                                plans.Add(plan);
                            }
                        }


                    }
                }
                return plans;
            }
            catch
            {
                return null;
            }

        }


        public static DataTable GetPlanDetailByProductID(string SubscriptionID)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["con"].ToString();
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                string command = "select * from ULDProduct where ULDProductID=@SubscriptionID";
                using (SqlCommand cmd = new SqlCommand(command, conn))
                {
                    cmd.Parameters.AddWithValue("@SubscriptionID", SubscriptionID);

                    conn.Open();
                    SqlDataAdapter adp = new SqlDataAdapter(cmd);

                    adp.Fill(dt);
                    if (dt.Rows.Count > 0 && dt != null)
                    {
                        return dt;
                    }
                    conn.Close();
                }
            }

            return dt;
        }
    }

    public class Month
    {
       public string ExpiryMonth { get; set; }
    }

    public class Year
    {
        public string ExpiryYear { get; set; }
    }
}