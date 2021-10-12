using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ContactsImporter
{
    class Program
    {
        private static HttpClient httpClient;

        static void Main(string[] args)
        {
            ImportContacts();
            Console.ReadLine();
        }

        static void ImportContacts()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.UseDefaultCredentials = true;
            //If you want to login with alternate credentials
            //handler.Credentials = new NetworkCredential(userName, password, domainName);

            httpClient = new HttpClient(handler);
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.BaseAddress = new Uri("http://springfield:5555/AdventureWorksCyclesDEV/api/data/v8.2/");

            string filename = @"c:\newcontacts.txt";

            using (StreamReader sr = new StreamReader(filename))
            {
                //Skip the header row
                sr.ReadLine();

                while (sr.Peek() >= 0)
                {
                    string line = sr.ReadLine();
                    string[] values = line.Split(',');

                    JObject newContact = new JObject();
                    newContact.Add("firstname", values[0]);
                    newContact.Add("lastname", values[1]);
                    newContact.Add("numberofchildren", values[2]);
                    //The following code can be used for regular Date/Time fields
                    //but birthdate is a Date Only field so it must be YYYY-MM-DD
                    //DateTimeOffset birthDate = DateTimeOffset.Parse(values[3]);
                    DateTime birthDate = DateTime.Parse(values[3]);
                    newContact.Add("birthdate", birthDate.ToString("yyyy-MM-dd"));
                    decimal creditLimit = Convert.ToDecimal(values[4]);
                    newContact.Add("creditlimit", creditLimit);

                    HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, "contacts");
                    createRequest.Content = new StringContent(newContact.ToString(), Encoding.UTF8, "application/json");

                    HttpResponseMessage createResponse = httpClient.SendAsync(createRequest).Result;

                    string json = createResponse.Content.ReadAsStringAsync().Result;

                    if (createResponse.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Contact '{0} {1}' created.",
                            newContact.GetValue("firstname"),
                            newContact.GetValue("lastname"));

                        string contactUri = createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault();
                        Console.WriteLine("Contact URI: {0}", contactUri);

                    }
                    else
                    {
                        Console.WriteLine("Error creating contact...");
                        Console.WriteLine(createResponse.StatusCode);
                        Console.WriteLine(createResponse.ReasonPhrase);
                    }

                }
            }

        }

    }
}
