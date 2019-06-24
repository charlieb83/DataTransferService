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

            //Service GetData
            string x = serviceRef.GetData(5);
            Console.WriteLine(x);

            //COMPOSITE
            //ServiceReference1.CompositeType CT = new ServiceReference1.CompositeType();
            //CT.BoolValue = true;
            //CT.StringValue = "CHARLIE";
            //Console.WriteLine(serviceRef.GetDataUsingDataContract(CT).StringValue);


            //JObject json = JObject.Parse(JsonFile.Text);
            //var details = JObject.Parse(JsonFile.Text);


            //Service MoveData
            //string y = serviceRef.MoveData(JsonFile.Text);

            string y = serviceRef.MoveDataReport(JsonFile.Text, JsonFile.Text2);
            Console.WriteLine("DataTransfer Result: " + y);

            //mergeJson();

            Console.ReadLine();
        }



        //public static void mergeJson()
        //{

        //    var dataObject1 = JObject.Parse(@"{
        //            ""data"": [{
        //                ""id"": ""id1"",
        //                ""field"": ""field1"",
        //                ""name"": ""charlie""
        //            }],
        //            ""paging"": {
        //                ""prev"": ""link1"",
        //            }
        //        }");
        //    var dataObject2 = JObject.Parse(@"{
        //        ""data"": [{
        //            ""id"": ""id2"",
        //            ""field"": ""field2""
        //        }],
        //        ""paging"": {
        //            ""prev"": ""link2"",
        //        }
        //    }");

        //    //[{"id":"id1","field":"field1"},{"id":"id2","field":"field2"}]


        //    var mergeSettings = new JsonMergeSettings
        //    {
        //        MergeArrayHandling = MergeArrayHandling.Union
        //    };

        //    // method 1
        //    (dataObject1.SelectToken("data") as JArray).Merge(dataObject2.SelectToken("data"), mergeSettings);
        //    // method 2
        //    //dataObject1.Merge(dataObject2, mergeSettings);

        //    var mergedArray = dataObject1.SelectToken("data") as JArray;

        //    //Console.WriteLine(mergedArray.ToString(Formatting.None));
        //}


        //Validate Json Online:
        //https://jsonlint.com/

        //{ 'server' : 'ds-fld-002', 'database' : 'Architecture', 'table' : 'dbo.CommonCDM', 'alias' : 'b'}
        public static class JsonFile
        {
            public static string Text = @"{
                                            'mappingName' : 'Aetna ACAS Header Standard Reports',
                                            
                                           'sourceServer' : '(localdb)\\MSSQLLocalDB', 

                                            'destinationDetails' : { 'server' : '(localdb)\\MSSQLLocalDB', 'database' : 'KPI_Database', 'table' : 'dbo.NEW', 'createTable' : true},

                                            'mappingDetails' : [
                                                                { 'destination' : 'Id',     'source' : 'Id' },
                                                                { 'destination' : 'FName',  'source' : 'FirstName'},
                                                                { 'destination' : 'LName',  'source' : 'LastName'}
                                                               ],
                                            'from' : 'KPI_Database.dbo.Table1',

                                            'where' : 'FirstName=\'Jon\''
                                        }";

            public static string Text2 = @"{
	                                            'mappingName' : 'Aetna ACAS Header Standard Reports',
	                                            'reportSource' : 'KPI_Database.dbo.TableJoin',
	                                            'reportMappingDetails' : [
						                                            { 'destination' : 'ClaimNumber',     'source' : 'Claim' }
					                                               ],
	                                            'joinToCdmDetails' : [ {'cdmColumn' : 'LName',  'reportSourceColumn' : 'LstName'} ],
	                                            'where' : 'LstName=\'Smith\''
                                            }";

            //public static string Text2 = @"{
	           //                                 'mappingName' : 'Aetna ACAS Header Standard Reports',
	           //                                 'reportSource' : 'AetnaTraditionalStandardReports.dbo.cnlyClaimHeaderReport',
	           //                                 'reportMappingDetails' : [
						      //                                      { 'destination' : 'Report',     'source' : 'Rpt' }
					       //                                        ],
	           //                                 'joinToCdmDetails' : [ {'cdmColumn' : 'ClaimNumber',  'reportSourceColumn' : 'ClaimNumber'} ],
	           //                                 'where' : 'Report=\'Rpt1234\''
            //                                }";

            //public static string Text = @"{
            //                                'mappingName' : 'Aetna ACAS Header Standard Reports',
                                            
            //                               'sourceServer' : '(localdb)\\MSSQLLocalDB', 

            //                                'sourceDetails' : [ 
            //                                                    { 'database' : 'KPI_Database', 'table' : 'dbo.Table1', 'alias' : 'a'}
                                                                
            //                                                  ],

            //                                'destinationDetails' : { 'server' : '(localdb)\\MSSQLLocalDB', 'database' : 'KPI_Database', 'table' : 'dbo.NEW', 'createTable' : true},

            //                                'mappingDetails' : [
            //                                                    { 'destination' : 'Id',     'source' : 'Id' },
            //                                                    { 'destination' : 'FName',  'source' : 'FirstName'},
            //                                                    { 'destination' : 'LName',  'source' : 'LastName'}
            //                                                   ],

            //                                'sourceFilter' : 'FirstName=\'Charlie\''
            //                            }";


            //public static string Text = @"{
            //                                'mappingName' : 'Aetna ACAS Header Standard Reports',

            //                               'sourceServer' : 'aetna.sql.prd.ccaintranet.com', 

            //                                'sourceDetails' : [ 
            //                                                    { 'database' : 'AetnaStandardReports', 'table' : 'dbo.cnlyClaimHeader', 'alias' : 'a'}

            //                                                  ],

            //                                'destinationDetails' : { 'server' : 'aetna.sql.prd.ccaintranet.com', 'database' : 'AetnaStandardReports', 'table' : 'dbo.CommonCDM'},

            //                                'mappingDetails' : [
            //                                                    { 'destination' : 'claimKey',       'source' : 'RCEClaimNum' },
            //                                                    { 'destination' : 'CnlyClaimNum',   'source' : 'ClaimNum'},
            //                                                    { 'destination' : 'ProviderNumber', 'source' : 'ProviderId'},
            //                                                    { 'destination' : 'MemberNumber',   'source' : 'UniqMemberId'},
            //                                                    { 'destination' : 'GroupNumber',    'source' : 'EmpGrpNum'},
            //                                                    { 'destination' : 'TaxId',          'source' : 'ProvTaxId'}
            //                                                   ]
            //                            }";





            /*
             ,

                                            'sourceFilters' : [
                                                                {'comparisonOperator' : '=', 'leftColumn' : 'ClaimNum', 'rightColumn' : 'b'}
                                                            ]
             */



            //--Single Array that works
            //public static string Text = @"{
            //                                'mappingName' : 'Aetna ACAS Header Standard Reports',

            //                                'sourceDetails' : [ { 'connection' : 'aetna.sql.stg.ccaintranet.com', 
            //                                             'database' : 'AetnaStandardReports',
            //                                             'table' : 'dbo.cnlyClaimHeader'
            //                                           }],

            //                                'destination' : 'aetna.sql.stg.ccaintranet.com',

            //                                'mapping' : 'claimKey'

            //                            }";


            //--No Array
            //public static string Text = @"{
            //                                'mappingName' : 'Aetna ACAS Header Standard Reports',

            //                                'source' : { 'connection' : 'aetna.sql.stg.ccaintranet.com', 
            //                                             'database' : 'AetnaStandardReports',
            //                                             'table' : 'dbo.cnlyClaimHeader'
            //                                           },

            //                                'destination' : 'aetna.sql.stg.ccaintranet.com',

            //                                'mapping' : 'claimKey'

            //                            }";
        }
    }
}
