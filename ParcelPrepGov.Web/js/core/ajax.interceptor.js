$(function (n, $) {
    $.ajaxSetup({
        beforeSend: function (xhr) {
            var securityToken = $('input[name="__RequestVerificationToken"]').val();
            xhr.setRequestHeader('RequestVerificationToken', securityToken);
            xhr.withCredentials = true; 
            //xhr.setRequestHeader('Access-Control-Allow-Origin', '*');
            //xhr.setRequestHeader('X-Requested-With', 'XMLHttpRequest');
        },
        error: function (xhr, exception) {
            if (xhr.status === 0 | xhr.status == 401) {
                // n.toastr.error('Not connected.\n Verify Network');
                window.location.href = "/Account/SignOut";
            } else if (xhr.status === 400) {
                n.toastr.error('Bad Request [400]');
            } else if (xhr.status === 403) {
                n.toastr.error('Access Denied/Forbidden [403]');               
            } else if (xhr.status === 404) {
                n.toastr.error('Requested page not found [404]');
            } else if (xhr.status === 500) {
                n.toastr.error('Internal Server Error [500]');
            } else if (exception === 'parsererror') {
                n.toastr.error('Requested JSON parse failed');
            } else if (exception === 'timeout') {
                n.toastr.error('Time out error');
            } else if (exception === 'abort') {
                n.toastr.error('Ajax request aborted');
            } else {
                n.toastr.error('Uncaught Error.\n' + xhr.responseText);
            }
        },
        cache: false
    });
}(window.PPG = window.PPG || {}, jQuery));