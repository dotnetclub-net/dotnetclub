using MarkdownSharp;
using System.Text.RegularExpressions;
using System.Text;
using MarkdownSharp.Extensions;
using System.Reflection;
using System;

namespace Discussion.Web.Services
{
    public class GfmCodeBlocks : IExtensionInterface
    {
        private static Regex _codeBlock = new Regex(@"(?:\r?\n|^)(`{3,}|~{3,})([\u0020\t]*(?<lang>\S+))?[\u0020\t]*\r?\n
	(?<code>[^\r^\n]*\r?\n)*?
	\1(?:\r?\n|$)",
        RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);
        private static Regex _lastLineBreak = new Regex(@"\r?\n$", RegexOptions.Compiled);


        private readonly Markdown _converterIntance;

        public GfmCodeBlocks(Markdown converterIntance)
        {
            _converterIntance = converterIntance;
        }

        public string Transform(string markdown)
        {
            var transformed = _codeBlock.Replace(markdown, new MatchEvaluator(CodeBlockEvaluator));

            Type converterType = typeof(Markdown);
            var hashHTMLBlocks = converterType.GetMethod("HashHTMLBlocks", BindingFlags.NonPublic | BindingFlags.Instance);
            var afterHashed = hashHTMLBlocks.Invoke(_converterIntance, new[] { transformed }) as string;

            return afterHashed;
        }


        private string CodeBlockEvaluator(Match match)
        {
            var preBuilder = new StringBuilder();
            preBuilder.AppendLine();
            preBuilder.Append("<pre");

            var lang = match.Groups["lang"].Value;
            if (!string.IsNullOrWhiteSpace(lang))
            {
                preBuilder.AppendFormat(@" class=""language-{0}""", lang);
            }

            preBuilder.Append("><code>");
            foreach (Capture line in match.Groups["code"].Captures)
            {
                preBuilder.Append(line.Value);
            }

            var pre = preBuilder.ToString();
            pre = _lastLineBreak.Replace(pre, string.Empty);
            return string.Concat(pre, "</code></pre>\n");
        }
    }
}
