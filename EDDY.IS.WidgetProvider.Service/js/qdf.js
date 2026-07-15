function checkClearError(element) {
    if (element.value != '') {
        if (element.parentElement.getElementsByClassName('error').length > 0) {
            element.classList.remove('error');
            element.parentElement.getElementsByClassName('error')[0].remove();
        }
    }
}

function qdfBindChange(renderingDiv, trackid, triggerControl, controlsToUpdate, ignoreTrackId) {

    var controls = controlsToUpdate.split(',');
    var currentData = new Object();
    var fieldsToUpdate = new Object();
    var selectedValues = new Object();

    checkClearError(triggerControl);

    if (ignoreTrackId == null)
        ignoreTrackId = false;

    for (i = 0; i < controls.length; i++) {
        var element = document.getElementById(renderingDiv + '_' + controls[i]);
        var dataNeeded = element.dataset.dependenton.split(',');

        fieldsToUpdate[controls[i]] = element.dataset.dependenton;
        selectedValues[controls[i]] = element.value;

        for (j = 0; j < dataNeeded.length; j++) {
            var dataElement = document.getElementById(renderingDiv + '_' + dataNeeded[j]);

            if (dataElement.value != '') {
                currentData[dataNeeded[j]] = getQDFValue(dataElement);
            }
        }
    }

    var qdfDataRequests = {
        TrackId: trackid
        , CurrentData: currentData
        , FieldsToUpdate: fieldsToUpdate
        , IgnoreTrackId: ignoreTrackId
    };

    var request = jQuery.ajax({
        method: 'POST',
        contentType: 'application/json;charset=utf-8',
        url: '##SERVICEURL##/QDF/RetrieveQDFData',
        data: JSON.stringify(qdfDataRequests)
    });

    request.done(function (msg) { processBindResponse(msg, controls, selectedValues, renderingDiv) });
}

function processBindResponse(msg, controls, selectedValues, renderingDiv) {
    if (msg != null && msg.returnData != null) {
        for (ctlIndex = 0; ctlIndex < controls.length; ctlIndex++) {
            if (controls[ctlIndex] == 'Desired_Degree_Level')
                UpdateSelectOptions(document.getElementById(renderingDiv + '_' + controls[ctlIndex]), selectedValues[controls[ctlIndex]], msg.returnData.ProgramLevels);
            else if (controls[ctlIndex] == 'Categories')
                UpdateSelectOptions(document.getElementById(renderingDiv + '_' + controls[ctlIndex]), selectedValues[controls[ctlIndex]], msg.returnData.Categories);
            else if (controls[ctlIndex] == 'SubCategories')
                UpdateSelectOptions(document.getElementById(renderingDiv + '_' + controls[ctlIndex]), selectedValues[controls[ctlIndex]], msg.returnData.SubCategories);
            else if (controls[ctlIndex] == 'Specialties')
                UpdateSelectOptions(document.getElementById(renderingDiv + '_' + controls[ctlIndex]), selectedValues[controls[ctlIndex]], msg.returnData.Specialties);

        }
    }
    console.log(JSON.stringify(msg));
}

function UpdateSelectOptions(selectControl, currentSelectedValue, newOptions) {
    for (remIndex = selectControl.length - 1; remIndex > 0; remIndex--) {
        selectControl.remove(remIndex);
    }

    for (optIndex = 0; optIndex < newOptions.length; optIndex++) {
        var option = document.createElement('option');
        option.value = newOptions[optIndex].id;
        option.text = newOptions[optIndex].name;

        if (currentSelectedValue == option.value) option.selected = true;
        selectControl.add(option);
    }
}

function submitQdf(targetURL, trackid, renderingDiv, formControls, additionalFields, openToNewWindow) {
    var desitinationURL = targetURL + '?trackid=' + trackid + '&WidgetName=' + renderingDiv + '&WidgetRequestGuid=' + widget_readCookie('EddyWidgetSession');
    var controlArray = formControls.split(',');
    var fieldsArray = additionalFields.split(',');
    var hasErrors = false;

    for (urlIndex = 0; urlIndex < fieldsArray.length; urlIndex++) {
        var field = document.getElementById(renderingDiv + '_' + fieldsArray[urlIndex]);
        if (field)
            desitinationURL += '&' + fieldsArray[urlIndex] + '=' + encodeURIComponent(field.value);
    }

    for (urlIndex = 0; urlIndex < controlArray.length; urlIndex++) {
        var control = document.getElementById(renderingDiv + '_' + controlArray[urlIndex]);
        var value = control.value;
        var required = control.dataset.isrequired;

        if (value != '') {
            desitinationURL += '&' + controlArray[urlIndex] + '=' + value;
        }
        else if (required == 'yes') {
            control.classList.add('error');
            hasErrors = true;

            if (control.parentElement.getElementsByClassName('error').length == 1) {
                control.parentElement.appendChild(createErrorDiv(control));
            }
        }
    }

    if (!hasErrors) {
        if (openToNewWindow == "true") {
            window.open(desitinationURL, '_blank');
        }
        else {
            window.location.href = desitinationURL;
        }
    }
}
 
function submitQdfAdStack(renderingDiv, formControls, additionalFields) {
    var settings = {}, hasErrors = false;
    var controlArray = formControls.split(',');
    var fieldsArray = additionalFields.split(',');

    for (urlIndex = 0; urlIndex < fieldsArray.length; urlIndex++) {
        var field = document.getElementById(renderingDiv + '_' + fieldsArray[urlIndex]);
        if (field)
            settings[fieldsArray[urlIndex].toLowerCase()] = encodeURIComponent(field.value);
    }

    for (urlIndex = 0; urlIndex < controlArray.length; urlIndex++) {
        var control = document.getElementById(renderingDiv + '_' + controlArray[urlIndex]);
        var value = getQDFValue(control);
        var required = control.dataset.isrequired;

        if (value != '') {
            settings[controlArray[urlIndex].toLowerCase()] = value;
        }
        else if (required == 'yes') {
            control.classList.add('error');
            hasErrors = true;
            if (control.parentElement.getElementsByClassName('error').length == 1) {
                control.parentElement.appendChild(createErrorDiv(control));
            }
        }
    }

    if (!hasErrors) {
        EddyAdListingApi.reload(settings);
    }
}

function getQDFValue(control) {
    if (control.type == 'select-multiple') {
        var options = control && control.options, values = '';
        for (var i = 0; i < control.options.length; i++) {
            if (options[i].selected) {
                values += options[i].value + ",";
            }
        }
        return values.slice(0, -1);
    }

    return control.value;
}


function checkYear(control) {
    checkClearError(control);
    control.value = control.value.trim();
    if (!control.value || control.value == "")
        return true;
    if (!/^\d{4}$/.test(control.value)) {
        showControlAlert(control, "Please enter a 4 digit number.");
        return false;
    }
    var inputYear = control.value;
    var currentYear = new Date().getFullYear();
    if (inputYear > currentYear) {
        showControlAlert(control, "Please enter a year below " + (currentYear + 1) + ".");
        return false;
    }
    else if (inputYear < currentYear - 100) {
        showControlAlert(control, "Please enter a year greater than " + (currentYear - 100) + ".");
        return false;
    }
    return true;
}

function showControlAlert(control, message) {
    alert(message);
    control.value = "";
    control.focus();
}

function createErrorDiv(control) {
    var errorLabel = document.createElement('label');
    errorLabel.setAttribute('for', control.name);
    errorLabel.classList.add('error');
    errorLabel.innerHTML = 'This field is required.';
    return errorLabel;
}