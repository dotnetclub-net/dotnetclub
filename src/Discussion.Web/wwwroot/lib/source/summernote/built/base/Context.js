"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var func_1 = require("./core/func");
var lists_1 = require("./core/lists");
var dom_1 = require("./core/dom");
var Context = /** @class */ (function () {
    /**
     * @param {jQuery} $note
     * @param {Object} options
     */
    function Context($note, options) {
        this.ui = jquery_1["default"].summernote.ui;
        this.$note = $note;
        this.memos = {};
        this.modules = {};
        this.layoutInfo = {};
        this.options = options;
        this.initialize();
    }
    /**
     * create layout and initialize modules and other resources
     */
    Context.prototype.initialize = function () {
        this.layoutInfo = this.ui.createLayout(this.$note, this.options);
        this._initialize();
        this.$note.hide();
        return this;
    };
    /**
     * destroy modules and other resources and remove layout
     */
    Context.prototype.destroy = function () {
        this._destroy();
        this.$note.removeData('summernote');
        this.ui.removeLayout(this.$note, this.layoutInfo);
    };
    /**
     * destory modules and other resources and initialize it again
     */
    Context.prototype.reset = function () {
        var disabled = this.isDisabled();
        this.code(dom_1["default"].emptyPara);
        this._destroy();
        this._initialize();
        if (disabled) {
            this.disable();
        }
    };
    Context.prototype._initialize = function () {
        var _this = this;
        // add optional buttons
        var buttons = jquery_1["default"].extend({}, this.options.buttons);
        Object.keys(buttons).forEach(function (key) {
            _this.memo('button.' + key, buttons[key]);
        });
        var modules = jquery_1["default"].extend({}, this.options.modules, jquery_1["default"].summernote.plugins || {});
        // add and initialize modules
        Object.keys(modules).forEach(function (key) {
            _this.module(key, modules[key], true);
        });
        Object.keys(this.modules).forEach(function (key) {
            _this.initializeModule(key);
        });
    };
    Context.prototype._destroy = function () {
        var _this = this;
        // destroy modules with reversed order
        Object.keys(this.modules).reverse().forEach(function (key) {
            _this.removeModule(key);
        });
        Object.keys(this.memos).forEach(function (key) {
            _this.removeMemo(key);
        });
        // trigger custom onDestroy callback
        this.triggerEvent('destroy', this);
    };
    Context.prototype.code = function (html) {
        var isActivated = this.invoke('codeview.isActivated');
        if (html === undefined) {
            this.invoke('codeview.sync');
            return isActivated ? this.layoutInfo.codable.val() : this.layoutInfo.editable.html();
        }
        else {
            if (isActivated) {
                this.layoutInfo.codable.val(html);
            }
            else {
                this.layoutInfo.editable.html(html);
            }
            this.$note.val(html);
            this.triggerEvent('change', html);
        }
    };
    Context.prototype.isDisabled = function () {
        return this.layoutInfo.editable.attr('contenteditable') === 'false';
    };
    Context.prototype.enable = function () {
        this.layoutInfo.editable.attr('contenteditable', true);
        this.invoke('toolbar.activate', true);
        this.triggerEvent('disable', false);
    };
    Context.prototype.disable = function () {
        // close codeview if codeview is opend
        if (this.invoke('codeview.isActivated')) {
            this.invoke('codeview.deactivate');
        }
        this.layoutInfo.editable.attr('contenteditable', false);
        this.invoke('toolbar.deactivate', true);
        this.triggerEvent('disable', true);
    };
    Context.prototype.triggerEvent = function () {
        var namespace = lists_1["default"].head(arguments);
        var args = lists_1["default"].tail(lists_1["default"].from(arguments));
        var callback = this.options.callbacks[func_1["default"].namespaceToCamel(namespace, 'on')];
        if (callback) {
            callback.apply(this.$note[0], args);
        }
        this.$note.trigger('summernote.' + namespace, args);
    };
    Context.prototype.initializeModule = function (key) {
        var module = this.modules[key];
        module.shouldInitialize = module.shouldInitialize || func_1["default"].ok;
        if (!module.shouldInitialize()) {
            return;
        }
        // initialize module
        if (module.initialize) {
            module.initialize();
        }
        // attach events
        if (module.events) {
            dom_1["default"].attachEvents(this.$note, module.events);
        }
    };
    Context.prototype.module = function (key, ModuleClass, withoutIntialize) {
        if (arguments.length === 1) {
            return this.modules[key];
        }
        this.modules[key] = new ModuleClass(this);
        if (!withoutIntialize) {
            this.initializeModule(key);
        }
    };
    Context.prototype.removeModule = function (key) {
        var module = this.modules[key];
        if (module.shouldInitialize()) {
            if (module.events) {
                dom_1["default"].detachEvents(this.$note, module.events);
            }
            if (module.destroy) {
                module.destroy();
            }
        }
        delete this.modules[key];
    };
    Context.prototype.memo = function (key, obj) {
        if (arguments.length === 1) {
            return this.memos[key];
        }
        this.memos[key] = obj;
    };
    Context.prototype.removeMemo = function (key) {
        if (this.memos[key] && this.memos[key].destroy) {
            this.memos[key].destroy();
        }
        delete this.memos[key];
    };
    /**
     * Some buttons need to change their visual style immediately once they get pressed
     */
    Context.prototype.createInvokeHandlerAndUpdateState = function (namespace, value) {
        var _this = this;
        return function (event) {
            _this.createInvokeHandler(namespace, value)(event);
            _this.invoke('buttons.updateCurrentStyle');
        };
    };
    Context.prototype.createInvokeHandler = function (namespace, value) {
        var _this = this;
        return function (event) {
            event.preventDefault();
            var $target = jquery_1["default"](event.target);
            _this.invoke(namespace, value || $target.closest('[data-value]').data('value'), $target);
        };
    };
    Context.prototype.invoke = function () {
        var namespace = lists_1["default"].head(arguments);
        var args = lists_1["default"].tail(lists_1["default"].from(arguments));
        var splits = namespace.split('.');
        var hasSeparator = splits.length > 1;
        var moduleName = hasSeparator && lists_1["default"].head(splits);
        var methodName = hasSeparator ? lists_1["default"].last(splits) : lists_1["default"].head(splits);
        var module = this.modules[moduleName || 'editor'];
        if (!moduleName && this[methodName]) {
            return this[methodName].apply(this, args);
        }
        else if (module && module[methodName] && module.shouldInitialize()) {
            return module[methodName].apply(module, args);
        }
    };
    return Context;
}());
exports["default"] = Context;
//# sourceMappingURL=Context.js.map