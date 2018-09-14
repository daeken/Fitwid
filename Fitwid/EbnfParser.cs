using System;
using System.Collections.Generic;
using System.Linq;
using PrettyPrinter;
using static Fitwid.Patterns;

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
			var Identifier = Regex("^[a-zA-Z_][a-zA-Z0-9_]*");
			var DblQuoteStringLiteral = Sequence(Literal("\""), Regex(@"^([^""\n]|\\""|\\)*"), Literal("\""));
			var SingleQuoteStringLiteral = Sequence(Literal("'"), Regex(@"^([^'\\\n]|\\'|\\\\)*"), Literal("'"));
			var StringLiteral = NamedPattern("StringLiteral", Choice(DblQuoteStringLiteral, SingleQuoteStringLiteral));
			var RegexLiteral = NamedPattern("RegexLiteral", Sequence(Literal("/"), Regex(@"^(\\/|\\\\|[^/\n])*"), Literal("/")));
			var (Expression, ExpressionBody) = Forward();
			var GroupSyntax = NamedPattern("Group", LooseSequence(Literal("("), Named("Expression", Expression), Literal(")")));
			var OptionalSyntax = NamedPattern("Optional", LooseSequence(Literal("["), Named("Expression", Expression), Literal("]")));
			var Element = NamedPattern("Element", LooseSequence(
				Named("Name", Optional(LooseSequence(Identifier, Literal(":")))), 
				Named("Body", Choice(GroupSyntax, OptionalSyntax, NamedPattern("RuleReference", Identifier), StringLiteral, RegexLiteral)), 
				Named("Modifiers", Optional(Choice(Literal("*"), Literal("+"))))
			));
			var SequenceSyntax = NamedPattern("SequenceSyntax", OneOrMore(IgnoreLeadingWhitespace(Element)));
			var ChoiceSyntax = NamedPattern("ChoiceSyntax", LooseSequence(SequenceSyntax, OneOrMore(LooseSequence(Literal("|"), SequenceSyntax))));
			ExpressionBody.Value = Choice(ChoiceSyntax, SequenceSyntax);
			var Rule = NamedPattern("Rule", LooseSequence(Named("Name", Identifier), Literal("="), Named("Expression", Expression), Literal(";")));
			var _Start = LooseSequence(ZeroOrMore(Rule), End);
			Grammar = new Grammar(_Start);
		}

		public static List<EbnfRule> Parse(string code) {
			var gast = Grammar.Parse(code);
			if(gast == null) return null;

			var rules = new List<EbnfRule>();

			foreach(var elem in gast[0]) {
				rules.Add(new EbnfRule {
					Name = (string) elem.Elements["Name"], 
					Expression = ParseExpression(elem.Elements["Expression"])
				});
			}
			
			return rules;
		}

		static EbnfExpression ParseExpression(NamedAst elem) {
			switch(elem.Name) {
				case "SequenceSyntax":
					return ParseSequence(elem);
				case "ChoiceSyntax":
					return ParseChoice(elem);
				case "RuleReference":
					return new EbnfIdentifier { Name = elem.Value };
				case "StringLiteral":
					return new EbnfString { Raw = string.Join("", elem.Value) };
				case "Group":
					return ParseExpression(elem.Elements["Expression"]);
				case "Optional":
					return new EbnfOptional { Expression = ParseExpression(elem.Elements["Expression"]) };
				case "RegexLiteral":
					return new EbnfRegex { Raw = elem.Value[1] };
				default:
					elem.Print();
					return null;
			}
		}

		static EbnfExpression ParseSequence(NamedAst elem) {
			var sub = new List<NamedAst>();
			foreach(var selem in elem.Value)
				sub.Add(selem);
			if(sub.Count == 1)
				return ParseElement(sub[0]);
			return new EbnfSequence { Items = sub.Select(ParseElement).ToList() };
		}

		static EbnfExpression ParseChoice(NamedAst elem) {
			var sub = new List<NamedAst> { elem.Value[0] };
			foreach(var selem in elem.Value[1])
				sub.Add(selem[1]);
			return new EbnfChoice { Choices = sub.Select(ParseSequence).ToList() };
		}

		static EbnfExpression ParseElement(NamedAst elem) {
			var expr = ParseExpression((NamedAst) elem.Elements["Body"]);
			if(elem.Elements["Modifiers"] == "*")
				expr = new EbnfZeroOrMore { Element = expr };
			else if(elem.Elements["Modifiers"] == "+")
				expr = new EbnfOneOrMore { Element = expr };
			return elem.Elements["Name"]?[0] == null
				? expr
				: new EbnfElement {
					Name = (string) elem.Elements["Name"]?[0],
					Expression = expr
				};
		}
	}
}