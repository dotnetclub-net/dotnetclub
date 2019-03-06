



function getImageDataURL(file, cb){
    var reader = new FileReader();    
    reader.onload = function () {
        var dataURL = reader.result;
        var img = new Image();
        img.onload = function() {
            cb(dataURL, file.name);
        };
        img.onerror = function() {
            console.error('所选择的文件不是图片文件');
            dataURL = null;
        };
        img.src = dataURL;
    };
    reader.readAsDataURL(file);
}

function upload(dataURL, filename, cb) {
    var u8Image  = b64ToUint8Array(dataURL);
    var formData = new FormData();
    formData.append('file', new Blob([ u8Image ]), filename);
    formData.append('__RequestVerificationToken', window.__RequestVerificationToken);

    $.ajax({
        url: window.uploadUrl,
        data: formData,
        type: 'POST',
        contentType: false,
        processData: false,
        success: function(data) {
            if (data.hasSucceeded) {
                cb(data.result.publicUrl);
            }else{
                console.warn('无法上传图片:' + data.errorMessage);
            }
        },
        error: function() {
            console.error('无法上传图片', arguments);
        }
    });



    function b64ToUint8Array(b64Image) {
        var img = atob(b64Image.split(',')[1]);
        var imgBuffer = [];
        var i = 0;
        while (i < img.length) {
            imgBuffer.push(img.charCodeAt(i));
            i++;
        }
        return new Uint8Array(imgBuffer);
    }
}


export function onImageSelected(file, callback){
    getImageDataURL(file, function(dataURL, filename){
        upload(dataURL, filename, callback);
    });
}
