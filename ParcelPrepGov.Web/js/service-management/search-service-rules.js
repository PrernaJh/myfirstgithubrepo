'use strict';

window.PPG = window.PPG || {};

PPG.SearchServiceRules = function ($) {

    let _ajaxing = null;

    const init = function () {
        $('#searchServiceRulesCard').ppgcard();
        _ajaxing = new PPG.Ajaxing('searchServiceRulesCard');
    };


    const searchButton_onSubmit = function (e) {

        if (!e.validationGroup.validate().isValid) {
            PPG.toastr.warning('Form is not valid.');
            return;
        }

        const customerName = $('input[name="CustomerName"]').val();

        let f = getFormInstance();

        var formData = new FormData(f[0]);

        //for (var pair of formData.entries())
        //    console.log(pair[0] + ', ' + pair[1]);

        formData.append('customerName', customerName);

        _ajaxing.start();

        var options = {
            type: 'POST',
            url: '/ServiceManagement/ServiceRuleManagerSearch',
            processData: false,
            contentType: false,
            data: formData
        };

        var ajax = $.ajax(options);

        ajax.done(function (response) {
            if (response.success) {
                displaySearchResults(response.data);
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error('Service rule manager search failed.');
        });
        ajax.always(function () {
            _ajaxing.stop();
        });

    }

    function displaySearchResults(data) {

        let popupOptions = {
            width: 350,
            height: 250,
            contentTemplate: function () {
                return $("<div />").append(
                    $("<p>" + data.ShippingCarrier + "</p>"),
                    $("<p>" + data.ShippingMethod + "</p>"),
                    $("<p>" + data.ServiceLevel + "</p>")
                );
            },
            showTitle: true,
            title: `${data.Title}`,
            visible: false,
            dragEnabled: true,
            closeOnOutsideClick: false
        };

        const searchResultsPopup = $("#search-result-popup").dxPopup(popupOptions).dxPopup("instance");

        searchResultsPopup.show();
    }

    function getFormInstance() {
        return $("#searchForm");
    }

    return {
        init: init,
        searchButton_onSubmit: searchButton_onSubmit
    };
}


PPG.searchServiceRules = new PPG.SearchServiceRules(jQuery);
