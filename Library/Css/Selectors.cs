using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.Reflection;

namespace Tableize.Css
{


	/// <summary>
	/// A selector, which is used to identify the object(s) that declaratiions are 
	/// </summary>
	/// <remarks>
	/// For more info on selectors, see the Cascading Style Sheets - Definitive Guide.
	/// Supported selectors:
	/// - Element
	/// - Class
	/// - ID
	/// - PseudoClass
	/// - Descendant
	/// - Chained (multiple selectors)
	/// 
	/// Descendant and Chained selectors are supported through the Child and ChildRelationship properties.
	/// 
	/// Unsupported:
	/// - Univeral (*)
	/// - Attribute ([....])
	/// - Child
	/// - Adjacent sibling
	/// 
	/// 
	/// </remarks>
    public abstract class CssSelector
	{

		public CssSelector Child {
			get { return m_child; }
			set { m_child = value; }
		}
		CssSelector m_child;

		public bool HasChild {
			get {
				return m_child != null;
			}
		}

		public SelectorRelationship ChildRelationship {
			get { return m_childRelationship; }
			set { m_childRelationship = value; }
		}
		SelectorRelationship m_childRelationship;


		/// <summary>
		/// The text shown by the selector.  This omits any directive chars as well as any child selectors.
		/// </summary>
		internal abstract string InnerText { get; set; }

		/// <summary>
		/// Gets the text for this selector, which includes any directive chars (.,#,etc) as well as any child selectors)
		/// </summary>
		internal string Text {
			get {
				StringBuilder sb = new StringBuilder();
				if (DirectiveChar.HasValue)
					sb.Append(DirectiveChar.Value);

				sb.Append(InnerText);
				if (HasChild) {
					if (ChildRelationship == SelectorRelationship.Descendant)
						sb.Append(' ');

					sb.Append(Child.Text);
				}
				return sb.ToString();
			}
		}

		protected virtual char? DirectiveChar {
			get {
				return null;
			}
		}

	}

    public enum SelectorRelationship
	{
		/// <summary>
		/// The two selectors are a combination, like p.warning.  In this case, the separators have no whitespace between them.
		/// </summary>
		Combination,
		/// <summary>
		/// The second selector is applied to decendants of the first selector.
		/// </summary>
		Descendant,
	}

	/// <summary>
	/// Selects elements that are a particular tag type.
	/// </summary>
    public sealed class TagSelector : CssSelector
	{

		/// <summary>
		/// The html element (tag) that gets selected.
		/// </summary>
		public HtmlTextWriterTag ElementTag {
			get { return m_tag; }
			set { m_tag = value; }
		}
		HtmlTextWriterTag m_tag;


		internal override string InnerText {
			get {
				return ElementTag.ToString().ToLowerInvariant() ;
			}
			set {
				ElementTag = (HtmlTextWriterTag) System.Enum.Parse(typeof(HtmlTextWriterTag),value,true);
			}
		}


	}

	/// <summary>
	/// Selects elements of a particular ID.
	/// </summary>
    public sealed class IDSelector : CssSelector
	{
	    /// <summary>
	    /// The ID of the DOM element that is selected.
	    /// </summary>
	    public string ElementID { get; set; }


	    internal override string InnerText {
			get {
				return ElementID;
			}
			set {
				ElementID = value;
			}
		}

		protected override char? DirectiveChar {
			get {
				return '#';
			}
		}
	}

	/// <summary>
	/// Selects elements whose class attribute contains the name of this class.
	/// </summary>
    public sealed class ClassSelector : CssSelector
	{
		public ClassSelector () {

		}

		public ClassSelector (string className) {
			ClassName = className;
		}

	    /// <summary>
	    /// The name of the class.
	    /// </summary>
	    public string ClassName { get; set; }

	    internal override string InnerText {
			get {
				return ClassName;
			}
			set {
				ClassName = value;
			}
		}

		protected override char? DirectiveChar {
			get {
				return '.';
			}
		}
	}

	/// <summary>
	/// Defines a pseudo class that is attached to the element.
	/// </summary>
    public sealed class PsuedoClassSelector : CssSelector
	{
	    internal override string InnerText { get; set; }

		protected override char? DirectiveChar {
			get {
				return ':';
			}
		}
	}

}
