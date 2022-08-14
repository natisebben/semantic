using Analyzer.Exceptions;
using Common;
using System;
using System.Linq;

namespace Analyzer
{
    public class SyntaxAnalyzer
    {
        SyntaxAnalyzerState _state;

        public SyntaxAnalyzer(SyntaxAnalyzerState state)
        {
            _state = state;
        }

        public void Analyze()
        {
            try
            {
                ReadToken();

                if (IsExternalDeclarationList())
                    Console.WriteLine("Valid");
                else
                    Console.WriteLine("Invalid");
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Invalid");
            }
        }

        public void ReadToken() =>
            ReadToken(++_state.CurrentTokenIndex);

        public void ReadToken(int pos)
        {
            if (_state.CurrentTokenIndex < _state.TokensQuantity)
                _state.CurrentToken = _state.Tokens.ElementAt(pos);
            else
                _state.CurrentToken = default;
        }

        public bool IsToken(Token token) =>
            _state.CurrentToken?.TokenType == token;

        public void CheckIdentifierDeclaration()
        {
            if (_state.CurrentToken.TokenType != Token.IDENTIFIER)
                return;

            if (_state.DeclarationTokens.Any(x => x.Token.Equals(_state.CurrentToken.Token)))
            {
                throw new IdentifierAlreadyDeclaredException(_state.CurrentToken.Line, _state.CurrentToken.Token);
            }
            else
            {
                _state.DeclarationTokens.Add(_state.CurrentToken);
            }
        }

        public bool IsExternalDeclarationList()
        {
            while (!IsToken(Token.EOF))
                if (!IsExternalDeclaration())
                    return false;

            return true;
        }

        public bool IsExternalDeclaration()
        {
            var pos = _state.CurrentTokenIndex;

            if (IsDeclaration())
            {
                return true;
            }
            else
            {
                _state.CurrentTokenIndex = pos - 1;
                ReadToken();

                if (IsFunctionDefinition())
                    return true;
            }

            return false;

            //var pos = _state.CurrentTokenIndex;

            //if (IsFunctionDefinition())
            //    return true;

            //else
            //{
            //    _state.CurrentTokenIndex = pos - 1;

            //    ReadToken();

            //    if (IsDeclaration())
            //        return true;
            //}

            //return false;
        }

        public bool IsFunctionDefinition()
        {
            if (IsDeclarationSpecifier())
            {
                if (IsDeclarator())
                {
                    if (IsDeclarationList())
                    {
                        if (IsCompoundStatement())
                            return true;
                    }
                    else if (IsCompoundStatement())
                        return true;
                }
            }
            else if (IsDeclarator())
            {
                if (IsDeclarationList())
                {
                    if (IsCompoundStatement())
                        return true;
                }
                else if (IsCompoundStatement())
                    return true;
            }

            return false;
        }

