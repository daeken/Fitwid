using System.Collections.Generic;
using System.Linq;

namespace Fitwid {
	public partial class EbnfParser {
		public partial class Start {
			public IReadOnlyList<EbnfParser.Rule> Rules => ((IEnumerable<object>) RuleDefs).Select(x => (EbnfParser.Rule) x).ToList();
		}
		
		public partial class Choice {
			public IReadOnlyList<EbnfParser> AstChoices =>
				((IEnumerable<object>) Choices).Select(x => (EbnfParser) x).ToList();
		}

		public partial class Sequence {
			public IReadOnlyList<EbnfParser> Items => ((IEnumerable<object>) Value).Select(x => (EbnfParser) x).ToList();
		}
	}
}