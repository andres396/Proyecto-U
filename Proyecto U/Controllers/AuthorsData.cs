using ICT.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace ICT.Controllers
{
    public class AuthorsData
    {
        public static DataTable Load_LoginAuthors()
        {
           PRDCR1BISERV02Conections.Load_RawDataConexion(@"
                              SELECT
                                 [MES_CADCAM_LOGIN],
                                 [LOGIN]
                              FROM [DB_Pilots].[dbo].[Manpower_From_OMS]
                              WHERE 
                                 (CURRENT_OPERATION LIKE '%ddt%' 
                                  OR CURRENT_OPERATION = 'Scan Quality Verification'
                                  OR CURRENT_OPERATION LIKE '%Group%'
                                  OR CURRENT_OPERATION LIKE '%Grupo%')
                                  AND (MES_CADCAM_LOGIN IS NOT NULL AND MES_CADCAM_LOGIN <> '') 
                              ORDER BY MES_CADCAM_LOGIN;
");
            return PRDCR1BISERV02Conections.rawdata;
        }
    }
}