using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Text.RegularExpressions;
using System.Diagnostics;

namespace Tableize.Css
{
    public class CssException : System.Exception
	{
		public CssException (string message)
			: base(message) {
		}

		public CssException (string message,Exception innerException)
			: base(message,innerException) {
		}
	}


	/// <summary>
	/// Parses css style sheets.
	/// </summary>
    public static class CssParser
	{
		public static List<CssRule> ParseRules (string cssSource) {
			// remove comments
			Regex commentPattern = new Regex(@"(//.*)|(/\*(.|\n)*?\*/)");
			cssSource = commentPattern.Replace(cssSource, String.Empty);

			RuleParsingObject currObject = RuleParsingObject.None;
			StringBuilder sb = null;

			List<CssRule> rules = new List<CssRule>();
			string currSelectorText = null;

			foreach (char currChar in cssSource) {
				switch (currObject) {
					case RuleParsingObject.None:
						//look for the beginning of a selector
						if (!char.IsWhiteSpace(currChar)) {
							if (IsBracket(currChar))
								throw new CssException("Selectors cannot begin with a bracket.");

							//advance to selector
							sb = new StringBuilder();
							sb.Append(currChar);
							currObject = RuleParsingObject.Selector;
						}
						break;
					case RuleParsingObject.Selector:
						//we encountered the beginning of a declaration block
						if (currChar == '{') {
							currSelectorText = sb.ToString().Trim();
							sb = new StringBuilder();
							currObject = RuleParsingObject.Declarations;
						} else {
							sb.Append(currChar);
						}
						break;
					case RuleParsingObject.Declarations:
						if (currChar == '}') {
							List<CssSelector> selectors;
							try {
								selectors = ParseSelectors(currSelectorText);
							} catch {
								throw new CssException(string.Format("Could not parse selector '{0}'",currSelectorText));
							}
							List<CssDeclaration> declarations;
							string declarationText = sb.ToString().Trim();
							try {
								declarations = ParseDeclarations(declarationText);
							} catch {
								throw new CssException(string.Format("Could not parse declarations '{0}'",declarationText));
							}
							rules.Add(new CssRule(selectors,declarations));

							sb = null;
							currObject = RuleParsingObject.None;
						} else {
							//still inside declaration block so keep going
							sb.Append(currChar);
						}
						break;
					default:
						throw new System.ComponentModel.InvalidEnumArgumentException();
				}

			}

			return rules;
		}



		public static CssRuleDictionary ParseRuleDictionary (string cssSource) {
			CssRuleDictionary dictionary = new CssRuleDictionary();
			dictionary.AddRules(ParseRules(cssSource));
			return dictionary;
		}

		private static bool IsBracket(char currChar )
		{
			return currChar == '{' || currChar == '}';
		}

		private enum RuleParsingObject
		{
			None,
			Selector,
			Declarations,
		}

		static List<CssSelector> ParseSelectors (string source) {

			List<CssSelector> selectors = new List<CssSelector>();

			foreach (string currSelector in source.Split(new char[] { ',' },StringSplitOptions.RemoveEmptyEntries)) {
				selectors.Add(ParseSelector(currSelector.Trim()));
			}
			return selectors;
		}

		/// <summary>
		/// Creates the selector(s) for the given text
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		internal static CssSelector ParseSelector (string source) {
			List<string> selectorTexts = new List<string>();//the text to apply to selectors.  this is done after parsing because I didn't want to write the appending of text into the selectors, since some use enums or whatever.  there is one of these per selector

			int numSelectors = 0;
			CssSelector currSelector = null;
			CssSelector rootSelector = null;
			StringBuilder sb = null;
			foreach (char currChar in source) {
				//we found a class or ID selector
				//todo: you can use a mapping to each selector's DirectiveChar and use that instead of these if statements
				if (currChar == '.' || currChar == '#' || currChar == ':') {
					CssSelector newSelector;
					if (currChar == '.')
						newSelector = new ClassSelector();
					else if (currChar == '#')
						newSelector = new IDSelector();
					else
						newSelector = new PsuedoClassSelector();

					if ( rootSelector == null )
						rootSelector = newSelector;

					if (currSelector != null)
						currSelector.Child = newSelector;//relationship is either set prior to descendant or it takes the default, which is combination

					currSelector = newSelector;
					numSelectors++;
					sb = new StringBuilder();
				} else if (char.IsWhiteSpace(currChar)) {
					//whitespace found between selectors means we got a descendant relationship.
					currSelector.ChildRelationship = SelectorRelationship.Descendant;
					sb = null;
				} else {
					//either this is a new tag selector or it is appending the text of the currSelector
					if (sb == null) {
						CssSelector newSelector = new TagSelector();
						if (currSelector != null)
							currSelector.Child = newSelector;

						if ( rootSelector == null )
							rootSelector = newSelector;

						currSelector = newSelector;
						numSelectors++;
						sb = new StringBuilder();
					}

					sb.Append(currChar);
					Debug.Assert(currSelector != null);

					if (selectorTexts.Count < numSelectors) {
						selectorTexts.Add(sb.ToString());
					} else {
						selectorTexts[numSelectors - 1] = sb.ToString();
					}
				}
			}

			currSelector = rootSelector;
			foreach (string currSelectorText in selectorTexts) {
				currSelector.InnerText = currSelectorText;
				currSelector = currSelector.Child;
			}
			return rootSelector;
		}

		/// <summary>
		/// Parses the given declaration block into its individual declarations.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static List<CssDeclaration> ParseDeclarations (string source) {
			List<CssDeclaration> declarations = new List<CssDeclaration>();

			string[] declarationPieces = source.Split(new char[] { ';' },StringSplitOptions.RemoveEmptyEntries);
            if (declarationPieces != null && declarationPieces.Length > 0)
            {
                foreach (string currDeclaration in declarationPieces)
                {
                    if (string.IsNullOrEmpty(currDeclaration))
						continue;

                    CssDeclaration declaration = ParseDeclaration(currDeclaration);
					declarations.Add(declaration);
                }
            }

			return declarations;
		}


        /// <summary>
        /// Parses a single css declaration.
        /// </summary>
        /// <param name="currDeclaration"></param>
        /// <returns></returns>
        internal static CssDeclaration ParseDeclaration (string text) {
			//instead of using split, I use indexof because certain declarations have multiple ":", like filter
			int splitIndex = text.IndexOf(':');
			if (splitIndex < 0)
				throw new CssException("Could not parse CSS declaration " + text + " because it had no ':'");

			CssDeclaration declaration = new CssDeclaration();
			declaration.Property = text.Substring(0,splitIndex).Trim();
            string value = text.Substring(splitIndex + 1).Trim();
            if (value.EndsWith(";",StringComparison.Ordinal))
                value = value.Substring(0,value.Length - 1);
			declaration.Value = value;
            return declaration;
        }


		/// <summary>
		/// Takes a declaration string like "font-size: 4px" and returns the property name.  This should only be run on valid declaration strings because it does no checking.
		/// </summary>
		/// <param name="declaration"></param>
		/// <returns></returns>
        internal static string GetDeclarationProperty (string declaration) {
			int splitIndex = declaration.IndexOf(':');
			if (splitIndex < 0)
				throw new CssException("Could not parse CSS declaration " + declaration + " because it had no ':'");

			return declaration.Substring(0,splitIndex).Trim();
        }

	}

}
