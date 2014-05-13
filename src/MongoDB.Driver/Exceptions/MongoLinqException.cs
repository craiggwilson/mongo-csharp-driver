using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace MongoDB.Driver
{
    /// <summary>
    /// Exception occured during a linq query.
    /// </summary>
    [Serializable]
    public class MongoLinqException : MongoException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoLinqException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        public MongoLinqException(string message) 
            : base(message) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoLinqException" /> class.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="inner">The inner.</param>
        public MongoLinqException(string message, Exception inner) 
            : base(message, inner) 
        { 
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoLinqException" /> class.
        /// </summary>
        /// <param name="info">The info.</param>
        /// <param name="context">The context.</param>
        protected MongoLinqException(SerializationInfo info, StreamingContext context)
            : base(info, context) 
        { 
        }
    }
}
