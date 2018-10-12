

export default function filterTags(node) {
    return processTag(node, null)
}

function processTag(node, parentNode){
    if(node.childNodes.length){
        processTag(node.childNodes[0], node);
    }

    if(!parentNode){
        return;
    }

    if(node.nextSibling){
        processTag(node.nextSibling, parentNode);
    }

    var transformedNode = processSingleTag(node);
    if(transformedNode !== node){
        if (!transformedNode.nodeName){
            $.each(transformedNode, function (i, p) {
                parentNode.insertBefore(p, node);
            });
        } else {
            parentNode.insertBefore(transformedNode, node);
        }
        parentNode.removeChild(node);
    }
}


const options = htmlFragmentOptions();

function processSingleTag(node){
    if(node.nodeType === 3){
        return node;
    }

    if(node.nodeName === 'A' && !$.trim(node.textContent) && !node.childNodes.length){
        return document.createTextNode('');
    }

    if(node.nodeType !== 1 || isIllegal(node.nodeName)){
        return document.createTextNode('');
    }

    if(isAllowed(node.nodeName)){
        stripAttributes(node);
        return node;
    }

    if(isBlockTag(node.nodeName)){
        return createParagraphs(node.childNodes);
    }

    return document.createTextNode(node.textContent);

    function createParagraphs(nodes){
        var paragraphs = [];
        var p, node;

        for(var i=0;i<nodes.length;i++){
            node = nodes[i];
            if(!p){
                p = document.createElement('P');
                paragraphs.push(p);
            }

            if(node.nodeName !== 'P'){
                p.appendChild(node);
            }else{
                paragraphs.push(nodes[i]);
                p = null;
            }
        }

        return paragraphs;
    }


    function isIllegal(tagName) {
        var illegal = false;
        $.each(options.illegalTags, function(i, tag){
            if(equals(tagName, tag)){
                illegal = true;
                return false;
            }
        });
        return illegal;
    }

    function isAllowed(tagName) {
        var allowed = false;
        $.each(options.allowedTags, function(i, tag){
            if(equals(tagName, tag)){
                allowed = true;
                return false;
            }
        });
        return allowed;
    }

    function isBlockTag(tagName){
        var isBlock = false;
        $.each(options.blockTags, function(i, tag){
            if(equals(tagName, tag)){
                isBlock = true;
                return false;
            }
        });
        return isBlock;
    }

    function stripAttributes(node){
        var attributesToRemove = [];
        $.each(node.attributes, function(i, attr){
            var tag = node.nodeName.toLowerCase();

            if(!options.allowedAttributes[tag] || options.allowedAttributes[tag].indexOf(attr.name) < 0){
                attributesToRemove.push(attr.name);
            }
        });

        $.each(attributesToRemove, function(i, attr){
            node.removeAttribute(attr);
        });
    }

    function equals(first, second){
        return first.toUpperCase() === second.toUpperCase();
    }
}

function htmlFragmentOptions(){
    var illegalTags = [
        'script',
        'style',
        'link',
        'iframe',
        'frameset',
        'object',
        'embed',
        'video',
        'audio',
        'canvas',
        'svg',
        'html',
        'head',
        'body',
        'meta',
        'input',
        'select',
        'option',
        'button'
    ];

    var blockTags = [
        'div',
        'main',
        'nav',
        'aside',
        'section',
        'article',
        'footer',
        'header',
        'figure',
        'form',
        'legend',
        'fieldset',
        'dl',
        'dt',
        'dd',
        'h1',
        'h2',
        'h4',
        'h5',
        'h6'
    ];

    var allowedTags = [
        'p',
        'br',
        'h3',
        'blockquote',
        'strong',
        'b',
        'em',
        'i',
        'strike',
        'ul',
        'ol',
        'li',
        'a',
        'img',
        'pre'
    ];
    var attributes = {
        'a': ['href', 'title', 'name'],
        'img': ['src', 'alt', 'title'],
        'pre': ['language']
    };

    return {
        illegalTags: illegalTags,
        blockTags: blockTags,
        allowedTags: allowedTags,
        allowedAttributes: attributes
    };
}
