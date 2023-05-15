'use strict';

window.PPG = window.PPG || {};

PPG.ajaxer = (function ($) {
    const get = function (url) {
        let options = {
            url: url,
            type: 'GET'
        };

        return $.ajax(options);
    }

    const post = function (url, data) {

        let options = {
            url: url,
            type: 'POST',
            dataType: 'json',
            cache: false,
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data)
        };
        
        return $.ajax(options);
    }

    const postFormData = function (url, data) {

        let options = {
            type: "POST",
            url: url,
            cache: false,
            processData: false,
            contentType: false,
            data: data
        };

        return $.ajax(options);
    }

    const deleteJsonData = function (url, data) {

        let options = {
            url: url,
            type: 'DELETE',
            dataType: 'json',
            cache: false,
            contentType: 'application/json;charset=utf-8',
            data: JSON.stringify(data)
        };

        return $.ajax(options);
    }

    return {
        get: get,
        post: post,
        postFormData: postFormData,
        deleteJsonData: deleteJsonData
    }
}(jQuery));
