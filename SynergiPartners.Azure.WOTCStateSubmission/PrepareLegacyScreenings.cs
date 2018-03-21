using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Newtonsoft.Json;
using SynergiPartners.WOTCScreening.Core;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public static class PrepareLegacyScreenings
    {
        [FunctionName("PrepareLegacyScreenings")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "v1.0/state/submitlegacy")]HttpRequestMessage req, TraceWriter log)
        {
            var content = req.Content.ReadAsStringAsync().Result;
            var prepareScreeningsPost =
                JsonConvert.DeserializeObject<PrepareScreeningsPost>(content);

            prepareScreeningsPost.Screenings=new List<Screening>();

            SqlConnection connection = new SqlConnection(System.Environment.GetEnvironmentVariable("LegacyDatabaseConnectionString", EnvironmentVariableTarget.Process));
            connection.Open();

            List<string> states = new List<string>();

            SqlCommand stateCommand = new SqlCommand(string.Format(@"select StateAbbreviation from [States]"),connection);
            SqlDataReader stateReader = stateCommand.ExecuteReader();
            while (stateReader.Read())
            {
                states.Add(stateReader.GetString(0));
            }

            stateReader.Close();

            foreach (var state in states)
            {
                string json = string.Empty;

                SqlCommand command = new SqlCommand(string.Format("exec GetStatePackageJSON '{0}','{1}','{2}'",
                    prepareScreeningsPost.StartDate.ToShortDateString(),
                    prepareScreeningsPost.EndDate.ToShortDateString(), state), connection);

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    if (reader.IsDBNull(0))
                        break;
                    json = reader.GetString(0);
                    break;
                }
                reader.Close();

                if (!string.IsNullOrEmpty(json))
                {
                    prepareScreeningsPost.Screenings.AddRange(JsonConvert.DeserializeObject<List<Screening>>(json));
                }
            }

            using (var httpClient = new HttpClient())
            {
                string json = JsonConvert.SerializeObject(prepareScreeningsPost);
                HttpResponseMessage response = await httpClient.PostAsync(
                    System.Environment.GetEnvironmentVariable("PrepareScreeningsEndpoint",
                        EnvironmentVariableTarget.Process), new StringContent(json,null,"application/json"));
            }

            return req.CreateResponse(HttpStatusCode.OK, "Hello");
        }
    }
}
