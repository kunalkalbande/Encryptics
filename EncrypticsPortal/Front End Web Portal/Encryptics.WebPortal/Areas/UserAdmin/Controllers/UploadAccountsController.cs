using AutoMapper;
using Encryptics.Common;
using Encryptics.WebPortal.Areas.UserAccount.Models;
using Encryptics.WebPortal.Areas.UserAdmin.Models;
using Encryptics.WebPortal.Controllers;
using Encryptics.WebPortal.Filters;
using Encryptics.WebPortal.Models;
using Encryptics.WebPortal.PortalService;
using LumenWorks.Framework.IO.Csv;
using ObjectSerializer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Encryptics.WebPortal.Areas.UserAdmin.Controllers
{
    public class UploadAccountsController : PortalServiceAwareController
    {
        public UploadAccountsController(PortalServiceSoap portalService) : base(portalService) { }

        #region Actions
        [HttpGet]
        public async Task<ActionResult> Index(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(long entityId, HttpPostedFileBase fileupload, int? month, int? year)
        {
            try
            {
                await InitializeViewBagAsync(entityId);

                var extension = Path.GetExtension(fileupload.FileName);

                var tempFileName = GetFilename(fileupload, extension);

                var newPendingAccounts = GetNewPendingAccounts(extension, tempFileName);

                DeleteTempUploadFile(tempFileName);

                if (newPendingAccounts != null)
                {
                    var insertAccountModels = ValidateAccounts(newPendingAccounts).ToList();

                    Trace.Write(string.Format("Accounts uploaded: {0}", insertAccountModels.Count()));

                    return View("ReviewPendingAccounts", newPendingAccounts);
                }

                Trace.Write("UploadAccounts: newPendingAccounts is null.");

                throw new ApplicationException("No accounts found in uploaded file.");
            }
            catch (Exception e)
            {
                Trace.TraceError(string.Format("Exception thrown: {0}", e.Message));
                Trace.TraceInformation(e.StackTrace);

                ViewData["ErrorMessage"] = e.Message;

                return View();
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<ActionResult> ReviewPendingAccounts(long entityId, IEnumerable<InsertableUserAccountModel> newPendingAccounts,
                                                  bool? showDownloadButton = true)
        {
            Debug.Print("EntityId: {0}", entityId);
            Debug.Print("ShowDownloadButton: {0}", showDownloadButton);
            TempData.Clear();
            await InitializeViewBagAsync(entityId);

            if (newPendingAccounts != null)
            {
                var editableUserAccountModels = newPendingAccounts as IList<InsertableUserAccountModel> ?? newPendingAccounts.ToList();
                Debug.Print("Accounts found ({0}).", editableUserAccountModels.Count);
                var accountList = ValidateAccounts(editableUserAccountModels).ToList();

                if (accountList.Any(x => x.HasValidationErrors))
                {
                    Debug.Print("Some usernames did not pass validation.");
                    ViewData["ErrorMessage"] = "Some usernames did not pass validation.";
                }
                else if (accountList.Count > 0)
                {
                    Debug.Print("Uploading accounts to server.");
                    var response = await UploadAccountsToServer(accountList, showDownloadButton ?? true, entityId);

                    if (response.Status == InsertUsersByBulkStatus.Success)
                    {
                        TempData["SuccessMessage"] = "All accounts uploaded successfully.";

                        return RedirectToAction("Dashboard", "CompanyHome", new { entityId, area = "Company" });
                    }

                    if (response.Failures.Any())
                    {
                        Debug.Print("Failures found.");
                        SetErrorMessage(response, accountList);
                    }
                    else
                    {
                        ViewData["ErrorMessage"] = "Upload attempt failed.";
                    }
                }

                ViewBag.PromptForDownload = showDownloadButton;

                return View("ReviewPendingAccounts", accountList);
            }

            Debug.Print("Accounts list is NULL!");

            return View("ReviewPendingAccounts", new List<InsertableUserAccountModel>());
        }

        [HttpGet]
        public async Task<ActionResult> UploadedFiles(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            var request = new GetImportUsersRequestsRequest
                            {
                                TokenAuth = _tokenAuth,
                                entity_id = entityId
                            };

            var response = await _portalService.GetImportUsersRequestsAsync(request);

            var model = new PageableViewModel<ImportFileModel>
                {
                    CurrentPage = 1,
                    PageSize = 10
                };

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetImportUsersRequestsResult != null)
            {
                model.DataItems = Mapper.Map<ImportUsersRequest[], IEnumerable<ImportFileModel>>(response.GetImportUsersRequestsResult).OrderBy(f => f.DateUploaded);
            }

            return View(model);
        }

        [HttpGet]
        public async Task<ActionResult> UploadedAccounts(long entityId, long importFileId)
        {
            await InitializeViewBagAsync(entityId);

            var request = new GetImportUsersAccountsRequest
                {
                    TokenAuth=_tokenAuth,
                    import_file_id = importFileId
                };

            var response = await _portalService.GetImportUsersAccountsAsync(request);

            var model = new PageableViewModel<InsertableUserAccountModel>
                {
                    CurrentPage=1,
                    PageSize=10
                };

            if (response.TokenAuth.Status == TokenStatus.Succes && response.GetImportUsersAccountsResult != null)
            {
                model.DataItems =
                    Mapper.Map<ImportUsersAccount[], IEnumerable<InsertableUserAccountModel>>(
                        response.GetImportUsersAccountsResult);
            }

            return View(model);
        }

        [HttpGet/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> UploadFile(long entityId)
        {
            await InitializeViewBagAsync(entityId);

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken/*, AuthorizeAction(true)*/]
        public async Task<ActionResult> UploadFile(long entityId, AccountsFileUploadModel model, HttpPostedFileBase uploadedFile)
        {
            await InitializeViewBagAsync(entityId);

            // Read bytes from http input stream
            var binaryReader = new BinaryReader(uploadedFile.InputStream);
            var fileContents = binaryReader.ReadBytes((int)uploadedFile.InputStream.Length);

            var importUsersRequest = new ImportUsersRequest
                {
                    FileContents = fileContents,
                    FilePath = uploadedFile.FileName,
                    ReserveLicense = model.ReserveLicenses,
                    SoftwarePreinstalled = model.SoftwarePreinstalled
                };

            var request = new InsertImportUsersRequestRequest
                {
                    TokenAuth = _tokenAuth,
                    entity_id = entityId,
                    import_request = importUsersRequest
                };

            var response = await _portalService.InsertImportUsersRequestAsync(request);

            if (response.TokenAuth.Status == TokenStatus.Succes &&
                response.InsertImportUsersRequestResult.Status == InsertImportUsersRequestStatus.Success)
            {
                ViewData["SuccessMessage"] = "File was successfully uploaded. ";
            }
            else
            {
                ViewData["ErrorMessage"] = string.Format("Upload unsuccessful ({0}).", Enum.GetName(typeof(InsertImportUsersRequestStatus),
                                                        response.InsertImportUsersRequestResult.Status));
            }

            return View();
        }

        [HttpGet]
        public async Task<FileResult> SampleFile()
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream);

            await streamWriter.WriteLineAsync("first, last, email");
            await streamWriter.WriteLineAsync("test, user1, testuser1@somedomain.com");
            await streamWriter.WriteLineAsync(", user2, testuser2@somedomain.com");
            await streamWriter.WriteLineAsync("user3, , testuser3@somedomain.com");
            await streamWriter.WriteLineAsync(", , testuser4@somedomain.com");

            await streamWriter.FlushAsync();

            return File(memoryStream, "text/csv", "samplefile.csv");
        }

        #endregion

        #region Helpers
        private static string GetFilename(HttpPostedFileBase fileupload, string extension)
        {
            if (fileupload == null) throw new ArgumentNullException("fileupload");

            var tempFileName = Path.GetTempFileName().Replace(".tmp", extension);

            fileupload.SaveAs(tempFileName);

            return tempFileName;
        }

        private static IEnumerable<InsertableUserAccountModel> GetNewPendingAccounts(string extension, string tempFileName)
        {
            if (extension == null) throw new ArgumentNullException("extension");

            if (tempFileName == null) throw new ArgumentNullException("tempFileName");

            if (new[] { ".csv", ".txt" }.Any(extension.Equals))
            {
                return GetAccountsFromCsv(tempFileName);
            }

            throw new ApplicationException("File uploaded not in a supported format.");
        }

        private static void DeleteTempUploadFile(string tempFileName)
        {
            if (System.IO.File.Exists(tempFileName))
            {
                System.IO.File.Delete(tempFileName);
            }
        }

        private static IEnumerable<InsertableUserAccountModel> ValidateAccounts(
            IEnumerable<InsertableUserAccountModel> newPendingAccounts)
        {
            var uniqueList = new HashSet<string>();
            var insertAccountModels = newPendingAccounts as IList<InsertableUserAccountModel> ?? newPendingAccounts.ToList();

            Debug.Print("Validating new accounts.");

            foreach (var account in insertAccountModels)
            {
                var regEx = new Regex(@"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                      + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                      + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$", RegexOptions.IgnoreCase);
                if (string.IsNullOrEmpty(account.UserName) || string.IsNullOrWhiteSpace(account.UserName))
                {
                    Debug.Print("Empty email address found: {0}", account.UserName);
                    account.ValidationError = "Email address required.";
                }
                else if (!regEx.IsMatch(account.UserName))
                {
                    Debug.Print("Email address failed regex validation: {0}", account.UserName);
                    account.ValidationError = "Invalid email format.";
                }

                if (!(string.IsNullOrEmpty(account.UserName) || string.IsNullOrWhiteSpace(account.UserName)) && (uniqueList.Contains(account.UserName)))
                {
                    Debug.Print("Duplicate email address found: {0}", account.UserName);
                    account.ValidationError += "This email address is already found in this list.";
                }
                else
                {
                    Debug.Print("Passed validation: {0}", account.UserName);
                    uniqueList.Add(account.UserName);
                }
            }

            return insertAccountModels;
        }

        private async Task<InsertUsersByBulkResult> UploadAccountsToServer(string accountsXml, bool showDownloadButton, long entityId)
        {
            var response = await _portalService.InsertUsersByBulkAsync(new InsertUsersByBulkRequest(_tokenAuth, entityId, accountsXml, showDownloadButton));

            return response.InsertUsersByBulkResult;
        }

        private async Task<InsertUsersByBulkResult> UploadAccountsToServer(IEnumerable<InsertableUserAccountModel> accountList,
                                                             bool showDownloadButton, long entityId)
        {
            var accountsXml = GetAccountsXml(accountList);

            return await UploadAccountsToServer(accountsXml, showDownloadButton, entityId);
        }

        private static string GetAccountsXml(IEnumerable<InsertableUserAccountModel> accountList)
        {
            var accountsToUpload = new UploadAccountsModel
            {
                Accounts = accountList.ToList()
            };

            var accountsXml = new XmlObjectSerializer<UploadAccountsModel>().Serialize(accountsToUpload);

            Trace.WriteLine(accountsXml);

            return accountsXml;
        }

        private void SetErrorMessage(InsertUsersByBulkResult response, IEnumerable<InsertableUserAccountModel> accountList)
        {
            if (response.Failures == null) return;

            var insertAccountModels = accountList as IList<InsertableUserAccountModel> ?? accountList.ToList();

            var errorMap = new Dictionary<InsertUsersByBulkFailureType, string>
                {
                    {InsertUsersByBulkFailureType.Username_Exists, "Username is in use"},
                    {
                        InsertUsersByBulkFailureType.Bad_Domain,
                        "Domain specified in the username does not match available domains."
                    },
                    {InsertUsersByBulkFailureType.Bad_First_Name, "First name failed validation."},
                    {InsertUsersByBulkFailureType.Bad_Last_Name, "Last name failed validation."}
                };

            foreach (var error in response.Failures)
            {
                var account =
                    insertAccountModels.Single(
                        x => x.UserName.Equals(error.Username, StringComparison.OrdinalIgnoreCase));
                if (account != null)
                {
                    account.ValidationError = errorMap[error.Type];
                }
            }

            TempData["ErrorMessage"] =
                "Upload failed. Hover over the information icon next to the email address to see specific errors for why that account did not upload.";
        }

        private static IEnumerable<InsertableUserAccountModel> GetAccountsFromCsv(string tempFileName)
        {
            if (tempFileName == null) throw new ArgumentNullException("tempFileName");

            var newPendingAccounts = new List<InsertableUserAccountModel>();
            var hasHeaders = HasHeaders(tempFileName);
            var headerMap = new Dictionary<string, int>();

            using (var csv = new CsvReader(new StreamReader(tempFileName), hasHeaders))
            {
                SetHeaderMap(hasHeaders, csv, headerMap);

                while (csv.ReadNextRecord())
                {
                    var userName = csv[headerMap["email"]];
                    var firstName = csv[headerMap["first"]];
                    var lastName = csv[headerMap["last"]];

                    if (!string.IsNullOrEmpty(userName) || !string.IsNullOrEmpty(firstName) ||
                        !string.IsNullOrEmpty(lastName))
                    {
                        newPendingAccounts.Add(new InsertableUserAccountModel
                        {
                            UserName = userName,
                            FirstName = firstName,
                            LastName = lastName,
                            AssignLicense = true
                        });
                    }
                }
            }

            return newPendingAccounts;
        }

        private static bool HasHeaders(string tempFileName)
        {
            if (tempFileName == null) throw new ArgumentNullException("tempFileName");

            var hasHeaders = false;
            var streamReader = new StreamReader(tempFileName);
            var firstLine = streamReader.ReadLine();
            if (firstLine != null && (firstLine.ToLower().Contains("email") ||
                                      firstLine.ToLower().Contains("first") ||
                                      firstLine.ToLower().Contains("last")))
            {
                hasHeaders = true;
            }
            streamReader.Close();
            streamReader.Dispose();

            return hasHeaders;
        }

        private static void SetHeaderMap(bool hasHeaders, CsvReader csv, IDictionary<string, int> headerMap)
        {
            if (csv == null) throw new ArgumentNullException("csv");

            if (headerMap == null) throw new ArgumentNullException("headerMap");

            if (hasHeaders)
            {
                SetHeaderMap(csv, headerMap);
            }
            else
            {
                headerMap.Add("email", 0);
                headerMap.Add("first", 1);
                headerMap.Add("last", 2);
            }
        }

        private static void SetHeaderMap(CsvReader csv, IDictionary<string, int> headerMap)
        {
            if (csv == null) throw new ArgumentNullException("csv");

            if (headerMap == null) throw new ArgumentNullException("headerMap");

            try
            {
                var fileHeaders = csv.GetFieldHeaders();

                SetFieldIndex(fileHeaders, new[] { "email", "user" }, csv, headerMap, "email");

                SetFieldIndex(fileHeaders, new[] { "first", "fname" }, csv, headerMap, "first");

                SetFieldIndex(fileHeaders, new[] { "last", "lname" }, csv, headerMap, "last");
            }
            catch (Exception e)
            {
                Trace.TraceWarning("Exception found {0} setting XML headers of uploaded file: {1}", new object[] { e.GetType().ToString(), e.Message });
                Trace.TraceInformation(e.StackTrace);

                throw new EncrypticsException(@"Invalid file format. CSV Column headings must be ""Email"", ""First"" and ""Last"".");
            }
        }

        private static void SetFieldIndex(IEnumerable<string> fileHeaders,
                                          IEnumerable<string> headerOptions, CsvReader csv,
                                          IDictionary<string, int> headerMap, string defaultHeaderName)
        {
            if (fileHeaders == null) throw new ArgumentNullException("fileHeaders");
            if (headerOptions == null) throw new ArgumentNullException("headerOptions");
            if (csv == null) throw new ArgumentNullException("csv");
            if (headerMap == null) throw new ArgumentNullException("headerMap");
            if (defaultHeaderName == null) throw new ArgumentNullException("defaultHeaderName");

            var fieldName = fileHeaders.First(s1 => headerOptions.Any(s1.ToLower().Contains));
            var fieldIndex = csv.GetFieldIndex(fieldName);
            headerMap.Add(defaultHeaderName, fieldIndex);
        }

        #endregion
    }
}
