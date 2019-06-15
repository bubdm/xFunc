﻿// Copyright 2012-2019 Dmitry Kischenko
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
using System.Collections.Specialized;
using System.Linq;
using xFunc.Maths.Expressions.Collections;
using Xunit;

namespace xFunc.Tests.Expressions.Collections
{

    public class ParameterCollectionTest
    {

        [Fact]
        public void InitializeWithConstantsTest()
        {
            var parameters = new ParameterCollection(true);

            Assert.True(parameters.Constants.Any());
        }

        [Fact]
        public void InitializeWithoutConstantsTest()
        {
            var parameters = new ParameterCollection(false);

            Assert.False(parameters.Constants.Any());
        }

        [Fact]
        public void ChangedEventTest()
        {
            var isExecuted = false;

            var parameters = new ParameterCollection(false);
            parameters.CollectionChanged += (sender, args) =>
            {
                isExecuted = true;

                Assert.Equal(NotifyCollectionChangedAction.Add, args.Action);
                Assert.Null(args.OldItems);
                Assert.Equal(1, args.NewItems.Count);
            };

            parameters.Add(new Parameter("xxx", 1.0));

            Assert.True(isExecuted);
        }

        [Fact]
        public void EnumeratorTest()
        {
            var parameters = new ParameterCollection(true);

            Assert.True(parameters.Any());
        }

        [Fact]
        public void AddNullParameter()
        {
            var parameters = new ParameterCollection(true);

            Assert.Throws<ArgumentNullException>(() => parameters.Add(null as Parameter));
        }

        [Fact]
        public void AddConstantParameter()
        {
            var parameters = new ParameterCollection(true);
            var parameter = new Parameter("xxx", 1.0, ParameterType.Constant);

            Assert.Throws<ArgumentException>(() => parameters.Add(parameter));
        }

        [Fact]
        public void AddParameter()
        {
            var parameters = new ParameterCollection(true);
            var parameter = new Parameter("xxx", 1.0);

            parameters.Add(parameter);

            var result = parameters[parameter.Key];

            Assert.Equal(parameter.Value, result);
        }

        [Fact]
        public void AddStringParameter()
        {
            var parameters = new ParameterCollection(true);
            var key = "xxx";

            parameters.Add(key);

            var result = parameters[key];

            Assert.Equal(0.0, result);
        }

        [Fact]
        public void RemoveNullParameter()
        {
            var parameters = new ParameterCollection(true);

            Assert.Throws<ArgumentNullException>(() => parameters.Remove(null as Parameter));
        }

        [Fact]
        public void RemoveConstantParameter()
        {
            var parameters = new ParameterCollection(true);
            var parameter = new Parameter("xxx", 1.0, ParameterType.Constant);

            Assert.Throws<ArgumentException>(() => parameters.Add(parameter));
        }

        [Fact]
        public void RemoveStringParameter()
        {
            var parameters = new ParameterCollection(true);

            Assert.Throws<ArgumentNullException>(() => parameters.Remove(string.Empty));
        }

    }

}