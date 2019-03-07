"use strict";
exports.__esModule = true;
var DropdownUI = /** @class */ (function () {
    function DropdownUI($node, options) {
        this.$button = $node;
        this.options = $.extend({}, {
            target: options.container
        }, options);
        this.setEvent();
    }
    DropdownUI.prototype.setEvent = function () {
        var _this = this;
        this.$button.on('click', function (e) {
            _this.toggle();
            e.stopImmediatePropagation();
        });
    };
    DropdownUI.prototype.clear = function () {
        var $parent = $('.note-btn-group.open');
        $parent.find('.note-btn.active').removeClass('active');
        $parent.removeClass('open');
    };
    DropdownUI.prototype.show = function () {
        this.$button.addClass('active');
        this.$button.parent().addClass('open');
        var $dropdown = this.$button.next();
        var offset = $dropdown.offset();
        var width = $dropdown.outerWidth();
        var windowWidth = $(window).width();
        var targetMarginRight = parseFloat($(this.options.target).css('margin-right'));
        if (offset.left + width > windowWidth - targetMarginRight) {
            $dropdown.css('margin-left', windowWidth - targetMarginRight - (offset.left + width));
        }
        else {
            $dropdown.css('margin-left', '');
        }
    };
    DropdownUI.prototype.hide = function () {
        this.$button.removeClass('active');
        this.$button.parent().removeClass('open');
    };
    DropdownUI.prototype.toggle = function () {
        var isOpened = this.$button.parent().hasClass('open');
        this.clear();
        if (isOpened) {
            this.hide();
        }
        else {
            this.show();
        }
    };
    return DropdownUI;
}());
$(document).on('click', function (e) {
    if (!$(e.target).closest('.note-btn-group').length) {
        $('.note-btn-group.open').removeClass('open');
    }
});
$(document).on('click.note-dropdown-menu', function (e) {
    $(e.target).closest('.note-dropdown-menu').parent().removeClass('open');
});
exports["default"] = DropdownUI;
//# sourceMappingURL=DropdownUI.js.map