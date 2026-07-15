var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
class AdAggregator {
    constructor() {
        this.queryString = window.location.search;
        this.queryStringParams = new URLSearchParams(this.queryString);
    }
    getAds(widgetSettings, containerId) {
        return __awaiter(this, void 0, void 0, function* () {
            var wasPopShow = this.getCookie('WasPopShow');
            var hasExitPop = this.getCookie('HasExitPop');
            console.log(widgetSettings);
            if (hasExitPop != null && hasExitPop != '' && hasExitPop == 'true') {
                if (wasPopShow == null || wasPopShow == 'false') {
                    //@ts-ignore
                    $('#waitingRoom').modal({ fadeDuration: 200, fadeDelay: 0.10, escapeClose: false, clickClose: false, showClose: false });
                    widgetSettings.TrackId = yield this.loadTrackId(widgetSettings);
                    if (this.isFormsEngineExist()) {
                        //@ts-ignore
                        $.when(this.readFieldsFromFormsEngine()).then(function (filterFields) {
                            return __awaiter(this, void 0, void 0, function* () {
                                widgetSettings.FilterFields = filterFields;
                                widgetSettings.Location = location.origin + location.pathname;
                                widgetSettings.PathName = location.pathname;
                                widgetSettings.HostName = location.hostname;
                                widgetSettings.QueryString = location.search;
                                widgetSettings.ReferrerUrl = document.referrer;
                                var adList = yield fetch('##SERVICEURL##/ExitPop/RenderAd', {
                                    method: 'POST',
                                    headers: {
                                        'content-type': 'application/json',
                                    },
                                    body: JSON.stringify(widgetSettings)
                                });
                                //@ts-ignore
                                $('#displayAds').modal({ fadeDuration: 200, fadeDelay: 0.10, escapeClose: true, clickClose: true, showClose: false });
                                const body = yield adList.text();
                                const container = document.getElementById(containerId);
                                const containerChild = document.createElement('div');
                                containerChild.innerHTML = body;
                                container === null || container === void 0 ? void 0 : container.appendChild(containerChild);
                            });
                        });
                    }
                    else {
                        var adList = yield fetch('##SERVICEURL##/ExitPop/RenderAd', {
                            method: 'POST',
                            headers: {
                                'content-type': 'application/json',
                            },
                            body: JSON.stringify(widgetSettings)
                        });
                        //@ts-ignore
                        $('#displayAds').modal({ fadeDuration: 200, fadeDelay: 0.10, escapeClose: false, clickClose: false, showClose: false });
                        const body = yield adList.text();
                        const container = document.getElementById(containerId);
                        const containerChild = document.createElement('div');
                        containerChild.innerHTML = body;
                        container === null || container === void 0 ? void 0 : container.appendChild(containerChild);
                    }
                }
                this.setCookie('WasPopShow', true, 1000);
            }
        });
    }
    getClientIp() {
        return __awaiter(this, void 0, void 0, function* () {
            var clientData = yield fetch('https://api.bigdatacloud.net/data/ip-geolocation?key=616a14b188da414cb34926d4d1b3616f', {
                method: 'GET',
            });
            const client = JSON.parse(yield clientData.text());
            return client.ip;
        });
    }
    readFieldsFromFormsEngine() {
        //@ts-ignore
        var searchFilters = $.Deferred();
        var formFilterFields = {};
        var formFieldNames = ['address', 'address_2', 'age', 'alternate_phone', 'area_of_interest',
            'besttimetocontact', 'campus', 'campuspreference', 'campussoftpreference', 'categories_selections', 'categories_hidden',
            'city', 'collegegraduationyear', 'completed_1600_hours_of_clinical_experience', 'concentration', 'contact_info',
            'country', 'currentschool', 'desired_area_of_study', 'desired_degree_level', 'desired_start_date',
            'dynamiccampussoftpreference', 'education_info', 'email', 'employed_radiology_or_graduated_past_5_years', 'employmentstatus',
            'first_name', 'furtheringeducation', 'gpa', 'havetransfercredits', 'highest_level_of_education_completed',
            'last_name', 'lpn_license', 'major', 'military_affiliation', 'newsletteroptin',
            'personal_info', 'phone', 'postal_code', 'preferred_methods_of_contact', 'prefix',
            'program_of_interest', 'program_questions', 'registered_and_licensure', 'registered_radiology', 'rn_license',
            'specialties_selections', 'specialties_hidden', 'state', 'studentloan', 'subcategories_selections', 'subcategories_hidden', 'subject',
            'teaching_certificate', 'uajobs', 'uaprospectflow', 'undergraduate_degree_education', 'undergraduate_degree_grad',
            'undergraduate_degree_nursing', 'us_citizen', 'year_of_highest_education_completed', 'yearofbirth', 'years_of_teaching_experience',
            'years_of_work_experience', 'schoolsselected', 'LeadSourceUrl', 'FormLeadUrl'];
        //@ts-ignore
        fe_getFormFieldsSessionValues(formFieldNames, function (data) {
            //@ts-ignore
            jQuery.each(data, function (i, val) {
                let value = data[i].Value;
                if (value != null && value != '' && value != 'null')
                    formFilterFields[data[i].Key] = value;
            });
            //@ts-ignore
            formFilterFields.formStep = JSON.stringify(FormsEngine.CurrentStep == undefined ? '1' : FormsEngine.CurrentStep);
            //@ts-ignore
            formFilterFields.workflowStep = FormsEngine.CurrentPage;
            //@ts-ignore
            formFilterFields.renderingStrategy = FormsEngine.RenderingStrategy;
            var check = false;
            //@ts-ignore
            (function (a) { if (/(android|bb\d+|meego).+mobile|avantgo|bada\/|blackberry|blazer|compal|elaine|fennec|hiptop|iemobile|ip(hone|od)|iris|kindle|lge |maemo|midp|mmp|mobile.+firefox|netfront|opera m(ob|in)i|palm( os)?|phone|p(ixi|re)\/|plucker|pocket|psp|series(4|6)0|symbian|treo|up\.(browser|link)|vodafone|wap|windows ce|xda|xiino/i.test(a) || /1207|6310|6590|3gso|4thp|50[1-6]i|770s|802s|a wa|abac|ac(er|oo|s\-)|ai(ko|rn)|al(av|ca|co)|amoi|an(ex|ny|yw)|aptu|ar(ch|go)|as(te|us)|attw|au(di|\-m|r |s )|avan|be(ck|ll|nq)|bi(lb|rd)|bl(ac|az)|br(e|v)w|bumb|bw\-(n|u)|c55\/|capi|ccwa|cdm\-|cell|chtm|cldc|cmd\-|co(mp|nd)|craw|da(it|ll|ng)|dbte|dc\-s|devi|dica|dmob|do(c|p)o|ds(12|\-d)|el(49|ai)|em(l2|ul)|er(ic|k0)|esl8|ez([4-7]0|os|wa|ze)|fetc|fly(\-|_)|g1 u|g560|gene|gf\-5|g\-mo|go(\.w|od)|gr(ad|un)|haie|hcit|hd\-(m|p|t)|hei\-|hi(pt|ta)|hp( i|ip)|hs\-c|ht(c(\-| |_|a|g|p|s|t)|tp)|hu(aw|tc)|i\-(20|go|ma)|i230|iac( |\-|\/)|ibro|idea|ig01|ikom|im1k|inno|ipaq|iris|ja(t|v)a|jbro|jemu|jigs|kddi|keji|kgt( |\/)|klon|kpt |kwc\-|kyo(c|k)|le(no|xi)|lg( g|\/(k|l|u)|50|54|\-[a-w])|libw|lynx|m1\-w|m3ga|m50\/|ma(te|ui|xo)|mc(01|21|ca)|m\-cr|me(rc|ri)|mi(o8|oa|ts)|mmef|mo(01|02|bi|de|do|t(\-| |o|v)|zz)|mt(50|p1|v )|mwbp|mywa|n10[0-2]|n20[2-3]|n30(0|2)|n50(0|2|5)|n7(0(0|1)|10)|ne((c|m)\-|on|tf|wf|wg|wt)|nok(6|i)|nzph|o2im|op(ti|wv)|oran|owg1|p800|pan(a|d|t)|pdxg|pg(13|\-([1-8]|c))|phil|pire|pl(ay|uc)|pn\-2|po(ck|rt|se)|prox|psio|pt\-g|qa\-a|qc(07|12|21|32|60|\-[2-7]|i\-)|qtek|r380|r600|raks|rim9|ro(ve|zo)|s55\/|sa(ge|ma|mm|ms|ny|va)|sc(01|h\-|oo|p\-)|sdk\/|se(c(\-|0|1)|47|mc|nd|ri)|sgh\-|shar|sie(\-|m)|sk\-0|sl(45|id)|sm(al|ar|b3|it|t5)|so(ft|ny)|sp(01|h\-|v\-|v )|sy(01|mb)|t2(18|50)|t6(00|10|18)|ta(gt|lk)|tcl\-|tdg\-|tel(i|m)|tim\-|t\-mo|to(pl|sh)|ts(70|m\-|m3|m5)|tx\-9|up(\.b|g1|si)|utst|v400|v750|veri|vi(rg|te)|vk(40|5[0-3]|\-v)|vm40|voda|vulc|vx(52|53|60|61|70|80|81|83|85|98)|w3c(\-| )|webc|whit|wi(g |nc|nw)|wmlb|wonu|x700|yas\-|your|zeto|zte\-/i.test(a.substr(0, 4)))
                check = true; })(navigator.userAgent || navigator.vendor || window.opera);
            if (check) {
                //@ts-ignore
                formFilterFields.devicetype = 'mobile';
            }
            else {
                //@ts-ignore
                formFilterFields.devicetype = 'desktop';
            }
            searchFilters.resolve(formFilterFields);
        }, true);
        return searchFilters.promise();
    }
    canRender(widgetSettings) {
        return __awaiter(this, void 0, void 0, function* () {
            var trackId = yield this.loadTrackId(widgetSettings);
            var canRender = yield fetch('##SERVICEURL##/ExitPop/CanRender', {
                method: 'POST',
                headers: {
                    'content-type': 'application/json',
                },
                body: JSON.stringify(trackId)
            });
            const hasExitPop = yield canRender.text();
            this.setCookie('HasExitPop', hasExitPop, 1000);
        });
    }
    isFormsEngineExist() {
        //@ts-ignore
        return typeof (fe_global) == 'function' && window.FormsEngine != 'undefined' && typeof (window.FormsEngine.readCookie) == 'function';
    }
    loadTrackId(widgetSettings) {
        return __awaiter(this, void 0, void 0, function* () {
            const urlTrackId = this.queryStringParams.get('trackId');
            const coockieTrackId = this.getCookie('_CampaignTrackID');
            const widgetTrackId = this.getCookie('_WidgetTrackID');
            if (urlTrackId !== null && urlTrackId !== '')
                return urlTrackId;
            else if (coockieTrackId !== null && widgetTrackId !== '')
                return coockieTrackId;
            else if (widgetTrackId !== null && widgetTrackId !== '')
                return widgetTrackId;
            else {
                var widgetTrackid = yield this.getWidgetTrackId(widgetSettings);
                this.setCookie('_WidgetTrackID', widgetTrackid, 1000);
                return widgetTrackid;
            }
        });
    }
    getWidgetTrackId(widgetSettings) {
        return __awaiter(this, void 0, void 0, function* () {
            var adList = yield fetch('##SERVICEURL##/ExitPop/GetWidgetTrackId', {
                method: 'POST',
                headers: {
                    'content-type': 'application/json',
                },
                body: JSON.stringify(widgetSettings)
            });
            const trackId = yield adList.text();
            return trackId;
        });
    }
    setCookie(name, value, days) {
        var expires = '';
        if (days) {
            var date = new Date();
            date.setTime(date.getTime() + (days * 24 * 60 * 60 * 1000));
            expires = '; expires=' + date.toUTCString();
        }
        document.cookie = name + '=' + (value || '') + expires + '; path=/';
    }
    getCookie(coockieName) {
        var name = coockieName + '=';
        var ca = document.cookie.split(';');
        for (var i = 0; i < ca.length; i++) {
            var c = ca[i];
            while (c.charAt(0) == ' ')
                c = c.substring(1, c.length);
            if (c.indexOf(name) == 0)
                return c.substring(name.length, c.length);
        }
        return null;
    }
}
//# sourceMappingURL=AdAggregator.js.map