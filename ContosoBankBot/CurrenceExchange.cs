using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ContosoBankBot
{
    public static class CurrenceExchange
    {
        public static Activity GetCurrencyExchangeInfo(Activity message)
        {
            WebRequest request = WebRequest.Create(@"http://api.fixer.io/latest?base=NZD&symbols=AUD,USD,GBP,RUB");
            var response = (HttpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responceData = streamReader.ReadToEnd();
            streamReader.Close();
            response.Close();

            RatesRootObject r = JsonConvert.DeserializeObject<RatesRootObject>(responceData);

            DateTime rateDate;
            if (!DateTime.TryParse(r.date, out rateDate))
                rateDate = DateTime.Today;

            Activity result = message.CreateReply();
            result.Recipient = message.From;
            result.Type = "message";
            result.Attachments = new List<Attachment>();

            HeroCard plCard = new HeroCard()
            {
                Title = "Foreign currency exchange rates",
                Subtitle = rateDate.ToString("d"),
                Text = $"1 NZD = {r.rates.USD} USD\n     {r.rates.AUD} AUD\n     {r.rates.GBP} GBP\n     {r.rates.RUB} RUB"
            };
            Attachment plAttachment = plCard.ToAttachment();
            result.Attachments.Add(plAttachment);

            return result;
        }

        private class Rates
        {
            public double AUD { get; set; }
            public double GBP { get; set; }
            public double RUB { get; set; }
            public double USD { get; set; }
        }

        private class RatesRootObject
        {
            public string @base { get; set; }
            public string date { get; set; }
            public Rates rates { get; set; }
        }
    }
}