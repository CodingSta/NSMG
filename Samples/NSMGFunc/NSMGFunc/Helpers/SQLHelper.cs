﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Data;
using System.Data.SqlClient;

namespace NSMGFunc.Helpers
{
    public static class SQLHelper
    {
        private static string SQLConnectString = Environment.GetEnvironmentVariable("SQLDatabaseEndpoint");

        public static DataSet RunSQL(string query)
        {
            DataSet ds = new DataSet();

            SqlConnection con = new SqlConnection(SQLConnectString);
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            adapter.Fill(ds);

            return ds;
        }

        public static DataSet RunSQL(string query, SqlParameter[] para)
        {
            DataSet ds = new DataSet();

            SqlConnection con = new SqlConnection(SQLConnectString);
            SqlCommand cmd = new SqlCommand(query, con);
            SqlDataAdapter adapter = new SqlDataAdapter(cmd);

            //파라메터의 반영
            cmd.Parameters.Clear();
            foreach (SqlParameter p in para)
            {
                cmd.Parameters.Add(p);
            }

            adapter.Fill(ds);

            return ds;
        }

        public static void ExecuteNonQuery(string query)
        {
            DataSet ds = new DataSet();

            SqlConnection con = new SqlConnection(SQLConnectString);
            SqlCommand cmd = new SqlCommand(query, con);

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

        public static void ExecuteNonQuery(string query, SqlParameter[] para)
        {
            DataSet ds = new DataSet();

            SqlConnection con = new SqlConnection(SQLConnectString);
            SqlCommand cmd = new SqlCommand(query, con);

            //파라메터의 반영
            cmd.Parameters.Clear();
            foreach (SqlParameter p in para)
            {
                cmd.Parameters.Add(p);
            }

            con.Open();
            cmd.ExecuteNonQuery();
            con.Close();
        }

    }
}