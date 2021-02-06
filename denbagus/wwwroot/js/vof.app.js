var $app;
$app = $app || (function () {
    "use strict";
    var pleaseWaitDiv, confirmDialogDiv, messageBoxDiv, waitPercent, waitInerval;
    waitPercent = 0;
    pleaseWaitDiv = $('<div class="hide"><div class="wait-backdrop fade in"></div><div class="wait wait-active"><div class="wait-progress" style="width: 50%" data-progress-text="Please wait"><div class="wait-activity"></div></div></div></div>');
    confirmDialogDiv = $('<div class="hide"><div class="dialog-backdrop"></div><div class="dialog"><div class="dialog-content"><div class="content-header">Header</div><div class="content-body">Lorem ipsum</div><div class="content-footer"><button class="btn btn-primary">OK</button><button class="btn btn-primary">Cancel</button></div></div></div></div>');
    messageBoxDiv = $('<div class="modal hide" id="messageBox" data-backdrop="static" style="z-index: 2050;top: 30%;"><div class="modal-header"><h3>Dialog</h3></div><div class="modal-body"><p></p></div><div class="modal-footer"><button class="btn btn-primary btn-ok" data-dismiss="modal" aria-hidden="true">OK</button></div></div>');
    pleaseWaitDiv.appendTo(document.body);
    confirmDialogDiv.appendTo(document.body);
    confirmDialogDiv.on("hide",
        function () {
            $(".modal-backdrop").css({ "z-index": "" });
        });
    messageBoxDiv.on("hide",
        function () {
            $(".modal-backdrop").css({ "z-index": "" });
        });
    return {
        showPleaseWait: function () {
            waitPercent = 40;
            $(".wait-progress", pleaseWaitDiv).css({ width: "40%" });
            waitInerval = setInterval(function () {
                waitPercent += 1;
                $(".wait-progress", pleaseWaitDiv).css({ width: waitPercent + "%" });
                if (waitPercent >= 95) {
                    clearInterval(waitInerval);
                }
            }, 50);
            pleaseWaitDiv.show();
        },
        hidePleaseWait: function () {
            clearInterval(waitInerval);
            pleaseWaitDiv.hide();
        },
        showConfirmDialog: function (title, message, okCallback, cancelCallback) {
            var okButton, cancelButton;
            $(".content-header", confirmDialogDiv).text(title);
            $(".content-body", confirmDialogDiv).text(message);
            confirmDialogDiv.show();
            okButton = $(".content-footer button:eq(0)", confirmDialogDiv);
            cancelButton = $(".content-footer button:eq(1)", confirmDialogDiv);
            okButton.unbind("click");
            cancelButton.unbind("click");
            okButton.bind("click",
                function () {
                    confirmDialogDiv.hide();
                    if (typeof okCallback === "function") {
                        okCallback.apply(arguments);
                    }
                });
            cancelButton.bind("click",
                function () {
                    confirmDialogDiv.hide();
                    if (typeof cancelCallback === "function") {
                        cancelCallback.apply(arguments);
                    }
                });
        },
        showMessageBox: function (title, message, isHtml) {
            $(".modal-backdrop").css({ "z-index": "2040" });
            messageBoxDiv.modal();
            if (title !== null && title !== undefined) {
                $("h3", messageBoxDiv).text(title);
            }
            if (message !== null && message !== undefined) {
                $("p", messageBoxDiv).html(message);
            }
        },
        escape: function (text) {
            text = escape(text).replace(/-/g, "%2D").replace(/\./g, "%2E").replace(/\//g, "%2F");;
            return text;
        },
        applicationPath: $("body").data("app-path")
    };
}());

(function () {
    $app.createReportViewer = function (selector, report, parameters) {
        var reportViewer = $(selector);
        reportViewer.telerik_ReportViewer({

            // The URL of the service which will serve reports.
            // The URL corresponds to the name of the controller class (ReportsController).
            // For more information on how to configure the service please check http://www.telerik.com/help/reporting/telerik-reporting-rest-conception.html.
            serviceUrl: $app.applicationPath + "/api/reports",

            // The URL for custom report viewer template. The template can be edited -
            // new functionalities can be added and unneeded ones can be removed.
            // For more information please check http://www.telerik.com/help/reporting/html5-report-viewer-templates.html.
            //templateUrl: '/ReportViewer/templates/telerikReportViewerTemplate-11.2.17.913.html',

            //ReportSource - report description
            reportSource: {
                // The report can be set to a report file name (trdx report definition) 
                // or CLR type name (report class definition).
                report: report,
                parameters: parameters
            },

            // Specifies whether the viewer is in interactive or print preview mode.
            // PRINT_PREVIEW - Displays the paginated report as if it is printed on paper. Interactivity is not enabled.
            // INTERACTIVE - Displays the report in its original width and height without paging. Additionally interactivity is enabled.
            viewMode: telerikReportViewer.ViewModes.INTERACTIVE,

            // Sets the scale mode of the viewer.
            // Three modes exist currently:
            // FIT_PAGE - The whole report will fit on the page (will zoom in or out), regardless of its width and height.
            // FIT_PAGE_WIDTH - The report will be zoomed in or out so that the width of the screen and the width of the report match.
            // SPECIFIC - Uses the scale to zoom in and out the report.
            scaleMode: telerikReportViewer.ScaleModes.SPECIFIC,

            // Zoom in and out the report using the scale
            // 1.0 is equal to 100%, i.e. the original size of the report
            scale: 1.0,
            enableAccessibility: false,

            ready: function () {
                //this.refreshReport();
            },
            parameterEditors: [
                {
                    match: function (parameter) {
                        return Boolean(parameter.availableValues) && !parameter.multivalue;
                    },

                    createEditor: function (placeholder, options) {
                        var dropDownElement = $(placeholder).html('<div></div>'),
                            parameter,
                            valueChangedCallback = options.parameterChanged,
                            dropDownList;

                        function onChange() {
                            var val = dropDownList.value();
                            valueChangedCallback(parameter, val);
                        }

                        return {
                            beginEdit: function (param) {

                                parameter = param;

                                $(dropDownElement).kendoDropDownList({
                                    dataTextField: "name",
                                    dataValueField: "value",
                                    value: parameter.value,
                                    dataSource: parameter.availableValues,
                                    change: onChange,
                                    filter: "contains",
                                    optionLabel: "Please select.."
                                });

                                dropDownList = $(dropDownElement).data("kendoDropDownList");
                            }
                        };
                    }
                }]

        });

        //refresh reportviewer and parameter area width when sidebar collapsed
        $(document).on("collapsed.pushMenu", function (e) {
            reportViewer.animate({ "left": "50px" }, function () {
                var splitter = reportViewer.find(".k-splitter").data("kendoSplitter");
                var size = splitter.size(".k-pane:first");
                splitter.size(".k-pane:first", "100%");
            });
        });

        //refresh reportviewer and parameter area width when sidebar expanded
        $(document).on("expanded.pushMenu", function (e) {
            reportViewer.animate({ "left": "230px" }, function () {
                var splitter = reportViewer.find(".k-splitter").data("kendoSplitter");
                var size = splitter.size(".k-pane:first");
                splitter.size(".k-pane:first", "100%");
            });
        });
        return reportViewer;
    }
})();