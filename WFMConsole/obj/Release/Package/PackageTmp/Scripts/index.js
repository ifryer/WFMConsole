// ================================ Events ================================


indexScript = (function () {
    var editedEventTitle = false;
    function initialize() {
        
        $("#event-start").timepicker({
            'defaultTimeDelta': null,
            'scrollDefault': 'now',
            'minTime': '6:00am',
            'maxTime': '8:30pm',
        });

        $("#event-end").timepicker({
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
                            <tr> 
                                <td> ` + item.LastName + ` </td>
                                <td> ` + item.TeamName + ` </td>
                                <td> ` + item.start + ` </td>
                                <td> ` + item.end  + ` </td>
                                <td> ` + item.title + ` </td>
                                <td> ` + item.EventType + ` </td>
                            </tr>
                        `)
                    })
                    

                    $('#event-calendar').fullCalendar({
                        defaultView: 'agendaDay',
                        header: {
                            left: 'prev,next today',
                            center: 'title',
                            right: 'month,agendaWeek,agendaDay,listWeek'
                        },
                        height: 900,
                        events: data.eventList,
                        resources: [
                            // resources go here
                        ]
                    })
                }
            }
        });
        
        $('#start-end-event-section').datepair();
        $('#event-day').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $('#event-day').datepicker();
    }
    $("").on("click", function () {

    })

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


    $("#pto-section").on("click", "#new-event-btn", function () {
        $(".event-form").slideToggle();
        editedEventTitle = false;
        $('#event-day').val($.datepicker.formatDate('mm/dd/yy', new Date()))
    });

    $("#fullDayCheckbox").on("change", function () {
        if (this.checked)
            $("#start-end-event-section").hide();
        else
            $("#start-end-event-section").show();

    });

    $("#event-title").on("keyup", function () {
        editedEventTitle = true;
    })

    $("#pto-section").on("change", "#event-type", function () {
        if (!editedEventTitle)
        {
            let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
            let teamName = $("#select-name-staff option:selected").attr("teamName");
            let eventType = $("#event-type").val()
            $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        }
    })

    $("#pto-section").on("change", "#select-name-staff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#select-name-staff option:selected").text();
        let selectedStaffLastName = $("#select-name-staff option:selected").attr("lastName");
        let teamName = $("#select-name-staff option:selected").attr("teamName");
        let eventType = $("#event-type").val()
        $("#event-title").val("*" + teamName + " - " + selectedStaffLastName + " - " + eventType);
        $("#pto-info-staff").slideDown();
        $(".selected-staff-name").html(selectedStaffName)
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
                    $("#team-info-name").html(" - " + data.teamInfo.TeamName)
                    $("#team-info-down-pto").html(data.teamInfo.PTO)
                    $("#team-info-down-training").html(data.teamInfo.Training)
                    $("#team-info-down-loa").html(data.teamInfo.LOA)
                    $("#team-info-down-other").html(data.teamInfo.Other)
                    $("#team-info-down-total").html(data.teamInfo.TotalDown)
                    $("#team-info-list").slideDown();
                    $("#loading-team-info").hide();
                    console.log(data)
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

    //PTO Form
    $("#pto-section").on("click", "#close-event-form-btn", function () {
        $(".event-form").slideUp();
        $(".event-form input, textarea").val("")
    });

    $("#pto-section").on("click", "#submit-event-form-btn", function () {
        submitEventForm();
    });

    function submitEventForm() {
        let date = $("#event-day").val();
        let startTime = $("#event-start").val();
        let endTime = $("#event-end").val();
        let fullDay = $("#fullDayCheckbox:checked").length > 0
        let notes = $("#event-notes").val();
        let eventType = $("#event-type").val();
        let eventTitle = $("#event-title").val();
        let id = $("#select-name-staff").val();
        let name = $("#select-name-staff option:selected").html();
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
