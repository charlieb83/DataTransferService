using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;

namespace DataTransferService
{
    public class SqlDataBCP
    {
        private SqlMapping sm;      //Main Sql Mapping

        public SqlDataBCP(SqlMapping pSM)
        {
            sm = pSM;
        }

        //Assembles Main Source SQL Statement
        public string BuildMainSqlStatement()
        {
            string sqlStatement = "";

            //Build Sql Columns
            string sqlColumns = "SELECT ";
            foreach (MappingDetail s in sm.MappingDetails)
            {
                sqlColumns = sqlColumns + s.Destination + " = " + s.Source + ", " + System.Environment.NewLine;
            }
            //Remove last lineFeed and comma
            sqlColumns = sqlColumns.Substring(0, sqlColumns.Length - 4);

            //Build From Tables
            string sqlFrom = "FROM " + sm.From + " ";       // + System.Environment.NewLine;

            //Source Filter (WHERE Statement)
            string sqlWhere = "";
            if (sm.Where != null)
            {
                sqlWhere = "WHERE " + sm.Where + System.Environment.NewLine; ;
            }
            //Remove last lineFeed
            //sqlWhere = sqlWhere.Substring(0, sqlWhere.Length - 1);

            //Concatnate all sections
            sqlStatement = sqlColumns + System.Environment.NewLine + sqlFrom + System.Environment.NewLine + sqlWhere;

            if (sm.ReportToCdmDetails != null)
            {
                //Console.WriteLine(sm.ReportToCdmDetails.ReportWhere);
                sqlStatement = BuildSqlWithReportStatement(sqlStatement);
            }

            return sqlStatement;
        }



        //Assembles and Returns the SQL Statement For Report Join
        private string BuildSqlWithReportStatement(string mainCdmSql)
        {
            string sqlStatement = "";
            string cdmQueryAlias = "X";
            string reportTableAlias = "Y";
            string reportMappingColumns = "";

            //Add additional columns from Report Mapping
            foreach (ReportMappingDetail r in sm.ReportToCdmDetails.ReportMappingDetails)
            {
                reportMappingColumns = reportMappingColumns + ", " + r.Destination + " = " + reportTableAlias + "." + r.Source + System.Environment.NewLine;
            }
            sqlStatement = "SELECT X.* " + reportMappingColumns;
            sqlStatement = sqlStatement + " FROM ( " + mainCdmSql + " ) AS " + cdmQueryAlias + System.Environment.NewLine;
            sqlStatement = sqlStatement + " INNER JOIN " + sm.ReportToCdmDetails.ReportSource + " AS " + reportTableAlias + " ON ";

            //Add the join details
            foreach (ReportJoinToCdmDetail j in sm.ReportToCdmDetails.ReportJoinToCdmDetails)
            {
                sqlStatement = sqlStatement + " " + reportTableAlias + "." + j.ReportSourceColumn + " = " + cdmQueryAlias + "." + j.CdmColumn + System.Environment.NewLine;
                sqlStatement = sqlStatement + "AND";
            }
            //Remove last "AND"
            sqlStatement = sqlStatement.Substring(0, sqlStatement.Length - 3);

            //Where TODO: Should really add alias after WHERE .... Y
            string sqlWhere = "";
            if (sm.ReportToCdmDetails.ReportWhere != null)
            {
                sqlWhere = "WHERE " + reportTableAlias + "." + sm.ReportToCdmDetails.ReportWhere + System.Environment.NewLine; ;
            }
            sqlStatement = sqlStatement + System.Environment.NewLine + sqlWhere;

            return sqlStatement;
        }



