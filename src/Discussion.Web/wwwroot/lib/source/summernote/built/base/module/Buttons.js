"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var func_1 = require("../core/func");
var lists_1 = require("../core/lists");
var env_1 = require("../core/env");
var Buttons = /** @class */ (function () {
    function Buttons(context) {
        this.ui = jquery_1["default"].summernote.ui;
        this.context = context;
        this.$toolbar = context.layoutInfo.toolbar;
        this.options = context.options;
        this.lang = this.options.langInfo;
        this.invertedKeyMap = func_1["default"].invertObject(this.options.keyMap[env_1["default"].isMac ? 'mac' : 'pc']);
    }
    Buttons.prototype.representShortcut = function (editorMethod) {
        var shortcut = this.invertedKeyMap[editorMethod];
        if (!this.options.shortcuts || !shortcut) {
            return '';
        }
        if (env_1["default"].isMac) {
            shortcut = shortcut.replace('CMD', '⌘').replace('SHIFT', '⇧');
        }
        shortcut = shortcut.replace('BACKSLASH', '\\')
            .replace('SLASH', '/')
            .replace('LEFTBRACKET', '[')
            .replace('RIGHTBRACKET', ']');
        return ' (' + shortcut + ')';
    };
    Buttons.prototype.button = function (o) {
        if (!this.options.tooltip && o.tooltip) {
            delete o.tooltip;
        }
        o.container = this.options.container;
        return this.ui.button(o);
    };
    Buttons.prototype.initialize = function () {
        this.addToolbarButtons();
        this.addImagePopoverButtons();
        this.addLinkPopoverButtons();
        this.addTablePopoverButtons();
        this.fontInstalledMap = {};
    };
    Buttons.prototype.destroy = function () {
        delete this.fontInstalledMap;
    };
    Buttons.prototype.isFontInstalled = function (name) {
        if (!this.fontInstalledMap.hasOwnProperty(name)) {
            this.fontInstalledMap[name] = env_1["default"].isFontInstalled(name) ||
                lists_1["default"].contains(this.options.fontNamesIgnoreCheck, name);
        }
        return this.fontInstalledMap[name];
    };
    Buttons.prototype.isFontDeservedToAdd = function (name) {
        var genericFamilies = ['sans-serif', 'serif', 'monospace', 'cursive', 'fantasy'];
        name = name.toLowerCase();
        return ((name !== '') && this.isFontInstalled(name) && (jquery_1["default"].inArray(name, genericFamilies) === -1));
    };
    Buttons.prototype.colorPalette = function (className, tooltip, backColor, foreColor) {
        var _this = this;
        return this.ui.buttonGroup({
            className: 'note-color ' + className,
            children: [
                this.button({
                    className: 'note-current-color-button',
                    contents: this.ui.icon(this.options.icons.font + ' note-recent-color'),
                    tooltip: tooltip,
                    click: function (e) {
                        var $button = jquery_1["default"](e.currentTarget);
                        if (backColor && foreColor) {
                            _this.context.invoke('editor.color', {
                                backColor: $button.attr('data-backColor'),
                                foreColor: $button.attr('data-foreColor')
                            });
                        }
                        else if (backColor) {
                            _this.context.invoke('editor.color', {
                                backColor: $button.attr('data-backColor')
                            });
                        }
                        else if (foreColor) {
                            _this.context.invoke('editor.color', {
                                foreColor: $button.attr('data-foreColor')
                            });
                        }
                    },
                    callback: function ($button) {
                        var $recentColor = $button.find('.note-recent-color');
                        if (backColor) {
                            $recentColor.css('background-color', '#FFFF00');
                            $button.attr('data-backColor', '#FFFF00');
                        }
                        if (!foreColor) {
                            $recentColor.css('color', 'transparent');
                        }
                    }
                }),
                this.button({
                    className: 'dropdown-toggle',
                    contents: this.ui.dropdownButtonContents('', this.options),
                    tooltip: this.lang.color.more,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                this.ui.dropdown({
                    items: (backColor ? [
                        '<div class="note-palette">',
                        '  <div class="note-palette-title">' + this.lang.color.background + '</div>',
                        '  <div>',
                        '    <button type="button" class="note-color-reset btn btn-light" data-event="backColor" data-value="inherit">',
                        this.lang.color.transparent,
                        '    </button>',
                        '  </div>',
                        '  <div class="note-holder" data-event="backColor"/>',
                        '  <div>',
                        '    <button type="button" class="note-color-select btn" data-event="openPalette" data-value="backColorPicker">',
                        this.lang.color.cpSelect,
                        '    </button>',
                        '    <input type="color" id="backColorPicker" class="note-btn note-color-select-btn" value="#FFFF00" data-event="backColorPalette">',
                        '  </div>',
                        '  <div class="note-holder-custom" id="backColorPalette" data-event="backColor"/>',
                        '</div>'
                    ].join('') : '') +
                        (foreColor ? [
                            '<div class="note-palette">',
                            '  <div class="note-palette-title">' + this.lang.color.foreground + '</div>',
                            '  <div>',
                            '    <button type="button" class="note-color-reset btn btn-light" data-event="removeFormat" data-value="foreColor">',
                            this.lang.color.resetToDefault,
                            '    </button>',
                            '  </div>',
                            '  <div class="note-holder" data-event="foreColor"/>',
                            '  <div>',
                            '    <button type="button" class="note-color-select btn" data-event="openPalette" data-value="foreColorPicker">',
                            this.lang.color.cpSelect,
                            '    </button>',
                            '    <input type="color" id="foreColorPicker" class="note-btn note-color-select-btn" value="#000000" data-event="foreColorPalette">',
                            '  <div class="note-holder-custom" id="foreColorPalette" data-event="foreColor"/>',
                            '</div>'
                        ].join('') : ''),
                    callback: function ($dropdown) {
                        $dropdown.find('.note-holder').each(function (idx, item) {
                            var $holder = jquery_1["default"](item);
                            $holder.append(_this.ui.palette({
                                colors: _this.options.colors,
                                colorsName: _this.options.colorsName,
                                eventName: $holder.data('event'),
                                container: _this.options.container,
                                tooltip: _this.options.tooltip
                            }).render());
                        });
                        /* TODO: do we have to record recent custom colors within cookies? */
                        var customColors = [
                            ['#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF', '#FFFFFF']
                        ];
                        $dropdown.find('.note-holder-custom').each(function (idx, item) {
                            var $holder = jquery_1["default"](item);
                            $holder.append(_this.ui.palette({
                                colors: customColors,
                                colorsName: customColors,
                                eventName: $holder.data('event'),
                                container: _this.options.container,
                                tooltip: _this.options.tooltip
                            }).render());
                        });
                        $dropdown.find('input[type=color]').each(function (idx, item) {
                            jquery_1["default"](item).change(function () {
                                var $chip = $dropdown.find('#' + jquery_1["default"](this).data('event')).find('.note-color-btn').first();
                                var color = this.value.toUpperCase();
                                $chip.css('background-color', color)
                                    .attr('aria-label', color)
                                    .attr('data-value', color)
                                    .attr('data-original-title', color);
                                $chip.click();
                            });
                        });
                    },
                    click: function (event) {
                        event.stopPropagation();
                        var $parent = jquery_1["default"]('.' + className);
                        var $button = jquery_1["default"](event.target);
                        var eventName = $button.data('event');
                        var value = $button.attr('data-value');
                        if (eventName === 'openPalette') {
                            var $picker = $parent.find('#' + value);
                            var $palette = jquery_1["default"]($parent.find('#' + $picker.data('event')).find('.note-color-row')[0]);
                            // Shift palette chips
                            var $chip = $palette.find('.note-color-btn').last().detach();
                            // Set chip attributes
                            var color = $picker.val();
                            $chip.css('background-color', color)
                                .attr('aria-label', color)
                                .attr('data-value', color)
                                .attr('data-original-title', color);
                            $palette.prepend($chip);
                            $picker.click();
                        }
                        else if (lists_1["default"].contains(['backColor', 'foreColor'], eventName)) {
                            var key = eventName === 'backColor' ? 'background-color' : 'color';
                            var $color = $button.closest('.note-color').find('.note-recent-color');
                            var $currentButton = $button.closest('.note-color').find('.note-current-color-button');
                            $color.css(key, value);
                            $currentButton.attr('data-' + eventName, value);
                            _this.context.invoke('editor.' + eventName, value);
                        }
                    }
                })
            ]
        }).render();
    };
    Buttons.prototype.addToolbarButtons = function () {
        var _this = this;
        this.context.memo('button.style', function () {
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents(_this.ui.icon(_this.options.icons.magic), _this.options),
                    tooltip: _this.lang.style.style,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdown({
                    className: 'dropdown-style',
                    items: _this.options.styleTags,
                    title: _this.lang.style.style,
                    template: function (item) {
                        if (typeof item === 'string') {
                            item = { tag: item, title: (_this.lang.style.hasOwnProperty(item) ? _this.lang.style[item] : item) };
                        }
                        var tag = item.tag;
                        var title = item.title;
                        var style = item.style ? ' style="' + item.style + '" ' : '';
                        var className = item.className ? ' class="' + item.className + '"' : '';
                        return '<' + tag + style + className + '>' + title + '</' + tag + '>';
                    },
                    click: _this.context.createInvokeHandler('editor.formatBlock')
                })
            ]).render();
        });
        var _loop_1 = function (styleIdx, styleLen) {
            var item = this_1.options.styleTags[styleIdx];
            this_1.context.memo('button.style.' + item, function () {
                return _this.button({
                    className: 'note-btn-style-' + item,
                    contents: '<div data-value="' + item + '">' + item.toUpperCase() + '</div>',
                    tooltip: _this.lang.style[item],
                    click: _this.context.createInvokeHandler('editor.formatBlock')
                }).render();
            });
        };
        var this_1 = this;
        for (var styleIdx = 0, styleLen = this.options.styleTags.length; styleIdx < styleLen; styleIdx++) {
            _loop_1(styleIdx, styleLen);
        }
        this.context.memo('button.bold', function () {
            return _this.button({
                className: 'note-btn-bold',
                contents: _this.ui.icon(_this.options.icons.bold),
                tooltip: _this.lang.font.bold + _this.representShortcut('bold'),
                click: _this.context.createInvokeHandlerAndUpdateState('editor.bold')
            }).render();
        });
        this.context.memo('button.italic', function () {
            return _this.button({
                className: 'note-btn-italic',
                contents: _this.ui.icon(_this.options.icons.italic),
                tooltip: _this.lang.font.italic + _this.representShortcut('italic'),
                click: _this.context.createInvokeHandlerAndUpdateState('editor.italic')
            }).render();
        });
        this.context.memo('button.underline', function () {
            return _this.button({
                className: 'note-btn-underline',
                contents: _this.ui.icon(_this.options.icons.underline),
                tooltip: _this.lang.font.underline + _this.representShortcut('underline'),
                click: _this.context.createInvokeHandlerAndUpdateState('editor.underline')
            }).render();
        });
        this.context.memo('button.clear', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.eraser),
                tooltip: _this.lang.font.clear + _this.representShortcut('removeFormat'),
                click: _this.context.createInvokeHandler('editor.removeFormat')
            }).render();
        });
        this.context.memo('button.strikethrough', function () {
            return _this.button({
                className: 'note-btn-strikethrough',
                contents: _this.ui.icon(_this.options.icons.strikethrough),
                tooltip: _this.lang.font.strikethrough + _this.representShortcut('strikethrough'),
                click: _this.context.createInvokeHandlerAndUpdateState('editor.strikethrough')
            }).render();
        });
        this.context.memo('button.superscript', function () {
            return _this.button({
                className: 'note-btn-superscript',
                contents: _this.ui.icon(_this.options.icons.superscript),
                tooltip: _this.lang.font.superscript,
                click: _this.context.createInvokeHandlerAndUpdateState('editor.superscript')
            }).render();
        });
        this.context.memo('button.subscript', function () {
            return _this.button({
                className: 'note-btn-subscript',
                contents: _this.ui.icon(_this.options.icons.subscript),
                tooltip: _this.lang.font.subscript,
                click: _this.context.createInvokeHandlerAndUpdateState('editor.subscript')
            }).render();
        });
        this.context.memo('button.fontname', function () {
            var styleInfo = _this.context.invoke('editor.currentStyle');
            // Add 'default' fonts into the fontnames array if not exist
            jquery_1["default"].each(styleInfo['font-family'].split(','), function (idx, fontname) {
                fontname = fontname.trim().replace(/['"]+/g, '');
                if (_this.isFontDeservedToAdd(fontname)) {
                    if (jquery_1["default"].inArray(fontname, _this.options.fontNames) === -1) {
                        _this.options.fontNames.push(fontname);
                    }
                }
            });
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents('<span class="note-current-fontname"/>', _this.options),
                    tooltip: _this.lang.font.name,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdownCheck({
                    className: 'dropdown-fontname',
                    checkClassName: _this.options.icons.menuCheck,
                    items: _this.options.fontNames.filter(_this.isFontInstalled.bind(_this)),
                    title: _this.lang.font.name,
                    template: function (item) {
                        return '<span style="font-family: \'' + item + '\'">' + item + '</span>';
                    },
                    click: _this.context.createInvokeHandlerAndUpdateState('editor.fontName')
                })
            ]).render();
        });
        this.context.memo('button.fontsize', function () {
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents('<span class="note-current-fontsize"/>', _this.options),
                    tooltip: _this.lang.font.size,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdownCheck({
                    className: 'dropdown-fontsize',
                    checkClassName: _this.options.icons.menuCheck,
                    items: _this.options.fontSizes,
                    title: _this.lang.font.size,
                    click: _this.context.createInvokeHandlerAndUpdateState('editor.fontSize')
                })
            ]).render();
        });
        this.context.memo('button.color', function () {
            return _this.colorPalette('note-color-all', _this.lang.color.recent, true, true);
        });
        this.context.memo('button.forecolor', function () {
            return _this.colorPalette('note-color-fore', _this.lang.color.foreground, false, true);
        });
        this.context.memo('button.backcolor', function () {
            return _this.colorPalette('note-color-back', _this.lang.color.background, true, false);
        });
        this.context.memo('button.ul', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.unorderedlist),
                tooltip: _this.lang.lists.unordered + _this.representShortcut('insertUnorderedList'),
                click: _this.context.createInvokeHandler('editor.insertUnorderedList')
            }).render();
        });
        this.context.memo('button.ol', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.orderedlist),
                tooltip: _this.lang.lists.ordered + _this.representShortcut('insertOrderedList'),
                click: _this.context.createInvokeHandler('editor.insertOrderedList')
            }).render();
        });
        var justifyLeft = this.button({
            contents: this.ui.icon(this.options.icons.alignLeft),
            tooltip: this.lang.paragraph.left + this.representShortcut('justifyLeft'),
            click: this.context.createInvokeHandler('editor.justifyLeft')
        });
        var justifyCenter = this.button({
            contents: this.ui.icon(this.options.icons.alignCenter),
            tooltip: this.lang.paragraph.center + this.representShortcut('justifyCenter'),
            click: this.context.createInvokeHandler('editor.justifyCenter')
        });
        var justifyRight = this.button({
            contents: this.ui.icon(this.options.icons.alignRight),
            tooltip: this.lang.paragraph.right + this.representShortcut('justifyRight'),
            click: this.context.createInvokeHandler('editor.justifyRight')
        });
        var justifyFull = this.button({
            contents: this.ui.icon(this.options.icons.alignJustify),
            tooltip: this.lang.paragraph.justify + this.representShortcut('justifyFull'),
            click: this.context.createInvokeHandler('editor.justifyFull')
        });
        var outdent = this.button({
            contents: this.ui.icon(this.options.icons.outdent),
            tooltip: this.lang.paragraph.outdent + this.representShortcut('outdent'),
            click: this.context.createInvokeHandler('editor.outdent')
        });
        var indent = this.button({
            contents: this.ui.icon(this.options.icons.indent),
            tooltip: this.lang.paragraph.indent + this.representShortcut('indent'),
            click: this.context.createInvokeHandler('editor.indent')
        });
        this.context.memo('button.justifyLeft', func_1["default"].invoke(justifyLeft, 'render'));
        this.context.memo('button.justifyCenter', func_1["default"].invoke(justifyCenter, 'render'));
        this.context.memo('button.justifyRight', func_1["default"].invoke(justifyRight, 'render'));
        this.context.memo('button.justifyFull', func_1["default"].invoke(justifyFull, 'render'));
        this.context.memo('button.outdent', func_1["default"].invoke(outdent, 'render'));
        this.context.memo('button.indent', func_1["default"].invoke(indent, 'render'));
        this.context.memo('button.paragraph', function () {
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents(_this.ui.icon(_this.options.icons.alignLeft), _this.options),
                    tooltip: _this.lang.paragraph.paragraph,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdown([
                    _this.ui.buttonGroup({
                        className: 'note-align',
                        children: [justifyLeft, justifyCenter, justifyRight, justifyFull]
                    }),
                    _this.ui.buttonGroup({
                        className: 'note-list',
                        children: [outdent, indent]
                    })
                ])
            ]).render();
        });
        this.context.memo('button.height', function () {
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents(_this.ui.icon(_this.options.icons.textHeight), _this.options),
                    tooltip: _this.lang.font.height,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdownCheck({
                    items: _this.options.lineHeights,
                    checkClassName: _this.options.icons.menuCheck,
                    className: 'dropdown-line-height',
                    title: _this.lang.font.height,
                    click: _this.context.createInvokeHandler('editor.lineHeight')
                })
            ]).render();
        });
        this.context.memo('button.table', function () {
            return _this.ui.buttonGroup([
                _this.button({
                    className: 'dropdown-toggle',
                    contents: _this.ui.dropdownButtonContents(_this.ui.icon(_this.options.icons.table), _this.options),
                    tooltip: _this.lang.table.table,
                    data: {
                        toggle: 'dropdown'
                    }
                }),
                _this.ui.dropdown({
                    title: _this.lang.table.table,
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
                        width: _this.options.insertTableMaxSize.col + 'em',
                        height: _this.options.insertTableMaxSize.row + 'em'
                    }).mousedown(_this.context.createInvokeHandler('editor.insertTable'))
                        .on('mousemove', _this.tableMoveHandler.bind(_this));
                }
            }).render();
        });
        this.context.memo('button.link', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.link),
                tooltip: _this.lang.link.link + _this.representShortcut('linkDialog.show'),
                click: _this.context.createInvokeHandler('linkDialog.show')
            }).render();
        });
        this.context.memo('button.picture', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.picture),
                tooltip: _this.lang.image.image,
                click: _this.context.createInvokeHandler('imageDialog.show')
            }).render();
        });
        this.context.memo('button.video', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.video),
                tooltip: _this.lang.video.video,
                click: _this.context.createInvokeHandler('videoDialog.show')
            }).render();
        });
        this.context.memo('button.hr', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.minus),
                tooltip: _this.lang.hr.insert + _this.representShortcut('insertHorizontalRule'),
                click: _this.context.createInvokeHandler('editor.insertHorizontalRule')
            }).render();
        });
        this.context.memo('button.fullscreen', function () {
            return _this.button({
                className: 'btn-fullscreen',
                contents: _this.ui.icon(_this.options.icons.arrowsAlt),
                tooltip: _this.lang.options.fullscreen,
                click: _this.context.createInvokeHandler('fullscreen.toggle')
            }).render();
        });
        this.context.memo('button.codeview', function () {
            return _this.button({
                className: 'btn-codeview',
                contents: _this.ui.icon(_this.options.icons.code),
                tooltip: _this.lang.options.codeview,
                click: _this.context.createInvokeHandler('codeview.toggle')
            }).render();
        });
        this.context.memo('button.redo', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.redo),
                tooltip: _this.lang.history.redo + _this.representShortcut('redo'),
                click: _this.context.createInvokeHandler('editor.redo')
            }).render();
        });
        this.context.memo('button.undo', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.undo),
                tooltip: _this.lang.history.undo + _this.representShortcut('undo'),
                click: _this.context.createInvokeHandler('editor.undo')
            }).render();
        });
        this.context.memo('button.help', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.question),
                tooltip: _this.lang.options.help,
                click: _this.context.createInvokeHandler('helpDialog.show')
            }).render();
        });
    };
    /**
     * image : [
     *   ['imagesize', ['imageSize100', 'imageSize50', 'imageSize25']],
     *   ['float', ['floatLeft', 'floatRight', 'floatNone' ]],
     *   ['remove', ['removeMedia']]
     * ],
     */
    Buttons.prototype.addImagePopoverButtons = function () {
        var _this = this;
        // Image Size Buttons
        this.context.memo('button.imageSize100', function () {
            return _this.button({
                contents: '<span class="note-fontsize-10">100%</span>',
                tooltip: _this.lang.image.resizeFull,
                click: _this.context.createInvokeHandler('editor.resize', '1')
            }).render();
        });
        this.context.memo('button.imageSize50', function () {
            return _this.button({
                contents: '<span class="note-fontsize-10">50%</span>',
                tooltip: _this.lang.image.resizeHalf,
                click: _this.context.createInvokeHandler('editor.resize', '0.5')
            }).render();
        });
        this.context.memo('button.imageSize25', function () {
            return _this.button({
                contents: '<span class="note-fontsize-10">25%</span>',
                tooltip: _this.lang.image.resizeQuarter,
                click: _this.context.createInvokeHandler('editor.resize', '0.25')
            }).render();
        });
        // Float Buttons
        this.context.memo('button.floatLeft', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.alignLeft),
                tooltip: _this.lang.image.floatLeft,
                click: _this.context.createInvokeHandler('editor.floatMe', 'left')
            }).render();
        });
        this.context.memo('button.floatRight', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.alignRight),
                tooltip: _this.lang.image.floatRight,
                click: _this.context.createInvokeHandler('editor.floatMe', 'right')
            }).render();
        });
        this.context.memo('button.floatNone', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.alignJustify),
                tooltip: _this.lang.image.floatNone,
                click: _this.context.createInvokeHandler('editor.floatMe', 'none')
            }).render();
        });
        // Remove Buttons
        this.context.memo('button.removeMedia', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.trash),
                tooltip: _this.lang.image.remove,
                click: _this.context.createInvokeHandler('editor.removeMedia')
            }).render();
        });
    };
    Buttons.prototype.addLinkPopoverButtons = function () {
        var _this = this;
        this.context.memo('button.linkDialogShow', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.link),
                tooltip: _this.lang.link.edit,
                click: _this.context.createInvokeHandler('linkDialog.show')
            }).render();
        });
        this.context.memo('button.unlink', function () {
            return _this.button({
                contents: _this.ui.icon(_this.options.icons.unlink),
                tooltip: _this.lang.link.unlink,
                click: _this.context.createInvokeHandler('editor.unlink')
            }).render();
        });
    };
    /**
     * table : [
     *  ['add', ['addRowDown', 'addRowUp', 'addColLeft', 'addColRight']],
     *  ['delete', ['deleteRow', 'deleteCol', 'deleteTable']]
     * ],
     */
    Buttons.prototype.addTablePopoverButtons = function () {
        var _this = this;
        this.context.memo('button.addRowUp', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.rowAbove),
                tooltip: _this.lang.table.addRowAbove,
                click: _this.context.createInvokeHandler('editor.addRow', 'top')
            }).render();
        });
        this.context.memo('button.addRowDown', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.rowBelow),
                tooltip: _this.lang.table.addRowBelow,
                click: _this.context.createInvokeHandler('editor.addRow', 'bottom')
            }).render();
        });
        this.context.memo('button.addColLeft', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.colBefore),
                tooltip: _this.lang.table.addColLeft,
                click: _this.context.createInvokeHandler('editor.addCol', 'left')
            }).render();
        });
        this.context.memo('button.addColRight', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.colAfter),
                tooltip: _this.lang.table.addColRight,
                click: _this.context.createInvokeHandler('editor.addCol', 'right')
            }).render();
        });
        this.context.memo('button.deleteRow', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.rowRemove),
                tooltip: _this.lang.table.delRow,
                click: _this.context.createInvokeHandler('editor.deleteRow')
            }).render();
        });
        this.context.memo('button.deleteCol', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.colRemove),
                tooltip: _this.lang.table.delCol,
                click: _this.context.createInvokeHandler('editor.deleteCol')
            }).render();
        });
        this.context.memo('button.deleteTable', function () {
            return _this.button({
                className: 'btn-md',
                contents: _this.ui.icon(_this.options.icons.trash),
                tooltip: _this.lang.table.delTable,
                click: _this.context.createInvokeHandler('editor.deleteTable')
            }).render();
        });
    };
    Buttons.prototype.build = function ($container, groups) {
        for (var groupIdx = 0, groupLen = groups.length; groupIdx < groupLen; groupIdx++) {
            var group = groups[groupIdx];
            var groupName = jquery_1["default"].isArray(group) ? group[0] : group;
            var buttons = jquery_1["default"].isArray(group) ? ((group.length === 1) ? [group[0]] : group[1]) : [group];
            var $group = this.ui.buttonGroup({
                className: 'note-' + groupName
            }).render();
            for (var idx = 0, len = buttons.length; idx < len; idx++) {
                var btn = this.context.memo('button.' + buttons[idx]);
                if (btn) {
                    $group.append(typeof btn === 'function' ? btn(this.context) : btn);
                }
            }
            $group.appendTo($container);
        }
    };
    /**
     * @param {jQuery} [$container]
     */
    Buttons.prototype.updateCurrentStyle = function ($container) {
        var _this = this;
        var $cont = $container || this.$toolbar;
        var styleInfo = this.context.invoke('editor.currentStyle');
        this.updateBtnStates($cont, {
            '.note-btn-bold': function () {
                return styleInfo['font-bold'] === 'bold';
            },
            '.note-btn-italic': function () {
                return styleInfo['font-italic'] === 'italic';
            },
            '.note-btn-underline': function () {
                return styleInfo['font-underline'] === 'underline';
            },
            '.note-btn-subscript': function () {
                return styleInfo['font-subscript'] === 'subscript';
            },
            '.note-btn-superscript': function () {
                return styleInfo['font-superscript'] === 'superscript';
            },
            '.note-btn-strikethrough': function () {
                return styleInfo['font-strikethrough'] === 'strikethrough';
            }
        });
        if (styleInfo['font-family']) {
            var fontNames = styleInfo['font-family'].split(',').map(function (name) {
                return name.replace(/[\'\"]/g, '')
                    .replace(/\s+$/, '')
                    .replace(/^\s+/, '');
            });
            var fontName_1 = lists_1["default"].find(fontNames, this.isFontInstalled.bind(this));
            $cont.find('.dropdown-fontname a').each(function (idx, item) {
                var $item = jquery_1["default"](item);
                // always compare string to avoid creating another func.
                var isChecked = ($item.data('value') + '') === (fontName_1 + '');
                $item.toggleClass('checked', isChecked);
            });
            $cont.find('.note-current-fontname').text(fontName_1).css('font-family', fontName_1);
        }
        if (styleInfo['font-size']) {
            var fontSize_1 = styleInfo['font-size'];
            $cont.find('.dropdown-fontsize a').each(function (idx, item) {
                var $item = jquery_1["default"](item);
                // always compare with string to avoid creating another func.
                var isChecked = ($item.data('value') + '') === (fontSize_1 + '');
                $item.toggleClass('checked', isChecked);
            });
            $cont.find('.note-current-fontsize').text(fontSize_1);
        }
        if (styleInfo['line-height']) {
            var lineHeight_1 = styleInfo['line-height'];
            $cont.find('.dropdown-line-height li a').each(function (idx, item) {
                // always compare with string to avoid creating another func.
                var isChecked = (jquery_1["default"](item).data('value') + '') === (lineHeight_1 + '');
                _this.className = isChecked ? 'checked' : '';
            });
        }
    };
    Buttons.prototype.updateBtnStates = function ($container, infos) {
        var _this = this;
        jquery_1["default"].each(infos, function (selector, pred) {
            _this.ui.toggleBtnActive($container.find(selector), pred());
        });
    };
    Buttons.prototype.tableMoveHandler = function (event) {
        var PX_PER_EM = 18;
        var $picker = jquery_1["default"](event.target.parentNode); // target is mousecatcher
        var $dimensionDisplay = $picker.next();
        var $catcher = $picker.find('.note-dimension-picker-mousecatcher');
        var $highlighted = $picker.find('.note-dimension-picker-highlighted');
        var $unhighlighted = $picker.find('.note-dimension-picker-unhighlighted');
        var posOffset;
        // HTML5 with jQuery - e.offsetX is undefined in Firefox
        if (event.offsetX === undefined) {
            var posCatcher = jquery_1["default"](event.target).offset();
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
        if (dim.c > 3 && dim.c < this.options.insertTableMaxSize.col) {
            $unhighlighted.css({ width: dim.c + 1 + 'em' });
        }
        if (dim.r > 3 && dim.r < this.options.insertTableMaxSize.row) {
            $unhighlighted.css({ height: dim.r + 1 + 'em' });
        }
        $dimensionDisplay.html(dim.c + ' x ' + dim.r);
    };
    return Buttons;
}());
exports["default"] = Buttons;
//# sourceMappingURL=Buttons.js.map