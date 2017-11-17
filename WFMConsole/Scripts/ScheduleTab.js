scheduleTab = (function () {

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

        $("#repeat-end-date-input").datepicker();
        $('#start-end-event-section').datepair();
        $('#start-end-event-section-modal').datepair();
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-start-date').datepicker();
        $('#event-start-date-modal').datepicker();
        $('#event-end-date').datepicker();
        $('#event-end-date-modal').datepicker();
    }



    $("#pto-section").on("change", "#select-name-staff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#select-name-staff option:selected").text();
        let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
        let teamName = $("#select-name-staff option:selected").attr("teamName");
        let eventType = $("#event-type").val();
        $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        //$("#pto-info-staff").slideDown("fast");
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


    $(document).on("change", "#repeat-type", function () {
        let repeat_type = $(this).val();

        SetUpRepeatForm(repeat_type, null, null, null, null, null)
    });

    $(document).on("click", "#cancel-event-modal-btn", function () {
        if (confirm('Are you sure you want to change the cancelled status of this event?')) {
            CancelEvent();
        };
    });

    $(document).on("click", "#save-repeat-modal-btn", function () {
        //Here are all the values we need to submit for this...
        let validationErrors = false;
        let validationText = "";
        let repeat_type = $("#repeat-type").val(); //repeat type, 0-6 same as google calendar
        let repeat_every_number = $("#repeat-every-number").val(); // repeat every X ___s
        if (repeat_every_number <= 0)
            repeat_every_number = 1;
        let repeat_on_days = ""; //MO TU WE , etc.
        //Get the days of the week to mrepeat on
        $.each($(".repeat-day-checkboxes input:checked"), function (index, item) {
            repeat_on_days += $(item).val() + ",";
        })
        let repeat_summary = $(".repeat-form-summary").html();
        let repeat_start_date = $("#repeat-start-date").val();
        let end_type = $(".repeat-end:checked").val(); // Get the repeat end type (Never, Number, Date)
        let end_date = $("#repeat-end-date-input").val();
        let end_after_number = $("#repeat-end-number-input").val();
        let origin_form = $(this).attr("origin");

        if (repeat_type == "0" || repeat_type == "4" || repeat_type == "5" || repeat_type == "6") {
            if (Number.parseInt(repeat_every_number) <= 0) {
                validationErrors = true;
                validationText += " Please enter a valid repeat frequency.  " + repeat_every_number + " is not a valid input. \r\n "
            }
            if (repeat_type == "4") {
                if (repeat_on_days == "") {
                    validationErrors = true;
                    validationText += " Please choose at least one day of the week to repeat the event on. "
                }
            }
        }

        if (!validationErrors) {
            $("#repeating-event-modal").modal("hide")
            if (origin_form == "create")
                var checkbox = $("#repeatingCheckbox");
            else
                var checkbox = $("#repeatingCheckbox-modal");
            checkbox.attr("repeat-type", repeat_type);
            checkbox.attr("repeat-every-number", repeat_every_number);
            checkbox.attr("repeat-on-days", repeat_on_days);
            checkbox.attr("end-type", end_type);
            checkbox.attr("end-date", end_date);
            checkbox.attr("end-after-number", end_after_number);
            checkbox.attr("summary", repeat_summary)
        }
        else {
            showSmallAlert(validationText)
        }

        //TODO: Add some validation to make sure everything was filled out correctly

    });

    $(document).on("change", ".repeat-end", function () {
        let end_type = $(".repeat-end:checked").val() // Get the repeat end type (Never, Number, Date)
        if (end_type == "number") {
            $("#repeat-end-date-input").attr("disabled", "disabled")
            $("#repeat-end-number-input").removeAttr("disabled")
        }
        if (end_type == "date") {
            $("#repeat-end-number-input").attr("disabled", "disabled")
            $("#repeat-end-date-input").removeAttr("disabled")
        }
        if (end_type == "never") {
            $("#repeat-end-number-input").attr("disabled", "disabled")
            $("#repeat-end-date-input").attr("disabled", "disabled")
        }
    });


    $(document).on("click", "#close-repeat-modal", function () {
        let origin = $("#save-repeat-modal-btn").attr("origin")
        let editing = $(this).attr("editing")
        $("#repeating-event-modal").modal("hide")
        if (editing != "editing") {
            if (origin == "create")
                $("#repeatingCheckbox").removeAttr("checked");
            else
                $("#repeatingCheckbox-modal").removeAttr("checked");
        }

    });



    //Edit Event Form

    $(document).on("change", "#event-type-modal", function () {
        switch ($(this).val()) {
            case "PTO (Unplanned)":
            case "Unpaid Time Off (Unplanned)":
            case "LOA":
                $(".event-color-modal.chosen").removeClass("chosen");
                $(".event-color-modal.color-9").addClass("chosen");
                break;
            case "Training":
                $(".event-color-modal.chosen").removeClass("chosen");
                $(".event-color-modal.color-7").addClass("chosen");
                break;
            case "PTO (Planned)":
            case "Unpaid Time Off (Planned)":
                $(".event-color-modal.chosen").removeClass("chosen");
                $(".event-color-modal.color-10").addClass("chosen");
                break;
            case "Other":
                $(".event-color-modal.chosen").removeClass("chosen");
                $(".event-color-modal.color-11").addClass("chosen");
                break;
        }
    })

    $(document).on("click", ".event-color-modal", function () {
        $(".event-color-modal.chosen").removeClass("chosen");
        $(this).addClass("chosen");
    })

    $(document).on("change", "#event-start-date-modal", function () {
        let startDate = $(this).val();
        if (moment(startDate) > moment($("#event-end-date-modal").val())) {
            $("#event-end-date-modal").val(startDate);
        }
        if ($("#editing-event-same-date").attr("same") == "1")
        {
            $("#event-end-date-modal").val(startDate);
        }
    })

    $(document).on("change", "#event-end-date-modal", function () {
        let endDate = $(this).val();
        if (moment(endDate) < moment($("#event-start-date-modal").val())) {
            $("#event-start-date-modal").val(endDate);
        }
        if ($("#editing-event-same-date").attr("same") == "1") {
            $("#editing-event-same-date").attr("same", "0");
        }
    })

    //TODO: Click event for repeating checkbox
    $(document).on("click", "#repeatingCheckbox-modal", function () {
        if ($(this).is(":checked")) {
            SetUpRepeatForm("0", "1", null, null, null, null)
            $("#save-repeat-modal-btn").attr("origin", "modal")
            $("#close-repeat-modal").removeAttr("editing")
            $("#repeating-event-modal").modal("toggle")
            let start_date = $("#event-start-date-modal").val();
            $("#repeat-start-date").val(start_date);
        }
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

    $(document).on("click", "#repeatingCheckbox-modal-edit", function () {
        //GetRepeatingEventInfo(eventId)
        let eventId = $("#editing-event-id").val();
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                eventId: eventId,
            },
            url: toUrl("Home/GetRepeatingEventInfo"),
            success: function (data) {
                if (!data.success) {
                    console.log("error -- " + data.msg);
                }
                else {
                    SetUpRepeatForm(data.results.repeatType, data.results.repeatEveryNumber, data.results.repeatOnDays, data.results.repeatEndType, data.results.repeatEndDate, data.results.repeatEndAfterNumber);
                    $("#save-repeat-modal-btn").attr("origin", "modal");
                    $("#close-repeat-modal").attr("editing", "editing");
                    $("#repeating-event-modal").modal("toggle");
                    let start_date = $("#event-start-date-modal").val();
                    $("#repeat-start-date").val(start_date);
                }
            }
        });
    })

    $(document).on("click", "#delete-event-modal-btn", function () {
        if (confirm('Are you sure you want to delete this event?')) {
            $(this).prop("disabled", "disabled")
            startLoading();
            let eventId = $("#editing-event-id").val();
            //let calendarId = $("#editing-event-calendar-id").val();
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    id: eventId
                },
                url: toUrl("Home/DeleteEvent"),
                success: function (data) {
                    stopLoading();
                    $("#delete-event-modal-btn").removeAttr("disabled")
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        $("#edit-calendar-event-modal").modal("toggle")
                        showSmallAlert(data.msg);
                        $("#event-calendar").fullCalendar('removeEvents', function (eventObject) {
                            if (eventObject.Id == eventId)
                                return true;
                        });
                    }
                }
            });
        }
    })

    function CancelEvent() {
        let eventId = $("#editing-event-id").val();
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                eventId: eventId,
            },
            url: toUrl("Home/CancelEvent"),
            success: function (data) {
                $("#save-event-modal-btn").removeAttr("disabled");
                stopLoading();
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    $("#event-calendar").fullCalendar('refetchEvents')
                    $("#edit-calendar-event-modal").modal("toggle")
                    $("#repeatingCheckbox-modal").removeAttr("checked")
                }
            }
        });
    }

    function submitEditEventForm() {

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
        let checkbox = $("#repeatingCheckbox-modal")
        let color = $(".event-color-modal.chosen").attr("colorId");
        let repeatingEvent = checkbox.is(":checked")
        let repeatType = checkbox.attr("repeat-type");
        let repeatEveryNumber = checkbox.attr("repeat-every-number");
        let repeatOnDays = checkbox.attr("repeat-on-days");
        let repeatEndType = checkbox.attr("end-type");
        let repeatEndDate = checkbox.attr("end-date");
        let repeatEndAfterNumber = checkbox.attr("end-after-number");
        let repeatSummary = checkbox.attr("summary");

        if (eventTitle.length < 1) {
            showSmallError("Please enter a title for the event");
            return;
        }


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
        startLoading();
        $("#save-event-modal-btn").prop("disabled", "disabled")
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                inputForm: {
                    eventId: eventId,
                    agentId: 0,
                    title: eventTitle,
                    color: color,
                    startDate: startDate,
                    endDate: endDate,
                    fullDay: fullDay,
                    startTime: startTime,
                    endTime: endTime,
                    notes: notes,
                    eventType: eventType,
                    repeatType: repeatType,
                    repeatEveryNumber: repeatEveryNumber,
                    repeatOnDays: repeatOnDays,
                    repeatEndType: repeatEndType,
                    repeatEndDate: repeatEndDate,
                    repeatEndAfterNumber: repeatEndAfterNumber,
                    repeatingEvent: repeatingEvent,
                    repeatSummary: repeatSummary
                }
            },
            url: toUrl("Home/SubmitEventForm"),
            success: function (data) {
                $("#save-event-modal-btn").removeAttr("disabled");
                stopLoading();
                if (!data.success) {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                else {
                    $("#event-calendar").fullCalendar('removeEvents', function (eventObject) {
                        if (eventObject.Id == eventId)
                            return true;
                    });
                    //$.each(data.eventObject, function (index, item) {
                    //    console.log(item)
                    //    let newEventHash = item;
                    //    $('#event-calendar').fullCalendar('addEventSource', [newEventHash]);
                    //});
                    $("#event-calendar").fullCalendar('refetchEvents')

                    $("#edit-calendar-event-modal").modal("toggle")
                    $("#repeatingCheckbox-modal").removeAttr("checked")
                    showSmallAlert(data.msg);
                }
            }
        });
    }

    //Create Event Form

    $(document).on("click", ".event-color", function () {
        $(".event-color.chosen").removeClass("chosen");
        $(this).addClass("chosen");
    })

    //TODO: Click event for repeating checkbox
    $(document).on("click", "#repeatingCheckbox", function () {
        if ($(this).is(":checked")) {
            SetUpRepeatForm("0", "1", null, null, null, null)
            $("#save-repeat-modal-btn").attr("origin", "create")
            $("#close-repeat-modal").removeAttr("editing")
            $("#repeating-event-modal").modal("toggle")
            let start_date = $("#event-start-date").val();
            $("#repeat-start-date").val(start_date);
        }
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
    });

    $("#pto-section").on("change", "#event-type", function () {
        if (!editedEventTitle) {
            let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
            let teamName = $("#select-name-staff option:selected").attr("teamName");
            let eventType = $("#event-type").val();
            $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        }
        switch ($(this).val()) {
            case "PTO (Unplanned)":
            case "Unpaid Time Off (Unplanned)":
            case "LOA":
                $(".event-color.chosen").removeClass("chosen");
                $(".event-color.color-9").addClass("chosen");
                break;
            case "Training":
                $(".event-color.chosen").removeClass("chosen");
                $(".event-color.color-7").addClass("chosen");
                break;
            case "Unpaid Time Off (Planned)":
            case "PTO (Planned)":
                $(".event-color.chosen").removeClass("chosen");
                $(".event-color.color-10").addClass("chosen");
                break;
            case "Other":
                $(".event-color.chosen").removeClass("chosen");
                $(".event-color.color-11").addClass("chosen");
                break;
        }
    })

    $("#pto-section").on("click", "#close-event-form-btn", function () {
        $(".event-form").slideUp("fast");
        $(".event-form input, textarea").val("")
    });

    $("#pto-section").on("click", "#submit-event-form-btn", function () {

        submitEventForm();
    });

    //TODO: Make sure if they selected "number" as end type, they have actually put in a number > 0
    function submitEventForm() {
        let startDate = $("#event-start-date").val();
        let endDate = $("#event-end-date").val();
        let startTime = $("#event-start-time").val();
        let endTime = $("#event-end-time").val();
        let fullDay = $("#fullDayCheckbox:checked").length > 0;
        let notes = $("#event-notes").val();
        let eventType = $("#event-type").val();
        let eventTitle = $("#event-title").val();
        let agentId = $("#select-name-staff").val();
        let checkbox = $("#repeatingCheckbox")
        let repeatingEvent = checkbox.is(":checked")
        let color = $(".event-color.chosen").attr("colorId");
        let repeatType = checkbox.attr("repeat-type");
        let repeatEveryNumber = checkbox.attr("repeat-every-number");
        let repeatOnDays = checkbox.attr("repeat-on-days");
        let repeatEndType = checkbox.attr("end-type");
        let repeatEndDate = checkbox.attr("end-date");
        let repeatEndAfterNumber = checkbox.attr("end-after-number");
        let repeatSummary = checkbox.attr("summary")


        if (eventTitle.length < 1) {
            showSmallError("Please enter a title for the event");
            return;
        }

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
        //TODO: get the right color here...
        //let color = "7";
        //let color = GetColor(eventType)

        if (agentId == "" || agentId == null) {
            showSmallError("Please select a staff member to submit time off.");
            return;
        }
        else {
            $("#submit-event-form-btn").prop("disabled", "disabled");
            startLoading();
            $.ajax({
                dataType: "json",
                type: "post",
                data: {
                    inputForm: {
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
                        eventType: eventType,
                        repeatType: repeatType,
                        repeatEveryNumber: repeatEveryNumber,
                        repeatOnDays: repeatOnDays,
                        repeatEndType: repeatEndType,
                        repeatEndDate: repeatEndDate,
                        repeatEndAfterNumber: repeatEndAfterNumber,
                        repeatingEvent: repeatingEvent,
                        repeatSummary: repeatSummary
                    }

                },
                url: toUrl("Home/SubmitEventForm"),
                success: function (data) {
                    stopLoading();
                    $("#submit-event-form-btn").removeAttr("disabled");
                    if (!data.success) {
                        console.log("error -- " + data.msg);
                        showSmallError(data.msg);
                    }
                    else {
                        //$.each(data.eventObject, function (index, item) {
                        //    console.log(item)
                        //    let newEventHash = item;
                        //    $('#event-calendar').fullCalendar('addEventSource', [newEventHash]);
                        //});
                        $("#event-calendar").fullCalendar('refetchEvents')
                        showSmallAlert(data.msg);
                        $(".event-form").slideUp("fast");
                        $(".event-form input, textarea").val("")
                        $("#repeatingCheckbox").removeAttr("checked")
                        showSmallAlert(data.msg);
                    }
                }
            });
        }
    }

    function GetColor(eventType) {
        switch (eventType) {
            case "Training":
                return "7"; //Teal training
            case "PTO (Unplanned)":
            case "Unpaid Time Off (Unplanned)":
            case "LOA":
                return "9"; //Blue unplanned PTO / LOA
            case "PTO (Planned)":
            case "Unpaid Time Off (Planned)":
                return "10"; //Green Planned PTO
            default:
                return "4"; //Pink
        }
    }

    

    function SetUpRepeatForm(repeatType, repeatEveryNumber, repeatOnDays, repeatEndType, repeatEndDate, repeatEndAfterNumber) {
        if (repeatType != null) {
            $("#repeat-type").val(repeatType);
            $("#repeat-every-weekday-checkboxes-tr").hide();
            $("#repeat-every-number-tr").hide()
            switch (repeatType) {
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
        }
        if (repeatEveryNumber != null) {
            $("#repeat-every-number").val(repeatEveryNumber)
        }
        if (repeatOnDays != null) {
            let repeatDaysList = repeatOnDays.split(",");
            $.each(repeatDaysList, function (index, item) {
                $(".repeat-day-checkbox[name='" + item + "']").prop('checked', true);
            });
        }
        if (repeatEndType != null) {
            $("#repeat-end-" + repeatEndType).prop("checked", true);
            $(".repeat-end-input").attr("disabled", "disabled");
            $("#repeat-end-" + repeatEndType + "-input").removeAttr("disabled");
            $("#repeat-end-" + repeatEndType + "-input").removeAttr("disabled");

        }
        if (repeatEndDate != null)
            $("#repeat-end-date-input").val(repeatEndDate);
        if (repeatEndAfterNumber > 0)
            $("#repeat-end-number-input").val(repeatEndAfterNumber);
        else
            $("#repeat-end-number-input").val("");
    }

    function SetUpCalendar() {
        $('#event-calendar').fullCalendar({
            defaultView: 'agendaDay',
            minTime: "07:00:00",
            maxTime: "21:00:00",
            header: {
                left: 'prev,next today',
                center: 'title',
                right: 'agendaWeek,agendaDay,listWeek'
            },
            height: 965,
            //events: eventList,
            eventSources: [
                {
                    url: toUrl("/Home/GetEvents"),
                    type: 'POST',
                    error: function () {
                        showSmallAlert("There was an error getting the calendar events!");
                    },
                }
            ],
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
                console.log(calEvent)
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
                let repeating = calEvent.repeating;
                let colorId = calEvent.ColorId;
                let repeatedEvent = calEvent.repeatedEvent
                let sameStartEnd = startDate == endDate
                let cancelled = calEvent.Cancelled
                //TODO: Save the repeating event info to the cal event and pull it up here so we can edit it...
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
                $(".event-color-modal").removeClass("chosen");
                if (sameStartEnd)
                    $("#editing-event-same-date").attr("same", "1")
                else
                    $("#editing-event-same-date").attr("same", "0")
                if (cancelled)
                    $("#cancel-event-modal-btn").html("Un-Cancel Event")
                else
                    $("#cancel-event-modal-btn").html("Cancel Event")
                $(".event-color-modal.color-" + colorId).addClass("chosen")
                if (allDay) {
                    $("#event-start-time-modal, #event-end-time-modal").attr("disabled", "disabled")
                    $("#fullDayCheckbox-modal").prop("checked", "checked")
                }
                else {
                    $("#event-start-time-modal, #event-end-time-modal").removeAttr("disabled", "disabled")
                    $("#fullDayCheckbox-modal").removeAttr("checked")
                }
                if (repeating) {
                    $("#repeatingCheckbox-modal").prop("checked", "checked")
                    $("#repeatingCheckbox-modal-edit").show();
                }
                else {
                    $("#repeatingCheckbox-modal").removeAttr("checked")
                    $("#repeatingCheckbox-modal-edit").hide();
                }
                if (repeatedEvent) {
                    $("#event-start-date-modal").val(calEvent.OriginalStartDate)
                    $("#event-end-date-modal").val(calEvent.OriginalEndDate)
                    $("#event-start-date-modal").prop("disabled", "disabled");
                    $("#event-end-date-modal").prop("disabled", "disabled");
                    $("#repeatingCheckbox-modal").prop("checked", "checked")
                    $("#repeatingCheckbox-modal-edit").show();
                }
                else {
                    $("#event-start-date-modal").removeAttr("disabled");
                    $("#event-end-date-modal").removeAttr("disabled");
                }
            }
        })
    }

    function AddPageInfo(data) {
        //TODO: when we break out the reports tab to new JS file, move these 2 lines there
        $("#mow-last-sent-date").html(data.mowDate)
        $("#down-by-last-sent-date").html(data.downByDate)

        //Set up agent dropdown
        $.each(data.agentList, function (index, item) {
            $("#select-name-staff").append("<option teamName = '" + item.TeamName + "' lastName = '" + item.LastName + "' value='" + item.AgentNo + "'> " + item.FirstName + " " + item.LastName + " </option>");
        })
        $("#select-name-staff").chosen({ search_contains: true });

        //Set up calendar
        SetUpCalendar();


    }

    return {
        AddPageInfo: AddPageInfo,
        initialize: initialize
    }
})();