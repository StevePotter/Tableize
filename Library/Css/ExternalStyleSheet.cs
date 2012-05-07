using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;

namespace Tableize.Css
{
    public class ExternalStyleSheet
	{

		/// <summary>
		/// The file path the style sheet came from.  For informational purposes only.
		/// </summary>
		public string FilePath {
			get { return m_filePath; }
			set { m_filePath = value; }
		}
		string m_filePath;


		/// <summary>
		/// The text that is parsed to fill the rules.
		/// </summary>
		public string SourceText {
			get { return m_sourceText; }
			set { m_sourceText = value; }
		}
		string m_sourceText;


		/// <summary>
		/// The CssRules that apply to this sheet.
		/// </summary>
		[NotifyParentProperty(true)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		public IList<CssRule> Rules {
			get {
				if (m_rules == null) m_rules = new List<CssRule>();
				return m_rules;
			}
		}
		List<CssRule> m_rules;



		/// <summary>
		/// Creates a new style sheet from the given path.
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static ExternalStyleSheet FromFile (string filePath) {
			ExternalStyleSheet sheet = new ExternalStyleSheet();
			sheet.FilePath = filePath;
			FromText(System.IO.File.ReadAllText(filePath),sheet);
			return sheet;
		}


		/// <summary>
		/// Creates a new style sheet from the given text
		/// </summary>
		/// <param name="text"></param>
		/// <returns></returns>
		public static ExternalStyleSheet FromText (string text) {
			ExternalStyleSheet sheet = new ExternalStyleSheet();
			FromText(text,sheet);
			return sheet;
		}


		static void FromText (string text, ExternalStyleSheet sheet) {
			sheet.SourceText = text;
			foreach( CssRule currRule in CssParser.ParseRules(text) )
				sheet.Rules.Add(currRule);
		}
	}


}
