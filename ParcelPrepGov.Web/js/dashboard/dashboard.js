'use strict';

window.PPG = window.PPG || {};

PPG.Dashboard = function ($) {

    let _dashboard, _panelExtension = null;
    let _expanded = true;

    const init = function () {
        $('#toggleButton').appendTo($('div.dx-toolbar-before'));
    };

    const onBeforeRender = function (dashboardControl) {
        _dashboard = dashboardControl;
        _panelExtension = new DevExpress.Dashboard.DashboardPanelExtension(dashboardControl);
        dashboardControl.surfaceLeft(_panelExtension.panelWidth);
        dashboardControl.registerExtension(_panelExtension);
        dashboardControl.registerExtension(new SaveAsDashboardExtension(dashboardControl));
        dashboardControl.registerExtension(new DeleteDashboardExtension(dashboardControl));   

        
    };

    const togglePanel = function (e) {
        if (!_dashboard || !_panelExtension) return;
        if (_dashboard.isDesignMode()) {
            _dashboard.switchToViewer();
            _expanded = false;
        }
        if (_expanded) {
            _panelExtension.hidePanelAsync({}).done(function (e) {
                _dashboard.surfaceLeft(e.surfaceLeft);
            });

        } else {
            _panelExtension.showPanelAsync({}).done(function (e) {
                _dashboard.surfaceLeft(e.surfaceLeft);
            });
        }
        _expanded = !_expanded;
    }

    return {
        init: init,
        onBeforeRender: onBeforeRender,
        togglePanel: togglePanel
    }

}

PPG.dashboard = new PPG.Dashboard(jQuery);