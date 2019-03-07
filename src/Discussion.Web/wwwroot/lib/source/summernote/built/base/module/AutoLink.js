"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var lists_1 = require("../core/lists");
var key_1 = require("../core/key");
var defaultScheme = 'http://';
var linkPattern = /^([A-Za-z][A-Za-z0-9+-.]*\:[\/]{2}|mailto:[A-Z0-9._%+-]+@)?(www\.)?(.+)$/i;
var AutoLink = /** @class */ (function () {
    function AutoLink(context) {
        var _this = this;
        this.context = context;
        this.events = {
            'summernote.keyup': function (we, e) {
                if (!e.isDefaultPrevented()) {
                    _this.handleKeyup(e);
                }
            },
            'summernote.keydown': function (we, e) {
                _this.handleKeydown(e);
            }
        };
    }
    AutoLink.prototype.initialize = function () {
        this.lastWordRange = null;
    };
    AutoLink.prototype.destroy = function () {
        this.lastWordRange = null;
    };
    AutoLink.prototype.replace = function () {
        if (!this.lastWordRange) {
            return;
        }
        var keyword = this.lastWordRange.toString();
        var match = keyword.match(linkPattern);
        if (match && (match[1] || match[2])) {
            var link = match[1] ? keyword : defaultScheme + keyword;
            var node = jquery_1["default"]('<a />').html(keyword).attr('href', link)[0];
            if (this.context.options.linkTargetBlank) {
                jquery_1["default"](node).attr('target', '_blank');
            }
            this.lastWordRange.insertNode(node);
            this.lastWordRange = null;
            this.context.invoke('editor.focus');
        }
    };
    AutoLink.prototype.handleKeydown = function (e) {
        if (lists_1["default"].contains([key_1["default"].code.ENTER, key_1["default"].code.SPACE], e.keyCode)) {
            var wordRange = this.context.invoke('editor.createRange').getWordRange();
            this.lastWordRange = wordRange;
        }
    };
    AutoLink.prototype.handleKeyup = function (e) {
        if (lists_1["default"].contains([key_1["default"].code.ENTER, key_1["default"].code.SPACE], e.keyCode)) {
            this.replace();
        }
    };
    return AutoLink;
}());
exports["default"] = AutoLink;
//# sourceMappingURL=AutoLink.js.map