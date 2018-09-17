using Discussion.Web.Services.Markdown;
using Xunit;

namespace Discussion.Web.Tests.Specs.Services
{
    public class MarkdownPipelineSpecs
    {
        [Fact]
        public void should_convert_markdown_to_html()
        {
            var md = "**Hello World**";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n", html);
        }
        
        [Fact]
        public void should_convert_markdown_with_fenced_code_block()
        {
            var md = @"**Hello World**

```js
console.log('hello js');
```

text after code block";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n<pre><code class=\"language-js\">console.log('hello js');\n</code></pre>\n\n<p>text after code block</p>\n", html);
        }
        
        
        [Fact]
        public void should_convert_markdown_with_fenced_code_block_and_unescape_code_content()
        {
            var md = @"**Hello World**

```html
&lt;html&gt;
&lt;title&gt;title text&lt;/title&gt;
&lt;/html&gt;
```

text after code block";

            var html = MarkdownConverter.ToHtml(md);
            
            Assert.Equal("<p><strong>Hello World</strong></p>\n<pre><code class=\"language-html\">&lt;html&gt;\n&lt;title&gt;title text&lt;/title&gt;\n&lt;/html&gt;\n</code></pre>\n\n<p>text after code block</p>\n", html);
        }
    }
}