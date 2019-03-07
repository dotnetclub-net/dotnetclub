"use strict";
exports.__esModule = true;
var renderer_1 = require("../base/renderer");
var TooltipUI_1 = require("./ui/TooltipUI");
var DropdownUI_1 = require("./ui/DropdownUI");
var ModalUI_1 = require("./ui/ModalUI");
var editor = renderer_1["default"].create('<div class="note-editor note-frame"/>');
var toolbar = renderer_1["default"].create('<div class="note-toolbar" role="toolbar"/>');
var editingArea = renderer_1["default"].create('<div class="note-editing-area"/>');
var codable = renderer_1["default"].create('<textarea class="note-codable" role="textbox" aria-multiline="true"/>');
var editable = renderer_1["default"].create('<div class="note-editable" contentEditable="true" role="textbox" aria-multiline="true"/>');
var statusbar = renderer_1["default"].create([
    '<output class="note-status-output" role="status" aria-live="polite"/>',
    '<div class="note-statusbar" role="resize">',
    '  <div class="note-resizebar" role="seperator" aria-orientation="horizontal" aria-label="resize">',
    '    <div class="note-icon-bar"/>',
    '    <div class="note-icon-bar"/>',
    '    <div class="note-icon-bar"/>',
    '  </div>',
    '</div>'
].join(''));
var airEditor = renderer_1["default"].create('<div class="note-editor"/>');
var airEditable = renderer_1["default"].create([
    '<div class="note-editable" contentEditable="true" role="textbox" aria-multiline="true"/>',
    '<output class="note-status-output" role="status" aria-live="polite"/>'
].join(''));
var buttonGroup = renderer_1["default"].create('<div class="note-btn-group">');
var button = renderer_1["default"].create('<button type="button" class="note-btn" role="button" tabindex="-1">', function ($node, options) {
    // set button type
    if (options && options.tooltip) {
        $node.attr({
            'aria-label': options.tooltip
        });
        $node.data('_lite_tooltip', new TooltipUI_1["default"]($node, {
            title: options.tooltip,
            container: options.container
        }));
    }
    if (options.contents) {
        $node.html(options.contents);
    }
    if (options && options.data && options.data.toggle === 'dropdown') {
        $node.data('_lite_dropdown', new DropdownUI_1["default"]($node, {
            container: options.container
        }));
    }
});
var dropdown = renderer_1["default"].create('<div class="note-dropdown-menu" role="list">', function ($node, options) {
    var markup = $.isArray(options.items) ? options.items.map(function (item) {
        var value = (typeof item === 'string') ? item : (item.value || '');
        var content = options.template ? options.template(item) : item;
        var $temp = $('<a class="note-dropdown-item" href="#" data-value="' + value + '" role="listitem" aria-label="' + item + '"></a>');
        $temp.html(content).data('item', item);
        return $temp;
    }) : options.items;
    $node.html(markup).attr({ 'aria-label': options.title });
    $node.on('click', '> .note-dropdown-item', function (e) {
        var $a = $(this);
        var item = $a.data('item');
        var value = $a.data('value');
        if (item.click) {
            item.click($a);
        }
        else if (options.itemClick) {
            options.itemClick(e, item, value);
        }
    });
});
var dropdownCheck = renderer_1["default"].create('<div class="note-dropdown-menu note-check" role="list">', function ($node, options) {
    var markup = $.isArray(options.items) ? options.items.map(function (item) {
        var value = (typeof item === 'string') ? item : (item.value || '');
        var content = options.template ? options.template(item) : item;
        var $temp = $('<a class="note-dropdown-item" href="#" data-value="' + value + '" role="listitem" aria-label="' + item + '"></a>');
        $temp.html([icon(options.checkClassName), ' ', content]).data('item', item);
        return $temp;
    }) : options.items;
    $node.html(markup).attr({ 'aria-label': options.title });
    $node.on('click', '> .note-dropdown-item', function (e) {
        var $a = $(this);
        var item = $a.data('item');
        var value = $a.data('value');
        if (item.click) {
            item.click($a);
        }
        else if (options.itemClick) {
            options.itemClick(e, item, value);
        }
    });
});
var dropdownButtonContents = function (contents, options) {
    return contents + ' ' + icon(options.icons.caret, 'span');
};
var dropdownButton = function (opt, callback) {
    return buttonGroup([
        button({
            className: 'dropdown-toggle',
            contents: opt.title + ' ' + icon('note-icon-caret'),
            tooltip: opt.tooltip,
            data: {
                toggle: 'dropdown'
            }
        }),
        dropdown({
            className: opt.className,
            items: opt.items,
            template: opt.template,
            itemClick: opt.itemClick
        })
    ], { callback: callback }).render();
};
var dropdownCheckButton = function (opt, callback) {
    return buttonGroup([
        button({
            className: 'dropdown-toggle',
            contents: opt.title + ' ' + icon('note-icon-caret'),
            tooltip: opt.tooltip,
            data: {
                toggle: 'dropdown'
            }
        }),
        dropdownCheck({
            className: opt.className,
            checkClassName: opt.checkClassName,
            items: opt.items,
            template: opt.template,
            itemClick: opt.itemClick
        })
    ], { callback: callback }).render();
};
var paragraphDropdownButton = function (opt) {
    return buttonGroup([
        button({
            className: 'dropdown-toggle',
            contents: opt.title + ' ' + icon('note-icon-caret'),
            tooltip: opt.tooltip,
            data: {
                toggle: 'dropdown'
            }
        }),
        dropdown([
            buttonGroup({
                className: 'note-align',
                children: opt.items[0]
            }),
            buttonGroup({
                className: 'note-list',
                children: opt.items[1]
            })
        ])
    ]).render();
};
var tableMoveHandler = function (event, col, row) {
    var PX_PER_EM = 18;
    var $picker = $(event.target.parentNode); // target is mousecatcher
    var $dimensionDisplay = $picker.next();
    var $catcher = $picker.find('.note-dimension-picker-mousecatcher');
    var $highlighted = $picker.find('.note-dimension-picker-highlighted');
    var $unhighlighted = $picker.find('.note-dimension-picker-unhighlighted');
    var posOffset;
    // HTML5 with jQuery - e.offsetX is undefined in Firefox
    if (event.offsetX === undefined) {
        var posCatcher = $(event.target).offset();
        posOffset = {
            x: event.pageX - posCatcher.left,
            y: event.pageY - posCatcher.top
        };
    }
    else {
        posOffset = {
            x: event.offsetX,
            y: event.offsetY
        };
    }
    var dim = {
        c: Math.ceil(posOffset.x / PX_PER_EM) || 1,
        r: Math.ceil(posOffset.y / PX_PER_EM) || 1
    };
    $highlighted.css({ width: dim.c + 'em', height: dim.r + 'em' });
    $catcher.data('value', dim.c + 'x' + dim.r);
    if (dim.c > 3 && dim.c < col) {
        $unhighlighted.css({ width: dim.c + 1 + 'em' });
    }
    if (dim.r > 3 && dim.r < row) {
        $unhighlighted.css({ height: dim.r + 1 + 'em' });
    }
    $dimensionDisplay.html(dim.c + ' x ' + dim.r);
};
var tableDropdownButton = function (opt) {
    return buttonGroup([
        button({
            className: 'dropdown-toggle',
            contents: opt.title + ' ' + icon('note-icon-caret'),
            tooltip: opt.tooltip,
            data: {
                toggle: 'dropdown'
            }
        }),
        dropdown({
            className: 'note-table',
            items: [
                '<div class="note-dimension-picker">',
                '  <div class="note-dimension-picker-mousecatcher" data-event="insertTable" data-value="1x1"/>',
                '  <div class="note-dimension-picker-highlighted"/>',
                '  <div class="note-dimension-picker-unhighlighted"/>',
                '</div>',
                '<div class="note-dimension-display">1 x 1</div>'
            ].join('')
        })
    ], {
        callback: function ($node) {
            var $catcher = $node.find('.note-dimension-picker-mousecatcher');
            $catcher.css({
                width: opt.col + 'em',
                height: opt.row + 'em'
            })
                .mousedown(opt.itemClick)
                .mousemove(function (e) {
                tableMoveHandler(e, opt.col, opt.row);
            });
        }
    }).render();
};
var palette = renderer_1["default"].create('<div class="note-color-palette"/>', function ($node, options) {
    var contents = [];
    for (var row = 0, rowSize = options.colors.length; row < rowSize; row++) {
        var eventName = options.eventName;
        var colors = options.colors[row];
        var colorsName = options.colorsName[row];
        var buttons = [];
        for (var col = 0, colSize = colors.length; col < colSize; col++) {
            var color = colors[col];
            var colorName = colorsName[col];
            buttons.push([
                '<button type="button" class="note-btn note-color-btn"',
                'style="background-color:', color, '" ',
                'data-event="', eventName, '" ',
                'data-value="', color, '" ',
                'title="', colorName, '" ',
                'aria-label="', colorName, '" ',
                'data-toggle="button" tabindex="-1"></button>'
            ].join(''));
        }
        contents.push('<div class="note-color-row">' + buttons.join('') + '</div>');
    }
    $node.html(contents.join(''));
    $node.find('.note-color-btn').each(function () {
        $(this).data('_lite_tooltip', new TooltipUI_1["default"]($(this), {
            container: options.container
        }));
    });
});
var colorDropdownButton = function (opt, type) {
    return buttonGroup({
        className: 'note-color',
        children: [
            button({
                className: 'note-current-color-button',
                contents: opt.title,
                tooltip: opt.lang.color.recent,
                click: opt.currentClick,
                callback: function ($button) {
                    var $recentColor = $button.find('.note-recent-color');
                    if (type !== 'foreColor') {
                        $recentColor.css('background-color', '#FFFF00');
                        $button.attr('data-backColor', '#FFFF00');
                    }
                }
            }),
            button({
                className: 'dropdown-toggle',
                contents: icon('note-icon-caret'),
                tooltip: opt.lang.color.more,
                data: {
                    toggle: 'dropdown'
                }
            }),
            dropdown({
                items: [
                    '<div>',
                    '<div class="note-btn-group btn-background-color">',
                    '  <div class="note-palette-title">' + opt.lang.color.background + '</div>',
                    '  <div>',
                    '<button type="button" class="note-color-reset note-btn note-btn-block" ' +
                        ' data-event="backColor" data-value="inherit">',
                    opt.lang.color.transparent,
                    '    </button>',
                    '  </div>',
                    '  <div class="note-holder" data-event="backColor"/>',
                    '  <div class="btn-sm">',
                    '    <input type="color" id="html5bcp" class="note-btn btn-default" value="#21104A" style="width:100%;" data-value="cp">',
                    '    <button type="button" class="note-color-reset btn" data-event="backColor" data-value="cpbackColor">',
                    opt.lang.color.cpSelect,
                    '    </button>',
                    '  </div>',
                    '</div>',
                    '<div class="note-btn-group btn-foreground-color">',
                    '  <div class="note-palette-title">' + opt.lang.color.foreground + '</div>',
                    '  <div>',
                    '<button type="button" class="note-color-reset note-btn note-btn-block" ' +
                        ' data-event="removeFormat" data-value="foreColor">',
                    opt.lang.color.resetToDefault,
                    '    </button>',
                    '  </div>',
                    '  <div class="note-holder" data-event="foreColor"/>',
                    '  <div class="btn-sm">',
                    '    <input type="color" id="html5fcp" class="note-btn btn-default" value="#21104A" style="width:100%;" data-value="cp">',
                    '    <button type="button" class="note-color-reset btn" data-event="foreColor" data-value="cpforeColor">',
                    opt.lang.color.cpSelect,
                    '    </button>',
                    '  </div>',
                    '</div>',
                    '</div>'
                ].join(''),
                callback: function ($dropdown) {
                    $dropdown.find('.note-holder').each(function () {
                        var $holder = $(this);
                        $holder.append(palette({
                            colors: opt.colors,
                            eventName: $holder.data('event')
                        }).render());
                    });
                    if (type === 'fore') {
                        $dropdown.find('.btn-background-color').hide();
                        $dropdown.css({ 'min-width': '210px' });
                    }
                    else if (type === 'back') {
                        $dropdown.find('.btn-foreground-color').hide();
                        $dropdown.css({ 'min-width': '210px' });
                    }
                },
                click: function (event) {
                    var $button = $(event.target);
                    var eventName = $button.data('event');
                    var value = $button.data('value');
                    var foreinput = document.getElementById('html5fcp').value;
                    var backinput = document.getElementById('html5bcp').value;
                    if (value === 'cp') {
                        event.stopPropagation();
                    }
                    else if (value === 'cpbackColor') {
                        value = backinput;
                    }
                    else if (value === 'cpforeColor') {
                        value = foreinput;
                    }
                    if (eventName && value) {
                        var key = eventName === 'backColor' ? 'background-color' : 'color';
                        var $color = $button.closest('.note-color').find('.note-recent-color');
                        var $currentButton = $button.closest('.note-color').find('.note-current-color-button');
                        $color.css(key, value);
                        $currentButton.attr('data-' + eventName, value);
                        if (type === 'fore') {
                            opt.itemClick('foreColor', value);
                        }
                        else if (type === 'back') {
                            opt.itemClick('backColor', value);
                        }
                        else {
                            opt.itemClick(eventName, value);
                        }
                    }
                }
            })
        ]
    }).render();
};
var dialog = renderer_1["default"].create('<div class="note-modal" aria-hidden="false" tabindex="-1" role="dialog"/>', function ($node, options) {
    if (options.fade) {
        $node.addClass('fade');
    }
    $node.attr({
        'aria-label': options.title
    });
    $node.html([
        '  <div class="note-modal-content">',
        (options.title
            ? '    <div class="note-modal-header">' +
                '      <button type="button" class="close" aria-label="Close" aria-hidden="true"><i class="note-icon-close"></i></button>' +
                '      <h4 class="note-modal-title">' + options.title + '</h4>' +
                '    </div>' : ''),
        '    <div class="note-modal-body">' + options.body + '</div>',
        (options.footer
            ? '    <div class="note-modal-footer">' + options.footer + '</div>' : ''),
        '  </div>'
    ].join(''));
    $node.data('modal', new ModalUI_1["default"]($node, options));
});
var videoDialog = function (opt) {
    var body = '<div class="note-form-group">' +
        '<label class="note-form-label">' +
        opt.lang.video.url + ' <small class="text-muted">' +
        opt.lang.video.providers + '</small>' +
        '</label>' +
        '<input class="note-video-url note-input" type="text" />' +
        '</div>';
    var footer = [
        '<button type="button" href="#" class="note-btn note-btn-primary note-video-btn disabled" disabled>',
        opt.lang.video.insert,
        '</button>'
    ].join('');
    return dialog({
        title: opt.lang.video.insert,
        fade: opt.fade,
        body: body,
        footer: footer
    }).render();
};
var imageDialog = function (opt) {
    var body = '<div class="note-form-group note-group-select-from-files">' +
        '<label class="note-form-label">' + opt.lang.image.selectFromFiles + '</label>' +
        '<input class="note-note-image-input note-input" type="file" name="files" accept="image/*" multiple="multiple" />' +
        opt.imageLimitation +
        '</div>' +
        '<div class="note-form-group" style="overflow:auto;">' +
        '<label class="note-form-label">' + opt.lang.image.url + '</label>' +
        '<input class="note-image-url note-input" type="text" />' +
        '</div>';
    var footer = [
        '<button href="#" type="button" class="note-btn note-btn-primary note-btn-large note-image-btn disabled" disabled>',
        opt.lang.image.insert,
        '</button>'
    ].join('');
    return dialog({
        title: opt.lang.image.insert,
        fade: opt.fade,
        body: body,
        footer: footer
    }).render();
};
var linkDialog = function (opt) {
    var body = '<div class="note-form-group">' +
        '<label class="note-form-label">' + opt.lang.link.textToDisplay + '</label>' +
        '<input class="note-link-text note-input" type="text" />' +
        '</div>' +
        '<div class="note-form-group">' +
        '<label class="note-form-label">' + opt.lang.link.url + '</label>' +
        '<input class="note-link-url note-input" type="text" value="http://" />' +
        '</div>' +
        (!opt.disableLinkTarget
            ? '<div class="checkbox">' +
                '<label>' + '<input type="checkbox" checked> ' + opt.lang.link.openInNewWindow + '</label>' +
                '</div>' : '');
    var footer = [
        '<button href="#" type="button" class="note-btn note-btn-primary note-link-btn disabled" disabled>',
        opt.lang.link.insert,
        '</button>'
    ].join('');
    return dialog({
        className: 'link-dialog',
        title: opt.lang.link.insert,
        fade: opt.fade,
        body: body,
        footer: footer
    }).render();
};
var popover = renderer_1["default"].create([
    '<div class="note-popover bottom">',
    '  <div class="note-popover-arrow"/>',
    '  <div class="popover-content note-children-container"/>',
    '</div>'
].join(''), function ($node, options) {
    var direction = typeof options.direction !== 'undefined' ? options.direction : 'bottom';
    $node.addClass(direction).hide();
    if (options.hideArrow) {
        $node.find('.note-popover-arrow').hide();
    }
});
var checkbox = renderer_1["default"].create('<div class="checkbox"></div>', function ($node, options) {
    $node.html([
        '<label' + (options.id ? ' for="' + options.id + '"' : '') + '>',
        ' <input role="checkbox" type="checkbox"' + (options.id ? ' id="' + options.id + '"' : ''),
        (options.checked ? ' checked' : ''),
        ' aria-checked="' + (options.checked ? 'true' : 'false') + '"/>',
        (options.text ? options.text : ''),
        '</label>'
    ].join(''));
});
var icon = function (iconClassName, tagName) {
    tagName = tagName || 'i';
    return '<' + tagName + ' class="' + iconClassName + '"/>';
};
var ui = {
    editor: editor,
    toolbar: toolbar,
    editingArea: editingArea,
    codable: codable,
    editable: editable,
    statusbar: statusbar,
    airEditor: airEditor,
    airEditable: airEditable,
    buttonGroup: buttonGroup,
    button: button,
    dropdown: dropdown,
    dropdownCheck: dropdownCheck,
    dropdownButton: dropdownButton,
    dropdownButtonContents: dropdownButtonContents,
    dropdownCheckButton: dropdownCheckButton,
    paragraphDropdownButton: paragraphDropdownButton,
    tableDropdownButton: tableDropdownButton,
    colorDropdownButton: colorDropdownButton,
    palette: palette,
    dialog: dialog,
    videoDialog: videoDialog,
    imageDialog: imageDialog,
    linkDialog: linkDialog,
    popover: popover,
    checkbox: checkbox,
    icon: icon,
    toggleBtn: function ($btn, isEnable) {
        $btn.toggleClass('disabled', !isEnable);
        $btn.attr('disabled', !isEnable);
    },
    toggleBtnActive: function ($btn, isActive) {
        $btn.toggleClass('active', isActive);
    },
    check: function ($dom, value) {
        $dom.find('.checked').removeClass('checked');
        $dom.find('[data-value="' + value + '"]').addClass('checked');
    },
    onDialogShown: function ($dialog, handler) {
        $dialog.one('note.modal.show', handler);
    },
    onDialogHidden: function ($dialog, handler) {
        $dialog.one('note.modal.hide', handler);
    },
    showDialog: function ($dialog) {
        $dialog.data('modal').show();
    },
    hideDialog: function ($dialog) {
        $dialog.data('modal').hide();
    },
    /**
     * get popover content area
     *
     * @param $popover
     * @returns {*}
     */
    getPopoverContent: function ($popover) {
        return $popover.find('.note-popover-content');
    },
    /**
     * get dialog's body area
     *
     * @param $dialog
     * @returns {*}
     */
    getDialogBody: function ($dialog) {
        return $dialog.find('.note-modal-body');
    },
    createLayout: function ($note, options) {
        var $editor = (options.airMode ? ui.airEditor([
            ui.editingArea([
                ui.airEditable()
            ])
        ]) : ui.editor([
            ui.toolbar(),
            ui.editingArea([
                ui.codable(),
                ui.editable()
            ]),
            ui.statusbar()
        ])).render();
        $editor.insertAfter($note);
        return {
            note: $note,
            editor: $editor,
            toolbar: $editor.find('.note-toolbar'),
            editingArea: $editor.find('.note-editing-area'),
            editable: $editor.find('.note-editable'),
            codable: $editor.find('.note-codable'),
            statusbar: $editor.find('.note-statusbar')
        };
    },
    removeLayout: function ($note, layoutInfo) {
        $note.html(layoutInfo.editable.html());
        layoutInfo.editor.remove();
        $note.off('summernote'); // remove summernote custom event
        $note.show();
    }
};
exports["default"] = ui;
//# sourceMappingURL=ui.js.map