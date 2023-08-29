using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
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
               public async Task<IActionResult> SentToServiceBus1(JsonObject GitData)
                {
                    try
                    {
                        ServiceBusClient serviceBusClient = new ServiceBusClient("Endpoint=sb://servicebus8.servicebus.windows.net/;SharedAccessKeyName=mypolicy;SharedAccessKey=qKB5Gq6kELfeAAH6A7Oj9RsWbBr4C83q1+ASbFomogM=;EntityPath=mytopic");
                        ServiceBusSender serviceBusSender = serviceBusClient.CreateSender("mytopic");
                        ServiceBusMessageBatch serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();
                        ServiceBusMessage serviceBusMessage = new ServiceBusMessage(JsonConvert.SerializeObject(GitData, Formatting.None,
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

       /* [HttpPost]
        [Route("Post")]
        public async Task<IActionResult> CreatePost([FromBody] WeatherForecast weatherForecast)
        {
            List <WeatherForecast> weatherForecasts = new List<WeatherForecast>();
             weatherForecasts.Add(weatherForecast);
            return Ok(weatherForecasts);

        }*/

    }
}