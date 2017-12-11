var Emails = (function() {

    var invitees;
    var emails;

    function getInvitee(email)
    {

    }

    function suggestionEngine(term, response) {

        // an array that will be populated with substring matches
        var matches = [];

        // regex used to determine if a string contains the term
        var substrRegex = new RegExp(term, 'i');

        // iterate through the pool of strings and for any string that
        // contains the substring `q`, add it to the `matches` array
        $.each(names, function(i, str) {
            if (substrRegex.test(str)) {
                matches.push(str);
            }
        });
        response(matches);
    };

    function loadInvitees(invitees) {
        console.log(invitees)
        emails = $.map(invitees, function(invitee) { return {FirstName: invitee.FirstName, LastName: invitee.LastName, label: invitee.Email, value: invitee.Email, email: invitee.Email, name: invitee.FullName, agentNo: invitee.AgentNo, reportTo: invitee.ReportTo, reportToName: invitee.ReportToName, reportToFirstName: invitee.ReportToFirstName, reportToLastName: invitee.ReportToLastName, reportToEmail: invitee.ReportToEmail}; });
        console.log(emails)
        //initialized.resolve();
    }

    //function getInvitees() {
    //    return Utilities.GetJsonData('Home/GetInvitees', null, loadInvitees, "Getting Emails");
    //}

    return {
        GetInvitee: getInvitee,
        Emails: function() { return emails; },
        SuggestionEngine: suggestionEngine,
        LoadInvitees: loadInvitees
    }

}());