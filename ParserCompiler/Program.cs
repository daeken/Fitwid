using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fitwid;
using PrettyPrinter;

namespace ParserCompiler {
	class Program {
		static void Main(string[] args) {
			var ast = EbnfParser.Parse(File.ReadAllText("ebnfGrammar.ebnf"));
			var deps = ast.Select(x => (x.Name, FindDeps(x.Expression).Distinct().ToList())).ToDictionary(x => x.Item1, x => x.Item2);
			var order = new List<string>();
			var forward = new List<string>();

			void BuildOrder(string name, List<string> ancestors) {
				foreach(var dep in deps[name]) {
					if(order.Contains(dep) || forward.Contains(dep)) continue;
					if(ancestors.Contains(dep))
						forward.Add(dep);
					else
						BuildOrder(dep, ancestors.Concat(new[] { name }).ToList());
				}
				order.Add(name);
			}
			BuildOrder(ast[0].Name, new List<string>());
			foreach(var elem in forward)
				Console.WriteLine($"var (_{elem}, __{elem}_body) = Forward();");
			foreach(var elem in order) {
				var body = $"(NamedPattern({elem.ToPrettyString()}, {Generate(ast.First(x => x.Name == elem).Expression)}))";
				if(forward.Contains(elem))
					Console.WriteLine($"__{elem}_body.Value = {body};");
				else
					Console.WriteLine($"var _{elem} = {body};");
			}
		}

		static string Generate(EbnfParser.EbnfExpression expr) {
			switch(expr) {
				case EbnfParser.EbnfChoice c: return $"IgnoreLeadingWhitespace(Choice({string.Join(", ", c.Choices.Select(Generate))}))";
				case EbnfParser.EbnfSequence s: return $"LooseSequence({string.Join(", ", s.Items.Select(Generate))})";
				case EbnfParser.EbnfElement e: return e.Name != null ? $"Named({e.Name.ToPrettyString()}, {Generate(e.Expression)})" : Generate(e.Expression);
				case EbnfParser.EbnfIdentifier i: return "_" + i.Name;
				case EbnfParser.EbnfOptional o: return $"IgnoreLeadingWhitespace(Optional({Generate(o.Expression)}))";
				case EbnfParser.EbnfRegex r: return $"IgnoreLeadingWhitespace(Regex({UnescapeRegex(r.Raw)}))";
				case EbnfParser.EbnfString s: return $"IgnoreLeadingWhitespace(Literal({UnescapeString(s.Raw)}))";
				case EbnfParser.EbnfOneOrMore o: return $"OneOrMore({Generate(o.Element)})";
				case EbnfParser.EbnfZeroOrMore o: return $"ZeroOrMore({Generate(o.Element)})";
				default: throw new NotImplementedException();
			}
		}

		static string UnescapeRegex(string inp) {
			var ret = "^";
			for(var i = 0; i < inp.Length;) {
				switch(inp[i++]) {
					case '\\':
						switch(inp[i++]) {
							case '/': ret += "/"; break;
							case 'n': ret += "\n"; break;
							case 'r': ret += "\r"; break;
							case 't': ret += "\t"; break;
							case '\\': ret += "\\\\"; break;
							case char x: ret += "\\" + x; break;
						}
						break;
					case char x:
						ret += x;
						break;
				}
			}
			return ret.ToPrettyString();
		}

		static string UnescapeString(string inp) {
			inp = inp.Substring(1, inp.Length - 2);

			var ret = "";
			for(var i = 0; i < inp.Length;) {
				switch(inp[i++]) {
					case '\\':
						switch(inp[i++]) {
							case '"': ret += "\""; break;
							case '\'': ret += "'"; break;
							case 'n': ret += "\n"; break;
							case 'r': ret += "\r"; break;
							case 't': ret += "\t"; break;
							case '\\': ret += "\\"; break;
							case char x: throw new NotImplementedException($"Unsupported escape: '{x}'");
						}
						break;
					case char x:
						ret += x;
						break;
				}
			}
			return ret.ToPrettyString();
		}

		static IEnumerable<string> FindDeps(EbnfParser.EbnfExpression expr) {
			switch(expr) {
				case EbnfParser.EbnfChoice c: return c.Choices.Select(FindDeps).SelectMany(x => x);
				case EbnfParser.EbnfSequence s: return s.Items.Select(FindDeps).SelectMany(x => x);
				case EbnfParser.EbnfElement e: return FindDeps(e.Expression);
				case EbnfParser.EbnfIdentifier i: return new[] { i.Name };
				case EbnfParser.EbnfOptional o: return FindDeps(o.Expression);
				case EbnfParser.EbnfRegex r: return new string[0];
				case EbnfParser.EbnfString s: return new string[0];
				case EbnfParser.EbnfOneOrMore o: return FindDeps(o.Element);
				case EbnfParser.EbnfZeroOrMore o: return FindDeps(o.Element);
				default: throw new NotImplementedException();
			}
		}
	}
}