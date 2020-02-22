// Copyright 2012-2020 Dmytro Kyshchenko
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

using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.LogicalAndBitwise;
using xFunc.Maths.Expressions.Matrices;
using xFunc.Maths.Expressions.Statistical;
using Xunit;

namespace xFunc.Tests.Expressions.Statistical
{
    public class MaxTest
    {
        [Fact]
        public void OneNumberTest()
        {
            var exp = new Max(new[] { new Number(2) });
            var result = exp.Execute();

            Assert.Equal(2.0, result);
        }

        [Fact]
        public void TwoNumberTest()
        {
            var exp = new Max(new[] { new Number(2), new Number(4) });
            var result = exp.Execute();

            Assert.Equal(4.0, result);
        }

        [Fact]
        public void ThreeNumberTest()
        {
            var exp = new Max(new[] { new Number(9), new Number(2), new Number(4) });
            var result = exp.Execute();

            Assert.Equal(9.0, result);
        }

        [Fact]
        public void VectorTest()
        {
            var exp = new Max(new[] { new Vector(new[] { new Number(1), new Number(2), new Number(3) }) });
            var result = exp.Execute();

            Assert.Equal(3.0, result);
        }

        [Fact]
        public void NotSupportedException()
        {
            var exp = new Max(new[] { new Bool(false), new Bool(false) });

            Assert.Throws<ResultIsNotSupportedException>(() => exp.Execute());
        }

        [Fact]
        public void CloneTest()
        {
            var exp = new Max(new[] { new Number(1), new Number(2) });
            var clone = exp.Clone();

            Assert.Equal(exp, clone);
        }
    }
}