        //private string TransferData(string sourceConnectionString, string destinationConnectionString, string destinationTable, string sqlStatement)
        public string TransferData(string sqlStatement)
        {
            string ret = "DONE!";
            //COMMENT FOR TESTING:
            string sourceConnectionString = GetConnectionString(sm.SourceServer, "master");
            string destinationConnectionString = GetConnectionString(sm.DestinationDetails.Server, sm.DestinationDetails.Database);

            //Set Source and Destination Connection Strings
            SqlConnection sourceConnection = new SqlConnection();
            sourceConnection.ConnectionString = sourceConnectionString;

            SqlConnection destinationConnection = new SqlConnection();
            destinationConnection.ConnectionString = destinationConnectionString;

            //Open Source and Destination Connections
            sourceConnection.Open();
            destinationConnection.Open();

            //Check if Source Query is Valid
            bool validSourceQuery = QueryIsValid(sqlStatement, sourceConnection);
            if (validSourceQuery == false)
            {
                throw new Exception("Source Query Is Not Valid");   //??
            }

            //Check if Destination Table exists...create if not exists
            bool destTableExists = DestinationTableExists(sm.DestinationDetails.Table, destinationConnection);
            if (destTableExists == false && sm.DestinationDetails.createTable == true)
            {
                string createTableStatement = "";

                SqlConnection tempSourceConnection = new SqlConnection();
                tempSourceConnection.ConnectionString = destinationConnectionString;

                createTableStatement = GetCreateTableFromSqlCode(sqlStatement, sm.DestinationDetails.Table, tempSourceConnection);

                SqlCommand cmd = new SqlCommand(createTableStatement, destinationConnection);
                cmd.ExecuteNonQuery();

                tempSourceConnection.Close();
            }

            SqlCommand commandSourceData = new SqlCommand(sqlStatement, sourceConnection);
            SqlDataReader reader = commandSourceData.ExecuteReader();

            // Set up the bulk copy object.
            using (SqlBulkCopy bulkCopy = new SqlBulkCopy(destinationConnection))
            {
                bulkCopy.DestinationTableName = sm.DestinationDetails.Table;   // "dbo.Table2";
                bulkCopy.BatchSize = 10000;
                bulkCopy.BulkCopyTimeout = 0;

                // Set up the event handler to notify after 10000 rows.
                bulkCopy.SqlRowsCopied +=
                new SqlRowsCopiedEventHandler(OnSqlRowsCopied);
                bulkCopy.NotifyAfter = 10000;

                try
                {
                    // Write from the source to the destination.
                    bulkCopy.WriteToServer(reader);
                    //Dts.TaskResult = (int)ScriptResults.Success; // only success when everything works fine
                }
                catch (Exception ex)
                {
                    ret = "Script failure :" + ex.Message;
                    throw new Exception("Script Failure!");   //??
                    //Dts.Events.FireInformation(2, "Script failure for object:" + (string)Dts.Variables["User::l_SourceObject"].Value, "error:" + ex.Message, "", 0, ref fireAgain);
                    //Dts.TaskResult = (int)ScriptResults.Failure;
                }
                finally
                {
                    reader.Close();
                }
            }
            sourceConnection.Close();
            destinationConnection.Close();
            return ret;
        }

        //Display number of rows copied so far
        private void OnSqlRowsCopied(object sender, SqlRowsCopiedEventArgs e)
        {
            Console.WriteLine("Copied so far..." + e.RowsCopied.ToString());
        }


        //Build Connection String
        static private string GetConnectionString(string server, string database)
        {
            //return "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=AdventureWorks;Integrated Security=true;";

            //return "Data Source=" + server + "; " +
            return "Server=" + server + "; " +
            "Database=" + database + "; " +
            //"Initial Catalog=KPI_Database; " +
            "Integrated Security=SSPI;";
        }



        //----------------------------------------------------------------
        //--Validate SQL Query, returns FALSE when NOT valid
        //----------------------------------------------------------------
        private bool QueryIsValid(string sqlStatement, SqlConnection sourceConn)
        {
            bool retVal = false;
            try
            {
                string sqlToBeChecked = sqlStatement;
                SqlCommand cmd = new SqlCommand("SET NOEXEC ON", sourceConn);
                cmd.ExecuteNonQuery();
                cmd = new SqlCommand(sqlToBeChecked, sourceConn);
                cmd.ExecuteNonQuery();

                //TODO: OPEN ITS OWN CONNECTION AND RUN1st????
                cmd = new SqlCommand("SET NOEXEC OFF", sourceConn);
                cmd.ExecuteNonQuery();
                retVal = true;
            }
            catch (SqlException ex)
            {
                retVal = false;
                Console.WriteLine("Error: SQL Statement is not valid: " + ex.Message.ToString());
            }
            return retVal;
        }

        //----------------------------------------------------------------
        //--Returns whether or not the destination table exists
        //----------------------------------------------------------------
        private bool DestinationTableExists(string tableName, SqlConnection con)
        {
            bool retVal = false;

            //Check if Destination table exists
            try
            {
                string commandText = @"IF EXISTS(SELECT 1 FROM sys.Objects " +
                                                 "WHERE Object_id = OBJECT_ID(N'" + tableName + "') " +
                                                 "AND Type = N'U')SELECT 1 ELSE SELECT 0";
                SqlCommand commandTableExists = new SqlCommand(commandText, con);
                int destinationTableExists = (int)commandTableExists.ExecuteScalar();
                if (destinationTableExists == 1)
                {
                    retVal = true;
                }
                else
                {
                    retVal = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message.ToString());
                retVal = false;
            }

            return retVal;
        }


        //----------------------------------------------------------------
        //--Returns Create Table Statement From a Query
        //----------------------------------------------------------------
        private string GetCreateTableFromSqlCode(string sqlStatement, string tableName, SqlConnection con)
        {
            SqlCommand cmd = new SqlCommand(string.Format("SET FMTONLY ON;\r\n{0}\r\nSET FMTONLY OFF;", sqlStatement), con);
            try
            {
                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();
                DataTable dt = reader.GetSchemaTable();
                reader.Close();
                return GetCreateTableScript(dt, tableName);
            }
            finally
            {
                if (con.State == ConnectionState.Open)
                    con.Close();
            }
        }

