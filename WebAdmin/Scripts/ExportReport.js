function ExportReport(ReportPage, PartNum, RevID, PrintType) {
    var form = $('<form></form>');
    form.attr('action', ReportPage);
    form.attr('method', 'post');
    form.attr('enctype', 'multipart/form-data');
    form.attr('target', '_blank');

    var InputPartNum = $('<input type="hidden" name="PartNum" />');
    var InputRevID = $('<input type="hidden" name="RevID" />');
    var InputType = $('<input type="hidden" name="PrintType" />');
    InputPartNum.attr('value', PartNum);
    InputRevID.attr('value', RevID);
    InputType.attr('value', PrintType);
    form.append(InputPartNum);
    form.append(InputRevID);
    form.append(InputType);
    $(document.body).append(form);
    form.submit();
}

/*----------信息提示框-------*/
function LoadWaitMsg(Msg) {
    if ($('#ModalLoading').length == 0) {
        var Modal = $('<div class="modal fade" style="text-align:center;vertical-align:middle;" id="ModalLoading" data-backdrop="static" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true"></div>');
        var ModalMsg = $('<div class="modal-centre" id="ModalLoadingCentre"><div class="icon-list-demo" id="ModalLoadingMsg"><div class="alert-link"><i class="fa fa-spin fa-cog"></i>' + Msg + '</div></div></div>');
        var ModalBody = $('<div class="modal-dialog" style="display:none;"><div class="modal-body"></div></div>');
        Modal.append(ModalMsg);
        Modal.append(ModalBody);
        $(document.body).append(Modal);
    }
    else {
        $('#ModalLoadingMsg').remove();
        var ModalLoadingMsg = $('<div class="icon-list-demo" id="ModalLoadingMsg"><div class="alert-link"><i class="fa fa-spin fa-cog"></i>' + Msg + '</div></div>');
        $('#ModalLoadingCentre').append(ModalLoadingMsg);
    }
    $("#ModalLoading").modal();
}

function CloseWaitMsg() {
    setTimeout(function () { $("#ModalLoading").modal("hide"); }, 1000);
}


/*----------文本提示框-------*/
function LoadMsg(Title, Msg) {
    if ($('#ModalLoadMsg').length == 0) {
        var Modal = $('<div class="modal fade" style="text-align:center;vertical-align:middle;" id="ModalLoadMsg" data-backdrop="true" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true"></div>');
        var ModalMsg = $('<div class="modal-centre2"  onclick="CloseMsg()" id="ModalLoadMsgCentre"><span id="CentreMsg" class="mytooltipNew tooltip-effect-1"><span class="tooltip-item2">' + Title + '</span><span class="tooltip-content4 clearfix"><span class="tooltip-text2">' + Msg + '</span></span></span></div>');
        var ModalBody = $('<div class="modal-dialog" style="display:none;"><div class="modal-body"></div></div>');
        Modal.append(ModalMsg);
        Modal.append(ModalBody);
        $(document.body).append(Modal);
    }
    else {
        $('#CentreMsg').remove();
        var CentreMsg = $('<span id="CentreMsg" class="mytooltipNew tooltip-effect-1"><span class="tooltip-item2">' + Title + '</span><span class="tooltip-content4 clearfix"><span class="tooltip-text2">' + Msg + '</span></span></span></div>');
        $('#ModalLoadMsgCentre').append(CentreMsg);
    }
    $("#ModalLoadMsg").modal();
}

function CloseMsg() {
    setTimeout(function () { $("#ModalLoadMsg").modal("hide"); }, 50);
}



/*---------DataTable--------*/
function setUserColumnsDefWidths(pageTableId, columnDefs) {
    var userColumnDef;
    // Get the settings for this table from localStorage
    var userColumnDefs = JSON.parse(localStorage.getItem(pageTableId)) || [];
    if (userColumnDefs.length === 0) return;
    //console.log(pageTableId, columnDefs);
    columnDefs.forEach(function (columnDef) {
        // Check if there is a width specified for this column
        userColumnDef = userColumnDefs.find(function (column) {
            return column.targets === columnDef.targets;
        });
        // If there is, set the width of this columnDef in px
        if (userColumnDef) {
            columnDef.width = userColumnDef.width + 'px';
            columnDef.sWidth = userColumnDef.width + 'px';
        }
    });
}

function saveColumnSettings(pageTableId, table) {
    var userColumnDefs = JSON.parse(localStorage.getItem(pageTableId)) || [];
    var width, header, existingSetting;

    table.columns().every(function (targets) {
        // Check if there is a setting for this column in localStorage
        existingSetting = userColumnDefs.findIndex(function (column) { return column.targets === targets; });
        // Get the width of this column
        header = this.header();
        width = $(header).width();

        if (existingSetting !== -1) {
            // Update the width
            userColumnDefs[existingSetting].width = width;
        } else {
            // Add the width for this column
            userColumnDefs.push({
                targets: targets,
                width: width,
            });
        }
    });

    // Save (or update) the settings in localStorage
    localStorage.setItem(pageTableId, JSON.stringify(userColumnDefs));
}


//右上角弹框
function ShowWaringMsg(Msg = '', MsgDetail = '', time = 5000, fun = null) {
    $.toast({
        heading: Msg,
        text: MsgDetail,
        position: 'top-right',
        loaderBg: '#ffb22b',
        icon: 'warning',
        hideAfter: time,
        stack: 6,
        afterHidden: function () {
            if (fun != null)
                fun();
            }
    });
}