"use strict";
var angular_js_1;

require(['.scripts/angular.js', 'angular'], function (require) {
    angular_js_1 = require('angular');
});

var AdAggregator = (function () {
    function AdAggregator() {
    }
    AdAggregator.prototype.GetAds = function () {
        var AdsList;
        var request = {
            IP: "",
            TestMode: true,
            DebugMode: true,
            Location: "https://cdpn.io/surfingonmars/fullpage/VwKoywG",
            PathName: "/surfingonmars/fullpage/VwKoywG",
            Hostname: "cdpn.io",
            UserAgent: "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.141 Safari/537.36",
            Referrer: "https://codepen.io/",
            PlacementToken: "e1369fc3-b5e0-4f7a-91bd-be1cd04f6e68",
            MaxAdditionalAds: "",
            Querystring: "editors=1010",
            TrackId: null,
            TrackingSession: null,
            FormFields: {},
            SiteFields: {},
            ExtendedFields: { "AdSessionID": "649e09a1-8193-8ddb-95b0-8f090475f287", "thank_you_experience": null, "device_type": "desktop" },
            CustomCreativeId: "",
            DuplicateForInstitutionList: [],
            Originator: "",
            FormLeadUrl: "",
            WidgetName: "",
            WidgetRequestGuid: ""
        };
        var requestJP = "requestJSON=" + encodeURIComponent(JSON.stringify(request));
        jQuery.ajax({
            async: true,
            type: 'GET',
            dataType: 'jsonp',
            data: requestJP,
            cache: false,
            url: 'http://localhost:3735/api/agrservice/ProcessAdJP',
            success: function (data) {
                AdsList = data;
                console.log(AdsList);
            },
            error: function (request, textStatus, errorThrown) {
            }
        });
    };
    AdAggregator.prototype.renderAngularTemplate = function (data, parentElement, element, id) {
        angular_js_1.angular.module('myApp' + id, ['ngSanitize']).controller('placementController', ['$scope', '$http', function ($scope, $http) {
            try {
                for (var d in data) {
                    $scope[d] = data[d];
                }
                $scope.additionalAds = $.extend(true, [], $scope.ads);
                $scope.ads = $scope.ads.slice(0, $scope.maxResults);
                $scope.additionalAds = $scope.additionalAds.slice($scope.maxResults);
            }
            catch (e) {
            }
            $scope.overrideNotify = function (ad, url) {
                $scope.notify(ad.dynamic.placementToken, ad.position, url);
            };
            $scope.notify = function (placementToken, position, overrideURL) {
                placementToken = placementToken.toUpperCase();
            };
        }])
            .directive('dynamic', function ($compile) {
                return {
                    restrict: 'A',
                    replace: true,
                    link: function (scope, element, attrs) {
                        scope.$watch(attrs.dynamic, function (html) {
                            element[0].innerHTML = html;
                            $compile(element.contents())(scope);
                        });
                    }
                };
            });
        angular_js_1.angular.element(parentElement).ready(function () {
            angular_js_1.angular.bootstrap(element, ['myApp' + id]);
        });
    };
    return AdAggregator;
}());
