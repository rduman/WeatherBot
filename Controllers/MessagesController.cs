using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.Bot.Connector;
using System.Xml;
using System.Text.RegularExpressions;


namespace WeatherBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {

        string Url,City, Temprature, Weather, UpdateTime,Icon;
        XmlDocument doc;
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity message)
        {
            if (message.Type == ActivityTypes.Message)
            {
                message.Text = RemoveSpecialCharacters(message.Text);
                ConnectorClient connector = new ConnectorClient(new Uri(message.ServiceUrl));

                if (!String.IsNullOrEmpty(message.Text))
                {
                    Url = "http://api.openweathermap.org/data/2.5/weather?q=" + message.Text + "&mode=xml&units=metric&APPID=b031434187f8e9ad979e512fa2cc83a9";
                    var xml = new WebClient().DownloadString(new Uri(Url));
                    
                    doc = new XmlDocument();
                    doc.LoadXml(xml);

                    City = doc.DocumentElement.SelectSingleNode("city").Attributes["name"].Value;
                    Temprature = doc.DocumentElement.SelectSingleNode("temperature").Attributes["value"].Value;
                    Weather = doc.DocumentElement.SelectSingleNode("weather").Attributes["value"].Value;
                    UpdateTime = doc.DocumentElement.SelectSingleNode("lastupdate").Attributes["value"].Value;
                    Icon = doc.DocumentElement.SelectSingleNode("weather").Attributes["icon"].Value;
                    Activity reply = message.CreateReply($"Şehir: {message.Text}\n Sıcaklık: {Temprature} °C \n  Hava: {Weather} ");

                    reply.Attachments.Add(new Attachment()
                    {
                        ContentUrl = "http://openweathermap.org/img/w/" + Icon + ".png",
                        ContentType = "image/png",
                        Name = Icon + ".png"
                    });

                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
                else
                {
                    Activity reply = message.CreateReply("Girdiğiniz şehri eşleştiremedik.\n Doğru bir şekilde yazabilir misin?");
                    await connector.Conversations.ReplyToActivityAsync(reply);
                }
            }
            else
            {
                HandleSystemMessage(message);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        public static string RemoveSpecialCharacters(string str)
        {
            return Regex.Replace(str, "[^a-zA-Z]", "", RegexOptions.Compiled);
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}