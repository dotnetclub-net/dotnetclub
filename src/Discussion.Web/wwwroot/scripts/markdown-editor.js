// -2. remove all illegal tags (or only allow tag white-list)
// -3. should remove all attributes & styles when paste
// 4. handle image uploading...
// 5. process pasting code with HTML tags...
// 6. should not use keyboard shortcuts in PRE


// find elements within  note-editable
// popover toolbar may be overflow editor height!
// make this translatable: 选择语言

(function(){

$(document).ready(function() {
    var editorOptions = defaultEditorOptions();
    editorOptions.callbacks.onPaste = function (ev) {
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
            var htmlOptions = htmlFragmentOptions();
            processTag(copiedNode[0], null, htmlOptions);
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
    };

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
        var htmlOptions = htmlFragmentOptions();
        processTag(element[0], null, htmlOptions);

        var mdContent = convertToMarkdown(element.html());
        var encodedContent = htmlEncode(mdContent);


        var title = $('#new-topic-title').val();
        var topicType = $('#topic-type-dropdown .topic-type-item[selected]>a').attr('attr-value');
        var newTopic = {
            title: htmlEncode(title),
            content: encodedContent,
            type: topicType
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
});

function defaultEditorOptions(){
    var defaultOptions = $.extend({}, $.summernote.options);
    var options = $.extend({}, defaultOptions, {
        toolbar: [
            ['style', ['style']],
            ['format', ['bold', 'italic', 'strikethrough', 'clear']],
            ['para', ['ul', 'ol']],
            ['insert', ['link', 'picture', 'insertCode']]
        ],
        styleTags: [
            {value: 'p', title: 'Paragraph', tag: 'span'},
            {value: 'h3', title: 'Header', tag: 'span' },
            {value: 'blockquote', title: 'Quote', tag: 'span' }],
        codeLanguages: [
            {title: 'C#', value: 'csharp'},
            {title: 'Razor(C#)', value: 'cshtml'},
            {title: 'JavaScript', value: 'javascript'},
            {title: 'HTML', value: 'html'},
            {title: 'CSS', value: 'css'}
        ],
        height: 300,
        callbacks: {
            onChange: function () { },
            onEnter: function(ev){
                ev.preventDefault();
                ev.stopPropagation();

                var editor = $(this).data('summernote');
                var patchedPre = patchPreTag.apply(this, [editor, ev]);
                if(!patchedPre){
                    editor.invoke('editor.insertParagraph');
                }
            }
        },
        buttons:{
            insertCode: CodeButton
        }
    });
    options.modules.codePopover = CodePopover;
    options.modules.preventToolsInCodeBlocks = PreventToolsInCodeBlocks;
    options.popover.codeLang = [
        ['codeLang', ['chooseCodeLanguage']]
    ];

    return options;
}

function CodeButton(context) {
    var ui = $.summernote.ui;

    // create button
    var button = ui.button({
        contents: '<i class="note-icon-code"/>',
        tooltip: 'Insert Code',
        click: function () {
            // invoke formatBlock method with 'PRE' on editor module.
            context.invoke('editor.formatBlock', 'PRE');
        }
    });

    return button.render();   // return button as jquery object
}

function CodePopover(context) {
    var self = this;
    var ui = $.summernote.ui;
    var options = context.options;
    var langList = [];
    var currentTarget;

    this.events = {
        'summernote.mouseup': function () {
            self.update();
        },
        'summernote.keyup summernote.dialog.shown summernote.scroll': function () {
            self.hide();
        }
    };

    this.shouldInitialize = function () {
        return true;
    };

    this.initialize = function () {
        setupLanguageList();

        this.$popover = ui.popover({
            className: 'note-code-popover',
            callback: function ($node) {  }
        }).render().appendTo('body');

        context.invoke('buttons.build', this.$popover.find('.popover-content'), options.popover.codeLang);
    };

    this.destroy = function () {
        this.$popover.remove();
    };

    this.update = function () {
        // Prevent focusing on editable when invoke('code') is executed
        if (!context.invoke('editor.hasFocus')) {
            this.hide();
            return;
        }

        var rng = context.invoke('editor.createRange');
        var pre;
        if (rng.isCollapsed() && (pre = closestPRE(rng.sc))) {
            currentTarget = pre;
            var pos = posFromPlaceholder(pre);

            this.$popover.css({
                display: 'block',
                left: pos.left,
                top: pos.top
            });

            var language = pre.attr('language');
            var dropdownButton = this.$popover.find('.dropdown-toggle .lang');
            var item = findLangItem(language);
            dropdownButton.text((item && item.title) || '选择语言');
        } else {
            this.hide();
        }
    };

    this.hide = function () {
        this.$popover.hide();
    };


    function posFromPlaceholder($placeholder) {
        var pos = $placeholder.offset();
        var height = $placeholder.outerHeight(true); // include margin

        return {
            left: pos.left,
            top: pos.top + height
        };
    }

    function setupLanguageList(){
        langList = options.codeLanguages || [
                {title: 'C#', value: 'csharp'},
                {title: 'Razor(C#)', value: 'cshtml'},
                {title: 'JavaScript', value: 'javascript'},
                {title: 'HTML', value: 'html'},
                {title: 'CSS', value: 'css'},
                {title: 'Razor(VB)', value: 'vbhtml'},
                {title: 'VB', value: 'vb'},
                {title: 'Java', value: 'java'},
                {title: 'PHP', value: 'php'},
                {title: 'PowerShell', value: 'powershell'},
                {title: 'Shell', value: 'sh'}
            ];

        context.memo('button.chooseCodeLanguage', function () {
            return ui.buttonGroup([
                ui.button({
                    className: 'dropdown-toggle',
                    contents: '<span class="lang">选择语言</span> ' + ui.icon(options.icons.caret, 'span'),
                    // tooltip: '选择语言', // lang.style.style,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                ui.dropdown({
                    className: 'dropdown-style',
                    items: langList,
                    template: function (item) {
                        var title = item.title;
                        var style = item.style ? ' style="' + item.style + '" ' : '';
                        var className = item.className ? ' className="' + item.className + '"' : '';

                        return '<span'+ style + className + '>' + title + '</span>';
                    },
                    click: function(event){
                        if(!currentTarget){
                            return;
                        }
                        event.preventDefault();

                        var langItem, langTitle;
                        var valueClicked = $(event.target).closest('[data-value]').data('value')
                        langItem = findLangItem(valueClicked);

                        if(langItem) {
                            currentTarget.attr('language', langItem.value);
                            langTitle = $(event.target).closest('.dropdown-menu').prev('.dropdown-toggle').find('.lang');
                            langTitle.text(langItem.title);
                            context.invoke('editor.afterCommand');
                            context.invoke('editor.focus');
                        }
                    }
                })
            ]).render();
        });
    }

    function findLangItem(val){
        var langItem;
        $.each(langList, function (idx, item) {
            if(item.value === val){
                langItem = item;
                return false;
            }
        });
        return langItem;
    }
}

function PreventToolsInCodeBlocks(context) {
    var self = this;

    this.events = {
        'summernote.keyup summernote.mouseup summernote.change summernote.scroll': function () {
            self.update();
        }
    };

    this.initialize = function () {

    };

    this.update = function () {
        if (!context.invoke('editor.hasFocus')) {
            context.invoke('toolbar.activate', true);
            return;
        }

        var rng = context.invoke('editor.createRange');
        var sc = closestPRE(rng.sc), ec = closestPRE(rng.ec);
        if (sc && ec && sc[0] === ec[0]) {
            context.invoke('toolbar.deactivate', true);
        }else{
            context.invoke('toolbar.activate', true);
        }

        // context.invoke('editor.focus');
    }
}

function closestPRE(el){
    var $el = $(el);
    var $pre = $el.is('pre') ? $el : $el.parents('pre');
    if(!$pre.length){
        return null;
    }

    return $pre;
}

function patchPreTag(editor, ev){
    var range = editor.invoke('editor.createRange');
    var $pre = $(range.sc).parents('pre');

    if (ev.ctrlKey || ev.metaKey || !$pre.length){   // in edge, when hit enter quickly after click at end, can still fail  (may be use find method from this page can solve finding container:  http://stackoverflow.com/questions/14667764/in-contenteditable-how-do-you-add-a-paragraph-after-blockquote-on-enter-key-pres)
        return;
    }

    appendLine();

    var isEdge = /Edge\/\d+/.test(navigator.userAgent);
    if(isEdge) {           // in edge, sometimes can still fail  (when hit DELETE at end, then ENTER)
        var linebreakPatch = 'linebreak-patch';
        var atEnd = range.eo + 2 >= $pre[0].textContent.length;

        if (atEnd && !$pre.data(linebreakPatch)) {
            $pre.data(linebreakPatch, true);
            appendLine();
        }
    }
    return true;

    function appendLine() {
        editor.invoke('editor.insertText', '\n ');
        document.execCommand('delete', false);
    }
}

function convertToMarkdown(htmlContent) {
    var markdownOptions = {
        converters: [
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
                filter: 'br',
                replacement: function () {
                    // http://stackoverflow.com/a/28633712/1817042
                    return '&nbsp;\n\n';
                }
            },
            // Fenced code blocks
            {
                filter: 'pre',
                replacement: function (content, node) {
                    // to-markdown supports Syntax-highlighted code blocks (search 'Syntax-highlighted code blocks' in to-markdown.js)
                    var language = node.getAttribute('language') || '';
                    return '\n\n``` ' + language + '\n' + node.firstChild.textContent + '\n```\n\n';
                }
            }
        ]
    };

    var markdown = toMarkdown(htmlContent, markdownOptions);
    return markdown;
}

function htmlFragmentOptions(){
    var illegalTags = [
        'script',
        'style',
        'link',
        'iframe',
        'frameset',
        'object',
        'embed',
        'video',
        'audio',
        'canvas',
        'svg',
        'html',
        'head',
        'body',
        'meta',
        'input',
        'select',
        'option',
        'button'
    ];

    var blockTags = [
        'div',
        'main',
        'nav',
        'aside',
        'section',
        'article',
        'footer',
        'header',
        'figure',
        'form',
        'legend',
        'fieldset',
        'dl',
        'dt',
        'dd',
        'h1',
        'h2',
        'h4',
        'h5',
        'h6'
    ];

    var allowedTags = [
        'p',
        'br',
        'h3',
        'blockquote',
        'strong',
        'b',
        'em',
        'i',
        'strike',
        'ul',
        'ol',
        'li',
        'a',
        'img',
        'pre'
    ];
    var attributes = {
        'a': ['href', 'title', 'name'],
        'img': ['src', 'alt', 'title'],
        'pre': ['language']
    };

    return {
        illegalTags: illegalTags,
        blockTags: blockTags,
        allowedTags: allowedTags,
        allowedAttributes: attributes
    };
}

function processTag(node, parentNode, options){
    if(node.childNodes.length){
        processTag(node.childNodes[0], node, options);
    }

    if(!parentNode){
        return;
    }

    if(node.nextSibling){
        processTag(node.nextSibling, parentNode, options);
    }

    var transformedNode = processSingleTag(node, options);
    if(transformedNode !== node){
        if (!transformedNode.nodeName){
            $.each(transformedNode, function (i, p) {
                parentNode.insertBefore(p, node);
            });
        } else {
            parentNode.insertBefore(transformedNode, node);
        }
        parentNode.removeChild(node);
    }
}

function processSingleTag(node, options){
    if(node.nodeType === 3){
        return node;
    }

    if(node.nodeName === 'A' && !$.trim(node.textContent) && !node.childNodes.length){
        return document.createTextNode('');
    }

    if(node.nodeType !== 1 || isIllegal(node.nodeName)){
        return document.createTextNode('');
    }

    if(isAllowed(node.nodeName)){
        stripAttributes(node);
        return node;
    }

    if(isBlockTag(node.nodeName)){
        return createParagraphs(node.childNodes);
    }

    return document.createTextNode(node.textContent);

    function createParagraphs(nodes){
        var paragraphs = [];
        var p, node;

        for(var i=0;i<nodes.length;i++){
            node = nodes[i];
            if(!p){
                p = document.createElement('P');
                paragraphs.push(p);
            }

            if(node.nodeName !== 'P'){
                p.appendChild(node);
            }else{
                paragraphs.push(nodes[i]);
                p = null;
            }
        }

        return paragraphs;
    }


    function isIllegal(tagName) {
        var illegal = false;
        $.each(options.illegalTags, function(i, tag){
            if(equals(tagName, tag)){
                illegal = true;
                return false;
            }
        });
        return illegal;
    }

    function isAllowed(tagName) {
        var allowed = false;
        $.each(options.allowedTags, function(i, tag){
            if(equals(tagName, tag)){
                allowed = true;
                return false;
            }
        });
        return allowed;
    }

    function isBlockTag(tagName){
        var isBlock = false;
        $.each(options.blockTags, function(i, tag){
            if(equals(tagName, tag)){
                isBlock = true;
                return false;
            }
        });
        return isBlock;
    }

    function stripAttributes(node){
        var attributesToRemove = [];
        $.each(node.attributes, function(i, attr){
            var tag = node.nodeName.toLowerCase();

            if(!options.allowedAttributes[tag] || options.allowedAttributes[tag].indexOf(attr.name) < 0){
                attributesToRemove.push(attr.name);
            }
        });

        $.each(attributesToRemove, function(i, attr){
            node.removeAttribute(attr);
        });
    }

    function equals(first, second){
        return first.toUpperCase() === second.toUpperCase();
    }
}

function htmlEncode(content){
    // todo: encode & to &amp;    .replace(/&(?!(amp|nbsp|copy);)/g, '&amp;')
    return content.replace(/</g, '&lt;')
                  .replace(/>/g, '&gt;')
                  .replace(/&#[\d]+;/g, '');
}

})();