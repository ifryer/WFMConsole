mowTab = (function () {

    //MOW & WFO Section
    var newMowRowOptionString = ``;
    var newMowRowString = ``;
    var MonthList = ["", "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
    var mowList;

    function initialize() {
        $(".late-shift-date").datepicker();
        $('.mow-schedule-event-date').val($.datepicker.formatDate('mm/dd/yy', new Date()));
        $(".mow-schedule-event-date").datepicker();
    }

    $(document).on("click", "#mow-tab.unclicked", function () {
        $(this).removeClass("unclicked")
        if ($("#mow-date-" + new moment().format("MMDDYYYY")).length > 0) {
            $(".mow-schedule-list").scrollTop(
                $("#mow-date-" + new moment().format("MMDDYYYY")).offset().top - $(".mow-schedule-list").offset().top + $(".mow-schedule-list").scrollTop()
            );
        }
    })

    $(document).on("click", "#mow-reset-form-btn", function () {
        if ($(".new-mow-row:not(.clean)").length > 0) {
            if (confirm('Are you sure you want to reset this form to the template?')) {
                ResetMowFormToTemplate();
            }
        }
        else {
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
                    }], Date: $(".mow-schedule-event-date").val()
                }, MondayString: currentMonday
            },
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
        if (agentNo != "") {
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

    function SetUpIcmTable(icmSchedule, latestIcmInfo) {
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
        if (latestIcmInfo != null) {
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

    function SetUpLateShiftTable(managerList, lateShift) {
        if (managerList != null) {
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

    function SetUpMowTable(mowSchedule, mowList, mondayMomentString) {
        //Set the date range
        if (mondayMomentString != null && mondayMomentString != "") {
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
        if (mowList != null) {
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

    //HTML Generator function

    function MowTableRow(Task, LastName, FirstName, StartTime, EndTime, RowId, AgentNo) {
        let extraClass = ""
        if (Task == "AD Escalations") {
            extraClass = "escalation-row"
        }
        return `<tr class="mow-display-row ` + extraClass + `"><td class="mow-display-task">` + Task + `</td><td class="mow-display-name" agent_no="` + AgentNo + `" first_name="` + FirstName + `" last_name="` + LastName + `">` + FirstName + " " + LastName + `</td><td start_time="` + StartTime + `" end_time="` + EndTime + `" class="mow-display-time">` + StartTime + " - " + EndTime + `</td><td style="width: 65px;"> <button rowId="` + RowId + `" class="btn btn-xs btn-info edit-mow-row-btn"><span class="glyphicon glyphicon-edit"></span></button> <button rowId="` + RowId + `" class="btn btn-xs btn-danger delete-mow-row-btn"><span class="glyphicon glyphicon-trash"></span></button> </td></tr>`
    }


    function AddPageInfo(data) {
        //Set up MOW stuff
        SetUpMowTable(data.mowSchedule, data.mowList, new moment().format("MM/DD/YYYY"))
        $(".mow-schedule-event-date").val(new moment().format("MM/DD/YYYY"))
        SetUpLateShiftTable(data.managerList, data.lateShift)

        //Set up ICM stuff
        $("#icm-year").val((new Date()).getFullYear())
        SetUpIcmTable(data.icmSchedule, data.latestIcmInfo);

    }

    return {
        AddPageInfo: AddPageInfo,
        initialize: initialize
    }
})();