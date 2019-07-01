using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataTransferService_Console.ServiceReference1;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DataTransferService_Console
{
    class Program
    {
        static void Main(string[] args)
        {
            //Setup Service
            var serviceRef = new ServiceReference1.Service1Client();

            //Simple Service GetData To Confirm Service Is Working
            string x = serviceRef.GetData(5);
            Console.WriteLine(x);

            //COMPOSITE
            //ServiceReference1.CompositeType CT = new ServiceReference1.CompositeType();
            //CT.BoolValue = true;
            //CT.StringValue = "CHARLIE";
            //Console.WriteLine(serviceRef.GetDataUsingDataContract(CT).StringValue);

            //Test Parsing to JSON Object
            //JObject json = JObject.Parse(JsonFile.Text);
            //var details = JObject.Parse(JsonFile.Text);


            //TEST Combining Json 
            string cdmJson = JsonFile.Text;
            string reportJson = JsonFile.Text2;

            reportJson = reportJson.Replace("<PARAMETER_VALUE>", @"Rpt in (\'RPT1234\', \'RPT4567\' )");
            cdmJson = cdmJson.Replace("<REPORT_CDM_PARAMETER>", reportJson);

            //HERE:
            //Service MoveData
            //string y = serviceRef.MoveData(JsonFile.Text);
            string y = serviceRef.MoveData(cdmJson);
            //string y = serviceRef.MoveDataReport(JsonFile.Text, JsonFile.Text2);

            Console.WriteLine("DataTransfer Result: " + y);

            Console.ReadLine();
        }


        //Validate Json Online:
        //https://jsonlint.com/

        public static class JsonFile
        {
            public static string Text = @"{
                                            'mappingName' : 'Aetna ACAS CDM',
                                            
                                           'sourceServer' : '(localdb)\\MSSQLLocalDB', 

                                            'destinationDetails' : { 'server' : '(localdb)\\MSSQLLocalDB', 'database' : 'KPI_Database', 'table' : 'dbo.NewCDM', 'createTable' : true},

                                            'mappingDetails' : [
                                                                { 'destination' : 'Id',     'source' : 'Id' },
                                                                { 'destination' : 'ClaimKey',  'source' : 'RCEClaimKey'},
                                                                { 'destination' : 'cnlyClaimNumber',  'source' : 'ClaimNumber'},
                                                                { 'destination' : 'ProviderNumber',  'source' : 'ProviderId'},
                                                                { 'destination' : 'MemberNumber',  'source' : 'MemberId'}
                                                               ],
                                            'from' : 'KPI_Database.dbo.AetnaHeader',
                                            'where' : 'FinalAction=1',

                                            'reportToCdmDetails' : <REPORT_CDM_PARAMETER>
                                        }";

            public static string Text2 = @"{
	                                            'reportMappingName' : 'Aetna ACAS Header Standard Reports',
	                                            'reportSource' : 'KPI_Database.dbo.AetnaStandardReportHeader',
	                                            'reportMappingDetails' : [ { 'destination' : 'ReportNumber', 'source' : 'Rpt' } ],
	                                            'reportJoinToCdmDetails' : [ {'cdmColumn' : 'ClaimKey',  'reportSourceColumn' : 'RCEClaimKey'} ],
	                                            'reportWhere' : '<PARAMETER_VALUE>'
                                            }";
                                        //  'reportWhere' : {'<PARAMETER_KEY>' : '<PARAMETER_VALUE>' }

        }
    }
}
