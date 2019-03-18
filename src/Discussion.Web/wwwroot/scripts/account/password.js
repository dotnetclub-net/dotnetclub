import * as utils from '../functions'


// "send", "re-send", "error", "sending", "sent"
var currentStatus = '';
var actionUrl = '';

export function setupForgot(actionApiUrl) {
    actionUrl = actionApiUrl;
    updateOperations('send');

    $('a.link.forgot-operation').click(send);

    $('input[name=UsernameOrEmail]').click(function () {
        updateOperations('send');
    });
}

function updateOperations(status) {
    currentStatus = status;
    $('.forgot-operation').hide();
    $('.forgot-operation[data-status=' + currentStatus + ']').show();

    if (status === 'sent') {
        $('span[data-status=sent-2]').hide();
        $('span[data-status=sent-1]').show();
        setTimeout(function () {
                $('span[data-status=sent-1]').hide();
                $('span[data-status=sent-2]').show();
                countdown(30);
            },
            3500);
    }

    function countdown(seconds) {
        utils.countDown(seconds, function(){
            updateOperations('re-send');
        }, function (cur) {
            $('span[data-status=sent-2].countdown').text(cur);
        });
    }
}

function send() {
    updateOperations('sending');

    $.ajax({
        url: actionUrl,
        method: 'POST',
        data: {
            __RequestVerificationToken: window.__RequestVerificationToken,
            UsernameOrEmail: $('input[name=UsernameOrEmail]').val()
        },
        dataType: 'json',
        success: function(data) {
            if (data.hasSucceeded) {
                updateOperations('sent');
            } else {
                $('span[data-status=error]').html(data.errorMessage);
                updateOperations('error');
            }
        },
        error: function(data) {
            $('span[data-status=error]').html('无法发送邮件，请稍后再试');
            updateOperations('error');
        }
    });
}
