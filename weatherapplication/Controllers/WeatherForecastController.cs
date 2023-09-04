using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using weatherapplication;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Azure;
using Octokit;

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
                Field1 = payload,
               // Field2 = payload.node_id,
               
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

                // Assuming you have the JSON payload as a string named 'jsonPayload'
              

                // Deserialize the JSON payload into the GitHubWebhookPayload class
               // GitHubWebhookPayload webhookPayload = JsonConvert.DeserializeObject<GitHubWebhookPayload>(jsonPayload);

                // Access the properties of the deserialized object
                //string branchRef = webhookPayload.@ref;
                //string beforeCommit = webhookPayload.before;
                //string afterCommit = webhookPayload.after;

                //// Access other properties as needed
                //GitHubRepository repository = webhookPayload.repository;
                //GitHubUser pusher = webhookPayload.pusher;
                //GitHubUser sender = webhookPayload.sender;

                //// Access commits if present
                //List<GitHubCommit> commits = webhookPayload.commits;
                //GitHubCommit headCommit = webhookPayload.head_commit;

                // Now you can work with the deserialized data

                string payload = await reader.ReadToEndAsync();
                dynamic jsonData = JsonConvert.DeserializeObject(payload);
                string jsonPayload = jsonData;

              /*  GitHubWebhookPayload webhookPayload = JsonConvert.DeserializeObject<GitHubWebhookPayload>(jsonPayload);
                string branchRef = jsonData.@ref;
                string beforeCommit = jsonData.before;
                string afterCommit = jsonData.after;

                GitHubRepository repository = jsonData.repository;
                GitHubUser pusher = jsonData.pusher;
                GitHubUser sender = jsonData.sender;*/

                List<GitHubCommit> commits = jsonData.commits;
                GitHubCommit headCommit = jsonData.head_commit;
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