        private string GetCreateTableScript(DataTable dt, string tableName)
        {
            string snip = string.Empty;
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("CREATE TABLE {0}\r\n(\r\n", tableName);
            for (int i = 0; i < dt.Rows.Count; i++)
            {
                DataRow dr = dt.Rows[i];
                snip = GetColumnSql(dr);
                sql.AppendFormat((i < dt.Rows.Count - 1) ? snip : snip.TrimEnd(',', '\r', '\n'));
            }
            sql.AppendFormat("\r\n)");
            return sql.ToString();
        }


        private string GetColumnSql(DataRow dr)
        {
            StringBuilder sql = new StringBuilder();
            sql.AppendFormat("\t[{0}] {1}{2} {3} {4},\r\n",
                dr["ColumnName"].ToString(),
                dr["DataTypeName"].ToString(),
                (HasSize(dr["DataTypeName"].ToString())) ? "(" + dr["ColumnSize"].ToString() + ")" : (HasPrecisionAndScale(dr["DataTypeName"].ToString())) ? "(" + dr["NumericPrecision"].ToString() + "," + dr["NumericScale"].ToString() + ")" : "",
                (dr["IsIdentity"].ToString() == "true") ? "IDENTITY" : "",
                (dr["AllowDBNull"].ToString() == "true") ? "NULL" : "NULL");        //TODO: FORCED TO ALLOW NULLs
                //(dr["AllowDBNull"].ToString() == "true") ? "NULL" : "NOT NULL");
            return sql.ToString();
        }


        private bool HasSize(string dataType)
        {
            Dictionary<string, bool> dataTypes = new Dictionary<string, bool>();
            dataTypes.Add("bigint", false);
            dataTypes.Add("binary", true);
            dataTypes.Add("bit", false);
            dataTypes.Add("char", true);
            dataTypes.Add("date", false);
            dataTypes.Add("datetime", false);
            dataTypes.Add("datetime2", false);
            dataTypes.Add("datetimeoffset", false);
            dataTypes.Add("decimal", false);
            dataTypes.Add("float", false);
            dataTypes.Add("geography", false);
            dataTypes.Add("geometry", false);
            dataTypes.Add("hierarchyid", false);
            dataTypes.Add("image", true);
            dataTypes.Add("int", false);
            dataTypes.Add("money", false);
            dataTypes.Add("nchar", true);
            dataTypes.Add("ntext", true);
            dataTypes.Add("numeric", false);
            dataTypes.Add("nvarchar", true);
            dataTypes.Add("real", false);
            dataTypes.Add("smalldatetime", false);
            dataTypes.Add("smallint", false);
            dataTypes.Add("smallmoney", false);
            dataTypes.Add("sql_variant", false);
            dataTypes.Add("sysname", false);
            dataTypes.Add("text", true);
            dataTypes.Add("time", false);
            dataTypes.Add("timestamp", false);
            dataTypes.Add("tinyint", false);
            dataTypes.Add("uniqueidentifier", false);
            dataTypes.Add("varbinary", true);
            dataTypes.Add("varchar", true);
            dataTypes.Add("xml", false);
            if (dataTypes.ContainsKey(dataType))
                return dataTypes[dataType];
            return false;
        }

        private static bool HasPrecisionAndScale(string dataType)
        {
            Dictionary<string, bool> dataTypes = new Dictionary<string, bool>();
            dataTypes.Add("bigint", false);
            dataTypes.Add("binary", false);
            dataTypes.Add("bit", false);
            dataTypes.Add("char", false);
            dataTypes.Add("date", false);
            dataTypes.Add("datetime", false);
            dataTypes.Add("datetime2", false);
            dataTypes.Add("datetimeoffset", false);
            dataTypes.Add("decimal", true);
            dataTypes.Add("float", true);
            dataTypes.Add("geography", false);
            dataTypes.Add("geometry", false);
            dataTypes.Add("hierarchyid", false);
            dataTypes.Add("image", false);
            dataTypes.Add("int", false);
            dataTypes.Add("money", false);
            dataTypes.Add("nchar", false);
            dataTypes.Add("ntext", false);
            dataTypes.Add("numeric", false);
            dataTypes.Add("nvarchar", false);
            dataTypes.Add("real", true);
            dataTypes.Add("smalldatetime", false);
            dataTypes.Add("smallint", false);
            dataTypes.Add("smallmoney", false);
            dataTypes.Add("sql_variant", false);
            dataTypes.Add("sysname", false);
            dataTypes.Add("text", false);
            dataTypes.Add("time", false);
            dataTypes.Add("timestamp", false);
            dataTypes.Add("tinyint", false);
            dataTypes.Add("uniqueidentifier", false);
            dataTypes.Add("varbinary", false);
            dataTypes.Add("varchar", false);
            dataTypes.Add("xml", false);
            if (dataTypes.ContainsKey(dataType))
                return dataTypes[dataType];
            return false;
        }

    }
}