using System;

namespace Microsoft.Bot.Framework.Builder.Exceptions
{
    /// <summary>
    /// Exception type thrown when an ActionHandler is not found
    /// </summary>
    [Serializable]
    internal class ActionHandlerNotFoundException : Exception
    {
        public ActionHandlerNotFoundException(string message) : base(message)
        {
        }
    }
}
