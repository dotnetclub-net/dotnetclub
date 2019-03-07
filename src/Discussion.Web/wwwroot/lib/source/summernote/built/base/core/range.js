"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var env_1 = require("./env");
var func_1 = require("./func");
var lists_1 = require("./lists");
var dom_1 = require("./dom");
/**
 * return boundaryPoint from TextRange, inspired by Andy Na's HuskyRange.js
 *
 * @param {TextRange} textRange
 * @param {Boolean} isStart
 * @return {BoundaryPoint}
 *
 * @see http://msdn.microsoft.com/en-us/library/ie/ms535872(v=vs.85).aspx
 */
function textRangeToPoint(textRange, isStart) {
    var container = textRange.parentElement();
    var offset;
    var tester = document.body.createTextRange();
    var prevContainer;
    var childNodes = lists_1["default"].from(container.childNodes);
    for (offset = 0; offset < childNodes.length; offset++) {
        if (dom_1["default"].isText(childNodes[offset])) {
            continue;
        }
        tester.moveToElementText(childNodes[offset]);
        if (tester.compareEndPoints('StartToStart', textRange) >= 0) {
            break;
        }
        prevContainer = childNodes[offset];
    }
    if (offset !== 0 && dom_1["default"].isText(childNodes[offset - 1])) {
        var textRangeStart = document.body.createTextRange();
        var curTextNode = null;
        textRangeStart.moveToElementText(prevContainer || container);
        textRangeStart.collapse(!prevContainer);
        curTextNode = prevContainer ? prevContainer.nextSibling : container.firstChild;
        var pointTester = textRange.duplicate();
        pointTester.setEndPoint('StartToStart', textRangeStart);
        var textCount = pointTester.text.replace(/[\r\n]/g, '').length;
        while (textCount > curTextNode.nodeValue.length && curTextNode.nextSibling) {
            textCount -= curTextNode.nodeValue.length;
            curTextNode = curTextNode.nextSibling;
        }
        // [workaround] enforce IE to re-reference curTextNode, hack
        var dummy = curTextNode.nodeValue; // eslint-disable-line
        if (isStart && curTextNode.nextSibling && dom_1["default"].isText(curTextNode.nextSibling) &&
            textCount === curTextNode.nodeValue.length) {
            textCount -= curTextNode.nodeValue.length;
            curTextNode = curTextNode.nextSibling;
        }
        container = curTextNode;
        offset = textCount;
    }
    return {
        cont: container,
        offset: offset
    };
}
/**
 * return TextRange from boundary point (inspired by google closure-library)
 * @param {BoundaryPoint} point
 * @return {TextRange}
 */
function pointToTextRange(point) {
    var textRangeInfo = function (container, offset) {
        var node, isCollapseToStart;
        if (dom_1["default"].isText(container)) {
            var prevTextNodes = dom_1["default"].listPrev(container, func_1["default"].not(dom_1["default"].isText));
            var prevContainer = lists_1["default"].last(prevTextNodes).previousSibling;
            node = prevContainer || container.parentNode;
            offset += lists_1["default"].sum(lists_1["default"].tail(prevTextNodes), dom_1["default"].nodeLength);
            isCollapseToStart = !prevContainer;
        }
        else {
            node = container.childNodes[offset] || container;
            if (dom_1["default"].isText(node)) {
                return textRangeInfo(node, 0);
            }
            offset = 0;
            isCollapseToStart = false;
        }
        return {
            node: node,
            collapseToStart: isCollapseToStart,
            offset: offset
        };
    };
    var textRange = document.body.createTextRange();
    var info = textRangeInfo(point.node, point.offset);
    textRange.moveToElementText(info.node);
    textRange.collapse(info.collapseToStart);
    textRange.moveStart('character', info.offset);
    return textRange;
}
/**
   * Wrapped Range
   *
   * @constructor
   * @param {Node} sc - start container
   * @param {Number} so - start offset
   * @param {Node} ec - end container
   * @param {Number} eo - end offset
   */
