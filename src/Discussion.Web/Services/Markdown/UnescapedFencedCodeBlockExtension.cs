using System.IO;
using Markdig;
using Markdig.Helpers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Discussion.Web.Services.Markdown
{
    public class UnescapedFencedCodeBlockExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            if (htmlRenderer != null && !htmlRenderer.ObjectRenderers.Contains<UnescapedFencedCodeBlockRenderer>())
            {
                var codeBlockRendererIndex = htmlRenderer.ObjectRenderers
                                            .FindIndex(objRenderer => objRenderer is CodeBlockRenderer);
                if (codeBlockRendererIndex > -1)
                {
                    htmlRenderer.ObjectRenderers.RemoveAt(codeBlockRendererIndex);
                }
                
                htmlRenderer.ObjectRenderers.Insert(codeBlockRendererIndex, new UnescapedFencedCodeBlockRenderer());
            }
        }
    }

    class UnescapedFencedCodeBlockRenderer: CodeBlockRenderer
    {
        const string CodeStart = "><code";
        const string CodeEnd = "</code></pre>";

        protected override void Write(HtmlRenderer renderer, CodeBlock obj)
        {
            string contentWrote;
            using (var writer = new StringWriter())
            {
                base.Write(new HtmlRenderer(writer), obj);
                contentWrote = writer.ToString();
            }

            if (!renderer.EnableHtmlForBlock)
            {
                renderer.Write(HtmlHelper.Unescape(contentWrote));
                return;
            }
            
            var codeStartTagBegin = contentWrote.IndexOf(CodeStart);
            var codeStartTagEnd = codeStartTagBegin + CodeStart.Length + contentWrote.Substring(codeStartTagBegin + CodeStart.Length).IndexOf(">");
            var codeEndTagBegin = contentWrote.LastIndexOf(CodeEnd);
            var codeContent = contentWrote.Substring(codeStartTagEnd + 1, codeEndTagBegin - (codeStartTagEnd + 1));

            renderer.Write(contentWrote.Substring(0, codeStartTagEnd + 1));
            renderer.Write( HtmlHelper.Unescape(codeContent) );
            renderer.Write(contentWrote.Substring(codeEndTagBegin));
        }
    }
}












