﻿using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;

namespace Doha.Bot.Bank
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            Conversation.UpdateContainer(
builder => 
    {
                builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));
                var store = new TableBotDataStore(ConfigurationManager.ConnectionStrings["StorageConnectionString"].ConnectionString);
                
builder.Register(c => store)
          .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
          .AsSelf()
          .SingleInstance();
                    });


            GlobalConfiguration.Configure(WebApiConfig.Register);



        }
    }

        //protected void Application_Start()
        //{
        //    GlobalConfiguration.Configure(WebApiConfig.Register);
        //}
    }
