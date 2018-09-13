using System;
using System.Collections.Generic;
using Xunit;
using Fitwid;
using static Fitwid.Patterns;

namespace Tests {
	public class DirectGrammarTests {
		[Fact]
		public void StringLiteral() {
			var grammar = new Grammar(Literal("test"));
			
			Assert.Equal(grammar.Parse("test"), "test");
			Assert.Equal(grammar.Parse("testing", requireAll: false), "test");
			Assert.Null(grammar.Parse("testing"));
			Assert.Null(grammar.Parse("ftest"));
		}

		[Fact]
		public void StringChoice() {
			var grammar = new Grammar(Choice(Literal("foo"), Literal("bar"), Literal("baz")));
			
			Assert.Equal(grammar.Parse("foo"), "foo");
			Assert.Equal(grammar.Parse("bar"), "bar");
			Assert.Equal(grammar.Parse("baz"), "baz");
			
			Assert.Null(grammar.Parse("fo"));
			Assert.Null(grammar.Parse("fooo"));
		}

		[Fact]
		public void StringSequence() {
			var grammar = new Grammar(Sequence(Literal("foo"), Literal("bar")));
			
			Assert.Equal(grammar.Parse("foobar"), new List<string> { "foo", "bar" });
			
			Assert.Null(grammar.Parse("foo bar"));
			Assert.Null(grammar.Parse(" foobar"));
			Assert.Null(grammar.Parse("foobar "));
			Assert.Null(grammar.Parse("foo"));
			Assert.Null(grammar.Parse("bar"));
			Assert.Null(grammar.Parse("foob"));
		}

