using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace RentACar.Workflows
{
    public class CalculateROI : CodeActivity
    {
        #region Parameters
        [Input("RecordId")]
        [RequiredArgument]
        [ReferenceTarget("new_vehicle")]
        public InArgument<EntityReference> RecordId { get; set; }

        [Input("TotalCoast")]
        public InArgument<Money> TotalCoast { get; set; }

        [Input("InitialCoast")]
        public InArgument<Money> InitialCoast { get; set; }

        [Output("Result")]
        public OutArgument<string> Result { get; set; }

        #endregion

        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            EntityReference vehicle = RecordId.Get(context);
            Money totalCoast = TotalCoast.Get(context);
            Money initialCoast = InitialCoast.Get(context);
            decimal operationalCoasts = 0;
            decimal vehicleLife = 10;
            decimal percentage = 100;

            var result = "";

            var query = new QueryExpression("new_movement");

            // Add columns to query.ColumnSet
            query.ColumnSet.AddColumn("new_price");

            // Add conditions to query.Criteria
            query.Criteria.AddCondition("new_vehicleid", ConditionOperator.Equal, vehicle.Id.ToString());
            query.Criteria.AddCondition("new_tipodemanutencao", ConditionOperator.NotEqual, 100000003);

            var resultMovement = service.RetrieveMultiple(query).Entities;

            if (resultMovement.Count > 0)
            {
                foreach (var entity in resultMovement)
                {
                    decimal coast = entity.GetAttributeValue<Money>("new_price").Value;
                    operationalCoasts += coast;
                }
            }

            int new_residualvalue = (int)(initialCoast.Value * (decimal)Math.Pow(1 - 0.10, 10));

            int annual_depretiation = (int)((initialCoast.Value - new_residualvalue) / vehicleLife);

            decimal roi = ((totalCoast.Value - (operationalCoasts + annual_depretiation)) / initialCoast.Value) * percentage;

            Entity new_vehicle = new Entity(vehicle.LogicalName, vehicle.Id);
            new_vehicle["new_residualvalue"] = new_residualvalue;
            new_vehicle["new_annualdepreciation"] = annual_depretiation;
            new_vehicle["new_operationalcostsmoney"] = new Money(operationalCoasts);
            new_vehicle["new_roidecimal"] = roi;

            service.Update(new_vehicle);

            result = "Valor residual: " + new_residualvalue.ToString() + ", depreciação anual = " + annual_depretiation.ToString() + ", custos operacionais: " + operationalCoasts.ToString() + ", roi: " + roi.ToString();

            Result.Set(context, result);
        }
    }
}
