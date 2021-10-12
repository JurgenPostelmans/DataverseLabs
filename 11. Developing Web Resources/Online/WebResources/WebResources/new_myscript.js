/// <reference path="new_jquery.js" />

$(document).ready(function () {

    $("#saveButton").click(function () {
        var firstName = $("#textboxFirstName").val();
        var lastName = $("#textboxLastName").val();
        var creditLimit = $("#textboxCreditLimit").val();

        var contact = {};
        contact.firstname = firstName;
        contact.lastname = lastName;
        contact.creditlimit = new Number(creditLimit);
        contact.preferredcontactmethodcode = 2;
        contact["parentcustomerid_account@odata.bind"] = "/accounts(9ED04FA5-1684-E811-A960-000D3AB20765)";

        Xrm.WebApi.createRecord("contact", contact).then(
            function (result) {
                var alertStrings = { confirmButtonLabel: "Ok", text: "Contact created with ID: " + result.id };
                var alertOptions = { height: 120, width: 400 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
             
            },
            function (error) {
                var alertStrings = { confirmButtonLabel: "Ok", text: error.message };
                var alertOptions = { height: 120, width: 400 };
                Xrm.Navigation.openAlertDialog(alertStrings, alertOptions);
            }
        );
    });

    let options = "?$select=contactid,firstname,lastname,creditlimit,birthdate,preferredcontactmethodcode,_parentcustomerid_value&$filter=statecode eq 0&$orderby=fullname";

    Xrm.WebApi.online.retrieveMultipleRecords("contact",options).then(
         function (result) {
             console.log(result.entities);

             let contacts = result.entities;

             $.each(contacts, function (index, contact) {
                 var contactDetails = contact.firstname + " " + contact.lastname +
                     " - " + contact["birthdate@OData.Community.Display.V1.FormattedValue"] +
                     " - " + contact["creditlimit@OData.Community.Display.V1.FormattedValue"] +
                     " - " + contact["preferredcontactmethodcode@OData.Community.Display.V1.FormattedValue"] +
                     " - " + contact["_parentcustomerid_value@OData.Community.Display.V1.FormattedValue"];
                 $("#contactsList").append("<li id='" + contact.contactid + "'>"
                     + contactDetails + " </li>");

             });

         },
         function (error) {
             alert(error.message);
         }
        );
});

