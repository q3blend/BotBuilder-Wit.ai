using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Microsoft.Bot.Framework.Builder.Witai.Dialogs
{
    [Serializable]
    internal class InvalidActionHandlerException : Exception
    {
        private MethodInfo method;
        private string v;

        public InvalidActionHandlerException()
        {
        }

        public InvalidActionHandlerException(string message) : base(message)
        {
        }

        public InvalidActionHandlerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public InvalidActionHandlerException(string v, MethodInfo method)
        {
            this.v = v;
            this.method = method;
        }

        protected InvalidActionHandlerException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}