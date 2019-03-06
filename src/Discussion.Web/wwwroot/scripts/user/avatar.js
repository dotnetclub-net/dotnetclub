

var selectors, uploadUrl;
var ids = {
  fileSelector: 'avatar-file-picker',
  previewCanvas: 'avatar-preview-canvas'  
};



export function setupUploadAvatar(domItems, uploadApiUrl){
    selectors = domItems;
    uploadUrl = uploadApiUrl;
    
    createHelperElements();
    
    $(selectors.avatarImage).click(function() {
        if ($(this).hasClass('uploading')) {
            return;
        }
        
        $('#' + ids.fileSelector).click();
    });
    $('#' + ids.fileSelector).change(fileSelected);
}


function createHelperElements(){
    var fileSelector = document.createElement('INPUT');
    fileSelector.type = 'file';
    fileSelector.style.display = 'none';
    fileSelector.id = ids.fileSelector;
    fileSelector.setAttribute('accept', 'image/*');
    document.body.appendChild(fileSelector);
    
    var previewCanvas = document.createElement('CANVAS');
    previewCanvas.setAttribute('width', 240);
    previewCanvas.setAttribute('height', 240);
    previewCanvas.style.display = 'none';
    previewCanvas.id = ids.previewCanvas;
    document.body.appendChild(previewCanvas);
}



function fileSelected() {
    if (!this.files.length) {
        return;
    }

    var blob = URL.createObjectURL(this.files[0]);
    var parsedFile = parseFileName(this.files[0].name);
    var img = new Image();
    img.onload = function() {
        var size = 240;
        if (this.width < size || this.height < size) {
            alert('所选的图片太小，请选择 ' + size + 'x' + size + ' 像素以上的方形图片');
        } else {
            var resized = resizeTo(img, size, parsedFile.canBeTransparent);
            $(selectors.avatarImage)[0].src = resized.dataURL;
            
            uploadAvatar(resized.dataURL, 
                parsedFile.name + '.' + resized.ext, 
                'image/' + resized.ext);
        }
        URL.revokeObjectURL(blob);
    };
    img.onerror = function() {
        console.error('所选择的文件不是图片文件');
        URL.revokeObjectURL(blob);
    };
    img.src = blob;
}


function resizeTo(img, size, canBeTransparent) {
    var canvas = $('#' + ids.previewCanvas)[0];
    var ctx = canvas.getContext('2d');

    var wRatio = img.width / size;
    var hRatio = img.height / size;
    var ratio = Math.min(wRatio, hRatio);

    ctx.clearRect(0, 0, canvas.width, canvas.height);
    ctx.drawImage(img,
        0, 0,
        Math.floor(size * ratio), Math.floor(size * ratio),
        0, 0,
        size, size);
    var usePng = canBeTransparent ? hasTransparent(ctx.getImageData(0, 0, size, size)) : false;
    return {
        dataURL: canvas.toDataURL(usePng ? 'image/png' : 'image/jpeg', 0.8),
        ext: usePng ? 'png' : 'jpg'
    };
}

function hasTransparent(imageData) {
    for(var x = 0; x < imageData.width; x++){
        for(var y = 0; y < imageData.height; y++){
            if (imageData.data[3] === 0) {
                return true;
            }
        }
    }

    return false;
}

function parseFileName(fileName) {
    var lastIndexOfDot = fileName.lastIndexOf('.');
    var hasExt = lastIndexOfDot > 0 && lastIndexOfDot < fileName.length - 2;
    var name = hasExt ? fileName.substr(0, lastIndexOfDot) : fileName;
    var ext = hasExt ? fileName.substr(lastIndexOfDot) : '';
    return {
        name: name,
        canBeTransparent: ext.toLowerCase() === '.png' || ext.toLowerCase() === '.gif'
    };
}


function uploadAvatar(dataURL, fileName, mimeType) {
    var u8Image  = b64ToUint8Array(dataURL);
    var formData = new FormData();
    formData.append('file', new Blob([ u8Image ], {type: mimeType}), fileName);
    formData.append('__RequestVerificationToken', window.__RequestVerificationToken);

    $(selectors.avatarImage).addClass('uploading').attr('title', '正在上传，请稍候...');
    $.ajax({
        url: uploadUrl,
        data: formData,
        type: 'POST',
        contentType: false,
        processData: false,
        success: function(data) {
            $(selectors.avatarImage).removeClass('uploading').attr('title', '点击更改头像');
            if (data.hasSucceeded) {
                $(selectors.avatarImage)[0].src = data.result.publicUrl;
                $(selectors.inputField).val(data.result.fileId);
                $(selectors.avatarNote).text('请点击 保存 确认使用新头像');
            }
        },
        error: function() {
            $(selectors.avatarImage).removeClass('uploading').attr('title', '点击更改头像');
            console.error('无法上传头像', arguments);
        }
    });
}


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
