using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using HtmlAgilityPack;
using Tableize.Css;

namespace Tableize
{
    /// <summary>
    /// This takes a regular html file (must have css in STYLE tags, not an external LINK), and takes the css rules and puts them into individual elements.  This is intended for use during 
    /// emails.  It supports a few custom css rules, one in particular that makes it possible to turn an element like DIV into a TABLE.
    /// </summary>
    /// <remarks>
    /// Since this is meant to be part of an outgoing email system, performance is a top concern.  Most emails should only take a few milliseconds.
    /// 
    /// Only tag selectors and class selectors are supported.  Nothing else - no mixtures (don't do td.field), descendants, pseudo selectors, attribute selectors or anything else.
    /// It's also not smart about selector order.  And it's not smart about replacing attributes that are already inline.  
    /// Also do not use margin style attribute it don't work!
    /// </remarks>
    public static class Tableizer
    {
        /// <summary>
        /// Takes the css styles in STYLE tags and inlines them into html elements.  Intended for use in sending html emails, where css sheets often aren't honored.
        /// </summary>
        public static HtmlDocument InlineHtml(string html, string css = null)
        {
            bool clearCss = true;
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var cssSource = new StringBuilder();
            if (css.HasChars())
            {
                cssSource.Append(css);
            }
            HtmlNode head = doc.DocumentNode.SelectSingleNode("//head");
            if (head != null)
            {
                HtmlNodeCollection styleElements = head.SelectNodes("//style");
                if (styleElements != null)
                {
                    foreach (HtmlNode node in styleElements)
                    {
                        cssSource.AppendLine(node.InnerText.Trim());
                        if (clearCss)
                            node.ParentNode.RemoveChild(node);
                    }
                }
            }
            //if there is no css, there could still be inline -tableize attributes so continue on

            CssRuleDictionary rules = CssParser.ParseRuleDictionary(cssSource.ToString());
            string[] selectors = rules.GetSelectors().ToArray();

            var tagsWithCssRules = new HashSet<string>();
            var classesWithCssRules = new HashSet<string>();

            foreach (CssSelector selector in rules.GetSelectors().Select(CssParser.ParseSelector))
            {
                if (selector is ClassSelector)
                    classesWithCssRules.Add(((ClassSelector) selector).ClassName);
                else if (selector is TagSelector)
                    tagsWithCssRules.Add(selector.Text.Trim().ToLowerInvariant());
                //else todo: warn of an invalid selector type
            }
            
            //originally I used xpath selectors but that was slow.  by looping through elements once and gathering the ones i need, it cut avg time from 4ms to 2
            var elementsWithCssTags = new List<HtmlNode>();
            var elementsWithCssClasses = new List<HtmlNode>();
            HtmlNode[] allElements = doc.DocumentNode.DescendantNodesAndSelf().Where(d => d.NodeType == HtmlNodeType.Element).ToArray();
            foreach (HtmlNode tag in allElements)
            {
                string tagName = tag.Name.ToLowerInvariant();
                if (tagsWithCssRules.Contains(tagName))
                    elementsWithCssTags.Add(tag);
                if (tag.Attributes["class"] != null)
                    elementsWithCssClasses.Add(tag);
            }

            //inline tags with matching selectors
            if (elementsWithCssTags.Count > 0)
            {
                foreach (HtmlNode element in elementsWithCssTags)
                {
                    string selector = element.Name.ToLowerInvariant();
                    List<CssDeclaration> declarations = rules[selector];
                    if (declarations.Count > 0)
                        InlineDeclarations(element, declarations);
                }
            }

            //inline css classes
            if (elementsWithCssClasses.Count > 0)
            {
                foreach (HtmlNode element in elementsWithCssClasses)
                {
                    HtmlAttribute classAtt = element.Attributes["class"];
                    foreach (var className in classAtt.Value.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries))
                    {
                        string selector = "." + className;
                        List<CssDeclaration> declarations = rules[selector]; //todo: add warning if no rule found
                        if (declarations != null && declarations.Count > 0)
                            InlineDeclarations(element, declarations);
                    }
                    if (clearCss)
                        element.Attributes.Remove(classAtt);
                }
            }

            //convert indicated items to tables
            foreach (HtmlNode element in allElements)
            {
                HtmlAttribute styleAtt = element.Attributes["style"];
                if (styleAtt != null && styleAtt.Value.Contains("-tableize"))
                {
                    ConvertElementToTable(doc, element);
                }
            }

            return doc;
        }

        public static string GetHtml(this HtmlDocument doc)
        {
            var output = new StringBuilder();
            using (var reader = new StringWriter(output))
            {
                doc.Save(reader);
            }
            return output.ToString();
        }

