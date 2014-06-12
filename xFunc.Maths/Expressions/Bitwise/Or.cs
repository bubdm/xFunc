﻿// Copyright 2012-2014 Dmitry Kischenko
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

namespace xFunc.Maths.Expressions.Bitwise
{

    /// <summary>
    /// Represents a bitwise OR operation.
    /// </summary>
    public class Or : BinaryExpression
    {

        internal Or() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Or"/> class.
        /// </summary>
        /// <param name="firstMathExpression">The left operand.</param>
        /// <param name="secondMathExpression">The right operand.</param>
        /// <seealso cref="IExpression"/>
        public Or(IExpression firstMathExpression, IExpression secondMathExpression)
            : base(firstMathExpression, secondMathExpression)
        {

        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode(7727, 5657);
        }

        /// <summary>
        /// Converts this expression to the equivalent string.
        /// </summary>
        /// <returns>The string that represents this expression.</returns>
        public override string ToString()
        {
            if (m_parent is BinaryExpression)
            {
                return ToString("({0} or {1})");
            }

            return ToString("{0} or {1}");
        }

        /// <summary>
        /// Calculates this bitwise OR expression.
        /// </summary>
        /// <param name="parameters">An object that contains all parameters and functions for expressions.</param>
        /// <returns>
        /// A result of the calculation.
        /// </returns>
        /// <seealso cref="ExpressionParameters" />
        public override object Calculate(ExpressionParameters parameters)
        {
#if PORTABLE
            return (int)Math.Round((double)left.Calculate(parameters)) | (int)Math.Round((double)right.Calculate(parameters));
#else
            return (int)Math.Round((double)m_left.Calculate(parameters), MidpointRounding.AwayFromZero) | (int)Math.Round((double)m_right.Calculate(parameters), MidpointRounding.AwayFromZero);
#endif
        }

        /// <summary>
        /// Clones this instance of the <see cref="Or"/>.
        /// </summary>
        /// <returns>Returns the new instance of <see cref="IExpression"/> that is a clone of this instance.</returns>
        public override IExpression Clone()
        {
            return new Or(m_left.Clone(), m_right.Clone());
        }

    }

}
