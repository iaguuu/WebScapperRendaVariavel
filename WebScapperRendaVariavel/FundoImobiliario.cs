
using System;
using System.Collections.Generic;
using static WebScapperRendaVariavel.Investimentos;

namespace WebScapperRendaVariavel
{
    internal class FundosImobiliario
    {
        public class FundoImobiliario
        {
            public Investimento Investimento { get; set; }
            public string segmento { get; set; }
            public string publicoAlvo { get; set; }
            public string mandato { get; set; }
            public string tipoDeFundo { get; set; }
            public string prazoDeDuração { get; set; }
            public string tipoDeGestão { get; set; }
            public string taxaDeAdministracao { get; set; }
            public string vacancia { get; set; }
            public string numeroDeCotistas { get; set; }
            public string cotasEmitidas { get; set; }
            public string valorPatrimonialPorCota { get; set; }
            public string valorPatrimonial { get; set; }
            public string ultimoRendimento { get; set; }
            public List<Dividend> DividendHistory { get; set; } = new List<Dividend>();
            public List<Cotacao> CotacaoHistory { get; set; } = new List<Cotacao>();

            public FundoImobiliario(string ticker, string tipoAtivo)
            {
                if (!ticker.EndsWith("11")) { throw new ArgumentException("O ticker do fundo imobiliário deve terminar com '11';"); }
                Investimento = new Investimento(ticker, tipoAtivo);
            }
        }

    }
}