		[Fact]
		public void StringLooseSequence() {
			var grammar = new Grammar(LooseSequence(Literal("foo"), Literal("bar")));
			
			Assert.Equal(grammar.Parse("foobar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse("foo bar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse(" foo bar"), new List<string> { "foo", "bar" });
			
			Assert.Null(grammar.Parse("foobar "));
			Assert.Null(grammar.Parse("foo"));
			Assert.Null(grammar.Parse("bar"));
			Assert.Null(grammar.Parse("foob"));
		}

		[Fact]
		public void ChoiceSequence() {
			var grammar = new Grammar(Sequence(
				Choice(Literal("foo"), Literal("bar"), Literal("baz")), 
				Literal("test")
			));
			
			Assert.Equal(grammar.Parse("footest"), new List<string> { "foo", "test" });
			Assert.Equal(grammar.Parse("bartest"), new List<string> { "bar", "test" });
			Assert.Equal(grammar.Parse("baztest"), new List<string> { "baz", "test" });
			
			Assert.Null(grammar.Parse("foo"));
			Assert.Null(grammar.Parse("bar"));
			Assert.Null(grammar.Parse("baz"));
			Assert.Null(grammar.Parse("foot"));
			Assert.Null(grammar.Parse("bart"));
			Assert.Null(grammar.Parse("bazt"));
		}

		[Fact]
		public void IgnoringWhitespaceInSequence() {
			var grammar = new Grammar(Sequence(Literal("foo"), IgnoreLeadingWhitespace(Literal("bar"))));
			
			Assert.Equal(grammar.Parse("foobar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse("foo bar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse("foo      bar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse("foo\t \nbar"), new List<string> { "foo", "bar" });
			
			Assert.Null(grammar.Parse("foo"));
			Assert.Null(grammar.Parse("foo "));
			Assert.Null(grammar.Parse("foo\t\n"));
			Assert.Null(grammar.Parse("foobar "));
			Assert.Null(grammar.Parse(" foo bar"));
		}

		[Fact]
		public void DeepMemoized() {
			var grammar = new Grammar(Choice(
				Sequence(Memoize(Literal("test")), Memoize(Literal("foo"))), 
				Sequence(Memoize(Literal("test")), Memoize(Literal("bar")))
			));
			
			Assert.Equal(grammar.Parse("testfoo"), new List<string> { "test", "foo" });
			Assert.Equal(grammar.Parse("testbar"), new List<string> { "test", "bar" });
			
			Assert.Null(grammar.Parse("test"));
			Assert.Null(grammar.Parse("testfoobar"));
		}

		[Fact]
		public void StringZeroOrMore() {
			var grammar = new Grammar(ZeroOrMore(Literal("foo")));
			
			Assert.Equal(grammar.Parse(""), new List<string> { });
			Assert.Equal(grammar.Parse("foo"), new List<string> { "foo" });
			Assert.Equal(grammar.Parse("foofoo"), new List<string> { "foo", "foo" });
			Assert.Equal(grammar.Parse("foofoofoo"), new List<string> { "foo", "foo", "foo" });
			
			Assert.Null(grammar.Parse("bar"));
			Assert.Null(grammar.Parse("foobar"));
		}

		[Fact]
		public void StringOneOrMore() {
			var grammar = new Grammar(OneOrMore(Literal("foo")));
			
			Assert.Equal(grammar.Parse("foo"), new List<string> { "foo" });
			Assert.Equal(grammar.Parse("foofoo"), new List<string> { "foo", "foo" });
			Assert.Equal(grammar.Parse("foofoofoo"), new List<string> { "foo", "foo", "foo" });
			
			Assert.Null(grammar.Parse(""));
			Assert.Null(grammar.Parse("bar"));
			Assert.Null(grammar.Parse("foobar"));
		}

		[Fact]
		public void StringPositiveLookahead() {
			var grammar = new Grammar(Sequence(Literal("foo"), PositiveLookahead(Literal("bar"))));
			
			Assert.NotNull(grammar.Parse("foobar", requireAll: false));
			
			Assert.Null(grammar.Parse("foobar"));
			Assert.Null(grammar.Parse("foo"));
		}

		[Fact]
		public void StringNegativeLookaheadBare() {
			var grammar = new Grammar(Sequence(Literal("foo"), NegativeLookahead(Literal("bar"))));
			
			Assert.NotNull(grammar.Parse("foo"));
			Assert.NotNull(grammar.Parse("footest", requireAll: false));
			
			Assert.Null(grammar.Parse("foobar"));
			Assert.Null(grammar.Parse("foobartest"));
		}

		[Fact]
		public void StringNegativeLookahead() {
			var grammar = new Grammar(Sequence(Literal("foo"), NegativeLookahead(Literal("bar")), Literal("test")));
			
			Assert.NotNull(grammar.Parse("footest"));
			Assert.NotNull(grammar.Parse("footestbar", requireAll: false));
			
			Assert.Null(grammar.Parse("foobar"));
			Assert.Null(grammar.Parse("foo"));
		}

		[Fact]
		public void OptionalTest() {
			var grammar = new Grammar(LooseSequence(Optional(Literal("foo")), Literal("bar")));
			
			Assert.Equal(grammar.Parse("foobar"), new List<string> { "foo", "bar" });
			Assert.Equal(grammar.Parse("bar"), new List<string> { null, "bar" });
			
			Assert.Null(grammar.Parse("foo"));
			Assert.Null(grammar.Parse("barbar"));
		}

		[Fact]
		public void ForwardTest() {
			var (expr, exprBody) = Forward();
			var group = LooseSequence(Literal("("), expr, Literal(")"));
			exprBody.Value = Choice(group, Literal("foo"));
			var grammar = new Grammar(expr);
			
			Assert.Equal(grammar.Parse("foo"), "foo");
			Assert.Equal(grammar.Parse("(foo)"), new List<string> { "(", "foo", ")" });
			
			Assert.Null(grammar.Parse("("));
			Assert.Null(grammar.Parse("()"));
			Assert.Null(grammar.Parse("(foofoo)"));
			Assert.Null(grammar.Parse("(foo"));
			Assert.Null(grammar.Parse("foo(foo)"));
		}
	}
}