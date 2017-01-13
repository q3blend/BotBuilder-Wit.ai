using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Framework.Builder.Witai.Dialogs;
using Microsoft.Bot.Framework.Builder.Witai.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.Bot.Framework.Builder.Witai.Tests
{
    public sealed class InvalidWitDialog : WitDialog<object>
    {
        public InvalidWitDialog(IWitService service)
            : base(service)
        {
        }

        [WitAction("HasAttributeButDoesNotMatchReturnType")]
        public void HasAttributeButDoesNotMatchReturnType(IDialogContext context, WitResult luisResult)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public sealed class TestWitDialog : WitDialog<object>
    {
        public TestWitDialog(IWitService service) : base(service)
        {

        }

        [LuisIntent("ActionOne")]
        public async Task ActionOne(IDialogContext context, WitResult witResult)
        {
            await context.PostAsync(witResult.Action);
        }

        [LuisIntent("ActionTwo")]
        public async Task ActionTwo(IDialogContext context, WitResult witResult)
        {
            await context.PostAsync(witResult.Action);
        }

        public async Task MessageReceived(IDialogContext context, IAwaitable<IMessageActivity> item)
        {
            await base.MessageReceived(context, item);
        }

    }

    [TestClass]
    public class WitDialogTests
    {
        [TestMethod]
        [ExpectedException(typeof(InvalidActionHandlerException))]
        public void Invalid_Action_Throws_Error()
        {
            //Arrange
            var service = new Mock<IWitService>();
            var dialog = new InvalidWitDialog(service.Object);

            //Act
            var handlers = WitDialog.EnumerateHandlers(dialog).ToArray();
        }

        [TestMethod]
        public void UrlEncoding_UTF8_Then_Hex()
        {
            //Arrange
            var service = new WitService(new WitModelAttribute("token", WitApiVersion.Standard));
            
            //Act
            var request = service.BuildRequest(new WitRequest("Français", "session"));

            // https://github.com/Microsoft/BotBuilder/issues/247
            //Assert
            Assert.AreNotEqual("https://api.wit.ai/converse?session_id=session&q=Fran%25u00e7ais", request.RequestUri.AbsoluteUri);
            Assert.AreEqual("https://api.wit.ai/converse?session_id=session&q=Fran%C3%A7ais", request.RequestUri.AbsoluteUri);
        }

        [TestMethod]
        public async Task Should_Execute_ActionOne_Then_Post_Message()
        {
            //Arrange
            var service = new Mock<IWitService>();
            var counter = 0;
            service.Setup(wit => wit.QueryAsync(It.IsAny<IWitRequest>(), It.IsAny<CancellationToken>())).ReturnsAsync(() =>
            {
                switch (counter)
                {
                    case 0:
                        counter++;
                        return new WitResult()
                        {
                            Type = "action",
                            Action = "ActionOne"
                        };
                    case 1:
                        counter++;
                        return new WitResult()
                        {
                            Type = "msg",
                            Message = "test"
                        };
                    case 2:
                        counter++;
                        return new WitResult()
                        {
                            Type = "stop",
                        };
                }

                Assert.Fail("The dialog did not stop after the stop response from wit");
                return default(WitResult);
            });
            var dialog = new TestWitDialog(service.Object);
            var message = MakeTestMessage();
            message.Text = "execute action one, then respond with message \"test\", then stop";
            var item = new AwaitableFromItem<IMessageActivity>(message);
            var context = new Mock<IDialogContext>(MockBehavior.Loose);
            context.Setup(c => c.MakeMessage()).Returns(() => new Activity());
            
            //Act
            await dialog.MessageReceived(context.Object, item);

            //Assert
            context.Verify(c => c.PostAsync(It.Is<IMessageActivity>(a => a.Text == "ActionOne"), It.IsAny<CancellationToken>()), Times.Once);
            context.Verify(c => c.PostAsync(It.Is<IMessageActivity>(a => a.Text == "test"), It.IsAny<CancellationToken>()), Times.Once);
            context.Verify(c => c.PostAsync(It.Is<IMessageActivity>(a => a.Text == "ActionTwo"), It.IsAny<CancellationToken>()), Times.Never);
        }

        public static IMessageActivity MakeTestMessage()
        {
            return new Activity()
            {
                Type = ActivityTypes.Message,
                From = new ChannelAccount { Id = ChannelID.User },
                Conversation = new ConversationAccount { Id = Guid.NewGuid().ToString() },
                Recipient = new ChannelAccount { Id = ChannelID.Bot },
                ServiceUrl = "InvalidServiceUrl",
                ChannelId = "Test",
            };
        }

        public static class ChannelID
        {
            public const string User = "testUser";
            public const string Bot = "testBot";
        }
    }
}