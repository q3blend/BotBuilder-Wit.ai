using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Scorables.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wit.Bot.Framework.Builder.Models;
using System.Linq;
using System.Runtime.Serialization;
using System.Web.Script.Serialization;

namespace Wit.Bot.Framework.Builder.Dialogs
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    [Serializable]
    public class WitActionAttribute : AttributeString
    {
        public readonly string ActionName;

        public WitActionAttribute(string actionName)
        {
            SetField.NotNull(out this.ActionName, nameof(actionName), actionName);
        }

        protected override string Text
        {
            get
            {
                return this.ActionName;
            }
        }
    }

    public delegate Task ActionHandler(IDialogContext context, WitResult witResult);

    public delegate Task ActionActivityHandler(IDialogContext context, IAwaitable<IMessageActivity> message, WitResult witResult);

    [Serializable]
    public class WitDialog<TResult> : IDialog<TResult>
    {
        protected readonly IWitService service;
       
        [NonSerialized]
        protected Dictionary<string, ActionActivityHandler> handlerByAction;
        public IDictionary<string, object> WitContext;
        public string WitSessionId;

        public IWitService MakeServiceFromAttributes()
        {
            var type = this.GetType();
            var witModels = type.GetCustomAttributes<WitModelAttribute>(inherit: true);
            if (witModels.ToArray().Length > 1)
            {
                throw new WitModelDisambiguationException("WitDialog does not support more than one WitModel per instance");
            }

            return new WitService(witModels.ToArray()[0]);
        }

        public WitDialog()
        {
            service = MakeServiceFromAttributes();
            SetField.NotNull(out this.service, nameof(service), service);
        }

        public virtual async Task StartAsync(IDialogContext context)
        {
            
            context.Wait(MessageReceived);
        }
        
        protected virtual async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            if (!context.PrivateConversationData.ContainsKey("witSessionId"))
            {
                await this.ResetConversation(context);
            }

            this.WitContext = context.PrivateConversationData.Get<IDictionary<string, object>>("witContext");
            this.WitSessionId = context.PrivateConversationData.Get<string>("witSessionId");
            await MessageHandler(context, item);
        }

        private async Task MessageHandler(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            context.PrivateConversationData.SetValue("witContext", this.WitContext);
            context.PrivateConversationData.SetValue("witSessionId", this.WitSessionId);
            var message = await item;
            var messageText = await GetWitQueryTextAsync(context, message);
            
            string jsonContext = new JavaScriptSerializer().Serialize(this.WitContext);
            var result = await this.service.QueryAsync(messageText, this.WitSessionId, jsonContext, context.CancellationToken);

            switch (result.Type)
            {
                case "action":
                    await DispatchToActionHandler(context, item, result);
                    await MessageHandler(context, item);
                    break;
                case "msg":
                    await context.PostAsync(result.message);
                    await MessageHandler(context, item);
                    break;
                case "stop":
                    await this.ResetConversation(context);
                    break;
                default:
                    throw new UnsupportedWitActionException($"Action {result.Type} is not supported");
            }
        }

        private Task ResetConversation(IDialogContext context)
        {
            context.PrivateConversationData.SetValue<IDictionary<string, object>>("witContext", new Dictionary<string, object>());
            context.PrivateConversationData.SetValue<string>("witSessionId", Guid.NewGuid().ToString());
            return Task.CompletedTask;
        }

        protected virtual async Task DispatchToActionHandler(IDialogContext context, IAwaitable<IMessageActivity> item, WitResult result)
        {
            if (this.handlerByAction == null)
            {
                this.handlerByAction = new Dictionary<string, ActionActivityHandler>(GetHandlersByAction());
            }

            ActionActivityHandler handler = null;
            if (string.IsNullOrEmpty(result.Action) || !this.handlerByAction.TryGetValue(result.Action, out handler))
            {
                handler = this.handlerByAction[string.Empty];
            }

            if (handler != null)
            {
                await handler(context, item, result);
            }
            else
            {
                throw new Exception("No default action handler found.");
            }
        }

        protected virtual IDictionary<string, ActionActivityHandler> GetHandlersByAction()
        {
            return WitDialog.EnumerateHandlers(this).ToDictionary(kv => kv.Key, kv => kv.Value);
        }

        protected virtual Task<string> GetWitQueryTextAsync(IDialogContext context, IMessageActivity message)
        {
            return Task.FromResult(message.Text);
        }
    }

    internal static class WitDialog
    {
        public static IEnumerable<KeyValuePair<string, ActionActivityHandler>> EnumerateHandlers(object dialog)
        {
            var type = dialog.GetType();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            foreach (var method in methods)
            {
                var actions = method.GetCustomAttributes<WitActionAttribute>(inherit: true).ToArray();
                ActionActivityHandler actionHandler = null;

                try
                {
                    actionHandler = (ActionActivityHandler)Delegate.CreateDelegate(typeof(ActionActivityHandler), dialog, method, throwOnBindFailure: false);
                }
                catch (ArgumentException)
                {
                    // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                    // https://github.com/Microsoft/BotBuilder/issues/634
                    // https://github.com/Microsoft/BotBuilder/issues/435
                }

                // fall back for compatibility
                if (actionHandler == null)
                {
                    try
                    {
                        var handler = (ActionHandler)Delegate.CreateDelegate(typeof(ActionHandler), dialog, method, throwOnBindFailure: false);

                        if (handler != null)
                        {
                            // thunk from new to old delegate type
                            actionHandler = (context, message, result) => handler(context, result);
                        }
                    }
                    catch (ArgumentException)
                    {
                        // "Cannot bind to the target method because its signature or security transparency is not compatible with that of the delegate type."
                        // https://github.com/Microsoft/BotBuilder/issues/634
                        // https://github.com/Microsoft/BotBuilder/issues/435
                    }
                }

                if (actionHandler != null)
                {
                    var actionNames = actions.Select(i => i.ActionName).DefaultIfEmpty(method.Name);

                    foreach (var actionName in actionNames)
                    {
                        var key = string.IsNullOrWhiteSpace(actionName) ? string.Empty : actionName;
                        yield return new KeyValuePair<string, ActionActivityHandler>(actionName, actionHandler);
                    }
                }
                else
                {
                    if (actions.Length > 0)
                    {
                        throw new InvalidIntentHandlerException(string.Join(";", actions.Select(i => i.ActionName)), method);
                    }
                }
            }
        }
    }

    [Serializable]
    internal class UnsupportedWitActionException : Exception
    {
        public UnsupportedWitActionException()
        {
        }

        public UnsupportedWitActionException(string message) : base(message)
        {
        }

        public UnsupportedWitActionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected UnsupportedWitActionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }

    [Serializable]
    internal class WitModelDisambiguationException : Exception
    {
        public WitModelDisambiguationException()
        {
        }

        public WitModelDisambiguationException(string message) : base(message)
        {
        }

        public WitModelDisambiguationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected WitModelDisambiguationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
