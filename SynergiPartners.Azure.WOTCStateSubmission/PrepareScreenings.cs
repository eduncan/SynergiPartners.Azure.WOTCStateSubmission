using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using MongoDB.Driver;
using Newtonsoft.Json;
using SynergiPartners.WOTCScreening.Core;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public static class PrepareScreenings
    {
        [FunctionName("PrepareScreenings")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1.0/state/submit")]HttpRequestMessage req, TraceWriter log)
        {
            //MongoClient client = new MongoClient(System.Environment.GetEnvironmentVariable("ScreeningCosmosDb", EnvironmentVariableTarget.Process));
            //IMongoDatabase database = client.GetDatabase("Screening");
            //IMongoCollection<PrepareScreeningsPost> collection = database.GetCollection<PrepareScreeningsPost>("Screening");
            //IMongoCollection<StateSubmissionConfiguration> configurationCollection =
            //    database.GetCollection<StateSubmissionConfiguration>("Screening");

            log.Info("PrepareScreeningsForState function started");

            var content = req.Content.ReadAsStringAsync().Result;

            var prepareScreeningsPost =
                JsonConvert.DeserializeObject<PrepareScreeningsPost>(content);

            if (prepareScreeningsPost != null)
            {
                log.Info("Screenings deserialized");

                //collection.InsertOne(prepareScreeningsPost);

                var screenings = new Dictionary<string, PrepareScreeningsForStatePost>();

                foreach (var screening in prepareScreeningsPost.Screenings)
                {
                    PrepareScreeningsForStatePost prepareScreeningsForState;
                    if (!screenings.TryGetValue(screening.Employer.State, out prepareScreeningsForState))
                    {
                        prepareScreeningsForState = new PrepareScreeningsForStatePost();
                        prepareScreeningsForState.StartDate = prepareScreeningsPost.StartDate;
                        prepareScreeningsForState.EndDate = prepareScreeningsPost.EndDate;
                        prepareScreeningsForState.Screenings = new List<Screening>();
                        prepareScreeningsForState.State = screening.Employer.State;

                        screenings.Add(screening.Employer.State, prepareScreeningsForState);
                    }

                    prepareScreeningsForState.Screenings.Add(screening);
                }

                foreach (var prepareScreeningsForStatePost in screenings.Values)
                {
                    using (var httpClient = new HttpClient())
                    {
                        string json = JsonConvert.SerializeObject(prepareScreeningsForStatePost);
                        HttpResponseMessage response = await httpClient.PostAsync(
                            System.Environment.GetEnvironmentVariable("PrepareScreeningsForStateEndpoint",
                                EnvironmentVariableTarget.Process).Replace("{stateAbbreviation}",prepareScreeningsForStatePost.State), new StringContent(json, null, "application/json"));
                    }
                }

                return req.CreateResponse(HttpStatusCode.OK, "Hello");
            }

            return req.CreateResponse(HttpStatusCode.OK, "Hello");
        }
    }
}
