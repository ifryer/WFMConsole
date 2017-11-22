
reportScript = (function () {

    function initialize() {
        initializeTiny();
        stopLoading();
    }

    function initializeTiny(){
        var that = this;
        return tinymce.init({
            plugins: "textcolor paste colorpicker link preview searchreplace autolink image code table lists",
            toolbar: ["undo redo | styleselect | table | bold italic underline | link unlink image | forecolor backcolor | preview | searchreplace | alignleft aligncenter alignright bullist numlist outdent indent | bullist numlist",
                "fontselect fontsizeselect  table | insertCheckbox insertList insertTextbox insertStop insertBreak editInputs uploadImage | code"],
            menubar: false, // "tools table format view insert edit code",
            statusbar: true,
            menu: false,
            height: "calc(100vh - 350px)",
            convert_urls: false,
            relative_urls: false,
            valid_elements: '*[*]',
            browser_spellcheck: true,
            setup: function (ed) {
            },
            selector: `#report-text`
        });
    }

    function ShowSuccessForm()
    {
        $(".message-div, .message-div-back").show();
    };

    $(document).on("click", "#close-message", function () {
        window.close();
    });

    $(document).on("click", "#submit-report-btn", function () {
        let reportType = $(this).attr("report");
        let reportContent = tinymce.activeEditor.getContent({ format: 'raw' });
        startLoading();
        $.ajax({
            dataType: "json",
            type: "post",
            data: {
                reportContent: reportContent.toString(),
                reportType: reportType
            },
            url: toUrl("/Home/SendReport"),
            success: function (data) {
                stopLoading();
                if (data.success)
                {
                    ShowSuccessForm();
                }
                else {
                    console.log("error -- " + data.msg);
                    showSmallError(data.msg);
                }
                //window.close();
            }
        });
    });

    $(document).ready(function () {
        initialize();
    });
})();