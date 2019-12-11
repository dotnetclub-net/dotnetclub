import TurndownService from "../../lib/node_modules/turndown"
import { tables as tdTables } from "../../lib/node_modules/turndown-plugin-gfm"
import Markd from "../../lib/node_modules/marked"


export class MarkdownCodeViewModule {
    constructor(context){
        this._context = context;
    }

    shouldInitialize() {
        return true;
    }

    initialize(){
        const context = this._context;        
        context.layoutInfo.note.on('summernote.codeview.toggled', function () {
            const isActivated = context.invoke('codeview.isActivated');
            const codable = context.layoutInfo.codable;
            if(isActivated){
                const html = codable.val();
                const md = convertToMarkdown(html);
                codable.val(md);
            }else{
                const md = codable.val();
                Markd.setOptions({
                    gfm: true,
                    breaks: false,
                    headerIds: false,
                    silent: true                    
                });
                const html = Markd(md);
                context.layoutInfo.editable.html(html);
                context.triggerEvent('change');
            }
        });   
    }
}

export function viewMarkdownButton() {
    return $.summernote.ui.button({
        className: 'btn-codeview',
        contents: '<i class="btn-view-markdown fab fa-markdown"></i>',
        tooltip: '查看 Markdown 源码',
        click: function () {
            var context = $(this).parents('.note-editor').prev().data('summernote');
            context.invoke('codeview.toggle');
        }
    }).render();
}

let turndownService;
function createTurnDownService(){
    if(turndownService){
        return turndownService;
    }
    
    const converters = [
        {
            filter: ['strike', 'del', 's'],
            replacement: function (content) {
                return '~~' + content + '~~';
            }
        },
        {
            filter: ['pre'],
            replacement: function (content, node) {
                // to-markdown supports Syntax-highlighted code blocks (search 'Syntax-highlighted code blocks' in to-markdown.js)
                var language = node.getAttribute('language') || '';
                return '\n\n```' + language + '\n' + node.firstChild.textContent.replace(/\n+$/,'') + '\n```\n\n';
            }
        }
    ];
    turndownService = new TurndownService({
        codeBlockStyle: 'fenced',
        emDelimiter: '*',
        strongDelimiter: '**',
        headingStyle: 'atx',
        hr: '---'
    });
    $.each(converters, function (i, rule) {
        turndownService.addRule(rule.filter.join(''), rule);
    });
    turndownService.use(tdTables);
    return turndownService;
}
export function convertToMarkdown(htmlContent) {
    var turnDown = createTurnDownService();
    return turnDown.turndown(htmlContent);
}
