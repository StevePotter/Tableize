using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Tableize;

namespace Tests
{
    [TestClass]
    public class TableizerTests
    {
        [TestMethod]
        public void ConvertElementToTableUsingInlineDeclaration()
        {
            var input = @"<p style='-tableize:1'>Text</p>";
            var output = Tableizer.InlineHtml(input).GetHtml();
            Assert.AreEqual(output,"<table cellspacing=\"0\" border=\"0\"><tr><td>Text</td></tr></table>");
        }

        [TestMethod]
        public void ConvertElementToTableUsingElementSelector()
        {
            var input = @"<html><head><style> p { color:white; -tableize: 1 } </style></head><body><p>Text</p></body></html>";
            var output = Tableizer.InlineHtml(input).GetHtml();
            Assert.AreEqual(output, "<html><head></head><body><table cellspacing=\"0\" border=\"0\"><tr><td style=\"color:white;\">Text</td></tr></table></body></html>");
        }


    }
}
