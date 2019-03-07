"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var lists_1 = require("../core/lists");
var func_1 = require("../core/func");
var dom_1 = require("../core/dom");
var range_1 = require("../core/range");
var Bullet = /** @class */ (function () {
    function Bullet() {
    }
    /**
     * toggle ordered list
     */
    Bullet.prototype.insertOrderedList = function (editable) {
        this.toggleList('OL', editable);
    };
    /**
     * toggle unordered list
     */
    Bullet.prototype.insertUnorderedList = function (editable) {
        this.toggleList('UL', editable);
    };
    /**
     * indent
     */
    Bullet.prototype.indent = function (editable) {
        var _this = this;
        var rng = range_1["default"].create(editable).wrapBodyInlineWithPara();
        var paras = rng.nodes(dom_1["default"].isPara, { includeAncestor: true });
        var clustereds = lists_1["default"].clusterBy(paras, func_1["default"].peq2('parentNode'));
        jquery_1["default"].each(clustereds, function (idx, paras) {
            var head = lists_1["default"].head(paras);
            if (dom_1["default"].isLi(head)) {
                var previousList_1 = _this.findList(head.previousSibling);
                if (previousList_1) {
                    paras
                        .map(function (para) { return previousList_1.appendChild(para); });
                }
                else {
                    _this.wrapList(paras, head.parentNode.nodeName);
                    paras
                        .map(function (para) { return para.parentNode; })
                        .map(function (para) { return _this.appendToPrevious(para); });
                }
            }
            else {
                jquery_1["default"].each(paras, function (idx, para) {
                    jquery_1["default"](para).css('marginLeft', function (idx, val) {
                        return (parseInt(val, 10) || 0) + 25;
                    });
                });
            }
        });
        rng.select();
    };
    /**
     * outdent
     */
    Bullet.prototype.outdent = function (editable) {
        var _this = this;
        var rng = range_1["default"].create(editable).wrapBodyInlineWithPara();
        var paras = rng.nodes(dom_1["default"].isPara, { includeAncestor: true });
        var clustereds = lists_1["default"].clusterBy(paras, func_1["default"].peq2('parentNode'));
        jquery_1["default"].each(clustereds, function (idx, paras) {
            var head = lists_1["default"].head(paras);
            if (dom_1["default"].isLi(head)) {
                _this.releaseList([paras]);
            }
            else {
                jquery_1["default"].each(paras, function (idx, para) {
                    jquery_1["default"](para).css('marginLeft', function (idx, val) {
                        val = (parseInt(val, 10) || 0);
                        return val > 25 ? val - 25 : '';
                    });
                });
            }
        });
        rng.select();
    };
    /**
     * toggle list
     *
     * @param {String} listName - OL or UL
     */
    Bullet.prototype.toggleList = function (listName, editable) {
        var _this = this;
        var rng = range_1["default"].create(editable).wrapBodyInlineWithPara();
        var paras = rng.nodes(dom_1["default"].isPara, { includeAncestor: true });
        var bookmark = rng.paraBookmark(paras);
        var clustereds = lists_1["default"].clusterBy(paras, func_1["default"].peq2('parentNode'));
        // paragraph to list
        if (lists_1["default"].find(paras, dom_1["default"].isPurePara)) {
            var wrappedParas_1 = [];
            jquery_1["default"].each(clustereds, function (idx, paras) {
                wrappedParas_1 = wrappedParas_1.concat(_this.wrapList(paras, listName));
            });
            paras = wrappedParas_1;
            // list to paragraph or change list style
        }
        else {
            var diffLists = rng.nodes(dom_1["default"].isList, {
                includeAncestor: true
            }).filter(function (listNode) {
                return !jquery_1["default"].nodeName(listNode, listName);
            });
            if (diffLists.length) {
                jquery_1["default"].each(diffLists, function (idx, listNode) {
                    dom_1["default"].replace(listNode, listName);
                });
            }
            else {
                paras = this.releaseList(clustereds, true);
            }
        }
        range_1["default"].createFromParaBookmark(bookmark, paras).select();
    };
    /**
     * @param {Node[]} paras
     * @param {String} listName
     * @return {Node[]}
     */
    Bullet.prototype.wrapList = function (paras, listName) {
        var head = lists_1["default"].head(paras);
        var last = lists_1["default"].last(paras);
        var prevList = dom_1["default"].isList(head.previousSibling) && head.previousSibling;
        var nextList = dom_1["default"].isList(last.nextSibling) && last.nextSibling;
        var listNode = prevList || dom_1["default"].insertAfter(dom_1["default"].create(listName || 'UL'), last);
        // P to LI
        paras = paras.map(function (para) {
            return dom_1["default"].isPurePara(para) ? dom_1["default"].replace(para, 'LI') : para;
        });
        // append to list(<ul>, <ol>)
        dom_1["default"].appendChildNodes(listNode, paras);
        if (nextList) {
            dom_1["default"].appendChildNodes(listNode, lists_1["default"].from(nextList.childNodes));
            dom_1["default"].remove(nextList);
        }
        return paras;
    };
    /**
     * @method releaseList
     *
     * @param {Array[]} clustereds
     * @param {Boolean} isEscapseToBody
     * @return {Node[]}
     */
    Bullet.prototype.releaseList = function (clustereds, isEscapseToBody) {
        var _this = this;
        var releasedParas = [];
        jquery_1["default"].each(clustereds, function (idx, paras) {
            var head = lists_1["default"].head(paras);
            var last = lists_1["default"].last(paras);
            var headList = isEscapseToBody ? dom_1["default"].lastAncestor(head, dom_1["default"].isList) : head.parentNode;
            var parentItem = headList.parentNode;
            if (headList.parentNode.nodeName === 'LI') {
                paras.map(function (para) {
                    var newList = _this.findNextSiblings(para);
                    if (parentItem.nextSibling) {
                        parentItem.parentNode.insertBefore(para, parentItem.nextSibling);
                    }
                    else {
                        parentItem.parentNode.appendChild(para);
                    }
                    if (newList.length) {
                        _this.wrapList(newList, headList.nodeName);
                        para.appendChild(newList[0].parentNode);
                    }
                });
                if (headList.children.length === 0) {
                    parentItem.removeChild(headList);
                }
                if (parentItem.childNodes.length === 0) {
                    parentItem.parentNode.removeChild(parentItem);
                }
            }
            else {
                var lastList = headList.childNodes.length > 1 ? dom_1["default"].splitTree(headList, {
                    node: last.parentNode,
                    offset: dom_1["default"].position(last) + 1
                }, {
                    isSkipPaddingBlankHTML: true
                }) : null;
                var middleList = dom_1["default"].splitTree(headList, {
                    node: head.parentNode,
                    offset: dom_1["default"].position(head)
                }, {
                    isSkipPaddingBlankHTML: true
                });
                paras = isEscapseToBody ? dom_1["default"].listDescendant(middleList, dom_1["default"].isLi)
                    : lists_1["default"].from(middleList.childNodes).filter(dom_1["default"].isLi);
                // LI to P
                if (isEscapseToBody || !dom_1["default"].isList(headList.parentNode)) {
                    paras = paras.map(function (para) {
                        return dom_1["default"].replace(para, 'P');
                    });
                }
                jquery_1["default"].each(lists_1["default"].from(paras).reverse(), function (idx, para) {
                    dom_1["default"].insertAfter(para, headList);
                });
                // remove empty lists
                var rootLists = lists_1["default"].compact([headList, middleList, lastList]);
                jquery_1["default"].each(rootLists, function (idx, rootList) {
                    var listNodes = [rootList].concat(dom_1["default"].listDescendant(rootList, dom_1["default"].isList));
                    jquery_1["default"].each(listNodes.reverse(), function (idx, listNode) {
                        if (!dom_1["default"].nodeLength(listNode)) {
                            dom_1["default"].remove(listNode, true);
                        }
                    });
                });
            }
            releasedParas = releasedParas.concat(paras);
        });
        return releasedParas;
    };
    /**
     * @method appendToPrevious
     *
     * Appends list to previous list item, if
     * none exist it wraps the list in a new list item.
     *
     * @param {HTMLNode} ListItem
     * @return {HTMLNode}
     */
    Bullet.prototype.appendToPrevious = function (node) {
        return node.previousSibling
            ? dom_1["default"].appendChildNodes(node.previousSibling, [node])
            : this.wrapList([node], 'LI');
    };
    /**
     * @method findList
     *
     * Finds an existing list in list item
     *
     * @param {HTMLNode} ListItem
     * @return {Array[]}
     */
    Bullet.prototype.findList = function (node) {
        return node
            ? lists_1["default"].find(node.children, function (child) { return ['OL', 'UL'].indexOf(child.nodeName) > -1; })
            : null;
    };
    /**
     * @method findNextSiblings
     *
     * Finds all list item siblings that follow it
     *
     * @param {HTMLNode} ListItem
     * @return {HTMLNode}
     */
    Bullet.prototype.findNextSiblings = function (node) {
        var siblings = [];
        while (node.nextSibling) {
            siblings.push(node.nextSibling);
            node = node.nextSibling;
        }
        return siblings;
    };
    return Bullet;
}());
exports["default"] = Bullet;
//# sourceMappingURL=Bullet.js.map