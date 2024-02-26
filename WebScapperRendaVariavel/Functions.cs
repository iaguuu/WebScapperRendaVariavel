using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Data.SqlClient;
using System.Data;

namespace WebScapperRendaVariavel
{
    public class HttpClientRequest
    {
        public string url { get; set; }
        public HttpMethod method { get; set; }
        public string userAgent { get; set; }
        public WebHeaderCollection headers { get; set; }
    }
    internal class Functions
    {
        private static readonly Random random = new Random();
        private static readonly HttpClient client = new HttpClient();

        public string encodeBase64(string value)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(value));
        }
        public string decodeBase64(string value)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(value));
        }

        public string randomString(int OpcionalLength = 0)
        {
            string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZÇabcdefghijklmnopqrstuvwxyç0123456789";
            char[] stringChars = new char[(OpcionalLength == 0) ? random.Next(0, 1000) : OpcionalLength];
            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }
            return new string(stringChars);
        }
        public async Task<List<HttpResponseMessage>> SendRequestsAsync(List<HttpClientRequest> requests)
        {

            var responses = new List<HttpResponseMessage>();

            using (var httpClient = client)
            {
                foreach (var request in requests)
                {
                    var httpRequestMessage = new HttpRequestMessage(request.method, request.url);

                    if (!string.IsNullOrEmpty(request.userAgent))
                    {
                        httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(request.userAgent);
                    }

                    if (request.headers != null)
                    {
                        foreach (var header in request.headers.AllKeys)
                        {
                            request.headers.Add(header, request.headers[header]);
                        }
                    }

                    var response = await httpClient.SendAsync(httpRequestMessage);
                    responses.Add(response);
                }
            }
            return responses;
        }

        public async Task<string> WebRequestAsync(HttpClientRequest HttpClientRequest)
        {
            try
            {
                using (var httpClient = client)
                {
                    var request = new HttpRequestMessage(HttpClientRequest.method, HttpClientRequest.url);
                    if (HttpClientRequest.headers != null)
                    {
                        foreach (var header in HttpClientRequest.headers.AllKeys)
                        {
                            request.Headers.Add(header, HttpClientRequest.headers[header]);
                        }
                    }

                    var response = await httpClient.SendAsync(request);

                    using (var stream = await response.Content.ReadAsStreamAsync())
                    using (var reader = new StreamReader(stream, Encoding.UTF8))
                    {
                        return await reader.ReadToEndAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }
        public string TextToFindBetween(string source, Regex regexPattern, int index = 0)
        {
            Match match = regexPattern.Match(source);
            if (!match.Success) { return ""; }
            return match.Groups[index].Value;
        }
        public string ReplaceLastOccurrence(string source, string find, string replace)
        {
            int place = source.LastIndexOf(find);
            if (place == -1) return source;
            return source.Remove(place, find.Length).Insert(place, replace);
        }

        public bool IsValidJson(string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) { return false; }
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || (strInput.StartsWith("[") && strInput.EndsWith("]")))
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

       // public string WebRequest(HttpClientRequest HttpClientRequest)
       // {
       //     try
       //     {
       //         HttpWebRequest request = (HttpWebRequest)WebRequest.Create(HttpClientRequest.url);
       //         request.Method = HttpClientRequest.method.ToString();
       //         if (request.UserAgent != null) { request.UserAgent = HttpClientRequest.userAgent.ToString(); }
       //         if (HttpClientRequest.headers != null) { request.Headers = HttpClientRequest.headers; }
       //
       //         using (HttpWebResponse iRequestResponse = (HttpWebResponse)request.GetResponse())
       //         {
       //             using (StreamReader iResposeStreamReader = new StreamReader(iRequestResponse.GetResponseStream(), Encoding.UTF8))
       //             {
       //                 return iResposeStreamReader.ReadToEnd();
       //             }
       //         }
       //     }
       //     catch (Exception ex)
       //     {
       //         return $"Erro: {ex.Message}";
       //     }
       // }
        private static string GetDataBaseServerConnection(string dataBase, string server = "MSSQL")
        {
            return $"Server=DESKTOP-2MCFPNQ;Database={dataBase};Trusted_Connection=True;";
        }
        public string ExecuteSqlBulkCopy(DataTable dataTable, string dataBase, string destinationTable, List<string> arrayColumns, string server = "MSSQL")
        {
            try
            {
                using (SqlBulkCopy sqlBulkCopy = new SqlBulkCopy(GetDataBaseServerConnection(dataBase, server)))
                {
                    sqlBulkCopy.BatchSize = 10000;
                    sqlBulkCopy.BulkCopyTimeout = 0;
                    sqlBulkCopy.DestinationTableName = destinationTable;
                    for (int i = 0; i <= dataTable.Columns.Count - 1; i++)
                    {
                        sqlBulkCopy.ColumnMappings.Add(dataTable.Columns[i].Caption, arrayColumns[i].ToString());
                    }
                    sqlBulkCopy.WriteToServer(dataTable);
                    return "Sucesso";
                }
            }
            catch (Exception ex)
            {
                return $"Erro: {ex.Message}";
            }
        }
        public DataSet ExecuteQueryDataSetSql(string query, string dataBase, string server = "MSSQL")
        {
            DataSet dSet = new DataSet();
            try
            {
                using (SqlConnection dbConn = new SqlConnection(GetDataBaseServerConnection(dataBase, server)))
                {
                    dbConn.Open();
                    using (SqlDataAdapter da = new SqlDataAdapter(query, dbConn))
                    {
                        da.SelectCommand.CommandTimeout = 600;
                        da.Fill(dSet);
                    }
                }
                return dSet;
            }
            catch (Exception)
            {
                return dSet;
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

        public void WriteTextToTxt(string fullFilePath, string fileName, string text)
        {
            try
            {
                DirectoryInfo ObjSearchDir = new DirectoryInfo(fullFilePath);
                if (!ObjSearchDir.Exists) { ObjSearchDir.Create(); }

                using (StreamWriter sw = new StreamWriter(Path.Combine(fullFilePath,fileName), true))
                {
                    sw.WriteLine(text);
                }
            }
            catch (Exception ex)
            {
                _ = ex.Message;
            }
        }
    }
}
