﻿// Copyright 2012-2015 Dmitry Kischenko
//
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either 
// express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
using System;
#if !PORTABLE
using System.Runtime.Serialization;
#endif

namespace xFunc.Logics
{

    /// <summary>
    /// The exception that is thrown in <see cref="LogicLexer"/>.
    /// </summary>
#if !PORTABLE
    [Serializable]
#endif
    public class LogicLexerException : Exception
    {

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicLexerException"/> class.
        /// </summary>
        public LogicLexerException() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicLexerException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">A <see cref="String"/> that describes the error.</param>
        public LogicLexerException(string message) : base(message) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicLexerException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">A <see cref="String"/> that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception.</param>
        public LogicLexerException(string message, Exception inner) : base(message, inner) { }

#if !PORTABLE
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicLexerException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The object that holds the serialized object data.</param>
        /// <param name="context">The contextual information about the source or destination.</param>
        protected LogicLexerException(SerializationInfo info, StreamingContext context) : base(info, context) { }
#endif

    }

}