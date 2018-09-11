using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Fitwid {
	public static class Patterns {
		static readonly Dictionary<object, Pattern> PatternCache = new Dictionary<object, Pattern>();

		static Pattern Cache(object key, Pattern sub) =>
			PatternCache.ContainsKey(key)
				? PatternCache[key]
				: PatternCache[key] = sub;

		public static Pattern Memoize(Pattern sub) =>
			text =>
				text.Memoization.ContainsKey((sub, text.Start))
					? text.Memoization[(sub, text.Start)]
					: text.Memoization[(sub, text.Start)] = sub(text);
		
		public static Pattern End() =>
			text => text.Length == 0 ? (text, null) : ((Bobbin, dynamic)?) null;

		public static Pattern PositiveLookahead(Pattern sub) =>
			Cache(sub, text => sub(text) != null ? (text, null) : ((Bobbin, dynamic)?) null);
		
		public static Pattern NegativeLookahead(Pattern sub) =>
			Cache(sub, text => sub(text) == null ? (text, null) : ((Bobbin, dynamic)?) null);
		
		public static Pattern IgnoreLeadingWhitespace(Pattern sub) =>
			Cache(sub, text => {
				var i = text.Start;
				for(; i < text.End; ++i)
					if(!char.IsWhiteSpace(text.String[i]))
						break;
				return sub(text.Forward(i - text.Start));
			});
		
		public static Pattern Sequence(params Pattern[] elems) =>
			Cache(elems, text => {
				var list = new List<dynamic>();
				foreach(var elem in elems) {
					var match = elem(text);
					if(match == null) return null;
					text = match.Value.Item1;
					list.Add(match.Value.Item2);
				}
				return (text, list);
			});
		
		public static Pattern Choice(params Pattern[] opt) => Cache(opt, text => opt.Select(x => x(text)).FirstOrDefault(x => x != null));

		public static Pattern ZeroOrMore(Pattern sub) =>
			Cache(sub, text => {
				var list = new List<dynamic>();
				while(text.Length != 0) {
					var match = sub(text);
					if(match == null) break;
					text = match.Value.Item1;
					list.Add(match.Value.Item2);
				}
				return (text, list);
			});

		public static Pattern OneOrMore(Pattern sub) =>
			Cache(sub, text => {
				var list = new List<dynamic>();
				while(text.Length != 0) {
					var match = sub(text);
					if(match == null) break;
					text = match.Value.Item1;
					list.Add(match.Value.Item2);
				}

				if(list.Count == 0) return null;
				return (text, list);
			});

		public static Pattern Literal(string val) =>
			Cache(val, text => text.ToString().StartsWith(val) ? (text.Forward(val.Length), val) : ((Bobbin, dynamic)?) null);

		public static Pattern Regex(Regex regex) =>
			Cache(regex, text => {
				var match = regex.Match(text.String, text.Start);
				return match.Success ? (text.Forward(match.Length), match.Value) : ((Bobbin, dynamic)?) null;
			});
	}
}