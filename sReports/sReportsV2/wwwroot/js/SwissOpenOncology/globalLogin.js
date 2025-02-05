﻿function loginUser(form, e) {
    e.preventDefault();
    e.stopPropagation();
    $(form).validate();
    if ($(form).valid()) {
       //
        let user = {};
        user["Username"] = $("#username").val();
        user["Password"] = $("#loginPartial").find("#password").val();
        callServer({
            type: "POST",
            data: user,
            url: `/ThesaurusGlobal/Login`,
            success: function (data, textStatus, jqXHR) {
                window.location.href = "/ThesaurusGlobal/Index"
            },
            error: function (xhr, ajaxOptions, thrownError) {
                handleResponseError(xhr);
            }
        });
    }
}

function goToSignIn() {
    $("#loginPartial").show();
    $("#registrationPartial").hide();

}
function goToRegister() {
    $("#loginPartial").hide();
    $("#registrationPartial").show();
}

function registerUser(form, e) {
    e.preventDefault();
    e.stopPropagation();
    $(form).validate();
    if ($(form).valid()){
        if ($("#password").val() == $("#confirmPassword").val()) {
            submitUser();
        } else {
            toastr.error("Please confirm your password!");
        }
        
    }
}

function submitUser() {
    let user = getUser();
    callServer({
        type: "POST",
        url: "/ThesaurusGlobal/RegisterUser",
        data: user,
        success: function (data, textStatus, jqXHR) {
            toastr.options = {
                timeOut: 5000
            }
            toastr.options.onHidden = function () {
                goToLogin();
            }
            toastr.success("Registration completed! In order to activate your account, open an email which has just been sent.");
        },
        error: function (xhr, ajaxOptions, thrownError) {
            handleResponseError(xhr);
        }
    });

}

function getUser() {
    let user = {};

    user["FirstName"] = $("#firstName").val();
    user["LastName"] = $("#secondName").val();
    user["Email"] = $("#email").val();
    user["Affiliation"] = $("#affiliation").val();
    user["Country"] = $("#country").val();
    user["Phone"] = $("#phone").val();
    user["Password"] = $("#password").val();

    return user;
}

function getTotalChartData() {
    callServer({
        url: '/ThesaurusGlobal/GetTotalChartData',
        method: 'GET',
        dataType: 'json',
        success: function (response) {
            $('#thesaurusEntriesTotal').html(response.thesaurusTotal);
            $('#documentsTotal').html(response.documentTotal);
            $('#datasetsTotal').html(response.datasetTotal);
        },
        error: function (xhr, textStatus, thrownError) {
            handleResponseError(xhr);
        }
    })
}

$(document).ready(function () {
    getTotalChartData();
});