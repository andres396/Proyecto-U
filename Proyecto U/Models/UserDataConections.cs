using ICT.ActiveDir;
using ICT.Controllers;
using Microsoft.Ajax.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ICT.Models
{
    public class UserDataConections
    {
        //Define public Variables 
        public static UserActiveDirectory UserData { get; private set; }
        public static bool ValidUser { get; private set; }
        public static string Badge { get; private set; }
        public static string Supervisor { get; private set; }
        public static string Name { get; private set; }
        public static string Site { get; private set; }
        public static string Shift { get; private set; }
        public static string Cell { get; private set; }
        public static DataTable UserCells { get; private set; }

        const string Con = "yourDataBase Connection";

        //Load the information in active Directory if the user exist
        public static void Autenticated_User(string _Username)
        {
            //Define local Variables
            ActiveDirectory ActiveDirectoryValidation = new ActiveDirectory();
            Array Domains;
            int X;
            int Z;
            //timeout expiration
            ActiveDirectoryValidation.Timeout = 6000;
            //Get all domains
            Domains = ActiveDirectoryValidation.GetDomainList().Split('|');
            Z = Domains.Length;
            X = 0;

            //Authenticate if the user exist in active directory
            do
            {
                UserData = ActiveDirectoryValidation.GetUserByLogin(_Username, Domains.GetValue(X).ToString());
                try
                {
                    if (UserData != null)
                    {
                        X = Z;
                        ValidUser = true;
                        break;
                    }
                    else
                    {
                        X += 1;
                        ValidUser = false;
                    }
                }
                catch
                {
                    X += 1;
                    ValidUser = false;
                }
            } while (Z >= X);

        }

        public static void User_Data(string _Query)
        {
            //reset variables
            Badge = null;
            Supervisor = null;
            Name = null;
            Site = null;

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
                        Badge = reader.GetInt32(0).ToString();
                        Supervisor = reader.GetString(1);
                        Name = reader.GetString(2);
                        Site = reader.GetString(3);
                        Shift = reader.GetString(4);
                        Cell = reader.GetString(5);

                    }
                    reader.Close();
                    connection.Close();
                }
            }



        }

        public static void Load_User_Cells(string _Query)
        {
            UserCells = new DataTable();

            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();

                    using (SqlDataAdapter source = new SqlDataAdapter(_Query, connection))
                    {

                        source.Fill(UserCells);

                    }

                    connection.Close();
                }
            }
        }

    }
}