var xolydRentACar = window.xolydRentACar || {};
(
    function () {

        var reservationRequestConfig =
        {
            "stagename": "Pedido De Reserva",
            "processstageid": "ceb93269-a6a8-4621-80fb-d961d21ea633"
        };

        var bookingConfirmationConfig =
        {
            "stagename": "Confirmação De Reserva",
            "processstageid": "283d8958-27ff-4eb2-b2a9-55a63ea0f46c"
        };

        var vehicleReturnConfig =
        {
            "stagename": "Devolução Do Veículo",
            "processstageid": "cc8905a7-7a91-49b1-bb1f-2d71ecbea4b8"
        };

        var loop = 0;

        this.onLoadForm = function (executionContext) {
            var formContext = executionContext.getFormContext();
            debugger;

            try {
                if (formContext.ui.getFormType() !== 1) {
                    formContext.data.process.addOnPreStageChange(OnPreStageChanged);
                    formContext.data.process.addOnStageChange(OnStageChanged);
                    CheckStageId(executionContext);
                }

            }

            catch (e) {
                alert(e.message);
            }
        }

        CheckStageId = function (executionContext) {
            var formContext = executionContext.getFormContext();
            var stageId = formContext.data.process.getActiveStage().getId();

            if (stageId != reservationRequestConfig.processstageid) {
                formContext.getControl('header_process_new_totaltax').setDisabled(true);
                formContext.getControl('header_process_new_diastotais').setDisabled(true);
                formContext.getControl('header_process_new_caution').setDisabled(true);

                formContext.ui.tabs.get('Geral').sections.get("null_section_4").setVisible(true);
            }
            else {
                formContext.ui.tabs.get('Geral').sections.get("null_section_4").setVisible(false);
            }

        }

        OnPreStageChanged = function (executionContext) {
            if (loop > 0) {
                loop = 0;

                return;
            }
            var formContext = executionContext.getFormContext();
            executionContext.getEventArgs().preventDefault();

            if (executionContext.getEventArgs().getDirection() == "Next") {

                var stageId = formContext.data.process.getActiveStage().getId();
                var recordId = formContext.data.entity.getId();
                var Id = recordId.replace(/[{}]/g, '');

                switch (stageId) {
                    case reservationRequestConfig.processstageid:
                        loop += 1;
                        formContext.data.process.moveNext();
                        break;
                    case bookingConfirmationConfig.processstageid:

                        var entityFormOptions = {};
                        entityFormOptions["entityName"] = "new_movement";
                        entityFormOptions["useQuickCreateForm"] = true;

                        var formParameters = {};

                        formParameters["subject"] = "Aluguer de " + formContext.getAttribute("new_vehicleid").getValue()[0].name.toString();
                        formParameters["new_tipodemanutencao"] = 100000003;
                        formParameters["new_price"] = formContext.getAttribute('new_totaltax').getValue();
                        formParameters["scheduledstart"] = formContext.getAttribute('new_startdate').getValue();
                        formParameters["scheduledend"] = formContext.getAttribute('new_enddate').getValue();

                        formParameters["new_vehicleid"] = formContext.getAttribute("new_vehicleid").getValue()[0].id;
                        formParameters["new_vehicleidname"] = formContext.getAttribute("new_vehicleid").getValue()[0].name;
                        formParameters["new_vehicleidtype"] = "new_vehicle";

                        formParameters["new_contractid"] = recordId;
                        formParameters["new_contractidname"] = formContext.getAttribute("new_name").getValue();
                        formParameters["new_contractidtype"] = "new_contract";

                        Xrm.Navigation.openForm(entityFormOptions, formParameters).then(
                            function (success) {
                                debugger;
                                if (success.savedEntityReference != null) {
                                    loop += 1;
                                    formContext.getAttribute('new_movementid').setValue(success.savedEntityReference);
                                    formContext.data.process.moveNext();

                                }
                            },
                            function (error) {
                                console.log(error);
                            });


                        break;
                    case vehicleReturnConfig.processstageid:
                        var parameters = {};
                        parameters.Vehicle = { "@odata.type": "Microsoft.Dynamics.CRM.new_vehicle", new_vehicleid: formContext.getAttribute("new_vehicleid").getValue()[0].id }; // mscrm.crmbaseentity
                        parameters.Movement = { "@odata.type": "Microsoft.Dynamics.CRM.new_movement", activityid: formContext.getAttribute("new_movementid").getValue()[0].id }; // mscrm.crmbaseentity
                        parameters.TotalCoast = formContext.getAttribute('new_totaltax').getValue(); // Edm.Decimal
                        parameters.Caution = formContext.getAttribute('new_caution').getValue(); // Edm.Decimal
                        parameters.NewMileage = formContext.getAttribute('new_newmileage').getValue(); // Edm.Int32
                        parameters.VehicleState = formContext.getAttribute('new_vehiclestate').getValue(); // Edm.Int32

                        var req = new XMLHttpRequest();
                        req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/new_contracts(" + Id + ")/Microsoft.Dynamics.CRM.new_AluguerAtualizarCarro", true);
                        req.setRequestHeader("OData-MaxVersion", "4.0");
                        req.setRequestHeader("OData-Version", "4.0");
                        req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
                        req.setRequestHeader("Accept", "application/json");
                        req.onreadystatechange = function () {
                            if (this.readyState === 4) {
                                req.onreadystatechange = null;
                                if (this.status === 200 || this.status === 204) {
                                    var result = JSON.parse(this.response);
                                    console.log(result);
                                    // Return Type: mscrm.new_AluguerAtualizarCarroResponse
                                    // Output Parameters
                                    var result = result["Result"]; // Edm.String
                                    loop += 1;
                                    formContext.data.process.moveNext();
                                } else {
                                    console.log(this.responseText);
                                }
                            }
                        };
                        req.send(JSON.stringify(parameters));
                        break;
                    default:
                        loop += 1;
                        formContext.data.process.moveNext();
                }
            }
            else {
                loop += 1;
                formContext.data.process.movePrevious();
            }
        }

        OnStageChanged = function (executionContext) {
            CheckStageId(executionContext);
        }

        this.CheckTaxesButton = function (executionContext) {
            formContext = executionContext;

            var recordId = formContext.data.entity.getId();
            var Id = recordId.replace(/[{}]/g, '');
            var vehicleId = formContext.getAttribute("new_vehicleid").getValue()[0].id.replace(/[{}]/g, '');;
            var startDate = formContext.getAttribute('new_startdate').getValue();
            var endDate = formContext.getAttribute('new_enddate').getValue();

            // Parameters
            var parameters = {};
            parameters.Vehicle = { "@odata.type": "Microsoft.Dynamics.CRM.new_vehicle", new_vehicleid: vehicleId }; // mscrm.new_vehicle
            parameters.StartDate = startDate.toISOString(); // Edm.DateTimeOffset
            parameters.EndDate = endDate.toISOString(); // Edm.DateTimeOffset

            var req = new XMLHttpRequest();
            req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/new_contracts(" + Id + ")/Microsoft.Dynamics.CRM.new_AluguervalidaodeTarifas", true);
            req.setRequestHeader("OData-MaxVersion", "4.0");
            req.setRequestHeader("OData-Version", "4.0");
            req.setRequestHeader("Content-Type", "application/json; charset=utf-8");
            req.setRequestHeader("Accept", "application/json");
            req.onreadystatechange = function () {
                if (this.readyState === 4) {
                    req.onreadystatechange = null;
                    if (this.status === 200 || this.status === 204) {
                        var result = JSON.parse(this.response);
                        console.log(result);

                        formContext.data.refresh();
                        var result = result["Result"]; // Edm.String
                    } else {
                        console.log(this.responseText);
                    }
                }
            };
            req.send(JSON.stringify(parameters));

        }

        this.ShowTaxesButton = function (executionContext) {
            formContext = executionContext;

            var flag = true;

            var stageId = formContext.data.process.getActiveStage().getId();

            if (stageId == configProcessStages.processstageid) {
                flag = true;
            }

            return flag
        }


    }).call(xolydRentACar);
