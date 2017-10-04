// ================================ Events ================================


indexScript = (function () {

    function initialize() {
        
        $("#time-off-start").timepicker({
            'defaultTimeDelta': null,
            'scrollDefault': 'now',
            'minTime': '6:00am',
            'maxTime': '8:30pm',
        });

        $("#time-off-end").timepicker({
            'defaultTimeDelta': null,
            'minTime': '6:00am',
            'maxTime': '8:30pm',
            'showDuration': true
        });

        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/GetPageInfo"),
            success: function (data) {
                console.log(data)
                if (data.success)
                {
                    $("#mow-last-sent-date").html(data.mow)
                    $("#down-by-last-sent-date").html(data.downBy)
                    $.each(data.nameList, function (index, item) {
                        $("#select-name-staff").append("<option value='" + data.idList[index] + "'> " + item + " </option>");
                    })
                    $("#select-name-staff").chosen({ search_contains: true });
                }
            }
        });
        
        $('#start-end-time-off-section').datepair();
        $('#time-off-day').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#time-off-day').datepicker();
    }

    $(".log-off-google").on("click", function () {
        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/LogOutGoogle"),
            success: function (data) {
                console.log(data)
            }
        });
    })


    $("#pto-section").on("click", "#new-time-off-btn", function () {
        $(".time-off-form").slideToggle();
        $('#time-off-day').val($.datepicker.formatDate('mm/dd/yy', new Date()))
    });

    $("#fullDayCheckbox").on("change", function () {
        if (this.checked)
            $("#start-end-time-off-section").hide();
        else
            $("#start-end-time-off-section").show();

    });

    $("#pto-section").on("change", "#select-name-staff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#select-name-staff option:selected").text();
        $("#pto-info-staff").slideDown();
        $(".selected-staff-name").html(selectedStaffName)

        let totalPto = 15;
        let usedPto = 4
        $("#total-pto-staff").html(totalPto);
        $("#used-pto-staff").html(usedPto);
        $("#remaining-pto-staff").html(totalPto - usedPto);

    });

    //PTO Form
    $("#pto-section").on("click", "#close-time-off-form-btn", function () {
        $(".time-off-form").slideUp();
        $(".time-off-form input, textarea").val("")
    });

    $("#pto-section").on("click", "#submit-time-off-form-btn", function () {
        submitTimeOffForm();
    });

    function submitTimeOffForm() {
        var date = $("#time-off-day").val();
        var startTime = $("#time-off-start").val();
        var endTime = $("#time-off-end").val();
        var fullDay = $("#fullDayCheckbox:checked").length > 0
        var notes = $("#time-off-notes").val();

        var id = $("#select-name-staff").val();
        var name = $("#select-name-staff option:selected").html();
        if (id == "" || id == null) {
            showSmallError("Please select a staff member to submit time off.");
        }
        else {
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    id: id,
                    name: name,
                    date: date,
                    fullDay: fullDay,
                    startTime: startTime,
                    endTime: endTime,
                    notes: notes
                },
                url: toUrl("Home/SubmitTimeOffForm"),
                success: function (data) {
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        console.log(data)
                        $(".time-off-form").slideUp();
                        $(".time-off-form input, textarea").val("")
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    }

    //Report section

    $("#report-area").on("click", "#create-down-by-report-btn", function () {
        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/CreateDownByReport"),
            success: function (data) {
                console.log(data)
            }
        });
    })

    $("#report-area").on("click", "#create-mow-report-btn", function () {
        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/CreateMOWReport"),
            success: function (data) {
                console.log(data)
            }
        });
    })

    $(document).ready(function () {
        initialize();
    });

})();
