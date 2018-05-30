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


        //public static string checkAuthorizedUser(string name, string upassword)
        //{

        //    var connection = CrmConnection.Parse("Authentication Type=Passport; Server=https://port.crm4.dynamics.com; Username=poc@kbschatbot.onmicrosoft.com; Password=Demo1234");// ; DeviceID=xxx-ws00001; DevicePassword=xxxx");
        //    var service = new OrganizationService(connection);
        //    var context = new CrmOrganizationServiceContext(connection);
        //    IOrganizationService _service = (IOrganizationService)service;


           
        //    return UserLoggedInName;
        //}

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

        public static void SaveNewAnswer(string selectedFlowType , string InputTit, string Desc , bool Usertype , string SubmittedBy , string filename)
        {
            try
            {
                var connection = CrmConnection.Parse("Url=https://kbschatbot.crm4.dynamics.com; Username=poc@kbschatbot.onmicrosoft.com; Password=Demo1234;");// ; DeviceID=xxx-ws00001; DevicePassword=xxxx");
                OrganizationService _Service = new OrganizationService(connection);

                Entity account = new Entity("account");
                account["name"] = "BotName";
                Guid _AccountId = _Service.Create(account);

                //Create new Case  for the above account
                Entity case1 = new Entity("incident");
                case1["title"] = InputTit;
                case1["description"] = Desc;
                case1["caseorigincode"] = new OptionSetValue(100000001);// "Chatbot";               
                case1["new_casetype"] = new OptionSetValue(100000000); //selectedFlowType;
                case1["new_submittedby"] = SubmittedBy;
                case1["new_anonymous"] = Usertype;
                if(Usertype == true)
                    case1["new_an"] = new OptionSetValue(100000001); 
                else
                    case1["new_an"] = new OptionSetValue(100000000); 


                EntityReference primaryContactId = new EntityReference("account" ,_AccountId);
                case1["customerid"] = primaryContactId;
                Guid _CaseId = _Service.Create(case1);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private static object EntityReference(object entityLogicalName, Guid accountId)
        {
            throw new NotImplementedException();
        }
    }
}
