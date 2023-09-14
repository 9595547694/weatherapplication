using Newtonsoft.Json;
using static weatherapplication.Controllers.WeatherForecastController;

namespace weatherapplication
{
        [JsonConverter(typeof(JsonPathConverter))]
        public class GithubPayload1
        {
            [JsonProperty("repository.owner.name")]
            public string name { get; set; }

            [JsonProperty("before")]
            public string before { get; set; }

            [JsonProperty("after")]
            public string after { get; set; }

            [JsonProperty("Action")]
            public string Action { get; set; }  // action

            [JsonProperty("repository.id")]
            public string id { get; set; }    // pull_request.title

            /* [JsonProperty("interest.details.months")]
             public string Months { get; set; }*/

        }

    }

