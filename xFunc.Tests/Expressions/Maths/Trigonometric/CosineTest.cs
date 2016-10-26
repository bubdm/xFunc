﻿// Copyright 2012-2016 Dmitry Kischenko
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
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Trigonometric;
using Xunit;

namespace xFunc.Tests.Expressions.Maths.Trigonometric
{
    
    public class CosineTest
    {

        [Fact]
        public void CalculateRadianTest()
        {
            var exp = new Cos(new Number(1));

            Assert.Equal(Math.Cos(1), exp.Execute(AngleMeasurement.Radian));
        }

        [Fact]
        public void CalculateDegreeTest()
        {
            var exp = new Cos(new Number(1));

            Assert.Equal(Math.Cos(1 * Math.PI / 180), exp.Execute(AngleMeasurement.Degree));
        }

        [Fact]
        public void CalculateGradianTest()
        {
            var exp = new Cos(new Number(1));

            Assert.Equal(Math.Cos(1 * Math.PI / 200), exp.Execute(AngleMeasurement.Gradian));
        }

        [Fact]
        public void CloneTest()
        {
            var exp = new Cos(new Number(1));
            var clone = exp.Clone();

            Assert.Equal(exp, clone);
        }

    }

}
