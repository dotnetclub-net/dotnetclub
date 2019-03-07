"use strict";
exports.__esModule = true;
var ModalUI = /** @class */ (function () {
    function ModalUI($node, options) {
        this.options = $.extend({}, {
            target: options.container || 'body'
        }, options);
        this.$modal = $node;
        this.$backdrop = $('<div class="note-modal-backdrop" />');
    }
    ModalUI.prototype.show = function () {
        if (this.options.target === 'body') {
            this.$backdrop.css('position', 'fixed');
            this.$modal.css('position', 'fixed');
        }
        else {
            this.$backdrop.css('position', 'absolute');
            this.$modal.css('position', 'absolute');
        }
        this.$backdrop.appendTo(this.options.target).show();
        this.$modal.appendTo(this.options.target).addClass('open').show();
        this.$modal.trigger('note.modal.show');
        this.$modal.off('click', '.close').on('click', '.close', this.hide.bind(this));
    };
    ModalUI.prototype.hide = function () {
        this.$modal.removeClass('open').hide();
        this.$backdrop.hide();
        this.$modal.trigger('note.modal.hide');
    };
    return ModalUI;
}());
exports["default"] = ModalUI;
//# sourceMappingURL=ModalUI.js.map