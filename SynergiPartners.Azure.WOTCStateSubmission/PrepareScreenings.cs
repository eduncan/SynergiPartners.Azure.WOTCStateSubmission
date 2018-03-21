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
            //TODO: handle error responses
            //TODO: integrate with something for JWT bearer tokens
            log.Info("PrepareScreeningsForState function started");

            var content = req.Content.ReadAsStringAsync().Result;

            var prepareScreeningsPost =
                JsonConvert.DeserializeObject<PrepareScreeningsPost>(content);

            if (prepareScreeningsPost != null)
            {
                log.Info("Screenings deserialized");

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

                return req.CreateResponse(HttpStatusCode.OK);
            }

            return req.CreateResponse(HttpStatusCode.OK);
        }
    }
}
