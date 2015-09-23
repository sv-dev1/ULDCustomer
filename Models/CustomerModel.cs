using ULDCustomer.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ULDCustomer.Models
{
    public class CustomerModel
    {
        public int AddCustomer(SiteSubscriber customer)
        {
            int id = 0;
            string connectionString = ConfigurationManager.ConnectionStrings["con"].ToString();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CustomerInsert", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add("@ULDSurveyID", SqlDbType.VarChar).Value = customer.ULDSurveyID;
                        cmd.Parameters.Add("@firstName", SqlDbType.VarChar).Value = customer.FirstName;
                        cmd.Parameters.Add("@lastName", SqlDbType.VarChar).Value = customer.LastName;
                        cmd.Parameters.Add("@email", SqlDbType.VarChar).Value = customer.Email;
                        cmd.Parameters.Add("@ProductIDPurchased", SqlDbType.VarChar).Value = customer.SubscriptionId;
                        con.Open();
                        //var dd = cmd.ExecuteNonQuery();
                        //var idd = cmd.Parameters["@newCustomerID"].Value;
                        var d = cmd.ExecuteScalar();
                        id = Convert.ToInt32(d);
                    }
                }
            }
            catch { }
            return id;
        }

        public bool IsEmailExist(string Email)
        {
            string connectionString = ConfigurationManager.ConnectionStrings["con"].ToString();
            DataTable table = new DataTable("allPrograms");
            using (SqlConnection conn = new SqlConnection(connectionString))
            {

                string command = "select * from ULDCustomer where email=@Email";
                using (SqlCommand cmd = new SqlCommand(command, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", Email);

                    conn.Open();
                    var dd = cmd.ExecuteScalar();
                    if (dd != null)
                    {
                        return false;
                    }
                    conn.Close();
                }
            }

            return true;
        }


        public void ValidateCustomerID(int id) 
        {
        string connectionString = ConfigurationManager.ConnectionStrings["con"].ToString();
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand("sp_CustomerValidateByCustomerID", con))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("@customerID", SqlDbType.VarChar).Value = id;
         

                        con.Open();

                        var d = cmd.ExecuteScalar();
                   //    return id = Convert.ToInt32(d);
                        con.Close();
                    }
                }
            }
            catch { }
        } 
    }
}