using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net;
using Newtonsoft.Json;
using System.Security.Policy;
using HtmlAgilityPack;
using System.Xml.Linq;

namespace WebScapperRendaVariavel
{
    public enum tiposAtivo { ACAO, FII, BDRS, rendaFixa };
    internal class Program
    {
        private static FuncoesGerais funcoes = new FuncoesGerais();
        static void Main(string[] args)
        {

           ConsultaFundamentus();
            
            //ListarFiis();

            //.Replace(@"""", "")
            //  Thread thread1 = new Thread(() => { ListarFiis(); });
            //  Thread thread2 = new Thread(() => { ListarFiis(); });
            //  Thread thread3 = new Thread(() => { ListarFiis(); });

        } 
        protected static void ListarFiis()
        {
            string[] fundUrls = {
                "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6NywicGFnZU51bWJlciI6MSwicGFnZVNpemUiOjIwfQ==",
                "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6MjcsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0=",
                "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6MzQsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0="
            };

            DataTable mergedTable = new DataTable();

            foreach (string fundUrl in fundUrls)
            {
                string requestResult = funcoes.mWebRequest(fundUrl, "GET");
                string plainText = funcoes.decodeBase64(requestResult.Trim('"'));
                DataTable fundTable = funcoes.ConvertCsvStringToDataTable(false, plainText, ';');
                mergedTable.Merge(fundTable);
            }

        }
        protected static DataTable ConsultaFundamentus()
        {
            string requestResult = funcoes.mWebRequest("https://www.fundamentus.com.br/resultado.php", "GET").Replace("\\", "").Trim('"');
            DataTable requestResultDataTable = ParseHtmlTableToDataTable(requestResult, "//*[@id=\"resultado\"]");
            return requestResultDataTable;
        }
        protected static DataTable ConsultaInvestidor10(tiposAtivo tipo, string ticker)
        {
            string requestResult = funcoes.mWebRequest($"https://investidor10.com.br/{tipo}/{ticker}/", "GET");
            DataTable requestResultDataTable = funcoes.JsonToTable(requestResult.Replace("\\", "").Trim('"'));
            return requestResultDataTable;
        }
        protected static DataTable ConsultaFundsExplorer()
        {
            WebHeaderCollection customHeaders = new WebHeaderCollection { { "x-funds-nonce", "61495f60b533cc40ad822e054998a3190ea9bca0d94791a1da" } };
            string requestResult = funcoes.mWebRequest("https://www.fundsexplorer.com.br/wp-json/funds/v1/get-ranking", "GET", Headers: customHeaders);
            DataTable requestResultDataTable = funcoes.JsonToTable(requestResult.Replace("\\", "").Trim('"'));
            return requestResultDataTable;
        }       

        protected static DataTable ParseHtmlTableToDataTable(string html, string attributePath)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            HtmlNodeCollection tableHeaders = doc.DocumentNode.SelectNodes($"{attributePath}//tr/th");
            HtmlNodeCollection tableRows = doc.DocumentNode.SelectNodes($"{attributePath}//tr[td]");
            return CreateTableFromHtml(tableHeaders, tableRows);
        }
        protected static DataTable CreateTableFromHtml(HtmlNodeCollection headers, HtmlNodeCollection rows)
        {
            DataTable table = new DataTable();
            foreach (HtmlNode header in headers) { table.Columns.Add(header.InnerText); }
            foreach (var row in rows) { table.Rows.Add(row.SelectNodes("td").Select(td => td.InnerText).ToArray()); }
            return table;
        }
    }
}
