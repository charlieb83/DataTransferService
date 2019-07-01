using Newtonsoft.Json;
using System;

namespace DataTransferService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class Service1 : IService1
    {
        public string GetData(int value)
        {
            return string.Format("You entered: {0}", value);
        }



        //Move Data
        public string MoveData(string jsonCDM)
        {
            //TODO: Validate JSON
            string ret = "";
            string sqlStatement = "";

            //Deserialize JSON into SqlMapping DataContract
            SqlMapping sm = JsonConvert.DeserializeObject<SqlMapping>(jsonCDM);
            SqlDataBCP BCP = new SqlDataBCP(sm);

            sqlStatement = BCP.BuildMainSqlStatement();
            ret = BCP.TransferData(sqlStatement);

            return ret;
        }


        //CANT HAVE METHOD OVERLOAD IN WCF!!!!
        //public string MoveData(string jsonCDM, string jsonReport)
        //{
        //    //TODO: Validate JSON
        //    string ret = "";
        //    string sqlStatement = "";

        //    //Deserialize JSON into SqlMapping DataContract
        //    SqlMapping sm = JsonConvert.DeserializeObject<SqlMapping>(jsonCDM);
        //    ReportMapping rm = JsonConvert.DeserializeObject<ReportMapping>(jsonReport);
        //    Console.WriteLine(rm.ReportSource);
        //    SqlDataBCP BCP = new SqlDataBCP(sm, rm);

        //    sqlStatement = BCP.BuildSqlWithReportStatement();
        //    ret = BCP.TransferData(sqlStatement);

        //    return ret;     // sqlStatement; //ret;
        //}

    }
}
