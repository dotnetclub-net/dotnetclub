
export function CodeButton(context) {
    return $.summernote.ui.button({
        contents: '<i class="note-icon-code"/>',
        tooltip: '插入代码',
        click: function () {
            // invoke formatBlock method with 'PRE' on editor module.
            context.invoke('editor.formatBlock', 'PRE');
        }
    }).render();
}

export function CodePopoverModule(context) {
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
        var height = $placeholder.outerHeight(true);

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
                        var valueClicked = $(event.target).closest('[data-value]').data('value');
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

export function PreventToolsInCodeBlocks(context) {
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

export function patchPreTag(editor, ev){
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
