using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static WebScapperRendaVariavel.Investimentos;
using static WebScapperRendaVariavel.FundosImobiliario;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Data;

namespace WebScapperRendaVariavel
{
    internal class Investidor10
    {
        private static Functions funcoes = new Functions();

        public async Task ProcessaFiisInvestidor10(List<FundoImobiliario> fundosImobiliarios)
        {
            await ProcessaResultRequestInvestidor10(await FiisRequestsList(fundosImobiliarios), fundosImobiliarios);
        }      
        private static async Task<List<HttpResponseMessage>> FiisRequestsList(List<FundoImobiliario> fundosImobiliarios)
        {
            return await funcoes.SendRequestsAsync(CreateFiisRequestsList(fundosImobiliarios));                        
        }
        private static List<HttpClientRequest> CreateFiisRequestsList(List<FundoImobiliario> fundosImobiliarios)
        {
            List<HttpClientRequest> requestsList = (from FundoImobiliario fii in fundosImobiliarios
                                                    select new HttpClientRequest { url = $"https://investidor10.com.br/FIIS/{fii.Investimento.ticker}", method = HttpMethod.Get }
                                                    ).ToList();
            return requestsList;
        }
        private async Task ProcessaResultRequestInvestidor10(List<HttpResponseMessage> responseMessages, List<FundoImobiliario> fundosImobiliarios) {

            var doc = new HtmlDocument();

            foreach (HttpResponseMessage responseMessage in responseMessages)
            {
                if (!responseMessage.IsSuccessStatusCode) { continue; }
                
                string content = await responseMessage.Content.ReadAsStringAsync();
                doc.LoadHtml(content);

                FundoImobiliario investimentoConsultado = fundosImobiliarios.FirstOrDefault(f => f.Investimento.ticker == ExtractTickerFromUrl(responseMessage.RequestMessage.RequestUri.AbsoluteUri));
                if (investimentoConsultado == null) { continue; }

                setIndicators(investimentoConsultado, doc);
                setDividendHistory(investimentoConsultado, doc);
                SetGraphs(investimentoConsultado,doc);
            }
        }
        private string ExtractTickerFromUrl(string url)
        {
            return new Uri(url).Segments.Last().Trim('/').ToUpper();
        }        
        private static void setIndicators(FundoImobiliario fundo, HtmlDocument doc)
        {
            HtmlNodeCollection htmlNodeCollection = doc.DocumentNode.SelectNodes("//*[@id='table-indicators']/div[@class='cell']/div[@class='desc']");
            if (htmlNodeCollection == null) { return; }

            foreach (HtmlNode node in htmlNodeCollection)
            {
                HtmlNode spanNode = node.SelectSingleNode("span"); // DESCRIPTION 
                HtmlNode valueNode = node.SelectSingleNode("div[@class='value']/span"); // VALUE

                if (spanNode == null || valueNode == null) { continue; }

                string spanNodeText = spanNode.InnerText.Trim();
                string spanNodeValue = valueNode.InnerText.Trim();

                Console.WriteLine($"{spanNodeText} - {spanNodeValue}");
                if (spanNodeText == "CNPJ") { fundo.Investimento.cnpj = spanNodeValue; }
                else if (spanNodeText == "Razão Social") { fundo.Investimento.razaoSocial = spanNodeValue; }
                else if (spanNodeText == "SEGMENTO") { fundo.segmento = spanNodeValue; }
                else if (spanNodeText == "PÚBLICO-ALVO") { fundo.publicoAlvo = spanNodeValue; }
                else if (spanNodeText == "MANDATO") { fundo.mandato = spanNodeValue; }
                else if (spanNodeText == "TIPO DE FUNDO") { fundo.tipoDeFundo = spanNodeValue; }
                else if (spanNodeText == "PRAZO DE DURAÇÃO") { fundo.prazoDeDuração = spanNodeValue; }
                else if (spanNodeText == "TIPO DE GESTÃO") { fundo.tipoDeGestão = spanNodeValue; }
                else if (spanNodeText == "TAXA DE ADMINISTRAÇÃO") { fundo.taxaDeAdministracao = spanNodeValue; }
                else if (spanNodeText == "VACÂNCIA") { fundo.vacancia = spanNodeValue; }
                else if (spanNodeText == "NUMERO DE COTISTAS") { fundo.numeroDeCotistas = spanNodeValue; }
                else if (spanNodeText == "COTAS EMITIDAS") { fundo.cotasEmitidas = spanNodeValue; }
                else if (spanNodeText == "VAL. PATRIMONIAL P/ COTA") { fundo.valorPatrimonialPorCota = spanNodeValue; }
                else if (spanNodeText == "VALOR PATRIMONIAL") { fundo.valorPatrimonial = spanNodeValue; }
                else if (spanNodeText == "ÚLTIMO RENDIMENTO") { fundo.ultimoRendimento = spanNodeValue; }

            }
        }
        private void setDividendHistory(FundoImobiliario fundo, HtmlDocument doc)
        {
            HtmlNodeCollection htmlNodeCollection = doc.DocumentNode.SelectNodes("//*[@id='table-dividends-history']//tr//td");
            if (htmlNodeCollection == null) { return; }

            List<Dividend> dividendsHistoryList = new List<Dividend>();

            for (int i = 0; i < htmlNodeCollection.Count; i += 4)
            {
                string tipoDividendo = htmlNodeCollection[i].InnerText;
                string parsedDataCom = htmlNodeCollection[i + 1].InnerText;
                string parsedDataPagamento = htmlNodeCollection[i + 2].InnerText;
                Double paredValor = Double.Parse(htmlNodeCollection[i + 3].InnerText);
                Dividend dividendsHistory = new Dividend(tipoDividendo, parsedDataCom, parsedDataPagamento, paredValor);
                dividendsHistoryList.Add(dividendsHistory);
            }

            fundo.DividendHistory = dividendsHistoryList;
            return;
        }
        private void SetGraphs(FundoImobiliario fundo, HtmlDocument doc)
        {
            if (Int32.TryParse(ExtractId(doc), out int numValue))
            {
                string requestResult = GetCotacaoValuesFromGraph(numValue).Result;
                setCotacaoValuesFromGraph(fundo, requestResult);
            }
        }
        private string ExtractId(HtmlDocument doc)
        {
            Match match = Regex.Match(doc.Text, @"/chart/(\d+)/");
            if (match.Success) return match.Groups[1].Value.Trim();
            return "";
        }
        private async Task<string> GetCotacaoValuesFromGraph(int fundId)
        {
            return await funcoes.WebRequestAsync(new HttpClientRequest { url = $"https://investidor10.com.br/api/fii/cotacoes/chart/{fundId}/1825/real/adjusted/true", method = HttpMethod.Get });
        }
        private static void setCotacaoValuesFromGraph(FundoImobiliario fundo, string json)
        {
            if (!funcoes.IsValidJson(json)) { return; }

            List<Cotacao> cotacaoHistoryList = new List<Cotacao>();
            JObject jsonObject = JObject.Parse(json);

            if (!jsonObject.ContainsKey("real")) { return; }

            foreach (JToken item in jsonObject.Property("real").Values().ToList())
            {
                Cotacao cotacao = new Cotacao(item.Value<Double>("price"), item.Value<string>("created_at"));
                cotacaoHistoryList.Add(cotacao);
            }

            fundo.CotacaoHistory = cotacaoHistoryList;
            return;
        }

    }
}
