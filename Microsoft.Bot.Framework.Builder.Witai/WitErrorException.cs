using System;

namespace Microsoft.Bot.Framework.Builder.Witai.Dialogs
{
    [Serializable]
    internal class WitErrorException : Exception
    {
        public WitErrorException()
        {
        }

        public WitErrorException(string message) : base(message)
        {
        }

    }
}