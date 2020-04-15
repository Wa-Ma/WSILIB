using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;



namespace WSILIB
{
    public static class Sqlhelper
    {
        public static DataTable Exesql(string mysql)
        {
            string mystr, sqlcomm;
            SqlConnection conn = new SqlConnection();
            //mystr = @"Data Source=.;Initial Catalog=WSILIB;Integrated Security=True";
            //
            mystr = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=WSILIB;Data Source=CXIST-KF-031PC\MSSQLSERVER01";
            conn.ConnectionString = mystr;
            conn.Open();

            SqlCommand cmd = new SqlCommand(mysql, conn);
            //cmd.CommandText = mysql;

            sqlcomm = firststr(mysql);

            if (sqlcomm == "INSERT" || sqlcomm == "DELETE" || sqlcomm == "UPDATE")
            {
                cmd.ExecuteNonQuery();
                conn.Close();
                return null;
            }
            else
            {
                DataSet myds = new DataSet();
                SqlDataAdapter myadp = new SqlDataAdapter();
                myadp.SelectCommand = cmd;
                cmd.ExecuteNonQuery();         //执行查询
                conn.Close();                  //关闭连接
                myadp.Fill(myds);                //填充数据
                return myds.Tables[0];           //返回表对象
            }
        }

        private static string firststr(string mystr)      //提取字符串中的第一个字符串
        {
            string[] strarr;
            strarr = mystr.Split(' ');
            return strarr[0].ToUpper().Trim();
        }
        public static int ExecuteNonQuery(string mysql)
        {
            int result = 0;
            string mystr;
            SqlConnection conn = new SqlConnection();
            mystr = @"Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=WSILIB;Data Source=CXIST-KF-031PC\MSSQLSERVER01";
            conn.ConnectionString = mystr;
            conn.Open();
            SqlCommand cmd = new SqlCommand(mysql, conn);
            result = cmd.ExecuteNonQuery();
            conn.Close();
            return result;
        }
    }

}
