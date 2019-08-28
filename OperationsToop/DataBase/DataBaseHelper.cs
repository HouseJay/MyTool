using OperationsTool.Tool;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace OperationsTool.DataBase
{
    class DataBaseHelper
    {
        //     try
        //        {
        //            OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strSourcePath);
        //    OleDbCommand cmd = conn.CreateCommand();
        //    cmd.CommandText = strExetuceOrder;
        //            conn.Open();
        //            OleDbDataReader dr = cmd.ExecuteReader();
        //    DataTable dt = new DataTable();
        //            if (dr.HasRows)
        //            {
        //                for (int i = 0; i<dr.FieldCount; i++)
        //                {
        //                    dt.Columns.Add(dr.GetName(i));
        //                }
        //dt.Rows.Clear();
        //            }
        //            while (dr.Read())
        //            {
        //                DataRow row = dt.NewRow();
        //                for (int i = 0; i<dr.FieldCount; i++)
        //                {
        //                    row[i] = dr[i];
        //                }
        //                dt.Rows.Add(row);
        //            }
        //            cmd.Dispose();
        //            conn.Close();
        //            return dt;
        //        }
        //        catch(Exception ex)
        //        {
        //            Console.WriteLine(ex);
        //            return new DataTable();
        //        }
        OleDbConnection conn;
        OleDbCommand cmd;

        /// <summary>
        /// 连接数据库
        /// </summary>
        /// <param name="strSourcePath"></param>
        /// <returns></returns>
        public Boolean connection(string strSourcePath)
        {
            try
            {
                if (conn == null)
                {
                    conn = new OleDbConnection("Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + strSourcePath);
                }
                if (cmd == null)
                {
                    cmd = conn.CreateCommand();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        //测试状态是否正常连接;
        public Boolean checkConn()
        {
            if (conn.State == ConnectionState.Closed || conn.State == ConnectionState.Broken)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// 查询数据
        /// </summary>
        /// <param name="strExetuceOrder">sql</param>
        /// <returns></returns>
        public DataTable selectData(string strExetuceOrder)
        {
            try
            {
                cmd.CommandText = strExetuceOrder;
                conn.Open();
                OleDbDataReader odr = cmd.ExecuteReader();
                DataTable dt = new DataTable();
                if (odr.HasRows)
                {
                    for (int i = 0; i < odr.FieldCount; i++)
                    {
                        dt.Columns.Add(odr.GetName(i));
                    }
                    dt.Rows.Clear();
                }

                while (odr.Read())
                {
                    DataRow row = dt.NewRow();
                    for (int i = 0; i < odr.FieldCount; i++)
                    {
                        row[i] = odr[i];
                    }
                    dt.Rows.Add(row);
                }
                cmd.Dispose();
                conn.Close();
                return dt;
            }
            catch (Exception ex)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + ex.ToString(), LOGSTATE.Error);
                return new DataTable();
            }
        }

        /// <summary>
        /// 执行简单sql
        /// </summary>
        /// <param name="strSQL"></param>
        /// <returns></returns>
        public int excuteSQL(string strSQL)
        {
            try
            {
                cmd = conn.CreateCommand();
                cmd.CommandText = strSQL;
                conn.Open();
                return cmd.ExecuteNonQuery();
            }
            catch (InvalidOperationException e)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + e.ToString(), LOGSTATE.Error);
                return -1;
            }
            catch (Exception ex)
            {
                LogWriter.WriteLogInfo(DateTime.Now.ToString() + ":" + ex.ToString(), LOGSTATE.Error);
                return -1;
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
        }

        /// <summary>
        /// 执行SQL语句，设置命令的执行等待时间
        /// </summary>
        /// <returns></returns>
        /// 
        public int ExecuteSqlByTime(string SQLString, int Times)
        {
            try
            {
                cmd = conn.CreateCommand();
                conn.Open();
                cmd.CommandTimeout = Times;
                int rows = cmd.ExecuteNonQuery();
                return rows;
            }
            catch (System.Data.OleDb.OleDbException E)
            {
                conn.Close();
                throw new Exception(E.Message);
            }
        }

        /// <summary>
            /// 执行多条SQL语句，实现数据库事务。
            /// </summary>
            /// <param name="SQLStringList">多条SQL语句</param>
        public void ExecuteSqlTran(ArrayList SQLStringList)
        {
            conn.Open();
            OleDbCommand cmd = new OleDbCommand();
            cmd.Connection = conn;
            OleDbTransaction tx = conn.BeginTransaction();
            cmd.Transaction = tx;
            try
            {
                for (int n = 0; n < SQLStringList.Count; n++)
                {
                    string strsql = SQLStringList[n].ToString();
                    if (strsql.Trim().Length > 1)
                    {
                        cmd.CommandText = strsql;
                        cmd.ExecuteNonQuery();
                    }
                }
                tx.Commit();
            }
            catch (System.Data.OleDb.OleDbException E)
            {
                tx.Rollback();
                throw new Exception(E.Message);
            }
            finally
            {
                cmd.Dispose();
                conn.Close();
            }
        }

    }
}