scheduleTab = (function () {

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

        $("#repeat-end-date-input").datepicker();
        $('#start-end-event-section').datepair();
        $('#start-end-event-section-modal').datepair();
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-start-date').datepicker();
        $('#event-start-date-modal').datepicker();
        $('#event-end-date').datepicker();
        $('#event-end-date-modal').datepicker();
    }

    //Page events

    $("#pto-section").on("change", "#select-name-staff", ChangeStaffName);


    $(".teamInfo, .mouseover-team-info").hover(function () {
        $(".mouseover-team-info").css({ "display": "block" });
    }, function () {
        $(".mouseover-team-info").css({ "display": "none" });
    });


    //Repeating event modal

    $(document).on("change", "#repeat-type", ChangeRepeatType);
    $(document).on("click", "#cancel-event-modal-btn", CancelEvent);
    $(document).on("click", "#save-repeat-modal-btn", SaveRepeatModal);
    $(document).on("change", ".repeat-end", ChangeRepeatEnd);
    $(document).on("click", "#close-repeat-modal", CloseRepeatModal);

    //Edit event modal

    $(document).on("change", "#event-type-modal", ChangeEventTypeModal);
    $(document).on("click", ".event-color-modal", ChangeEventColorModal);
    $(document).on("change", "#event-start-date-modal", ChangeEventStartDateModal);
    $(document).on("change", "#event-end-date-modal", ChangeEventEndDateModal);
    $(document).on("click", "#repeatingCheckbox-modal", ClickRepeatingCheckboxModal);
    $(document).on("change", "#fullDayCheckbox-modal",  ChangeFullDayModal);
    $(document).on("click", "#save-event-modal-btn", SubmitEditEventForm);
    $(document).on("click", "#repeatingEvent-modal-edit", ClickEditRepeatingEvent);
    $(document).on("click", "#delete-event-modal-btn", DeleteEvent);
    $(document).on("click", "#add-event-notification-modal", AddNotificationModal);

    $(document).on("click", ".remove-notification-row", RemoveNotificationRow);

    //Create event modal
    $(document).on("click", "#add-event-notification", AddEventNotification);
    $(document).on("click", ".remove-notification-row", RemoveNotificationRow);
    $(document).on("click", ".event-color", ChangeEventColor);
    $(document).on("click", "#repeatingCheckbox", ClickRepeatingCheckbox);
    $("#pto-section").on("click", "#new-event-btn", ClickNewEventButton);
    $("#pto-section").on("change", "#fullDayCheckbox", ClickFullDay);
    $("#pto-section").on("keyup", "#event-title", ChangeEventTitle);
    $("#pto-section").on("change", "#event-type", ChangeEventType);
    $("#pto-section").on("click", "#close-event-form-btn", CloseEventForm);
    $("#pto-section").on("click", "#submit-event-form-btn", SubmitEventForm);
    $(document).on("click", "#refresh-calendar", RefreshCalendar);
    $(document).on("click", ".remove-invitee-row", RemoveInvitee);

    //Page event functions
    function ChangeStaffName()
    {
        let selectedStaffName = $("#select-name-staff option:selected").text();
        let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
        let teamName = $("#select-name-staff option:selected").attr("teamName");
        let eventType = $("#event-type").val();
        $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        $(".selected-staff-name").html(selectedStaffName);
        $("#team-info-list").hide();
        ReloadTeamInfo();
    }
    function CancelEvent() {
        if (confirm('Are you sure you want to change the cancelled status of this event?')) {
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
                        ReloadTeamInfo();
                        $("#event-calendar").fullCalendar('refetchEvents')
                        $("#edit-calendar-event-modal").modal("toggle")
                        $("#repeatingCheckbox-modal").removeAttr("checked")
                    }
                }
            });
        };
    }
    function ChangeRepeatType() {
        let repeat_type = $(this).val();
        SetUpRepeatForm(repeat_type, null, null, null, null, null);
    }
    function SaveRepeatModal() {
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
                validationText += " Please enter a valid repeat frequency.  " + repeat_every_number + " is not a valid input. \r\n ";
            }
            if (repeat_type == "4") {
                if (repeat_on_days == "") {
                    validationErrors = true;
                    validationText += " Please choose at least one day of the week to repeat the event on. ";
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
            checkbox.attr("summary", repeat_summary);
        }
        else {
            showSmallAlert(validationText)
        }
        //TODO: Add some validation to make sure everything was filled out correctly
    }
    function ChangeRepeatEnd() {
        let end_type = $(".repeat-end:checked").val() // Get the repeat end type (Never, Number, Date)
        if (end_type == "number") {
            $("#repeat-end-date-input").attr("disabled", "disabled");
            $("#repeat-end-number-input").removeAttr("disabled");
        }
        if (end_type == "date") {
            $("#repeat-end-number-input").attr("disabled", "disabled");
            $("#repeat-end-date-input").removeAttr("disabled");
        }
        if (end_type == "never") {
            $("#repeat-end-number-input").attr("disabled", "disabled");
            $("#repeat-end-date-input").attr("disabled", "disabled");
        }
    }
    function CloseRepeatModal() {
        let origin = $("#save-repeat-modal-btn").attr("origin");
        let editing = $(this).attr("editing");
        $("#repeating-event-modal").modal("hide");
        if (editing != "editing") {
            if (origin == "create")
                $("#repeatingCheckbox").removeAttr("checked");
            else
                $("#repeatingCheckbox-modal").removeAttr("checked");
        }
    }


    //========== Edit event form ====================
    function ChangeEventTypeModal() {
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
    }
    function ChangeEventColorModal() {
        $(".event-color-modal.chosen").removeClass("chosen");
        $(this).addClass("chosen");
    }
    function ChangeEventStartDateModal() {
        let startDate = $(this).val();
        if (moment(startDate) > moment($("#event-end-date-modal").val())) {
            $("#event-end-date-modal").val(startDate);
        }
        if ($("#editing-event-same-date").attr("same") == "1") {
            $("#event-end-date-modal").val(startDate);
        }
    }
    function ChangeEventEndDateModal() {
        let endDate = $(this).val();
        if (moment(endDate) < moment($("#event-start-date-modal").val())) {
            $("#event-start-date-modal").val(endDate);
        }
        if ($("#editing-event-same-date").attr("same") == "1") {
            $("#editing-event-same-date").attr("same", "0");
        }
    }
    function ClickRepeatingCheckboxModal() {
        if ($(this).is(":checked")) {
            SetUpRepeatForm("0", "1", null, null, null, null);
            $("#save-repeat-modal-btn").attr("origin", "modal")
            $("#close-repeat-modal").removeAttr("editing")
            $("#repeating-event-modal").modal("toggle")
            let start_date = $("#event-start-date-modal").val();
            $("#repeat-start-date").val(start_date);
        }
    }
    function ChangeFullDayModal() {
        if (this.checked) {
            $("#event-start-time-modal, #event-end-time-modal").attr("disabled", "disabled")
        }
        else {
            $("#event-start-time-modal, #event-end-time-modal").removeAttr("disabled", "disabled")
        }
    }
    function SubmitEditEventForm() {
        //Gather up all the values
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
        let invitees = [];
        let hasInvitees = $(".invitee-area-modal .invitee-row").length > 0;

        //TODO: notifications and invitees
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

        let notifications = []
        let notificationsPresent = $(".notification-area-modal .notification-row").length > 0
        if (notificationsPresent) {
            $.each($(".notification-area-modal .notification-row"), function (index, item) {
                let notificationType = $(item).find(".notification-type").val();
                let notificationTime = $(item).find(".notification-time").val();
                let notificationId = $(item).find(".notification-id").val();
                notifications.push({ Id: notificationId, notificationType: notificationType, notificationTime: notificationTime })
            });
        }

        if (hasInvitees) {
            $.each($(".invitee-area-modal .invitee-row"), function (index, item) {
                let editId = $(item).attr("editId")
                let agentNo = $(item).attr("agentNo");
                let email = $(item).attr("email");
                let firstName = $(item).attr("firstName");
                let lastName = $(item).attr("lastName");
                invitees.push({ Id: editId, firstName: firstName, lastName: lastName,  agentNo: agentNo, email: email });
            });
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
                    repeatSummary: repeatSummary,
                    notifications: notifications,
                    notificationsPresent: notificationsPresent,
                    hasInvitees: hasInvitees,
                    invitees: invitees
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
                    ReloadTeamInfo();
                    $("#event-calendar").fullCalendar('refetchEvents')
                    $("#edit-calendar-event-modal").modal("toggle")
                    $("#repeatingCheckbox-modal").removeAttr("checked")
                    showSmallAlert("Successfully updated event");
                }
            }
        });
    }
    function ClickEditRepeatingEvent() {
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
    }
    function DeleteEvent() {
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
                        ReloadTeamInfo();
                        $("#event-calendar").fullCalendar('removeEvents', function (eventObject) {
                            if (eventObject.Id == eventId)
                                return true;
                        });
                    }
                }
            });
        }
    }
    function AddNotificationModal() {
        $(".notification-area-modal").append(NotificationRow());
        $(".no-notifications-modal").hide();
    }
    function RemoveNotificationRow(){
        $(this).parents(".notification-row").remove();
        if ($(".notification-row").length < 1) {
            $(".no-notifications-modal").show();
        }
    }
    function AddEventNotification() {
        $(".notification-area").append(NotificationRow());
        $(".no-notifications").hide();
    }
    function RemoveNotificationRow() {
        $(this).parents(".notification-row").remove();
        if ($(".notification-row").length < 1) {
            $(".no-notifications").show();
        }
    }
    function ChangeEventColor() {
        $(".event-color.chosen").removeClass("chosen");
        $(this).addClass("chosen");
    }
    
    //============ Create Event Form ================

    function ClickRepeatingCheckbox() {
        if ($(this).is(":checked")) {
            SetUpRepeatForm("0", "1", null, null, null, null)
            $("#save-repeat-modal-btn").attr("origin", "create")
            $("#close-repeat-modal").removeAttr("editing")
            $("#repeating-event-modal").modal("toggle")
            let start_date = $("#event-start-date").val();
            $("#repeat-start-date").val(start_date);
        }
    }
    function ClickNewEventButton() {
        $(".event-form").slideToggle("fast");
        editedEventTitle = false;
        $("#event-type").val("PTO (Unplanned)");
        $('#event-start-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-end-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-start-time').val(moment().format("hh:mma"));
        $('#event-end-time').val(moment().add(1, "hour").format("hh:mma"));
        $(".invitee-area").empty();
        $(".notification-area").empty();
        $(".notification-area").append(NotificationRow());
    }
    function ClickFullDay() {
        if (this.checked) {
            $("#event-start-time, #event-end-time").attr("disabled", "disabled")
            //$("#start-end-event-section").hide();
        }
        else {
            $("#event-start-time, #event-end-time").removeAttr("disabled", "disabled")
            //$("#start-end-event-section").show();
        }
    }
    function ChangeEventTitle() {
        editedEventTitle = true;
    }
    function ChangeEventType() {
        if (!editedEventTitle) {
            let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
            let teamName = $("#select-name-staff option:selected").attr("teamName");
            if (teamName == null)
                teamName = "Team Name"
            let eventType = $("#event-type").val();
            if (selectedStaffLastName == null)
                selectedStaffLastName = "Name"
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
    }

    function CloseEventForm() {
        $(".event-form").slideUp("fast");
        $(".event-form input, textarea").val("");
        $(".invitee-area").empty();
        $(".notification-area").empty();
    }

    function RefreshCalendar() {
        $(this).find(".glyphicon").addClass("spinning")
        ReloadTeamInfo();
        $("#event-calendar").fullCalendar('refetchEvents')
    }

    function RemoveInvitee() {
        $(this).parents(".invitee-row").remove();
    }

    


    //This is the CREATE event method. 
    function SubmitEventForm() {
        let startDate = $("#event-start-date").val();
        let endDate = $("#event-end-date").val();
        let startTime = $("#event-start-time").val();
        let endTime = $("#event-end-time").val();
        let fullDay = $("#fullDayCheckbox:checked").length > 0;
        let notes = $("#event-notes").val();
        let eventType = $("#event-type").val();
        let eventTitle = $("#event-title").val();
        let agentId = $("#select-name-staff").val();
        let checkbox = $("#repeatingCheckbox");
        let repeatingEvent = checkbox.is(":checked");
        let color = $(".event-color.chosen").attr("colorId");
        let repeatType = checkbox.attr("repeat-type");
        let repeatEveryNumber = checkbox.attr("repeat-every-number");
        let repeatOnDays = checkbox.attr("repeat-on-days");
        let repeatEndType = checkbox.attr("end-type");
        let repeatEndDate = checkbox.attr("end-date");
        let repeatEndAfterNumber = checkbox.attr("end-after-number");
        let repeatSummary = checkbox.attr("summary");
        let notifications = [];
        let notificationsPresent = $(".notification-area .notification-row").length > 0;
        let invitees = [];
        let hasInvitees = $(".invitee-area .invitee-row").length > 0;

        if(notificationsPresent)
        {
            $.each($(".notification-area .notification-row"), function (index, item) {
                let notificationType = $(item).find(".notification-type").val();
                let notificationTime = $(item).find(".notification-time").val();
                notifications.push({ notificationType: notificationType, notificationTime: notificationTime });
            });
        }

        if (hasInvitees)
        {
            $.each($(".invitee-area .invitee-row"), function (index, item) {
                let agentNo = $(item).attr("agentNo");
                let email = $(item).attr("email");
                let firstName = $(item).attr("firstName");
                let lastName = $(item).attr("lastName");

                invitees.push({ firstName: firstName, lastName: lastName, agentNo: agentNo, email: email });
            });
        }


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

        //if (agentId == "" || agentId == null) {
        //    showSmallError("Please select a staff member to submit time off.");
        //    return;
        //}
        //else {
            if (agentId == "" || agentId == null) {
                agentId = 0;
            }
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
                        repeatSummary: repeatSummary,
                        notifications: notifications,
                        notificationsPresent: notificationsPresent,
                        hasInvitees: hasInvitees,
                        invitees: invitees
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
                        ReloadTeamInfo();
                        $("#event-calendar").fullCalendar('refetchEvents')
                        showSmallAlert(data.msg);
                        $(".event-form").slideUp("fast");
                        $(".event-form input, textarea").val("")
                        $("#repeatingCheckbox").removeAttr("checked")
                        showSmallAlert(data.msg);
                    }
                }
            });
        //}
    }

    function ReloadTeamInfo()
    {
        let selectedStaff = $("#select-name-staff").val();
        if (selectedStaff != null && selectedStaff != "")
        {
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
                        console.log(data.teamInfo)
                        $("#team-info-name").html(" - " + data.teamInfo.TeamName);
                        $(".mouseover-team-name").html(data.teamInfo.TeamName);
                        $("#team-info-down-pto").html(data.teamInfo.PTO);
                        $("#team-info-down-unpaid").html(data.teamInfo.Unpaid);
                        $("#team-info-down-training").html(data.teamInfo.Training);
                        $("#team-info-down-other").html(data.teamInfo.Other);
                        $("#team-info-down-total").html(data.teamInfo.TotalDown);

                        //-----
                        $("#planned-number").html(data.teamInfo.Planned);
                        $("#planned-details").html(data.teamInfo.HiddenTeamInfo.PlannedString);
                        $("#unplanned-number").html(data.teamInfo.Unplanned);
                        $("#unplanned-details").html(data.teamInfo.HiddenTeamInfo.UnplannedString);
                        $("#training-number").html(data.teamInfo.Training);
                        $("#training-details").html(data.teamInfo.HiddenTeamInfo.TrainingString);
                        $("#unpaid-number").html(data.teamInfo.Unpaid);
                        $("#unpaid-details").html(data.teamInfo.HiddenTeamInfo.UnpaidString);
                        $("#loa-number").html(data.teamInfo.LOA);
                        $("#loa-details").html(data.teamInfo.HiddenTeamInfo.LOAString);
                        $("#other-number").html(data.teamInfo.Other);
                        $("#other-details").html(data.teamInfo.HiddenTeamInfo.OtherString);


                        $("#team-info-list").slideDown("fast");
                        $("#loading-team-info").hide();
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
                    success: function (data) {
                        $("#refresh-calendar").find(".glyphicon").removeClass("spinning");
                    }
                }
            ],
            loading: function (isLoading, view) {
                if (isLoading){
                    startLoading();
                }
                else {
                    stopLoading();
                }
            },
            navLinks: true,
            resources: [
                // resources go here
            ],
            dayClick: function (date, jsEvent, view) {
                let strTime = date.format("h:mma")
                let strTimeEnd = (date.add(1, "hours")).format("h:mma")
                let dateString = date.format("MM/DD/YYYY")
                $(".event-form").show();
                $("#event-start-date, #event-end-date").val(dateString);
                $("#event-start-time").val(strTime);
                $("#event-end-time").val(strTimeEnd);
                $("#create-event-form")[0].scrollIntoView({ behavior: "smooth" });
                $(".notification-area").empty();
                $(".notification-area").append(NotificationRow());
            },
            eventClick: function (calEvent, jsEvent, view) {
                let notifications = calEvent.Notifications
                let invitees = calEvent.Invitees
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
                $(".notification-area-modal").empty();
                if (notifications.length > 0)
                {
                    $.each(notifications, function (index, item) {
                        $(".notification-area-modal").append(EditNotificationRow(item.Id, item.notificationType, item.notificationTime));
                    })
                }
                $(".invitee-area-modal").empty();
                if (invitees.length > 0) {
                    $.each(invitees, function (index, item) {
                        $(".invitee-area-modal").append(InviteeRow(item.Id, item.email, item.agentNo, item.firstName, item.lastName));
                        //TODO: this
                        //$(".notification-area-modal").append(EditNotificationRow(item.Id, item.notificationType, item.notificationTime));
                    })
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

        $("#add-invitee").autocomplete({
            source: function(request, response) {
                var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
                response($.grep(Emails.Emails(), function(value) {
                    return matcher.test(value.name) || matcher.test(value.email);
                }));
            },
            select: function (event, ui) {
                console.log(ui.item)
                AddInvitee(ui.item, false);
                this.value = "";
                return false; // This will prevent the box from filling in with the selected item
            }
        });
        $("#add-invitee-modal").autocomplete({
            source: function (request, response) {
                var matcher = new RegExp($.ui.autocomplete.escapeRegex(request.term), "i");
                response($.grep(Emails.Emails(), function (value) {
                    return matcher.test(value.name) || matcher.test(value.email);
                }));
            },
            select: function (event, ui) {
                AddInvitee(ui.item, true);
                this.value = "";
                return false; // This will prevent the box from filling in with the selected item
            }
        });
    }

    function AddInvitee(invitee, modal)
    {
        let appendArea = $(".invitee-area");
        if (modal)
        {
            appendArea = $(".invitee-area-modal");
        }
        let existingRow = $(appendArea).find(".invitee-row[agentNo='" + invitee.agentNo + "']");
        if (existingRow.length < 1)
        {
            appendArea.append(InviteeRow(0, invitee.email, invitee.agentNo, invitee.FirstName, invitee.LastName));
        }
        
        if (invitee.reportToEmail !== "")
        {
            let existingRowReport = $(appendArea).find(".invitee-row[agentNo='" + invitee.reportTo + "']")
            if (existingRowReport.length < 1)
            {
                appendArea.append(InviteeRow(0, invitee.reportToEmail, invitee.reportTo, invitee.reportToFirstName, invitee.reportToLastName))
                    
            }
            
        }
    }


    function NotificationRow() {
        return `<div class="notification-row">
                    <select style="height:22px;" class="notification-type"> 
                        <option value="email"> Email </option>
                        <option value="popup" selected="selected"> Notification </option>
                    </select>
                    <span>
                        <input type="number" style="height:22px; width: 50px;" class="notification-time" value="10" />
                        <span> minutes </span>
                    </span>
                    <button class="btn btn-xs btn-default remove-notification-row"><span class="glyphicon glyphicon-remove"></span></button>
                </div>`;
    }

    function InviteeRow(Id, email, agentNo, firstName, lastName)
    {
        return `
                <div class="invitee-row" firstName="` + firstName + `" lastName="` + lastName + `" editId="` + Id + `" agentNo="` + agentNo + `" email="` + email + `" title="` + email + `">
                    <span class="invitee-name"> ` + firstName + " " + lastName + ` </span>
                    <button class="btn btn-default btn-xs remove-invitee-row pull-right">
                        <span class="glyphicon small glyphicon-remove"></span>
                    </button>
                </div>`;
    }

    function EditNotificationRow(Id, Type, Time) {
        return `<div class="notification-row">
                    <input style="display:none" class="notification-id" value=` + Id + ` />
                    <select style="height:22px;" class="notification-type"> 
                        <option value="email" ` + (Type == "email" ? `selected="selected"` : ``) + `> Email </option>
                        <option value="popup" ` + (Type == "popup" ? `selected="selected"` : ``) +  `> Notification </option>
                    </select>
                    <span>
                        <input type="number" style="height:22px; width: 50px;" class="notification-time" value="` + Time + `" />
                        <span> minutes </span>
                    </span>
                    <button class="btn btn-xs btn-default remove-notification-row"><span class="glyphicon glyphicon-remove"></span></button>
                </div>`;
    }

    function ParticipantRow(email, name, agentNo)
    {

    }

    return {
        AddPageInfo: AddPageInfo,
        initialize: initialize
    }
})();