﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using xFunc.Maths;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Trigonometric;

namespace xFunc.Test.Expressions.Maths
{

    [TestClass]
    public class DivisionTest
    {

        [TestMethod]
        public void CalculateTest()
        {
            IMathExpression exp = new Division(new Number(1), new Number(2));

            Assert.AreEqual(1.0 / 2.0, exp.Calculate(null));
        }

        [TestMethod]
        public void DerivativeTest1()
        {
            IMathExpression exp = new Division(new Number(1), new Variable('x'));
            IMathExpression deriv = exp.Differentiate();

            Assert.AreEqual("-(1 * 1) / (x ^ 2)", deriv.ToString());
        }

        [TestMethod]
        public void DerivativeTest2()
        {
            // sin(x) / x
            IMathExpression exp = new Division(new Sine(new Variable('x')), new Variable('x'));
            IMathExpression deriv = exp.Differentiate();

            Assert.AreEqual("(((cos(x) * 1) * x) - (sin(x) * 1)) / (x ^ 2)", deriv.ToString());
        }

        [TestMethod]
        public void DerivativeTest3()
        {
            // (2x) / (3x)
            Number num1 = new Number(2);
            Variable x = new Variable('x');
            Multiplication mul1 = new Multiplication(num1, x);

            Number num2 = new Number(3);
            Multiplication mul2 = new Multiplication(num2, x.Clone());

            IMathExpression exp = new Division(mul1, mul2);
            IMathExpression deriv = exp.Differentiate();

            Assert.AreEqual("(((2 * 1) * (3 * x)) - ((2 * x) * (3 * 1))) / ((3 * x) ^ 2)", deriv.ToString());

            num1.Value = 4;
            num2.Value = 5;
            Assert.AreEqual("(4 * x) / (5 * x)", exp.ToString());
            Assert.AreEqual("(((2 * 1) * (3 * x)) - ((2 * x) * (3 * 1))) / ((3 * x) ^ 2)", deriv.ToString());
        }

        [TestMethod]
        public void PartialDerivativeTest1()
        {
            // (y + x ^ 2) / x
            IMathExpression exp = new Division(new Addition(new Variable('y'), new Exponentiation(new Variable('x'), new Number(2))), new Variable('x'));
            IMathExpression deriv = exp.Differentiate();
            Assert.AreEqual("(((1 * (2 * (x ^ (2 - 1)))) * x) - ((y + (x ^ 2)) * 1)) / (x ^ 2)", deriv.ToString());
        }

        [TestMethod]
        public void PartialDerivativeTest2()
        {
            IMathExpression exp = new Division(new Variable('y'), new Variable('x'));
            IMathExpression deriv = exp.Differentiate();
            Assert.AreEqual("-(y * 1) / (x ^ 2)", deriv.ToString());
        }

        [TestMethod]
        public void PartialDerivativeTest3()
        {
            IMathExpression exp = new Division(new Variable('y'), new Variable('x'));
            IMathExpression deriv = exp.Differentiate(new Variable('y'));
            Assert.AreEqual("1 / x", deriv.ToString());
        }

        [TestMethod]
        public void PartialDerivativeTest4()
        {
            // (x + 1) / x
            IMathExpression exp = new Division(new Addition(new Variable('x'), new Number(1)), new Variable('x'));
            IMathExpression deriv = exp.Differentiate(new Variable('y'));
            Assert.AreEqual("0", deriv.ToString());
        }

    }

}
