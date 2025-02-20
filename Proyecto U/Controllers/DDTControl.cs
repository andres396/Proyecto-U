using ICT.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ICT.Controllers
{
    public class DDTControl
    {
        //count the treatments in pool 
        public static int count_oftreatments(string _area, string _site)
        {
            if (_area == "Celdas Especiales")
            {
                _area = @"
                            DDT Celdas Especiales' or 
                            Area like 'ALTA GO' or 
                            Area like 'MA' or 
                            Area like 'I FIRST' or 
                            Area like 'IPL' or 
                            Area like 'NPD";
            }
            if (_area == "General Pool")
            {
                PRDCR1PEAPPS01Conections.Count_DDTPoolConections(@"
                        DECLARE @EndDate DATE
                        DECLARE @StartDate DATE
                        SET @StartDate = DATEADD(DAY, -2, GETDATE())
                        SET @EndDate = DATEADD(DAY, 1, GETDATE())
                        SELECT 
                        COUNT([Ticket]) as QTY
                        FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
                        where (Status Like 'In Inspection' or  status is null)
                        and [Date] between @StartDate and @EndDate ");

            }
            else
            {
                PRDCR1PEAPPS01Conections.Count_DDTPoolConections(@"
                     SELECT
                     COUNT([Ticket]) as QTY
                     FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
                     WHERE
                     (Area like '" + _area + @"')  and 
                     (Status Like 'In Inspection' or status is null ) or 
                     ([Rework Autor User] like 'OLD CCMOD' and  (Status Like 'In Inspection' or  status is null ) )");
            }

            return PRDCR1PEAPPS01Conections.Count;
        }
        //load the data to diplay in to the gridview
        public static DataTable Load_RawData(string _area)
        {
            if (_area == "Celdas Especiales")
            {
                _area = "DDT Celdas Especiales' or Area like 'ALTA GO' or Area like 'MA' or Area like 'I FIRST' or Area like 'IPL' or Area like 'NPD";
                PRDCR1PEAPPS01Conections.Load_RawDataConexion(@"
                        DECLARE @EndDate DATE
                        DECLARE @StartDate DATE
                        SET @StartDate = DATEADD(DAY, -2, GETDATE())
                        SET @EndDate =  DATEADD(DAY, 1, GETDATE())
						SELECT 
                               TICKET
                              ,CAST([Date] as date) as [DATE]
	                          ,CAST([Date] as time) as [HOUR]
                              ,[Send by]as [Sender]
                              ,[PID]
                              ,[Priority]
                              ,DATEDIFF(MINUTE,[Tagging],GETDATE()) as [TAG Mins]
                              ,[Rework Autor Name]
                              ,[Autor CELL]
                              ,[Autor site]
                              ,[Status]
                              ,[Inspector user]
                              ,[Sender Site]
                          FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
                         where (convert(varchar,Area) like '" + _area + @"')  and (Status Like 'In Inspection' or  status is null )
                         and [Date] between @StartDate and @EndDate
                          order by [Date] desc, [Priority] desc
                        ");
            }
            else if (_area == "General Pool")
            {
                PRDCR1PEAPPS01Conections.Load_RawDataConexion(@"
       DECLARE @EndDate DATE
       DECLARE @StartDate DATE
       SET @StartDate = DATEADD(DAY, -2, GETDATE())
       SET @EndDate =  DATEADD(DAY, 1, GETDATE())
       SELECT 
       TICKET
      ,CAST([Date] as date) as [DATE]
	  ,CAST([Date] as time) as [HOUR]
      ,[Send by]as [Sender]
      ,[PID]
      ,[Priority]
      ,DATEDIFF(MINUTE,[Tagging],GETDATE()) as [TAG Mins]
      ,[Rework Autor Name]
      ,[Autor CELL]
      ,[Autor site]
      ,[Status]
      ,[Inspector user]
      ,[Sender Site]
  FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
 where (Status Like 'In Inspection' or  status is null)
 and [Date] between @StartDate and @EndDate
  order by [Date] desc, [Priority] desc
");
            }
            else
            {

                PRDCR1PEAPPS01Conections.Load_RawDataConexion(@"
       DECLARE @EndDate DATE
       DECLARE @StartDate DATE
       SET @StartDate = DATEADD(DAY, -2, GETDATE())
       SET @EndDate =  DATEADD(DAY, 1, GETDATE())
       SELECT 
       TICKET
      ,CAST([Date] as date) as [DATE]
	  ,CAST([Date] as time) as [HOUR]
      ,[Send by]as [Sender]
      ,[PID]
      ,[Priority]
      ,DATEDIFF(MINUTE,[Tagging],GETDATE()) as [TAG Mins]
      ,[Rework Autor Name]
      ,[Autor CELL]
      ,[Autor site]
      ,[Status]
      ,[Inspector user]
      ,[Sender Site]
  FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
 where (Area like '" + _area + @"')  and (Status Like 'In Inspection' or  status is null )
 and [Date] between @StartDate and @EndDate
  order by [Date] desc, [Priority] desc
");


            }
            return PRDCR1PEAPPS01Conections.rawdata;
        }


        internal static void UpdateTicketDetails(int ticket, string Status, int priority)
        {
            try
            {
                // Actualizar la prioridad del ticket
                PRDCR1PEAPPS01Conections.Execute_Request($@"
            UPDATE [dbo].[Reworks Report Phase II]
            SET [Priority] = {priority}
            WHERE Ticket = {ticket}");

                // Actualizar el inspector asignado y el estado del ticket
                PRDCR1PEAPPS01Conections.Execute_Request($@"
            UPDATE [DB_RMT].[dbo].[Reworks Report Phase II]
                SET [Status] = {Status},
            WHERE Ticket = {ticket}");
            }
            catch (Exception ex)
            {
                // Manejar errores (log o excepciones)
                throw new ApplicationException($"Error updating ticket {ticket}: {ex.Message}", ex);
            }
        }


        //update the priority for a ic in process
        public static void UpdatePriority(int _priority, int _ticket)
        {
            PRDCR1PEAPPS01Conections.Execute_Request(@"UPDATE [dbo].[Reworks Report Phase II]
                       SET [Priority] = '" + _priority + @"'
                       WHERE ticket like '" + _ticket + "'");
        }
        //update a ic to assign a inspector and update the status
        public static void updateInspector(string Status, int _ticket)
        {
            try
            {
                // Actualizar el inspector asignado y el estado del ticket
                PRDCR1PEAPPS01Conections.Execute_Request($@"
             UPDATE [DB_RMT].[dbo].[Reworks Report Phase II]
                SET [Status] = '" + Status + @"'
            WHERE Ticket = '" + _ticket + "'");
            }
            catch (Exception ex)
            {
                // Manejar errores (log o excepciones)
                throw new ApplicationException($"Error updating inspector and status for ticket {_ticket}: {ex.Message}", ex);
            }
        }

        public static DataTable Search_Ticket(int _ticket)
        {
            PRDCR1PEAPPS01Conections.Load_RawDataConexion(@"
SELECT 
       TICKET
      ,CAST([Date] as date) as [DATE]
	  ,CAST([Date] as time) as [HOUR]
      ,[Send by]as [Sender]
      ,[PID]
      ,[Priority]
      ,DATEDIFF(hour,[Tagging],GETDATE()) as [TAG HOURS]
      ,[Rework Autor Name]
      ,[Autor CELL]
      ,[Autor site]
      ,[Status]
      ,[Inspector user]
      ,[Sender Site]
  FROM [DB_RMT].[dbo].[Reworks Report Phase II]    with(nolock)
 where (Status Like 'In Inspection' or  status is null) and Ticket like '" + _ticket + @"'
  order by [Priority] desc,  DATEDIFF(hour,[Tagging],GETDATE()) desc
");


            return PRDCR1PEAPPS01Conections.rawdata;
        }


    }
}