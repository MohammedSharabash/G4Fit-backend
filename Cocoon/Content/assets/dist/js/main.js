$(function () {
    $("#table").DataTable({
        ordering: false,
        language: {
            "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json"
        }
    })
    $('#report-range, #report-export-range').daterangepicker({
        "locale": {
            "format": "MM/DD/YYYY",
            "separator": " - ",
            "applyLabel": "اختيار",
            "cancelLabel": "إلغاء",
            "fromLabel": "من",
            "toLabel": "إلى",
            "customRangeLabel": "Custom",
            "daysOfWeek": [
                "Su",
                "Mo",
                "Tu",
                "We",
                "Th",
                "Fr",
                "Sa"
            ],
            "monthNames": [
                "يناير",
                "فبراير",
                "مارس",
                "ابريل",
                "مايو",
                "يونيو",
                "يوليو",
                "اغسطس",
                "سبتمبر",
                "اكتوبر",
                "نوفمبر",
                "ديسمبر"
            ],
            "firstDay": 1
        }
    })

    $("#report-range").change(function () {
        var StoreId = $("#Store").val();
        if (StoreId == null || StoreId == undefined || StoreId == NaN) {
            alert("من فضلك قم باختيار المخزن أولاً");
        }
        else {
            $("#cover-spin").show();
            $.ajax({
                url: '/Reports/SelectImportReportDateRange',
                data: { Range: $(this).val(), StoreId: StoreId },
                success: function (results) {
                    $("#report-data").empty();
                    $("#report-data").html(results);
                    $("#cover-spin").hide();
                    $("#table").DataTable({
                        ordering: false,
                        language: {
                            "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json"
                        }
                    })
                }
            })
        }
    })

    $("#report-export-range").change(function () {
        var StoreId = $("#Store").val();
        if (StoreId == null || StoreId == undefined || StoreId == NaN) {
            alert("من فضلك قم باختيار المخزن أولاً");
        }
        else {
            $("#cover-spin").show();
            $.ajax({
                url: '/Reports/SelectExportReportDateRange',
                data: { Range: $(this).val(), StoreId: StoreId },
                success: function (results) {
                    $("#report-data").empty();
                    $("#report-data").html(results);
                    $("#cover-spin").hide();
                    $("#table").DataTable({
                        ordering: false,
                        language: {
                            "url": "//cdn.datatables.net/plug-ins/1.10.20/i18n/Arabic.json"
                        }
                    })
                }
            })
        }
    })
})


function Quick(q) {
    var StoreId = $("#Store").val();
    if (StoreId == null || StoreId == undefined || StoreId == NaN) {
        alert("من فضلك قم باختيار المخزن أولاً");
    }
    else {
        $("#cover-spin").show();
        $.ajax({
            url: '/Reports/QuickImportActions',
            data: { q: q, StoreId: StoreId },
            success: function (results) {
                $("#report-data").empty();
                $("#report-data").html(results);
                $("#cover-spin").hide();
            }
        })
    }
}


function QuickExport(q) {
    var StoreId = $("#Store").val();
    if (StoreId == null || StoreId == undefined || StoreId == NaN) {
        alert("من فضلك قم باختيار المخزن أولاً");
    }
    else {
        $("#cover-spin").show();
        $.ajax({
            url: '/Reports/QuickExportActions',
            data: { q: q, StoreId: StoreId },
            success: function (results) {
                $("#report-data").empty();
                $("#report-data").html(results);
                $("#cover-spin").hide();
            }
        })
    }
}

// To mark the notificattion as seen
function MarkNotificationAsSeen(id) {
    $.ajax({
        url: "/Notifications/MarkNotificationAsSeen",
        data: { Id: id },
        type: "POST"
    });
    $("#not_" + id).removeAttr("style");
    var CurrentNotificationNumber = parseInt($("#inside-notifications-number").text());
    if (CurrentNotificationNumber - 1 > 0) {
        $("#inside-notifications-number").text(CurrentNotificationNumber - 1);
        $("#outside-notifications-number").text(CurrentNotificationNumber - 1);
        document.title = "لوحة تحكم نزهة (" + (CurrentNotificationNumber - 1) + ")";
    }
    else {
        $("#inside-notifications-number").text("0");
        $("#outside-notifications-number").hide();
        document.title = "لوحة تحكم نزهة";
    }
    
}