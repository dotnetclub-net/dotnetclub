"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var env_1 = require("../core/env");
var func_1 = require("../core/func");
var lists_1 = require("../core/lists");
var dom_1 = require("../core/dom");
var Style = /** @class */ (function () {
    function Style() {
    }
    /**
     * @method jQueryCSS
     *
     * [workaround] for old jQuery
     * passing an array of style properties to .css()
     * will result in an object of property-value pairs.
     * (compability with version < 1.9)
     *
     * @private
     * @param  {jQuery} $obj
     * @param  {Array} propertyNames - An array of one or more CSS properties.
     * @return {Object}
     */
    Style.prototype.jQueryCSS = function ($obj, propertyNames) {
        if (env_1["default"].jqueryVersion < 1.9) {
            var result_1 = {};
            jquery_1["default"].each(propertyNames, function (idx, propertyName) {
                result_1[propertyName] = $obj.css(propertyName);
            });
            return result_1;
        }
        return $obj.css(propertyNames);
    };
    /**
     * returns style object from node
     *
     * @param {jQuery} $node
     * @return {Object}
     */
    Style.prototype.fromNode = function ($node) {
        var properties = ['font-family', 'font-size', 'text-align', 'list-style-type', 'line-height'];
        var styleInfo = this.jQueryCSS($node, properties) || {};
        styleInfo['font-size'] = parseInt(styleInfo['font-size'], 10);
        return styleInfo;
    };
    /**
     * paragraph level style
     *
     * @param {WrappedRange} rng
     * @param {Object} styleInfo
     */
    Style.prototype.stylePara = function (rng, styleInfo) {
        jquery_1["default"].each(rng.nodes(dom_1["default"].isPara, {
            includeAncestor: true
        }), function (idx, para) {
            jquery_1["default"](para).css(styleInfo);
        });
    };
    /**
     * insert and returns styleNodes on range.
     *
     * @param {WrappedRange} rng
     * @param {Object} [options] - options for styleNodes
     * @param {String} [options.nodeName] - default: `SPAN`
     * @param {Boolean} [options.expandClosestSibling] - default: `false`
     * @param {Boolean} [options.onlyPartialContains] - default: `false`
     * @return {Node[]}
     */
    Style.prototype.styleNodes = function (rng, options) {
        rng = rng.splitText();
        var nodeName = (options && options.nodeName) || 'SPAN';
        var expandClosestSibling = !!(options && options.expandClosestSibling);
        var onlyPartialContains = !!(options && options.onlyPartialContains);
        if (rng.isCollapsed()) {
            return [rng.insertNode(dom_1["default"].create(nodeName))];
        }
        var pred = dom_1["default"].makePredByNodeName(nodeName);
        var nodes = rng.nodes(dom_1["default"].isText, {
            fullyContains: true
        }).map(function (text) {
            return dom_1["default"].singleChildAncestor(text, pred) || dom_1["default"].wrap(text, nodeName);
        });
        if (expandClosestSibling) {
            if (onlyPartialContains) {
                var nodesInRange_1 = rng.nodes();
                // compose with partial contains predication
                pred = func_1["default"].and(pred, function (node) {
                    return lists_1["default"].contains(nodesInRange_1, node);
                });
            }
            return nodes.map(function (node) {
                var siblings = dom_1["default"].withClosestSiblings(node, pred);
                var head = lists_1["default"].head(siblings);
                var tails = lists_1["default"].tail(siblings);
                jquery_1["default"].each(tails, function (idx, elem) {
                    dom_1["default"].appendChildNodes(head, elem.childNodes);
                    dom_1["default"].remove(elem);
                });
                return lists_1["default"].head(siblings);
            });
        }
        else {
            return nodes;
        }
    };
    /**
     * get current style on cursor
     *
     * @param {WrappedRange} rng
     * @return {Object} - object contains style properties.
     */
    Style.prototype.current = function (rng) {
        var $cont = jquery_1["default"](!dom_1["default"].isElement(rng.sc) ? rng.sc.parentNode : rng.sc);
        var styleInfo = this.fromNode($cont);
        // document.queryCommandState for toggle state
        // [workaround] prevent Firefox nsresult: "0x80004005 (NS_ERROR_FAILURE)"
        try {
            styleInfo = jquery_1["default"].extend(styleInfo, {
                'font-bold': document.queryCommandState('bold') ? 'bold' : 'normal',
                'font-italic': document.queryCommandState('italic') ? 'italic' : 'normal',
                'font-underline': document.queryCommandState('underline') ? 'underline' : 'normal',
                'font-subscript': document.queryCommandState('subscript') ? 'subscript' : 'normal',
                'font-superscript': document.queryCommandState('superscript') ? 'superscript' : 'normal',
                'font-strikethrough': document.queryCommandState('strikethrough') ? 'strikethrough' : 'normal',
                'font-family': document.queryCommandValue('fontname') || styleInfo['font-family']
            });
        }
        catch (e) { }
        // list-style-type to list-style(unordered, ordered)
        if (!rng.isOnList()) {
            styleInfo['list-style'] = 'none';
        }
        else {
            var orderedTypes = ['circle', 'disc', 'disc-leading-zero', 'square'];
            var isUnordered = jquery_1["default"].inArray(styleInfo['list-style-type'], orderedTypes) > -1;
            styleInfo['list-style'] = isUnordered ? 'unordered' : 'ordered';
        }
        var para = dom_1["default"].ancestor(rng.sc, dom_1["default"].isPara);
        if (para && para.style['line-height']) {
            styleInfo['line-height'] = para.style.lineHeight;
        }
        else {
            var lineHeight = parseInt(styleInfo['line-height'], 10) / parseInt(styleInfo['font-size'], 10);
            styleInfo['line-height'] = lineHeight.toFixed(1);
        }
        styleInfo.anchor = rng.isOnAnchor() && dom_1["default"].ancestor(rng.sc, dom_1["default"].isAnchor);
        styleInfo.ancestors = dom_1["default"].listAncestor(rng.sc, dom_1["default"].isEditable);
        styleInfo.range = rng;
        return styleInfo;
    };
    return Style;
}());
exports["default"] = Style;
//# sourceMappingURL=Style.js.map