var WrappedRange = /** @class */ (function () {
    function WrappedRange(sc, so, ec, eo) {
        this.sc = sc;
        this.so = so;
        this.ec = ec;
        this.eo = eo;
        // isOnEditable: judge whether range is on editable or not
        this.isOnEditable = this.makeIsOn(dom_1["default"].isEditable);
        // isOnList: judge whether range is on list node or not
        this.isOnList = this.makeIsOn(dom_1["default"].isList);
        // isOnAnchor: judge whether range is on anchor node or not
        this.isOnAnchor = this.makeIsOn(dom_1["default"].isAnchor);
        // isOnCell: judge whether range is on cell node or not
        this.isOnCell = this.makeIsOn(dom_1["default"].isCell);
        // isOnData: judge whether range is on data node or not
        this.isOnData = this.makeIsOn(dom_1["default"].isData);
    }
    // nativeRange: get nativeRange from sc, so, ec, eo
    WrappedRange.prototype.nativeRange = function () {
        if (env_1["default"].isW3CRangeSupport) {
            var w3cRange = document.createRange();
            w3cRange.setStart(this.sc, this.so);
            w3cRange.setEnd(this.ec, this.eo);
            return w3cRange;
        }
        else {
            var textRange = pointToTextRange({
                node: this.sc,
                offset: this.so
            });
            textRange.setEndPoint('EndToEnd', pointToTextRange({
                node: this.ec,
                offset: this.eo
            }));
            return textRange;
        }
    };
    WrappedRange.prototype.getPoints = function () {
        return {
            sc: this.sc,
            so: this.so,
            ec: this.ec,
            eo: this.eo
        };
    };
    WrappedRange.prototype.getStartPoint = function () {
        return {
            node: this.sc,
            offset: this.so
        };
    };
    WrappedRange.prototype.getEndPoint = function () {
        return {
            node: this.ec,
            offset: this.eo
        };
    };
    /**
     * select update visible range
     */
    WrappedRange.prototype.select = function () {
        var nativeRng = this.nativeRange();
        if (env_1["default"].isW3CRangeSupport) {
            var selection = document.getSelection();
            if (selection.rangeCount > 0) {
                selection.removeAllRanges();
            }
            selection.addRange(nativeRng);
        }
        else {
            nativeRng.select();
        }
        return this;
    };
    /**
     * Moves the scrollbar to start container(sc) of current range
     *
     * @return {WrappedRange}
     */
    WrappedRange.prototype.scrollIntoView = function (container) {
        var height = jquery_1["default"](container).height();
        if (container.scrollTop + height < this.sc.offsetTop) {
            container.scrollTop += Math.abs(container.scrollTop + height - this.sc.offsetTop);
        }
        return this;
    };
    /**
     * @return {WrappedRange}
     */
    WrappedRange.prototype.normalize = function () {
        /**
         * @param {BoundaryPoint} point
         * @param {Boolean} isLeftToRight
         * @return {BoundaryPoint}
         */
        var getVisiblePoint = function (point, isLeftToRight) {
            if ((dom_1["default"].isVisiblePoint(point) && !dom_1["default"].isEdgePoint(point)) ||
                (dom_1["default"].isVisiblePoint(point) && dom_1["default"].isRightEdgePoint(point) && !isLeftToRight) ||
                (dom_1["default"].isVisiblePoint(point) && dom_1["default"].isLeftEdgePoint(point) && isLeftToRight) ||
                (dom_1["default"].isVisiblePoint(point) && dom_1["default"].isBlock(point.node) && dom_1["default"].isEmpty(point.node))) {
                return point;
            }
            // point on block's edge
            var block = dom_1["default"].ancestor(point.node, dom_1["default"].isBlock);
            if (((dom_1["default"].isLeftEdgePointOf(point, block) || dom_1["default"].isVoid(dom_1["default"].prevPoint(point).node)) && !isLeftToRight) ||
                ((dom_1["default"].isRightEdgePointOf(point, block) || dom_1["default"].isVoid(dom_1["default"].nextPoint(point).node)) && isLeftToRight)) {
                // returns point already on visible point
                if (dom_1["default"].isVisiblePoint(point)) {
                    return point;
                }
                // reverse direction
                isLeftToRight = !isLeftToRight;
            }
            var nextPoint = isLeftToRight ? dom_1["default"].nextPointUntil(dom_1["default"].nextPoint(point), dom_1["default"].isVisiblePoint)
                : dom_1["default"].prevPointUntil(dom_1["default"].prevPoint(point), dom_1["default"].isVisiblePoint);
            return nextPoint || point;
        };
        var endPoint = getVisiblePoint(this.getEndPoint(), false);
        var startPoint = this.isCollapsed() ? endPoint : getVisiblePoint(this.getStartPoint(), true);
        return new WrappedRange(startPoint.node, startPoint.offset, endPoint.node, endPoint.offset);
    };
    /**
     * returns matched nodes on range
     *
     * @param {Function} [pred] - predicate function
     * @param {Object} [options]
     * @param {Boolean} [options.includeAncestor]
     * @param {Boolean} [options.fullyContains]
     * @return {Node[]}
     */
    WrappedRange.prototype.nodes = function (pred, options) {
        pred = pred || func_1["default"].ok;
        var includeAncestor = options && options.includeAncestor;
        var fullyContains = options && options.fullyContains;
        // TODO compare points and sort
        var startPoint = this.getStartPoint();
        var endPoint = this.getEndPoint();
        var nodes = [];
        var leftEdgeNodes = [];
        dom_1["default"].walkPoint(startPoint, endPoint, function (point) {
            if (dom_1["default"].isEditable(point.node)) {
                return;
            }
            var node;
            if (fullyContains) {
                if (dom_1["default"].isLeftEdgePoint(point)) {
                    leftEdgeNodes.push(point.node);
                }
                if (dom_1["default"].isRightEdgePoint(point) && lists_1["default"].contains(leftEdgeNodes, point.node)) {
                    node = point.node;
                }
            }
            else if (includeAncestor) {
                node = dom_1["default"].ancestor(point.node, pred);
            }
            else {
                node = point.node;
            }
            if (node && pred(node)) {
                nodes.push(node);
            }
        }, true);
        return lists_1["default"].unique(nodes);
    };
    /**
     * returns commonAncestor of range
     * @return {Element} - commonAncestor
     */
    WrappedRange.prototype.commonAncestor = function () {
        return dom_1["default"].commonAncestor(this.sc, this.ec);
    };
    /**
     * returns expanded range by pred
     *
     * @param {Function} pred - predicate function
     * @return {WrappedRange}
     */
    WrappedRange.prototype.expand = function (pred) {
        var startAncestor = dom_1["default"].ancestor(this.sc, pred);
        var endAncestor = dom_1["default"].ancestor(this.ec, pred);
        if (!startAncestor && !endAncestor) {
            return new WrappedRange(this.sc, this.so, this.ec, this.eo);
        }
        var boundaryPoints = this.getPoints();
        if (startAncestor) {
            boundaryPoints.sc = startAncestor;
            boundaryPoints.so = 0;
        }
        if (endAncestor) {
            boundaryPoints.ec = endAncestor;
            boundaryPoints.eo = dom_1["default"].nodeLength(endAncestor);
        }
        return new WrappedRange(boundaryPoints.sc, boundaryPoints.so, boundaryPoints.ec, boundaryPoints.eo);
    };
    /**
     * @param {Boolean} isCollapseToStart
     * @return {WrappedRange}
     */
    WrappedRange.prototype.collapse = function (isCollapseToStart) {
        if (isCollapseToStart) {
            return new WrappedRange(this.sc, this.so, this.sc, this.so);
        }
        else {
            return new WrappedRange(this.ec, this.eo, this.ec, this.eo);
        }
    };
    /**
     * splitText on range
     */
    WrappedRange.prototype.splitText = function () {
        var isSameContainer = this.sc === this.ec;
        var boundaryPoints = this.getPoints();
        if (dom_1["default"].isText(this.ec) && !dom_1["default"].isEdgePoint(this.getEndPoint())) {
            this.ec.splitText(this.eo);
        }
        if (dom_1["default"].isText(this.sc) && !dom_1["default"].isEdgePoint(this.getStartPoint())) {
            boundaryPoints.sc = this.sc.splitText(this.so);
            boundaryPoints.so = 0;
            if (isSameContainer) {
                boundaryPoints.ec = boundaryPoints.sc;
                boundaryPoints.eo = this.eo - this.so;
            }
        }
        return new WrappedRange(boundaryPoints.sc, boundaryPoints.so, boundaryPoints.ec, boundaryPoints.eo);
    };
    /**
     * delete contents on range
     * @return {WrappedRange}
     */
    WrappedRange.prototype.deleteContents = function () {
        if (this.isCollapsed()) {
            return this;
        }
        var rng = this.splitText();
        var nodes = rng.nodes(null, {
            fullyContains: true
        });
        // find new cursor point
        var point = dom_1["default"].prevPointUntil(rng.getStartPoint(), function (point) {
            return !lists_1["default"].contains(nodes, point.node);
        });
        var emptyParents = [];
        jquery_1["default"].each(nodes, function (idx, node) {
            // find empty parents
            var parent = node.parentNode;
            if (point.node !== parent && dom_1["default"].nodeLength(parent) === 1) {
                emptyParents.push(parent);
            }
            dom_1["default"].remove(node, false);
        });
        // remove empty parents
        jquery_1["default"].each(emptyParents, function (idx, node) {
            dom_1["default"].remove(node, false);
        });
        return new WrappedRange(point.node, point.offset, point.node, point.offset).normalize();
    };
    /**
     * makeIsOn: return isOn(pred) function
     */
    WrappedRange.prototype.makeIsOn = function (pred) {
        return function () {
            var ancestor = dom_1["default"].ancestor(this.sc, pred);
            return !!ancestor && (ancestor === dom_1["default"].ancestor(this.ec, pred));
        };
    };
    /**
     * @param {Function} pred
     * @return {Boolean}
     */
    WrappedRange.prototype.isLeftEdgeOf = function (pred) {
        if (!dom_1["default"].isLeftEdgePoint(this.getStartPoint())) {
            return false;
        }
        var node = dom_1["default"].ancestor(this.sc, pred);
        return node && dom_1["default"].isLeftEdgeOf(this.sc, node);
    };
    /**
     * returns whether range was collapsed or not
     */
    WrappedRange.prototype.isCollapsed = function () {
        return this.sc === this.ec && this.so === this.eo;
    };
    /**
     * wrap inline nodes which children of body with paragraph
     *
     * @return {WrappedRange}
     */
    WrappedRange.prototype.wrapBodyInlineWithPara = function () {
        if (dom_1["default"].isBodyContainer(this.sc) && dom_1["default"].isEmpty(this.sc)) {
            this.sc.innerHTML = dom_1["default"].emptyPara;
            return new WrappedRange(this.sc.firstChild, 0, this.sc.firstChild, 0);
        }
        /**
         * [workaround] firefox often create range on not visible point. so normalize here.
         *  - firefox: |<p>text</p>|
         *  - chrome: <p>|text|</p>
         */
        var rng = this.normalize();
        if (dom_1["default"].isParaInline(this.sc) || dom_1["default"].isPara(this.sc)) {
            return rng;
        }
        // find inline top ancestor
        var topAncestor;
        if (dom_1["default"].isInline(rng.sc)) {
            var ancestors = dom_1["default"].listAncestor(rng.sc, func_1["default"].not(dom_1["default"].isInline));
            topAncestor = lists_1["default"].last(ancestors);
            if (!dom_1["default"].isInline(topAncestor)) {
                topAncestor = ancestors[ancestors.length - 2] || rng.sc.childNodes[rng.so];
            }
        }
        else {
            topAncestor = rng.sc.childNodes[rng.so > 0 ? rng.so - 1 : 0];
        }
        // siblings not in paragraph
        var inlineSiblings = dom_1["default"].listPrev(topAncestor, dom_1["default"].isParaInline).reverse();
        inlineSiblings = inlineSiblings.concat(dom_1["default"].listNext(topAncestor.nextSibling, dom_1["default"].isParaInline));
        // wrap with paragraph
        if (inlineSiblings.length) {
            var para = dom_1["default"].wrap(lists_1["default"].head(inlineSiblings), 'p');
            dom_1["default"].appendChildNodes(para, lists_1["default"].tail(inlineSiblings));
        }
        return this.normalize();
    };
    /**
     * insert node at current cursor
     *
     * @param {Node} node
     * @return {Node}
     */
    WrappedRange.prototype.insertNode = function (node) {
        var rng = this.wrapBodyInlineWithPara().deleteContents();
        var info = dom_1["default"].splitPoint(rng.getStartPoint(), dom_1["default"].isInline(node));
        if (info.rightNode) {
            info.rightNode.parentNode.insertBefore(node, info.rightNode);
        }
        else {
            info.container.appendChild(node);
        }
        return node;
    };
    /**
     * insert html at current cursor
     */
    WrappedRange.prototype.pasteHTML = function (markup) {
        var contentsContainer = jquery_1["default"]('<div></div>').html(markup)[0];
        var childNodes = lists_1["default"].from(contentsContainer.childNodes);
        var rng = this.wrapBodyInlineWithPara().deleteContents();
        if (rng.so > 0) {
            childNodes = childNodes.reverse();
        }
        childNodes = childNodes.map(function (childNode) {
            return rng.insertNode(childNode);
        });
        if (rng.so > 0) {
            childNodes = childNodes.reverse();
        }
        return childNodes;
    };
    /**
     * returns text in range
     *
     * @return {String}
     */
    WrappedRange.prototype.toString = function () {
        var nativeRng = this.nativeRange();
        return env_1["default"].isW3CRangeSupport ? nativeRng.toString() : nativeRng.text;
    };
    /**
     * returns range for word before cursor
     *
     * @param {Boolean} [findAfter] - find after cursor, default: false
     * @return {WrappedRange}
     */
    WrappedRange.prototype.getWordRange = function (findAfter) {
        var endPoint = this.getEndPoint();
        if (!dom_1["default"].isCharPoint(endPoint)) {
            return this;
        }
        var startPoint = dom_1["default"].prevPointUntil(endPoint, function (point) {
            return !dom_1["default"].isCharPoint(point);
        });
        if (findAfter) {
            endPoint = dom_1["default"].nextPointUntil(endPoint, function (point) {
                return !dom_1["default"].isCharPoint(point);
            });
        }
        return new WrappedRange(startPoint.node, startPoint.offset, endPoint.node, endPoint.offset);
    };
    /**
     * create offsetPath bookmark
     *
     * @param {Node} editable
     */
    WrappedRange.prototype.bookmark = function (editable) {
        return {
            s: {
                path: dom_1["default"].makeOffsetPath(editable, this.sc),
                offset: this.so
            },
            e: {
                path: dom_1["default"].makeOffsetPath(editable, this.ec),
                offset: this.eo
            }
        };
    };
    /**
     * create offsetPath bookmark base on paragraph
     *
     * @param {Node[]} paras
     */
    WrappedRange.prototype.paraBookmark = function (paras) {
        return {
            s: {
                path: lists_1["default"].tail(dom_1["default"].makeOffsetPath(lists_1["default"].head(paras), this.sc)),
                offset: this.so
            },
            e: {
                path: lists_1["default"].tail(dom_1["default"].makeOffsetPath(lists_1["default"].last(paras), this.ec)),
                offset: this.eo
            }
        };
    };
    /**
     * getClientRects
     * @return {Rect[]}
     */
    WrappedRange.prototype.getClientRects = function () {
        var nativeRng = this.nativeRange();
        return nativeRng.getClientRects();
    };
    return WrappedRange;
}());
/**
 * Data structure
 *  * BoundaryPoint: a point of dom tree
 *  * BoundaryPoints: two boundaryPoints corresponding to the start and the end of the Range
 *
 * See to http://www.w3.org/TR/DOM-Level-2-Traversal-Range/ranges.html#Level-2-Range-Position
 */
