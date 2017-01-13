namespace Microsoft.Bot.Framework.Builder
{
    /// <summary>
    /// Defines methods to manipulate Wit context variables
    /// </summary>
    public interface IWitContext
    {
        /// <summary>
        /// Uses the specified functions to add a key/value pair to the WitContext
        /// if the key does not already exist, or to update a key/value pair in 
        /// the IWitContext if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The new value for the key. This will override the current value if one exists</param>
        void AddOrUpdate(string key, object value);

        /// <summary>
        /// Attempts to get the value associated with the specified key from the IWitContext
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the IWitContext</param>
        /// <returns>true if the key was found in the WitContext; otherwise, false</returns>
        bool TryGetValue(string key, out object value);

        /// <summary>
        /// Removes the value associated with the specified key from the IWitContext 
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>true if the object was removed; otherwise, false.</returns>
        bool RemoveIfExists(string key);

        /// <summary>
        /// Removes all keys and values from the IWitContext 
        /// </summary>
        void Clear();

        /// <summary>
        /// Converts this object into a Json string
        /// </summary>
        /// <returns>the json string representation of this object</returns>
        string ToJsonString();

        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        object this[string index] { get; set; }
    }
}
