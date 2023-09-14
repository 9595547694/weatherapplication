using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Octokit;
using System;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;

namespace weatherapplication.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _log;

        public WeatherForecastController(ILogger<WeatherForecastController> logger)
        {
            _log = logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

       
       public  class JsonPathConverter : JsonConverter
        {
            public override object ReadJson(JsonReader reader, Type objectType,
                                            object existingValue, JsonSerializer serializer)
            {
                JObject jo = JObject.Load(reader);
                object targetObj = Activator.CreateInstance(objectType);

                foreach (PropertyInfo prop in objectType.GetProperties()
                                                        .Where(p => p.CanRead && p.CanWrite))
                {
                    JsonPropertyAttribute att = prop.GetCustomAttributes(true)
                                                    .OfType<JsonPropertyAttribute>()
                                                    .FirstOrDefault();

                    string jsonPath = (att != null ? att.PropertyName : prop.Name);
                    JToken token = jo.SelectToken(jsonPath);

                    if (token != null && token.Type != JTokenType.Null)
                    {
                        object value = token.ToObject(prop.PropertyType, serializer);
                        prop.SetValue(targetObj, value, null);
                    }
                }

                return targetObj;
            }

            public override bool CanConvert(Type objectType)
            {
                // CanConvert is not called when [JsonConverter] attribute is used
                return false;
            }

            public override bool CanWrite
            {
                get { return false; }
            }

            public override void WriteJson(JsonWriter writer, object value,
                                           JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
        [HttpPost]
        [Route("SentToServiceBus")]
        public async Task<IActionResult> SentToServiceBus()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {

                /*string json = @"{
                ""action"": ""deleted"",
                 ""ref"": ""refs/heads/master"",
                 ""before"": ""4b2655489b7e90a4208d3615b9959a17a8f0b92f"",
                    ""after"": ""5a7768ffdfc2672572ef4d4f67b60070c098e7bd"",
                ""pull_request"": {
                ""title"": ""Fix button1""
                   }
                }";
                dynamic obj = JsonConvert.DeserializeObject(json);
                GithubPayload entity = new GithubPayload();
                entity.Action = obj.action;
                entity.Name = obj.pull_request.title;*/

                string payload = await reader.ReadToEndAsync();
                GithubPayload1 p = JsonConvert.DeserializeObject<GithubPayload1>(payload);
                dynamic jsonData = JsonConvert.DeserializeObject(payload);
               
                string commitId = jsonData.after;

              //  var payloadJson = Encoding.UTF8.GetString(payload);
                GithubPayload1 gitHubPayload = System.Text.Json.JsonSerializer.Deserialize<GithubPayload1>(payload);

                try
                {
                    ServiceBusClient serviceBusClient = new ServiceBusClient("Endpoint=sb://servicebuspayload.servicebus.windows.net/;SharedAccessKeyName=payload;SharedAccessKey=cZhgSZ1zOPeUqAfpH6n+Kqj+GB3I0JcTQ+ASbPTytQE=");
                    ServiceBusSender serviceBusSender = serviceBusClient.CreateSender("payloadtopic");
                    ServiceBusMessageBatch serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(p, Formatting.None,
                    new JsonSerializerSettings()
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    }));
                    serviceBusMessage.ContentType = "application/json";
                    serviceBusMessageBatch.TryAddMessage(serviceBusMessage);
                    await serviceBusSender.SendMessagesAsync(serviceBusMessageBatch);
                    await serviceBusSender.DisposeAsync();
                    await serviceBusClient.DisposeAsync();
                    return Ok("Data Send To Topic");
                }
                catch (Exception ex)
                {
                 return Ok(ex.ToString());
                }
            }
        }

        [HttpPost]
        [Route("Post")]
        public async Task<IActionResult> CreatePost1([FromBody] WeatherForecast weatherForecast)
        {
            List <WeatherForecast> weatherForecasts = new List<WeatherForecast>();
             weatherForecasts.Add(weatherForecast);
            return Ok(weatherForecasts);

        }

    }
}