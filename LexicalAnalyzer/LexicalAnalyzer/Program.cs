using Analyzer;
using System;
using System.IO;

namespace Analyzer
{
    public class Program
    {
        static void Main(string[] args)
        {
            using var input = new StreamReader(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"declarationtest.txt"));

            using var resultFile = File.CreateText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), $"{DateTime.Now:yyyyMMddhhmmss}.txt"));

            var lexicalAnalyser = new LexicalAnalyzer();

            var lexicalResult = lexicalAnalyser.Analyze(new LexicalAnalyzerState(input, resultFile));

            var syntaxAnalyser = new SyntaxAnalyzer(new SyntaxAnalyzerState(lexicalResult));

            //var syntaxResult = 
            syntaxAnalyser.Analyze();
        }
    }
}