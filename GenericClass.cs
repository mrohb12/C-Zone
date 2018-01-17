using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using RateChange2018.MessageString;
using System.Configuration;
using System.IO;

namespace RateChange2018.BusinessLayer
{
    public class GenericClass : DataFactory.DataBaseHandler
    {
        public static string connectionString =
            System.Configuration.ConfigurationManager.ConnectionStrings["AppConString"].ConnectionString;

        static string serverConnectionString = System.Configuration.ConfigurationManager
            .ConnectionStrings["ServerConnection"].ConnectionString;

        public static string UId;
        public static string UserName;
        public static string MandaliCode;
        public static int MessageId;
        public static int RoleId;
        public static string MandaliName;
        public static string MandaliNameGuj;
        public static string MandaliNameEng;
        public static int LastNotificationId;
        public static string sqlpath;
        public static string defaultpath;



        public static string RoleName;


        /************************* Added by Bhavesh Rana - 06/05/2016 ***************************/
        public static bool UseReverseFeed;
        public static int UseLineSpacing;
        public static string PrinterName;

        public static bool PromptCutter;
        /****************************************************************************************/

        private HelperFunction hf;

        SqlConnection conn;
        public GenericClass(bool useServer = false) : base((useServer) ? serverConnectionString : connectionString)
        {
            hf = new HelperFunction();
            conn = new SqlConnection(connectionString);
        }
        public void SqlBulkUpdate(DataTable dt)
        {
            if (dt.Rows.Count > 0)
            {
                        conn.Open();
                    using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(conn))
                    {
                        //Set the database table name
                        sqlBulkCopy.DestinationTableName = "[Trans_MilkPurchase]";
                       
                        //[OPTIONAL]: Map the DataTable columns with that of the database table
                        sqlBulkCopy.ColumnMappings.Add("NewRate", "Rate");
                        sqlBulkCopy.ColumnMappings.Add("NewAmount", "Amount");
                        sqlBulkCopy.ColumnMappings.Add("NewMemberRate", "MemberRate");
                      
                        sqlBulkCopy.WriteToServer(dt);
                        conn.Close();
                    }
               
            }
        }
        public static string GetMachineIPAddress()
        {
            string hostName = Dns.GetHostName();
            string myIP = Dns.GetHostByName(hostName).AddressList[0].ToString();
            return myIP;
        }
        public int ExecuteSqlFile(string path)
        {
            if (File.Exists(path))
            {
                //   string sql = File.ReadAllText(path);
                int count = 0;
                StreamReader readfilequery = new StreamReader(path);
                string query = "";
                string line = readfilequery.ReadLine();
                while (line != null)
                {
                    if (line.IndexOf("GO") == -1)
                    {
                        query = query + " " + line;
                    }
                    else
                    {
                        count = SetByQuery(query);
                        query = null;
                    }
                    line = readfilequery.ReadLine();
                }

                return count;



            }
            return 0;
        }
      
        public  bool IsDbExist()
        {
            bool result = false;
            try
            {
                string data = this.GetString("SELECT database_id FROM sys.databases WHERE Name like 'AMCS%'");
                if (!string.IsNullOrEmpty(data))
                {
                    result = true;
                }
            }
            catch (Exception ex)
            {
                hf.LogException(ex);
                result = false;
            }
            return result;
        }

      

       
    }

    public enum MenuPattern
    {
        Master,
        Transaction,
        Share,
        Report,
        UserManagement,
        Utility
    }

    public enum Mode
    {
        Insert,
        Update,
        Delete
    }
    public enum ShareTransactionType
    {
        ISSU,
        TRAN,       
        CANC,
        OWRD
    }
    public enum EntryMode
    { 
        A,
        M
    }
    public enum TransactionType
    {
        PURC,
        SALE,
        DEPO
    }
    public enum TranType
    {
        CREDIT,
        DEBIT
    }

    public enum DBCType
    {
        PURC,
        SALE,
        DEPO,
        SALM,
        CASR,
        CASP,
        JOUR


    }
}
