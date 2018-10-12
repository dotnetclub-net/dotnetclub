
import TurndownService from "../../lib/node_modules/turndown"
import Markd from "../../lib/node_modules/marked"


export function MarkdownButton(context) {
    context.layoutInfo.note.on('summernote.codeview.toggled', function () {
        const isActivated = context.invoke('codeview.isActivated');
        const codable = context.layoutInfo.codable;
        if(isActivated){
            const html = codable.val();
            const md = convertToMarkdown(html);
            codable.val(md);
        }else{
            const md = codable.val();
            const html = Markd(md);
            context.layoutInfo.editable.html(html);
            context.triggerEvent('change');
        }
    });

    const button = $.summernote.ui.button({
        contents: '<i class="note-icon-question"/>',
        tooltip: '切换到 Markdown 模式',
        click: function () {
            context.invoke('codeview.toggle');
        }
    });

    return button.render();   // return button as jquery object
}

export function convertToMarkdown(htmlContent) {
    var converters = [
        {
            filter: ['strike', 'del', 's'],
            replacement: function (content) {
                return '~~' + content + '~~';
            }
        },
        {
            filter: ['i', 'em'],
            replacement: function (content) {
                return '*' + content + '*';
            }
        },
        {
            filter: ['b', 'strong'],
            replacement: function (content) {
                return '**' + content + '**';
            }
        }, {
            filter: ['br'],
            replacement: function () {
                // http://stackoverflow.com/a/28633712/1817042
                return '&nbsp;\n\n';
            }
        },
        // Fenced code blocks
        {
            filter: ['pre'],
            replacement: function (content, node) {
                // to-markdown supports Syntax-highlighted code blocks (search 'Syntax-highlighted code blocks' in to-markdown.js)
                var language = node.getAttribute('language') || '';
                return '\n\n```' + language + '\n' + node.firstChild.textContent.replace(/\n+$/,'') + '\n```\n\n';
            }
        }
    ];
    var turndownService = new TurndownService({codeBlockStyle: 'fenced'});
    $.each(converters, function (i, rule) {
        turndownService.addRule(rule.filter.join(''), rule);
    });
    return turndownService.turndown(htmlContent);
}
