using System.Text.RegularExpressions;
using static Fitwid.Patterns;

namespace Fitwid {
	public class EbnfParser {
		static readonly Grammar Grammar;

		static EbnfParser() {
			var Identifier = Regex("^[a-zA-Z_][a-zA-Z0-9_]*");
			var DblQuoteStringLiteral = Sequence(Literal("\""), Regex(@"^([^""\n]|\\""|\\)*"), Literal("\""));
			var SingleQuoteStringLiteral = Sequence(Literal("'"), Regex(@"^([^'\\\n]|\\'|\\\\)*"), Literal("'"));
			var StringLiteral = NamedPattern("StringLiteral", Choice(DblQuoteStringLiteral, SingleQuoteStringLiteral));
			var RegexLiteral = NamedPattern("RegexLiteral", Sequence(Literal("/"), Regex(@"^([^/\\\n]|\\/|\\\\)*"), Literal("/")));
			var Element = Choice(Identifier, StringLiteral, RegexLiteral);
			var SequenceSyntax = NamedPattern("SequenceSyntax", OneOrMore(IgnoreLeadingWhitespace(Element)));
			var ChoiceSyntax = NamedPattern("ChoiceSyntax", LooseSequence(SequenceSyntax, OneOrMore(LooseSequence(Literal("|"), SequenceSyntax))));
			var Expression = Choice(ChoiceSyntax, SequenceSyntax);
			var Rule = NamedPattern("Rule", LooseSequence(Named("Name", Identifier), Literal("="), Named("Expression", Expression), Literal(";")));
			var Start = LooseSequence(ZeroOrMore(Rule), End);
			Grammar = new Grammar(Start);
		}

		public static dynamic Parse(string code) =>
			Grammar.Parse(code);
	}
}