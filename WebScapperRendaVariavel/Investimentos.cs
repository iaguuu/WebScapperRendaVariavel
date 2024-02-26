using System;
using System.Security.Cryptography;

namespace WebScapperRendaVariavel
{
    internal class Investimentos
    {
        public enum tipoAtivo
        {
            FII,
            FII_INFRA,
            FII_AGRO,
            ACOES,
            BDRS
        }
        public enum tipoDividendo
        {
            JCP,
            DIVIDENDO
        }
        public class Investimento
        {
            public string ticker { get; set; }
            public tipoAtivo tipoAtivo { get; set; }
            public string cnpj { get; set; }
            public string razaoSocial { get; set; }

            public Investimento(string ticker, string tipoAtivo)
            {
                if (string.IsNullOrEmpty(ticker)) { throw new ArgumentException("Ticker vazio ou em branco. Certifique-se de preenchê-lo"); }
                this.ticker = ticker;
                this.tipoAtivo = StringParaTipotipoAtivo(tipoAtivo);
            }
        }
        public class Dividend
        {
            public tipoDividendo tipoDividendo { get; set; }
            public DateTime dataCom { get; set; }
            public DateTime dataPagamento { get; set; }
            public Double valor { get; set; }
           
            public Dividend(string tipoDividendo, string dataCom, string dataPagamento, Double valor)
            {
                this.tipoDividendo = StringParaTipoDividendo(tipoDividendo);
                this.dataCom = stringDateToDateTime(dataCom);
                this.dataPagamento = stringDateToDateTime(dataPagamento);
                this.valor = valor;
            }
        }
        public class Cotacao
        {
            public Double valor { get; set; }
            public DateTime data { get; set; }

            public Cotacao(Double valor, string data)
            {
                this.valor = valor;
                this.data = stringDateToDateTime(data);
            }
        }
        private protected static DateTime stringDateToDateTime(string stringDate)
        {
            DateTime parsedDate;
            if (DateTime.TryParseExact(stringDate, new string[] { "dd/MM/yyyy", "yyyy-MM-dd" }, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out parsedDate))
            {
                return DateTime.ParseExact(parsedDate.ToString("yyyy-MM-dd"), "yyyy-MM-dd", System.Globalization.CultureInfo.CurrentCulture);
            }
            else
            {
                throw new ArgumentException("Data inválida. Certifique-se de fornecer uma data no formato 'dd/MM/yyyy' ou 'yyyy-MM-dd'.");
            }
        }
        private protected static tipoDividendo StringParaTipoDividendo(string texto)
        {
            texto = texto.ToUpper();
            if (texto == "JCP") return tipoDividendo.JCP;
            else if (texto == "DIVIDENDOS") return tipoDividendo.DIVIDENDO;
            throw new ArgumentException($"Tipo de dividendo inválido: {texto}");
        }
        private protected static tipoAtivo StringParaTipotipoAtivo(string texto)
        {
            texto = texto.ToUpper();
            if (texto == "FII") return tipoAtivo.FII;
            else if (texto == "FII_INFRA") return tipoAtivo.FII_INFRA;
            else if (texto == "FII_AGRO") return tipoAtivo.FII_AGRO;
            else if (texto == "ACOES") return tipoAtivo.ACOES;
            else if (texto == "BDRS") return tipoAtivo.BDRS;
            throw new ArgumentException($"Tipo de ativo inválido: {texto}");
        }
    }
}
