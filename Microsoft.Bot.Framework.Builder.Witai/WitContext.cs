using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Microsoft.Bot.Framework.Builder
{
    /// <summary>
    /// Represents a thread-safe object of key/value pairs that can be accessed by
    /// multiple threads concurrently. The keys as case insensitive.
    /// </summary>
    [Serializable]
    public class WitContext : IWitContext
    {
        
        private ConcurrentDictionary<string, object> dictionary;
        
        /// <summary>
        /// Gets or sets the value associated with the specified key.
        /// </summary>
        /// <param name="key">The key of the value to get or set.</param>
        /// <returns>The value of the key/value pair at the specified index.</returns>
        public object this[string key]
        {
            get
            {
                object value;

                if(this.TryGetValue(key, out value))
                {
                    return value;
                }
                else
                {
                    throw new KeyNotFoundException(key);
                }
            }
            set
            {
                this.AddOrUpdate(key, value);
            }
        }

        /// <summary>
        /// Initializes a new instance of the WitContext class that is empty,
        /// has the default concurrency level, has the default initial capacity
        /// </summary>
        public WitContext()
        {
            this.dictionary = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// Uses the specified functions to add a key/value pair to the WitContext
        /// if the key does not already exist, or to update a key/value pair in 
        /// the WitContext if the key already exists.
        /// </summary>
        /// <param name="key">The key to be added or whose value should be updated</param>
        /// <param name="value">The new value for the key. This will override the current value if one exists</param>
        public void AddOrUpdate(string key, object value)
        {
            dictionary[key.ToLower()] = value;
        }

        /// <summary>
        /// Removes all keys and values from the WitContext 
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();
        }

        /// <summary>
        /// Attempts to get the value associated with the specified key from the WitContext
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <param name="value">When this method returns, contains the object from the WitContext</param>
        /// <returns>true if the key was found in the WitContext; otherwise, false</returns>
        public bool TryGetValue(string key, out object value)
        {
            return dictionary.TryGetValue(key.ToLower(), out value);
        }

        /// <summary>
        /// Converts this object into a Json string
        /// </summary>
        /// <returns>the json string representation of this object</returns>
        public string ToJsonString()
        {
            return new JavaScriptSerializer().Serialize(this.dictionary); ;
        }

        /// <summary>
        /// Removes the value associated with the specified key from the WitContext 
        /// </summary>
        /// <param name="key">The key of the value to get.</param>
        /// <returns>true if the object was removed; otherwise, false.</returns>
        public bool RemoveIfExists(string key)
        {
            object value;
            return this.dictionary.TryRemove(key.ToLower(), out value);
        }
    }
}
