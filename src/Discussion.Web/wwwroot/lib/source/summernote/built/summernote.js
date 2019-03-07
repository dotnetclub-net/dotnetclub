"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var env_1 = require("./base/core/env");
var lists_1 = require("./base/core/lists");
var Context_1 = require("./base/Context");
jquery_1["default"].fn.extend({
    /**
     * Summernote API
     *
     * @param {Object|String}
     * @return {this}
     */
    summernote: function () {
        var type = jquery_1["default"].type(lists_1["default"].head(arguments));
        var isExternalAPICalled = type === 'string';
        var hasInitOptions = type === 'object';
        var options = jquery_1["default"].extend({}, jquery_1["default"].summernote.options, hasInitOptions ? lists_1["default"].head(arguments) : {});
        // Update options
        options.langInfo = jquery_1["default"].extend(true, {}, jquery_1["default"].summernote.lang['en-US'], jquery_1["default"].summernote.lang[options.lang]);
        options.icons = jquery_1["default"].extend(true, {}, jquery_1["default"].summernote.options.icons, options.icons);
        options.tooltip = options.tooltip === 'auto' ? !env_1["default"].isSupportTouch : options.tooltip;
        this.each(function (idx, note) {
            var $note = jquery_1["default"](note);
            if (!$note.data('summernote')) {
                var context = new Context_1["default"]($note, options);
                $note.data('summernote', context);
                $note.data('summernote').triggerEvent('init', context.layoutInfo);
            }
        });
        var $note = this.first();
        if ($note.length) {
            var context = $note.data('summernote');
            if (isExternalAPICalled) {
                return context.invoke.apply(context, lists_1["default"].from(arguments));
            }
            else if (options.focus) {
                context.invoke('editor.focus');
            }
        }
        return this;
    }
});
//# sourceMappingURL=summernote.js.map