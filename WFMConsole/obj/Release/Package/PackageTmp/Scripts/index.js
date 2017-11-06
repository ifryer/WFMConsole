// ================================ Events ================================


indexScript = (function () {
    var editedEventTitle = false;
    var mowList;
    var MonthList = ["", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    function initialize() {
        
        $("#event-start-time").timepicker({
            'defaultTimeDelta': null,
            'scrollDefault': 'now',
            'minTime': '6:00am',
            'maxTime': '8:30pm',
        });

        $("#event-end-time").timepicker({
            'defaultTimeDelta': null,
            'minTime': '6:00am',
            'maxTime': '8:30pm',
            'showDuration': true
        });

        $("#event-start-time-modal").timepicker({
            'defaultTimeDelta': null,
            'scrollDefault': 'now',
            'minTime': '6:00am',
            'maxTime': '8:30pm',
        });

        $("#event-end-time-modal").timepicker({
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
                if (data.success)
                {
                    $("#mow-last-sent-date").html(data.mowDate)
                    $("#down-by-last-sent-date").html(data.downByDate)

                    //Set up agent dropdown
                    $.each(data.agentList, function (index, item) {
                        $("#select-name-staff").append("<option teamName = '" + item.TeamName + "' lastName = '"+ item.LastName + "' value='" + item.AgentNo + "'> " + item.FirstName + " " + item.LastName + " </option>");
                    })
                    $("#select-name-staff").chosen({ search_contains: true });

                    //Maybe delete this part? Not really needed with the calendar on the front page anymore...
                    //Set up event list
                    //$.each(data.eventList, function (index, item) {
                    //    $("#schedule-table-body").append(`
                    //        <tr style='background-color:` + item.backgroundColor + `'> 
                    //            <td> ` + item.LastName + ` </td>
                    //            <td> ` + item.TeamName + ` </td>
                    //            <td> ` + item.start + ` </td>
                    //            <td> ` + item.end  + ` </td>
                    //            <td> ` + item.title + ` </td>
                    //            <td> ` + item.EventType + ` </td>
                    //        </tr>
                    //    `)
                    //})

                    //Set up calendar
                    SetUpCalendar(data.eventList);

                    //Set up MOW stuff
                    SetUpMowTable(data.mowSchedule, data.mowList, new moment().format("MM/DD/YYYY"))
                    $(".mow-schedule-event-date").val(new moment().format("MM/DD/YYYY"))

                    SetUpLateShiftTable(data.managerList, data.lateShift)

                    //Set up ICM stuff
                    $("#icm-year").val((new Date()).getFullYear())
                    SetUpIcmTable(data.icmSchedule, data.latestIcmInfo);

                    //Close loading dialog, all done
                    stopLoading();
                }
            }
        });
        $("#repeat-end-date").datepicker();
        $(".late-shift-date").datepicker();
        $('#start-end-event-section').datepair();
        $('#start-end-event-section-modal').datepair();
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-start-date').datepicker();
        $('#event-start-date-modal').datepicker();
        $('#event-end-date').datepicker();
        $('#event-end-date-modal').datepicker();
        $('.mow-schedule-event-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $(".mow-schedule-event-date").datepicker();
    }

    $(document).on("click", "#mow-tab.unclicked", function () {
        $(this).removeClass("unclicked")
        $(".mow-schedule-list").scrollTop(
            $("#mow-date-" + new moment().format("MMDDYYYY")).offset().top - $(".mow-schedule-list").offset().top + $(".mow-schedule-list").scrollTop()
        );
    })

    $(document).on("click", ".log-off-google", function () {
        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/LogOutGoogle"),
            success: function (data) {
                console.log(data)
            }
        });
    })

    $("#pto-section").on("change", "#select-name-staff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#select-name-staff option:selected").text();
        let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
        let teamName = $("#select-name-staff option:selected").attr("teamName");
        let eventType = $("#event-type").val();
        $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        $("#pto-info-staff").slideDown("fast");
        $(".selected-staff-name").html(selectedStaffName);
        $("#team-info-list").hide();
        $("#loading-team-info").show();
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                agentNo: selectedStaff,
            },
            url: toUrl("Home/GetTeamInfo"),
            success: function (data) {
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    $("#team-info-name").html(" - " + data.teamInfo.TeamName);
                    $("#team-info-down-pto").html(data.teamInfo.PTO);
                    $("#team-info-down-training").html(data.teamInfo.Training);
                    $("#team-info-down-loa").html(data.teamInfo.LOA);
                    $("#team-info-down-other").html(data.teamInfo.Other);
                    $("#team-info-down-total").html(data.teamInfo.TotalDown);
                    $("#team-info-list").slideDown("fast");
                    $("#loading-team-info").hide();
                    showSmallAlert(data.msg);
                }
            }
        });


    });


    //Repeating event modal
    

    //TODO: Change event for selectbox
    $(document).on("change", "#repeat-type", function () {
        let repeat_type = $(this).val();
        $("#repeat-every-weekday-checkboxes-tr").hide();
        $("#repeat-every-number-tr").hide()
        switch (repeat_type) {
            case "0": //Daily
                $("#repeat-every-number-tr").show();
                $("#repeat-every-unit").html("days");
                break;
            case "1": //Weekday
                //Hide all
                break;
            case "2": // M W F
                //Hide all
                break;
            case "3": // TU TH
                //Hide all
                break;
            case "4": //Weekly
                $("#repeat-every-number-tr").show();
                $("#repeat-every-unit").html("weeks");
                $("#repeat-every-weekday-checkboxes-tr").show();
                break;
            case "5": //Monthly
                $("#repeat-every-number-tr").show();
                $("#repeat-every-unit").html("months");
                //TODO: Add new thing to choose - repeat by day on month or day number (i.e first thursday of each month, OR every month on the 6th)
                break;
            case "6": //Yearly
                $("#repeat-every-number-tr").show();
                $("#repeat-every-unit").html("years")
                break;
            default:
        }
    });


    //TODO: submit btn
    $(document).on("click", "#save-repeat-modal-btn", function () {
        //Here are all the values we need to submit for this...
        let validationErrors = false;
        let validationText = "";
        let repeat_type = $("#repeat-type") //repeat type, 0-6 same as google calendar
        let repeat_number = $("#repeat-number") // repeat every X ___s
        let repeat_days = "";
         //Get the days of the week to mrepeat on
        $.each($(".repeat-day-checkboxes input:checked"), function (index, item) {
            repeat_days += $(item).val() + " ";
        })
        console.log(repeat_days)
        let repeat_start_date = $("#repeat-start-date")
        let end_type = $(".repeat-end:checked") // Get the repeat end type (Never, Number, Date)
        let end_date = $("#repeat-end-date-input").val();
        let end_number = $("#repeat-end-count-input").val();
        let origin_form = $(this).attr("origin");

        if (repeat_type == "0" || repeat_type == "4" || repeat_type == "5" || repeat_type == "6")
        {
            if (Number.parseInt(repeat_number) <= 0)
            {
                validationErrors = true;
                validationText += " Please enter a valid repeat frequency.  " + repeat_number + " is not a valid input. \r\n "
            }
            if (repeat_type == "4")
            {
                if (repeat_days == "")
                {
                    validationErrors = true;
                    validationText += " Please choose at least one day of the week to repeat the event on. "
                }
            }
        }
        

        if (!validationErrors)
        {
            $("#repeating-event-modal").modal("hide")
        }
        else
        {
            showSmallAlert(validationText)
        }

        //TODO: Add some validation to make sure everything was filled out correctly

    });

    $(document).on("change", ".repeat-end", function () {
        let end_type = $(".repeat-end:checked").val() // Get the repeat end type (Never, Number, Date)
        if (end_type == "number") {
            $("#repeat-end-date-input").attr("disabled", "disabled")
            $("#repeat-end-count-input").removeAttr("disabled")
        }
        if (end_type == "date") {
            $("#repeat-end-count-input").attr("disabled", "disabled")
            $("#repeat-end-date-input").removeAttr("disabled")
        }
        if (end_type == "never") {
            $("#repeat-end-count-input").attr("disabled", "disabled")
            $("#repeat-end-date-input").attr("disabled", "disabled")
        }
    });


    $("document").on("click", '#close-repeat-modal', function () {
        let origin = $("#save-repeat-modal-btn").attr("origin")
        $("#repeating-event-modal").modal("hide")
        if(origin == "create")
            $("#repeatingCheckbox").removeAttr("checked");
        else
            $("#repeatingCheckbox-modal").removeAttr("checked");
    });



    //Edit Event Form

    //TODO: Click event for repeating checkbox
    $(document).on("click", "#repeatingCheckbox-modal", function () {
        $("#save-repeat-modal-btn").attr("origin", "modal")
        $("#repeating-event-modal").modal("toggle")
        let start_date = $("#event-start-date").val();
        $("#repeat-start-date").val(start_date);
        $("#repeat-type").val("0");
        $("#repeat-every-weekday-checkboxes-tr").hide();
        $("#repeat-every-number-tr").show();
        $("#repeat-every-unit").html("days");
    });

    $("#fullDayCheckbox-modal").on("change", function () {
        if (this.checked) {
            $("#event-start-time-modal, #event-end-time-modal").attr("disabled", "disabled")
        }
        else {
            $("#event-start-time-modal, #event-end-time-modal").removeAttr("disabled", "disabled")
        }
    });

    $(document).on("click", "#save-event-modal-btn", function () {
        submitEditEventForm();
    });

    $(document).on("click", "#delete-event-modal-btn", function () {
        if (confirm('Are you sure you want to delete this event?')) {
            let eventId = $("#editing-event-id").val();
            let calendarId = $("#editing-event-calendar-id").val();
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    id: eventId
                },
                url: toUrl("Home/DeleteEvent"),
                success: function (data) {
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $("#edit-calendar-event-modal").modal("toggle")
                        showSmallAlert(data.msg);
                        $('#event-calendar').fullCalendar('removeEvents', calendarId);
                    }
                }
            });
        }
    })

    function submitEditEventForm() {
        startLoading();
        let startDate = $("#event-start-date-modal").val();
        let endDate = $("#event-end-date-modal").val();
        let calendarId = $("#editing-event-calendar-id").val();
        let startTime = $("#event-start-time-modal").val();
        let endTime = $("#event-end-time-modal").val();
        let fullDay = $("#fullDayCheckbox-modal:checked").length > 0
        let notes = $("#event-notes-modal").val();
        let eventType = $("#event-type-modal").val();
        let eventTitle = $("#event-title-modal").val();
        let eventId = $("#editing-event-id").val();

        let originalEvent = $('#event-calendar').fullCalendar('clientEvents', calendarId)[0];

        if (!fullDay && (startTime == null || startTime == "" || endTime == null || endTime == "")) {
            showSmallError("Please make sure you have selected a start and end time, or checked the Full Day checkbox.");
            return;
        }

        if (startDate == null || startDate == "") {
            showSmallError("Please make sure you have selected a start date.");
            return;
        }
        if (endDate == null || endDate == "") {
            endDate = startDate;
        }

        let color = "7";


        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                agentId: 0,
                eventId: eventId,
                title: eventTitle,
                color: color,
                startDate: startDate,
                endDate: endDate,
                fullDay: fullDay,
                startTime: startTime,
                endTime: endTime,
                notes: notes,
                eventType: eventType
            },
            url: toUrl("Home/SubmitEventForm"),
            success: function (data) {
                stopLoading();
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    var newEventHash = data.eventObject;

                    originalEvent.title = newEventHash.title;
                    originalEvent.start = newEventHash.start;
                    originalEvent.end = newEventHash.end;
                    originalEvent.allDay = newEventHash.allDay;
                    originalEvent.backgroundColor = newEventHash.backgroundColor;
                    originalEvent.textColor = newEventHash.textColor;
                    originalEvent.borderColor = newEventHash.borderColor;



                    $('#event-calendar').fullCalendar('updateEvents', [originalEvent]);
                    $("#edit-calendar-event-modal").modal("toggle")
                    //$(".event-form").slideUp();
                    //$(".event-form input, textarea").val("")
                    showSmallAlert(data.msg);
                }
            }
        });

    }

    //Create Event Form

    //TODO: Click event for repeating checkbox
    $(document).on("click", "#repeatingCheckbox", function () {
        $("#save-repeat-modal-btn").attr("origin", "create")
        $("#repeating-event-modal").modal("toggle")
        let start_date = $("#event-start-date").val();
        $("#repeat-start-date").val(start_date);
        $("#repeat-type").val("0");
        $("#repeat-every-weekday-checkboxes-tr").hide();
        $("#repeat-every-number-tr").show();
        $("#repeat-every-unit").html("days");
    });

    $("#pto-section").on("click", "#new-event-btn", function () {
        $(".event-form").slideToggle("fast");
        editedEventTitle = false;
        $("#event-type").val("PTO (Unplanned)");
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-end-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));

    });

    $("#pto-section").on("change", "#fullDayCheckbox", function () {
        if (this.checked) {
            $("#event-start-time, #event-end-time").attr("disabled", "disabled")
            //$("#start-end-event-section").hide();
        }
        else {
            $("#event-start-time, #event-end-time").removeAttr("disabled", "disabled")
            //$("#start-end-event-section").show();
        }
    });

    $("#pto-section").on("keyup", "#event-title", function () {
        editedEventTitle = true;
    })

    $("#pto-section").on("change", "#event-type", function () {
        if (!editedEventTitle) {
            let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
            let teamName = $("#select-name-staff option:selected").attr("teamName");
            let eventType = $("#event-type").val();
            $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        }
    })

    $("#pto-section").on("click", "#close-event-form-btn", function () {
        $(".event-form").slideUp("fast");
        $(".event-form input, textarea").val("")
    });

    $("#pto-section").on("click", "#submit-event-form-btn", function () {
        submitEventForm();
    });

    function submitEventForm() {
        let startDate = $("#event-start-date").val();
        let endDate = $("#event-end-date").val();

        let startTime = $("#event-start-time").val();
        let endTime = $("#event-end-time").val();
        let fullDay = $("#fullDayCheckbox:checked").length > 0
        let notes = $("#event-notes").val();to
        let eventType = $("#event-type").val();
        let eventTitle = $("#event-title").val();
        let agentId = $("#select-name-staff").val();

        if (!fullDay && (startTime == null || startTime == "" || endTime == null || endTime == ""))
        {
            showSmallError("Please make sure you have selected a start and end time, or checked the Full Day checkbox.");
            return;
        }

        if (startDate == null || startDate == "" )
        {
            showSmallError("Please make sure you have selected a start date.");
            return;
        }
        if (endDate == null || endDate == "")
        {
            endDate = startDate;
        }

        let color = "7";
        if (agentId == "" || agentId == null) {
            showSmallError("Please select a staff member to submit time off.");
            return;
        }
        else {
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    eventId: 0,
                    agentId: agentId,
                    title: eventTitle,
                    color: color,
                    startDate: startDate,
                    endDate: endDate,
                    fullDay: fullDay,
                    startTime: startTime,
                    endTime: endTime,
                    notes: notes,
                    eventType: eventType
                },
                url: toUrl("Home/SubmitEventForm"),
                success: function (data) {
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $('#event-calendar').fullCalendar('addEventSource', [data.eventObject]);
                        $(".event-form").slideUp("fast");
                        $(".event-form input, textarea").val("")
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    }

    //MOW & WFO Section
    var newMowRowOptionString = ``;
    var newMowRowString = ``;

    $(document).on("click", "#mow-reset-form-btn", function () {
        if ($(".new-mow-row:not(.clean)").length > 0)
        {
            if (confirm('Are you sure you want to reset this form to the template?')) {
                ResetMowFormToTemplate();
            }
        }
        else
        {
            ResetMowFormToTemplate();
        }
    });

    $(document).on("keypress", '.mow-event-shift-start,  .mow-event-shift-end', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            let tbody = $(this).parents("tbody");
            AppendNewMowRow(tbody);
            $(this).blur();

        }
    });

    $(document).on("click", ".mow-event-add-row", function () {
        let tbody = $(this).parents(".edit-mow-schedule-date-area").find("tbody");
        AppendNewMowRow(tbody)
        
    });

    $(document).on("click", "#add-new-mow-schedule", function () {
        if ($("#new-mow-event-form").css("display") == "none") {
            let tbody = $(".mow-schedule-edit-table").find("tbody");
            AppendNewMowRow(tbody)
            $("#new-mow-event-form").slideDown("fast");
        }
    });

    $(document).on("click", "#close-new-mow-schedule-form", function () {
        if ($(".new-mow-row:not(.clean)").length > 0) {
            if (confirm('Are you sure you want to cancel adding MOW schedule rows?')) {
                $(".mow-schedule-edit-table").find("tbody tr").remove();
                $("#new-mow-event-form").slideUp("fast");
            }
        }
        else {
            $(".mow-schedule-edit-table").find("tbody tr").remove();
            $("#new-mow-event-form").slideUp("fast");
        }
    });

    $(document).on("click", "#save-new-mow-schedule-form", function () {
        SubmitMowForm();
    });

    $(document).on("click", ".mow-event-remove-row", function () {
        $(this).parents("tr").remove();
    });

    $(document).on("click", ".mow-event-save-edit-row", function () {
        let tr = $(this).parents("tr");
        tr.find(".mow-event-save-edit-row").attr("disabled", "disabled")
        tr.find(".mow-event-cancel-edit-row").attr("disabled", "disabled")
        let row_id = tr.attr("row_id");
        let task = tr.find(".mow-event-task").val();
        let agent_no = tr.find(".mow-event-manager").val();
        let start = tr.find(".mow-event-shift-start").val();
        let end = tr.find(".mow-event-shift-end").val();

        let currentMonday = $("#mow-wfo-schedule-date-span").attr("current-monday")
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                    Item: {
                        InputItems: [{
                            "shiftStart": start,
                            "shiftEnd": end,
                            "task": task,
                            "agentNo": agent_no,
                            "rowId": row_id
                        }], Date: $(".mow-schedule-event-date").val() }, MondayString: currentMonday },
            url: toUrl("Home/SubmitMowForm"),
            success: function (data) {

                //$("#save-new-mow-schedule-form").removeAttr("disabled")
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    $(".mow-schedule-edit-table").find("tbody tr").remove();
                    $("#new-mow-event-form").slideUp("fast");
                    SetUpMowTable(data.mowSchedule, null, null);
                    showSmallAlert(data.msg);
                }
            }

        });

    })

    $(document).on("click", ".mow-event-cancel-edit-row", function () {
        let tr = $(this).parents("tr");
        let task = tr.attr("task");
        let row_id = tr.attr("row_id");
        let first_name = tr.attr("first_name");
        let agent_no = tr.attr("agent_no");
        let last_name = tr.attr("last_name");
        let shift_start = tr.attr("shift_start");
        let shift_end = tr.attr("shift_end");

        let original_tr = MowTableRow(task, last_name, first_name, shift_start, shift_end, row_id, agent_no);

        tr.replaceWith(original_tr);

    });

    $(document).on("change", ".new-mow-row.clean input", function () {
        $(".new-mow-row.clean").removeClass("clean");
        let tbody = $(this).parents("tbody");
        AppendNewMowRow(tbody)
    });

    $(document).on("click", ".edit-mow-row-btn", function () {

        let rowId = $(this).attr("rowId");
        let tr = $(this).parents("tr");
        let action_td = $(this).parents("tr");

        let task = tr.find(".mow-display-task").html();
        let last_name = tr.find(".mow-display-name").attr("last_name");
        let first_name = tr.find(".mow-display-name").attr("first_name");
        let shift_start = tr.find(".mow-display-time").attr("start_time");
        let shift_end = tr.find(".mow-display-time").attr("end_time");

        let agent_no = tr.find(".mow-display-name").attr("agent_no")

        //make sure the right option is selected
        let replacement_tr =
            `
            <tr class="edit-mow-row" id="edit-mow-row-` + rowId + `" row_id="` + rowId + `" task="` + task + `" first_name="` + first_name + `" last_name="` + last_name + `" shift_start="` + shift_start + `" shift_end="` + shift_end + `" agent_no="` + agent_no + `" >
                <td>
                    <select class="form-control input-xs mow-event-task" placeholder=""> 
                        <option> AD Escalations </option>
                        <option> Early Shift </option>
                        <option> MOW </option>
                        <option> WFO </option>
                    </select>
                </td>
                <td>
                    <select class="form-control input-xs mow-event-manager" placeholder="">
                        ` + newMowRowOptionString + `
                    </select>
                </td>
                <td>
                    <div style="max-width: 130px" class="form-inline">
                        <input class="form-control input-xs mow-event-shift-start" style="width: calc(50% - 10px);" placeholder="" value="` + shift_start + `">
                        <span style="width: 9px">to</span>
                        <input class="form-control input-xs mow-event-shift-end" style="width: calc(50% - 10px);" placeholder="" value="` + shift_end + `">
                    </div>
                </td>
                <td>
                    <button tabindex="-1" class="btn btn-xs btn-success mow-event-save-edit-row"> <span class="glyphicon glyphicon-ok"></span> </button>
                    <button tabindex="-1" class="btn btn-xs btn-danger mow-event-cancel-edit-row"> <span class="glyphicon glyphicon-remove"></span> </button>
                </td>
            </tr>
        `;
        tr.replaceWith(replacement_tr)
        $(".mow-event-shift-start").timepicker({
            'scrollDefault': '7:00am',
            'minTime': '7:00am',
            'maxTime': '8:30pm',
        });

        $(".mow-event-shift-end").timepicker({
            'minTime': '7:00am',
            'maxTime': '8:30pm',
            'showDuration': true,
        });
        let new_tr = $("#edit-mow-row-" + rowId);
        new_tr.find(".mow-event-task").val(task)
        new_tr.find(".mow-event-manager").val(agent_no)
        //TODO: --
        //Replace each of the 3 TDs with inputs
        //set the initial values and save the old values

        //Hide the edit/delete buttons and show the save/cancel buttons
    })

    $(document).on("click", ".delete-mow-row-btn", function () {
        let rowId = $(this).attr("rowId");
        let deleteRow = $(this).parents("tr");
        if (confirm('Are you sure you want to delete this schedule row?')) {
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    rowId: rowId
                },
                url: toUrl("Home/DeleteMowRow"),
                success: function (data) {
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $(deleteRow).remove();
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    })

    $(document).on("click", ".mow-week-left", function () {
        let currentMonday = $("#mow-wfo-schedule-date-span").attr("current-monday")
        let day = moment(currentMonday).subtract(7, "days");//Get last week monday
        $(".mow-week-right, .mow-week-left").attr("disabled", "disabled")
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                mondayString: day.format("MM/DD/YYYY")
            },
            url: toUrl("Home/GetMowScheduleWeek"),
            success: function (data) {
                $(".mow-week-right, .mow-week-left").removeAttr("disabled")
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    SetUpMowTable(data.mowSchedule, null, day.format("MM/DD/YYYY"))
                }
            }
        });


    });

    $(document).on("click", ".mow-week-right", function () {
        let currentMonday = $("#mow-wfo-schedule-date-span").attr("current-monday")
        let day = moment(currentMonday).add(7, "days");//Get last week monday
        $(".mow-week-right, .mow-week-left").attr("disabled", "disabled")
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                mondayString: day.format("MM/DD/YYYY")
            },
            url: toUrl("Home/GetMowScheduleWeek"),
            success: function (data) {
                $(".mow-week-right, .mow-week-left").removeAttr("disabled")
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    SetUpMowTable(data.mowSchedule, null, day.format("MM/DD/YYYY"))
                }
            }
        });
    });

    function ResetMowFormToTemplate() {
        $(".mow-schedule-edit-table tbody tr").remove();
        var itemList = [{ task: "Early Shift", start: "7:00am", end: "8:00am" }, { task: "MOW", start: "8:00am", end: "12:00pm" }, { task: "WFO", start: "8:00am", end: "12:00pm" }, { task: "AD Escalations", start: "8:00am", end: "12:00pm" }, { task: "MOW", start: "12:00pm", end: "3:30pm" }, { task: "MOW", start: "3:30pm", end: "4:30pm" }, { task: "WFO", start: "12:00pm", end: "4:30pm" }, { task: "AD Escalations", start: "12:00pm", end: "4:30pm" }]
        $.each(itemList, function (index, item) {
            $(".mow-schedule-edit-table tbody").append(
                `
                <tr class="clean new-mow-row">
                    <td>
                        <select class="form-control input-xs mow-event-task" placeholder=""> 
                            <option> AD Escalations </option>
                            <option> Early Shift </option>
                            <option> MOW </option>
                            <option> WFO </option>
                        </select>
                    </td>
                    <td>
                        <select class="form-control input-xs mow-event-manager" placeholder="">
                            ` + newMowRowOptionString + `
                        </select>
                    </td>
                    <td>
                        <div class="form-inline">
                            <input class="form-control input-xs mow-event-shift-start" style="width: calc(50% - 10px);" placeholder="" value="` + item.start + `">
                            <span style="width: 9px">to</span>
                            <input class="form-control input-xs mow-event-shift-end" style="width: calc(50% - 10px);" placeholder="" value="` + item.end + `">
                        </div>
                    </td>
                    <td>
                        <button tabindex="-1" class="btn btn-xs btn-danger mow-event-remove-row"> <span class="glyphicon glyphicon-remove"></span> </button>
                    </td>
                </tr>
            `);
            $(".mow-event-task:last").val(item.task)

        })
        $(".mow-event-shift-start").timepicker({
            'scrollDefault': '7:00am',
            'minTime': '7:00am',
            'maxTime': '8:30pm',
        });

        $(".mow-event-shift-end").timepicker({
            'minTime': '7:00am',
            'maxTime': '8:30pm',
            'showDuration': true,
        });
        $(".mow-event-manager").val("")

    }

    function AppendNewMowRow(tbody) {
        tbody.append(newMowRowString);
        $(".mow-event-shift-start").timepicker({
            'scrollDefault': '7:00am',
            'minTime': '7:00am',
            'maxTime': '8:30pm',
        });

        $(".mow-event-shift-end").timepicker({
            'minTime': '7:00am',
            'maxTime': '8:30pm',
            'showDuration': true,
        });
    }

    function SubmitMowForm() {
        let dataList = [];
        $.each($(".mow-schedule-edit-table tbody tr"), function (index, item) {
            let shiftStart = $(item).find(".mow-event-shift-start").val();
            let shiftEnd = $(item).find(".mow-event-shift-end").val();
            if (shiftStart != "" && shiftEnd != "") {
                let task = $(item).find(".mow-event-task").val();
                let agentNo = $(item).find(".mow-event-manager").val();
                dataList.push({
                    "shiftStart": shiftStart,
                    "shiftEnd": shiftEnd,
                    "task": task,
                    "agentNo": agentNo,
                    "rowId": 0
                });
            }
        });
        if (dataList.length > 0) {
            let currentMonday = $("#mow-wfo-schedule-date-span").attr("current-monday")
            $("#save-new-mow-schedule-form").attr("disabled", "disabled")
            var stringDataList = JSON.stringify(dataList)
            $.ajax({
                type: "post",
                data: { Item: { InputItems: dataList, Date: $(".mow-schedule-event-date").val() }, MondayString: currentMonday },
                dataType: "json",
                url: toUrl("Home/SubmitMowForm"),
                success: function (data) {
                    $("#save-new-mow-schedule-form").removeAttr("disabled")
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $(".mow-schedule-edit-table").find("tbody tr").remove();
                        $("#new-mow-event-form").slideUp("fast");
                        SetUpMowTable(data.mowSchedule, null, null);
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    }


    //ICM Form

    $(document).on("click", "#add-new-icm-row", function () {
        $(".new-icm-form").slideDown("fast");
    });

    $(document).on("click", "#cancel-new-icm-row", function () {
        $(".new-icm-form").slideUp("fast");
    });

    $(document).on("click", "#save-new-icm-row", function () {
        SubmitIcmForm();
    })

    $(document).on("click", ".edit-icm-row-btn", function () {
        let tr = $(this).parents("tr");
        let month = $(this).attr("month");
        let year = $(this).attr("year");
        let manager = $(this).attr("manager");

        $(".new-icm-form").slideDown("fast");
        $("#icm-month").val(month);
        $("#icm-year").val(year);
        $("#icm-manager").val(manager);
    });

    function SubmitIcmForm() {
        let month = $("#icm-month").val();
        let year = $("#icm-year").val();
        let managerAgentNo = $("#icm-manager").val();
        $("#save-new-icm-row").attr("disabled", "disabled")
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                month: month,
                year: year,
                agentNo: managerAgentNo
            },
            url: toUrl("Home/SubmitIcmForm"),
            success: function (data) {
                $("#save-new-icm-row").removeAttr("disabled")
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    //Refresh table
                    SetUpIcmTable(data.icmSchedule, null);
                    //Select next month (and year if needed)
                    if (month == 12) {
                        $("#icm-year").val(Number.parseInt(year) + 1);
                        $("#icm-month").val(1);
                    }
                    else {
                        $("#icm-month").val(Number.parseInt(month) + 1);
                    }
                    //select next manager
                    var sel = document.getElementById('icm-manager');
                    var i = sel.selectedIndex;
                    sel.options[++i % sel.options.length].selected = true;

                    showSmallAlert(data.msg);
                }
            }
        });
    }


    //Late Shift Manager Form

    $(document).on("click", ".save-late-shift-row", function () {
        SubmitLateShiftForm();
    });

    $(document).on("click", ".edit-late-shift-row-btn", function () {
        let tr = $(this).parents("tr")
        let date = tr.find(".date-td").html();
        let name = tr.find(".name-td").attr("agentNo");
        $(".late-shift-date").val(date);
        $(".late-shift-manager-select").val(name);
        $(".new-late-shift-form").slideDown("fast");
    })

    $(document).on("click", "#edit-late-shift-btn", function () {
        $(".new-late-shift-form").slideDown("fast");
    })

    $(document).on("click", "#cancel-late-shift-row", function () {
        $(".new-late-shift-form").slideUp("fast");
    })

    function SubmitLateShiftForm() {
        let date = $(".late-shift-date").val();
        let agentNo = $(".late-shift-manager-select").val();
        if (agentNo != "")
        {
            $.ajax({
                type: "post",
                data: { date: date, agentNo: agentNo },
                dataType: "json",
                url: toUrl("Home/SubmitLateShiftForm"),
                success: function (data) {
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $(".new-late-shift-form").hide();
                        $(".late-shift-date").val("");
                        $(".late-shift-manager-select").val("");
                        SetUpLateShiftTable(null, data.lateShift)
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



    //Initialization functions

    function SetUpIcmTable(icmSchedule, latestIcmInfo)
    {
        let icmTableHtml = ``;
        $.each(icmSchedule, function (index, item) {
            icmTableHtml += `
                            <h4 class="icm-year-header center-text">` + index + `</h4>
                                <table class="icm-schedule-table table table-condensed" id="icm-schedule-table">
                                    <thead>
                                        <tr><th>Month</th><th>Manager</th></tr>
                                    </thead>
                                    <tbody>
                                `;
            $.each(item, function (i, icmRow) {
                icmTableHtml += `<tr><td class="icm-month-td">` + MonthList[icmRow.Month] + `</td><td class="icm-manager-td">` + icmRow.ManagerName + ` <button manager="` + icmRow.AgentNo + `" month="` + icmRow.Month + `" year="` + index + `" class="btn btn-xs btn-info edit-icm-row-btn pull-right"><span class="glyphicon glyphicon-edit"></span></button></td></tr>`;
            });
            icmTableHtml += `</tbody></table>`
        });
        $(".icm-schedule-list").html(icmTableHtml);
        if (latestIcmInfo != null)
        {
            let optionString = ``;
            $.each(latestIcmInfo.ManagerOrder, function (index, manager) {
                optionString += `<option value='` + manager.AgentNo + `'>` + manager.Name + `</option>`
            });
            $("#icm-manager").html(optionString);
            $("#icm-month").val(latestIcmInfo.CurrentMonth)
            $("#icm-year").val(latestIcmInfo.CurrentYear)
            $("#icm-manager").val(latestIcmInfo.CurrentManager)
        }
    }

    function SetUpLateShiftTable(managerList, lateShift)
    {
        if (managerList != null)
        {
            let optionString = ``;
            $.each(managerList, function (i, item) {
                optionString += `<option value="` + item.AgentNo + `"> ` + item.FirstName + " " + item.LastName + `</option>`;
            });
            $(".late-shift-manager-select").append(optionString)
        }
        $(".late-shift-manager-tbody tr").remove();
        $.each(lateShift, function (i, item) {
            $(".late-shift-manager-tbody").append(`<tr> <td class="date-td"> ` + moment(item.Date).format("MM/DD/YYYY") + ` </td> <td agentNo="` + item.AgentNo + `" class="name-td"> ` + item.ManagerName + ` </td> <td> <button rowId="` + item.Id + `" class="btn btn-xs btn-info edit-late-shift-row-btn pull-right"><span class="glyphicon glyphicon-edit"></span></button> </td> </tr>`)
        });
    }

    function SetUpMowTable(mowSchedule, mowList, mondayMomentString)
    {
        //Set the date range
        if (mondayMomentString != null && mondayMomentString != "")
        {
            let mondayMoment = moment(mondayMomentString)
            let day = (mondayMoment).startOf('isoweek'); //Get this week's monday
            let mondayString = day.format("MM/DD")
            fullMondayString = day.format("MM/DD/YYYY")
            $(".mow-schedule-event-date").val(fullMondayString)
            $("#mow-wfo-schedule-date-span").attr("current-monday", fullMondayString);
            day.day(5) //Add 5 days to get friday
            let fridayString = day.format("MM/DD")
            let dateRange = mondayString + " - " + fridayString;
            $("#mow-wfo-schedule-date-span").html(dateRange)
        }

        //Set up the table of events
        let mowTableHtml = ``;
        $.each(mowSchedule, function (index, item) {
            let date_string = moment(index).format("dddd - MM/DD/YYYY")
            mowTableHtml += `
                            <h4 id="mow-date-` + moment(index).format('MMDDYYYY') + `" class="mow-date-header center-text"> ` + date_string + ` </h4>
                                <table class="mow-schedule-table table table-bordered table-condensed" id="mow-schedule-table">
                                    <thead>
                                        <tr><th>Task</th><th>Staff Member</th><th>Shift</th><th></th></tr>
                                    </thead>
                                    <tbody>
                                `;
            $.each(item, function (i, mowRow) {
                mowTableHtml += MowTableRow(mowRow.Task, mowRow.LastName, mowRow.FirstName, mowRow.StartTime, mowRow.EndTime, mowRow.Id, mowRow.AgentNo);
            });
            mowTableHtml += `</tbody></table>`
        });
        $(".mow-schedule-list").html(mowTableHtml);

        //Set up the new MOW row form
        if (mowList != null)
        {
            $.each(mowList, function (index, item) {
                newMowRowOptionString += `<option value=` + item.AgentNo + `> ` + item.LastName + ` </option>`
            });
            newMowRowString = `
                <tr class="clean new-mow-row">
                    <td>
                        <select class="form-control input-xs mow-event-task" placeholder=""> 
                            <option> AD Escalations </option>
                            <option> Early Shift </option>
                            <option> MOW </option>
                            <option> WFO </option>
                        </select>
                    </td>
                    <td>
                        <select class="form-control input-xs mow-event-manager" placeholder="">
                            ` + newMowRowOptionString + `
                        </select>
                    </td>
                    <td>
                        <div class="form-inline">
                            <input class="form-control input-xs mow-event-shift-start" style="width: calc(50% - 10px);" placeholder="">
                            <span style="width: 9px">to</span>
                            <input class="form-control input-xs mow-event-shift-end" style="width: calc(50% - 10px);" placeholder="">
                        </div>
                    </td>
                    <td>
                        <button tabindex="-1" class="btn btn-xs btn-danger mow-event-remove-row"> <span class="glyphicon glyphicon-remove"></span> </button>
                    </td>
                </tr>
            `;
        }
    }


    function SetUpCalendar(eventList) {
        $('#event-calendar').fullCalendar({
            defaultView: 'agendaDay',
            minTime: "07:00:00",
            maxTime: "21:00:00",
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay,listWeek'
            },
            height: 965,
            events: eventList,
            navLinks: true,
            resources: [
                // resources go here
            ],
            dayClick: function (date, jsEvent, view) {

                let strTime = date.format("h:mma")
                let dateString = date.format("MM/DD/YYYY")

                $(".event-form").show();
                $("#event-start-date, #event-end-date").val(dateString);
                $("#event-start-time").val(strTime);
                $("#create-event-form")[0].scrollIntoView({ behavior: "smooth" });

            },
            eventClick: function (calEvent, jsEvent, view) {
                let calendarId = calEvent._id;
                let eventId = calEvent.Id
                let notes = calEvent.Notes;
                let title = calEvent.title;
                let allDay = calEvent.allDay;
                let startDate = calEvent.StartDate;
                let endDate = calEvent.EndDate;
                let startTime = calEvent.StartTime;
                let endTime = calEvent.EndTime;
                let eventType = calEvent.EventType;

                if (allDay) {
                    $("#event-start-time-modal, #event-end-time-modal").attr("disabled", "disabled")
                }
                else {
                    $("#event-start-time-modal, #event-end-time-modal").removeAttr("disabled", "disabled")
                }

                $("#event-title-modal").val(title);
                $("#event-notes-modal").val(notes);
                $("#event-start-date-modal").val(startDate);
                $("#event-end-date-modal").val(endDate);
                $("#event-start-time-modal").val(startTime);
                $("#event-end-time-modal").val(endTime);
                $("#event-type-modal").val(eventType);
                $("#editing-event-id").val(eventId)
                $("#editing-event-calendar-id").val(calendarId)
                $("#edit-calendar-event-modal").modal();

                // change the border color just for fun
                //$(this).css('border-color', 'red');

            }
        })
    }


    //HTML Generator functions

    function MowTableRow(Task, LastName, FirstName, StartTime, EndTime, RowId, AgentNo)
    {
        let extraClass = ""
        if (Task == "AD Escalations") {
            extraClass = "escalation-row"
        }
        return `<tr class="mow-display-row ` + extraClass + `"><td class="mow-display-task">` + Task + `</td><td class="mow-display-name" agent_no="` + AgentNo +`" first_name="` + FirstName + `" last_name="` + LastName + `">` + FirstName + " " + LastName + `</td><td start_time="` + StartTime + `" end_time="` + EndTime + `" class="mow-display-time">` + StartTime + " - " + EndTime + `</td><td style="width: 65px;"> <button rowId="` + RowId + `" class="btn btn-xs btn-info edit-mow-row-btn"><span class="glyphicon glyphicon-edit"></span></button> <button rowId="` + RowId + `" class="btn btn-xs btn-danger delete-mow-row-btn"><span class="glyphicon glyphicon-trash"></span></button> </td></tr>`
    }

})();
