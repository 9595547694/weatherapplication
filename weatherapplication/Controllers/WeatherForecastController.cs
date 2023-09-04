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
        public IActionResult ReceivePayload([FromBody] WebhookPayload payload)
        {
            var filteredData = new
            {
                Field1 = payload.id,
                Field2 = payload.node_id,
               
            };
            SentToServiceBus(filteredData);

            return Ok("Filtered payload sent to Azure Service Bus.");


            // Validate the secret token provided in the request headers
           /* string secretToken = "YourWebhookSecret"; // Replace with your actual secret
            string receivedSignature = Request.Headers["X-Hub-Signature"];

            if (!VerifyWebhookSignature(Request.Body, secretToken, receivedSignature))
            {
                return Unauthorized("Invalid signature.");
            }

            // Handle the payload data
            return Ok("Payload received successfully.");
            */
        }


       /* private bool VerifyWebhookSignature(Stream requestBody, string secretToken, string receivedSignature)
        {
            // Implement verification logic here
            // Compare receivedSignature with computed signature based on secretToken and requestBody
            return true; // Return true if the signature is valid
        }*/



        [HttpPost]
        [Route("SentToServiceBus")]
        public async Task<IActionResult> SentToServiceBus(object filteredData)
        {
            using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                string payload = await reader.ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(payload);
                string commitId = jsonData.after;
                try
                {
                    ServiceBusClient serviceBusClient = new ServiceBusClient("Endpoint=sb://servicebus8.servicebus.windows.net/;SharedAccessKeyName=mypolicy;SharedAccessKey=qKB5Gq6kELfeAAH6A7Oj9RsWbBr4C83q1+ASbFomogM=;EntityPath=mytopic");
                    ServiceBusSender serviceBusSender = serviceBusClient.CreateSender("mytopic");
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
        public async Task<IActionResult> CreatePost([FromBody] WeatherForecast weatherForecast)
        {
            List <WeatherForecast> weatherForecasts = new List<WeatherForecast>();
             weatherForecasts.Add(weatherForecast);
            return Ok(weatherForecasts);

        }

    }
}