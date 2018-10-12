// -2. remove all illegal tags (or only allow tag white-list)
// -3. should remove all attributes & styles when paste
// 4. handle image uploading...
// 5. process pasting code with HTML tags...
// 6. should not use keyboard shortcuts in PRE


// find elements within  note-editable
// popover toolbar may be overflow editor height!
// make this translatable: 选择语言

import filterTags from './editor/html-tag-filter';
import * as InsertCode from './editor/insert-code';
import * as MD from './editor/markdown-support'


export function setupEditor() {
    var editorOptions = defaultEditorOptions();

    $('#content-editor').summernote(editorOptions);
    $('#topic-type-dropdown .topic-type-item').on('click', function (e) {
        var item = $(this);
        
        $('#topic-type-dropdown .topic-type-item').removeAttr('selected');
        item.attr('selected', 'selected');
        $('#topic-type-dropdown .selected-type').text(item.text());
    });
    $('#submit-create').on('click', function () {
        var button = $(this);

        var contentEditor = $('#content-editor').data('summernote');
        var htmlContent = contentEditor.code();

        var element = $('<div>').append(htmlContent);
        filterTags(element[0]);

        var mdContent = MD.convertToMarkdown(element.html());

        var title = $('#new-topic-title').val();
        var topicType = $('#topic-type-dropdown .topic-type-item[selected]>a').attr('attr-value');
        var newTopic = {
            title: title,
            content: mdContent,
            type: topicType,
            __RequestVerificationToken: window.__RequestVerificationToken
        };
        
        if (!newTopic.title || !newTopic.content || !newTopic.type){
            return;
        }

        button.attr('disabled', 'disabled');
        $.post(window.createTopicUrl, newTopic)
            .done(function () {
                location.replace("/");
            }).fail(function(){
                console.log('error on creating new topic');
                button.removeAttr('disabled');
            });
    });
}

function defaultEditorOptions(){
    var defaultOptions = $.extend({}, $.summernote.options);
    var options = $.extend({}, defaultOptions, {
        toolbar: [
            ['style', ['style']],
            ['format', ['bold', 'italic', 'strikethrough', 'clear']],
            ['para', ['ul', 'ol']],
            ['insert', ['link', 'picture', 'insertCode', 'markdown', 'codeview']]
        ],
        styleTags: [
            {value: 'p', title: '常规', tag: 'span'},
            {value: 'h3', title: '标题', tag: 'span' },
            {value: 'blockquote', title: '引用', tag: 'span' }],
        codeLanguages: [
            {title: 'C#', value: 'csharp'},
            {title: 'Visual Basic', value: 'vb'},
            {title: 'TypeScript', value: 'ts'},
            {title: 'JavaScript', value: 'javascript'},
            {title: 'Json', value: 'json'},
            {title: 'HTML', value: 'html'},
            {title: 'XML', value: 'xml'},
            {title: 'YAML', value: 'yml'},
            {title: 'CSS', value: 'css'},
            {title: 'SASS', value: 'sass'},
            {title: 'LESS', value: 'less'},
            {title: 'PowerShell', value: 'ps1'},
            {title: 'Shell', value: 'sh'},
            {title: 'Java', value: 'java'},
            {title: 'Python', value: 'py'},
            {title: 'Ruby', value: 'rb'}
        ],
        placeholder: '正文',
        height: 300,
        callbacks: {
            onChange: function () { },
            onEnter: function(ev){
                ev.preventDefault();
                ev.stopPropagation();

                var editor = $(this).data('summernote');
                var patchedPre = InsertCode.patchPreTag.apply(this, [editor, ev]);
                if(!patchedPre){
                    editor.invoke('editor.insertParagraph');
                }
            },
            onPaste : function (ev) {
                ev.preventDefault();
                var editor = $(this).data('summernote');

                var clipboardData = (ev.originalEvent || ev).clipboardData || window.clipboardData;
                var copiedNode;
                var copiedNodeContent = clipboardData.getData('text/html');
                if(!copiedNodeContent){
                    copiedNodeContent = clipboardData.getData('text/plain') || clipboardData.getData('text');
                    editor.invoke('editor.insertText', copiedNodeContent);
                }else{
                    copiedNode = $('<div>').append($(copiedNodeContent));
                    filterTags(copiedNode[0]);
                    $.each($.makeArray(copiedNode[0].childNodes), function(i, node){
                        if(node.nodeName === 'P' && !$.trim(node.textContent)) {
                            copiedNode[0].removeChild(node);
                        }
                    });
                    var rng = editor.invoke('editor.createRange');
                    var editable = editor.layoutInfo.editable;

                    if(!rng.isCollapsed() || rng.sc.nodeName !== 'P' || rng.sc.parentNode !== editable[0]){
                        editor.invoke('editor.insertParagraph');
                        rng = editor.invoke('editor.createRange');
                    }
                    var paragraph = $(rng.sc).closest('.note-editable>p', editable)[0];
                    $.each($.makeArray(copiedNode[0].childNodes), function(i, node){
                        editable[0].insertBefore(node, paragraph);
                    });
                    editor.invoke('editor.focus');
                }
            }
        },
        buttons:{
            insertCode: InsertCode.CodeButton,
            markdown: MD.MarkdownButton
        }
    });
    options.modules.codePopover = InsertCode.CodePopover;
    options.modules.preventToolsInCodeBlocks = InsertCode.PreventToolsInCodeBlocks;
    options.popover.codeLang = [
        ['codeLang', ['chooseCodeLanguage']]
    ];

    return options;
}
