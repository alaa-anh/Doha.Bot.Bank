using Microsoft.SharePoint.Client;
using System;
using System.Configuration;
using System.Security;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;

namespace Common
{
    public class CRM
    {
        private static string _userName;
        private static string _userPassword;
        private static string UserLoggedInName;

        
        private static string _serverURL = ConfigurationManager.AppSettings["ServerURL"];

        private static string _userNameAdmin = ConfigurationManager.AppSettings["DomainAdmin"];
        private static string _userPasswordAdmin = ConfigurationManager.AppSettings["DomainAdminPassword"];


        public static string checkAuthorizedUser(string name, string upassword)
        {

            var connection = CrmConnection.Parse("Authentication Type=Passport; Server=https://port.crm4.dynamics.com; Username=poc@kbschatbot.onmicrosoft.com; Password=Demo1234");// ; DeviceID=xxx-ws00001; DevicePassword=xxxx");
            var service = new OrganizationService(connection);
            var context = new CrmOrganizationServiceContext(connection);
            IOrganizationService _service = (IOrganizationService)service;


           
            return UserLoggedInName;
        }

        public static ListItemCollection GetSelectedTypeQuestions( string selectedFlowType)
        {

            using (ClientContext ctx = new ClientContext(_serverURL))
            {
                SecureString passWord = new SecureString();
                foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                List oList = ctx.Web.Lists.GetByTitle("Question Flow");
                CamlQuery camlQuery = new CamlQuery();
                camlQuery.ViewXml = @"<Query>
                                   <Where>
                                      <Eq>
                                         <FieldRef Name = 'Flow_x0020_Type'/> 
                                          <Value Type = 'Choice'> "+ selectedFlowType + @" </Value>  
                                        </Eq>  
                                     </Where>
                                  </Query>";
                ListItemCollection collListItem = oList.GetItems(camlQuery);
                ctx.Load(collListItem);

                ctx.ExecuteQuery();

                return collListItem;
            }

        }

        //public static ListItem GetQuestion(int QuestionID)
        //{

        //    using (ClientContext ctx = new ClientContext(_serverURL))
        //    {
        //        SecureString passWord = new SecureString();
        //        foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
        //        ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


        //        List oList = ctx.Web.Lists.GetByTitle("Question Flow");
               
        //       ListItem collListItem = oList.GetItemById(QuestionID);
        //        ctx.Load(collListItem);

        //        ctx.ExecuteQuery();

        //            return collListItem;
        //    }

        //}

        public static void SaveNewAnswer(string selectedFlowType , string NewTitle, string InputTit, string Desc , bool Usertype , string SubmittedBy , string filename)
        {
            try
            {
                //BEGIN CONNECTION STUFF
                var connection = CrmConnection.Parse("Authentication Type=Passport; Server=https://port.crm4.dynamics.com; Username=poc@kbschatbot.onmicrosoft.com; Password=Demo1234");// ; DeviceID=xxx-ws00001; DevicePassword=xxxx");
                var service = new OrganizationService(connection);
                var context = new CrmOrganizationServiceContext(connection);
                IOrganizationService _service = (IOrganizationService)service;


                //BEGIN LATE BOUND CREATION SAMPLE
                Entity incident = new Entity();
                incident.LogicalName = "incident";
                incident["title"] = "Test Case Creation";
                incident["description"] = "This is a test incident";

                //Set customerid with an existing contact guid 
                Guid customerid = new Guid("9BA22E13-1149-E211-8BE3-78E3B5107E67");     //the actual contact GUID.

                //Set customerid as contact to field customerid 
                EntityReference CustomerId = new EntityReference("contact", customerid);
                incident["customerid"] = CustomerId;

                //create the incident
                _service.Create(incident);


                //BEGIN EARLY BOUND CREATION SAMPLE

                //create a contact and assign the id to the Id above
                //Contact newContact = new Contact();
                //newContact.Id = customerid;

                //Incident newIncident = new Incident();
                //newIncident.Title = "Test Created With Proxy";                    //set the title
                //newIncident.Description = "This is a test incident";               //set the description
                //newIncident.CustomerId = newContact.ToEntityReference(); //set the Customer Id to the Entity Reference of the Contact

                ////create the incident
                //_service.Create(newIncident);
            }
            catch (Exception e)
            {
                throw e;
            }
        }


      


        


    }
}
