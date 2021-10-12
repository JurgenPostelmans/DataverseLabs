using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ContactsBrowser
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //URI of your Dynamics 365 Instance
        private string resource = "https://u2u365.crm4.dynamics.com";
        //Application ID of your app as registered in Azure AD.
        private string clientId = "efea9fc6-77a4-4fe8-b842-f637d0daf590";
        //Must match with what is in Azure AD!
        private string redirectUri = "https://login.live.com/oauth20_desktop.srf";
        //This is the address of the Azure AD service that will return the access token 
        private string AADAuthorityUri = "https://login.windows.net/common/oauth2/authorize";

        private AuthenticationContext ac;
        private HttpClient httpClient;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AuthenticateAndSetup()
        {
            ac = new AuthenticationContext(AADAuthorityUri);
            AuthenticationResult ar = ac.AcquireTokenAsync(resource, clientId, new Uri(redirectUri), new PlatformParameters(PromptBehavior.Auto)).Result;

            ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;

            httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("OData-MaxVersion", "4.0");
            httpClient.DefaultRequestHeaders.Add("OData-Version", "4.0");
            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            httpClient.BaseAddress = new Uri("https://u2u365.crm4.dynamics.com/api/data/v9.0/");

            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AuthenticateAndSetup();

            HttpRequestMessage retrieveRequest = new HttpRequestMessage(HttpMethod.Get, "contacts?$select=fullname,contactid&$orderby=fullname&$filter=statecode eq 0");
            HttpResponseMessage retrieveResponse =  httpClient.SendAsync(retrieveRequest).Result;

            string json = retrieveResponse.Content.ReadAsStringAsync().Result;

            JObject body = JsonConvert.DeserializeObject<JObject>(json);

            JArray collectionOfContacts = (JArray)body["value"];

            ObservableCollection<contact> contacts;
            contacts = collectionOfContacts.ToObject<ObservableCollection<contact>>();

            listboxContacts.ItemsSource = contacts;
            listboxContacts.DisplayMemberPath = "fullname";

        }

        private void listboxContacts_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            contact selectedContact = listboxContacts.SelectedItem as contact;
            if (selectedContact != null)
            {
                string queryUri = String.Format(@"contacts({0})?$select=fullname,firstname, lastname, 
            creditlimit,_parentcustomerid_value,numberofchildren", selectedContact.contactid);

                HttpRequestMessage retrieveRequest = new HttpRequestMessage(HttpMethod.Get, queryUri);
                retrieveRequest.Headers.Add("Prefer", "odata.include-annotations=\"*\"");

                HttpResponseMessage retrieveResponse = httpClient.SendAsync(retrieveRequest).Result;
                string json = retrieveResponse.Content.ReadAsStringAsync().Result;

                contact c = JsonConvert.DeserializeObject<contact>(json);

                textboxFirstName.Text = c.firstname;
                textboxLastName.Text = c.lastname;
                textboxNumberOfChildren.Text = c.numberofchildren.ToString();
            }

        }

        private void buttonUpdate_Click(object sender, RoutedEventArgs e)
        {
            contact selectedContact = listboxContacts.SelectedItem as contact;
            if (selectedContact != null)
            {
                contact updateContact = new contact();

                updateContact.firstname = textboxFirstName.Text;
                updateContact.lastname = textboxLastName.Text;
                string noc = textboxNumberOfChildren.Text;

                if (String.IsNullOrEmpty(noc))
                    updateContact.numberofchildren = null;
                else
                {
                    int nocInt;
                    if (Int32.TryParse(noc, out nocInt))
                        updateContact.numberofchildren = nocInt;
                    else
                        updateContact.numberofchildren = null;
                }

                string queryUri = String.Format("contacts({0})", selectedContact.contactid);
                HttpRequestMessage updateRequest = new HttpRequestMessage(new HttpMethod("PATCH"), queryUri);
                var settings = new JsonSerializerSettings();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.DefaultValueHandling = DefaultValueHandling.Ignore;
                string updateContactJson = JsonConvert.SerializeObject(updateContact, settings);

                updateRequest.Content = new StringContent(updateContactJson, Encoding.UTF8, "application/json");

                HttpResponseMessage updateResponse = httpClient.SendAsync(updateRequest).Result;

                if (updateResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    MessageBox.Show("Contact updated.");
                }
                else
                {
                    MessageBox.Show("Error updating contact.");
                }

            }

        }

        private void buttonDelete_Click(object sender, RoutedEventArgs e)
        {
            contact selectedContact = listboxContacts.SelectedItem as contact;
            if (selectedContact != null)
            {
                string queryUri = String.Format("contacts({0})", selectedContact.contactid);

                HttpRequestMessage deleteRequest = new HttpRequestMessage(HttpMethod.Delete, queryUri);

                HttpResponseMessage deleteResponse = httpClient.SendAsync(deleteRequest).Result;

                if (deleteResponse.StatusCode == HttpStatusCode.NoContent)
                {
                    ObservableCollection<contact> contacts = (ObservableCollection<contact>)listboxContacts.ItemsSource;
                    contacts.Remove(selectedContact);
                    MessageBox.Show("Contact was deleted");
                }
                else if (deleteResponse.StatusCode == HttpStatusCode.NotFound)
                {
                    MessageBox.Show("Contact was not found.");
                }
            }

        }

        private void buttonCreate_Click(object sender, RoutedEventArgs e)
        {
            contact newContact = new contact();

            newContact.firstname = textboxFirstName.Text;
            newContact.lastname = textboxLastName.Text;
            string noc = textboxNumberOfChildren.Text;

            if (String.IsNullOrEmpty(noc))
                newContact.numberofchildren = null;
            else
            {
                int nocInt;
                if (Int32.TryParse(noc, out nocInt))
                    newContact.numberofchildren = nocInt;
                else
                    newContact.numberofchildren = null;
            }
            newContact.creditlimit = (decimal)12345.6;

            var settings = new JsonSerializerSettings();
            settings.NullValueHandling = NullValueHandling.Ignore;
            settings.DefaultValueHandling = DefaultValueHandling.Ignore;
            string newContactJson = JsonConvert.SerializeObject(newContact, settings);

            HttpRequestMessage createRequest = new HttpRequestMessage(HttpMethod.Post, "contacts");
            createRequest.Content = new StringContent(newContactJson, Encoding.UTF8, "application/json");

            HttpResponseMessage createResponse = httpClient.SendAsync(createRequest).Result;

            if (createResponse.StatusCode == HttpStatusCode.NoContent)
            {
                string uri = createResponse.Headers.GetValues("OData-EntityId").FirstOrDefault();
                string msg = String.Format("Contact was created with uri {0}", uri);
                MessageBox.Show(msg);

                uri = uri + "?$select=fullname,firstname, lastname, creditlimit,_parentcustomerid_value,numberofchildren";
                HttpRequestMessage retrieveRequest = new HttpRequestMessage(HttpMethod.Get, uri);
                HttpResponseMessage retrieveResponse = httpClient.SendAsync(retrieveRequest).Result;

                string json = retrieveResponse.Content.ReadAsStringAsync().Result;

                newContact = JsonConvert.DeserializeObject<contact>(json);
                ObservableCollection<contact> contacts = (ObservableCollection<contact>)listboxContacts.ItemsSource;
                contacts.Add(newContact);

            }
            else
            {
                MessageBox.Show("Error creating contact");
            }

        }
    }

    public class contact
    {
        public Guid contactid { get; set; }
        public string fullname { get; set; }
        public string lastname { get; set; }
        public string firstname { get; set; }
        public decimal? creditlimit { get; set; }
        public int? numberofchildren { get; set; }
    }

}
