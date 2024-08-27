using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParseHtml.Service
{
    public class CheckData
    {
        public bool ShouldProcessNode(HtmlNode bodyNode)
        {
            var fromNameNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'from_name')]");
            return fromNameNode == null || fromNameNode.InnerText.Trim().Equals("N", StringComparison.OrdinalIgnoreCase);
        }


        public string? GetDateFromBodyNode(HtmlNode bodyNode, string? lastDateValue)
        {
            var dateNode = bodyNode.SelectSingleNode(".//div[contains(@class, 'pull_right date details')]");
            return dateNode?.GetAttributeValue("title", null) ?? lastDateValue;
        }
    }
}
