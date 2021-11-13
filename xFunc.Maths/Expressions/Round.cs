// Copyright (c) Dmytro Kyshchenko. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace xFunc.Maths.Expressions;

/// <summary>
/// Represents the "round" function.
/// </summary>
public class Round : DifferentParametersExpression
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class.
    /// </summary>
    /// <param name="argument">The expression that represents a double-precision floating-point number to be rounded.</param>
    public Round(IExpression argument)
        : this(ImmutableArray.Create(argument))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class.
    /// </summary>
    /// <param name="argument">The expression that represents a double-precision floating-point number to be rounded.</param>
    /// <param name="digits">The expression that represents the number of fractional digits in the return value.</param>
    public Round(IExpression argument, IExpression digits)
        : this(ImmutableArray.Create(argument, digits))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <exception cref="ArgumentNullException"><paramref name="args"/> is null.</exception>
    internal Round(ImmutableArray<IExpression> args)
        : base(args)
    {
    }

    /// <inheritdoc />
    public override object Execute(ExpressionParameters? parameters)
    {
        var result = Argument.Execute(parameters);
        var digits = Digits?.Execute(parameters) ?? new NumberValue(0.0);

        return (result, digits) switch
        {
            (NumberValue left, NumberValue right) => NumberValue.Round(left, right),
            _ => throw new ResultIsNotSupportedException(this, result, digits),
        };
    }

    /// <inheritdoc />
    protected override TResult AnalyzeInternal<TResult>(IAnalyzer<TResult> analyzer)
        => analyzer.Analyze(this);

    /// <inheritdoc />
    [ExcludeFromCodeCoverage]
    protected override TResult AnalyzeInternal<TResult, TContext>(
        IAnalyzer<TResult, TContext> analyzer,
        TContext context)
        => analyzer.Analyze(this, context);

    /// <inheritdoc />
    public override IExpression Clone(ImmutableArray<IExpression>? arguments = null)
        => new Round(arguments ?? Arguments);

    /// <summary>
    /// Gets the expression that represents a double-precision floating-point number to be rounded.
    /// </summary>
    public IExpression Argument => this[0];

    /// <summary>
    /// Gets the expression that represents the number of fractional digits in the return value.
    /// </summary>
    public IExpression? Digits => ParametersCount == 2 ? this[1] : null;

    /// <summary>
    /// Gets the minimum count of parameters.
    /// </summary>
    public override int? MinParametersCount => 1;

    /// <summary>
    /// Gets the maximum count of parameters. <c>null</c> - Infinity.
    /// </summary>
    public override int? MaxParametersCount => 2;
}