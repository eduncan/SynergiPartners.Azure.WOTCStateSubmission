using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using MongoDB.Driver;
using Newtonsoft.Json;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public static class PrepareScreeningsForState
    {
        [FunctionName("PrepareScreeningsForState")]
        public static HttpResponseMessage Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1.0/state/{stateAbbreviation}/submit")]HttpRequestMessage req, string stateAbbreviation, TraceWriter log)
        {
            MongoClient client = new MongoClient(System.Environment.GetEnvironmentVariable("ScreeningCosmosDb", EnvironmentVariableTarget.Process));
            IMongoDatabase database = client.GetDatabase("Screening");
            IMongoCollection<PrepareScreeningsForStatePost> collection = database.GetCollection<PrepareScreeningsForStatePost>("Screening");
            IMongoCollection<StateSubmissionConfiguration> configurationCollection =
                database.GetCollection<StateSubmissionConfiguration>("Screening");

            log.Info("PrepareScreeningsForState function started");

            var content = req.Content.ReadAsStringAsync().Result;

            var prepareScreeningsPost =
                JsonConvert.DeserializeObject<PrepareScreeningsForStatePost>(content);

            if (prepareScreeningsPost != null)
            {
                log.Info("Screenings deserialized");

                collection.InsertOne(prepareScreeningsPost);

                

                var filter = string.Format("{{ State: '{0}',Type: 'StateSubmissionConfiguration'}}", stateAbbreviation);

                var result = configurationCollection.Find(filter).FirstOrDefault();

                if (result == null)
                {
                    var unitOfWork = new GeneratePhysicalStatePacket();
                    unitOfWork.State = stateAbbreviation;
                    unitOfWork.StartDate = prepareScreeningsPost.StartDate;
                    unitOfWork.EndDate = prepareScreeningsPost.EndDate;
                    unitOfWork.Screenings = prepareScreeningsPost.Screenings;
                    unitOfWork.Execute(() => {
                    }, (f) => { });
                }
                else
                {
                    var unitOfWork = new GenerateSystemXBatchUploadFile();
                    unitOfWork.Configuration = result;
                    unitOfWork.StartDate = prepareScreeningsPost.StartDate;
                    unitOfWork.EndDate = prepareScreeningsPost.EndDate;
                    unitOfWork.Screenings = prepareScreeningsPost.Screenings;
                    unitOfWork.Execute(()=>{
                    }, (f) => { });
                }

                return req.CreateResponse(HttpStatusCode.OK, "Hello " + stateAbbreviation);
            }

            return req.CreateResponse(HttpStatusCode.OK, "Hello " + stateAbbreviation);
        }
    }
}
