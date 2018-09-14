using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Fitwid;
using MoreLinq.Extensions;
using PrettyPrinter;

namespace ParserCompiler {
	class Program {
		static string CurrentRuleName;
		static string ClassName = "Ast";
		static string StartRuleName;
		
		static void Main(string[] args) {
			var ast = EbnfParser.Parse(File.ReadAllText("ebnfGrammar.ebnf"));
			//((object) ast).Print();
			if(ast == null)
				return;

			var ns = "Generated";
			if(ast.TypeDecl != null) {
				var td = (string) ((EbnfParser.TypeDecl) ast.TypeDecl).Name;
				var i = td.LastIndexOf('.');
				ClassName = i == -1 ? td : td.Substring(i + 1);
				if(i != -1)
					ns = td.Substring(0, i);
			}

			Console.WriteLine($"namespace {ns} {{");
			Console.WriteLine($"\tpublic abstract partial class {ClassName} {{");
			var byValue = new List<string>();
			string NewIf(string name) => ast.Rules.Count(x => (string) x.Name == name) != 0 ? "new " : "";
			ast.Rules.ForEach(rule => {
				Console.WriteLine($"\t\tpublic partial class {(string) rule.Name} : {ClassName} {{");
				var names = GetNamedElements((EbnfParser) rule.Expression).ToList();
				if(names.Count == 0) {
					byValue.Add((string) rule.Name);
					Console.WriteLine($"\t\t\tpublic {NewIf("Value")}dynamic Value;");
				} else
					foreach(var name in names)
						Console.WriteLine($"\t\t\tpublic {NewIf(name)}dynamic {name};");
				Console.WriteLine("\t\t}");
			});
			
			var deps = ast.Rules.Select(x => ((string) x.Name, ((IEnumerable<string>) FindDeps(x.Expression)).Distinct().ToList())).ToDictionary(x => x.Item1, x => x.Item2);
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

			StartRuleName = (string) ast.Rules[0].Name;
			BuildOrder(StartRuleName, new List<string>());
			Console.WriteLine("\t\tstatic readonly Grammar Grammar;");
			Console.WriteLine($"\t\tstatic {ClassName}() {{");
			foreach(var elem in forward)
				Console.WriteLine($"\t\t\tvar (_{elem}, __{elem}_body) = Patterns.Forward();");
			foreach(var elem in order) {
				CurrentRuleName = elem;
				var body = Generate(ast.Rules.First(x => x.Name == elem).Expression);
				if(byValue.Contains(elem))
					body = $"Patterns.With<{ClassName}.{elem}>((x, d) => x.Value = d, {body})";
				body = $"Patterns.Memoize(Patterns.Bind<{ClassName}.{elem}>({body}))";
				if(forward.Contains(elem))
					Console.WriteLine($"\t\t\t__{elem}_body.Value = {body};");
				else
					Console.WriteLine($"\t\t\tvar _{elem} = {body};");
			}
			Console.WriteLine($"\t\t\tGrammar = new Grammar(_{StartRuleName});");
			Console.WriteLine("\t\t}");
			Console.WriteLine($"\t\tpublic static {ClassName}.{StartRuleName} Parse(string input) => ({ClassName}.{StartRuleName}) Grammar.Parse(input);");
			Console.WriteLine("\t}");
			Console.WriteLine("}");
		}

		static string Generate(EbnfParser expr) {
			switch(expr) {
				case EbnfParser.Expression e: return Generate(e.Value);
				case EbnfParser.Group g: return Generate(g.Expression);
				case EbnfParser.Choice c: return $"Patterns.IgnoreLeadingWhitespace(Patterns.Choice({string.Join(", ", c.Choices.Select(Generate))}))";
				case EbnfParser.Sequence s: return s.Items.Count == 1 ? Generate(s.Items[0]) : $"Patterns.LooseSequence({string.Join(", ", s.Items.Select(Generate))})";
				case EbnfParser.Element e:
					var body = Generate(e.Body);
					if(e.Modifiers is EbnfParser.ZeroOrMore)
						body = $"Patterns.ZeroOrMore(Patterns.IgnoreLeadingWhitespace({body}))";
					else if(e.Modifiers is EbnfParser.OneOrMore)
						body = $"Patterns.OneOrMore(Patterns.IgnoreLeadingWhitespace({body}))";
					return e.Name != null ? $"Patterns.With<{ClassName}.{CurrentRuleName}>((x, d) => x.{(string) e.Name[0]} = d, {body})" : body;
				case EbnfParser.Identifier i: return "_" + i;
				case EbnfParser.Optional o: return $"Patterns.IgnoreLeadingWhitespace(Patterns.Optional({Generate(o.Expression)}))";
				case EbnfParser.RegexLiteral r:
					var regex = ((string) r.Value).Substring(1);
					var mpos = regex.LastIndexOf('/');
					// TODO: Process the modifiers and put them into the regex pattern
					regex = regex.Substring(0, mpos);
					return $"Patterns.IgnoreLeadingWhitespace(Patterns.Regex({UnescapeRegex(regex)}))";
				case EbnfParser.StringLiteral s: return $"Patterns.IgnoreLeadingWhitespace(Patterns.Literal({UnescapeString((string) s.Value)}))";
				case EbnfParser.End e: return "Patterns.IgnoreLeadingWhitespace(Patterns.End)";
				default: throw new NotImplementedException(expr.ToPrettyString());
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

		static IEnumerable<string> FindDeps(EbnfParser expr) {
			switch(expr) {
				case EbnfParser.Expression e: return FindDeps(e.Value);
				case EbnfParser.Group g: return FindDeps(g.Expression);
				case EbnfParser.Choice c: return c.Choices.Select(FindDeps).SelectMany(x => x);
				case EbnfParser.Sequence s: return s.Items.Select(FindDeps).SelectMany(x => x);
				case EbnfParser.Element e: return FindDeps(e.Body);
				case EbnfParser.Identifier i: return new string[] { i };
				case EbnfParser.Optional o: return FindDeps(o.Expression);
				case EbnfParser.RegexLiteral r: return new string[0];
				case EbnfParser.StringLiteral s: return new string[0];
				case EbnfParser.End e: return new string[0];
				default: throw new NotImplementedException(expr.ToPrettyString());
			}
		}

		static IEnumerable<string> GetNamedElements(EbnfParser expr) {
			switch(expr) {
				case EbnfParser.Expression e: return GetNamedElements(e.Value);
				case EbnfParser.Choice c: return c.Choices.Select(GetNamedElements).SelectMany(x => x);
				case EbnfParser.Sequence s: return s.Items.Select(GetNamedElements).SelectMany(x => x);
				case EbnfParser.Element e when e.Name != null: return new[] { (string) e.Name[0] }.Concat(GetNamedElements((EbnfParser) e.Body));
				case EbnfParser.Element e: return GetNamedElements((EbnfParser) e.Body);
				case EbnfParser.Identifier i: return new string[0];
				case EbnfParser.Optional o: return GetNamedElements(o.Expression);
				case EbnfParser.RegexLiteral r: return new string[0];
				case EbnfParser.StringLiteral s: return new string[0];
				case EbnfParser.Group g: return GetNamedElements(g.Expression);
				case EbnfParser.End e: return new string[0];
				default: throw new NotImplementedException(expr.ToPrettyString());
			}
		}
	}
}