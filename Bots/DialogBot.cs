// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio EchoBot v4.9.2

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using PluralsightBot.Helpers;
using PluralsightBot.Services;

namespace PluralsightBot.Bots
{
    public class DialogBot<T> : ActivityHandler where T : Dialog
    {
        public readonly Dialog _dialog;
        public readonly StateService _stateService;
        public readonly ILogger _logger;

        public DialogBot(StateService stateService, T dialog, ILogger<DialogBot<T>> logger)
        {
            this._stateService = stateService ?? throw new ArgumentNullException(nameof(stateService)); ;
            this._dialog = dialog ?? throw new ArgumentNullException(nameof(dialog)); ;
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger)); ;
        }


        public override async Task OnTurnAsync(ITurnContext turnContext, CancellationToken cancellationToken = default(CancellationToken))
        {
            await base.OnTurnAsync(turnContext, cancellationToken);

            await _stateService.UserState.SaveChangesAsync(turnContext, false, cancellationToken);
            await _stateService.ConversationState.SaveChangesAsync(turnContext, false, cancellationToken);
        }

        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Running dialog with Message Activity");
            await _dialog.Run(turnContext, _stateService.DialogStateAccessor, cancellationToken);
        }
    }
}
