namespace Fitwid {
	public abstract partial class EbnfParser {
		public partial class Start : EbnfParser {
			public new dynamic TypeDecl;
			public dynamic RuleDefs;
		}
		public partial class TypeDecl : EbnfParser {
			public dynamic Name;
		}
		public partial class Rule : EbnfParser {
			public dynamic Name;
			public new dynamic Expression;
		}
		public partial class Expression : EbnfParser {
			public dynamic Value;
		}
		public partial class Choice : EbnfParser {
			public dynamic Value;
		}
		public partial class Sequence : EbnfParser {
			public dynamic Value;
		}
		public partial class Element : EbnfParser {
			public dynamic Name;
			public dynamic Body;
			public dynamic Modifiers;
		}
		public partial class End : EbnfParser {
			public dynamic Value;
		}
		public partial class Group : EbnfParser {
			public new dynamic Expression;
		}
		public partial class Optional : EbnfParser {
			public new dynamic Expression;
		}
		public partial class Identifier : EbnfParser {
			public dynamic Value;
		}
		public partial class StringLiteral : EbnfParser {
			public dynamic Value;
		}
		public partial class RegexLiteral : EbnfParser {
			public dynamic Value;
		}
		public partial class ZeroOrMore : EbnfParser {
			public dynamic Value;
		}
		public partial class OneOrMore : EbnfParser {
			public dynamic Value;
		}
		static readonly Grammar Grammar;
		static EbnfParser() {
			var (_Expression, __Expression_body) = Patterns.Forward();
			var _TypeDecl = Patterns.Memoize(Patterns.Bind<EbnfParser.TypeDecl>(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("%class")), Patterns.With<EbnfParser.TypeDecl>((x, d) => x.Name = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_.]*"))), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Identifier = Patterns.Memoize(Patterns.Bind<EbnfParser.Identifier>(Patterns.With<EbnfParser.Identifier>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_]*")))));
			var _Group = Patterns.Memoize(Patterns.Bind<EbnfParser.Group>(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("(")), Patterns.With<EbnfParser.Group>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(")")))));
			var _Optional = Patterns.Memoize(Patterns.Bind<EbnfParser.Optional>(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("[")), Patterns.With<EbnfParser.Optional>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("]")))));
			var _StringLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.StringLiteral>(Patterns.With<EbnfParser.StringLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^\"([^\"\n]|\\\\\"|\\\\)*\"")), Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^'([^'\n]|\\\\'|\\\\)*'")))))));
			var _RegexLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.RegexLiteral>(Patterns.With<EbnfParser.RegexLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^/(\\\\/|\\\\\\\\|[^/\n])+/[imsx]*")))));
			var _End = Patterns.Memoize(Patterns.Bind<EbnfParser.End>(Patterns.With<EbnfParser.End>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("$")))));
			var _ZeroOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.ZeroOrMore>(Patterns.With<EbnfParser.ZeroOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("*")))));
			var _OneOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.OneOrMore>(Patterns.With<EbnfParser.OneOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("+")))));
			var _Element = Patterns.Memoize(Patterns.Bind<EbnfParser.Element>(Patterns.LooseSequence(Patterns.With<EbnfParser.Element>((x, d) => x.Name = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.LooseSequence(_Identifier, Patterns.IgnoreLeadingWhitespace(Patterns.Literal(":")))))), Patterns.With<EbnfParser.Element>((x, d) => x.Body = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Group, _Optional, _Identifier, _StringLiteral, _RegexLiteral, _End))), Patterns.With<EbnfParser.Element>((x, d) => x.Modifiers = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_ZeroOrMore, _OneOrMore))))))));
			var _Sequence = Patterns.Memoize(Patterns.Bind<EbnfParser.Sequence>(Patterns.With<EbnfParser.Sequence>((x, d) => x.Value = d, Patterns.OneOrMore(Patterns.IgnoreLeadingWhitespace(_Element)))));
			var _Choice = Patterns.Memoize(Patterns.Bind<EbnfParser.Choice>(Patterns.With<EbnfParser.Choice>((x, d) => x.Value = d, Patterns.LooseSequence(_Sequence, Patterns.OneOrMore(Patterns.IgnoreLeadingWhitespace(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("|")), _Sequence)))))));
			__Expression_body.Value = Patterns.Memoize(Patterns.Bind<EbnfParser.Expression>(Patterns.With<EbnfParser.Expression>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Choice, _Sequence)))));
			var _Rule = Patterns.Memoize(Patterns.Bind<EbnfParser.Rule>(Patterns.LooseSequence(Patterns.With<EbnfParser.Rule>((x, d) => x.Name = d, _Identifier), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("=")), Patterns.With<EbnfParser.Rule>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Start = Patterns.Memoize(Patterns.Bind<EbnfParser.Start>(Patterns.LooseSequence(Patterns.With<EbnfParser.Start>((x, d) => x.TypeDecl = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(_TypeDecl))), Patterns.With<EbnfParser.Start>((x, d) => x.RuleDefs = d, Patterns.ZeroOrMore(Patterns.IgnoreLeadingWhitespace(_Rule))), Patterns.IgnoreLeadingWhitespace(Patterns.End))));
			Grammar = new Grammar(_Start);
		}
		public static EbnfParser.Start Parse(string input) => (EbnfParser.Start) Grammar.Parse(input);
	}
}
