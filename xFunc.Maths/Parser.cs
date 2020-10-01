﻿// Copyright 2012-2020 Dmytro Kyshchenko
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
using System.Collections.Immutable;
using System.Globalization;
using System.Numerics;
using xFunc.Maths.Analyzers;
using xFunc.Maths.Expressions;
using xFunc.Maths.Expressions.Angles;
using xFunc.Maths.Expressions.ComplexNumbers;
using xFunc.Maths.Expressions.LogicalAndBitwise;
using xFunc.Maths.Expressions.Matrices;
using xFunc.Maths.Expressions.Programming;
using xFunc.Maths.Resources;
using xFunc.Maths.Tokenization;
using static xFunc.Maths.ThrowHelpers;
using static xFunc.Maths.Tokenization.TokenKind;
using Vector = xFunc.Maths.Expressions.Matrices.Vector;

namespace xFunc.Maths
{
    /// <summary>
    /// The parser for mathematical expressions.
    /// </summary>
    public partial class Parser : IParser
    {
        private readonly IDifferentiator differentiator;
        private readonly ISimplifier simplifier;

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser"/> class with default implementations of <see cref="IDifferentiator"/> and <see cref="ISimplifier" />.
        /// </summary>
        public Parser()
            : this(new Differentiator(), new Simplifier())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Parser" /> class.
        /// </summary>
        /// <param name="differentiator">The differentiator.</param>
        /// <param name="simplifier">The simplifier.</param>
        public Parser(IDifferentiator differentiator, ISimplifier simplifier)
        {
            if (differentiator is null)
                ArgNull(ExceptionArgument.differentiator);
            if (simplifier is null)
                ArgNull(ExceptionArgument.simplifier);

            this.differentiator = differentiator;
            this.simplifier = simplifier;
        }

        /// <summary>
        /// Parses the specified function.
        /// </summary>
        /// <param name="function">The string that contains the functions and operators.</param>
        /// <returns>The parsed expression.</returns>
        public IExpression Parse(string function)
        {
            if (string.IsNullOrWhiteSpace(function))
                throw new ArgumentNullException(nameof(function), Resource.NotSpecifiedFunction);

            var lexer = new Lexer(function);
            var tokenReader = new TokenReader(ref lexer);

            try
            {
                var exp = ParseStatement(ref tokenReader);
                var token = tokenReader.GetCurrent();
                if (exp == null || !tokenReader.IsEnd || token.IsNotEmpty())
                    throw new ParseException(Resource.ErrorWhileParsingTree);

                return exp;
            }
            finally
            {
                tokenReader.Dispose();
            }
        }

        // TODO: to expressions?
        private IExpression? ParseStatement(ref TokenReader tokenReader)
            => ParseBinaryAssign(ref tokenReader) ??
               ParseAssign(ref tokenReader) ??
               ParseDef(ref tokenReader) ??
               ParseUndef(ref tokenReader) ??
               ParseIf(ref tokenReader) ??
               ParseFor(ref tokenReader) ??
               ParseWhile(ref tokenReader) ??
               ParseExpression(ref tokenReader);

        private IExpression? AssignmentKey(ref TokenReader tokenReader)
            => ParseFunctionDeclaration(ref tokenReader) ??
               ParseVariable(ref tokenReader);

