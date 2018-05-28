Add the following Namespaces to use this into your application.

System.ServiceModel
System.DirectoryServices.AccountManagement
System.Security
System.Runtime.Serialization
CRMConnect

SDK files - 5.0.0.0 version.
microsoft.crm.sdk.proxy
microsoft.xrm.sdk

Have made some customization in ServerConnection.cs.

How to use it.


class Program
    {
        static void Main(string[] args)
        {
            Microsoft.Xrm.Sdk.IOrganizationService _service = null;
            Microsoft.Xrm.Sdk.Client.OrganizationServiceProxy _serviceProxy;
            ServerConnection serverConnect = new ServerConnection();
            
			// Important Statement
			// Final parameter of this method is your CRM version. like crm4, crm5 etc. If you pass Blank value it will use crm default.

			ServerConnection.Configuration config = serverConnect.GetServerConfiguration("yourusername", "yourpassword","crm");
            _serviceProxy = ServerConnection.GetOrganizationProxy(config);

            // This statement is required to enable early-bound type support.
            _serviceProxy.EnableProxyTypes();

            _service = (Microsoft.Xrm.Sdk.IOrganizationService)_serviceProxy;
        }
    }

Use the _Service object as your own way.




