import * as utils from '../functions'


// "send", "re-send", "error", "sending", "sent"
var currentStatus = null;
var sendConfirmationMailUrl = '';

export function setupEditEmailAddress(originalEmail, sendConfirmationMailApiUrl) {
    sendConfirmationMailUrl = sendConfirmationMailApiUrl;

    keepUpdatingConfirmationStatus(originalEmail);
    updateOperations('send');
    $('a.link.confirmation-operation').click(send);
}



function keepUpdatingConfirmationStatus(originalEmail){
    $('input[name=EmailAddress]').keyup(function() {
        var changed = !this.value || this.value !== originalEmail;
        if (changed) {
            $('[rel=confirmation]').hide();
        } else {
            $('[rel=confirmation]').show();
        }
    });
}

function updateOperations(status) {
    currentStatus = status;
    $('.confirmation-operation').hide();
    $('.confirmation-operation[data-status=' + currentStatus + ']').show();

    if (status === 'sent') {
        $('span[data-status=sent-2]').hide();
        $('span[data-status=sent-1]').show();
        setTimeout(function() {
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
    $('.confirmation').addClass('sent');
    updateOperations('sending');

    $.ajax({
        url: sendConfirmationMailUrl,
        method: 'POST',
        data: { __RequestVerificationToken: window.__RequestVerificationToken },
        dataType: 'json',
        success: function(data) {
            if (!data.hasSucceeded) {
                updateOperations('error');
            } else {
                updateOperations('sent');
            }
        },
        error: function() {
            updateOperations('error');
        }
    });
}



