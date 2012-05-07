using System;
using System.Collections.Generic;
using System.Text;
using System.Web.UI;
using System.ComponentModel;

namespace Tableize.Css
{

	/// <summary>
	/// Defines a css name/value declaration, also known as an 'attribute'.  For example: font-size: large;
	/// </summary>
	/// <remarks>
	/// 
	/// </remarks>
    public sealed class CssDeclaration
	{
		public CssDeclaration () {

		}


		public CssDeclaration (string property,string value) {
			if (string.IsNullOrEmpty(property))
				throw new ArgumentNullException();
			if (string.IsNullOrEmpty(value))
				throw new ArgumentNullException();

			Property = property.ToLowerInvariant();
			Value = value;
		}


	    /// <summary>
	    /// The property that is set.  This will be in lower-case, like "font-size".
	    /// </summary>
	    public string Property { get; set; }


	    public string Value { get; set; }


	    public override bool Equals (object obj) {
			CssDeclaration objAsDec = obj as CssDeclaration;
			if (objAsDec == null)
				return false;
			return this.GetHashCode().Equals(objAsDec.GetHashCode());
		}


		public override string ToString () {
			string property = Property;
			string value = Value;
            if (string.IsNullOrEmpty(property))
            {
                if (string.IsNullOrEmpty(value))
                {
                    return string.Empty;
                }
                return value;
            }
            if (string.IsNullOrEmpty(value))
            {
                return property;
            }
			return property + ":" + value + ";";
		}


		public override int GetHashCode () {
			return ToString().GetHashCode();
		}

	}

    /// <summary>
    /// Common css properties.  Just easier for looking stuff up.
    /// </summary>
    public static class CssProperties
    {
        public const string Direction = "direction";
        public const string Font = "font";
        public const string FontFamily = "font-family";
        public const string FontSize = "font-size";
        public const string FontStyle = "font-style";
        public const string FontVariant = "font-variant";
        public const string FontWeight = "font-weight";
        public const string ImeMode = "ime-mode";
        public const string LayoutGrid = "layout-grid";
        public const string LayoutGridChar = "layout-grid-char";
        public const string LayoutGridLine = "layout-grid-line";
        public const string LayoutGridMode = "layout-grid-mode";
        public const string LayoutGridType = "layout-grid-type";
        public const string LetterSpacing = "letter-spacing";
        public const string LineBreak = "line-break";
        public const string LineHeight = "line-height";
        public const string MinHeight = "min-height";
        public const string RubyAlign = "ruby-align";
        public const string RubyOverhang = "ruby-overhang";
        public const string RubyPosition = "ruby-position";
        public const string TextAlign = "text-align";
        public const string TextAutospace = "text-autospace";
        public const string TextDecoration = "text-decoration";
        public const string TextIndent = "text-indent";
        public const string TextJustify = "text-justify";
        public const string TextKashidaSpace = "text-kashida-space";
        public const string TextOverflow = "text-overflow";
        public const string TextTransform = "text-transform";
        public const string TextUnderlinePosition = "text-underline-position";
        public const string UnicodeBidi = "unicode-bidi";
        public const string VerticalAlign = "vertical-align";
        public const string WhiteSpace = "white-space";
        public const string WordBreak = "word-break";
        public const string WordSpacing = "word-spacing";
        public const string WordWrap = "word-wrap";
        public const string WritingMode = "writing-mode";
        public const string BackgroundAttachment = "background-attachment";
        public const string BackgroundColor = "background-color";
        public const string BackgroundImage = "background-image";
        public const string BackgroundPosition = "background-position";
        public const string BackgroundPositionX = "background-position-x";
        public const string BackgroundPositionY = "background-position-y";
        public const string BackgroundRepeat = "background-repeat";
        public const string Color = "color";
        public const string Filter = "filter";
        public const string BorderBottom = "border-bottom";
        public const string BorderBottomColor = "border-bottom-color";
        public const string BorderBottomStyle = "border-bottom-style";
        public const string BorderBottomWidth = "border-bottom-width";
        public const string BorderTop = "border-top";
        public const string BorderTopColor = "border-top-color";
        public const string BorderTopStyle = "border-top-style";
        public const string BorderTopWidth = "border-top-width";
        public const string BorderLeft = "border-left";
        public const string BorderLeftColor = "border-left-color";
        public const string BorderLeftStyle = "border-left-style";
        public const string BorderLeftWidth = "border-left-width";
        public const string BorderRight = "border-right";
        public const string BorderRightColor = "border-right-color";
        public const string BorderRightStyle = "border-right-style";
        public const string BorderRightWidth = "border-right-width";
        public const string BorderCollapse = "border-collapse";
        public const string BorderColor = "border-color";
        public const string BorderStyle = "border-style";
        public const string BorderWidth = "border-width";
        public const string Clear = "clear";
        public const string Float = "float";
        public const string LayoutFlow = "layout-flow";
        public const string Margin = "margin";
        public const string MarginBottom = "margin-bottom";
        public const string MarginLeft = "margin-left";
        public const string MarginRight = "margin-right";
        public const string MarginTop = "margin-top";
        public const string Padding = "padding";
        public const string PaddingBottom = "padding-bottom";
        public const string PaddingLeft = "padding-left";
        public const string PaddingRight = "padding-right";
        public const string PaddingTop = "padding-top";
        public const string TableLayout = "table-layout";
        public const string Zoom = "zoom";
        public const string ListStyle = "list-style";
        public const string Clip = "clip";
        public const string Height = "height";
        public const string Left = "left";
        public const string Overflow = "overflow";
        public const string OverflowX = "overflow-x";
        public const string OverflowY = "overflow-y";
        public const string Position = "position";
        public const string Right = "right";
        public const string Top = "top";
        public const string Visibility = "visibility";
        public const string Width = "width";
        public const string ZIndex = "z-index";
        public const string Opacity = "opacity";
    }


}
