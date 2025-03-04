// Copyright (c) Dmytro Kyshchenko. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace xFunc.Tests.Expressions.Programming;

public class LessOrEqualTest
{
    [Test]
    public void CalculateLessTrueTest1()
    {
        var parameters = new ExpressionParameters { new Parameter("x", 0) };
        var lessOrEqual = new LessOrEqual(Variable.X, new Number(10));

        Assert.That((bool)lessOrEqual.Execute(parameters), Is.True);
    }

    [Test]
    public void CalculateLessTrueTest2()
    {
        var parameters = new ExpressionParameters { new Parameter("x", 10) };
        var lessOrEqual = new LessOrEqual(Variable.X, new Number(10));

        Assert.That((bool)lessOrEqual.Execute(parameters), Is.True);
    }

    [Test]
    public void CalculateLessFalseTest()
    {
        var parameters = new ExpressionParameters { new Parameter("x", 666) };
        var lessOrEqual = new LessOrEqual(Variable.X, new Number(10));

        Assert.That((bool)lessOrEqual.Execute(parameters), Is.False);
    }

    [Test]
    public void LessOrEqualAngleTest()
    {
        var exp = new LessOrEqual(
            AngleValue.Degree(10).AsExpression(),
            AngleValue.Radian(3.14).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualPowerTest()
    {
        var exp = new LessOrEqual(
            PowerValue.Watt(10).AsExpression(),
            PowerValue.Kilowatt(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualTemperatureTest()
    {
        var exp = new LessOrEqual(
            TemperatureValue.Fahrenheit(10).AsExpression(),
            TemperatureValue.Celsius(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualMassTest()
    {
        var exp = new LessOrEqual(
            MassValue.Gram(10).AsExpression(),
            MassValue.Kilogram(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualLengthTest()
    {
        var exp = new LessOrEqual(
            LengthValue.Meter(10).AsExpression(),
            LengthValue.Kilometer(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualTimeTest()
    {
        var exp = new LessOrEqual(
            TimeValue.Second(10).AsExpression(),
            TimeValue.Minute(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void LessOrEqualAreaTest()
    {
        var exp = new LessOrEqual(
            AreaValue.Meter(10).AsExpression(),
            AreaValue.Kilometer(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.True(result);
    }

    [Test]
    public void LessOrEqualVolumeTest()
    {
        var exp = new LessOrEqual(
            VolumeValue.Centimeter(10).AsExpression(),
            VolumeValue.Meter(12).AsExpression()
        );
        var result = (bool)exp.Execute();

        Assert.That(result, Is.True);
    }

    [Test]
    public void CalculateInvalidTypeTest()
    {
        var lessOrEqual = new LessOrEqual(Bool.True, Bool.True);

        Assert.Throws<ExecutionException>(() => lessOrEqual.Execute());
    }

    [Test]
    public void CloneTest()
    {
        var exp = new LessOrEqual(Number.Two, new Number(3));
        var clone = exp.Clone();

        Assert.That(clone, Is.EqualTo(exp));
    }
}