        private IExpression? ParseAssign(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var left = AssignmentKey(ref tokenReader);
            if (left == null)
            {
                tokenReader.Rollback(scope);

                return null;
            }

            var @operator = tokenReader.GetCurrent(AssignOperator);
            if (@operator.IsNotEmpty())
            {
                var right = ParseExpression(ref tokenReader) ??
                            MissingSecondOperand(@operator.Kind);

                tokenReader.Commit();

                return new Define(left, right);
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? ParseDef(ref TokenReader tokenReader)
        {
            var def = tokenReader.GetCurrent(DefineKeyword);
            if (def.IsEmpty())
                return null;

            if (!tokenReader.Check(OpenParenthesisSymbol))
                MissingOpenParenthesis(def.Kind);

            var key = AssignmentKey(ref tokenReader) ??
                      throw new ParseException(Resource.AssignKeyParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(key);

            var value = ParseExpression(ref tokenReader) ??
                        throw new ParseException(Resource.DefValueParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                MissingCloseParenthesis(def.Kind);

            return new Define(key, value);
        }

        private IExpression? ParseUndef(ref TokenReader tokenReader)
        {
            var undef = tokenReader.GetCurrent(UndefineKeyword);
            if (undef.IsEmpty())
                return null;

            if (!tokenReader.Check(OpenParenthesisSymbol))
                MissingOpenParenthesis(undef.Kind);

            var key = AssignmentKey(ref tokenReader) ??
                      throw new ParseException(Resource.AssignKeyParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                MissingCloseParenthesis(undef.Kind);

            return new Undefine(key);
        }

        private IExpression? ParseIf(ref TokenReader tokenReader)
        {
            var @if = tokenReader.GetCurrent(IfKeyword);
            if (@if.IsEmpty())
                return null;

            if (!tokenReader.Check(OpenParenthesisSymbol))
                MissingOpenParenthesis(@if.Kind);

            var condition = ParseConditionalOrOperator(ref tokenReader) ??
                            throw new ParseException(Resource.IfConditionParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(condition);

            var then = ParseExpression(ref tokenReader) ??
                       throw new ParseException(Resource.IfThenParseException);

            IExpression? @else = null;
            if (tokenReader.Check(CommaSymbol))
                @else = ParseExpression(ref tokenReader) ??
                        throw new ParseException(Resource.IfElseParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                MissingCloseParenthesis(@if.Kind);

            if (@else != null)
                return new If(condition, then, @else);

            return new If(condition, then);
        }

        private IExpression? ParseFor(ref TokenReader tokenReader)
        {
            var @for = tokenReader.GetCurrent(ForKeyword);
            if (@for.IsEmpty())
                return null;

            if (!tokenReader.Check(OpenParenthesisSymbol))
                MissingOpenParenthesis(@for.Kind);

            var body = ParseStatement(ref tokenReader) ??
                       throw new ParseException(Resource.ForBodyParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(body);

            var init = ParseStatement(ref tokenReader) ??
                       throw new ParseException(Resource.ForInitParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(init);

            var condition = ParseConditionalOrOperator(ref tokenReader) ??
                            throw new ParseException(Resource.ForConditionParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(condition);

            var iter = ParseStatement(ref tokenReader) ??
                       throw new ParseException(Resource.ForIterParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                MissingCloseParenthesis(@for.Kind);

            return new For(body, init, condition, iter);
        }

        private IExpression? ParseWhile(ref TokenReader tokenReader)
        {
            var @while = tokenReader.GetCurrent(WhileKeyword);
            if (@while.IsEmpty())
                return null;

            if (!tokenReader.Check(OpenParenthesisSymbol))
                MissingOpenParenthesis(@while.Kind);

            var body = ParseStatement(ref tokenReader) ??
                       throw new ParseException(Resource.WhileBodyParseException);

            if (!tokenReader.Check(CommaSymbol))
                MissingComma(body);

            var condition = ParseConditionalOrOperator(ref tokenReader) ??
                            throw new ParseException(Resource.WhileConditionParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                MissingCloseParenthesis(@while.Kind);

            return new While(body, condition);
        }

        private IExpression? ParseFunctionDeclaration(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var id = tokenReader.GetCurrent(Id);
            if (id.IsNotEmpty() && tokenReader.Check(OpenParenthesisSymbol))
            {
                var parameterList = ImmutableArray.CreateBuilder<IExpression>(1);

                var exp = ParseVariable(ref tokenReader);
                if (exp != null)
                {
                    parameterList.Add(exp);

                    while (tokenReader.Check(CommaSymbol))
                    {
                        exp = ParseVariable(ref tokenReader);
                        if (exp == null)
                        {
                            tokenReader.Rollback(scope);

                            return null;
                        }

                        parameterList.Add(exp);
                    }
                }

                if (tokenReader.Check(CloseParenthesisSymbol))
                {
                    tokenReader.Commit();

                    return CreateFunction(id, parameterList.ToImmutableArray());
                }
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? ParseExpression(ref TokenReader tokenReader)
            => ParseBinaryAssign(ref tokenReader) ??
               Ternary(ref tokenReader);

        private IExpression? ParseBinaryAssign(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var variable = ParseVariable(ref tokenReader);
            if (variable == null)
            {
                tokenReader.Rollback(scope);

                return null;
            }

            var @operator = tokenReader.GetCurrent(MulAssignOperator) ||
                            tokenReader.GetCurrent(DivAssignOperator) ||
                            tokenReader.GetCurrent(AddAssignOperator) ||
                            tokenReader.GetCurrent(SubAssignOperator) ||
                            tokenReader.GetCurrent(LeftShiftAssignOperator) ||
                            tokenReader.GetCurrent(RightShiftAssignOperator);

            if (@operator.IsNotEmpty())
            {
                var exp = ParseExpression(ref tokenReader) ??
                          MissingSecondOperand(@operator.Kind);

                tokenReader.Commit();

                return CreateBinaryAssign(@operator, variable, exp);
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? Ternary(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var condition = ParseConditionalOrOperator(ref tokenReader);
            if (condition == null)
            {
                tokenReader.Rollback(scope);

                return null;
            }

            if (!tokenReader.Check(QuestionMarkSymbol))
            {
                tokenReader.Commit();

                return condition;
            }

            var then = ParseExpression(ref tokenReader) ??
                       throw new ParseException(Resource.TernaryThenParseException);

            if (!tokenReader.Check(ColonSymbol))
                throw new ParseException(Resource.TernaryColonParseException);

            var @else = ParseExpression(ref tokenReader) ??
                        throw new ParseException(Resource.TernaryElseParseException);

            tokenReader.Commit();

            return new If(condition, then, @else);
        }

        private IExpression? ParseConditionalOrOperator(ref TokenReader tokenReader)
        {
            var left = ParseConditionalAndOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.Check(ConditionalOrOperator);
                if (!@operator)
                    return left;

                var right = ParseConditionalAndOperator(ref tokenReader) ??
                            MissingSecondOperand(ConditionalOrOperator);

                left = new ConditionalOr(left, right);
            }
        }

        private IExpression? ParseConditionalAndOperator(ref TokenReader tokenReader)
        {
            var left = ParseBitwiseOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.Check(ConditionalAndOperator);
                if (!@operator)
                    return left;

                var right = ParseBitwiseOperator(ref tokenReader) ??
                            MissingSecondOperand(ConditionalAndOperator);

                left = new ConditionalAnd(left, right);
            }
        }

        private IExpression? ParseBitwiseOperator(ref TokenReader tokenReader)
        {
            var left = ParseOrOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var token = tokenReader.GetCurrent(ImplicationOperator) ||
                            tokenReader.GetCurrent(EqualityOperator) ||
                            tokenReader.GetCurrent(NAndKeyword) ||
                            tokenReader.GetCurrent(NOrKeyword) ||
                            tokenReader.GetCurrent(EqKeyword) ||
                            tokenReader.GetCurrent(ImplKeyword);

                if (token.IsEmpty())
                    return left;

                var right = ParseOrOperator(ref tokenReader) ??
                            MissingSecondOperand(token.Kind);

                left = CreateBitwiseOperator(token, left, right);
            }
        }

        private IExpression? ParseOrOperator(ref TokenReader tokenReader)
        {
            var left = ParseXOrOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var token = tokenReader.Check(OrOperator) ||
                            tokenReader.Check(OrKeyword);

                if (!token)
                    return left;

                var right = ParseXOrOperator(ref tokenReader) ??
                            MissingSecondOperand(OrOperator);

                left = new Or(left, right);
            }
        }

        private IExpression? ParseXOrOperator(ref TokenReader tokenReader)
        {
            var left = ParseAndOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var token = tokenReader.Check(XOrKeyword);
                if (!token)
                    return left;

                var right = ParseAndOperator(ref tokenReader) ??
                            MissingSecondOperand(XOrKeyword);

                left = new XOr(left, right);
            }
        }

        private IExpression? ParseAndOperator(ref TokenReader tokenReader)
        {
            var left = ParseEqualityOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var token = tokenReader.Check(AndOperator) ||
                            tokenReader.Check(AndKeyword);

                if (!token)
                    return left;

                var right = ParseEqualityOperator(ref tokenReader) ??
                            MissingSecondOperand(AndOperator);

                left = new And(left, right);
            }
        }

        private IExpression? ParseEqualityOperator(ref TokenReader tokenReader)
        {
            var left = ParseRelationalOperator(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.GetCurrent(EqualOperator) ||
                                tokenReader.GetCurrent(NotEqualOperator);

                if (@operator.IsEmpty())
                    return left;

                var right = ParseRelationalOperator(ref tokenReader) ??
                            MissingSecondOperand(@operator.Kind);

                left = CreateEqualityOperator(@operator, left, right);
            }
        }

        private IExpression? ParseRelationalOperator(ref TokenReader tokenReader)
        {
            var left = ParseShift(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.GetCurrent(LessThanOperator) ||
                                tokenReader.GetCurrent(LessOrEqualOperator) ||
                                tokenReader.GetCurrent(GreaterThanOperator) ||
                                tokenReader.GetCurrent(GreaterOrEqualOperator);

                if (@operator.IsEmpty())
                    return left;

                var right = ParseShift(ref tokenReader) ??
                            MissingSecondOperand(@operator.Kind);

                left = CreateRelationalOperator(@operator, left, right);
            }
        }

        private IExpression? ParseShift(ref TokenReader tokenReader)
        {
            var left = ParseAddSub(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.GetCurrent(LeftShiftOperator) ||
                                tokenReader.GetCurrent(RightShiftOperator);

                if (@operator.IsEmpty())
                    return left;

                var right = ParseAddSub(ref tokenReader) ??
                            MissingSecondOperand(@operator.Kind);

                left = CreateShift(@operator, left, right);
            }
        }

        private IExpression? ParseAddSub(ref TokenReader tokenReader)
        {
            var left = ParseMulDivMod(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var @operator = tokenReader.GetCurrent(PlusOperator) ||
                                tokenReader.GetCurrent(MinusOperator);

                if (@operator.IsEmpty())
                    return left;

                var right = ParseMulDivMod(ref tokenReader) ??
                            MissingSecondOperand(@operator.Kind);

                left = CreateAddSub(@operator, left, right);
            }
        }

        private IExpression? ParseMulDivMod(ref TokenReader tokenReader)
        {
            var left = ParseMulImplicit(ref tokenReader);
            if (left == null)
                return null;

            while (true)
            {
                var token = tokenReader.GetCurrent(MultiplicationOperator) ||
                            tokenReader.GetCurrent(DivisionOperator) ||
                            tokenReader.GetCurrent(ModuloOperator) ||
                            tokenReader.GetCurrent(ModKeyword);

                if (token.IsEmpty())
                    return left;

                var right = ParseMulImplicit(ref tokenReader) ??
                            MissingSecondOperand(token.Kind);

                left = CreateMulDivMod(token, left, right);
            }
        }

        private IExpression? ParseMulImplicit(ref TokenReader tokenReader)
            => ParseMulImplicitLeftUnary(ref tokenReader) ??
               ParseLeftUnary(ref tokenReader);

        private IExpression? ParseMulImplicitLeftUnary(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var @operator = tokenReader.GetCurrent(MinusOperator);
            var number = ParseNumber(ref tokenReader);
            if (number != null)
            {
                var rightUnary = ParseMulImplicitExponentiation(ref tokenReader) ??
                                 ParseParenthesesExpression(ref tokenReader) ??
                                 ParseMatrix(ref tokenReader) ??
                                 ParseVector(ref tokenReader);
                if (rightUnary != null)
                {
                    if (@operator.IsNotEmpty())
                        number = new UnaryMinus(number);

                    tokenReader.Commit();

                    return new Mul(number, rightUnary);
                }
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? ParseMulImplicitExponentiation(ref TokenReader tokenReader)
        {
            var left = ParseFunctionOrVariable(ref tokenReader);
            if (left == null)
                return null;

            var @operator = tokenReader.GetCurrent(ExponentiationOperator);
            if (@operator.IsEmpty())
                return left;

            var right = ParseExponentiation(ref tokenReader) ??
                        throw new ParseException(Resource.ExponentParseException);

            return new Pow(left, right);
        }

        private IExpression? ParseLeftUnary(ref TokenReader tokenReader)
        {
            var token = tokenReader.GetCurrent(NotOperator) ||
                        tokenReader.GetCurrent(MinusOperator) ||
                        tokenReader.GetCurrent(PlusOperator) ||
                        tokenReader.GetCurrent(NotKeyword);

            var operand = ParseExponentiation(ref tokenReader);
            if (operand == null || token.IsEmpty() || token.Is(PlusOperator))
                return operand;

            if (token.Is(MinusOperator))
                return new UnaryMinus(operand);

            return new Not(operand);
        }

        private IExpression? ParseExponentiation(ref TokenReader tokenReader)
        {
            var left = ParseRightUnary(ref tokenReader);
            if (left == null)
                return null;

            var @operator = tokenReader.GetCurrent(ExponentiationOperator);
            if (@operator.IsEmpty())
                return left;

            var right = ParseLeftUnary(ref tokenReader) ??
                        throw new ParseException(Resource.ExponentParseException);

            return new Pow(left, right);
        }

        private IExpression? ParseRightUnary(ref TokenReader tokenReader)
            => ParseFactorial(ref tokenReader) ??
               ParseIncDec(ref tokenReader) ??
               ParseOperand(ref tokenReader);

        private IExpression? ParseFactorial(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var number = ParseNumber(ref tokenReader);
            if (number != null && tokenReader.Check(FactorialOperator))
            {
                tokenReader.Commit();

                return new Fact(number);
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? ParseIncDec(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            var variable = ParseVariable(ref tokenReader);
            if (variable != null)
            {
                tokenReader.Commit();

                if (tokenReader.Check(IncrementOperator))
                    return new Inc(variable);

                if (tokenReader.Check(DecrementOperator))
                    return new Dec(variable);

                return variable;
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private IExpression? ParseOperand(ref TokenReader tokenReader)
            => ParseComplexNumber(ref tokenReader) ??
               ParseNumber(ref tokenReader) ??
               ParseFunctionOrVariable(ref tokenReader) ??
               ParseBoolean(ref tokenReader) ??
               ParseParenthesesExpression(ref tokenReader) ??
               ParseMatrix(ref tokenReader) ??
               ParseVector(ref tokenReader);

        private IExpression? ParseParenthesesExpression(ref TokenReader tokenReader)
        {
            if (!tokenReader.Check(OpenParenthesisSymbol))
                return null;

            var exp = ParseExpression(ref tokenReader) ??
                      throw new ParseException(Resource.ExpParenParseException);

            if (!tokenReader.Check(CloseParenthesisSymbol))
                throw new ParseException(string.Format(CultureInfo.InvariantCulture, Resource.CloseParenParseException, exp));

            return exp;
        }

        private IExpression? ParseFunctionOrVariable(ref TokenReader tokenReader)
        {
            var function = tokenReader.GetCurrent(Id);
            if (function.IsEmpty())
                return null;

            var parameterList = ParseParameterList(ref tokenReader);
            if (parameterList == null)
                return CreateVariable(function);

            return CreateFunction(function, parameterList);
        }

        private ImmutableArray<IExpression> ParseParameterList(ref TokenReader tokenReader)
        {
            if (!tokenReader.Check(OpenParenthesisSymbol))
                return default;

            var parameterList = ImmutableArray.CreateBuilder<IExpression>(1);

            var exp = ParseExpression(ref tokenReader);
            if (exp != null)
            {
                parameterList.Add(exp);

                while (tokenReader.Check(CommaSymbol))
                {
                    exp = ParseExpression(ref tokenReader) ??
                          MissingExpression();

                    parameterList.Add(exp);
                }
            }

            if (!tokenReader.Check(CloseParenthesisSymbol))
                throw new ParseException(Resource.ParameterListCloseParseException);

            return parameterList.ToImmutableArray();
        }

        private IExpression? ParseNumber(ref TokenReader tokenReader)
        {
            var number = tokenReader.GetCurrent(TokenKind.Number);
            if (number.IsEmpty())
                return null;

            if (tokenReader.Check(DegreeSymbol) || tokenReader.Check(DegreeKeyword))
                return AngleValue.Degree(number.NumberValue).AsExpression();

            if (tokenReader.Check(RadianKeyword))
                return AngleValue.Radian(number.NumberValue).AsExpression();

            if (tokenReader.Check(GradianKeyword))
                return AngleValue.Gradian(number.NumberValue).AsExpression();

            return new Number(number.NumberValue);
        }

        private IExpression? ParseComplexNumber(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            // plus symbol can be ignored
            tokenReader.GetCurrent(PlusOperator);

            var magnitude = tokenReader.GetCurrent(TokenKind.Number);
            if (magnitude.IsNotEmpty())
            {
                var hasAngleSymbol = tokenReader.Check(AngleSymbol);
                if (hasAngleSymbol)
                {
                    var phaseSign = tokenReader.GetCurrent(PlusOperator) ||
                                    tokenReader.GetCurrent(MinusOperator);

                    var phase = tokenReader.GetCurrent(TokenKind.Number);
                    if (phase.IsEmpty())
                        throw new ParseException(Resource.PhaseParseException);

                    var hasDegreeSymbol = tokenReader.Check(DegreeSymbol);
                    if (!hasDegreeSymbol)
                        throw new ParseException(Resource.DegreeComplexNumberParseException);

                    tokenReader.Commit();

                    var magnitudeNumber = magnitude.NumberValue;
                    var sign = phaseSign.Is(MinusOperator) ? -1 : 1;
                    var phaseNumber = phase.NumberValue * sign;
                    var complex = Complex.FromPolarCoordinates(magnitudeNumber, phaseNumber);

                    return new ComplexNumber(complex);
                }
            }

            tokenReader.Rollback(scope);

            return null;
        }

        private Variable? ParseVariable(ref TokenReader tokenReader)
        {
            var variable = tokenReader.GetCurrent(Id);

            // usually we use 'scope' in such cases, but here we can ignore it,
            // because parsing of variable is always 'scoped'
            // if it is not true anymore, then use 'scope'
            if (variable.IsEmpty() || tokenReader.Check(OpenParenthesisSymbol))
                return null;

            return CreateVariable(variable);
        }

        private IExpression? ParseBoolean(ref TokenReader tokenReader)
        {
            if (tokenReader.Check(TrueKeyword))
                return Bool.True;

            if (tokenReader.Check(FalseKeyword))
                return Bool.False;

            return null;
        }

        private Vector? ParseVector(ref TokenReader tokenReader)
        {
            if (!tokenReader.Check(OpenBraceSymbol))
                return null;

            var exp = ParseExpression(ref tokenReader);
            if (exp == null)
                throw new ParseException(Resource.VectorEmptyError);

            var parameterList = ImmutableArray.CreateBuilder<IExpression>(1);
            parameterList.Add(exp);

            while (tokenReader.Check(CommaSymbol))
            {
                exp = ParseExpression(ref tokenReader) ??
                      throw new ParseException(Resource.VectorCommaParseException);

                parameterList.Add(exp);
            }

            if (!tokenReader.Check(CloseBraceSymbol))
                throw new ParseException(Resource.VectorCloseBraceParseException);

            return new Vector(parameterList.ToImmutableArray());
        }

        private IExpression? ParseMatrix(ref TokenReader tokenReader)
        {
            var scope = tokenReader.CreateScope();

            if (tokenReader.Check(OpenBraceSymbol))
            {
                var exp = ParseVector(ref tokenReader);
                if (exp != null)
                {
                    var vectors = ImmutableArray.CreateBuilder<Vector>(1);
                    vectors.Add(exp);

                    while (tokenReader.Check(CommaSymbol))
                    {
                        exp = ParseVector(ref tokenReader) ??
                              throw new ParseException(Resource.MatrixCommaParseException);

                        vectors.Add(exp);
                    }

                    if (!tokenReader.Check(CloseBraceSymbol))
                        throw new ParseException(Resource.MatrixCloseBraceParseException);

                    tokenReader.Commit();

                    return new Matrix(vectors.ToImmutableArray());
                }
            }

            tokenReader.Rollback(scope);

            return null;
        }
    }
}