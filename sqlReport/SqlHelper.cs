using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Collections;
using System.Data.SqlClient;
using System.Data;

namespace SqlManager
{
    class SqlHelper : IDisposable
    {
        SqlConnection conn;
        SqlCommand cmd;
        string connString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\Project\CSharp\SqlReport\sqlReport\example.mdf;Integrated Security=True";


        public SqlHelper()
        {
        }
        public SqlHelper(string connectString)
        {
            this.connString = "Data Source=" + connectString;
        }
        public void open()
        {
            try
            {
                conn = new SqlConnection(connString);
                cmd = conn.CreateCommand();
                cmd.Connection = conn;
                //cmd.CommandType = System.Data.CommandType.Text;
                conn.Open();
            }
            catch //(DataException e)
            {
                //打开数据库失败
            }

        }
        public void close()
        {
            conn.Close();
            conn.Dispose();
        }
        public void Dispose()
        {
            if (null != conn)
            {
                conn.Close();
                conn.Dispose();
            }
            cmd.Dispose();
        }
        /// <summary>
        /// 数据逐条读取
        /// </summary>
        /// <param name="sql">读取语句（select语句）</param>
        /// <returns>读取reader（读取完成后需要手动关闭，reader.Close()）</returns>
        public SqlDataReader ExecuteReader(string sql)
        {
            cmd.CommandText = sql;
            SqlDataReader reader = null;
            try
            {
                reader = cmd.ExecuteReader();
            }
            catch (DataException e)
            {
                //
            }
            return reader;
        }
        /// <summary>
        /// 按数据集合查询
        /// </summary>
        /// <param name="sql">查询语句（select语句）</param>
        /// <returns>返回数据集合</returns>
        public DataSet ExecuteDataSet(string sql)
        {
            cmd.CommandText = sql;
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
            DataSet ds = new DataSet();
            try
            {
                adapter.Fill(ds, "Table");
            }
            catch
            {

            }
            return ds;
        }
        /// <summary>
        /// 数据操作请求（增删查改）
        /// </summary>
        /// <param name="sql">操作语句（select/insert/update/delete）</param>
        /// <returns>返回受影响的行数</returns>
        public int ExecuteNonQuery(string sql)
        {
            cmd.CommandText = sql;
            int ret = -1;
            try
            {
                ret = cmd.ExecuteNonQuery();
            }
            catch { }
            return ret;
        }
        /// <summary>
        /// 开始事务
        /// </summary>
        public void BeginTransaction()
        {
            cmd.Transaction = conn.BeginTransaction();
        }
        /// <summary>
        /// 提交事务
        /// </summary>
        public void CommitTransaction()
        {
            cmd.Transaction.Commit();
        }
        /// <summary>
        /// 回滚事务
        /// </summary>
        public void RollBackTranscation()
        {
            cmd.Transaction.Rollback();
        }

        /// <summary>
        /// 插入语句生成
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="colNames">列名以逗号分隔（示例：id,name,value）</param>
        /// <param name="colValues">列值以逗号分隔（示例：11,'1221','2233'）</param>
        /// <returns></returns>
        public string insertFormat(string table, string colNames, string colValues)
        {
            return string.Format(@"Insert into {0} ({1}) values ({2})", table, colNames, colValues);
        }
        /// <summary>
        /// 更新语句生成
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="cols">列字段表达式（示例：name='six',value='111'）</param>
        /// <param name="condition">更新条件表达式（示例：id=1）</param>
        /// <returns></returns>
        public string updateFormat(string table, string cols, string condition)
        {
            return string.Format(@"update {0} set {1} where {2}", table, cols, condition);
        }
        /// <summary>
        /// 删除语句生成
        /// </summary>
        /// <param name="table">表名</param>
        /// <param name="condition">条件表达式（示例：id=1）</param>
        /// <returns></returns>
        public string deleteFormat(string table, string condition)
        {
            return string.Format(@"delete from {0} where {1}", table, condition);
        }


    }
}
