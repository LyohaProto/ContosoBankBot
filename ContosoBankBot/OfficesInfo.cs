using System.Collections.Generic;
using System.IO;
using System.Net;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ContosoBankBot
{
    public static class OfficesInfo
    {
        public static Activity GetOfficesLocations(Activity message, string city)
        {
            WebRequest request = WebRequest.Create(@"https://contosobank-easytables.azurewebsites.net/tables/OfficeLocations");
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

            bool noOfficesFound = true;

            foreach (var rootObject in listRootObjects)
            {
                if (rootObject.City.ToLower() == city.ToLower())
                {
                    noOfficesFound = false;
                    List<CardAction> cardButtons = new List<CardAction>();
                    result.Attachments.Add(new HeroCard()
                    {
                        Title = "Office in " + rootObject.City,
                        Subtitle = rootObject.Location,
                        Text = "Phone: " + rootObject.Phone,
                        Buttons = cardButtons
                    }.ToAttachment());

                    cardButtons.Add(new CardAction()
                    {
                        Value = @"https://www.bing.com/maps?rtp=~adr.One" + rootObject.Location.Replace(" ", "%20") + @"&style=r&lvl=16&trfc=1",
                        Type = "openUrl",
                        Title = "Open on Bing Maps"
                    });

                    cardButtons.Add(new CardAction()
                    {
                        Value = rootObject.Phone,
                        Type = "call",
                        Title = "Call office"
                    });
                }
            }

            if (noOfficesFound)
                result.Text = $"We are sorry, but Contoso Bank does not have a branch in {city} yet.";

            return result;
        }

        public class RootObject
        {
            public string Id { get; set; }
            public string CreatedAt { get; set; }
            public string UpdatedAt { get; set; }
            public string Version { get; set; }
            public bool Deleted { get; set; }
            public string City { get; set; }
            public string Location { get; set; }
            public string Phone { get; set; }
        }
    }
}