using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;

namespace Tableize.Css
{

	/// <summary>
	/// Represents a css rule, which is a bunch of declarations for some selector(s).  
	/// </summary>
	/// <remarks>
	/// A rule is, for example:
	/// 
	/// .mainText, .hi
	/// {
	///		font-size: smaller;
	///		color: blue;
	/// }
	/// 
	/// </remarks>
    public sealed class CssRule
	{
		public CssRule () {

		}

		public CssRule (CssSelector selector,IEnumerable<CssDeclaration> declarations) {
			Selectors.Add(selector);
			m_declarations = new List<CssDeclaration>(declarations);
		}

		public CssRule (IEnumerable<CssSelector> selectors,IEnumerable<CssDeclaration> declarations) {
			m_selectors = new List<CssSelector>(selectors);
			m_declarations = new List<CssDeclaration>(declarations);
		}

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public IList<CssSelector> Selectors {
			get {
				if (m_selectors == null) m_selectors = new List<CssSelector>();
				return m_selectors;
			}
		}
		List<CssSelector> m_selectors;

		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public IList<CssDeclaration> Declarations {
			get {
				if (m_declarations == null) m_declarations = new List<CssDeclaration>();
				return m_declarations;
			}
		}
		List<CssDeclaration> m_declarations;

		/// <summary>
		/// Gets a value indicating if this rule has the same exact declarations as the ones passed, although not necessarily in the same order.  This is a slow method that was not meant to be used much.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="yDeclarations"></param>
		/// <returns></returns>
		internal bool HasEqualDeclarations (IList<CssDeclaration> matches) {
			if (Declarations.Count != matches.Count)
				return false;

            //just loop through until you find an attribute that doesn't match
            foreach (CssDeclaration declarationToCheck in matches)
            {
                string property = declarationToCheck.Property;
                bool foundMatchingAttribute = false;
                foreach (CssDeclaration existingAtt in Declarations)
                {
					if (existingAtt.Equals(declarationToCheck)) {
						foundMatchingAttribute = true;
						break;
					}
                }

                if (!foundMatchingAttribute)
                    return false;
            }
            return true;
		}

	}

}
