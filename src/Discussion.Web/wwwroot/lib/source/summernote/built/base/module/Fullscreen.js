"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var Fullscreen = /** @class */ (function () {
    function Fullscreen(context) {
        var _this = this;
        this.context = context;
        this.$editor = context.layoutInfo.editor;
        this.$toolbar = context.layoutInfo.toolbar;
        this.$editable = context.layoutInfo.editable;
        this.$codable = context.layoutInfo.codable;
        this.$window = jquery_1["default"](window);
        this.$scrollbar = jquery_1["default"]('html, body');
        this.onResize = function () {
            _this.resizeTo({
                h: _this.$window.height() - _this.$toolbar.outerHeight()
            });
        };
    }
    Fullscreen.prototype.resizeTo = function (size) {
        this.$editable.css('height', size.h);
        this.$codable.css('height', size.h);
        if (this.$codable.data('cmeditor')) {
            this.$codable.data('cmeditor').setsize(null, size.h);
        }
    };
    /**
     * toggle fullscreen
     */
    Fullscreen.prototype.toggle = function () {
        this.$editor.toggleClass('fullscreen');
        if (this.isFullscreen()) {
            this.$editable.data('orgHeight', this.$editable.css('height'));
            this.$editable.data('orgMaxHeight', this.$editable.css('maxHeight'));
            this.$editable.css('maxHeight', '');
            this.$window.on('resize', this.onResize).trigger('resize');
            this.$scrollbar.css('overflow', 'hidden');
        }
        else {
            this.$window.off('resize', this.onResize);
            this.resizeTo({ h: this.$editable.data('orgHeight') });
            this.$editable.css('maxHeight', this.$editable.css('orgMaxHeight'));
            this.$scrollbar.css('overflow', 'visible');
        }
        this.context.invoke('toolbar.updateFullscreen', this.isFullscreen());
    };
    Fullscreen.prototype.isFullscreen = function () {
        return this.$editor.hasClass('fullscreen');
    };
    return Fullscreen;
}());
exports["default"] = Fullscreen;
//# sourceMappingURL=Fullscreen.js.map