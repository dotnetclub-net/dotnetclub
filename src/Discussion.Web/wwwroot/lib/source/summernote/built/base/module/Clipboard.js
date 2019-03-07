"use strict";
exports.__esModule = true;
var lists_1 = require("../core/lists");
var Clipboard = /** @class */ (function () {
    function Clipboard(context) {
        this.context = context;
        this.$editable = context.layoutInfo.editable;
    }
    Clipboard.prototype.initialize = function () {
        this.$editable.on('paste', this.pasteByEvent.bind(this));
    };
    /**
     * paste by clipboard event
     *
     * @param {Event} event
     */
    Clipboard.prototype.pasteByEvent = function (event) {
        var clipboardData = event.originalEvent.clipboardData;
        if (clipboardData && clipboardData.items && clipboardData.items.length) {
            // paste img file
            var item = clipboardData.items.length > 1 ? clipboardData.items[1] : lists_1["default"].head(clipboardData.items);
            if (item.kind === 'file' && item.type.indexOf('image/') !== -1) {
                this.context.invoke('editor.insertImagesOrCallback', [item.getAsFile()]);
            }
            this.context.invoke('editor.afterCommand');
        }
    };
    return Clipboard;
}());
exports["default"] = Clipboard;
//# sourceMappingURL=Clipboard.js.map