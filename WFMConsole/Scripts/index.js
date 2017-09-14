// ================================ Events ================================


indexScript = (function () {

    $(document).on("click", "#newTimeOffBtn", function () {
        $(".timeOffForm").slideDown();

    });

    $(document).on("change", "#selectNameStaff", function () {
        let selectedStaff = $(this).val();
        let selectedStaffName = $("#selectNameStaff option:selected").text();
        $("#ptoInfoStaff").slideDown();
        $(".selectedStaffName").html(selectedStaffName)

        let totalPto = 15;
        let usedPto = 4
        $("#totalPtoStaff").html(totalPto);
        $("#usedPtoStaff").html(usedPto);
        $("#remainingPtoStaff").html(totalPto - usedPto);


        //$("#selectNameStaffForm").val(selectedStaff)
    });

    //PTO Form
    $(document).on("click", "#closeTimeOffFormBtn", function () {
        $(".timeOffForm").slideUp();
        $(".timeOffForm input, textarea").val("")
    });




})();
