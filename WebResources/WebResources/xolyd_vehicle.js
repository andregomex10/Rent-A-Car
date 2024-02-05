xolydRentACar = window.xolydRentACar || {};
(
    function () {

        var loop = 0;

        this.onLoadForm = function (executionContext) {
            var formContext = executionContext.getFormContext();
            debugger;

            try {
                if (formContext.ui.getFormType() !== 1) {
                    formContext.getControl('new_totalcoast').setDisabled(true);
                }

            }

            catch (e) {
                alert(e.message);
            }
        }

        this.CalculateRoiButton = function (executionContext) {
            formContext = executionContext;
            
            var recordId = formContext.data.entity.getId();
            var Id = recordId.replace(/[{}]/g, '');

            // Parameters
            var parameters = {};
            parameters.TotalCoast = formContext.getAttribute('new_totalcoast').getValue(); // Edm.Decimal
            parameters.InitialCoast = formContext.getAttribute('new_initialcost').getValue(); // Edm.Decimal

            var req = new XMLHttpRequest();
            req.open("POST", Xrm.Utility.getGlobalContext().getClientUrl() + "/api/data/v9.2/new_vehicles(" + Id + ")/Microsoft.Dynamics.CRM.new_VeculoCalcularRoi", true);
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
                        // Return Type: mscrm.new_VeculoCalcularRoiResponse
                        // Output Parameters
                        formContext.data.refresh();
                        var result = result["Result"]; // Edm.String
                    } else {
                        console.log(this.responseText);
                    }
                }
            };
            req.send(JSON.stringify(parameters));
        }

    }).call(xolydRentACar);
