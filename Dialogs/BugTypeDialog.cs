using Microsoft.Azure.Cosmos;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using PluralsightBot.Helpers;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    public class BugTypeDialog : ComponentDialog
    {
        private readonly StateService _stateService;
        private readonly BotServices _botServices;

        public BugTypeDialog(string dialogId, StateService stateService, BotServices botServices) : base(dialogId)
        {
            this._stateService = stateService ?? throw new NotImplementedException(nameof(stateService));
            this._botServices = botServices ?? throw new NotImplementedException(nameof(botServices));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                InitialStepAsync, FinalStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(BugTypeDialog)}.mainFlow", waterfallSteps));

            InitialDialogId = $"{nameof(BugTypeDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> InitialStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            var result = await _botServices.Dispatch.RecognizeAsync(stepContext.Context, cancellationToken);
            var token = result.Entities.FindTokens("BugType").First();
            Regex regex = new Regex("[^a-zA-Z0-9 -]");
            var value = regex.Replace(token.ToString(), "").Trim();

            if (Common.BugTypes.Any(s =>s.Equals(value, StringComparison.OrdinalIgnoreCase)))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Yes! {value} is a Bug Type."), cancellationToken);
            }
            else
            {
                 await stepContext.Context.SendActivityAsync(MessageFactory.Text($"No {value} is not a Bug Type."), cancellationToken);
            }
            
            return await stepContext.NextAsync(null, cancellationToken);
        }
        private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
               return await stepContext.EndDialogAsync(null, cancellationToken);
        }
    }
}
