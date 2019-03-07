"use strict";
exports.__esModule = true;
var PopoverUI = /** @class */ (function () {
    function PopoverUI($node, options) {
        this.$node = $node;
        this.options = $.extend({}, {
            title: '',
            content: '',
            target: options.container,
            trigger: 'hover focus',
            placement: 'bottom'
        }, options);
        // create popover node
        this.$popover = $([
            '<div class="note-popover in">',
            ' <div class="note-popover-arrow" />',
            ' <div class="note-popover-content" />',
            '</div>'
        ].join(''));
        // define event
        if (this.options.trigger !== 'manual') {
            var showCallback_1 = this.show.bind(this);
            var hideCallback_1 = this.hide.bind(this);
            var toggleCallback_1 = this.toggle.bind(this);
            this.options.trigger.split(' ').forEach(function (eventName) {
                if (eventName === 'hover') {
                    $node.off('mouseenter').on('mouseenter', showCallback_1);
                    $node.off('mouseleave').on('mouseleave', hideCallback_1);
                }
                else if (eventName === 'click') {
                    $node.on('click', toggleCallback_1);
                }
                else if (eventName === 'focus') {
                    $node.on('focus', showCallback_1);
                    $node.on('blur', hideCallback_1);
                }
            });
        }
    }
    PopoverUI.prototype.show = function () {
        var $node = this.$node;
        var offset = $node.offset();
        var $popover = this.$popover;
        var content = this.options.content || $node.data('content');
        var placement = $node.data('placement') || this.options.placement;
        var dist = 6;
        $popover.addClass(placement);
        $popover.addClass('in');
        $popover.find('.note-popover-content').html(content);
        $popover.appendTo(this.options.target);
        var nodeWidth = $node.outerWidth();
        var nodeHeight = $node.outerHeight();
        var popoverWidth = $popover.outerWidth();
        var popoverHeight = $popover.outerHeight();
        if (placement === 'bottom') {
            $popover.css({
                top: offset.top + nodeHeight + dist,
                left: offset.left + (nodeWidth / 2 - popoverWidth / 2)
            });
        }
        else if (placement === 'top') {
            $popover.css({
                top: offset.top - popoverHeight - dist,
                left: offset.left + (nodeWidth / 2 - popoverWidth / 2)
            });
        }
        else if (placement === 'left') {
            $popover.css({
                top: offset.top + (nodeHeight / 2 - popoverHeight / 2),
                left: offset.left - popoverWidth - dist
            });
        }
        else if (placement === 'right') {
            $popover.css({
                top: offset.top + (nodeHeight / 2 - popoverHeight / 2),
                left: offset.left + nodeWidth + dist
            });
        }
    };
    PopoverUI.prototype.hide = function () {
        this.$popover.removeClass('in');
        this.$popover.remove();
    };
    PopoverUI.prototype.toggle = function () {
        if (this.$popover.hasClass('in')) {
            this.hide();
        }
        else {
            this.show();
        }
    };
    return PopoverUI;
}());
exports["default"] = PopoverUI;
//# sourceMappingURL=PopoverUI.js.map