        /// <summary>
        /// Takes an element and turns it into a table with a single cell.  The width attribute will be at the table level, and everything else will be at td level
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="element"></param>
        private static void ConvertElementToTable(HtmlDocument doc, HtmlNode element)
        {
            HtmlNode table = doc.CreateElement("table");
            HtmlNode tr = doc.CreateElement("tr");
            HtmlNode td = doc.CreateElement("td");
            table.SetAttributeValue("cellspacing", "0");
            table.SetAttributeValue("border", "0");
            table.AppendChild(tr).AppendChild(td);
            if (element.HasAttributes)
            {
                foreach (HtmlAttribute att in element.Attributes)
                {
                    //width style attribute will go at the table level
                    if (att.Name.Equals("style", StringComparison.OrdinalIgnoreCase))
                    {
                        List<CssDeclaration> existingStyles = CssParser.ParseDeclarations(att.Value);
                        if (existingStyles.Count > 0)
                        {
                            var tableStyle = new StringBuilder();
                            var tdStyle = new StringBuilder();
                            foreach (CssDeclaration dec in existingStyles)
                            {
                                if (dec.Property.EqualsCaseInsensitive("-tableize"))
                                {
                                    continue;
                                }
                                if (dec.Property.EqualsCaseInsensitive("width"))
                                {
                                    //tableStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    table.SetAttributeValue("width", dec.Value);
                                    td.SetAttributeValue("width", "100%");
                                    //it's a single table with a single td, so you can just make the cell take up the table's width
                                    //tdStyle.Append(dec.Property + ":" + dec.Value + ";");
                                    //InlineNonStyleAttribute(table, "width", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseInsensitive("padding"))
                                {
                                    table.SetAttributeValue("cellpadding", dec.Value.FilterNumbers(false));
                                }
                                else if (dec.Property.EqualsCaseInsensitive(CssProperties.TextAlign))
                                {
                                    //putting text-align in the style attribte along with the align attribute caused it to be always left aligned, at least in FF.  no clue why but this works fine
                                    InlineNonStyleAttribute(td, "align", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseInsensitive(CssProperties.VerticalAlign))
                                {
                                    InlineNonStyleAttribute(td, "valign", dec.Value);
                                }
                                else if (dec.Property.EqualsCaseInsensitive(CssProperties.BackgroundColor))
                                {
                                    InlineNonStyleAttribute(td, "bgcolor", dec.Value);
                                }
                                else
                                {
                                    tdStyle.Append(dec.Property + ":" + dec.Value + ";");
                                }
                            }
                            if (tableStyle.Length > 0)
                                table.SetAttributeValue("style", tableStyle.ToString());
                            if (tdStyle.Length > 0)
                                td.SetAttributeValue("style", tdStyle.ToString());
                        }
                    }
                    else
                    {
                        td.SetAttributeValue(att.Name, att.Value);
                    }
                }
            }
            foreach (HtmlNode child in element.ChildNodes)
            {
                td.AppendChild(child);
            }
            element.ParentNode.ReplaceChild(table, element);
        }

        /// <summary>
        /// Takes a list of css declarations, like color:white, and puts them in the element's style attribute.  Certain cases are treated special.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="declarations"></param>
        /// <remarks>
        /// Special Cases:
        /// img: width & height attributes
        /// td: align & valign
        /// table: cellspacing, border, and padding
        /// </remarks>
        public static void InlineDeclarations(HtmlNode element, List<CssDeclaration> declarations)
        {
            HtmlAttribute style = element.Attributes["style"];
            StringBuilder styleVal = null;
            HashSet<string> existingStyles = null; //so we don't overwrite any preexisting inline styles
            if (style != null)
            {
                existingStyles =
                    new HashSet<string>(CssParser.ParseDeclarations(style.Value).Select(dec => dec.Property));
                if (existingStyles.Count > 0)
                {
                    styleVal = new StringBuilder(style.Value.Trim());
                    if (styleVal[styleVal.Length - 1] != ';')
                        styleVal.Append(';');
                }
            }

            string tagName = element.Name.ToLowerInvariant();
            foreach (var declaration in declarations)
            {
                if (existingStyles != null && existingStyles.Contains(declaration.Property))
                    continue; //ignore the duplicate

                if (tagName.EqualsCaseInsensitive("td"))
                {
                    if (declaration.Property.EqualsCaseInsensitive(CssProperties.TextAlign))
                    {
                        if (!InlineNonStyleAttribute(element, "align", declaration.Value))
                            continue;
                    }
                    else if (declaration.Property.EqualsCaseInsensitive(CssProperties.VerticalAlign))
                    {
                        if (!InlineNonStyleAttribute(element, "valign", declaration.Value))
                            continue;
                    }
                }

                if (tagName.EqualsAny("table", "td", "img") &&
                    declaration.Property.EqualsAny(CssProperties.Width, CssProperties.Height))
                {
                    if (!InlineNonStyleAttribute(element, declaration.Property, declaration.Value))
                        continue;
                }
                if (tagName.EqualsAny("table") && declaration.Property.EqualsAny(CssProperties.BackgroundColor))
                {
                    if (!InlineNonStyleAttribute(element, "bgcolor", declaration.Value))
                        continue;
                }

                //a nice way to auto insert cellpadding and cellspacing
                if (declaration.Property.StartsWith("-attr-"))
                {
                    InlineNonStyleAttribute(element, declaration.Property.Strip("-attr-"), declaration.Value);
                    continue;
                }
                if (styleVal == null)
                    styleVal = new StringBuilder();

                styleVal.Append(declaration.Property + ":" + declaration.Value + ";");
            }

            if (styleVal != null)
                element.SetAttributeValue("style", styleVal.ToString());
        }

        private static bool InlineNonStyleAttribute(HtmlNode element, string attributeName, string value)
        {
            HtmlAttribute att = element.Attributes[attributeName];
            if (att != null && att.Value.HasChars())
                return false;

            element.SetAttributeValue(attributeName, value);
            return true;
        }
    }
}