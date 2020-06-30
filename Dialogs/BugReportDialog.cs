using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using PluralsightBot.Models;
using PluralsightBot.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PluralsightBot.Dialogs
{
    public class BugReportDialog : ComponentDialog
    {
        private readonly StateService _stateService;

        public BugReportDialog(string dialogId, StateService stateService) : base(dialogId)
        {
            this._stateService = stateService ?? throw new NotImplementedException(nameof(stateService));

            InitializeWaterfallDialog();
        }

        private void InitializeWaterfallDialog()
        {
            var waterfallSteps = new WaterfallStep[]
            {
                DescriptionStepAsync, CallbackTimeStepAsync, PhoneNumberStepAsync, BugStepAsync, SummaryStepAsync
            };

            AddDialog(new WaterfallDialog($"{nameof(BugReportDialog)}.mainFlow", waterfallSteps));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.description"));
            AddDialog(new DateTimePrompt($"{nameof(BugReportDialog)}.callbackTime", CallbackTimeValidatorAsync));
            AddDialog(new TextPrompt($"{nameof(BugReportDialog)}.phoneNumber", PhoneNumberValidatorAsync));
            AddDialog(new ChoicePrompt($"{nameof(BugReportDialog)}.bug"));

            InitialDialogId = $"{nameof(BugReportDialog)}.mainFlow";
        }

        private async Task<DialogTurnResult> DescriptionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.description",
                    new PromptOptions { Prompt = MessageFactory.Text("Enter a description for your report.") },
                    cancellationToken);
        }
        private async Task<DialogTurnResult> CallbackTimeStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["description"] = $"{stepContext.Result}";
            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.callbackTime",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Please enter in a callback time"),
                            RetryPrompt = MessageFactory.Text("The value entered must be between the hours of 9 am and 5 pm.")
                        },
                        cancellationToken);
        }
        private async Task<DialogTurnResult> PhoneNumberStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["callbackTime"] = Convert.ToDateTime(((List<DateTimeResolution>)stepContext.Result).FirstOrDefault().Value);

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.phoneNumber",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Please enter the phone number that we can call you back at."),
                            RetryPrompt = MessageFactory.Text("Please enter a valid phone number.")
                        },
                        cancellationToken);
        }
        private async Task<DialogTurnResult> BugStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["phoneNumber"] = $"{stepContext.Result}";

            return await stepContext.PromptAsync($"{nameof(BugReportDialog)}.bug",
                        new PromptOptions
                        {
                            Prompt = MessageFactory.Text("Please the type of bug."),
                            Choices = ChoiceFactory.ToChoices(new List<string> { "Security", "Crash", "Power", "Performance", "Usability", "Serious Bug", "Other" })
                        },
                        cancellationToken);
        }
        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["bug"] = (stepContext.Result as FoundChoice).Value;
            UserProfile userProfile = await _stateService.UserProfileAccessor.GetAsync(stepContext.Context, () => new UserProfile());
            userProfile.Description = $"{stepContext.Values["description"]}";
            userProfile.CallbackTime = (DateTime)stepContext.Values["callbackTime"];
            userProfile.PhoneNumber = $"{stepContext.Values["phoneNumber"]}";
            userProfile.Bug = $"{stepContext.Values["bug"]}";

            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Here is a summury of your bug report :"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Description : {userProfile.Description}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Callback Time : {userProfile.CallbackTime.ToString()}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Phone Number : {userProfile.PhoneNumber}"), cancellationToken);
            await stepContext.Context.SendActivityAsync(MessageFactory.Text($"Bug : {userProfile.Bug}"), cancellationToken);

            await _stateService.UserProfileAccessor.SetAsync(stepContext.Context, userProfile);
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }

        private Task<bool> CallbackTimeValidatorAsync(PromptValidatorContext<IList<DateTimeResolution>> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            if (promptContext.Recognized.Succeeded)
            {
                var resolution = promptContext.Recognized.Value.First();
                DateTime selectedDate = Convert.ToDateTime(resolution.Value);
                TimeSpan start = new TimeSpan(9, 0, 0);
                TimeSpan end = new TimeSpan(17, 0, 0);
                valid = selectedDate.TimeOfDay >= start && selectedDate.TimeOfDay <= end;
            }

            return Task.FromResult(valid);
        }
        private Task<bool> PhoneNumberValidatorAsync(PromptValidatorContext<string> promptContext, CancellationToken cancellationToken)
        {
            var valid = false;
            if (promptContext.Recognized.Succeeded)
            {
                valid = Regex.Match(promptContext.Recognized.Value, @"^(\+\d{1,2}\s)?\(?\d{3}\)?[\s.-]?\d{3}[\s.-]?\d{4}$").Success;
            }

            return Task.FromResult(valid);
        }
    }
}
