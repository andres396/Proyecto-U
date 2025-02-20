using System;
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using System.Linq;
using System.Web;
using ICT.ActiveDir;
using ICT.Models;

namespace ICT.Controllers
{
   
    public class UserData
    {
       
        public static bool Is_Valid_User(string _username)
        {
            //create the connetions whit active directory
            UserDataConections.Autenticated_User(_username);
            return UserDataConections.ValidUser;
        }

        public static string[] Get_User_Information(string _username)
        {
            //Create the container
            string[] UserData = new string[8];
            UserData[1] = UserDataConections.UserData.Title; //User Title
            //Load Data accord the position
            if (UserDataConections.UserData.Title.Contains("Supervisor"))
            {
                //load the data if is a supervisor
                UserDataConections.User_Data(@"
                                                        SELECT top 1
                                                               [BADGE]
                                                              ,[NAME]
                                                              ,[NAME]
                                                              ,[SITE_NAME]
                                                              ,[AREA]
                                                              ,[CELL]
                                                        FROM [DB_Pilots].[dbo].[Manpower_Supervisors_Staff] with(nolock)
                                                        Where LOGIN_NAME like '" + _username + @"'");
            }
            else if (UserDataConections.UserData.Title.Contains("Clinical"))
            {
                //load the data if is a Process in active directory
                UserDataConections.User_Data(@"
                                                        SELECT TOP 1 
	                                                           [BADGE]
	                                                          ,[SUPERVISOR]
                                                              ,[NAME] 
                                                              ,[SITE_NAME]
                                                              ,[AREA]
                                                              ,[CELL]
                                                        FROM [DB_Pilots].[dbo].[Manpower_Clinicalt_Staff] with(nolock)
                                                        Where LOGIN_NAME like '" + _username + @"'");
            }
            else if (UserDataConections.UserData.Title.Contains("Specialist") || UserDataConections.UserData.Title.Contains("-M") || UserDataConections.UserData.Title.Contains("-A") || UserDataConections.UserData.Title.Contains("Associate") || UserDataConections.UserData.Title.Contains("4") || UserDataConections.UserData.Title.Contains("5"))
            {
                //load the data if is a Process in active directory
                UserDataConections.User_Data(@"
                                                        SELECT TOP 1 
	                                                           [BADGE]
	                                                          ,[SUPERVISOR]
                                                              ,[NAME] 
                                                              ,[SITE_NAME]
                                                              ,[AREA]
                                                              ,[CELL]
                                                        FROM [DB_Pilots].[dbo].[Manpower_Specialists_Staff] with(nolock)
                                                        Where LOGIN_NAME like '" + _username + @"'");
        
            }
            else // if is a cad desgner
            {
                UserDataConections.User_Data(@"
                                                        SELECT top 1
                                                               cast( [BADGE] as int) as [BADGE]
                                                              ,[LEAD_NAME]
                                                              ,[NAME]
                                                              ,[SITE]
                                                              ,[SHIFT]
                                                              ,[CELL]
                                                          FROM [DB_Pilots].[dbo].[Manpower_From_OMS]   with(nolock) 
                                                          Where LOGIN like '" + _username + @"'");
               
            }
            UserData[2] = UserDataConections.Badge;
            if(UserData[2] == null)
            {
                UserDataConections.User_Data(@"
                                                        SELECT top 1
                                                               cast( [BADGE] as int) as [BADGE]
                                                              ,[LEAD_NAME]
                                                              ,[NAME]
                                                              ,[SITE]
                                                              ,[SHIFT]
                                                              ,[CELL]
                                                          FROM [DB_Pilots].[dbo].[Manpower_From_OMS]   with(nolock) 
                                                          Where LOGIN like '" + _username + @"'");
                UserData[2] = UserDataConections.Badge;
            }
            UserData[3] = UserDataConections.Supervisor;
            UserData[4] = UserDataConections.Name;
            UserData[5] = UserDataConections.Site;
            UserData[6] = UserDataConections.Shift;//AREA in case if is a support
            UserData[7] = UserDataConections.Cell;
            return UserData;
        }
        public static DataTable Cells(string _supervisor)
        {
            UserDataConections.Load_User_Cells(@"
SELECT distinct 
      CELL
	 ,REGION
FROM [DB_Pilots].[dbo].[Manpower_Supervisors_Staff] with(nolock)
Where name like '" + _supervisor+@"'
");
            return UserDataConections.UserCells;
        }

    }
}