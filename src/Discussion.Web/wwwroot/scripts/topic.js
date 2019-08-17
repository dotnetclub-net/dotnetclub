

export function submitNewReply(editorSelector, postUrl) {
    return function() {
        var editor = $(editorSelector);
        var replyContent = editor.val();
        if (!$.trim(replyContent)) {
            editor.focus();
            return false;
        }

        $.ajax({
            url: postUrl,
            method: 'POST',
            data: {
                content: replyContent,
                __RequestVerificationToken: window.__RequestVerificationToken
            },
            dataType: 'json',
            success: function() {
                location.reload();
            },
            error: function(xhr) {
                var errors = JSON.parse(xhr.responseText);
                console.log(errors);
            }
        });
    };
}


export function setupReplyPreview(selectors, previewApiUrl){

    $(selectors.tabPreview).on('shown.bs.tab',
        function(e) {
            var editor = $(selectors.contentEditor);
            var replyContent = editor.val();
            if (!$.trim(replyContent)) {
                editor.focus();
                e.preventDefault();
                return false;
            }

            $(selectors.contentPreview).html('正在加载预览...');
            $.ajax({
                url: previewApiUrl,
                method: 'POST',
                data: { 
                    markdown: replyContent, 
                    __RequestVerificationToken: window.__RequestVerificationToken },
                success: function(res) {
                    if (res.hasSucceeded) {
                        $(selectors.contentPreview).html(res.result.html);
                    } else {
                        $(selectors.contentPreview).html('<span style="color:red">' + res.errorMessage + '</span>');
                    }
                },
                error: function() {
                    $(selectors.contentPreview).html('<span style="color:red">暂时无法预览</span>');
                }
            });
        });

    $(selectors.tabEditor).on('shown.bs.tab',
        function() {
            $(selectors.contentEditor).focus();
            $(selectors.contentPreview).empty();
        });
}