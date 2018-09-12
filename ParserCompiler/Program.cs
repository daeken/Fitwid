using System;
using Fitwid;
using PrettyPrinter;

namespace ParserCompiler {
	class Program {
		static void Main(string[] args) {
			((object) EbnfParser.Parse(@"test = 'testing!'; foo = test | test | foo;")).PrettyPrint();
		}
	}
}