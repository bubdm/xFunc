﻿// Copyright 2012-2013 Dmitry Kischenko
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

namespace xFunc.Maths.Expressions
{

    public class Addition : BinaryMathExpression
    {

        public Addition() : base(null, null) { }

        public Addition(IMathExpression firstOperand, IMathExpression secondOperand) : base(firstOperand, secondOperand) { }

        public override string ToString()
        {
            if (parentMathExpression is BinaryMathExpression)
            {
                return ToString("({0} + {1})");
            }

            return ToString("{0} + {1}");
        }

        public override double Calculate(MathParameterCollection parameters)
        {
            return firstMathExpression.Calculate(parameters) + secondMathExpression.Calculate(parameters);
        }

        public override IMathExpression Differentiate(Variable variable)
        {
            var first = MathParser.HasVar(firstMathExpression, variable);
            var second = MathParser.HasVar(secondMathExpression, variable);

            if (first && second)
            {
                return new Addition(firstMathExpression.Clone().Differentiate(variable), secondMathExpression.Differentiate(variable).Clone());
            }
            if (first)
            {
                return firstMathExpression.Clone().Differentiate(variable);
            }
            if (second)
            {
                return secondMathExpression.Differentiate(variable).Clone();
            }

            return new Number(0);
        }

        public override IMathExpression Clone()
        {
            return new Addition(firstMathExpression.Clone(), secondMathExpression.Clone());
        }

    }

}
