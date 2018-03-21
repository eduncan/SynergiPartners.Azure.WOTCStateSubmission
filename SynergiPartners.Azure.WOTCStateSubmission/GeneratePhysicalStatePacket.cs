using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Aspose.Words.Tables;
using ChakraCore.NET;
using Microsoft.Azure.Documents.SystemFunctions;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SynergiPartners.WOTCScreening.Core;
using SynergiPartners.WOTCScreening.Core.ComponentModel;
using SynergiPartners.WOTCStateSubmission.Core;

namespace SynergiPartners.Azure.WOTCStateSubmission
{
    public class GeneratePhysicalStatePacket : UnitOfWork<GeneratePhysicalStatePacket>
    {
        public string State { get; set; }
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
                StorageCredentials storageCredentials = new StorageCredentials(System.Environment.GetEnvironmentVariable("StorageAccountName", EnvironmentVariableTarget.Process), System.Environment.GetEnvironmentVariable("StorageKeyVault", EnvironmentVariableTarget.Process));
                CloudStorageAccount account = new CloudStorageAccount(storageCredentials, useHttps: true);
                CloudBlobClient storageClient = account.CreateCloudBlobClient();
                CloudBlobContainer storageSupportContainer = storageClient.GetContainerReference("support");

                CloudBlockBlob blob = storageSupportContainer.GetBlockBlobReference("Aspose.Total.lic");
                blob.FetchAttributes();
                long fileByteLength = blob.Properties.Length;
                Byte[] b = new Byte[fileByteLength];
                blob.DownloadToByteArray(b, 0);

                SharedFilesConfiguration.Current.AsposeLicense = b;

                blob = storageSupportContainer.GetBlockBlobReference("ETA_Form_9061_English_FINAL_11_(expires_January_31,2020).pdf");
                blob.FetchAttributes();
                fileByteLength = blob.Properties.Length;
                b = new Byte[fileByteLength];
                blob.DownloadToByteArray(b, 0);

                SharedFilesConfiguration.Current.Form9061 = b;

                blob = storageSupportContainer.GetBlockBlobReference("8850-page2.docx");
                blob.FetchAttributes();
                fileByteLength = blob.Properties.Length;
                b = new Byte[fileByteLength];
                blob.DownloadToByteArray(b, 0);

                SharedFilesConfiguration.Current.Form8850Page2 = b;

                blob = storageSupportContainer.GetBlockBlobReference("WOTCCover.docx");
                blob.FetchAttributes();
                fileByteLength = blob.Properties.Length;
                b = new Byte[fileByteLength];
                blob.DownloadToByteArray(b, 0);

                SharedFilesConfiguration.Current.CoverLetter = b;

                Aspose.Words.License wordsLicense = new Aspose.Words.License();
                wordsLicense.SetLicense(new MemoryStream(SharedFilesConfiguration.Current.AsposeLicense));

                Aspose.Pdf.License pdfLicense = new Aspose.Pdf.License();
                pdfLicense.SetLicense(new MemoryStream(SharedFilesConfiguration.Current.AsposeLicense));

