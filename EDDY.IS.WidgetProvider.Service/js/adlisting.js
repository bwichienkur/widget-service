
function reloadAdListing(renderingDiv, settings) {
    var widgetRequest = getWidgetRequest();
    var container = widgetRequest.ContainerList.filter(x => x.ContainerName == renderingDiv);
    settings.email = FormsEngine.ProspectEmail;
    container[0].Settings = settings;
    widgetRequest.ContainerList = container;

    $.ajax({
        method: 'POST',
        contentType: 'application/json;charset=utf-8',
        url: '##SERVICEURL##/WidgetProvider/UpdateWidgetPackage',
        data: JSON.stringify(widgetRequest),
        success: function (response) {
            jQuery.globalEval(response);
        },
        error: function (xhr, status, error) {
            console.warn("WidgetError reloadAdListing: " + error);
        }
    });
}

var FormsEngine = FormsEngine || {};
$(FormsEngine).on("WorkflowChangedCompleted", function (pg) {
    var allowed = ['MANAGEDCHOICE', 'THANKYOU', 'NOMATCH'];

    if ($.inArray(FormsEngine.CurrentPage, allowed) >= 0) {
        $.each(widget_adListings, function (i, value) {
            var allowedEvents = $('#' + value).attr('data-loadonevent');
            if (allowedEvents != undefined && allowedEvents != null) {
                if ($.inArray(FormsEngine.CurrentPage, allowedEvents.split(',')) >= 0) {
                    var settings = {};
                    settings.fesessionid = widget_readCookie('FE_SessionId');
                    return reloadAdListing(value, settings);
                }
            }   
        });
    }   
});