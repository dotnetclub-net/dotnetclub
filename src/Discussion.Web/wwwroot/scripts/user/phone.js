import * as utils from '../functions'

// "send", "re-send", "error", "sending", "sent"
var currentStatus = null;
var sendCodeUrl, verifyNumberUrl;


export function setupVerifyPhoneNumber(sendCodeApiUrl, verifyNumberApiUrl){
    sendCodeUrl = sendCodeApiUrl;
    verifyNumberUrl = verifyNumberApiUrl;
    
    updateOperations('send');
    
    $('a.link.change-phone-number').click(function() {
        $('.edit-phone-number').removeClass('hide');
        $('.has-phone-number').removeClass('show').addClass('hide');
    });
    $('a.link.verification-operation').click(send);
    $('#btn-verify').click(verify);
}



function updateOperations(status) {
    currentStatus = status;
    $('.verification-operation').hide();
    $('.verification-operation[data-status=' + currentStatus + ']').show();

    if (status === 'sent') {
        $('.verification-submit').show();
        $('span[data-status=sent-2]').hide();
        $('span[data-status=sent-1]').show();
        setTimeout(function() {
                $('span[data-status=sent-1]').hide();
                $('span[data-status=sent-2]').show();
                countdown(118);
            },
            5000);
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
    var phoneNumberInput = $('#VerifiedPhoneNumber_PhoneNumber');
    var phoneNumber = phoneNumberInput.val();
    if (!phoneNumber) {
        phoneNumberInput.focus();
        return;
    }

    if (!(/^1\d{10}$/.test(phoneNumber))) {
        alert('手机号的格式不正确，请输入 11 位中国大陆手机号');
        phoneNumberInput.focus();
        phoneNumberInput.select();
        return;
    }

    $('.verification').addClass('sent');
    updateOperations('sending');

    $.ajax({
        url: sendCodeUrl,
        method: 'POST',
        data: {
            __RequestVerificationToken: window.__RequestVerificationToken,
            phoneNumber: phoneNumber
        },
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

function verify() {
    var codeInput = $('#VerificationCode');
    var code = codeInput.val();
    if (!code) {
        codeInput.focus();
        return;
    }

    if (!(/^\d{6}$/.test(code))) {
        alert('验证码格式不正确，请输入收到的6位验证码');
        codeInput.focus();
        codeInput.select();
        return;
    }

    $.ajax({
        url: verifyNumberUrl,
        method: 'POST',
        data: {
            code: code,
            __RequestVerificationToken: window.__RequestVerificationToken
        },
        dataType: 'json',
        success: function(data) {
            if (data.hasSucceeded) {
                location.reload();
            } else {
                alert('无法验证手机号：' + data.message);
            }
        },
        error: function() {
            updateOperations('error');
        }
    });
}
