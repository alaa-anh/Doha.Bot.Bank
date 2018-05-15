using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class Sharepoint
    {
        public static string checkAuthorizedUser(string name, string upassword)
        {
            string _userNameAdmin = ConfigurationManager.AppSettings["DomainAdmin"];
            string _userPasswordAdmin = ConfigurationManager.AppSettings["DomainAdminPassword"];
            string UserLoggedInName = string.Empty;
            try
            {
                using (ClientContext ctx = new ClientContext(ConfigurationManager.AppSettings["ServerURL"]))
                {

                    SecureString passWord = new SecureString();
                    foreach (char c in _userPasswordAdmin) passWord.AppendChar(c);
                    ctx.Credentials = new SharePointOnlineCredentials(_userNameAdmin, passWord);


                    var user = ctx.Web.EnsureUser(name);
                    ctx.Load(user);
                    ctx.ExecuteQuery();

                    if (user != null)
                    {
                        UserLoggedInName = user.Title;
                    }
                    //else
                    //    Authorized = false;
                }
            }
            catch (Exception ex)
            {
                //UserLoggedInName = ex.Message;
                UserLoggedInName = string.Empty;
                //Authorized = false;
            }

            //return Authorized;
            return UserLoggedInName;
        }
    }
}
