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

        private string baseURL = "http://api.jotform.com/";
        private string apiVersion = "v1";

        public APIClient(String apiKey, bool debugMode=false)
        {
            this.apiKey = apiKey;

            this.debugMode = debugMode;
        }

        public JObject executeHttpRequest(string path, NameValueCollection parameters, string method)
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
            

            if (method == "POST")
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

        private string ToQueryString(NameValueCollection nvc)
        {
            var array = (from key in nvc.AllKeys
                         from value in nvc.GetValues(key)
                         select string.Format("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value)))
                .ToArray();

            return string.Join("&", array);
        }

        public JObject executeGetRequest(string path, NameValueCollection parameters=null)
        {
            return executeHttpRequest(path, parameters, "GET");
        }

        public JObject executePostRequest(string path, NameValueCollection parameters=null)
        {
            return executeHttpRequest(path, parameters, "POST");
        }

        public JObject executeDeleteRequest(string path, NameValueCollection parameters = null)
        {
            return executeHttpRequest(path, parameters, "DELETE");
        }

        public JObject getUser()
        {
            return executeGetRequest("/user");
        }

        public JObject getUsage()
        {
            return executeGetRequest("/user/usage");
        }

        public JObject getForms()
        {
            return executeGetRequest("/user/forms");
        }

        public JObject getSubmissions()
        {
            return executeGetRequest("/user/submissions");
        }

        public JObject getSubusers()
        {
            return executeGetRequest("/user/subusers");
        }

        public JObject getFolders()
        {
            return executeGetRequest("/user/folders");
        }

        public JObject getReports()
        {
            return executeGetRequest("/user/reports");
        }

        public JObject getSettings()
        {
            return executeGetRequest("/user/settings");
        }

        public JObject getHistory()
        {
            return executeGetRequest("/user/history");
        }

        public JObject getForm(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString());
        }

        public JObject getFormQuestions(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/questions");
        }

        public JObject getFormQuestion(long formID, long qid)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/question/" + qid.ToString());
        }

        public JObject getFormSubmissons(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/submissions");
        }
        
        public JObject createFormSubmissions(long formID, Dictionary<string, string> submission)
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
        
        public JObject getFormFiles(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/files");
        }

        public JObject getFormWebhooks(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/webhooks");
        }

        public JObject createFormWebhook(long formID, string webhookURL)
        {
            var data = new NameValueCollection();
            data.Add("webhookURL", webhookURL);
            return executePostRequest("/form/" + formID.ToString() + "/webhooks", data);
        }        

        public JObject getSubmission(long sid)
        {
            return executeGetRequest("/submission/" + sid.ToString());
        }

        public JObject getReport(long reportID)
        {
            return executeGetRequest("/report/" + reportID.ToString());
        }

        public JObject getFolder(long folderID.ToString())
        {
            return executeGetRequest("/folder/" + folderID);
        }

        public JObject getFormProperties(long formID)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/properties");
        }

        public JObject getFormProperty(long formID, string propertyKey)
        {
            return executeGetRequest("/form/" + formID.ToString() + "/properties/" + propertyKey);
        }

        public JObject deleteSubmission(long sid)
        {
            return executeDeleteRequest("/submission/" + sid.ToString());
        }

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

        public JObject cloneForm(long formID)
        {
            return executePostRequest("/form/" + formID.ToString() + "/clone");
        }

        public JObject deleteFormQuestion(long formID, long qid)
        {
            return executeDeleteRequest("/form/" + formID.ToString() + "/question/" + qid.ToString());
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