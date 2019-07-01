using System.Runtime.Serialization;
using System.ServiceModel;

namespace DataTransferService
{
    [ServiceContract]
    public interface IService1
    {
        [OperationContract]
        string GetData(int value);

        //[OperationContract]
        //CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
        //[OperationContract]
        [OperationContract(Name = "MoveData")]
        //string MoveData(JObject json);
        string MoveData(string jsonMapping);


        //CANT HAVE METHOD OVERLOAD IN WCF!!!!
        //[OperationContract]
        //[OperationContract(Name = "MoveDataReport")]
        ////--Overload - also includes jsonReportMapping
        //string MoveData(string jsonMapping, string jsonReportMapping);
    }



    //https://docs.microsoft.com/en-us/bingmaps/rest-services/json-data-contracts

    //json datacontracts:
    [DataContract]
    public class SqlMapping
    {
        //bool boolValue = true;

        [DataMember(Name = "mappingName")]
        public string MappingName { get; set; }

        [DataMember(Name = "sourceServer")]
        public string SourceServer { get; set; }

        [DataMember(Name = "destinationDetails")]
        public DestinationDetail DestinationDetails { get; set; }

        [DataMember(Name = "mappingDetails")]
        public MappingDetail[] MappingDetails { get; set; }

        [DataMember(Name = "from")]
        public string From { get; set; }

        [DataMember(Name = "joinDetails")]
        public JoinDetail[] JoinDetails { get; set; }

        [DataMember(Name = "where")]
        public string Where { get; set; }

        //NEW
        [DataMember(Name = "reportToCdmDetails")]
        public ReportToCdmDetail ReportToCdmDetails { get; set; }
    }

    //NEW
    [DataContract]
    public class ReportToCdmDetail
    {
        [DataMember(Name = "reportMappingName")]
        public string ReportMappingName { get; set; }

        [DataMember(Name = "reportSource")]
        public string ReportSource { get; set; }

        [DataMember(Name = "reportMappingDetails")]
        public ReportMappingDetail[] ReportMappingDetails { get; set; }

        [DataMember(Name = "reportJoinToCdmDetails")]
        public ReportJoinToCdmDetail[] ReportJoinToCdmDetails { get; set; }

        [DataMember(Name = "reportWhere")]
        public string ReportWhere { get; set; }
    }

    //NEW
    [DataContract]
    public class ReportMappingDetail
    {
        [DataMember(Name = "destination")]
        public string Destination { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }
    }


    //NEW
    [DataContract]
    public class ReportJoinToCdmDetail
    {
        [DataMember(Name = "cdmColumn")]
        public string CdmColumn { get; set; }

        [DataMember(Name = "reportSourceColumn")]
        public string ReportSourceColumn { get; set; }
    }



    [DataContract]
    public class DestinationDetail
    {
        [DataMember(Name = "server")]
        public string Server { get; set; }

        /// <summary>
        /// This is the Destination Database
        /// </summary>
        [DataMember(Name = "database")]
        public string Database { get; set; }

        [DataMember(Name = "table")]
        public string Table { get; set; }

        [DataMember(Name = "createTable")]
        public bool createTable { get; set; }
    }


    [DataContract]
    public class MappingDetail
    {
        [DataMember(Name = "destination")]
        public string Destination { get; set; }

        [DataMember(Name = "source")]
        public string Source { get; set; }
    }


    [DataContract]
    public class JoinDetail
    {
        [DataMember(Name = "joinType")]
        public string JoinType { get; set; }

        [DataMember(Name = "leftObjectAlias")]
        public string LeftObjectAlias { get; set; }

        [DataMember(Name = "rightObjectAlias")]
        public string RightObjectAlias { get; set; }
    }

}