        public bool IsDeclarationSpecifier()
        {
            if (IsTypeSpecifier())
            {
                if (IsDeclarationSpecifier())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsTypeSpecifier()
        {
            if (IsToken(Token.VOID))
            {
                ReadToken();
                return true;
            }
            else if (IsPrimitiveTypeSpecifier())
                return true;
            else if (IsUnsignedSpecifier())
                return true;
            else if (IsStructSpecifier())
                return true;

            return false;
        }

        public bool IsPrimitiveTypeSpecifier()
        {
            if (IsToken(Token.CHAR)
                || IsToken(Token.INT)
                || IsToken(Token.FLOAT)
                || IsToken(Token.DOUBLE))
            {
                ReadToken();

                return true;
            }
            else if (LongIntSpecifier())
                return true;

            return false;
        }

        public bool LongIntSpecifier()
        {
            if (IsToken(Token.LONG))
            {
                ReadToken();

                if (LongIntSpecifier())
                    return true;

                else if (IsToken(Token.INT))
                {
                    ReadToken();
                    return true;
                }
            }

            return false;
        }

        public bool IsUnsignedSpecifier()
        {
            if (IsToken(Token.UNSIGNED))
            {
                ReadToken();

                if (IsPrimitiveTypeSpecifier())
                    return true;
            }

            return false;
        }

        public bool IsStructSpecifier()
        {
            if (IsToken(Token.STRUCT))
            {
                ReadToken();

                if (IsToken(Token.IDENTIFIER))
                {
                    CheckIdentifierDeclaration();
                    ReadToken();

                    if (IsToken(Token.BRACEOPEN))
                    {
                        ReadToken();

                        if (IsStructDeclarationList())
                        {
                            if (IsToken(Token.BRACECLOSE))
                            {
                                ReadToken();
                                return true;
                            }
                        }

                    }
                    else
                        return true;
                }
                else if (IsToken(Token.BRACEOPEN))
                {
                    ReadToken();

                    if (IsStructDeclarationList())
                        if (IsToken(Token.BRACECLOSE))
                            return true;
                }
            }

            return false;
        }

        public bool IsStructDeclarationList()
        {
            if (IsStructDeclaration())
            {
                if (IsStructDeclarationList())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsStructDeclaration()
        {
            if (IsSpecifierList())
            {
                if (IsStructDeclaratorList())
                {
                    if (IsToken(Token.SEMICOLLON))
                    {
                        ReadToken();
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsSpecifierList()
        {
            if (IsTypeSpecifier())
            {
                if (IsSpecifierList())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsStructDeclaratorList()
        {
            if (IsStructDeclarator())
            {
                if (IsStructDeclaratorListLine())
                    return true;
            }

            return false;
        }

        public bool IsStructDeclaratorListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsStructDeclarator())
                {
                    if (IsStructDeclaratorListLine())
                        return true;
                }


            }

            return true;
        }

        public bool IsStructDeclarator()
        {
            if (IsDeclarator())
            {
                if (IsToken(Token.COLLON))
                {
                    ReadToken();

                    if (IsLogicalOrExpression())
                        return true;
                    else
                    {

                        return false;
                    }
                }

                return true;
            }

            else if (IsToken(Token.COLLON))
            {
                ReadToken();

                if (IsLogicalOrExpression())
                    return true;


            }

            return false;
        }

        public bool IsDeclarator()
        {
            if (IsPointer())
            {
                if (IsDirectDeclarator())
                    return true;
            }

            else if (IsDirectDeclarator())
                return true;

            return false;
        }

        public bool IsPointer()
        {
            if (IsToken(Token.PRODUCT))
            {
                ReadToken();

                if (IsPointer())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsDirectDeclarator()
        {
            if (IsToken(Token.IDENTIFIER))
            {
                CheckIdentifierDeclaration();
                ReadToken();

                if (IsDirectDeclaratorLine())
                    return true;


            }

            else if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsDeclarator())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsDirectDeclaratorLine())
                            return true;

                    }
                }


            }

            return false;
        }

        public bool IsDirectDeclaratorLine()
        {
            if (IsToken(Token.BRACKETOPEN))
            {
                ReadToken();

                if (IsLogicalOrExpression())
                {
                    if (IsToken(Token.BRACKETCLOSE))
                    {
                        ReadToken();

                        if (IsDirectDeclaratorLine())
                            return true;

                    }
                }

                else if (IsToken(Token.BRACKETCLOSE))
                {
                    ReadToken();

                    if (IsDirectDeclaratorLine())
                        return true;

                }

            }

            else if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsParameterTypeList())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsDirectDeclaratorLine())
                            return true;

                    }
                }

                else if (IsIdentifierList())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsDirectDeclaratorLine())
                            return true;

                    }
                }
                else if (IsToken(Token.PARENTHESISCLOSE))
                {
                    ReadToken();

                    if (IsDirectDeclaratorLine())
                        return true;

                }

            }

            return true;
        }

        public bool IsLogicalOrExpression()
        {
            if (IsLogicalAndExpression())
                if (IsLogicalOrExpressionLine())
                    return true;

            return false;
        }

        public bool IsLogicalOrExpressionLine()
        {
            if (IsToken(Token.LOGICALOR))
            {
                ReadToken();

                if (IsLogicalAndExpression())
                    if (IsLogicalOrExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsLogicalAndExpression()
        {
            if (IsInclusiveOrExpression())
                if (IsLogicalAndExpressionLine())
                    return true;

            return false;
        }

        public bool IsLogicalAndExpressionLine()
        {
            if (IsToken(Token.LOGICALAND))
            {
                ReadToken();

                if (IsInclusiveOrExpression())
                    if (IsLogicalAndExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsInclusiveOrExpression()
        {
            if (IsExclusiveOrExpression())
                if (IsInclusiveOrExpressionLine())
                    return true;

            return false;
        }

        public bool IsInclusiveOrExpressionLine()
        {
            if (IsToken(Token.OR))
            {
                ReadToken();

                if (IsExclusiveOrExpression())
                    if (IsInclusiveOrExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsExclusiveOrExpression()
        {
            if (IsAndExpression())
                if (IsExclusiveOrExpressionLine())
                    return true;

            return false;
        }

        public bool IsExclusiveOrExpressionLine()
        {
            if (IsToken(Token.XOR))
            {
                ReadToken();

                if (IsAndExpression())
                    if (IsExclusiveOrExpressionLine())
                        return true;
                    else 
                        return false;

            }

            return true;
        }

        public bool IsAndExpression()
        {
            if (IsEqualityExpression())
                if (IsAndExpressionLine())
                    return true;

            return false;
        }

        public bool IsAndExpressionLine()
        {
            if (IsToken(Token.AND))
            {
                ReadToken();

                if (IsAndExpressionLine())
                    return true;


            }

            return true;
        }

        public bool IsEqualityExpression()
        {
            if (IsRelationalExpression())
                if (IsEqualityExpressionLine())
                    return true;

            return false;
        }

        public bool IsEqualityExpressionLine()
        {
            if (IsToken(Token.EQUALS))
            {
                ReadToken();

                if (IsRelationalExpression())
                    if (IsEqualityExpressionLine())
                        return true;

            }

            else if (IsToken(Token.NOTEQUALS))
            {
                ReadToken();

                if (IsRelationalExpression())
                    if (IsEqualityExpressionLine())
                        return true;

            }
            return true;
        }

        public bool IsRelationalExpression()
        {
            if (IsShiftExpression())
                if (IsRelationalExpressionLine())
                    return true;

            return false;
        }

        public bool IsRelationalExpressionLine()
        {
            if (IsToken(Token.LESS))
            {
                ReadToken();

                if (IsShiftExpression())
                    if (IsRelationalExpressionLine())
                        return true;

            }

            else if (IsToken(Token.GREATER))
            {
                ReadToken();

                if (IsShiftExpression())
                    if (IsRelationalExpressionLine())
                        return true;

            }

            else if (IsToken(Token.LESSOREQUAL))
            {
                ReadToken();

                if (IsShiftExpression())
                    if (IsRelationalExpressionLine())
                        return true;

            }

            else if (IsToken(Token.GREATEROREQUAL))
            {
                ReadToken();

                if (IsShiftExpression())
                    if (IsRelationalExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsShiftExpression()
        {
            if (IsAdditiveExpression())
                if (IsShiftExpressionLine())
                    return true;

            return false;
        }

        public bool IsShiftExpressionLine()
        {
            if (IsToken(Token.SHIFTLEFT))
            {
                ReadToken();

                if (IsAdditiveExpression())
                    if (IsShiftExpressionLine())
                        return true;

            }

            else if (IsToken(Token.SHIFTRIGHT))
            {
                ReadToken();

                if (IsAdditiveExpression())
                    if (IsShiftExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsAdditiveExpression()
        {
            if (IsMultiplicativeExpression())
                if (IsAdditiveExpressionLine())
                    return true;

            return false;
        }

        public bool IsAdditiveExpressionLine()
        {
            if (IsToken(Token.PLUS))
            {
                ReadToken();

                if (IsMultiplicativeExpression())
                    if (IsAdditiveExpressionLine())
                        return true;


            }
            else if (IsToken(Token.MINUS))
            {
                ReadToken();

                if (IsMultiplicativeExpression())
                    if (IsAdditiveExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsMultiplicativeExpression()
        {

            if (IsUnaryExpression())
                if (IsMultiplicativeExpressionLine())
                    return true;


            return false;
        }

        public bool IsMultiplicativeExpressionLine()
        {
            if (IsToken(Token.PRODUCT))
            {
                ReadToken();

                if (IsUnaryExpression())
                    if (IsMultiplicativeExpressionLine())
                        return true;

            }

            else if (IsToken(Token.DIVISION))
            {
                ReadToken();

                if (IsUnaryExpression())
                    if (IsMultiplicativeExpressionLine())
                        return true;

            }

            else if (IsToken(Token.MODULE))
            {
                ReadToken();

                if (IsUnaryExpression())
                    if (IsMultiplicativeExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsUnaryExpression()
        {
            if (IsPostfixExpression())
                return true;

            if (IsToken(Token.INCREMENT))
            {
                ReadToken();

                if (IsUnaryExpression())
                    return true;
            }

            if (IsToken(Token.DECREMENT))
            {
                ReadToken();

                if (IsUnaryExpression())
                    return true;
            }

            if (IsUnaryOperator())
                if (IsUnaryExpression())
                    return true;

            return false;
        }

        public bool IsPostfixExpression()
        {
            if (IsPrimaryExpression())
                if (IsPostfixExpressionLine())
                    return true;

            return false;
        }

        public bool IsPostfixExpressionLine()
        {
            if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsArgumentExpressionList())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsPostfixExpressionLine())
                            return true;

                    }
                }

                else if (IsToken(Token.PARENTHESISCLOSE))
                {
                    ReadToken();

                    if (IsPostfixExpressionLine())
                        return true;

                }

            }

            else if (IsToken(Token.DOT))
            {
                ReadToken();

                if (IsToken(Token.IDENTIFIER))
                {
                    CheckIdentifierDeclaration();
                    ReadToken();

                    if (IsPostfixExpressionLine())
                        return true;


                }

            }

            else if (IsToken(Token.STRUCTACCESSOR))
            {
                ReadToken();

                if (IsToken(Token.IDENTIFIER))
                {
                    CheckIdentifierDeclaration();
                    ReadToken();

                    if (IsPostfixExpressionLine())
                        return true;

                }

            }

            else if (IsToken(Token.INCREMENT))
            {
                ReadToken();

                if (IsPostfixExpressionLine())
                    return true;


            }

            else if (IsToken(Token.DECREMENT))
            {
                ReadToken();

                if (IsPostfixExpressionLine())
                    return true;


            }

            return true;
        }

        public bool IsPrimaryExpression()
        {
            if (IsToken(Token.IDENTIFIER))
            {
                CheckIdentifierDeclaration();
                ReadToken();
                return true;
            }

            if (IsToken(Token.INTEGERCONSTANT))
            {
                ReadToken();
                return true;
            }

            if (IsToken(Token.CHARCONSTANT))
            {
                ReadToken();
                return true;
            }

            if (IsToken(Token.FLOATINGPOINTCONSTANT))
            {
                ReadToken();
                return true;
            }

            if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsExpression())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();
                        return true;
                    }
                }

            }

            return false;
        }

        public bool IsExpression()
        {
            if (IsAssignmentExpression())
                if (IsExpressionLine())
                    return true;

            return false;
        }

        public bool IsExpressionLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsAssignmentExpression())
                    if (IsExpressionLine())
                        return true;

            }

            return true;
        }

        public bool IsAssignmentExpression()
        {
            var pos = _state.CurrentTokenIndex;
            if (IsUnaryExpression())
            {
                if (IsAssignmentOperator())
                {
                    var pos_assing = _state.CurrentTokenIndex;

                    if (IsAssignmentExpression())
                        return true;

                    else
                    {
                        _state.CurrentTokenIndex = pos_assing - 1;

                        ReadToken();

                        if (IsLogicalOrExpression())
                            return true;
                    }
                }
            }

            {
                _state.CurrentTokenIndex = pos - 1;
                ReadToken();

                if (IsLogicalOrExpression())
                    return true;
            }

            return false;
        }

        public bool IsAssignmentOperator()
        {
            if (IsToken(Token.ASSIGN)
                || IsToken(Token.PRODUCTASSIGN)
                || IsToken(Token.DIVISIONASSIGN)
                || IsToken(Token.MODULEASSIGN)
                || IsToken(Token.PLUSASSIGN)
                || IsToken(Token.MINUSASSIGN)
                || IsToken(Token.LEFTASSIGN)
                || IsToken(Token.RIGHTASSIGN))
            {
                ReadToken();
                return true;
            }

            return false;
        }

        public bool IsArgumentExpressionList()
        {
            if (IsAssignmentExpression())
                if (IsArgumentExpressionListLine())
                    return true;

            return false;
        }

        public bool IsArgumentExpressionListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsAssignmentExpression())
                    if (IsArgumentExpressionListLine())
                        return true;

            }

            return true;
        }

        public bool IsUnaryOperator()
        {
            if (IsToken(Token.AND)
                || IsToken(Token.PRODUCT)
                || IsToken(Token.PLUS)
                || IsToken(Token.MINUS)
                || IsToken(Token.NEGATE)
                || IsToken(Token.LOGICALNOT))
            {
                ReadToken();
                return true;
            }

            return false;
        }

        public bool IsParameterTypeList()
        {
            if (IsParameterList())
            {
                if (IsToken(Token.COMMA))
                {
                    ReadToken();

                    if (IsToken(Token.ELLIPSIS))
                    {
                        ReadToken();
                        return true;
                    }


                }
                else
                    return true;
            }

            return false;
        }

        public bool IsParameterList()
        {
            if (IsParameterDeclaration())
                if (IsParameterListLine())
                    return true;

            return false;
        }

        public bool IsParameterListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsParameterDeclaration())
                    if (IsParameterListLine())
                        return true;


            }

            return true;
        }

        public bool IsParameterDeclaration()
        {
            if (IsDeclarationSpecifier())
            {
                if (IsDeclarator())
                    return true;

                else if (IsAbstractDeclarator())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsAbstractDeclarator()
        {
            if (IsPointer())
            {
                if (IsDirectAbstractDeclarator())
                    return true;

                return true;
            }

            else if (IsDirectAbstractDeclarator())
                return true;

            return false;
        }

        public bool IsDirectAbstractDeclarator()
        {
            if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsParameterTypeList())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsDirectAbstractDeclaratorLine())
                            return true;

                    }
                }
                else if (IsAbstractDeclarator())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsDirectAbstractDeclaratorLine())
                            return true;


                    }
                }
                else if (IsToken(Token.PARENTHESISCLOSE))
                {
                    ReadToken();

                    if (IsDirectAbstractDeclaratorLine())
                        return true;

                }

            }

            else if (IsToken(Token.BRACKETOPEN))
            {
                ReadToken();

                if (IsLogicalOrExpression())
                {
                    if (IsToken(Token.BRACKETCLOSE))
                    {
                        ReadToken();

                        if (IsDirectAbstractDeclaratorLine())
                            return true;

                    }
                }
                else if (IsToken(Token.BRACKETCLOSE))
                {
                    ReadToken();

                    if (IsDirectAbstractDeclaratorLine())
                        return true;

                }


            }

            return false;
        }

        public bool IsDirectAbstractDeclaratorLine()
        {

            if (IsToken(Token.BRACKETOPEN))
            {
                ReadToken();

                if (IsLogicalOrExpression())
                {
                    if (IsToken(Token.BRACKETCLOSE))
                    {
                        ReadToken();
                        if (IsDirectAbstractDeclaratorLine())
                            return true;

                    }
                }

                else if (IsToken(Token.BRACKETCLOSE))
                {
                    ReadToken();
                    if (IsDirectAbstractDeclaratorLine())
                        return true;

                }

            }

            else if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsParameterTypeList())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();
                        if (IsDirectAbstractDeclaratorLine())
                            return true;

                    }
                }
                
                else if (IsToken(Token.PARENTHESISCLOSE))
                {
                    ReadToken();
                    if (IsDirectAbstractDeclaratorLine())
                        return true;

                }

            }

            return true;
        }

        public bool IsIdentifierList()
        {
            if (IsToken(Token.IDENTIFIER))
            {
                CheckIdentifierDeclaration();
                ReadToken();

                if (IsIdentifierListLine())
                    return true;


            }

            return false;
        }

        public bool IsIdentifierListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsToken(Token.IDENTIFIER))
                {
                    CheckIdentifierDeclaration();
                    ReadToken();

                    if (IsIdentifierListLine())
                        return true;

                }

            }

            return true;
        }

        public bool IsCompoundStatement()
        {
            if (IsToken(Token.BRACEOPEN))
            {
                ReadToken();

                if (IsCompoundBodyList())
                {
                    if (IsToken(Token.BRACECLOSE))
                    {
                        ReadToken();

                        return true;
                    }
                }


            }

            return false;
        }

        public bool IsCompoundBodyList()
        {
            if (IsCompoundBody())
            {
                if (IsCompoundBodyList())
                    return true;

                return false;
            }

            return true;
        }

        public bool IsCompoundBody()
        {
            if (IsDeclarationList())
                return true;

            else if (IsStatementList())
                return true;

            return false;
        }

        public bool IsDeclarationList()
        {
            if (IsDeclaration())
            {
                if (IsDeclarationList())
                    return true;

                //return true;
            }
            return false;
        }

        public bool IsDeclaration()
        {
            if (IsDeclarationSpecifier())
            {
                if (IsToken(Token.SEMICOLLON))
                {
                    ReadToken();
                    return true;
                }

                else if (IsInitDeclaratorList())
                {
                    if (IsToken(Token.SEMICOLLON))
                    {
                        ReadToken();
                        return true;
                    }
                }
            }

            return false;
        }

        public bool IsInitDeclaratorList()
        {
            if (IsInitDeclarator())
                if (IsInitDeclaratorListLine())
                    return true;

            return false;
        }

        public bool IsInitDeclaratorListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsInitDeclarator())
                    if (IsInitDeclaratorListLine())
                        return true;

            }

            return true;
        }

        public bool IsInitDeclarator()
        {
            if (IsDeclarator())
            {
                if (IsToken(Token.ASSIGN))
                {
                    ReadToken();

                    if (IsInitializer())
                        return true;

                }

                return true;
            }

            return false;
        }

        public bool IsInitializer()
        {
            if (IsAssignmentExpression())
                return true;

            else if (IsToken(Token.BRACEOPEN))
            {
                ReadToken();

                if (IsInitializerList())
                {
                    if (IsToken(Token.BRACECLOSE))
                    {
                        ReadToken();
                        return true;
                    }

                    else if (IsToken(Token.COMMA))
                    {
                        ReadToken();

                        if (IsToken(Token.BRACECLOSE))
                        {
                            ReadToken();
                            return true;
                        }

                    }
                }

            }

            return false;
        }

        public bool IsInitializerList()
        {
            if (IsInitializer())
                if (IsInitializerListLine())
                    return true;

            return false;
        }

        public bool IsInitializerListLine()
        {
            if (IsToken(Token.COMMA))
            {
                ReadToken();

                if (IsInitializer())
                    if (IsInitializerListLine())
                        return true;

            }

            return true;
        }

        public bool IsStatementList()
        {
            if (IsStatement())
            {
                if (IsStatementList())
                    return true;

                return true;
            }

            return false;
        }

        public bool IsStatement()
        {
            if (IsLabeledStatement()
               || IsCompoundStatement()
               || IsExpressionStatement()
               || IsSelectionStatement()
               || IsIterationStatement()
               || IsJumpStatement())
                return true;

            return false;
        }

        public bool IsLabeledStatement()
        {
            if (IsToken(Token.CASE))
            {
                ReadToken();

                if (IsLogicalOrExpression())
                {
                    if (IsToken(Token.COLLON))
                    {
                        ReadToken();

                        if (IsStatement())
                            return true;

                    }
                }

            }

            else if (IsToken(Token.DEFAULT))
            {
                ReadToken();

                if (IsToken(Token.COLLON))
                {
                    ReadToken();

                    if (IsStatement())
                        return true;

                }

            }

            return false;
        }

        public bool IsExpressionStatement()
        {
            if (IsToken(Token.SEMICOLLON))
            {
                ReadToken();
                return true;
            }

            else if (IsExpression())
            {
                if (IsToken(Token.SEMICOLLON))
                {
                    ReadToken();
                    return true;
                }
            }

            return false;
        }

        public bool IsSelectionStatement()
        {
            if (IsToken(Token.IF))
            {
                ReadToken();

                if (IsExpressionStatementStructure())
                {
                    if (IsToken(Token.ELSE))
                    {
                        ReadToken();

                        if (IsStatement())
                            return true;

                    }
                    else
                        return true;
                }

            }

            else if (IsToken(Token.SWITCH))
            {
                ReadToken();

                if (IsExpressionStatementStructure())
                    return true;


            }

            return false;
        }

        public bool IsExpressionStatementStructure()
        {
            if (IsToken(Token.PARENTHESISOPEN))
            {
                ReadToken();

                if (IsExpression())
                {
                    if (IsToken(Token.PARENTHESISCLOSE))
                    {
                        ReadToken();

                        if (IsStatement())
                            return true;

                    }
                }

            }

            return false;
        }

        public bool IsIterationStatement()
        {
            if (IsToken(Token.WHILE))
            {
                ReadToken();

                if (IsExpressionStatementStructure())
                    return true;

            }
            else if (IsToken(Token.DO))
            {
                ReadToken();

                if (IsStatement())
                {
                    if (IsToken(Token.WHILE))
                    {
                        ReadToken();

                        if (IsToken(Token.PARENTHESISOPEN))
                        {
                            ReadToken();

                            if (IsExpression())
                            {
                                if (IsToken(Token.PARENTHESISCLOSE))
                                {
                                    ReadToken();

                                    if (IsToken(Token.SEMICOLLON))
                                    {
                                        ReadToken();
                                        return true;
                                    }

                                }
                            }

                        }

                    }
                }

            }
            else if (IsToken(Token.FOR))
            {
                ReadToken();
                if (IsToken(Token.PARENTHESISOPEN))
                {
                    ReadToken(); 
                    
                    if (IsExpressionStatement())
                    {
                        if (IsExpressionStatement())
                        {
                            if (IsExpression())
                            {
                                if (IsToken(Token.PARENTHESISCLOSE))
                                {
                                    ReadToken();

                                    if (IsStatement())
                                        return true;

                                }
                            }
                            else if (IsToken(Token.PARENTHESISCLOSE))
                            {
                                ReadToken();

                                if (IsStatement())
                                    return true;


                            }
                        }
                    }

                    else if (IsExpressionStatement())
                    {
                        if (IsExpressionStatement())
                        {
                            if (IsExpression())
                            {
                                if (IsToken(Token.PARENTHESISCLOSE))
                                {
                                    ReadToken();

                                    if (IsStatement())
                                        return true;

                                }
                            }
                            else if (IsToken(Token.PARENTHESISCLOSE))
                            {
                                ReadToken();

                                if (IsStatement())
                                    return true;


                            }
                        }
                    }

                }
            }

            return false;
        }

        public bool IsJumpStatement()
        {
            if (IsToken(Token.CONTINUE))
            {
                ReadToken();

                if (IsToken(Token.SEMICOLLON))
                {
                    ReadToken();
                    return true;
                }

            }
            else if (IsToken(Token.BREAK))
            {
                ReadToken();

                if (IsToken(Token.SEMICOLLON))
                {
                    ReadToken();
                    return true;
                }



            }
            else if (IsToken(Token.RETURN))
            {
                ReadToken();
                if (IsToken(Token.SEMICOLLON))
                {
                    ReadToken();
                    return true;
                }
                else if (IsExpression())
                {
                    if (IsToken(Token.SEMICOLLON))
                    {
                        ReadToken();
                        return true;
                    }
                }

            }

            return false;
        }
    }
}
