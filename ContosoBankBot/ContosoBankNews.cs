using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ContosoBankBot
{
    public static class ContosoBankNews
    {
        public static Activity GetLatestNews(Activity message)
        {
            WebRequest request = WebRequest.Create(@"https://contosobank-easytables.azurewebsites.net/tables/BankNews");
            request.Method = "GET";
            request.ContentType = "application/json";
            request.Headers.Add("ZUMO-API-VERSION", "2.0.0");

            var response = (HttpWebResponse)request.GetResponse();
            StreamReader streamReader = new StreamReader(response.GetResponseStream());
            string responceData = streamReader.ReadToEnd();
            streamReader.Close();
            response.Close();

            List<RootObject> listRootObjects = JsonConvert.DeserializeObject<List<RootObject>>(responceData);

            Activity result = message.CreateReply();
            result.Recipient = message.From;
            result.Type = "message";
            result.Attachments = new List<Attachment>();

            foreach (var rootObject in listRootObjects)
            {
                DateTime newsDate;
                if (!DateTime.TryParse(rootObject.UpdatedAt, out newsDate))
                    newsDate = DateTime.Today;

                HeroCard plCard = new HeroCard()
                {
                    Title = rootObject.Title,
                    Subtitle = newsDate.ToString("d"),
                    Text = rootObject.Text
                };
                Attachment plAttachment = plCard.ToAttachment();
                result.Attachments.Add(plAttachment);
            }

            return result;
        }

        private class RootObject
        {
            public string Id { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
            public string Version { get; set; }
            public bool Deleted { get; set; }
            public string Title { get; set; }
            public string Text { get; set; }
        }
    }
}