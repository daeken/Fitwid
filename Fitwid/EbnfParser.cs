using System;
using System.Collections.Generic;
using System.Linq;
using PrettyPrinter;

namespace Fitwid {
	public class EbnfParser {
		public abstract class EbnfExpression {}

		public class EbnfElement : EbnfExpression {
			public string Name;
			public EbnfExpression Expression;
		}

		public class EbnfZeroOrMore : EbnfExpression {
			public EbnfExpression Element;
		}
		
		public class EbnfOneOrMore : EbnfExpression {
			public EbnfExpression Element;
		}

		public class EbnfOptional : EbnfExpression {
			public EbnfExpression Expression;
		}

		public class EbnfIdentifier : EbnfExpression {
			public string Name;
		}
		
		public class EbnfEnd : EbnfExpression {}

		public class EbnfString : EbnfExpression {
			public string Raw;
		}

		public class EbnfRegex : EbnfExpression {
			public string Raw;
		}
		
		public class EbnfSequence : EbnfExpression {
			public List<EbnfExpression> Items;
		}
		
		public class EbnfChoice : EbnfExpression {
			public List<EbnfExpression> Choices;
		}
		
		public class EbnfRule {
			public string Name;
			public EbnfExpression Expression;
		}
		
		static readonly Grammar Grammar;

		static EbnfParser() {
			var (_Expression, __Expression_body) = Patterns.Forward();
			var _Identifier = Patterns.Memoize(Patterns.NamedPattern("Identifier", Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^[a-zA-Z_][a-zA-Z0-9_]*"))));
			var _Group = Patterns.Memoize(Patterns.NamedPattern("Group", Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("(")), Patterns.Named("Expression", _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(")")))));
			var _Optional = Patterns.Memoize(Patterns.NamedPattern("Optional", Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("[")), Patterns.Named("Expression", _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("]")))));
			var _StringLiteral = Patterns.Memoize(Patterns.NamedPattern("StringLiteral", Patterns.IgnoreLeadingWhitespace(Patterns.Choice(Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^\"([^\"\n]|\\\\\"|\\\\)*\"")), Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^'([^'\n]|\\\\'|\\\\)*'"))))));
			var _RegexLiteral = Patterns.Memoize(Patterns.NamedPattern("RegexLiteral", Patterns.IgnoreLeadingWhitespace(Patterns.Regex("^/(\\\\/|\\\\\\\\|[^/\n])+/[imsx]*"))));
			var _End = Patterns.Memoize(Patterns.NamedPattern("End", Patterns.IgnoreLeadingWhitespace(Patterns.Literal("$"))));
			var _ZeroOrMore = Patterns.Memoize(Patterns.NamedPattern("ZeroOrMore", Patterns.IgnoreLeadingWhitespace(Patterns.Literal("*"))));
			var _OneOrMore = Patterns.Memoize(Patterns.NamedPattern("OneOrMore", Patterns.IgnoreLeadingWhitespace(Patterns.Literal("+"))));
			var _Element = Patterns.Memoize(Patterns.NamedPattern("Element", Patterns.LooseSequence(Patterns.Named("Name", Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.LooseSequence(_Identifier, Patterns.IgnoreLeadingWhitespace(Patterns.Literal(":")))))), Patterns.Named("Body", Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Group, _Optional, _Identifier, _StringLiteral, _RegexLiteral, _End))), Patterns.Named("Modifiers", Patterns.IgnoreLeadingWhitespace(Patterns.Optional(Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_ZeroOrMore, _OneOrMore))))))));
			var _Sequence = Patterns.Memoize(Patterns.NamedPattern("Sequence", Patterns.OneOrMore(_Element)));
			var _Choice = Patterns.Memoize(Patterns.NamedPattern("Choice", Patterns.LooseSequence(_Sequence, Patterns.OneOrMore(Patterns.LooseSequence(Patterns.IgnoreLeadingWhitespace(Patterns.Literal("|")), _Sequence)))));
			__Expression_body.Value = Patterns.Memoize(Patterns.NamedPattern("Expression", Patterns.IgnoreLeadingWhitespace(Patterns.Choice(_Choice, _Sequence))));
			var _Rule = Patterns.Memoize(Patterns.NamedPattern("Rule", Patterns.LooseSequence(Patterns.Named("Name", _Identifier), Patterns.IgnoreLeadingWhitespace(Patterns.Literal("=")), Patterns.Named("Expression", _Expression), Patterns.IgnoreLeadingWhitespace(Patterns.Literal(";")))));
			var _Start = Patterns.Memoize(Patterns.NamedPattern("Start", Patterns.LooseSequence(Patterns.ZeroOrMore(_Rule), Patterns.IgnoreLeadingWhitespace(Patterns.End))));
			Grammar = new Grammar(_Start);
		}

		public static List<EbnfRule> Parse(string code) {
			var gast = Grammar.Parse(code);
			if(gast == null) return null;

			var rules = new List<EbnfRule>();

			foreach(var elem in gast.Value[0]) {
				rules.Add(new EbnfRule {
					Name = (string) elem.Elements["Name"].Value, 
					Expression = ParseExpression(elem.Elements["Expression"])
				});
			}
			
			return rules;
		}

		static EbnfExpression ParseExpression(Patterns.NamedAst elem) {
			switch(elem.Name) {
				case "Expression":
					return ParseExpression(elem.Value);
				case "Sequence":
					return ParseSequence(elem);
				case "Choice":
					return ParseChoice(elem);
				case "Identifier":
					return new EbnfIdentifier { Name = elem.Value };
				case "StringLiteral":
					return new EbnfString { Raw = elem.Value };
				case "Group":
					return ParseExpression(elem.Elements["Expression"]);
				case "Optional":
					return new EbnfOptional { Expression = ParseExpression(elem.Elements["Expression"]) };
				case "RegexLiteral":
					return new EbnfRegex { Raw = elem.Value };
				case "End":
					return new EbnfEnd();
				default:
					elem.Print();
					return null;
			}
		}

		static EbnfExpression ParseSequence(Patterns.NamedAst elem) {
			var sub = new List<Patterns.NamedAst>();
			foreach(var selem in elem.Value)
				sub.Add(selem);
			if(sub.Count == 1)
				return ParseElement(sub[0]);
			return new EbnfSequence { Items = sub.Select(ParseElement).ToList() };
		}

		static EbnfExpression ParseChoice(Patterns.NamedAst elem) {
			var sub = new List<Patterns.NamedAst> { elem.Value[0] };
			foreach(var selem in elem.Value[1])
				sub.Add(selem[1]);
			return new EbnfChoice { Choices = sub.Select(ParseSequence).ToList() };
		}

		static EbnfExpression ParseElement(Patterns.NamedAst elem) {
			var expr = ParseExpression((Patterns.NamedAst) elem.Elements["Body"]);
			if(elem.Elements["Modifiers"] != null && elem.Elements["Modifiers"].Name == "ZeroOrMore")
				expr = new EbnfZeroOrMore { Element = expr };
			else if(elem.Elements["Modifiers"] != null && elem.Elements["Modifiers"].Name == "OneOrMore")
				expr = new EbnfOneOrMore { Element = expr };
			return elem.Elements["Name"]?[0] == null
				? expr
				: new EbnfElement {
					Name = (string) elem.Elements["Name"]?[0].Value,
					Expression = expr
				};
		}
	}
}