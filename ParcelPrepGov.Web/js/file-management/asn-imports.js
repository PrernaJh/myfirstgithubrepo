'use strict';

window.PPG = window.PPG || {};

PPG.ASNImports = function ($) {

    let _asnImportsCard, _grid = null;

    let _subClientName = null;


    const init = function () {
        _asnImportsCard = $('#asnImportsCard');
        _asnImportsCard.hide();
        _grid = $('#asnImportsGrid').dxDataGrid('instance');
    };

    const onSubClientValueChanged = function (e) {
        _subClientName = e.value;        
        _asnImportsCard.fadeIn('slow');
        _grid.refresh();
    };

    const getSelectedSubClient = function () {
        return _subClientName;
    };

    return {
        init: init,
        onSubClientValueChanged: onSubClientValueChanged,
        getSelectedSubClient: getSelectedSubClient
    };

}

PPG.asnImports = new PPG.ASNImports(jQuery);