using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API CODE");

            // Print new submission id
            var user = client.getUser();

            var submissions = client.getSubmissions()["content"];

            var fs = from sub in submissions
                     where (string)sub["new"] == "1"
                     select sub.Value<string>("id");

            foreach (var item in fs)
            {
                Console.Out.Write(item.ToString());
            }
            

            // Print all forms of the user

            var forms = client.getForms()["content"];

            var formTitles = from form in forms
                             select form.Value<string>("title");

            foreach (var item in formTitles)
            {
                Console.Out.WriteLine(item);
            }
            
            // Get latest submissions of the user

            var allsubmissions = client.getSubmissions()["content"];

            var createdat = (from submission in allsubmissions
                            select submission.Value<string>("created_at")).ToArray();

            for (int i = 0; i < createdat.Length; i++)
            {
                Console.Out.WriteLine(createdat[i]);

                var answer = from sub in allsubmissions
                             from ans in sub["answers"]
                             where sub.Value<string>("created_at") == createdat[i]
                             select ans;

                foreach (var item in answer)
                {
                    Console.Out.WriteLine(item);
                }
            }
            
            Console.ReadLine();
        }
    }
}
