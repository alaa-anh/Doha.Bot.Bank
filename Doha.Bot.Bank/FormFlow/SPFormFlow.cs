using Microsoft.Bot.Builder.FormFlow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Doha.Bot.Bank.FormFlow
{
    public class SPFormFlow
    {
            //public MovieTheatreLocation movieTheatreLocation;
            //public MovieTheatre movieTheatre;
            //public MovieTypes movieTypes;
            //public ClassTypes classTypes;
            //[Optional]
            //public DoYouNeedAMeal doYouNeedAMeal;
            //public FoodMenu foodMenu;
            //public DateTime? Date;
            //[Numeric(1, 5)]
            //public int? NumberOfAdult;

            //public int? NumberOfChild;

            public static IForm<SPFormFlow> BuildForm()
            {
                return new FormBuilder<SPFormFlow>()
                    .Message("Welcome to the Movie Booking BOT created by Neel.")
                     //.OnCompletion(async (context, profileForm) =>
                     //{
                     //    var userName = string.Empty;
                     //    context.UserData.TryGetValue<string>("Name", out userName);
                     //// Tell the user that the form is complete  
                     //await context.PostAsync("Thanks for the confirmation " + userName + ". Your booking is successfull");
                     //})
                    .Build();
            }
    }
}