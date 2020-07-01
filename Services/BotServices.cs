using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PluralsightBot.Services
{
    public class BotServices
    {
        public LuisRecognizer Dispatch { get; private set; }
        public BotServices(IConfiguration configuration)
        {
            string applicationId = configuration.GetValue<string>("luisAppId");
            string endpointKey = configuration.GetValue<string>("luisApiKey");
            string endpoint = $"https://{configuration.GetValue<string>("luisApiHostName")}.api.cognitive.microsoft.com";
            var luisApplication = new LuisApplication(applicationId, endpointKey, endpoint);
            var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            {
                PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                {
                    IncludeAllIntents = true,
                    IncludeInstanceData = true
                }
            };

            Dispatch = new LuisRecognizer(recognizerOptions);
        }
    }
}
