var widget_packagesLoaded = false;
var widgetRequest;

if (checkJquery) {
    if (window.addEventListener) {
        window.addEventListener('load', widget_checkjQuery)
    } else if (window.attachEvent) {
        window.attachEvent('onload', widget_checkjQuery)
    }
    else if (typeof window.onload !== 'function') {
        window.onload = widget_checkjQuery;
    }
    else {
        var oldHandler = window.onload;
        window.onload = function () {
            if (oldHandler) {
                oldHandler();
            }
            widget_checkjQuery();
        };
    }
    console.log('Checked for jquery');
}
else {
    widget_getPackages();
    console.log('Just got packages');
}

function widget_checkjQuery() {
    if (window.jQuery) {
        widget_getPackages();
    }
    else {
        loadScript('##JQUERY_VERSION##', widget_getPackages);
    }
}

function loadScript(url, callback) {
    var head = document.head;
    var script = document.createElement('script');
    script.type = 'text/javascript';
    script.src = url;

    if (callback && typeof callback === "function") {
        script.addEventListener("load", callback, false);
    }

    head.appendChild(script);
}

function loadCss(url, callback) {
    var head = document.head;
    var link = document.createElement('link');
    link.type = 'text/css';
    link.rel = 'stylesheet';
    link.media = 'all';
    link.href = url;

    if (callback && typeof callback === "function") {
        link.addEventListener("load", callback, false);
    }

    head.appendChild(link);
}

function widget_buildPackageRequest() {
    var widgetArray = [];
    var containerNameArray = [];
    var duplicateArray = [];

    jQuery('div[id^="eddyws_"]').each(function (index, element) {
        var tempObject = { 'ContainerName': jQuery(this).attr('id'), 'Settings': jQuery(this).data() };

        var duplicateIndex = jQuery.inArray(tempObject.ContainerName, containerNameArray);

        if (duplicateIndex > -1) {
            if (jQuery.inArray(duplicateIndex, duplicateArray) === -1)
                duplicateArray.push(duplicateIndex);
        }
        else {
            widgetArray.push(tempObject);
            containerNameArray.push(tempObject.ContainerName);
        }
    });

    if (duplicateArray.length > 0) {
        duplicateArray.sort(function (a, b) { //sort descending
            return a > b ? -1 : b > a ? 1 : 0;
        });

        jQuery.each(duplicateArray, function (index, element) {
            console.warn("WidgetError: Cannot have duplicate widget containers on the same page. WidgetName - " + widgetArray[element].ContainerName);
            widgetArray.splice(element, 1);
        });
    }
    return widgetArray;
}

function widget_getPackages() {
    if (widget_packagesLoaded) return;

    widget_packagesLoaded = true;

    var widgetRequest = getWidgetRequest();

    if (widgetRequest.ContainerList.length === 0) {
        console.warn("WidgetError: No properly configured widgets found on page!");
    }
    else {

        jQuery.ajax({
            method: 'POST',
            contentType: 'application/json;charset=utf-8',
            url: '##SERVICEURL##/WidgetProvider/GetWidgetPackage',
            //dataType: 'text script',
            data: JSON.stringify(widgetRequest)
        })
            .done(function (msg) {
                if (msg.indexOf("WidgetError:") >= 0) {
                    console.warn(msg);
                }
                else {
                    jQuery.globalEval(msg);
                    widget_saveImpression(widget_readCookie('EddyWidgetSession'));
                }
                //console.log(msg);
                console.log("Widgets loaded successfully");
            })
            .fail(function (xhr, status, error) {
                console.warn("WidgetError: " + error);
            });
    }
}

function getWidgetRequest() {

    var localVendorGuid = widget_readCookie('EddyVendorToken');

    if (localVendorGuid == null) {
        localVendorGuid = globalVendorGuid;
    }

    var widgetRequest = {
        ContainerList: widget_buildPackageRequest()
        , VendorToken: localVendorGuid
        , PageUrl: window.location.href
        , ReferrerUrl: document.referrer
        , UserAgent: navigator.userAgent
        , JqueryVersionNumber: jQuery.fn.jquery
        , IPAddress: null
        , CookieTrackId: widget_readCookie('_CampaignTrackID')
    };

    if (widget_readCookie('_IsModalLoad') === null || widget_readCookie('_IsModalLoad') === '') {

        var len = typeof jQuery.modal;

        widgetRequest.IsModalLoad = len == 'function';
        widget_setCookie('_IsModalLoad', len == 'function' ? 'true' : 'false', 1000);
    }
    else {
        widgetRequest.IsModalLoad = widget_readCookie('_IsModalLoad') == 'true';
    }

    return widgetRequest;
}

function widget_saveImpression(widgetRequestGuid) {
    jQuery.ajax({
        method: 'GET',
        //contentType: "application/json;charset=utf-8",
        url: '##SERVICEURL##/WidgetProvider/SaveWidgetImpression',
        data: { widgetSessionGuid: widgetRequestGuid }
    })

        .fail(function (xhr, status, error) {
            console.warn("WidgetError: " + error);
        });
}

function widget_readCookie(name) {
    var nameEQ = name + "=";
    var ca = document.cookie.split(';');
    for (var i = 0; i < ca.length; i++) {
        var c = ca[i];
        while (c.charAt(0) === ' ') c = c.substring(1, c.length);
        if (c.indexOf(nameEQ) == 0) return c.substring(nameEQ.length, c.length);
    }
    return null;
}
function widget_setCookie(name, value, hours) {
    var expires = "";
    if (hours) {
        var date = new Date();
        date.setTime(date.getTime() + (hours * 60 * 60 * 1000));
        expires = "; expires=" + date.toUTCString();
    }
    document.cookie = name + "=" + (value || "") + expires + "; path=/";
}