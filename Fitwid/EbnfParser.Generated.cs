using Fitwid;
using System.Collections.Generic;
namespace Fitwid {
	public abstract partial class EbnfParser {
		public partial class Start : EbnfParser {
			public new EbnfParser.TypeDecl TypeDecl;
			public List<EbnfParser.Rule> RuleDefs;
		}
		public partial class TypeDecl : EbnfParser {
			public string Name;
		}
		public partial class Rule : EbnfParser {
			public string Name;
			public EbnfParser Expression;
		}
		public partial class Choice : EbnfParser {
			public List<EbnfParser.Sequence> Choices;
		}
		public partial class Sequence : EbnfParser {
			public List<EbnfParser.Element> Value;
		}
		public partial class Element : EbnfParser {
			public string Name;
			public EbnfParser Body;
			public EbnfParser Modifiers;
		}
		public partial class RuleReference : EbnfParser {
			public string Name;
		}
		public partial class End : EbnfParser {
			public string Value;
		}
		public partial class Optional : EbnfParser {
			public EbnfParser Expression;
		}
		public partial class StringLiteral : EbnfParser {
			public string Value;
		}
		public partial class RegexLiteral : EbnfParser {
			public string Value;
		}
		public partial class ZeroOrMore : EbnfParser {
			public string Value;
		}
		public partial class OneOrMore : EbnfParser {
			public string Value;
		}
		static readonly Grammar Grammar;
		static EbnfParser() {
			var (_Expression, __Expression_body) = Patterns.Forward();
			var _TypeDecl = Patterns.Memoize(Patterns.Bind<EbnfParser.TypeDecl>(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(string), typeof(string)  }, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("%class")), Patterns.With<EbnfParser.TypeDecl>((x, d) => x.Name = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_.]*"))), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Identifier = Patterns.Memoize(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_]*")));
			var _ElementIdentifier = Patterns.Memoize(Patterns.PopValue(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(string)  }, Patterns.PushValue(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^([a-zA-Z_][a-zA-Z0-9_]*|@)\\+?"))), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(":")))));
			var _Group = Patterns.Memoize(Patterns.PopValue(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(EbnfParser), typeof(string)  }, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("(")), Patterns.PushValue(_Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(")")))));
			var _Optional = Patterns.Memoize(Patterns.Bind<EbnfParser.Optional>(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(EbnfParser), typeof(string)  }, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("[")), Patterns.With<EbnfParser.Optional>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("]")))));
			var _RuleReference = Patterns.Memoize(Patterns.Bind<EbnfParser.RuleReference>(Patterns.With<EbnfParser.RuleReference>((x, d) => x.Name = d, _Identifier)));
			var _StringLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.StringLiteral>(Patterns.With<EbnfParser.StringLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^\"([^\"\n]|\\\\\"|\\\\)*\"")), Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^'([^'\n]|\\\\'|\\\\)*'")))))));
			var _RegexLiteral = Patterns.Memoize(Patterns.Bind<EbnfParser.RegexLiteral>(Patterns.With<EbnfParser.RegexLiteral>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^/(\\\\/|\\\\\\\\|[^/\n])+/[imsx]*")))));
			var _End = Patterns.Memoize(Patterns.Bind<EbnfParser.End>(Patterns.With<EbnfParser.End>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("$")))));
			var _ZeroOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.ZeroOrMore>(Patterns.With<EbnfParser.ZeroOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("*")))));
			var _OneOrMore = Patterns.Memoize(Patterns.Bind<EbnfParser.OneOrMore>(Patterns.With<EbnfParser.OneOrMore>((x, d) => x.Value = d, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("+")))));
			var _Element = Patterns.Memoize(Patterns.Bind<EbnfParser.Element>(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(EbnfParser), typeof(EbnfParser)  }, Patterns.With<EbnfParser.Element>((x, d) => x.Name = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(_ElementIdentifier))), Patterns.With<EbnfParser.Element>((x, d) => x.Body = d, Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Group, _Optional, _RuleReference, _StringLiteral, _RegexLiteral, _End))), Patterns.With<EbnfParser.Element>((x, d) => x.Modifiers = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_ZeroOrMore, _OneOrMore))))))));
			var _Sequence = Patterns.Memoize(Patterns.Bind<EbnfParser.Sequence>(Patterns.With<EbnfParser.Sequence>((x, d) => x.Value = d, Patterns.OneOrMore<EbnfParser.Element>(Patterns.IgnoreLeadingWhitespace(_Element)))));
			var _Choice = Patterns.Memoize(Patterns.Bind<EbnfParser.Choice>(Patterns.TupleLooseSequence(new[] { typeof(EbnfParser.Sequence), typeof(List<(string, EbnfParser.Sequence)>)  }, Patterns.With<EbnfParser.Choice>((x, d) => (x.Choices = x.Choices ?? new List<EbnfParser.Sequence>()).Add(d), _Sequence), Patterns.OneOrMore<(string, EbnfParser.Sequence)>(Patterns.IgnoreLeadingWhitespace(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(EbnfParser.Sequence)  }, Patterns.IgnoreLeadingWhitespace(Patterns.Literal("|")), Patterns.With<EbnfParser.Choice>((x, d) => (x.Choices = x.Choices ?? new List<EbnfParser.Sequence>()).Add(d), _Sequence)))))));
			__Expression_body.Value = Patterns.Memoize(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Choice, _Sequence)));
			var _Rule = Patterns.Memoize(Patterns.Bind<EbnfParser.Rule>(Patterns.TupleLooseSequence(new[] { typeof(string), typeof(string), typeof(EbnfParser), typeof(string)  }, Patterns.With<EbnfParser.Rule>((x, d) => x.Name = d, _Identifier), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("=")), Patterns.With<EbnfParser.Rule>((x, d) => x.Expression = d, _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Start = Patterns.Memoize(Patterns.Bind<EbnfParser.Start>(Patterns.TupleLooseSequence(new[] { typeof(EbnfParser.TypeDecl), typeof(List<EbnfParser.Rule>), typeof(object)  }, Patterns.With<EbnfParser.Start>((x, d) => x.TypeDecl = d, Patterns.IgnoreLeadingWhitespace(Patterns.Optional(_TypeDecl))), Patterns.With<EbnfParser.Start>((x, d) => x.RuleDefs = d, Patterns.ZeroOrMore<EbnfParser.Rule>(Patterns.IgnoreLeadingWhitespace(_Rule))), Patterns.IgnoreLeadingWhitespace(Patterns.End))));
			Grammar = new Grammar(_Start);
		}
		public static EbnfParser.Start Parse(string input) => (EbnfParser.Start) Grammar.Parse(input);
	}
}
