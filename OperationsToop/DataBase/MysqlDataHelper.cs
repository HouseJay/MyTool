using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Data;


namespace OperationsTool.DataBase
{
    class MysqlDataHelper
    {
        public MySqlCommand com;
        public MySqlConnection conn;

        public MySqlConnection ConnectMysql()
        {
            string mysqlStr = "SERVER=localhost;DATABASE=cdsidb_new;UID=root;PASSWORD=intasect;";
            conn = new MySqlConnection(mysqlStr);
            return conn;
        }

        public DataTable selectMysql(string strSQL,DataTable dt)
        {
            try
            {
                conn.Open();
                com = new MySqlCommand(strSQL, conn);
                MySqlDataReader dr = com.ExecuteReader();
                while (dr.Read())
                {
                    DataRow row = dt.NewRow();
                     row[0] = dr.GetString("ACCOUNTSTREAMCODE"); 
                     row[1] = dr.GetString("PERSONCODE"); 
                     row[2] = dr.GetDecimal("COSTTOTAL"); 
                     row[3] = dr.GetDecimal("FULLEXPENSEPART"); 
                     row[4] = dr.GetDecimal("SELFPAYPART"); 
                     row[5] = dr.GetDecimal("ACCORDRANGEPART");
                     row[6] = dr.GetDecimal("PERSONPAYTOTAL"); 
                     row[7] = dr.GetDecimal("SIPAYTOTAL"); 
                     row[8] = dr.GetString("OPERATETIME"); 
                     row[9] = dr.GetString("LIQUIDATIONCENTER"); 
                    dt.Rows.Add(row);
                }
                return dt;
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
           
        }

        public int updateMysql(string strSql)
        {
            try
            {
                conn.Open();
                MySqlCommand comd = new MySqlCommand(strSql, conn);
                return comd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                conn.Close();
                conn.Dispose();
            }
        }


    }
}