import QRCode from "../../lib/node_modules/qrcode"

let _getBotInfoUrl, _verifyUrl;

export function setupVerifyWeChatAcount(getBotInfoUrl, verifyUrl){
    _getBotInfoUrl = getBotInfoUrl;
    _verifyUrl = verifyUrl;
    fetchBotInfo()
}


function fetchBotInfo() {
    $.getJSON(_getBotInfoUrl).then(data => {
        const notice = $('[rel=notice]');
        if(!data.hasSucceeded || !data.result.qrCode){
            notice.text('暂时无法绑定微信账号，请稍后再试：服务不可用');
            return;
        }

        notice.remove();
        $('.bot-name').text(data.result.name);
        generateQrCode(data.result.qrCode, (qrUrl) => {
            $('.bot-qrcode').attr('src', qrUrl); 
        });
        $('[rel=qrcode],[rel=start-verify]').removeClass('hide').addClass('show');
        $('[rel=start-verify]>a.link').click(() =>{
            $('[rel=start-verify]').removeClass('show').addClass('hide');
            $('[rel=verify]').removeClass('hide').addClass('show');
        });
        $('#btn-verify').click(() =>{
            verify();
        });
    }).fail(function () {
        const notice = $('[rel=notice]');
        notice.text('暂时无法绑定微信账号，请稍后再试：网络失败');
    });
}

function generateQrCode(content, callback){
    const qrOptions = {
        type: 'image/jpeg',
        rendererOpts: {
            quality: 0.8
        }
    };
    
    QRCode.toDataURL(content, qrOptions, function (err, url) {
        if (err) console.error(err);
        callback(url);
    })
}


function verify() {
    const codeInput = $('#VerificationCode');
    const code = codeInput.val();
    if (!code) {
        codeInput.focus();
        return;
    }

    if (!(/^\d{6}$/.test(code))) {
        alert('验证码格式不正确，请输入收到的 6 位验证码');
        codeInput.focus();
        codeInput.select();
        return;
    }

    $.ajax({
        url: _verifyUrl,
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
                alert('无法验证你输入的验证码：' + (data.message || ''));
            }
        },
        error: function() {
            alert('发生了网络错误，请稍后再试');
        }
    });
}
