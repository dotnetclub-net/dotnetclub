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
import * as imageUploader from './editor/image-uploader'
import * as imageResizing from './editor/image-resize'


export function setupEditor(userCanImport) {
    var editorOptions = defaultEditorOptions();

    $('#content-editor').summernote(editorOptions);
    $('#topic-type-dropdown .topic-type-item').on('click', function (e) {
        var item = $(this);
        
        $('#topic-type-dropdown .topic-type-item').removeAttr('selected');
        item.attr('selected', 'selected');
        $('#topic-type-dropdown .selected-type').text(item.text());
    });
    $('#submit-create').on('click', function () {
        var topicType = $('#topic-type-dropdown .topic-type-item[selected]>a').attr('attr-value');
        if (!topicType) {
<<<<<<< HEAD
            alert("请先选择话题类型");
=======
            alert("请选择话题类型");
>>>>>>> origin/master
        }
        var button = $(this);
        var contentEditor = $('#content-editor').data('summernote');
        var htmlContent = contentEditor.code();

        var element = $('<div>').append(htmlContent);
        filterTags(element[0]);

        var mdContent = MD.convertToMarkdown(element.html());

        var title = $('#new-topic-title').val();
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
        var url = window.createTopicUrl; 
        var importChatId = $('input[name=import-chat-id]').val();
        if(importChatId){
            newTopic.chatId = importChatId;
            url = window.createWithImportingUrl;
        }
        $.post(url, newTopic)
            .done(function () {
                location.replace("/");
            }).fail(function(){
                console.error('error on creating new topic');
                button.removeAttr('disabled');
                alert('无法创建新的话题');
            });
    });
    
    if(userCanImport){
        $.getJSON(window.loadChatySessionListUrl)
            .then(function (data) {
                if(!data.hasSucceeded){
                    return;
                }
              
                data.result.forEach(function (item) {
                    var li = $('<li class="importable-chat-item">');
                    li.html(item.messageSummaryList.join('<br />'));
                    li.attr('attr-chat-id', item.chatId);
                    $('.topic-import-chaty-section .importable-chats').append(li);
                    $('.topic-import-chaty-section').removeClass('hide').addClass('show');
                });

                $('.topic-import-chaty-section .importable-chats li').click(function () {
                    $('.topic-import-chaty-section .importable-chats li').removeClass('selected');
                    
                    var inputBox = $('input[name=import-chat-id]');
                    var thisChatId = $(this).attr('attr-chat-id');
                    if(inputBox.val() === thisChatId){
                        inputBox.val('');
                    }else {
                        inputBox.val(thisChatId);
                        $(this).addClass('selected');
                    }
                });
            });
    }
}

function defaultEditorOptions(){
    var options = $.extend({}, $.summernote.options, {
        lang: 'zh-CN',
        toolbar: [
            ['style', ['style']],
            ['format', ['bold', 'italic', 'strikethrough', 'clear']],
            ['para', ['ul', 'ol']],
            ['insert', ['link', 'picture', 'insertCode', 'markdown']]
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
            {title: 'Python', value: 'py'}
        ],
        placeholder: '正文',
        height: 300,
        callbacks: {
            onChange: function () { },
            onEnter: function(ev){
                var editor = $(this).data('summernote');
                var patchedPre = InsertCode.patchPreTag.apply(this, [editor, ev]);
                if(patchedPre){
                    ev.preventDefault();
                    ev.stopPropagation();
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
            },
            onImageUpload: function(files) {
                var editor = $(this).data('summernote');
                if(editor.options.maximumImageFileSize < files[0].size){
                    console.log('所选择的图片文件超过了 ' + editor.options.maximumImageFileSize + ' 字节');
                    alert('图片文件太大了，无法上传');
                    return;
                }
                
                imageUploader.onImageSelected(files[0], function(url){
                    editor.invoke('insertImage', url, function(){ /*no-op fn to prevent auto set width*/ });
                });
            }
        },
        buttons:{
            insertCode: InsertCode.insertCodeButton,
            markdown: MD.viewMarkdownButton
        },
        popover: {
            codeLang : [
                ['codeLang', ['chooseCodeLanguage']]
            ],
            image: [
                ['imagesize', ['imageSizeSmall', 'imageSizeMiddle', 'imageSizeRaw']]
            ],
        },
        disableResizeImage: true,
        maximumImageFileSize: 2 * 1024 * 1024  /* 2M */
    });
    options.modules.markdownCodeView = MD.MarkdownCodeViewModule;
    options.modules.codePopover = InsertCode.CodePopoverModule;
    options.modules.preventToolsInCodeBlocks = InsertCode.PreventToolsInCodeBlocks;
    options.modules.imageResizing = imageResizing.ImageResizingPopoverModule;

    return options;
}
