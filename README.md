jotform-api-csharp
==================
[JotForm API](http://api.jotform.com/docs/) - C# Client

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

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class PrintFormList
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API KEY");

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
``` 

Get submissions of the latest form

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class PrintFormSubmissions
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API KEY");

            var forms = client.getForms(0, 1, null, "")["content"];

            var latestForm = forms[0];

            var submissions = client.getFormSubmissons(Convert.ToInt64(latestForm["id"]));

            Console.Out.WriteLine(submissions);

            Console.ReadLine();
        }
    }
}
```

Get latest 100 submissions ordered by creation date

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class PrintLastSubmissions
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API KEY");

            var submissions = client.getSubmissions(0, 100, null, "created_at");

            Console.Out.WriteLine(submissions);

            Console.ReadLine();
        }
    }
}
```

Submission and form filter examples

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class Filters
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API KEY");

            Dictionary<String, String> submissionFilter = new Dictionary<string, string>();
            submissionFilter.Add("id:gt", "FORM ID");
            submissionFilter.Add("created_at:gt", "DATE");

            var submissions = client.getSubmissions(0, 0, submissionFilter, "");
            Console.Out.WriteLine(submissions);

            Dictionary<String, String> formFilter = new Dictionary<string, string>();
            formFilter.Add("id:gt", "FORM ID");

            var forms = client.getForms(0, 0, formFilter, "");
            Console.Out.WriteLine(forms);

            Console.ReadLine();
        }
    }
}
```

Delete last 50 submissions

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JotForm;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JotFormTest
{
    class DeleteSubmissions
    {
        static void Main(string[] args)
        {
            var client = new JotForm.APIClient("YOUR API KEY");

            var submissions = client.getSubmissions(0, 50, null, "")["content"];

            foreach (var submission in submissions)
            {
                var result = client.deleteSubmission(Convert.ToInt64(submission["id"]));
                Console.Out.WriteLine(result);
            }

            Console.ReadLine();
        }
    }
}
```

First the _APIClient_ class is included from the _jotform-api-csharp/APIClient.cs_ file. This class provides access to JotForm's API. You have to create an API client instance with your API key. 
In any case of exception (wrong authentication etc.), you can catch it or let it fail with fatal error.


    

