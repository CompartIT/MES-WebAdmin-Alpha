// <summary>
// Get the parameters from URL by name
// </summary>
(function ($) {
    $.getUrlParam
        = function (name) {
            var reg
                = new RegExp("(^|&)" +
                    name + "=([^&]*)(&|$)");
            var r
                = window.location.search.substr(1).match(reg);
            if (r != null) return unescape(r[2]); return null;
        }
})(jQuery);


(function ($) {
    Date.prototype.Format = function (fmt) { //author: meizz   
        var o = {
            "M+": this.getMonth() + 1, //月份   
            "d+": this.getDate(), //日   
            "H+": this.getHours(), //小时   
            "m+": this.getMinutes(), //分   
            "s+": this.getSeconds(), //秒   
            "q+": Math.floor((this.getMonth() + 3) / 3), //季度   
            "S": this.getMilliseconds() //毫秒   
        };
        if (/(y+)/.test(fmt)) fmt = fmt.replace(RegExp.$1, (this.getFullYear() + "").substr(4 - RegExp.$1.length));
        for (var k in o)
            if (new RegExp("(" + k + ")").test(fmt)) fmt = fmt.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        return fmt;
    }  
})(jQuery);

//1000 saparator
(function ($) {
    $.fn.prettynumber = function (options) {
        var opts = $.extend({}, $.fn.prettynumber.defaults, options);
        return this.each(function () {
            $this = $(this);
            var o = $.meta ? $.extend({}, opts, $this.data()) : opts;
            var str = $this.html();
            $this.html($this.html().toString().replace(new RegExp("(^\\d{" + ($this.html().toString().length % 3 || -1) + "})(?=\\d{3})"), "$1" + o.delimiter).replace(/(\d{3})(?=\d)/g, "$1" + o.delimiter));
        });
    };
    $.fn.prettynumber.defaults = {
        delimiter: '.'
    };
})(jQuery);
//example
//$("#number1").prettynumber();
//$("#number2").prettynumber({
//    delimiter: ','
//});


function HoldOnLoading(loadingMsg) {
    HoldOn.open({
        theme: "sk-circle",//If not given or inexistent theme throws default theme sk-rect
        message: "<label class='m-t-20'> " + loadingMsg + " </label>",
        backgroundColor: "#666",//Change the background color of holdon with javascript
        textColor: "white" // Change the font color of the message
    });
}

//Reset the form 
function ResetForm(formID) {
    $("#" + formID).find('input[type=text],select,input[type=hidden]').each(function () {
        $(this).val('');
    });
}