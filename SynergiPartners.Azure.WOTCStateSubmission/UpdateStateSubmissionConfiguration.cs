using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Azure.WebJobs.Host.Executors;
using Newtonsoft.Json;
using SynergiPartners.WOTCStateSubmission.Core;
using MongoDB.Driver;
using MongoDB.Bson.Serialization.Attributes;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public static class UpdateStateSubmissionConfiguration
    {
        [FunctionName("UpdateStateSubmissionConfiguration")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post", Route =
                "v1.0/state/{stateAbbreviation}/submit/configuration")]
            HttpRequestMessage req, string stateAbbreviation,
            TraceWriter log)
        {
            //TODO: handle error responses
            //TODO: integrate with something for JWT bearer tokens
            MongoClient client = new MongoClient(System.Environment.GetEnvironmentVariable("ScreeningCosmosDb", EnvironmentVariableTarget.Process));
            IMongoDatabase database = client.GetDatabase("Screening");
            IMongoCollection<StateSubmissionConfiguration> collection = database.GetCollection<StateSubmissionConfiguration>("Screening");

            log.Info("UpdateStateSubmissionConfiguration function started");

            var content = req.Content.ReadAsStringAsync().Result;

            var updateStateSubmissionConfigurationPost =
                JsonConvert.DeserializeObject<UpdateStateSubmissionConfigurationPost>(content);

            if (updateStateSubmissionConfigurationPost != null)
            {
                log.Info("State submission configuration deserialized");

                var stateSubmissionConfiguration = updateStateSubmissionConfigurationPost.StateSubmissionConfiguration;

                var filter = string.Format("{{ State: '{0}',Type: 'StateSubmissionConfiguration'}}", stateAbbreviation);

                var result = collection.Find(filter).FirstOrDefault();

                if (result==null)
                {
                    collection.InsertOne(stateSubmissionConfiguration);
                }
                else
                {
                    stateSubmissionConfiguration.Id = result.Id;
                    var replaceOneResult = collection.ReplaceOne(filter, stateSubmissionConfiguration);
                }

                return req.CreateResponse(HttpStatusCode.OK);
            }

            else
                //stateSubmissionConfiguration = null;
                return req.CreateErrorResponse(HttpStatusCode.BadRequest,
                    "Could not deserialize the state submission configuration");
        }
    }
}
