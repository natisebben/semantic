using Common;
using System.Collections.Generic;
using System.Linq;

namespace Analyzer
{
    public class SyntaxAnalyzerState
    {
        public List<Result> Tokens { get; private set; }
        public Result CurrentToken { get; set; }
        public int CurrentTokenIndex { get; set; }
        public int TokensQuantity { get; private set; }
        public List<Result> DeclarationTokens { get; set; }

        public SyntaxAnalyzerState(List<Result> tokens)
        {
            Tokens = tokens;
            CurrentTokenIndex = -1;
            TokensQuantity = tokens.Count();
            DeclarationTokens = new List<Result>();
        }
    }
}