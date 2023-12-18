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

        public string mWebRequest(string pUrl, string pMethod, string pUserAgent = ".NET Framework project", WebHeaderCollection pHeaders = null)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(pUrl);
                request.Method = pMethod;
                request.UserAgent = pUserAgent;

                if (pHeaders != null) { request.Headers = pHeaders; }

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

        public DataTable mJsonToTable(string pJson)
        {
            DataTable iDataTable = JsonConvert.DeserializeObject<DataTable>(pJson);
            return iDataTable;
        }

    }
}
