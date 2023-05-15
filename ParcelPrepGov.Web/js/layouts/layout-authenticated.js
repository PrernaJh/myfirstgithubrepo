"use strict";

window.PPG = window.PPG || {};

PPG.LayoutAuthenticated = function ($) {

    const DRAWER_OPENED_KEY = "EF32B9C5F742";

    const breakpoints = {
        xSmallMedia: window.matchMedia("(max-width: 599.99px)"),
        smallMedia: window.matchMedia("(min-width: 600px) and (max-width: 959.99px)"),
        mediumMedia: window.matchMedia("(min-width: 960px) and (max-width: 1279.99px)"),
        largeMedia: window.matchMedia("(min-width: 1280px)")
    };

    function getDrawer() {
        return $("#layout-drawer").dxDrawer("instance");
    }

    function saveDrawerOpened() {
        sessionStorage.setItem(DRAWER_OPENED_KEY, getDrawer().option("opened"));
    }

    function updateDrawer() {
        let isXSmall = breakpoints.xSmallMedia.matches,
            isLarge = breakpoints.largeMedia.matches;

        let drawer = getDrawer();

        drawer.option({
            openedStateMode: isLarge ? "shrink" : "overlap",
            revealMode: isXSmall ? "slide" : "expand",
            minSize: isXSmall ? 0 : 60,
            shading: !isLarge,
        });

        //let actualOpened = drawer.option("opened");
        //if (!actualOpened) {
        //    let $items = $("#layout-treeview").dxTreeView("instance");
        //    $items.collapseAll();
        //}

    }

    function updateToolbar() {
        const isXSmall = breakpoints.xSmallMedia.matches;

        if (isXSmall) {
            $('#toolbar').find('.dx-toolbar-button.dx-toolbar-menu-container').show();
            $('#LargeMediaToolbarOptions').hide();
        } else {
            $('#toolbar').find('.dx-toolbar-button.dx-toolbar-menu-container').hide();
            $('#LargeMediaToolbarOptions').show();
        }
    }

    function navigate(url, delay) {
        if (url)
            setTimeout(function () { location.href = url }, delay);
    }

    function navigateNewWindow(url, delay) {
        if (url)
            window.open(url, "_blank");
    }

    function signOut() {
        window.location.href = '/account/signout';
    }

    function openProfile() {
        window.location.href = '/account/profile';
    }

    const restoreDrawerOpened = function () {
        let isLarge = breakpoints.largeMedia.matches;
        if (!isLarge)
            return false;

        let state = sessionStorage.getItem(DRAWER_OPENED_KEY);
        if (state === null)
            return isLarge;

        return state === "true";
    }

    const onMenuButtonClick = function () {
        getDrawer().toggle();
        saveDrawerOpened();
    }

    const onTreeViewItemClick = function (e) {
        let drawer = getDrawer();
        let savedOpened = restoreDrawerOpened();
        let actualOpened = drawer.option("opened");
        let $treeView = $("#layout-treeview").dxTreeView("instance");
        let allNodes = $("#layout-treeview").dxTreeView("getNodes");
        $.each(allNodes, function (_, node) {
            if (e.itemData !== node.itemData && !node.selected)
                $treeView.collapseItem(node.itemData);
        });
        if (!actualOpened) {
            drawer.show();
        } else {
            let willHide = !savedOpened || !breakpoints.largeMedia.matches;

            let willNavigate = !e.itemData.selected;
            // don't close if node doesn't have a path to navigate to
            // only close when navigating
            if (willHide && ('path' in e.itemData || 'newWindow' in e.itemData))
                drawer.hide();

            if (willNavigate)
                if ('newWindow' in e.itemData)
                    navigateNewWindow(e.itemData.newWindow);
                else
                    navigate(e.itemData.path, willHide ? 400 : 0);
        }
    }

    const onSelectionChanged = function (e) {

    }

    const onSignOut = function () {
        signOut();
    }

    const onHeaderMenuClick = function (e) {
        switch (e.itemData) {
            case "Sign out":
                signOut();
                break;
            case "Profile":
                openProfile();
                break;
        }
    }

    const plantSelector_onSelectionChanged = function (e) {
        new PPG.Ajaxing().start();

        const ajax = PPG.ajaxer.post('/account/changesite', e.item)

        ajax.done(function (response) {
            if (response.success) {
                location.reload();
            } else {
                PPG.toastr.error(response.message);
            }
        });
        ajax.fail(function (xhr, status) {
            PPG.toastr.error();
        });
        ajax.always(function () { });
    }

    const init = function () {
        $("#layout-drawer-scrollview").dxScrollView({ direction: "vertical" });

        $.each(breakpoints, function (_, size) {
            size.addListener(function (e) {
                if (e.matches) {
                    updateDrawer();
                    updateToolbar();
                }
            });
        });

        updateDrawer();
        updateToolbar();

        $('.layout-body').removeClass('layout-body-hidden');

    }

    return {
        restoreDrawerOpened: restoreDrawerOpened,
        onMenuButtonClick: onMenuButtonClick,
        onTreeViewItemClick: onTreeViewItemClick,
        onSelectionChanged: onSelectionChanged,
        onSignOut: onSignOut,
        onHeaderMenuClick: onHeaderMenuClick,
        plantSelector_onSelectionChanged: plantSelector_onSelectionChanged,
        init: init
    };
}

PPG.layoutAuthenticated = new PPG.LayoutAuthenticated(jQuery);