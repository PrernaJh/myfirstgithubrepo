'use strict';

window.PPG = window.PPG || {};

PPG.Ajaxing = function (targetSelector) {

    var ajaxingPanel = document.createElement('div');
    ajaxingPanel.setAttribute('id', `${targetSelector}-ajaxing-panel`);
    document.getElementById('ajaxing-panels').appendChild(ajaxingPanel);

    const loadingPanel = $(`#${ajaxingPanel.getAttribute('id')}`).dxLoadPanel({
        shadingColor: 'rgba(0,0,0,0.4)',
        position: { of: `#${targetSelector}`},
        visible: false,
        showIndicator: true,
        showPane: true,
        shading: true,
        closeOnOutsideClick: false,
        onShown: function () {},
        onHidden: function () {}
    }).dxLoadPanel('instance');

    const start = function () {
        loadingPanel.show();
    }

    const stop = function () {
        loadingPanel.hide();
    }

    return {
        start: start,
        stop: stop
    };

}
