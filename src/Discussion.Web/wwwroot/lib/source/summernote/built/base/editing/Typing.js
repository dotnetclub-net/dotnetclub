"use strict";
exports.__esModule = true;
var jquery_1 = require("jquery");
var dom_1 = require("../core/dom");
var range_1 = require("../core/range");
var Bullet_1 = require("../editing/Bullet");
/**
 * @class editing.Typing
 *
 * Typing
 *
 */
var Typing = /** @class */ (function () {
    function Typing(context) {
        // a Bullet instance to toggle lists off
        this.bullet = new Bullet_1["default"]();
        this.options = context.options;
    }
    /**
     * insert tab
     *
     * @param {WrappedRange} rng
     * @param {Number} tabsize
     */
    Typing.prototype.insertTab = function (rng, tabsize) {
        var tab = dom_1["default"].createText(new Array(tabsize + 1).join(dom_1["default"].NBSP_CHAR));
        rng = rng.deleteContents();
        rng.insertNode(tab, true);
        rng = range_1["default"].create(tab, tabsize);
        rng.select();
    };
    /**
     * insert paragraph
     *
     * @param {jQuery} $editable
     * @param {WrappedRange} rng Can be used in unit tests to "mock" the range
     *
     * blockquoteBreakingLevel
     *   0 - No break, the new paragraph remains inside the quote
     *   1 - Break the first blockquote in the ancestors list
     *   2 - Break all blockquotes, so that the new paragraph is not quoted (this is the default)
     */
    Typing.prototype.insertParagraph = function (editable, rng) {
        rng = rng || range_1["default"].create(editable);
        // deleteContents on range.
        rng = rng.deleteContents();
        // Wrap range if it needs to be wrapped by paragraph
        rng = rng.wrapBodyInlineWithPara();
        // finding paragraph
        var splitRoot = dom_1["default"].ancestor(rng.sc, dom_1["default"].isPara);
        var nextPara;
        // on paragraph: split paragraph
        if (splitRoot) {
            // if it is an empty line with li
            if (dom_1["default"].isEmpty(splitRoot) && dom_1["default"].isLi(splitRoot)) {
                // toogle UL/OL and escape
                this.bullet.toggleList(splitRoot.parentNode.nodeName);
                return;
            }
            else {
                var blockquote = null;
                if (this.options.blockquoteBreakingLevel === 1) {
                    blockquote = dom_1["default"].ancestor(splitRoot, dom_1["default"].isBlockquote);
                }
                else if (this.options.blockquoteBreakingLevel === 2) {
                    blockquote = dom_1["default"].lastAncestor(splitRoot, dom_1["default"].isBlockquote);
                }
                if (blockquote) {
                    // We're inside a blockquote and options ask us to break it
                    nextPara = jquery_1["default"](dom_1["default"].emptyPara)[0];
                    // If the split is right before a <br>, remove it so that there's no "empty line"
                    // after the split in the new blockquote created
                    if (dom_1["default"].isRightEdgePoint(rng.getStartPoint()) && dom_1["default"].isBR(rng.sc.nextSibling)) {
                        jquery_1["default"](rng.sc.nextSibling).remove();
                    }
                    var split = dom_1["default"].splitTree(blockquote, rng.getStartPoint(), { isDiscardEmptySplits: true });
                    if (split) {
                        split.parentNode.insertBefore(nextPara, split);
                    }
                    else {
                        dom_1["default"].insertAfter(nextPara, blockquote); // There's no split if we were at the end of the blockquote
                    }
                }
                else {
                    nextPara = dom_1["default"].splitTree(splitRoot, rng.getStartPoint());
                    // not a blockquote, just insert the paragraph
                    var emptyAnchors = dom_1["default"].listDescendant(splitRoot, dom_1["default"].isEmptyAnchor);
                    emptyAnchors = emptyAnchors.concat(dom_1["default"].listDescendant(nextPara, dom_1["default"].isEmptyAnchor));
                    jquery_1["default"].each(emptyAnchors, function (idx, anchor) {
                        dom_1["default"].remove(anchor);
                    });
                    // replace empty heading, pre or custom-made styleTag with P tag
                    if ((dom_1["default"].isHeading(nextPara) || dom_1["default"].isPre(nextPara) || dom_1["default"].isCustomStyleTag(nextPara)) && dom_1["default"].isEmpty(nextPara)) {
                        nextPara = dom_1["default"].replace(nextPara, 'p');
                    }
                }
            }
            // no paragraph: insert empty paragraph
        }
        else {
            var next = rng.sc.childNodes[rng.so];
            nextPara = jquery_1["default"](dom_1["default"].emptyPara)[0];
            if (next) {
                rng.sc.insertBefore(nextPara, next);
            }
            else {
                rng.sc.appendChild(nextPara);
            }
        }
        range_1["default"].create(nextPara, 0).normalize().select().scrollIntoView(editable);
    };
    return Typing;
}());
exports["default"] = Typing;
//# sourceMappingURL=Typing.js.map