exports["default"] = {
    /**
     * create Range Object From arguments or Browser Selection
     *
     * @param {Node} sc - start container
     * @param {Number} so - start offset
     * @param {Node} ec - end container
     * @param {Number} eo - end offset
     * @return {WrappedRange}
     */
    create: function (sc, so, ec, eo) {
        if (arguments.length === 4) {
            return new WrappedRange(sc, so, ec, eo);
        }
        else if (arguments.length === 2) { // collapsed
            ec = sc;
            eo = so;
            return new WrappedRange(sc, so, ec, eo);
        }
        else {
            var wrappedRange = this.createFromSelection();
            if (!wrappedRange && arguments.length === 1) {
                wrappedRange = this.createFromNode(arguments[0]);
                return wrappedRange.collapse(dom_1["default"].emptyPara === arguments[0].innerHTML);
            }
            return wrappedRange;
        }
    },
    createFromSelection: function () {
        var sc, so, ec, eo;
        if (env_1["default"].isW3CRangeSupport) {
            var selection = document.getSelection();
            if (!selection || selection.rangeCount === 0) {
                return null;
            }
            else if (dom_1["default"].isBody(selection.anchorNode)) {
                // Firefox: returns entire body as range on initialization.
                // We won't never need it.
                return null;
            }
            var nativeRng = selection.getRangeAt(0);
            sc = nativeRng.startContainer;
            so = nativeRng.startOffset;
            ec = nativeRng.endContainer;
            eo = nativeRng.endOffset;
        }
        else { // IE8: TextRange
            var textRange = document.selection.createRange();
            var textRangeEnd = textRange.duplicate();
            textRangeEnd.collapse(false);
            var textRangeStart = textRange;
            textRangeStart.collapse(true);
            var startPoint = textRangeToPoint(textRangeStart, true);
            var endPoint = textRangeToPoint(textRangeEnd, false);
            // same visible point case: range was collapsed.
            if (dom_1["default"].isText(startPoint.node) && dom_1["default"].isLeftEdgePoint(startPoint) &&
                dom_1["default"].isTextNode(endPoint.node) && dom_1["default"].isRightEdgePoint(endPoint) &&
                endPoint.node.nextSibling === startPoint.node) {
                startPoint = endPoint;
            }
            sc = startPoint.cont;
            so = startPoint.offset;
            ec = endPoint.cont;
            eo = endPoint.offset;
        }
        return new WrappedRange(sc, so, ec, eo);
    },
    /**
     * @method
     *
     * create WrappedRange from node
     *
     * @param {Node} node
     * @return {WrappedRange}
     */
    createFromNode: function (node) {
        var sc = node;
        var so = 0;
        var ec = node;
        var eo = dom_1["default"].nodeLength(ec);
        // browsers can't target a picture or void node
        if (dom_1["default"].isVoid(sc)) {
            so = dom_1["default"].listPrev(sc).length - 1;
            sc = sc.parentNode;
        }
        if (dom_1["default"].isBR(ec)) {
            eo = dom_1["default"].listPrev(ec).length - 1;
            ec = ec.parentNode;
        }
        else if (dom_1["default"].isVoid(ec)) {
            eo = dom_1["default"].listPrev(ec).length;
            ec = ec.parentNode;
        }
        return this.create(sc, so, ec, eo);
    },
    /**
     * create WrappedRange from node after position
     *
     * @param {Node} node
     * @return {WrappedRange}
     */
    createFromNodeBefore: function (node) {
        return this.createFromNode(node).collapse(true);
    },
    /**
     * create WrappedRange from node after position
     *
     * @param {Node} node
     * @return {WrappedRange}
     */
    createFromNodeAfter: function (node) {
        return this.createFromNode(node).collapse();
    },
    /**
     * @method
     *
     * create WrappedRange from bookmark
     *
     * @param {Node} editable
     * @param {Object} bookmark
     * @return {WrappedRange}
     */
    createFromBookmark: function (editable, bookmark) {
        var sc = dom_1["default"].fromOffsetPath(editable, bookmark.s.path);
        var so = bookmark.s.offset;
        var ec = dom_1["default"].fromOffsetPath(editable, bookmark.e.path);
        var eo = bookmark.e.offset;
        return new WrappedRange(sc, so, ec, eo);
    },
    /**
     * @method
     *
     * create WrappedRange from paraBookmark
     *
     * @param {Object} bookmark
     * @param {Node[]} paras
     * @return {WrappedRange}
     */
    createFromParaBookmark: function (bookmark, paras) {
        var so = bookmark.s.offset;
        var eo = bookmark.e.offset;
        var sc = dom_1["default"].fromOffsetPath(lists_1["default"].head(paras), bookmark.s.path);
        var ec = dom_1["default"].fromOffsetPath(lists_1["default"].last(paras), bookmark.e.path);
        return new WrappedRange(sc, so, ec, eo);
    }
};
//# sourceMappingURL=range.js.map