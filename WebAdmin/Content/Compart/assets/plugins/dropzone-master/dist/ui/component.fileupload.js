!function (n) {
        "use strict";
        function t() {
            this.$body = n("body");
        }
        t.prototype.init = function () {
            Dropzone.autoDiscover = !1,
                n('[data-plugin="dropzone"]').each(function () {
                    var t = n(this).attr("action"),
                        i = n(this).data("previewsContainer"),
                        e = {
                            url: t,
                            acceptedFiles: ".mp4,.avi,.wmv,.mpg,.mpeg,.jpg,.pdf",
                            
                            init: function () {
                                this.on("success", function (file, data) {
                                    ////上传成功触发的事件
                                    //$('.ErrMsgBox').append("<div class='ErrMsg'><i class='fa fa-check-square'></i>" + data.ErrMsg + "!</div>");
                                    //$(".ErrMsgBox").show();
                                });
                                this.on("error", function (file, data) {
                                    //$('.ErrMsgBox').append("<div class='ErrMsg'><i class='fa fa-warning'></i>" + data.ErrMsg + "!</div>");
                                    //$(".ErrMsgBox").show();
                                });
                            },

                        };
                    i && (e.previewsContainer = i);
                    var o = n(this).data("uploadPreviewTemplate");
                    o && (e.previewTemplate = n(o).html());
                    n(this).dropzone(e)
                });


        },
            n.FileUpload = new t,
            n.FileUpload.Constructor = t
    }(window.jQuery),
    function () {
        "use strict";
        window.jQuery.FileUpload.init()
    }();