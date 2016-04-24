// 2. remove all illegal tags (or only allow tag white-list)
// 3. should remove all attributes & styles when paste
// 4. handle image uploading...
// 6. should not use keyboard shortcuts in PRE


// find elements within  note-editable
// popover toolbar may be overflow editor height!
// make this translatable: 选择语言


$(document).ready(function() {
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
            onChange: function () {
                convertToMarkdown();
            },
            onEnter: patchPreTag
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

    $('#editor').summernote(options);
    $('#editor-mapped').summernote({
        height: 300,
        toolbar: [
            ['format', ['bold', 'italic', 'strikethrough', 'clear']],
            ['para', ['ul', 'ol']]
        ],
        callbacks: { }
    });
});

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

function patchPreTag(ev){
    var editor = $('#editor').data('summernote');
    var range = editor.invoke('editor.createRange');
    var $pre = $(range.sc).parents('pre');

    if (!$pre.length){   // in edge, when hit enter quickly after click at end, can still fail  (may be use find method from this page can solve finding container:  http://stackoverflow.com/questions/14667764/in-contenteditable-how-do-you-add-a-paragraph-after-blockquote-on-enter-key-pres)
        return;
    }

    ev.preventDefault();
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

    function appendLine() {
        editor.invoke('editor.insertText', '\n ');
        document.execCommand('delete', false);
    }
}

function convertToMarkdown(){
    var editor = $('#editor').data('summernote');
    var htmlContent = editor.code();

    var markdown = toMarkdown(htmlContent, {
        converters: [
            {
                filter: 'strike',
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
            },{
                filter: 'br',
                replacement: function () {
                    // http://stackoverflow.com/a/28633712/1817042
                    return '&nbsp;\n\n';
                }
            },
            // Fenced code blocks
            {
                filter: function (node) {
                    return node.nodeName === 'PRE';
                },
                replacement: function(content, node) {
                    // to-markdown supports Syntax-highlighted code blocks (search 'Syntax-highlighted code blocks' in to-markdown.js)
                    var language = node.getAttribute('language') || '';
                    return '\n\n```' + language + '\n' + node.firstChild.textContent + '\n```\n\n';
                }
            }
        ]
    });
    $('#markdown').val(markdown);

    var htmlConvertedBack = marked( markdown );
    $('#editor-mapped').data('summernote').code(htmlConvertedBack);
}