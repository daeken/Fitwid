using System.Collections.Generic;
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
			public dynamic Expression;
		}
		public partial class Choice : EbnfParser {
			public dynamic Choices;
		}
		public partial class Sequence : EbnfParser {
			public dynamic Value;
		}
		public partial class Element : EbnfParser {
			public dynamic Name;
			public dynamic Body;
			public dynamic Modifiers;
		}
		public partial class RuleReference : EbnfParser {
			public dynamic Name;
		}
		public partial class End : EbnfParser {
			public dynamic Value;
		}
		public partial class Optional : EbnfParser {
			public dynamic Expression;
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
			var _Identifier = Patterns.Memoize(Patterns.PopValue(Patterns.PushValue(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_]*")))));
			var _ElementIdentifier = Patterns.Memoize(Patterns.PopValue(Patterns.LooseSequence(Patterns.PushValue(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^([a-zA-Z_][a-zA-Z0-9_]*|@)\\+?"))), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(":")))));
			var _Group = Patterns.Memoize(Patterns.PopValue(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("(")), Patterns.PushValue(_Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(")")))));
			var _Optional = Patterns.Memoize(Patterns.Bind<EbnfParser.Optional>(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("[")), Patterns.With<EbnfParser.Optional>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("]")))));
			var _RuleReference = Patterns.Memoize(Patterns.Bind<EbnfParser.RuleReference>(Patterns.With<EbnfParser.RuleReference>((x, d) => x.Name = d, _Identifier)));
			var _StringLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.StringLiteral>(Patterns.With<EbnfParser.StringLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^\"([^\"\n]|\\\\\"|\\\\)*\"")), Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^'([^'\n]|\\\\'|\\\\)*'")))))));
			var _RegexLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.RegexLiteral>(Patterns.With<EbnfParser.RegexLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^/(\\\\/|\\\\\\\\|[^/\n])+/[imsx]*")))));
			var _End = Patterns.Memoize(Patterns.Bind<EbnfParser.End>(Patterns.With<EbnfParser.End>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("$")))));
			var _ZeroOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.ZeroOrMore>(Patterns.With<EbnfParser.ZeroOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("*")))));
			var _OneOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.OneOrMore>(Patterns.With<EbnfParser.OneOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("+")))));
			var _Element = Patterns.Memoize(Patterns.Bind<EbnfParser.Element>(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.With<EbnfParser.Element>((x, d) => x.Name = d, _ElementIdentifier))), Patterns.With<EbnfParser.Element>((x, d) => x.Body = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Group, _Optional, _RuleReference, _StringLiteral, _RegexLiteral, _End))), Patterns.With<EbnfParser.Element>((x, d) => x.Modifiers = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_ZeroOrMore, _OneOrMore))))))));
			var _Sequence = Patterns.Memoize(Patterns.Bind<EbnfParser.Sequence>(Patterns.With<EbnfParser.Sequence>((x, d) => x.Value = d, Patterns.OneOrMore(Patterns.IgnoreLeadingWhitespace(_Element)))));
			var _Choice = Patterns.Memoize(Patterns.Bind<EbnfParser.Choice>(Patterns.LooseSequence(Patterns.With<EbnfParser.Choice>((x, d) => (x.Choices = x.Choices ?? new List<dynamic>()).Add(d), _Sequence), Patterns.OneOrMore(Patterns.IgnoreLeadingWhitespace(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("|")), Patterns.With<EbnfParser.Choice>((x, d) => (x.Choices = x.Choices ?? new List<dynamic>()).Add(d), _Sequence)))))));
			__Expression_body.Value = Patterns.Memoize(Patterns.PopValue(Patterns.PushValue(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Choice, _Sequence)))));
			var _Rule = Patterns.Memoize(Patterns.Bind<EbnfParser.Rule>(Patterns.LooseSequence(Patterns.With<EbnfParser.Rule>((x, d) => x.Name = d, _Identifier), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("=")), Patterns.With<EbnfParser.Rule>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Start = Patterns.Memoize(Patterns.Bind<EbnfParser.Start>(Patterns.LooseSequence(Patterns.With<EbnfParser.Start>((x, d) => x.TypeDecl = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(_TypeDecl))), Patterns.With<EbnfParser.Start>((x, d) => x.RuleDefs = d, Patterns.ZeroOrMore(Patterns.IgnoreLeadingWhitespace(_Rule))), Patterns.IgnoreLeadingWhitespace(Patterns.End))));
			Grammar = new Grammar(_Start);
		}
		public static EbnfParser.Start Parse(string input) => (EbnfParser.Start) Grammar.Parse(input);
	}
}
