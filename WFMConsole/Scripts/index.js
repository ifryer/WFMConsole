// ================================ Events ================================


indexScript = (function () {
    var editedEventTitle = false;
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
                console.log(data)
                if (data.success)
                {
                    $("#mow-last-sent-date").html(data.mow)
                    $("#down-by-last-sent-date").html(data.downBy)
                    $.each(data.agentList, function (index, item) {
                        $("#select-name-staff").append("<option teamName = '" + item.TeamName + "' lastName = '"+ item.LastName + "' value='" + item.AgentNo + "'> " + item.FirstName + " " + item.LastName + " </option>");
                    })
                    $("#select-name-staff").chosen({ search_contains: true });

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

                    setUpCalendar(data.eventList);
                    stopLoading();
                }
            }
        });
        
        $('#start-end-event-section').datepair();
        $('#start-end-event-section-modal').datepair();
        $('#event-day').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-day').datepicker();
        $('#event-day-modal').datepicker();
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
                    //console.log(data);
                    //$(".event-form").slideUp();
                    //$(".event-form input, textarea").val("")
                    showSmallAlert(data.msg);
                }
            }
        });


        //let totalPto = 15;
        //let usedPto = 4
        //$("#total-pto-staff").html(totalPto);
        //$("#used-pto-staff").html(usedPto);
        //$("#remaining-pto-staff").html(totalPto - usedPto);

    });

    function setUpCalendar(eventList)
    {
        $('#event-calendar').fullCalendar({
            defaultView: 'agendaDay',
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'month,agendaWeek,agendaDay,listWeek'
            },
            height: 900,
            events: eventList,
            resources: [
                // resources go here
            ],
            eventClick: function (calEvent, jsEvent, view) {

                console.log(calEvent)
                console.log(jsEvent)
                console.log(view)

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

    //Edit Event Form



    $("#fullDayCheckbox-modal").on("change", function () {
        console.log(this.checked)
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
            console.log(calendarId)
            console.log("0-----------0")
            console.log(eventId)
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
        let startDate = $("#event-start-date-modal").val();
        let endDate = $("#event-end-date-modal").val();

        let startTime = $("#event-start-time-modal").val();
        let endTime = $("#event-end-time-modal").val();
        let fullDay = $("#fullDayCheckbox-modal:checked").length > 0
        let notes = $("#event-notes-modal").val();
        let eventType = $("#event-type-modal").val();
        let eventTitle = $("#event-title-modal").val();
        let eventId = $("#editing-event-id").val();

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

        if (id == "" || id == null) {
            showSmallError("Please select a staff member to submit time off.");
            return;
        }
        else {
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    id: id,
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
                        console.log(data)
                        $(".event-form").slideUp();
                        $(".event-form input, textarea").val("")
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
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
        let id = $("#select-name-staff").val();

        console.log(startTime)
        if (!fullDay && (startTime == null || startTime == "" || endTime == null || endTime == ""))
        {
            console.log("!!!!!!!!")
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
        if (id == "" || id == null) {
            showSmallError("Please select a staff member to submit time off.");
            return;
        }
        else {
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    id: id,
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
                        console.log(data)
                        $(".event-form").slideUp();
                        $(".event-form input, textarea").val("")
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
