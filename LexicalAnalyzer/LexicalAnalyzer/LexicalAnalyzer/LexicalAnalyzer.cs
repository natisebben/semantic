using Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer
{
    public class LexicalAnalyzer
    {
        public LexicalAnalyzer()
        {

        }

        public List<Result> Analyze(LexicalAnalyzerState state)
        {
            var analysisResult = new List<Result>();

            if (state.AnalyzeStream == null)
                return null;

            state.WriteHeader();

            do
            {
                state.ReadNextLine();

                while (!state.EOF && state.ColumnIndex < state.CurrentLine.Length - 1)
                {
                    state.ReadNextChar();

                    var result = CheckToken(state);

                    if (result is null)
                    {
                        continue;
                    }
                    else
                    {
                        //state.WriteTokenResult(result);
                        analysisResult.Add(result);
                        state.ResetCurrentToken();
                    }
                }

            } while (!state.EOF);

            //state.WriteTokenResult(new Result(Token.EOF, string.Empty, state.LineIndex, state.ColumnIndex), true);
            analysisResult.Add(new Result(Token.EOF, string.Empty, state.LineIndex, state.ColumnIndex));
            return analysisResult;
        }

        private Result CheckToken(LexicalAnalyzerState state)
        {
            switch (state.CurrentTokenType)
            {
                case TokenType.InitialSymbol:
                    return CheckInitialSymbolToken(state);

                case TokenType.Word:
                    return CheckWordToken(state);

                case TokenType.Number:
                    return CheckNumberToken(state);

                default:
                    return default;
            }
        }

        private Result CheckInitialSymbolToken(LexicalAnalyzerState state)
        {
            if (state.CurrentChar == ' ' || 
                state.CurrentChar == '\0' ||
                state.CurrentChar == '\t' || 
                state.CurrentChar == '\n')
            {
                state.ResetCurrentToken();
                return default;
            }
            else if (char.IsLetter(state.CurrentChar) ||
                     state.CurrentChar == '_' ||
                     state.CurrentChar == '\'')
            {
                state.DefineTokenType(TokenType.Word);
                return default;
            }
            else if (char.IsDigit(state.CurrentChar))
            {
                state.DefineTokenType(TokenType.Number);
                return default;
            }
            else
            {
                return CheckSymbolToken(state);
            }
        }

        private Result CheckSymbolToken(LexicalAnalyzerState state)
        {
            switch (state.CurrentChar)
            {
                case '(':
                    return GetTokenResult(state, Token.PARENTHESISOPEN);
                case ')':
                    return GetTokenResult(state, Token.PARENTHESISCLOSE);
                case '[':
                    return GetTokenResult(state, Token.BRACKETOPEN);
                case ']':
                    return GetTokenResult(state, Token.BRACKETCLOSE);
                case '{':
                    return GetTokenResult(state, Token.BRACEOPEN);
                case '}':
                    return GetTokenResult(state, Token.BRACECLOSE);
                case ';':
                    return GetTokenResult(state, Token.SEMICOLLON);
                case ',':
                    return GetTokenResult(state, Token.COMMA);
                case '~':
                    return GetTokenResult(state, Token.NEGATE);
                case '^':
                    return GetTokenResult(state, Token.XOR);
                case ':':
                    return GetTokenResult(state, Token.COLLON);
                case '.':
                    return GetTokenResult(state, Token.DOT);
                case '=':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.ASSIGN,
                        new List<Token> { Token.EQUALS });
                case '!':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.LOGICALNOT,
                        new List<Token> { Token.NOTEQUALS });
                case '>':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.GREATER,
                        new List<Token> { Token.GREATEROREQUAL, Token.SHIFTRIGHT });
                case '<':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.LESS,
                        new List<Token> { Token.LESSOREQUAL, Token.SHIFTLEFT });
                case '+':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.PLUS,
                        new List<Token> { Token.PLUSASSIGN, Token.INCREMENT });
                case '-':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.MINUS,
                        new List<Token> { Token.MINUSASSIGN, Token.DECREMENT, Token.STRUCTACCESSOR });
                case '/':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.DIVISION,
                        new List<Token> { Token.DIVISIONASSIGN });
                case '*':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.PRODUCT,
                        new List<Token> { Token.PRODUCTASSIGN });
                case '%':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.MODULE,
                        new List<Token> { Token.MODULEASSIGN });
                case '|':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.OR,
                        new List<Token> { Token.LOGICALOR });
                case '&':
                    return GetInitialSymbolTokenResultCheckingPossibilities(
                        state,
                        Token.AND,
                        new List<Token> { Token.LOGICALAND });
                default:
                    return default;
            }
        }

        private Result CheckWordToken(LexicalAnalyzerState state)
        {
            if (char.IsLetter(state.CurrentChar) ||
                state.CurrentChar == '_' || 
                char.IsDigit(state.CurrentChar))
            {
                return default;
            }
            if(state.CurrentToken.First() == '\'' &&
               state.CurrentToken.Last() != '\'')
            {
                return default;
            }
            else if(state.CurrentToken.Length == 3 &&
                    state.CurrentToken.First().ToString().Equals("'") &&
                    state.CurrentToken.Last().ToString().Equals("'") &&
                    char.TryParse(state.CurrentToken.ElementAt(1).ToString(), out _))
            {
                state.ReadNextChar();

                if (state.CurrentChar != '\'')
                {
                    state.UnreadLastChar();
                }
                return GetTokenResult(state, Token.CHARCONSTANT);

                state.UnreadLastChar();
            }
            else if(state.CurrentToken.Length == 4 &&
                    state.CurrentToken.ElementAt(1).ToString().Equals(@"\") &&
                    state.CurrentToken.First().ToString().Equals("'") &&
                    state.CurrentToken.Last().ToString().Equals("'") &&
                    char.TryParse(state.CurrentToken.ElementAt(2).ToString(), out _))
            {
                if (state.CurrentToken != "'\\n'" &&
                    state.CurrentToken != "'\\t'")
                {
                    state.CleanScapeBar();
                }
                return GetTokenResult(state, Token.CHARCONSTANT);
            }
            else
            {
                state.UnreadLastChar();
                state.DefineTokenType(TokenType.InitialSymbol);

                if (Enum.IsDefined(typeof(Token), state.CurrentToken.ToUpper()))
                {
                    return GetTokenResult(state, (Token)Enum.Parse(typeof(Token), state.CurrentToken.ToUpper()));
                }
                else
                {
                    return GetTokenResult(state, Token.IDENTIFIER);
                }
            }
        }

        private Result CheckNumberToken(LexicalAnalyzerState state)
        {
            if (char.IsDigit(state.CurrentChar))
            {
                return default;
            }
            else if (state.CurrentChar == '.')
            {
                if (!state.HasPoint)
                {
                    state.TokenNumberWithPoint(true);
                    return default;
                }
                else
                {
                    state.DefineTokenType(TokenType.InitialSymbol);

                    return GetTokenResult(state, Token.LEXICALERROR);
                }
            }
            else
            {
                state.UnreadLastChar();
                state.DefineTokenType(TokenType.InitialSymbol);
                state.TokenNumberWithPoint(false);

                var token =
                    int.TryParse(state.CurrentToken, out _) ? Token.INTEGERCONSTANT :
                    float.TryParse(state.CurrentToken, out _) ? Token.FLOATINGPOINTCONSTANT :
                    Token.LEXICALERROR;

                return GetTokenResult(state, token);
            }
        }

        private Result GetInitialSymbolTokenResultCheckingPossibilities(LexicalAnalyzerState state, Token originalToken, List<Token> possibleTokens)
        {
            foreach (var possibleToken in possibleTokens)
            {
                state.ReadNextChar();

                if (state.CurrentToken == possibleToken.GetEnumDisplayName())
                    return GetTokenResult(state, possibleToken);

                state.UnreadLastChar();
            }
            return GetTokenResult(state, originalToken);
        }

        private Result GetTokenResult(LexicalAnalyzerState state, Token token)
        {
            var result = new Result(token, state.CurrentToken, state.LineIndex, state.ColumnIndex);
            state.ResetCurrentToken();
            return result;
        }
    }
}
