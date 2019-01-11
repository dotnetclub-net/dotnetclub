using System;
using System.IO;
using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Discussion.Core.Markdown
{
    public class CustomizableHeadingLevelExtension : IMarkdownExtension
    {
        private readonly int _maxHeadingLevel;

        public CustomizableHeadingLevelExtension(int maxHeadingLevel)
        {
            _maxHeadingLevel = maxHeadingLevel;
        }

        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            var htmlRenderer = renderer as HtmlRenderer;
            if (htmlRenderer == null || htmlRenderer.ObjectRenderers.Contains<CustomizableLevelHeadingRenderer>()) 
                return;
            
            
            var headingRendererIndex = htmlRenderer.ObjectRenderers
                .FindIndex(objRenderer => objRenderer is HeadingRenderer);
            if (headingRendererIndex > -1)
            {
                htmlRenderer.ObjectRenderers.RemoveAt(headingRendererIndex);
            }
                
            htmlRenderer.ObjectRenderers.Insert(headingRendererIndex, 
                new CustomizableLevelHeadingRenderer(_maxHeadingLevel));
        }
    }

    class CustomizableLevelHeadingRenderer: HeadingRenderer
    {
        private readonly int _minLevel;
        public CustomizableLevelHeadingRenderer(int minLevel)
        {
            this._minLevel = minLevel;
            if (_minLevel > 6)
            {
                throw new ArgumentException("level should not be greater than 6 (h6)");
            }
        }
        
        
        protected override void Write(HtmlRenderer renderer, HeadingBlock obj)
        {
            string contentWrote;
            using (var writer = new StringWriter())
            {
                base.Write(new HtmlRenderer(writer), obj);
                contentWrote = writer.ToString();
            }

            if (!renderer.EnableHtmlForBlock)
            {
                renderer.Write(contentWrote);
                return;
            }

            var minStart = string.Format("<h{0}>", _minLevel);
            var minEnd = string.Format("</h{0}>", _minLevel);
            var currentLevel = 1;
            while (currentLevel < _minLevel)
            {
                var replaceStart = string.Format("<h{0}>", currentLevel);
                var replaceEnd = string.Format("</h{0}>", currentLevel);
                contentWrote = contentWrote.Replace(replaceStart, minStart).Replace(replaceEnd, minEnd);

                currentLevel++;
            }
            
            renderer.Write( contentWrote );
        }
    }
}












