using ICT.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ICT.Controllers
{
    public class SSControl
    {

        public static void Load_RawData(string _cell)
        {
            PRDCR1PEAPPS01Conections.Load_RawDataConexion(@"
                        SELECT 
                       [TICKET]
                      ,cast ([Date] as smalldatetime) as [DATE]
	                  ,[PID]
                      ,[Send by user]     
                      ,[Rework Autor User]
                      ,[Autor CELL]
                      ,[Status]
                      ,[Inspector Name]
                       FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
                       where ([Cell] like '" + _cell + @"' and [Status] is null) or ([Cell] like '" + _cell + @"' and [Status]  not like  'Release') 
order by [TICKET] desc
");


            
        }

        public static void Send_IC(int _PID, string _Author, string _Source, int _Priority, string _Modification, string _Instruction, string _Username, string _Cell, string _Site, string _UserRegion, string _UserSupervisor, string _UserCompleteName, string _CaseType)

        {

            string AuthorArea;
            string AuthorName;
            string AuthorCell;
            string AuthorSupervisor;
            string AuthorStartDate;
            string AuthorUbication;
            //load the author data
            if (_Author.Contains("AMERICAS"))
            {
                AuthorArea = "AMERICAS";
                AuthorName = _Author.Replace("AMERICAS", "");
                AuthorCell = _Author.Replace("AMERICAS", "");
                AuthorSupervisor = _Author.Replace("AMERICAS", "");
                AuthorStartDate = DateTime.Now.ToShortDateString();
                AuthorUbication = _Site;
            }
            else if (_Author.Contains("EMEA"))
            {
                AuthorArea = "EMEA";
                AuthorName = _Author.Replace("EMEA", "");
                AuthorCell = _Author.Replace("EMEA", "");
                AuthorSupervisor = _Author.Replace("EMEA", "");
                AuthorStartDate = DateTime.Now.ToShortDateString();
                AuthorUbication = _Site;
            }
            else if (_Author.Contains("APAC"))
            {
                AuthorArea = "APAC";
                AuthorName = _Author.Replace("APAC", "");
                AuthorCell = _Author.Replace("APAC", "");
                AuthorSupervisor = _Author.Replace("APAC", "");
                AuthorStartDate = DateTime.Now.ToShortDateString();
                AuthorUbication = _Site;
            }
            else
            {
                PRDCR1BISERV02Conections.Read_AuthorDataConection(@"
                SELECT
                               [NAME]
                              ,[CELL]
                              ,[LEAD_NAME]
                              ,[START_DATE]
                              , concat([SITE], ' - ',[LOCATION]) as [SITE]
                              ,[AREA]
        FROM[DB_Pilots].[dbo].[Manpower_From_OMS] with(nolock)
        
                          WHERE[MES_CADCAM_LOGIN] like '" + _Author + "'"
                );
                AuthorArea = PRDCR1BISERV02Conections.AuthorArea;
                AuthorName = PRDCR1BISERV02Conections.AuthorName;
                AuthorCell = PRDCR1BISERV02Conections.AuthorCell;
                AuthorSupervisor = PRDCR1BISERV02Conections.AuthorSupervisor;
                AuthorStartDate = PRDCR1BISERV02Conections.AuthorStartDate;
                AuthorUbication = PRDCR1BISERV02Conections.AuthorUbication;
            }

            //load the scan type is is recc and tag time
            //* **remove because create a innecesary charge of Information
            //PRDCR1BISERV02Conections.Read_TretmentDataConection(
            //    @"SELECT DISTINCT
            //          A.TypeScan
            //         ,Case 
            //          When B.RCC = 1 THEN 'YES' ELSE 'NO'
            //          END AS RCC
            //         ,B.Tagg_CCModStartTime

            //          FROM [DB_Pilots].[dbo].[Cases_Received_From_MES] A with(nolock)
            //          Left join [DB_Pilots].[dbo].[WIP_ManagePool_From_MES] B with(nolock) on B.PatientID = A.PatientID

            //          WHERE B.PatientID like '" + _PID + "'");

                PRDCR1PEAPPS01Conections.Execute_Request(@"
declare @duplicates int =0
declare @isduplicate int=0

 SELECT @duplicates = count(pid) 
FROM[DB_RMT].[dbo].[Reworks Report Phase II]
where pid like '"+ _PID + @"' and cast( Date as nvarchar(50)) between FORMAT(DATEADD(minute, -3, getdate()), 'yyyy-dd-MM hh:mm:ss.fff') and  format(getdate(), 'yyyy-dd-MM hh:mm:ss.fff')
  group by PID 

select @duplicates 

if @duplicates = 0
            INSERT INTO [DB_RMT].[dbo].[Reworks Report Phase II]
           ([Date]
           ,[Area]
           ,[Region]
           ,[Cell]
           ,[Supervisor S&S]
           ,[Send by user]
           ,[Send by]
           ,[PID]
           ,[Source]
           ,[Modification]
           ,[Detail] 
           ,[Case type] 
           ,[Priority]" +
               //,[RCC] *** remove because is not needed
               @",[Tagging]
           ,[Rework Autor Name]
           ,[Rework Autor User]
           ,[Autor CELL]
           ,[Autor Supervisor]
           ,[Autor StartDate]
           ,[Autor site]
           ,[Sender Site]) 
                VALUES
                       (
            '" + DateTime.Now + @"',
            '" + AuthorArea + @"',
            '" + _UserRegion + @"',
            '" + _Cell + @"',
            '" + _UserSupervisor + @"',
            '" + _Username + @"',
            '" + _UserCompleteName + @"',
            '" + _PID + @"',
            '" + _Source + @"',
            '" + _Modification + @"',
            '" + _Instruction.Replace("'", "").Replace("<", "").Replace(">", "") +
                //'" + PRDCR1BISERV02Conections.TypeScan + @"',*** remove because is not needed
                @"','" + _CaseType +
                @"','" + _Priority +
                //'" + PRDCR1BISERV02Conections.Recc + @"', *** remove because is not needed
                //'" + PRDCR1BISERV02Conections.TagTime + @"',*** change by datetime now
                @"','" + DateTime.Now + @"',
            '" + AuthorName + @"',
            '" + _Author + @"',
            '" + AuthorCell + @"',
            '" + AuthorSupervisor + @"',
            '" + AuthorStartDate + @"',
            '" + AuthorUbication + @"',
            '" + _Site + "')");


            

        }

        /*public static void Send_IC(int _PID, string _Author, string _Source, int _Priority, string _Modification, string _Instruction, string _Username, string _Cell, string _Site, string _UserRegion, string _UserSupervisor, string _UserCompleteName, string _CaseType)
        {
            PRDCR1PEAPPS01Conections.Execute_Request($@"
        INSERT INTO [DB_RMT].[dbo].[Reworks Report Phase II]
        ([Date], [Area], [Region], [Cell], [Supervisor S&S], [Send by user], [Send by], [PID], [Source], [Modification], [Detail], [Case type], [Priority], [Rework Autor Name], [Rework Autor User], [Autor CELL], [Autor Supervisor], [Autor StartDate], [Autor site], [Sender Site]) 
        VALUES
        (
            GETDATE(),
            '{_UserRegion}',
            '{_UserRegion}',
            '{_Cell}',
            '{_UserSupervisor}',
            '{_Username}',
            '{_UserCompleteName}',
            '{_PID}',
            '{_Source}',
            '{_Modification}',
            '{_Instruction}',
            '{_CaseType}',
            '{_Priority}',
            '{_Author}',
            '{_Author}',
            '{_Cell}',
            '{_UserSupervisor}',
            GETDATE(),
            '{_Site}',
            '{_Site}'
        )");
        }*/


        public static void Load_RowData(int _Ticket, string _User)
        {

            PRDCR1PEAPPS01Conections.Load_SSrowData(@"
                 SELECT
                        [Inspector user]
                       ,[Source]
                       , Modification
                       , Priority
                       FROM[DB_RMT].[dbo].[Reworks Report Phase II] with(nolock)
                       where[TICKET] like '" + _Ticket + "' and [Inspector user] like '" + _User + "' ");

        }

        public static void Release_Treatment(int _Ticket, string _User)
        {
            //define the variables
            string SStime;
            string Totaltime;
            //asign the time the treament stay in the ss pool until DDT release the treatment
            SStime = PRDCR1PEAPPS01Conections.GetElapseTime(@" SELECT [End DDT] FROM [Reworks Report Phase II]    with(nolock) WHERE Ticket like '" + _Ticket + "'");
            //asign the total time the treatment stay in the tool to it release 
           Totaltime = PRDCR1PEAPPS01Conections.GetElapseTime(@" SELECT [Date] FROM [Reworks Report Phase II]    with(nolock)  WHERE Ticket like '" + _Ticket + "'");

            PRDCR1PEAPPS01Conections.Execute_Request(@"
UPDATE [dbo].[Reworks Report Phase II]
   SET 
       [Status] = 'Release'
      ,[Realeased by] = '" + _User + @"'
      ,[Released Time] = '" + DateTime.Now + @"'
      ,[Time S&S] = '" + SStime + @"'
      ,[Total Time] = '" + Totaltime + @"'    

 WHERE ticket like'" + _Ticket + "'");
            
        }
    }
}