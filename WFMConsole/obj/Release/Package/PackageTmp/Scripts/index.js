indexScript = (function () {

    function initialize() {
        mowTab.initialize();
        scheduleTab.initialize();
        $.ajax({
            dataType: "json",
            type: "post",
            url: toUrl("/Home/GetPageInfo"),
            success: function (data) {
                if (data.success)
                {
                    Emails.LoadInvitees(data.invitees);
                    scheduleTab.AddPageInfo(data);
                    mowTab.AddPageInfo(data);
                    

                    //Close loading dialog, all done
                    stopLoading();
                }
            }
        });
    }

    //Should redurect to the login screen
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
