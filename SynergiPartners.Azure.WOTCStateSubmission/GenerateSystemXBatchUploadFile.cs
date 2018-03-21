using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ChakraCore.NET;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MongoDB.Bson;
using MongoDB.Driver;
using Newtonsoft.Json.Linq;
using SynergiPartners.WOTCScreening.Core;
using SynergiPartners.WOTCScreening.Core.ComponentModel;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public class GenerateSystemXBatchUploadFile : UnitOfWork<GenerateSystemXBatchUploadFile>
    {
        public StateSubmissionConfiguration Configuration { get; set; }
        public List<Screening> Screenings { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string OutputText { get; set; }
        public override bool CanExecute()
        {
            return base.CanExecute();
        }

        public override void PreExecute(Action onSuccess, Action<Exception> onFailure)
        {
            base.PreExecute(onSuccess, onFailure);
        }

        public override void Execute(Action onSuccess, Action<Exception> onFailure)
        {
            try
            {
                string filename = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}.txt",
                    Configuration.State,
                    StartDate.Month,
                    StartDate.Day,
                    StartDate.Year,
                    EndDate.Month,
                    EndDate.Day,
                    EndDate.Year,
                    DateTime.Now.Ticks);

                MongoClient client = new MongoClient(System.Environment.GetEnvironmentVariable("ScreeningCosmosDb", EnvironmentVariableTarget.Process));
                IMongoDatabase database = client.GetDatabase("Screening");
                IMongoCollection<BsonDocument> collection = database.GetCollection<BsonDocument>("Screening");

                OutputText = string.Empty;
                ChakraRuntime runtime = ChakraRuntime.Create();

                JObject headerObject = new JObject();
                headerObject.Add("StartMonth", StartDate.Month.ToString());
                headerObject.Add("StartDay", StartDate.Day.ToString());
                headerObject.Add("StartYear", StartDate.Year.ToString());
                headerObject.Add("EndMonth", EndDate.Month.ToString());
                headerObject.Add("EndDay", EndDate.Day.ToString());
                headerObject.Add("EndYear", EndDate.Year.ToString());
                headerObject.Add("Ticks", DateTime.Now.Ticks.ToString());
                headerObject.Add("RecordCount", Screenings.Count);

                List<string> headerValues = new List<string>();
                if (Configuration.HeaderConfiguration != null)
                {
                    foreach (StateSubmissionField field in Configuration.HeaderConfiguration)
                    {
                        headerValues.Add(ParseValues(field, headerObject, runtime));
                    }

                    if (Configuration.HeaderConfiguration.Count > 0)
                    {
                        OutputText += ((Configuration.EncloseFieldsInQuotes ? "\"" : string.Empty) + string.Join(Configuration.DefaultDelimiter, headerValues) + (Configuration.EncloseFieldsInQuotes ? "\"" : string.Empty)) + "\n";
                    }
                }

                if (Screenings != null)
                {
                    List<Screening> screeningsToPrint = new List<Screening>();
                    foreach (var screening in Screenings)
                    {
                        var jsonObject = JObject.FromObject(screening);

                        if (!string.IsNullOrEmpty(Configuration.OnExecuteJavascript))
                        {
                            var jsonObjectString = jsonObject.ToString();
                            ChakraContext context = runtime.CreateContext(true);
                            context.GlobalObject.WriteProperty<string>("screeningString", jsonObjectString);
                            context.RunScript(Configuration.OnExecuteJavascript);
                            context.RunScript("var screening=JSON.parse(screeningString);");
                            jsonObjectString = context.GlobalObject.CallFunction<string>("onExecute");
                            jsonObject = JObject.Parse(jsonObjectString);
                        }

                        JToken forcePrint = null;
                        if (jsonObject.TryGetValue("ForcePrint", out forcePrint))
                        {
                            if (forcePrint.Value<bool>() == true)
                            {
                                screeningsToPrint.Add(screening);
                                continue;
                            }
                        }

                        jsonObject.Add("Type", "StateSubmissionRecord");
                        jsonObject.Add("State", Configuration.State);
                        jsonObject.Add("DatePrepared", DateTime.Now.ToString());
                        jsonObject.Add("FileProduced", string.Empty);
                        jsonObject.Add("DateSent", string.Empty);
                        jsonObject.Add("DateValidated", string.Empty);
                        jsonObject.Add("OutputFile", filename);

                        collection.InsertOne(
                            MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonObject.ToString()));

                        List<string> screeningValues = new List<string>();

                        if (Configuration.RecordConfiguration != null)
                        {
                            foreach (StateSubmissionField field in Configuration.RecordConfiguration)
                            {
                                screeningValues.Add(ParseValues(field, jsonObject, runtime));
                            }
                        }

                        OutputText += ((Configuration.EncloseFieldsInQuotes ? "\"" : string.Empty) +
                                       string.Join(Configuration.DefaultDelimiter, screeningValues) +
                                       (Configuration.EncloseFieldsInQuotes ? "\"" : string.Empty)) + "\n";
                    }

                    if (screeningsToPrint.Count > 0)
                    {
                        var unitOfWork = new GeneratePhysicalStatePacket();
                        unitOfWork.State = Configuration.State;
                        unitOfWork.StartDate = StartDate;
                        unitOfWork.EndDate = EndDate;
                        unitOfWork.Screenings = screeningsToPrint;
                        unitOfWork.Execute(() => {
                        }, (f) => { });
                    }
                }

                //Microsoft.Azure.KeyVault.KeyVaultKeyResolver cloudResolver = new Microsoft.Azure.KeyVault.KeyVaultKeyResolver(GetToken);
                //var rsa = cloudResolver.ResolveKeyAsync(@"https://neonprod01vault.vault.azure.net/keys/FileStorage",
                //    CancellationToken.None).Result;
                //BlobEncryptionPolicy policy = new BlobEncryptionPolicy(rsa, cloudResolver);
                //BlobRequestOptions options = new BlobRequestOptions() { EncryptionPolicy = policy };

                StorageCredentials storageCredentials = new StorageCredentials(System.Environment.GetEnvironmentVariable("StorageAccountName", EnvironmentVariableTarget.Process), System.Environment.GetEnvironmentVariable("StorageKeyVault", EnvironmentVariableTarget.Process));
                CloudStorageAccount account = new CloudStorageAccount(storageCredentials, useHttps: true);
                CloudBlobClient storageClient = account.CreateCloudBlobClient();
                CloudBlobContainer storageContainer = storageClient.GetContainerReference("statepackages");
                storageContainer.CreateIfNotExists(BlobContainerPublicAccessType.Off);

                CloudBlockBlob blob = storageContainer.GetBlockBlobReference(filename);

                using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(OutputText ?? "")))
                {
                    if (stream.Length == 0)
                        return;
                    blob.UploadFromStream(stream, stream.Length, null, null, null);
                }

                string filter = string.Format("{{ Type: 'StateSubmissionRecord', OutputFile : '{0}'}}", filename);
                var update = Builders<BsonDocument>.Update.Set("FileProduced", DateTime.Now.ToString());

                collection.UpdateMany(filter, update);
            }
            catch (Exception e)
            {
                onFailure(e);
            }

            onSuccess();
        }

        public override void PostExecute(Action onSuccess, Action<Exception> onFailure)
        {
            base.PostExecute(onSuccess, onFailure);
        }

        string ParseValues(StateSubmissionField field, JObject source, ChakraRuntime runtime)
        {
            string output = string.Empty;
            foreach (string value in field.Values)
            {
                if (value.StartsWith("literal:"))
                    output += value.Replace("literal:", string.Empty);
                else
                {
                    try
                    {
                        var token = source.SelectToken(value);
                        var tokenString = (string)token;
                        tokenString = tokenString == null ? string.Empty : tokenString;


                        if (!string.IsNullOrEmpty(field.OnGetValueJavascript) && !string.IsNullOrEmpty(tokenString))
                        {
                            ChakraContext context = runtime.CreateContext(true);
                            context.GlobalObject.WriteProperty<string>("token", tokenString);
                            context.RunScript(field.OnGetValueJavascript);
                            tokenString = context.GlobalObject.CallFunction<string>("onGetValue");
                        }

                        output += tokenString;
                    }
                    catch (Exception e1)
                    {
                        int x = 1;
                    }
                }
            }

            return Format(field.MaxLength, field.Format, output);
        }

        string Format(int maxLength, string pattern, params object[] inputs)
        {
            if (string.IsNullOrEmpty(pattern))
                pattern = "{0}";
            if (maxLength == 0)
                maxLength = Int32.MaxValue;
            string returnValue = string.Format(pattern, inputs);
            if (returnValue.Length > maxLength)
                returnValue = returnValue.Substring(0, maxLength);

            return returnValue;
        }

        public static async Task<string> GetToken(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            ClientCredential clientCred = new ClientCredential(
                @"2ca7960a-22fa-442e-97ac-974dd6d673ef",
                @"YuNrcIgWsivBNQPYGfQbnk1nI8PbTCn4gERxq9VwfVc=");
            AuthenticationResult result = await authContext.AcquireTokenAsync(resource, clientCred);

            if (result == null)
                throw new InvalidOperationException("Failed to obtain the JWT token");

            return result.AccessToken;
        }
    }
}
