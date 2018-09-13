using System;
using System.IO;
using Fitwid;
using PrettyPrinter;

namespace ParserCompiler {
	class Program {
		static void Main(string[] args) {
			((object) EbnfParser.Parse(File.ReadAllText("ebnfGrammar.ebnf"))).PrettyPrint();
		}
	}
}