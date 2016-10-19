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
using xFunc.Maths.Expressions.LogicalAndBitwise;
using Xunit;

namespace xFunc.Tests.Expressions.Maths.LogicalAndBitwise
{
    
    public class AndTest
    {
        
        [Fact]
        public void ExecuteTest1()
        {
            var exp = new And(new Number(1), new Number(3));

            Assert.Equal(1.0, exp.Execute());
        }

        [Fact]
        public void ExecuteTest2()
        {
            var exp = new And(new Number(1.5), new Number(2.5));

            Assert.Equal(2.0, exp.Execute());
        }

        [Fact]
        public void ExecuteTest3()
        {
            var exp = new And(new Bool(true), new Bool(false));

            Assert.Equal(false, exp.Execute());
        }

        [Fact]
        public void ExecuteTest4()
        {
            var exp = new And(new Bool(true), new Bool(true));

            Assert.Equal(true, exp.Execute());
        }

    }

}
