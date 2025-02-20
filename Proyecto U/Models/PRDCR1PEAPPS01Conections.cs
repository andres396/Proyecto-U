using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace ICT.Models
{

    public class PRDCR1PEAPPS01Conections
    {
        //all variables 
        public static DataTable rawdata { get; private set; }


        //to get data from the treatment
        public static string AuthorUser { get; private set; }
        public static string Source { get; private set; }
        public static string Modification { get; private set; }
        public static int Priority { get; private set; }
        private static DateTime ValueDate { get; set; }
        // to get data from the DDT pool
        public static int Count { get; private set; }
        public static int Ticket { get; private set; }
        public static int PID { get; private set; }
        public static string Detail { get; private set; }
        public static string Impression { get; private set; }
        public static string Sender { get; private set; }
        public static bool Isduplicate { get; private set; }
        public static string qtyduplicates { get; private set; }




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
        //Load data from the selected PID
        public static void Load_SSrowData(string _Query)
        {
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        AuthorUser = reader.GetString(0);
                        Source = reader.GetString(1);
                        Modification = reader.GetString(2);
                        Priority = reader.GetInt32(3);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
        }
        //execute queries
        public static void Execute_Request(string _Query)
        {
            int retryCount = 3; // Número de intentos
            while (retryCount > 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Con))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(_Query, connection))
                        {
                            // Inicia la transacción
                            SqlTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                            command.Transaction = transaction;

                            try
                            {
                                // Ejecuta la consulta
                                _ = command.ExecuteNonQuery();

                                // Confirma la transacción si todo fue bien
                                transaction.Commit();
                            }
                            catch (SqlException)
                            {
                                // En caso de error, deshace la transacción
                                transaction.Rollback();
                                throw; // Relanza la excepción para que el retry la maneje
                            }
                        }
                        connection.Close();
                    }
                    break; // Si se ejecuta correctamente, sale del loop.
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Código de error para deadlock.
                    {
                        retryCount--; // Reduce el número de intentos.
                        if (retryCount == 0)
                        {
                            throw; // Relanza la excepción si se acabaron los intentos.
                        }
                        System.Threading.Thread.Sleep(1000); // Espera antes de reintentar.
                    }
                    else
                    {
                        throw; // Relanza la excepción si no es un deadlock.
                    }
                }
            }
        }

        /*public static void Execute_Request(string _Query)
        {
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    //command.CommandTimeout = 60;
                    _ = command.ExecuteNonQuery();
                    connection.Close();
                }
            }
        }*/
        //get the duration in ss pool
        public static string GetElapseTime(string _Query)
        {

            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    command.CommandTimeout = 60;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        ValueDate = reader.GetDateTime(0);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
            return DateTime.Now.Subtract(ValueDate).TotalMinutes.ToString(@"0");

        }
        //count the treatments
        public static void Count_DDTPoolConections(string _Query)
        {
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    command.CommandTimeout = 60;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Count = reader.GetInt32(0);
                    }
                    reader.Close();
                    connection.Close();
                }
            }
        }
        //read the of the data for a assigned treatment

        public static void Read_TreatmentAssigned(string _Query)
        {
            int retryCount = 3; // Number of attempts
            while (retryCount > 0)
            {
                try
                {
                    using (SqlConnection connection = new SqlConnection(Con))
                    {
                        connection.Open();
                        using (SqlCommand command = new SqlCommand(_Query, connection))
                        {
                            command.CommandTimeout = 60;

                            // Start the transaction with the isolation level of your choice (in this case, ReadCommitted).
                            SqlTransaction transaction = connection.BeginTransaction(System.Data.IsolationLevel.ReadCommitted);
                            command.Transaction = transaction;

                            try
                            {
                                SqlDataReader reader = command.ExecuteReader();
                                while (reader.Read())
                                {
                                    // Processing the results.
                                    Ticket = reader.GetInt32(0);
                                    PID = reader.GetInt32(1);
                                    Source = reader.GetString(2);
                                    Modification = reader.GetString(3);
                                    Detail = reader.GetString(4);
                                    AuthorUser = reader.GetString(6);
                                    Priority = reader.GetInt32(7);
                                    Sender = reader.GetString(8);
                                }
                                reader.Close();

                                // If all went well, the transaction is confirmed.
                                transaction.Commit();
                            }
                            catch (SqlException)
                            {
                                // If there is an error, the transaction is rolled back..
                                transaction.Rollback();
                                throw;
                            }
                        }
                        connection.Close();
                    }
                    break; // If successful, it exits the loop.
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 1205) // Error code for deadlock.
                    {
                        retryCount--; // Reduces the number of attempts.
                        if (retryCount == 0)
                        {
                            throw; // Relaunch the exception if all attempts have been exhausted.
                        }
                        System.Threading.Thread.Sleep(1000); // Wait before retrying.
                    }
                    else
                    {
                        throw; // Relaunch the exception if it is not a deadlock.
                    }
                }
            }
        }

        // *********************************************************CODIGO VIEJO QUE FUE REEMPLAZADO*********************************************************
        /*  public static void Read_TreatmentAssigned(string _Query)
          {
              using (SqlConnection connection = new SqlConnection(Con))
              {
                  using (SqlCommand command = new SqlCommand(_Query, connection))
                  {
                      command.CommandTimeout = 60;
                      connection.Open();

                      SqlDataReader reader = command.ExecuteReader();
                      while (reader.Read())
                      {

                          Ticket = reader.GetInt32(0);
                          PID = reader.GetInt32(1);
                          Source = reader.GetString(2);
                          Modification = reader.GetString(3);
                          Detail = reader.GetString(4);
                          //Impression = reader.GetString(5); *** removed because is not needed
                          AuthorUser = reader.GetString(6);
                          Priority = reader.GetInt32(7);
                          Sender = reader.GetString(8);

                      }
                      reader.Close();
                      connection.Close();
                  }
              }
          }*/
        // *********************************************************CODIGO VIEJO QUE FUE REEMPLAZADO*********************************************************
        public static void readDuplicates(string _Query)
        {
            using (SqlConnection connection = new SqlConnection(Con))
            {
                using (SqlCommand command = new SqlCommand(_Query, connection))
                {
                    connection.Open();
                    command.CommandTimeout = 60;
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        qtyduplicates = reader.GetInt32(0).ToString();
                    }

                    reader.Close();
                    connection.Close();
                    if (qtyduplicates == "0" || qtyduplicates == null || qtyduplicates == "")
                    {
                        Isduplicate = false;
                    }
                    else
                    {
                        Isduplicate = true;

                    }
                }
            }

        }
    }
}