using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Dynamic;
using System.Text;
using System.Text.Json.Nodes;
using static weatherapplication.Commit;

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
        private readonly string jsonString;

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
                string payload = await reader.ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(payload);
                string commitId = jsonData.after;
              
                try
                {
                    ServiceBusClient serviceBusClient = new ServiceBusClient("Endpoint=sb://servicebuspayload.servicebus.windows.net/;SharedAccessKeyName=payload;SharedAccessKey=+eJk5G3T28htUMgVzUWRzwdDjzIR/DpNS+ASbEq4Emc=");
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