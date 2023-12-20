using Newtonsoft.Json;
using System;
using System.Data;
using System.IO;
using System.Net;
using System.Text;

namespace WebScapperRendaVariavel
{
    internal class FuncoesGerais
    {
        public string encodeBase64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
        public string decodeBase64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }
        private static readonly Random random = new Random();
        public string randomString(int OpcionalLength = 0)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            char[] stringChars = new char[(OpcionalLength == 0) ? random.Next(0, 1000) : OpcionalLength];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }

        public string mWebRequest(string Url, string Method, string UserAgent = ".NET Framework project", WebHeaderCollection Headers = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(Url);
                request.Method = Method;
                request.UserAgent = UserAgent;
                if (Headers != null) { request.Headers = Headers; }

                using (HttpWebResponse iRequestResponse = (HttpWebResponse)request.GetResponse())
                {
                    using (StreamReader iResposeStreamReader = new StreamReader(iRequestResponse.GetResponseStream(), Encoding.UTF8))
                    {
                        return iResposeStreamReader.ReadToEnd();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }

        public DataTable JsonToTable(string Json)
        {
            DataTable iDataTable = JsonConvert.DeserializeObject<DataTable>(Json);
            return iDataTable;
        }

        public DataTable ConvertCsvStringToDataTable(bool isFilePath, string CSVContent, char CharDelimiter)
        {
            string[] Lines;
            if (isFilePath) { Lines = File.ReadAllLines(CSVContent); } else { Lines = CSVContent.Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries); }
            string[] Fields = Lines[0].Split(new char[] { CharDelimiter });
            int Cols = Fields.GetLength(0);
            DataTable dt = new DataTable();
            //1st row must be column names; force lower case to ensure matching later on.
            for (int i = 0; i < Cols; i++)
            {
                dt.Columns.Add(Fields[i].ToLower(), typeof(string));
            }
            DataRow Row;
            for (int i = 1; i < Lines.GetLength(0); i++)
            {
                Fields = Lines[i].Split(new char[] { CharDelimiter });
                Row = dt.NewRow();
                for (int f = 0; f < Cols; f++)
                    Row[f] = Fields[f];
                dt.Rows.Add(Row);
            }
            return dt;
        }


    }
}