                string filename = string.Format("{0}_{1}_{2}_{3}_{4}_{5}_{6}_{7}.pdf",
                    State,
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

                if (Screenings != null)
                {
                    Aspose.Pdf.Document pdfDocument = new Aspose.Pdf.Document();

                    foreach (var screening in Screenings.OrderBy(s=>s.Applicant.LastName+"_"+s.Applicant.FirstName))
                    {
                        Aspose.Pdf.Document form8850Pdf = new Aspose.Pdf.Document(RenderIRSForm8850Page2(screening));
                        pdfDocument.Pages.Add(form8850Pdf.Pages);

                        Aspose.Pdf.Document form9061Pdf = new Aspose.Pdf.Document(RenderIRSForm9061(screening));
                        pdfDocument.Pages.Add(form9061Pdf.Pages);

                        var jsonObject = JObject.FromObject(screening);

                        jsonObject.Add("Type", "StateSubmissionRecord");
                        jsonObject.Add("State", State);
                        jsonObject.Add("DatePrepared", DateTime.Now.ToString());
                        jsonObject.Add("FileProduced", string.Empty);
                        jsonObject.Add("DateSent", string.Empty);
                        jsonObject.Add("DateValidated", string.Empty);
                        jsonObject.Add("OutputFile", filename);

                        collection.InsertOne(
                            MongoDB.Bson.Serialization.BsonSerializer.Deserialize<BsonDocument>(jsonObject.ToString()));

                    }

                    Aspose.Pdf.Document wotcCoverPdf = new Aspose.Pdf.Document(RenderCoverLetter("Address Goes Here","Salutation Goes Here", Screenings));
                    pdfDocument.Pages.Add(wotcCoverPdf.Pages);

                    MemoryStream stream = new MemoryStream();
                    pdfDocument.Save(stream);

                    CloudBlobContainer storagePackageContainer = storageClient.GetContainerReference("statepackages");
                    storagePackageContainer.CreateIfNotExists(BlobContainerPublicAccessType.Off);

                    blob = storagePackageContainer.GetBlockBlobReference(filename);

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

        MemoryStream RenderIRSForm8850Page2(Screening screening)
        {
            Aspose.Words.Document wordDocument = new Aspose.Words.Document(new MemoryStream(SharedFilesConfiguration.Current.Form8850Page2));
            Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(wordDocument);

            builder.MoveToBookmark("CompanyName");
            builder.Writeln(screening.Employer.Name.Trim());

            var employeeName = string.Format("{0}, {1} {2}", screening.Applicant.LastName.Trim(), screening.Applicant.FirstName.Trim(), string.IsNullOrEmpty(screening.Applicant.MiddleName) ? string.Empty : screening.Applicant.MiddleName.Trim());

            builder.MoveToBookmark("EmployeeName");
            builder.Writeln(employeeName.Trim());

            var ein = screening.Employer.EIN.Trim().Length == 9
                ? screening.Employer.EIN.Trim().Substring(0, 2) + "-" + screening.Employer.EIN.Trim().Substring(2, 7)
                : screening.Employer.EIN.Trim();

            builder.MoveToBookmark("EIN");
            builder.Writeln(ein);

            var companyPhone = string.IsNullOrEmpty(screening.Employer.TelephoneNumber)
                ? string.Empty
                : (screening.Employer.TelephoneNumber.Trim().Length == 7
                    ? screening.Employer.TelephoneNumber.Trim().Substring(0, 3) + "-" +
                      screening.Employer.TelephoneNumber.Trim().Substring(3, 4)
                    : (screening.Employer.TelephoneNumber.Trim().Length == 10
                        ? screening.Employer.TelephoneNumber.Trim().Substring(0, 3) + '-' +
                          screening.Employer.TelephoneNumber.Trim().Substring(3, 3) + '-' +
                          screening.Employer.TelephoneNumber.Trim().Substring(6, 4)
                        : screening.Employer.TelephoneNumber.Trim()));

            builder.MoveToBookmark("Phone");
            builder.Writeln(companyPhone);

            builder.MoveToBookmark("CompanyAddress");
            builder.Writeln(screening.Employer.StreetAddress.Trim());

            builder.MoveToBookmark("CompanyCityStateZip");
            builder.Writeln(string.Format("{0}, {1} {2}",
                screening.Employer.City.Trim(),
                screening.Employer.State.Trim(),
                screening.Employer.ZipCode.Trim()));

            builder.MoveToBookmark("InfoDate");
            builder.Writeln(screening.GaveInformation.HasValue
                ? screening.GaveInformation.Value.ToString("MM/dd/yyyy")
                : string.Empty);

            builder.MoveToBookmark("OfferDate");
            builder.Writeln(screening.OfferedJob.HasValue?screening.OfferedJob.Value.ToString("MM/dd/yyyy"): string.Empty);

            builder.MoveToBookmark("HireDate");
            builder.Writeln(screening.WasHired.HasValue?screening.WasHired.Value.ToString("MM/dd/yyyy") : string.Empty);

            builder.MoveToBookmark("StartDate");
            builder.Writeln(screening.StartedJob.HasValue?screening.StartedJob.Value.ToString("MM/dd/yyyy") : string.Empty);

            builder.MoveToBookmark("Date");
            builder.Writeln(screening.ProcessedDate.HasValue?screening.ProcessedDate.Value.ToString("MM/dd/yyyy") : string.Empty);

            MemoryStream stream = new MemoryStream();
            wordDocument.Save(stream, Aspose.Words.SaveFormat.Pdf);
            stream.Position = 0;
            return stream;
        }

        MemoryStream RenderIRSForm9061(Screening screening)
        {
            Aspose.Pdf.Facades.Form form = new Aspose.Pdf.Facades.Form(
                 new MemoryStream(SharedFilesConfiguration.Current.Form9061)
                );

            #region CompanyName
            form.FillField("EmployerName", screening.Employer.Name.Trim());
            #endregion

            #region EmployerAddress

            string employerAddressPhrase = screening.Employer.StreetAddress;
            employerAddressPhrase += Environment.NewLine;
            employerAddressPhrase += string.Format("{0}, {1} {2}",
                screening.Employer.City,
                screening.Employer.State,
                screening.Employer.ZipCode);
      
            var companyPhone= string.IsNullOrEmpty(screening.Employer.TelephoneNumber) ? string.Empty :
                (screening.Employer.TelephoneNumber.Trim().Length == 7 ? screening.Employer.TelephoneNumber.Trim().Substring(0, 3) + "-" + screening.Employer.TelephoneNumber.Trim().Substring(3, 4) :
                    (screening.Employer.TelephoneNumber.Trim().Length == 10 ? screening.Employer.TelephoneNumber.Trim().Substring(0, 3) + '-' + screening.Employer.TelephoneNumber.Trim().Substring(3, 3) + '-' + screening.Employer.TelephoneNumber.Trim().Substring(6, 4) :
                        screening.Employer.TelephoneNumber.Trim()));
            employerAddressPhrase += companyPhone;

            form.FillField("EmployerAddressAndTelephone", employerAddressPhrase);
            #endregion

            #region EmployerEIN
            var ein = screening.Employer.EIN.Trim().Length == 9 ? screening.Employer.EIN.Trim().Substring(0, 2) + "-" + screening.Employer.EIN.Trim().Substring(2, 7) : screening.Employer.EIN.Trim();
            form.FillField("EmployerFEIN", ein);
            #endregion

            #region ApplicantName
            var employeeName = string.Format("{0}, {1} {2}", screening.Applicant.LastName.Trim(), screening.Applicant.FirstName.Trim(), string.IsNullOrEmpty(screening.Applicant.MiddleName) ? string.Empty : screening.Applicant.MiddleName.Trim());

            form.FillField("ApplicantName", employeeName.Trim());
            #endregion

            #region Social
            var social = string.IsNullOrEmpty(screening.Applicant.SocialSecurityNumber) ? "000-00-0000" :
                (screening.Applicant.SocialSecurityNumber.Length == 7 ? "0" + screening.Applicant.SocialSecurityNumber.Substring(0, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(2, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(4, 4) :
                    screening.Applicant.SocialSecurityNumber.Substring(0, 3) + "-" + screening.Applicant.SocialSecurityNumber.Substring(3, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(5, 4));

            form.FillField("SocialSecurityNumber", social);
            #endregion

            #region IsRehire
            form.FillField("WorkedForEmployerNo", "X");
            #endregion

            #region StartDate
            if (screening.StartedJob.HasValue && screening.StartedJob.Value.Year > 1900)
            {
                form.FillField("EmploymentStartDate", screening.StartedJob.Value.ToShortDateString());
            }
            #endregion

            #region StartingWage
            if (screening.StartingWage > 0)
            {
                form.FillField("StartingWage", screening.StartingWage.ToString());
            }
            #endregion

            #region Position
            form.FillField("Position", screening.Position.ToString().ToUpper().Trim());
            #endregion

            #region Under40
            if (screening.IRSForm9061.Q12)
            {
                form.FillField("Under40Yes", "X");
                form.FillField("Under40DateOfBirth", screening.Applicant.DateOfBirth.Value.ToShortDateString());
            }
            else
            {
                form.FillField("Under40No", "X");
            }
            #endregion

            #region Veteran
            if (screening.IRSForm9061.Q13a)
            {
                form.FillField("VeteranYes", "X");
            }
            else
            {
                form.FillField("VeteranNo", "X");
            }
            #endregion

            #region Military

            if (screening.IRSForm9061.Q13b)
            {
                form.FillField("MilitaryFoodStampsYes", "X");
                form.FillField("MilitaryFoodStampsRecipientName", screening.IRSForm9061.Q13bFirstName + " " + screening.IRSForm9061.Q13bLastName);
                form.FillField("MilitaryFoodStampsCityAndState", screening.IRSForm9061.Q13bCity + ", " + screening.IRSForm9061.Q13bState);
            }
            else
            {
                form.FillField("MilitaryFoodStampsNo", "X");
            }

            

            if (screening.IRSForm9061.Q13c)
            {
                form.FillField("DisabledVeteranYes", "X");
            }
            else
            {
                form.FillField("DisabledVeteranNo", "X");
            }

            if (screening.IRSForm9061.Q13d)
            {
                form.FillField("DisabledVeteranDischargedInPastYearYes", "X");
            }
            else
            {
                form.FillField("DisabledVeteranDischargedInPastYearNo", "X");
            }

            if (screening.IRSForm9061.Q13e)
            {
                form.FillField("DisabledVeteranUnemployedForSixMonthsYes", "X");
            }
            else
            {
                form.FillField("DisabledVeteranUnemployedForSixMonthsNo", "X");
            }
            #endregion

            #region FoodStamps
            if (screening.IRSForm9061.Q14a)
            {
                form.FillField("FoodStamps6MonthsYes", "X");
            }
            else
            {
                form.FillField("FoodStamps6MonthsNo", "X");
            }
            #endregion

            #region FoodStamps3Months
            if (screening.IRSForm9061.Q14b)
            {
                form.FillField("FoodStamps3MonthsYes", "X");
            }
            else
            {
                form.FillField("FoodStamps3MonthsNo", "X");
            }
            #endregion

            #region FoodStampsRecipient
            if (screening.IRSForm9061.Q14a||screening.IRSForm9061.Q14b)
            {
                form.FillField("FoodStampsRecipientName", screening.IRSForm9061.Q14FirstName.Trim() + " " + screening.IRSForm9061.Q14LastName.Trim());
                form.FillField("FoodStampsCityAndState", screening.IRSForm9061.Q14City.Trim() + ", " + screening.IRSForm9061.Q14State.Trim());
            }
            #endregion

            #region VocationalRehab
            if (screening.IRSForm9061.Q15a)
            {
                form.FillField("VocationalRehabStateYes", "X");
            }
            else
            {
                form.FillField("VocationalRehabStateNo", "X");
            }
            #endregion

            #region TicketToWork
            if (screening.IRSForm9061.Q15b)
            {
                form.FillField("VocationalRehabTicketToWorkYes", "X");
            }
            else
            {
                form.FillField("VocationalRehabTicketToWorkNo", "X");
            }
            #endregion

            #region VeteranAffairsReferral
            if (screening.IRSForm9061.Q15c)
            {
                form.FillField("VocationalRehabVeteransAffairsYes", "X");
            }
            else
            {
                form.FillField("VocationalRehabVeteransAffairsNo", "X");
            }
            #endregion

            #region TANFA
            if (screening.IRSForm9061.Q16a)
            {
                form.FillField("LongTermTANFPast18MonthsYes", "X");
            }
            else
            {
                form.FillField("LongTermTANFPast18MonthsNo", "X");
            }
            #endregion

            #region TANFB
            if (screening.IRSForm9061.Q16b)
            {
                form.FillField("LongTermTANFWithinTwoYearsYes", "X");
            }
            else
            {
                form.FillField("LongTermTANFWithinTwoYearsNo", "X");
            }
            #endregion

            #region TANFC
            if (screening.IRSForm9061.Q16c)
            {
                form.FillField("LongTermTANFMaximumTimeYes", "X");
            }
            else
            {
                form.FillField("LongTermTANFMaximumTimeNo", "X");
            }
            #endregion

            #region TANFRecipient
            if (screening.IRSForm9061.Q16a||screening.IRSForm9061.Q16b||screening.IRSForm9061.Q16c)
            {
                form.FillField("LongTermTANFRecipientName", screening.IRSForm9061.Q16FirstName.Trim() + " " + screening.IRSForm9061.Q16LastName.Trim());
                form.FillField("LongTermTANFCityAndState", screening.IRSForm9061.Q16City.Trim() + ", " + screening.IRSForm9061.Q16State.Trim());
            }
            else
            {
                form.FillField("LongTermTANFNineMonthsNo", "X");
            }
            #endregion

            #region Felony
            if (screening.IRSForm9061.Q17)
            {
                form.FillField("FelonyYes", "X");
            }
            else
            {
                form.FillField("FelonyNo", "X");
            }
            #endregion

            #region Q13DConv
            if (screening.IRSForm9061.Q17)
            {
                if (screening.IRSForm9061.Q17ConvictionDate.HasValue && screening.IRSForm9061.Q17ConvictionDate.Value.Year > 1900)
                {
                    form.FillField("FelonyConvictionDate", screening.IRSForm9061.Q17ConvictionDate.Value.ToShortDateString());
                }

                if (screening.IRSForm9061.Q17ReleaseDate.HasValue && screening.IRSForm9061.Q17ReleaseDate.Value.Year > 1900)
                {
                    form.FillField("FelonyReleaseDate", screening.IRSForm9061.Q17ReleaseDate.Value.ToShortDateString());
                }
            }
            #endregion

            #region LivesInZone
            if (screening.IRSForm9061.Q18)
            {
                form.FillField("EmpowermentZoneYes", "X");
            }
            else
            {
                form.FillField("EmpowermentZoneNo", "X");
            }
            #endregion

            #region LivesInZoneYouth
            if (screening.IRSForm9061.Q19)
            {
                form.FillField("SummerYouthYes", "X");
            }
            else
            {
                form.FillField("SummerYouthNo", "X");
            }
            #endregion

            #region SSI
            if (screening.IRSForm9061.Q20)
            {
                form.FillField("SSIYes", "X");
            }
            else
            {
                form.FillField("SSINo", "X");
            }
            #endregion

            #region UnemployedVetLongTerm
            if (screening.IRSForm9061.Q21)
            {
                form.FillField("UnemployedVeteranSixMonthsYes", "X");
            }
            else
            {
                form.FillField("UnemployedVeteranSixMonthsNo", "X");
            }
            #endregion

            #region UnemployedVetShortTerm
            if (screening.IRSForm9061.Q22)
            {
                form.FillField("UnemployedVeteranFourWeeksYes", "X");
            }
            else
            {
                form.FillField("UnemployedVeteranFourWeeksNo", "X");
            }
            #endregion

            #region LongTermUnemployed
            if (screening.IRSForm9061.Q23a)
            {
                form.FillField("LongTermUnemployedYes", "X");
                //form.FillField("LongTermUnemployedReceivedBenefitsYes", "X");
                form.FillField("LongTermUnemployedState", screening.LongTermUnemployed.StateReceived.Trim());
            }
            else
            {
                form.FillField("LongTermUnemployedNo", "X");
            }
            #endregion

            #region SourceDocuments
            form.FillField("DocumentationSources", screening.IRSForm9061.Q24 == null ? string.Empty : screening.IRSForm9061.Q24.Trim());
            #endregion

            #region Consultant
            form.FillField("ConsultantSignature", true);
            #endregion

            #region Date
            form.FillField("SignatureDate", screening.ProcessedDate.HasValue ? screening.ProcessedDate.Value.ToShortDateString() : string.Empty);
            #endregion

            form.FlattenAllFields();

            MemoryStream outputStream = new MemoryStream();
            form.Save(outputStream);

            Aspose.Pdf.Document outputDocument = new Aspose.Pdf.Document(outputStream);

            MemoryStream returnValueStream = new MemoryStream();
            outputDocument.Save(returnValueStream);

            returnValueStream.Position = 0;

            return returnValueStream;
        }

        public MemoryStream RenderCoverLetter(string addressBlock, string salutation, List<Screening> screenings)
        {
            Aspose.Words.Document wordDocument = new Aspose.Words.Document(new MemoryStream(SharedFilesConfiguration.Current.CoverLetter));
            Aspose.Words.DocumentBuilder builder = new Aspose.Words.DocumentBuilder(wordDocument);

            //string address = string.Format("{0} {1}", state.WOTCCoordinator.FirstName == null ? string.Empty : state.WOTCCoordinator.FirstName.Trim(), state.WOTCCoordinator.LastName == null ? string.Empty : state.WOTCCoordinator.LastName.Trim());
            //if (!string.IsNullOrEmpty(state.WOTCCoordinator.Title))
            //    address += "\n" + state.WOTCCoordinator.Title.Trim();

            //if (!string.IsNullOrEmpty(state.WOTCCoordinator.Department))
            //    address += "\n" + state.WOTCCoordinator.Department.Trim();

            //if (!string.IsNullOrEmpty(state.WOTCCoordinator.Division))
            //    address += "\n" + state.WOTCCoordinator.Division.Trim();

            //address += string.Format("\n{0}, {1} {2}", state.WOTCCoordinator.City.Trim(), state.WOTCCoordinator.State.Trim(), state.WOTCCoordinator.ZipCode.Trim());

            builder.MoveToBookmark("WOTCAddress");
            builder.Writeln(addressBlock);

            builder.MoveToBookmark("Salutation");
            builder.Writeln(salutation);

            builder.MoveToBookmark("EnclosureTable");
            var table = builder.StartTable();

            builder.InsertCell();
            builder.RowFormat.Height = 15;
            builder.RowFormat.HeightRule = Aspose.Words.HeightRule.Exactly;
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPoints(35);
            builder.Writeln("DOC");
            builder.InsertCell();
            builder.CellFormat.PreferredWidth = PreferredWidth.FromPoints(175);
            builder.Writeln("NAME");
            builder.InsertCell();
            builder.Writeln("SSN");
            builder.InsertCell();
            builder.Writeln("HIRE DATE");
            builder.EndRow();

            foreach (var screening in screenings.OrderBy(s => s.Applicant.LastName + "_" + s.Applicant.FirstName))
            {

                builder.InsertCell();
                builder.RowFormat.Height = 15;
                builder.RowFormat.HeightRule = Aspose.Words.HeightRule.Exactly;
                builder.Writeln("8850");
                builder.InsertCell();
                builder.RowFormat.Height = 15;
                builder.RowFormat.HeightRule = Aspose.Words.HeightRule.Exactly;
                builder.Writeln("9061");

                builder.InsertCell();
                builder.Writeln(string.Format("{0} {1}", screening.Applicant.FirstName.Trim(), screening.Applicant.LastName.Trim()));

                var social = string.IsNullOrEmpty(screening.Applicant.SocialSecurityNumber) ? "000-00-0000" :
                    (screening.Applicant.SocialSecurityNumber.Length == 7 ? "0" + screening.Applicant.SocialSecurityNumber.Substring(0, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(2, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(4, 4) :
                        screening.Applicant.SocialSecurityNumber.Substring(0, 3) + "-" + screening.Applicant.SocialSecurityNumber.Substring(3, 2) + "-" + screening.Applicant.SocialSecurityNumber.Substring(5, 4));


                builder.InsertCell();
                builder.Writeln(social);

                builder.InsertCell();
                builder.Writeln(screening.WasHired.Value.ToString("MM/dd/yyyy"));
                builder.EndRow();
            }

            builder.EndTable();

            for (int i = 0; i < table.Rows.Count; i++)
            {
                table.Rows[i].RowFormat.Borders.LineStyle = Aspose.Words.LineStyle.None;
            }

            MemoryStream stream = new MemoryStream();
            wordDocument.Save(stream, Aspose.Words.SaveFormat.Pdf);
            stream.Position = 0;
            return stream;
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

    public class SharedFilesConfiguration
    {
        static SharedFilesConfiguration current;

        static SharedFilesConfiguration()
        {

        }

        public static SharedFilesConfiguration Current
        {
            get
            {
                if (current == null)
                    current = new SharedFilesConfiguration();
                return current;
            }
        }

        public byte[] AsposeLicense { get; set; }
        public byte[] Form8850Page1 { get; set; }
        public byte[] Form8850Page2 { get; set; }
        public byte[] Form9061 { get; set; }

        public byte[] Form9175 { get; set; }
        public byte[] CoverLetter { get; set; }
        public byte[] TCQ { get; set; }
        public byte[] SSA { get; set; }
    }
}
