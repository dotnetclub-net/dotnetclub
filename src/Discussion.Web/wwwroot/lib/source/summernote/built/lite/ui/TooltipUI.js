"use strict";
exports.__esModule = true;
var TooltipUI = /** @class */ (function () {
    function TooltipUI($node, options) {
        this.$node = $node;
        this.options = $.extend({}, {
            title: '',
            target: options.container,
            trigger: 'hover focus',
            placement: 'bottom'
        }, options);
        // create tooltip node
        this.$tooltip = $([
            '<div class="note-tooltip in">',
            '  <div class="note-tooltip-arrow"/>',
            '  <div class="note-tooltip-content"/>',
            '</div>'
        ].join(''));
        // define event
        if (this.options.trigger !== 'manual') {
            var showCallback_1 = this.show.bind(this);
            var hideCallback_1 = this.hide.bind(this);
            var toggleCallback_1 = this.toggle.bind(this);
            this.options.trigger.split(' ').forEach(function (eventName) {
                if (eventName === 'hover') {
                    $node.off('mouseenter mouseleave');
                    $node.on('mouseenter', showCallback_1).on('mouseleave', hideCallback_1);
                }
                else if (eventName === 'click') {
                    $node.on('click', toggleCallback_1);
                }
                else if (eventName === 'focus') {
                    $node.on('focus', showCallback_1).on('blur', hideCallback_1);
                }
            });
        }
    }
    TooltipUI.prototype.show = function () {
        var $node = this.$node;
        var offset = $node.offset();
        var $tooltip = this.$tooltip;
        var title = this.options.title || $node.attr('title') || $node.data('title');
        var placement = this.options.placement || $node.data('placement');
        $tooltip.addClass(placement);
        $tooltip.addClass('in');
        $tooltip.find('.note-tooltip-content').text(title);
        $tooltip.appendTo(this.options.target);
        var nodeWidth = $node.outerWidth();
        var nodeHeight = $node.outerHeight();
        var tooltipWidth = $tooltip.outerWidth();
        var tooltipHeight = $tooltip.outerHeight();
        if (placement === 'bottom') {
            $tooltip.css({
                top: offset.top + nodeHeight,
                left: offset.left + (nodeWidth / 2 - tooltipWidth / 2)
            });
        }
        else if (placement === 'top') {
            $tooltip.css({
                top: offset.top - tooltipHeight,
                left: offset.left + (nodeWidth / 2 - tooltipWidth / 2)
            });
        }
        else if (placement === 'left') {
            $tooltip.css({
                top: offset.top + (nodeHeight / 2 - tooltipHeight / 2),
                left: offset.left - tooltipWidth
            });
        }
        else if (placement === 'right') {
            $tooltip.css({
                top: offset.top + (nodeHeight / 2 - tooltipHeight / 2),
                left: offset.left + nodeWidth
            });
        }
    };
    TooltipUI.prototype.hide = function () {
        this.$tooltip.removeClass('in');
        this.$tooltip.remove();
    };
    TooltipUI.prototype.toggle = function () {
        if (this.$tooltip.hasClass('in')) {
            this.hide();
        }
        else {
            this.show();
        }
    };
    return TooltipUI;
}());
exports["default"] = TooltipUI;
//# sourceMappingURL=TooltipUI.js.map