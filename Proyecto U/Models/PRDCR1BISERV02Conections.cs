using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Web;

namespace ICT.Models
{
    public class PRDCR1BISERV02Conections
    {

        //all variables
        public static DataTable rawdata;
       
        //this variables are for the author
        public static string AuthorArea { get; private set; }
        public static string AuthorName { get; private set; }
        public static string AuthorCell { get; private set; }
        public static string AuthorSupervisor { get; private set; }
        public static string AuthorStartDate { get; private set; }
        public static string AuthorUbication { get; private set; }
        //this variables are for the treament
        public static string TypeScan { get; private set; }
        public static string Recc { get; private set; }
        public static DateTime TagTime { get; private set; }
      


        const string Con = "yourDataBase Connection";
        //all conections
        //use to fill the fill the Raw Data 
        public static void Load_RawDataConexion(string _Query)
        {
            rawdata = new DataTable();
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    command.CommandTimeout = 60;
                    using (SqlDataAdapter source = new SqlDataAdapter(_Query, connection))
                    {
                        source.Fill(rawdata);
                    }
                    connection.Close();
                }
            }
        }

        public static void Read_AuthorDataConection(string _Query)
        {
            //make the connection
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    command.CommandTimeout = 60;
                    SqlDataReader reader = command.ExecuteReader();
                    //read the data extracted from the data base and write on the variables
                    while (reader.Read())
                    {
                        AuthorName = reader.GetString(0);
                        AuthorCell = reader.GetString(1);
                        AuthorSupervisor = reader.GetString(2);
                        AuthorStartDate = reader.GetDateTime(3).ToString();
                        AuthorUbication = reader.GetString(4);
                        AuthorArea = reader.GetString(5);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
        }

        public static void Read_TretmentDataConection(string _Query)
        {
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    try
                    {
                        connection.Open();
                        command.CommandTimeout = 60;
                        SqlDataReader reader = command.ExecuteReader();
                        while (reader.Read())
                        {
                            TypeScan = reader.GetString(0);
                            Recc = reader.GetString(1);
                            TagTime = reader.GetDateTime(2);
                        }
                        reader.Close();
                        connection.Close();
                    }
                    catch
                    {
                        TypeScan = "No Data";
                        Recc = "0";
                        TagTime = new DateTime();
                        TagTime = DateTime.Now;
                    }
                }
            }
            if (TypeScan == "" || TypeScan == null)
            {
                TypeScan = "No Data";
                Recc = "0";
                TagTime = new DateTime();
                TagTime = DateTime.Now;
            }
        }

        
        
    }
}