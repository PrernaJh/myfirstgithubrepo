'use strict';

window.PPG = window.PPG || {};

PPG.toastr = (function () {

    let options = {
        displayTime: 8000,
        closeOnClick: true,
        shading: false
    };

    const toast = function (message, overrideDefaultOptions) {
        let config = Object.assign(options, overrideDefaultOptions);
        config = Object.assign(options, { message: message });
        DevExpress.ui.notify(config);
        log('toast: ' + message);
    };

    const error = function (message, overrideDefaultOptions) {
        let config = Object.assign(options, overrideDefaultOptions);
        config = Object.assign(config, { message: message, type: 'error' });
        DevExpress.ui.notify(config);
        log('error: ' + message);
    };

    const info = function (message, overrideDefaultOptions) {
        let config = Object.assign(options, overrideDefaultOptions);
        config = Object.assign(config, { message: message, type: 'info' });
        DevExpress.ui.notify(config);
        log('info: ' + message);
    };

    const success = function (message, overrideDefaultOptions) {
        let config = Object.assign(options, overrideDefaultOptions);
        config = Object.assign(config, { message: message, type: 'success' });
        DevExpress.ui.notify(config);
        log('success: ' + message);
    };

    const warning = function (message, overrideDefaultOptions) {
        let config = Object.assign(options, overrideDefaultOptions);
        config = Object.assign(config, { message: message, type: 'warning' });
        DevExpress.ui.notify(config);
        log('Warning: ' + message);
    };

    const tempData = function (tempData) {
        if (!tempData) return;
        const data = tempData.split(',');
        if (!data[0])
            return;

        const message = decodeHtml(data[0].trim());

        let type = 'toast';
        if (data[1]) {
            type = data[1].trim().toLowerCase();
        }
        switch (type) {
            case 'toast':
                toast(message);
                log('Toast: ' + message);
                break;
            case 'error':
                error(message);
                log('Error: ' + message);
                break;
            case 'info':
                info(message);
                log('Info: ' + message);
                break;
            case 'success':
                success(message);
                log('Success: ' + message);
                break;
            case 'warning':
                warning(message);
                log('Warning: ' + message);
                break;
            default:
                return;
        }

        tempData = null;
    }

    // IE and google chrome workaround
    // http://code.google.com/p/chromium/issues/detail?id=48662
    const log = function () {
        var console = window.console;
        !!console && console.log && console.log.apply && console.log.apply(console, arguments);
    }

    function decodeHtml(html) {
        var txt = document.createElement("textarea");
        txt.innerHTML = html;
        return txt.value;
    }

    return {
        toast: toast,
        error: error,
        info: info,
        success: success,
        warning: warning,
        tempData: tempData,
        log: log
    };

}());