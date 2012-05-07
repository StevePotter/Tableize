using System;
using System.Collections.Generic;
using System.Text;

namespace Tableize.Css
{
	/// <summary>
	/// Contains a dictionary of css declarations indexed by their selectors.   The difference between this and a list of rules is that this contains a unique 
	/// rule per selector text.  This will combine alike selectors into a single rule and will split multiple selectors into multiple rules.
	/// </summary>
    public class CssRuleDictionary
	{
		private Dictionary<string,List<CssDeclaration>> m_innerTable = new Dictionary<string,List<CssDeclaration>>();

		public void AddRules (IEnumerable<CssRule> rules) {
			foreach (CssRule currRule in rules) {
				foreach (CssSelector currSelector in currRule.Selectors) {
					string selectorText = currSelector.Text;
					List<CssDeclaration> currDeclarations;
					if (m_innerTable.TryGetValue(selectorText,out currDeclarations)) {
						foreach (CssDeclaration currDeclaration in currRule.Declarations) {
							currDeclarations.Add(currDeclaration);
						}
					} else {
						currDeclarations = new List<CssDeclaration>();
						currDeclarations.AddRange(currRule.Declarations);
						m_innerTable.Add(selectorText,currDeclarations);
					}
				}
			}
		}

		public List<CssDeclaration> this[string selector]
		{
			get{
				List<CssDeclaration> declarations;
				if (m_innerTable.TryGetValue(selector, out declarations))
					return declarations;
				else
					return null;
			}
		}


		public List<CssDeclaration> GetClassDeclarations (string cssClassName) {
			return this["." + cssClassName];
		}


		public IEnumerable<string> GetSelectors () {
			return m_innerTable.Keys;
		}

		/// <summary>
		/// Gets a list of the unique single css classes.  Combination or descendant selectors are not included.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetClassNames () {
		    foreach (string currSelector in m_innerTable.Keys) {
		        CssSelector selector = CssParser.ParseSelector(currSelector);
		        if (selector is ClassSelector && !selector.HasChild) {
		            yield return ((ClassSelector) selector).ClassName;
		        }
		    }
		}

	}
}
