using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using static WebScapperRendaVariavel.FundosImobiliario;

namespace WebScapperRendaVariavel
{
    internal class B3
    {
        private static Functions funcoes = new Functions();

        private static readonly List<HttpClientRequest> requestsFundosImobiliariosB3 = new List<HttpClientRequest>()
        {
            new HttpClientRequest { url = "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6NywicGFnZU51bWJlciI6MSwicGFnZVNpemUiOjIwfQ==", method = HttpMethod.Get },
            new HttpClientRequest { url = "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6MjcsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0=", method = HttpMethod.Get },
            new HttpClientRequest { url = "https://sistemaswebb3-listados.b3.com.br/fundsProxy/fundsCall/GetListFundDownload/eyJ0eXBlRnVuZCI6MzQsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0=", method = HttpMethod.Get }
        };
        public static Task ConsultarFundosImobiliariosB3()
        {
            SaveFundosImobiliariosB3(GetFundosImobiliariosB3(requestsFundosImobiliariosB3).Result);
            return Task.CompletedTask;
        }
        private static async Task<DataTable> GetFundosImobiliariosB3(List<HttpClientRequest> httpClientRequests)
        {
            DataTable mergedTable = new DataTable();

            foreach (HttpResponseMessage responseMessage in await funcoes.SendRequestsAsync(httpClientRequests))
            {
                if (!responseMessage.IsSuccessStatusCode) { continue; } //marcar log

                string content = await responseMessage.Content.ReadAsStringAsync();
                
                if (string.IsNullOrEmpty(content)) { continue; } //marcar log

                string requestResultDecoded = funcoes.decodeBase64(content.Trim('"'));
                DataTable result = funcoes.ConvertCsvStringToDataTable(false, requestResultDecoded, ';');

                DataColumn newColumn = new DataColumn("TIPO_ATIVO", typeof(System.String));
                newColumn.DefaultValue = TipoAtivo(responseMessage.RequestMessage.RequestUri.AbsoluteUri);
                result.Columns.Add(newColumn);
            
                mergedTable.Merge(result);

            }
            return mergedTable;        
        }
        private static string TipoAtivo(string urlRequest)
        {        
            if (urlRequest.Contains("eyJ0eXBlRnVuZCI6NywicGFnZU51bWJlciI6MSwicGFnZVNpemUiOjIwfQ==")) { return "FII"; }  //0 FII
            else if (urlRequest.Contains("eyJ0eXBlRnVuZCI6MjcsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0=")) { return "FII_INFRA"; } //1 FII_INFRA
            else if (urlRequest.Contains("eyJ0eXBlRnVuZCI6MzQsInBhZ2VOdW1iZXIiOjEsInBhZ2VTaXplIjoyMH0=")) { return "FII_AGRO"; } //2 FII_AGRO
            return "";
        }
        private static void SaveFundosImobiliariosB3(DataTable listaFundosB3)
        {
            if (listaFundosB3.Rows.Count == 0) { return; } //marcar log

            string result = funcoes.ExecuteSqlBulkCopy(listaFundosB3, "RENDA_VARIAVEL", "FUNDOS_LISTADOS", new List<string> { "RAZAO_SOCIAL", "FUNDO", "SEGMENTO", "TICKER", "TIPO_ATIVO" });

            if (result != "Sucesso")
            {
                funcoes.WriteTextToTxt("Logs","Erro.txt", $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff")} - {result}"); // marcar log 
            }
        }
        public static List<FundoImobiliario> ListarFundosImobiliarios() {
            List<FundoImobiliario> fundosImobiliarios = new List<FundoImobiliario>();
            DataSet dtFundos = funcoes.ExecuteQueryDataSetSql("SELECT TOP 1 TICKER FROM [RENDA_VARIAVEL].[dbo].[FUNDOS_LISTADOS](NOLOCK) WHERE TICKER = 'ALZR'", "RENDA_VARIAVEL");
            fundosImobiliarios = (from DataRow dr in dtFundos.Tables[0].Rows 
                                        select new FundoImobiliario(
                                             dr["TICKER"].ToString().EndsWith("11") ? dr["TICKER"].ToString(): dr["TICKER"].ToString() + "11" 
                                            ,dr["TIPO_ATIVO"].ToString())
                                        ).ToList();
            return fundosImobiliarios;
        }

    }
}
