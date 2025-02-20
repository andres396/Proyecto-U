using ICT.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ICT.Controllers
{
    public class InspectionControl
    {

        public static int CountTreatmentstoPick(string _Area, string _Site)
        {
            try
            {
                if (_Area == "Celdas Especiales")
                {
                    _Area = "DDT Celdas Especiales' or Area like 'ALTA GO' or Area like 'MA' or Area like 'I FIRST' or Area like 'IPL' or Area like 'NPD";
                }

                PRDCR1PEAPPS01Conections.Count_DDTPoolConections(@"
            SELECT COUNT([Ticket]) as QTY
            FROM [DB_RMT].[dbo].[Reworks Report Phase II] WITH(NOLOCK)
            WHERE (Area like '" + _Area + @"') AND [Autor site] LIKE '%" + _Site + @"%'");

                return PRDCR1PEAPPS01Conections.Count > 0 ? PRDCR1PEAPPS01Conections.Count : 0;
            }
            catch (Exception ex)
            {
                // Registrar error en logs si es necesario
                System.Diagnostics.Debug.WriteLine("Error en CountTreatmentstoPick: " + ex.Message);
                return 0;  // Evita NullReferenceException
            }
        }


        public static void SendToCustomer(string ticket, string pid, string username)
        {
            PRDCR1PEAPPS01Conections.Execute_Request($@"
        UPDATE [DB_RMT].[dbo].[Reworks Report Phase II]
        SET [Status] = 'Sent to Customer',
            [ProcessedBy] = '{username}',
            [ProcessedDate] = GETDATE()
        WHERE [Ticket] = '{ticket}' AND [PID] = '{pid}'
    ");
        }


        public static void RegisterReason(string ticket, string pid, string reason, string username)
        {
            PRDCR1PEAPPS01Conections.Execute_Request($@"
        INSERT INTO [DB_RMT].[dbo].[InspectionReasons]
        ([Ticket], [PID], [Reason], [SubmittedBy], [SubmissionDate])
        VALUES
        ('{ticket}', '{pid}', '{reason}', '{username}', GETDATE())
    ");
        }


        public static int CountAssignedtome(string _username, string _InspectorName)
        {

            PRDCR1PEAPPS01Conections.Count_DDTPoolConections(@" 
                            SELECT 
                               COUNT([Ticket]) as qty_Ticket
                            FROM [DB_RMT].[dbo].[Reworks Report Phase II] WITH(NOLOCK)
                            WHERE convert(varchar,[Status]) = 'In Inspection' AND ([Inspector user] like '" + _username + @"')");
            return PRDCR1PEAPPS01Conections.Count;
        }
        public static void Load_Treatment(string _site, string _InspectorUserName, string _InspectorName, string _InspectorShift, string _InspectorSite, string _InspectorCell)
        {
            PRDCR1PEAPPS01Conections.Read_TreatmentAssigned(@"
            SELECT TOP 1
                              [Ticket]
                              ,[PID]
                              ,[Source]
                              ,[Modification]
                              ,[Detail]
                              ,[Case type]
                              ,[Rework Autor User]
                              ,[priority]
                              ,[Send by]
        FROM[DB_RMT].[dbo].[Reworks Report Phase II]  with(nolock)
                          WHERE[Status] like 'In Inspection' and ([Inspector user] like '" + _InspectorUserName + @"')
                          order by[Priority] desc,  DATEDIFF(hour, [Tagging], [DATE]) desc");

            string TimeDDT = PRDCR1PEAPPS01Conections.GetElapseTime(@" 
                                                                        SELECT [Date] 
                                                                        FROM [Reworks Report Phase II] with(nolock)   
                                                                        WHERE Ticket like (" + PRDCR1PEAPPS01Conections.Ticket + ")");
            PRDCR1PEAPPS01Conections.Execute_Request(@"
                                                    UPDATE [dbo].[Reworks Report Phase II]
                                                    SET
                                                           [Status] = 'In Inspection'
                                                          ,[Inspector Name] = '" + _InspectorName + @"'
                                                          ,[Inspector user] = '" + _InspectorUserName + @"'
                                                          ,[Inspector Shift] = '" + _InspectorShift + @"'
                                                          ,[Inspector site] = '" + _InspectorSite + @"'
                                                          ,[Inspector cell] = '" + _InspectorCell + @"'
                                                          ,[Start DDT] = '" + DateTime.Now + @"'  
                                                          ,[Time DDT] = '" + TimeDDT + @"' 
                                                           WHERE  Ticket like '" + PRDCR1PEAPPS01Conections.Ticket + "'");
        }


        public static void Assign_Treatment(string _area, string _site, string _InspectorUserName, string _InspectorName, string _InspectorShift, string _InspectorSite, string _InspectorCell)
        {

            //CELDAS ESPECIALES contains a lot off rtts so is needed to separate the query from the others 
            if (_area == "Celdas Especiales")
            {
                _area = "DDT Celdas Especiales' or Area like 'ALTA GO' or Area like 'MA' or Area like 'I FIRST' or Area like 'IPL' or Area like 'NPD";
            }
            PRDCR1PEAPPS01Conections.Read_TreatmentAssigned(@"

                         declare @asigntiket int = 0
                         declare @ticketdate int = 0
                         select top 1
                         @asigntiket = [Ticket]

                            from
                                DB_RMT.dbo.[Reworks Report Phase II] with(nolock) 
                            where 
                                [Status]  IS NULL and (Area like '" + _area + @"') 
                                and ([Autor site] like '%" + _site + @"%' or [Autor site] like '')
                             order by[Priority] desc,  DATEDIFF(hour,[Tagging],GETDATE()) desc				

				SELECT @ticketdate = DATEDIFF(minute,date,getdate())
                FROM  DB_RMT.dbo.[Reworks Report Phase II] with(nolock)   
                WHERE Ticket like @asigntiket
				
				 UPDATE  DB_RMT.dbo.[Reworks Report Phase II]
                                                    SET
                                                           [Status] = 'In Inspection'
                                                          ,[Inspector Name] = '" + _InspectorName + @"'
                                                          ,[Inspector user] = '" + _InspectorUserName + @"'
                                                          ,[Inspector Shift] = '" + _InspectorShift + @"'
                                                          ,[Inspector site] = '" + _InspectorSite + @"'
                                                          ,[Inspector cell] = '" + _InspectorCell + @"'
                                                          ,[Start DDT] = '" + DateTime.Now + @"'  
                                                          ,[Time DDT] = cast(@ticketdate as nvarchar)
                                                           WHERE  Ticket like @asigntiket

	SELECT 
                              [Ticket]
                              ,[PID]
                              ,[Source]
                              ,[Modification]
                              ,[Detail]
                              ,[Case type]
                              ,[Rework Autor User]
                              ,[priority]
                              ,[Send by]
        FROM[DB_RMT].[dbo].[Reworks Report Phase II] with(nolock)
                          WHERE [Status] like 'In Inspection' and ([Inspector user] like '" + _InspectorUserName + @"' or [Inspector Name] like '" + _InspectorName + @"')
                          order by[Priority] desc, [DATE] desc");


        }


        public static void EndInspection(int _Ticket, string _Status, string _Apply, string _Category, string _Subcategory)
        {

            string reworkTime = PRDCR1PEAPPS01Conections.GetElapseTime(@" SELECT [Start DDT] FROM [Reworks Report Phase II] with(nolock) WHERE Ticket like '" + _Ticket + "'");
            PRDCR1PEAPPS01Conections.Execute_Request(@"

                        UPDATE[dbo].[Reworks Report Phase II]
                        SET
                               [Status] = '" + _Status + @"'
                              ,[Apply as] = '" + _Apply + @"'
                              ,[Category] = '" + _Category + @"'
                              ,[Sub Category] = '" + _Subcategory + @"'
                              ,[End DDT] = '" + DateTime.Now + @"'     
                              ,[Rework TIme] = '" + reworkTime + @"'
                              
                         WHERE Ticket like '" + _Ticket + "'");

        }

    }
}