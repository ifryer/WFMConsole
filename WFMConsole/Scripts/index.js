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
            url: toUrl("/Home/GetStaffList"),
            success: function (data) {
                console.log(data)
                if (data.success)
                {
                    $.each(data.nameList, function (index, item) {
                        $("#select-name-staff").append("<option value='" + data.idList[index] + "'> " + item + " </option>")
                    })
                    $("#select-name-staff").chosen({ search_contains: true });
                }
            }
        });
        
        $('#start-end-time-off-section').datepair();
        $('#time-off-day').val($.datepicker.formatDate('mm/dd/yy', new Date()))
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

    })

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
        submitTimeOffForm().done(function (json) {
            console.log(json)
        });
    });

    function submitTimeOffForm() {
        var date = $("#time-off-day").val();
        var startTime = $("#time-off-start").val();
        var endTime = $("#time-off-end").val();
        var fullDay = $("#fullDayCheckbox:checked").length > 0
        var notes = $("#time-off-notes").val();

        var id = $("#select-name-staff").val();
        var name = $("#select-name-staff option:selected").html();
        var name = "";
        return $.ajax({
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
                else
                {
                    $(".time-off-form").slideUp();
                    $(".time-off-form input, textarea").val("")
                    showSmallAlert(data.msg);
                }
            }
        });
    }



    $(document).ready(function () {
        initialize();
    });

})();
