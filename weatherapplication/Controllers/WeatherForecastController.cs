using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Nodes;

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


        [HttpPost]
        [Route("SentToServiceBus")]
        public async Task<IActionResult> SentToServiceBus()
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {

                string json = @"{
                ""action"": ""deleted"",
                 ""ref"": ""refs/heads/master"",
                 ""before"": ""4b2655489b7e90a4208d3615b9959a17a8f0b92f"",
                    ""after"": ""5a7768ffdfc2672572ef4d4f67b60070c098e7bd"",
                ""pull_request"": {
                ""title"": ""Fix button""
                   }
                }";
                dynamic obj = JsonConvert.DeserializeObject(json);
                GithubPayload entity = new GithubPayload();
                entity.Action = obj.action;
                entity.Name = obj.pull_request.title;



                string payload = await reader.ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(payload);
                string commitId = jsonData.after;
              
                try
                {
                    ServiceBusClient serviceBusClient = new ServiceBusClient("Endpoint=sb://servicebuspayload.servicebus.windows.net/;SharedAccessKeyName=payload;SharedAccessKey=cZhgSZ1zOPeUqAfpH6n+Kqj+GB3I0JcTQ+ASbPTytQE=");
                    ServiceBusSender serviceBusSender = serviceBusClient.CreateSender("payloadtopic");
                    ServiceBusMessageBatch serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();
                    ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(jsonData, Formatting.None,
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