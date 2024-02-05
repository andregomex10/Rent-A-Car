using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections;


namespace RentACar.Workflows
{
    public class UpdateVehicle : CodeActivity
    {
        #region Parameters
        
        [Input("RecordId")]
        [RequiredArgument]
        [ReferenceTarget("new_contract")]
        public InArgument<EntityReference> RecordId { get; set; }

        [Input("Vehicle")]
        [ReferenceTarget("new_vehicle")]
        public InArgument<EntityReference> Vehicle { get; set; }

        [Input("Movement")]
        [ReferenceTarget("new_movement")]
        public InArgument<EntityReference> Movement { get; set; }

        [Input("TotalCoast")]
        public InArgument<Money> TotalCoast { get; set; }

        [Input("Caution")]
        public InArgument<Money> Caution { get; set; }

        [Input("NewMileage")]
        public InArgument<int> NewMileage { get; set; }

        [Input("VehicleState")]
        public InArgument<int> VehicleState { get; set; }

        [Output("Result")]
        public OutArgument<string> Result { get; set; }

        #endregion


        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            EntityReference recordId = RecordId.Get(context);
            EntityReference vehicleRef = Vehicle.Get(context);
            Money totalCoast = TotalCoast.Get(context);
            int newMileage = NewMileage.Get(context);
            EntityReference movementRef = Movement.Get(context);
            Money caution = Caution.Get(context);
            int vehicleState = VehicleState.Get(context);
            decimal calculation = 0;

            var result = "";

            if (vehicleState.ToString() == "100000001")
            {
                calculation = totalCoast.Value - caution.Value;
                Entity contractEntity = new Entity(recordId.LogicalName, recordId.Id);
                contractEntity["new_totaltax"] = new Money(calculation);

                service.Update(contractEntity);
            }

            else
            {
                calculation = totalCoast.Value;
            }
            var queryVehicle = new QueryExpression("new_vehicle");

            queryVehicle.ColumnSet.AddColumns("new_totalcoast", "new_mileage");
            queryVehicle.Criteria.AddCondition("new_vehicleid", ConditionOperator.Equal, vehicleRef.Id.ToString());

            var result_vehicle = service.RetrieveMultiple(queryVehicle).Entities[0];


            decimal vehicleTotalCoast = result_vehicle.GetAttributeValue<Money>("new_totalcoast").Value;
            vehicleTotalCoast += calculation;

            int vehicleMileage = result_vehicle.GetAttributeValue<int>("new_mileage");
            vehicleMileage += newMileage;

            Entity vehicle = new Entity(vehicleRef.LogicalName, vehicleRef.Id);
            vehicle["new_totalcoast"] = new Money(vehicleTotalCoast);
            vehicle["new_mileage"] = vehicleMileage;

            service.Update(vehicle);

            Entity movement = new Entity(movementRef.LogicalName, movementRef.Id);
            movement["statecode"] = new OptionSetValue(1);

            service.Update(movement);

            result = "Foi realizado com sucesso";

            Result.Set(context, result);

        }
    }
}
