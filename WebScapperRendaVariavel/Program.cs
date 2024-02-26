using HtmlAgilityPack;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static WebScapperRendaVariavel.Functions;
using static WebScapperRendaVariavel.B3;
using static WebScapperRendaVariavel.FundosImobiliario;

namespace WebScapperRendaVariavel
{
    internal class Program
    {
        private static Investidor10 funcoes = new Investidor10();
        static async Task Main(string[] args)
        {
           
             List<FundoImobiliario> fundosImobiliarios = new List<FundoImobiliario>();// ListarFundosImobiliarios();
             FundosImobiliario.FundoImobiliario fundo = new FundosImobiliario.FundoImobiliario("ALZR11","FII");
             FundosImobiliario.FundoImobiliario fundo1 = new FundosImobiliario.FundoImobiliario("JURO11", "FII");
             FundosImobiliario.FundoImobiliario fundo2 = new FundosImobiliario.FundoImobiliario("RURA11", "FII");
             fundosImobiliarios.Add(fundo);
             fundosImobiliarios.Add(fundo1);
             fundosImobiliarios.Add(fundo2);
             await funcoes.ProcessaFiisInvestidor10(fundosImobiliarios);
             var teste = "";
            // Investidor10.ProcessaFiisInvestidor10(fundosImobiliarios);

            //static can be used without declaration of class

            // Functions.HttpClientRequest request = new HttpClientRequest { url = $"https://investidor10.com.br/FIIS/{fii}", method = HttpMethod.Get };
            // Teste(new ClasseInvestimento("", tipoAtivo.FIIS));
            //ListarFiisB3();
            //(true)?1:2; IIF IN LINE
            //  Thread thread1 = new Thread(() => { ListarFiis(); });
            //  Thread thread2 = new Thread(() => { ListarFiis(); });
            //.Replace(@"""", "")

            //CultureInfo enUs = new CultureInfo("en-US");
            //var doc = new HtmlDocument();
            //doc.LoadHtml(File.ReadAllText("C:\\Users\\Iago\\source\\repos\\WebScapperRendaVariavel\\WebScapperRendaVariavel\\alzr.txt"));
        }


        public async Task runApi()
        {
            await Task.WhenAll(ConsultarFundosImobiliariosB3());
        }

    }
}
