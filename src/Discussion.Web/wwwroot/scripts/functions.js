
export function describeTime(timestamp) {
    var second = 1000;
    var minute = 60 * second;
    var hour = 60 * minute;
    var day = 24 * hour;
    var month = 30 * day;
    var year = 12 * month;

    var utcNow = new Date().getTime();
    var span = utcNow - timestamp;

    if (span > year) {
        return Math.round(span / year) + ' 年之前';
    }
    if (span > month) {
        return Math.round(span / month) + ' 个月前';
    }
    if (span > day) {
        return Math.round(span / day) + ' 天之前';
    }
    if (span > hour) {
        return Math.round(span / hour) + ' 小时前';
    }
    if (span > minute) {
        return Math.round(span / minute) + ' 分钟前';
    }

    return '刚刚';
}

function realTime(timestamp) {
    var time = new Date();
    time.setTime(timestamp);
    
    var parts = [ time.getFullYear(), '-', time.getMonth() + 1, '-', time.getDate(), ' ', time.getHours(), ':', time.getMinutes()];
    return parts.join('');
}

export function transformTimestampOn(domSelector, attr) {
    $(domSelector).each(function() {
        var item = $(this);
        var lastRepliedAt = item.attr(attr);
        if (lastRepliedAt) {
            var timestamp = parseInt(lastRepliedAt);
            item.text(describeTime(timestamp))
                .attr('title', realTime(timestamp));
        }
    });
}
