// JotForm API - C# Client
// copyright   2013 Interlogy, LLC.
// link        http://www.jotform.com
// version     1.0
// package     JotFormAPI

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.IO;
using System.Text;
using System.Net;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotForm
{
    public class APIClient
    {
        private string apiKey;
        private bool debugMode;

        private string baseURL = "https://api.jotform.com/";
        private string apiVersion = "v1";

        public APIClient()
        {
            this.apiKey = null;
            this.debugMode = false;
        }

        public APIClient(String apiKey, bool debugMode=false)
        {
            this.apiKey = apiKey;
            this.debugMode = debugMode;
        }

        private JObject executeHttpRequest(string path, NameValueCollection parameters, string method)
        {
            if (method == "GET" && parameters != null)
            {
                path = path + "?" + ToQueryString(parameters);
            }

            WebRequest req = WebRequest.Create(this.baseURL + this.apiVersion + path);
            req.Method = method;

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("apiKey:" + apiKey );
            req.Headers = headers;
            

            if (method == "POST" && parameters != null)
            {
                var data = Encoding.UTF8.GetBytes(ToQueryString(parameters));
                
                req.ContentLength = data.Length;
                req.ContentType = "application/x-www-form-urlencoded";

                Stream dataStream = req.GetRequestStream();

                dataStream.Write(data, 0, data.Length);
                dataStream.Close();
            }

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream answerStream = resp.GetResponseStream();
            String answerString = new StreamReader(answerStream).ReadToEnd();

            var responseCode = JObject.Parse(answerString)["responseCode"];

            if (responseCode.ToString() != "200")
            {
                if (responseCode.ToString() == "401")
                {
                    throw new JotformException("Unauthozired API call");
                }
                else if (responseCode.ToString() == "404")
                {
                    throw new JotformException(JObject.Parse(answerString)["message"].ToString());
                }
                else if (responseCode.ToString() == "503")
                {
                    throw new JotformException("Service is unavaible, rate limits etc exceeded!");
                }
            }

            return JObject.Parse(answerString);
        }

        private JObject executeHttpRequest(string path, string parameters)
        {
            WebRequest req = WebRequest.Create(this.baseURL + path);
            req.Method = "PUT";

            WebHeaderCollection headers = new WebHeaderCollection();
            headers.Add("apiKey:" + apiKey);
            req.Headers = headers;

            var data = Encoding.UTF8.GetBytes(parameters.ToString());

            req.ContentLength = data.Length;
            req.ContentType = "application/x-www-form-urlencoded";

            Stream dataStream = req.GetRequestStream();

            dataStream.Write(data, 0, data.Length);
            dataStream.Close();

            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            Stream answerStream = resp.GetResponseStream();
            String answerString = new StreamReader(answerStream).ReadToEnd();

            return JObject.Parse(answerString); 
        }

        private JObject executeGetRequest(string path, NameValueCollection parameters = null)
        {
            return executeHttpRequest(path, parameters, "GET");
        }

        private JObject executePostRequest(string path, NameValueCollection parameters = null)
        {
            return executeHttpRequest(path, parameters, "POST");
        }

        private JObject executeDeleteRequest(string path, NameValueCollection parameters = null)
        {
            return executeHttpRequest(path, parameters, "DELETE");
        }

        private JObject executePutRequest(string path, string parameters = null)
        {
            return executeHttpRequest(path, parameters);
        }

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();

            return string.Join("&", array);
        }

        private NameValueCollection CreateConditions(int offset, int limit, Dictionary<String, String> filter, String orderBy) 
        {
            NameValueCollection parameters = new NameValueCollection();

            if (offset != 0)
            {
                parameters.Add("offset", offset.ToString());
            }

            if (limit != 0)
            {
                parameters.Add("limit", limit.ToString());
            }

            if (filter != null)
            {
                String value = "{";
                int count = 0;

                foreach (KeyValuePair<String, String> pair in filter)
                {
                    value = value + "\"" + pair.Key + "\":\"" + pair.Value + "\"";

                    count++;

                    if (count < filter.Count) 
                    {
                        value = value + ",";
                    }
                }

                value = value + "}";

                parameters.Add("filter", value);
            }

            if (orderBy != "")
            {
                parameters.Add("order_by", orderBy);
            }

            return parameters;
 
        }

        private NameValueCollection CreateHistoryQuery(string action, string date, string sortBy, string startDate, string endDate)
        {
            Dictionary<String, String> args = new Dictionary<string, string>();
            args.Add("action", action);
            args.Add("date", date);
            args.Add("sortBy", sortBy);
            args.Add("startDate", startDate);
            args.Add("endDate", endDate);

            NameValueCollection parameters = new NameValueCollection();

            foreach (KeyValuePair<String, String> pair in args)
            {
                parameters.Add(pair.Key, pair.Value);
            }

            return parameters;
        }

        /// <summary>
        /// Get user account details for a JotForm user
        /// </summary>
        /// <returns>User account type, avatar URL, name, email, website URL and account limits</returns>
        public JObject getUser()
        {
            return executeGetRequest("/user");
        }

        /// <summary>
        /// Get number of form submissions received this month
        /// </summary>
        /// <returns>Number of submissions, number of SSL form submissions, payment form submissions and upload space used by user</returns>
        public JObject getUsage()
        {
            return executeGetRequest("/user/usage");
        }

        /// <summary>
        /// Get a list of forms for this account
        /// </summary>
        /// <param name="offset">Start of each result set for form list (optional)</param>
        /// <param name="limit">Number of results in each result set for form list (optional)</param>
        /// <param name="filter">Filters the query results to fetch a specific form range (optional)</param>
        /// <param name="orderBy">Order results by a form field name (optional)</param>
        /// <returns>Basic details such as title of the form, when it was created, number of new and total submissions</returns>
        public JObject getForms(int offset = 0, int limit = 0, Dictionary<String, String> filter = null, String orderBy = null)
        {
            NameValueCollection parameters = CreateConditions(offset, limit, filter, orderBy);

            return executeGetRequest("/user/forms", parameters);
        }

        /// <summary>
        /// Get a list of submissions for this account
        /// </summary>
        /// <param name="offset">Start of each result set for form list. (optional)</param>
        /// <param name="limit">Number of results in each result set for form list. (optional)</param>
        /// <param name="filter">Filters the query results to fetch a specific form range.(optional)</param>
        /// <param name="orderBy">Order results by a form field name. (optional)</param>
        /// <returns>Basic details such as title of the form, when it was created, number of new and total submissions</returns>
        public JObject getSubmissions(int offset = 0, int limit = 0, Dictionary<String, String> filter = null, String orderBy = null)
        {
            NameValueCollection parameters = CreateConditions(offset, limit, filter, orderBy);

            return executeGetRequest("/user/submissions", parameters);
        }

        /// <summary>
        /// Get a list of sub users for this account
        /// </summary>
        /// <returns>List of forms and form folders with access privileges</returns>
        public JObject getSubusers()
        {
            return executeGetRequest("/user/subusers");
        }

        /// <summary>
        /// Get a list of form folders for this account
        /// </summary>
        /// <returns>List of forms and form folders with access privileges</returns>
        public JObject getFolders()
        {
            return executeGetRequest("/user/folders");
        }

        /// <summary>
        /// List of URLS for reports in this account
        /// </summary>
        /// <returns>Reports for all of the forms. ie. Excel, CSV, printable charts, embeddable HTML tables</returns>
        public JObject getReports()
        {
            return executeGetRequest("/user/reports");
        }

        /// <summary>
        /// Get user's settings for this account
        /// </summary>
        /// <returns>User's time zone and language</returns>
        public JObject getSettings()
        {
            return executeGetRequest("/user/settings");
        }
        /// <summary>
        /// Update user's settings
        /// </summary>
        /// <param name="settings">New user setting values with setting keys</param>
        /// <returns>Changes on user settings</returns>
        public JObject updateSettings(Dictionary<string, string> settings)
        {
            NameValueCollection parameters = new NameValueCollection();

            foreach (var pair in settings)
            {
                parameters.Add(pair.Key, pair.Value);
            }

            return executePostRequest("/user/settings", parameters);
        }

        /// <summary>
        /// Get user activity log
        /// </summary>
        /// <param name="action">Filter results by activity performed. Default is 'all'.</param>
        /// <param name="date">Limit results by a date range. If you'd like to limit results by specific dates you can use startDate and endDate fields instead.</param>
        /// <param name="sortBy">Lists results by ascending and descending order.</param>
        /// <param name="starDate">Limit results to only after a specific date. Format: MM/DD/YYYY.</param>
        /// <param name="endDate">Limit results to only before a specific date. Format: MM/DD/YYYY.</param>
        /// <returns>Activity log about things like forms created/modified/deleted, account logins and other operations</returns>
        public JObject getHistory(string action = "", string date = "", string sortBy = "", string startDate = "", string endDate = "")
        {
            NameValueCollection parameters = CreateHistoryQuery(action, date, sortBy, startDate, endDate);

            return executeGetRequest("/user/history", parameters);
        }

        /// <summary>
        /// Get basic information about a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>Form ID, status, update and creation dates, submission count etc.</returns>
        public JObject getForm(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString());
        }

        /// <summary>
        /// Get a list of all questions on a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>Question properties of a form</returns>
        public JObject getFormQuestions(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/questions");
        }

        /// <summary>
        /// Get details about a question
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="qid">Identifier for each question on a form. You can get a list of question IDs from /form/{id}/questions.</param>
        /// <returns>Question properties like required and validation</returns>
        public JObject getFormQuestion(long formID, long qid)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/question/" + qid.ToString());
        }

        /// <summary>
        /// List of a form submissions
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="offset">Start of each result set for form list. (optional)</param>
        /// <param name="limit">Number of results in each result set for form list. (optional)</param>
        /// <param name="filter">Filters the query results to fetch a specific form range.(optional)</param>
        /// <param name="orderBy">Order results by a form field name. (optional)</param>
        /// <returns>Submissions of a specific form</returns>
        public JObject getFormSubmissons(long formID, int offset = 0, int limit = 0, Dictionary<String, String> filter = null, String orderBy = null)
        {
            NameValueCollection parameters = CreateConditions(offset, limit, filter, orderBy);

            return executeGetRequest("/form/" + formID.ToString() + "/submissions", parameters);
        }
        
        /// <summary>
        /// Submit data to this form using the API
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="submission">Submission data with question IDs</param>
        /// <returns>Posted submission ID and URL</returns>
        public JObject createFormSubmission(long formID, Dictionary<string, string> submission)
        {
            var data = new NameValueCollection();

            var keys = submission.Keys;

            foreach (var key in keys)
            {
                if (key.Contains("_")) {
                    data.Add("submission[" + key.Substring(0, key.IndexOf("_")) + "][" + key.Substring(key.IndexOf("_") + 1) + "]", submission[key]);
                } else {
                    data.Add("submission[" + key + "]", submission[key]);
                }
            }

            return executePostRequest("/form/" + formID.ToString() + "/submissions", data);
        }

        /// <summary>
        /// Submit data to this form using the API
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="submissions">Submission data with question IDs.</param>
        /// <returns>Posted submission ID and URL.</returns>
        public JObject createFormSubmissions(long formID, string submissions)
        {
            return executePutRequest("/form/" + formID.ToString() + "/submissions", submissions);
        }
        
        /// <summary>
        /// List of files uploaded on a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>uploaded file information and URLs on a specific form</returns>
        public JObject getFormFiles(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/files");
        }

        /// <summary>
        /// Get list of webhooks for a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>List of webhooks for a specific form</returns>
        public JObject getFormWebhooks(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/webhooks");
        }

        /// <summary>
        /// Add a new webhook
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="webhookURL">Webhook URL is where form data will be posted when form is submitted.</param>
        /// <returns>List of webhooks for a specific form</returns>
        public JObject createFormWebhook(long formID, string webhookURL)
        {
            var data = new NameValueCollection();
            data.Add("webhookURL", webhookURL);
            return executePostRequest("/form/" + formID.ToString() + "/webhooks", data);
        }

        /// <summary>
        /// Delete a specific webhook of a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="webhookID">You can get webhook IDs when you call /form/{formID}/webhooks.</param>
        /// <returns>Remaining webhook URLs of form.</returns>
        public JObject deleteFormWebhook(long formID, long webhookID)
        {
            return executeDeleteRequest("/form/" + formID.ToString() + "/webhooks/" + webhookID.ToString());
        }

        /// <summary>
        /// Get submission data
        /// </summary>
        /// <param name="sid">You can get submission IDs when you call /form/{id}/submissions.</param>
        /// <returns>Information and answers of a specific submission</returns>
        public JObject getSubmission(long sid)
        {
            return executeGetRequest("/submission/" + sid.ToString());
        }

        /// <summary>
        /// Get report details
        /// </summary>
        /// <param name="reportID">You can get a list of reports from /user/reports</param>
        /// <returns>Properties of a speceific report like fields and status</returns>
        public JObject getReport(long reportID)
        {
            return executeGetRequest("/report/" + reportID.ToString());
        }

        /// <summary>
        /// Get folder details
        /// </summary>
        /// <param name="folderID">You can get folders IDs when you call /user/folders.</param>
        /// <returns>List of forms in a folder, and other details about the form such as folder color.</returns>
        public JObject getFolder(long folderID)
        {
            return executeGetRequest("/folder/" + folderID.ToString());
        }

        /// <summary>
        /// Get a list of all properties on a form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>Form properties like width, expiration date, style etc.</returns>
        public JObject getFormProperties(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/properties");
        }

        /// <summary>
        /// Get a specific property of the form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="propertyKey">Property key</param>
        /// <returns>Given property key value.]</returns>
        public JObject getFormProperty(long formID, string propertyKey)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/properties/" + propertyKey);
        }

        /// <summary>
        /// Delete a single submission
        /// </summary>
        /// <param name="sid">You can get submission IDs when you call /user/submissions.</param>
        /// <returns>Status of request</returns>
        public JObject deleteSubmission(long sid)
        {
            return executeDeleteRequest("/submission/" + sid.ToString());
        }

        /// <summary>
        /// Edit a single submission
        /// </summary>
        /// <param name="sid">You can get submission IDs when you call /user/submissions.</param>
        /// <param name="submission">New submission data with question IDs</param>
        /// <returns>Status of request</returns>
        public JObject editSubmission(long sid, Dictionary<string, string> submission)
        {
            var data = new NameValueCollection();

            var keys = submission.Keys;

            foreach (var key in keys)
            {
                if (key.Contains("_")) {
                    data.Add("submission[" + key.Substring(0, key.IndexOf("_")) + "][" + key.Substring(key.IndexOf("_") + 1) + "]", submission[key]);
                } else {
                    data.Add("submission[" + key + "]", submission[key]);
                }
            }

            return executePostRequest("/submission/" + sid, data);
        }

        /// <summary>
        /// Clone a single form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>Status of request</returns>
        public JObject cloneForm(long formID)
        {
            return executePostRequest("/form/" + formID.ToString() + "/clone");
        }

        /// <summary>
        /// Delete a single form question
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="qid">Identifier for each question on a form. You can get a list of question IDs from /form/{id}/questions.</param>
        /// <returns></Status of request<returns>
        public JObject deleteFormQuestion(long formID, long qid)
        {
            return executeDeleteRequest("/form/" + formID.ToString() + "/question/" + qid.ToString());
        }

        /// <summary>
        /// Add new question to specified form.
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="questionProperties">New question properties like type and text.</param>
        /// <returns>Returns properties of new question.</returns>
        public JObject createFormQuestion(long formID, Dictionary<string, string> question)
        {
            var data = new NameValueCollection();

            var keys = question.Keys;

            foreach (var key in keys)
            {
                data.Add("question[" + key + "]", question[key]);
            }

            return executePostRequest("/form/" + formID.ToString() + "/questions", data);
        }
        
        /// <summary>
        /// Add new questions to specified form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="questions">New question properties like type and text.</param>
        /// <returns>Returns properties of new questions.</returns>
        public JObject createFormQuestions(long formID, string questions)
        {
            return executePutRequest("/form/" + formID + "/questions", questions);
        }

        /// <summary>
        /// Add or edit a single question properties
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="qid">Identifier for each question on a form. You can get a list of question IDs from /form/{id}/questions.</param>
        /// <param name="questionProperties">New question properties like text and order.</param>
        /// <returns>Returns edited property and type of question.</returns>
        public JObject editFormQuestion(long formID, long qid, Dictionary<string, string> questionProperties)
        {
            var question = new NameValueCollection();

            var keys = questionProperties.Keys;

            foreach (var key in keys)
            {
                question.Add("question[" + key + "]", questionProperties[key]);
            }

            return executePostRequest("/form/" + formID.ToString() + "/question/" + qid.ToString(), question);
        }

        /// <summary>
        /// Add or edit properties of a specific form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="formProperties">New properties like label width.</param>
        /// <returns>Returns edited properties.</returns>
        public JObject setFormProperties(long formID, Dictionary<string, string> formProperties)
        {
            var properties = new NameValueCollection();

            var keys = formProperties.Keys;

            foreach (var key in keys)
            {
                properties.Add("properties[" + key + "]", formProperties[key]);
            }

            return executePostRequest("/form/" + formID + "/properties", properties);
        }

        /// <summary>
        /// Add or edit properties of a specific form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <param name="formProperties">New properties like label width.</param>
        /// <returns>Returns edited properties.</returns>
        public JObject setMultipleFormProperties(long formID, string formProperties)
        {
            return executePutRequest("/form/" + formID + "/properties", formProperties);
        }

        /// <summary>
        /// Create a new form
        /// </summary>
        /// <param name="form">Questions, properties and emails of new form.</param>
        /// <returns>Returns new form.</returns>
        public JObject createForm(Dictionary<string, dynamic> form)
        {
            NameValueCollection parameters = new NameValueCollection();

            foreach (var formPair in form)
            {
                if (formPair.Key.Equals("properties"))
                {
                    foreach (var propertyPair in formPair.Value)
                    {
                        parameters.Add(formPair.Key + "[" + propertyPair.Key + "]", propertyPair.Value);
                    }
                }
                else
                {
                    Dictionary<string, string>[] formItem = formPair.Value;

                    for (int i = 0; i < formItem.Length; i++)
                    {
                        Dictionary<string, string> item = formItem[i];

                        foreach (var pair in item)
                        {
                            parameters.Add(formPair.Key + "[" + i.ToString() + "][" + pair.Key + "]", pair.Value);
                        }

                    }

                }
            }

            return executePostRequest("/user/forms", parameters);
        }

        /// <summary>
        /// Create new forms
        /// </summary>
        /// <param name="form">Questions, properties and emails of new forms.</param>
        /// <returns>Returns new forms.</returns>
        public JObject createForms(string form)
        {
            return executePutRequest("/user/forms", form);
        }

        /// <summary>
        /// Delete a specific form
        /// </summary>
        /// <param name="formID">Form ID is the numbers you see on a form URL. You can get form IDs when you call /user/forms.</param>
        /// <returns>Properties of deleted form.</returns>
        public JObject deleteForm(long formID)
        {
            return executeDeleteRequest("/form/" + formID.ToString());
        }

        /// <summary>
        /// Register with username, password and email
        /// </summary>
        /// <param name="userDetails">Username, password and email to register a new user</param>
        /// <returns>Returns new user's details</returns>
        public JObject registerUser(Dictionary<string, string> userDetails)
        {
            NameValueCollection parameters = new NameValueCollection();

            foreach (var item in userDetails)
            {
                parameters.Add(item.Key, item.Value);
            }

            return executePostRequest("/user/register", parameters);
        }

        /// <summary>
        /// Login user with given credentials
        /// </summary>
        /// <param name="credentials">Username, password, application name and access type of user</param>
        /// <returns>Returns logged in user's settings and app key</returns>
        public JObject loginUser(Dictionary<string, string> credentials)
        {
            NameValueCollection parameters = new NameValueCollection();

            foreach (var item in credentials)
            {
                parameters.Add(item.Key, item.Value);
            }

            return executePostRequest("/user/login", parameters);
        }
    }

    public class JotformException : System.Exception
    {
        public JotformException(string message)
            : base(message)
        {
        }
    }
}