jotform-api-csharp
==================

JotForm API - C# Client

### Installation

Install via git clone:

        $ git clone git://github.com/jotform/jotform-api-csharp.git
        $ cd jotform-api-csharp
        

### Documentation

You can find the docs for the API of this client at [http://api.jotform.com/docs/](http://api.jotform.com/docs)

### Authentication

JotForm API requires API key for all user related calls. You can create your API Keys at  [API section](http://www.jotform.com/myaccount/api) of My Account page.

### Examples

Print all forms of the user

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

            var forms = client.getForms()["content"];

            var formTitles = from form in forms
                             select form.Value<string>("title");

            foreach (var item in formTitles)
            {
                Console.Out.WriteLine(item);
            }
            
            Console.ReadLine();
        }
    }
}

   
Get latest submissions of the user

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


    

