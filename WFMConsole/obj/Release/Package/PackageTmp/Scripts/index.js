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
                    $.each(data.eventList, function (index, item) {
                        $("#schedule-table-body").append(`
                            <tr style='background-color:` + item.backgroundColor + `'> 
                                <td> ` + item.LastName + ` </td>
                                <td> ` + item.TeamName + ` </td>
                                <td> ` + item.start + ` </td>
                                <td> ` + item.end  + ` </td>
                                <td> ` + item.title + ` </td>
                                <td> ` + item.EventType + ` </td>
                            </tr>
                        `)
                    })

                    //Set up calendar
                    SetUpCalendar(data.eventList);

                    //Set up MOW stuff
                    SetUpMowTable(data.mowSchedule, data.mowList, new moment().format("MM/DD/YYYY"))
                    
                    //Set up ICM stuff
                    $("#icm-year").val((new Date()).getFullYear())
                    SetUpIcmTable(data.icmSchedule, data.latestIcmInfo);

                    //Close loading dialog, all done
                    stopLoading();
                }
            }
        });
        
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


    $("#pto-section").on("change", "#select-name-staff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#select-name-staff option:selected").text();
        let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
        let teamName = $("#select-name-staff option:selected").attr("teamName");
        let eventType = $("#event-type").val();
        $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        $("#pto-info-staff").slideDown();
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
                    $("#team-info-list").slideDown();
                    $("#loading-team-info").hide();
                    showSmallAlert(data.msg);
                }
            }
        });


    });

    //Edit Event Form

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

    $("#pto-section").on("click", "#new-event-btn", function () {
        $(".event-form").slideToggle();
        editedEventTitle = false;
        $("#event-type").val("PTO (Unplanned)");
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-end-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));

    });

    $("#fullDayCheckbox").on("change", function () {
        if (this.checked) {
            $("#event-start-time, #event-end-time").attr("disabled", "disabled")
            //$("#start-end-event-section").hide();
        }
        else {
            $("#event-start-time, #event-end-time").removeAttr("disabled", "disabled")
            //$("#start-end-event-section").show();
        }
    });

    $("#event-title").on("keyup", function () {
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
        $(".event-form").slideUp();
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
        let notes = $("#event-notes").val();
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
                        $(".event-form").slideUp();
                        $(".event-form input, textarea").val("")
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    }

    //MOW Section
    var newMowRowOptionString = ``;
    var newMowRowString = ``;

    $(document).on("keypress", '.mow-event-shift-start,  .mow-event-shift-end', function (event) {
        var keycode = (event.keyCode ? event.keyCode : event.which);
        if (keycode == '13') {
            let tbody = $(this).parents("tbody");
            AppendNewMowRow(tbody)
            $(this).blur();

        }
    });

    $(document).on("click", ".mow-event-add-row", function () {
        let tbody = $(this).parents(".edit-mow-schedule-date-area").find("tbody");
        AppendNewMowRow(tbody)
        
    });

    $(document).on("click", "#add-new-icm-row", function () {
        $(".new-icm-form").slideDown("fast");
    });

    $(document).on("click", "#cancel-new-icm-row", function () {
        $(".new-icm-form").slideUp("fast");
    });

    $(document).on("click", "#save-new-icm-row", function () {
        SubmitIcmForm();
    })

    $(document).on("click", "#add-new-mow-schedule", function () {
        if ($("#new-mow-event-form").css("display") == "none") {
            let tbody = $(".mow-schedule-edit-table").find("tbody");
            AppendNewMowRow(tbody)
            $("#new-mow-event-form").slideDown("fast");
        }
    });

    $(document).on("click", "#close-new-mow-schedule-form", function () {
        $(".mow-schedule-edit-table").find("tbody tr").remove();
        $("#new-mow-event-form").slideUp("fast");
    });

    $(document).on("click", "#save-new-mow-schedule-form", function () {
        SubmitMowForm();
    })

    $(document).on("click", ".mow-event-remove-row", function () {
        $(this).parents("tr").remove();
    });

    $(document).on("change", ".new-mow-row.clean input", function () {
        $(".new-mow-row.clean").removeClass("clean");
        let tbody = $(this).parents("tbody");
        AppendNewMowRow(tbody)
    });

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

    function AppendNewMowRow(tbody)
    {
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

    function SubmitMowForm() {
        let dataList = [];
        $.each($(".mow-schedule-edit-table tbody tr"), function (index, item) {
            let shiftStart = $(item).find(".mow-event-shift-start").val();
            let shiftEnd = $(item).find(".mow-event-shift-end").val();
            if (shiftStart != "" && shiftEnd != "")
            {
                let task = $(item).find(".mow-event-task").val();
                let agentNo = $(item).find(".mow-event-manager").val();
                dataList.push({
                    "shiftStart": shiftStart,
                    "shiftEnd": shiftEnd,
                    "task": task,
                    "agentNo": agentNo
                });
            }
        });
        if (dataList.length > 0)
        {
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
                icmTableHtml += `<tr><td>` + MonthList[icmRow.Month] + `</td><td>` + icmRow.ManagerName + `</td></tr>`;
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
                let extraClass = ""
                console.log(mowRow.Task)
                if (mowRow.Task == "AD Escalations")
                {
                    extraClass = "escalation-row"
                }
                mowTableHtml += `<tr class="` + extraClass + `"><td>` + mowRow.Task + `</td><td>` + mowRow.FirstName + " " + mowRow.LastName + `</td><td>` + mowRow.StartTime + " - " + mowRow.EndTime + `</td><td style="width: 30px;"> <button rowId="`+ mowRow.Id + `" class="btn btn-xs btn-danger delete-mow-row-btn"><span class="glyphicon glyphicon-trash"></span></button> </td></tr>`;
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
                        <button class="btn btn-xs btn-danger mow-event-remove-row"> <span class="glyphicon glyphicon-remove"></span> </button>
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

})();
