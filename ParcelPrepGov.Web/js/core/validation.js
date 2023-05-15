'use strict';


window.PPG = window.PPG || {};

PPG.validation = (function () {

    const isContainerOrTrackingValid = function (barcode) {
        let isValid = true;
        let message = '';

        if (!barcode) {
            message = 'Search cannot be empty.';
            isValid = false;
        }
        else if (barcode.indexOf(',') > -1) {
            message = 'Search cannot contain commas.';
            isValid = false;
        }
        else if (barcode.length > 100) {
            message = 'Search cannot exceed 100 characters.'
            isValid = false;
        }

        return {
            'IsValid': isValid,
            'Message': message
        };
    }

    const isContainerIdValid = function (containerId) {
        // could also have validation feature make a ajax request to validate a container, but if a container is not found it is not the end of the world
        let isValid = true;
        let message = '';

        if (!containerId) {
            message = 'No Container Id has been entered.';
            isValid = false;
        }
        else if (containerId.indexOf(',') > -1) {
            message = 'Container Id cannot contain commas.';
            isValid = false;
        }
        else if (containerId.length > 50) {
            message = 'Container Id cannot exceed 50 characters.';
            isValid = false;
        }

        return {
            'IsValid': isValid,
            'Message': message
        };
    }

    const isContainerTrackingValid = function (trackingNumber) {
        let isValid = true;
        let message = '';

        if (!trackingNumber) {
            message = 'No tracking has been entered.';
            isValid = false;
        }
        else if (trackingNumber.indexOf(',') > -1) {
            message = 'Tracking number cannot contain commas.';
            isValid = false;
        }
        else if (trackingNumber.length > 100) {
            message = 'Tracking number cannot exceed 100 characters.'
            isValid = false;
        }

        return {
            'IsValid': isValid,
            'Message': message
        };
    }

    return {        
        isContainerIdValid: isContainerIdValid,
        isContainerTrackingValid: isContainerTrackingValid,
        isContainerOrTrackingValid: isContainerOrTrackingValid
    };
}());
