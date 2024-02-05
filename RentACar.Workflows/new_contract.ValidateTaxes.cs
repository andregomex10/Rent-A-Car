using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace RentACar.Workflows
{
    public class ValidateTaxes : CodeActivity
    {
        #region Parameters

        [Input("RecordId")]
        [RequiredArgument]
        [ReferenceTarget("new_contract")]
        public InArgument<EntityReference> RecordId { get; set; }

        [Input("Vehicle")]
        [ReferenceTarget("new_vehicle")]
        public InArgument<EntityReference> Vehicle { get; set; }

        [Input("StartDate")]
        public InArgument<DateTime> StartDate { get; set; }

        [Input("EndDate")]
        public InArgument<DateTime> EndDate { get; set; }

        [Output("Result")]
        public OutArgument<string> Result { get; set; }

        #endregion
        protected override void Execute(CodeActivityContext context)
        {
            IWorkflowContext workflowContext = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(workflowContext.UserId);

            decimal totalCoast = 0;
            int totalDays = 0;
            decimal caution = 100;
            string response = "";
            var id = "";
            var idEntities = "";
            int daysAtTax = 0;
            var taxName = "";
            var taxNameVerify = "";

            Guid recordId = RecordId.Get(context).Id;

            EntityReference vehicleRef = Vehicle.Get(context);

            Guid vehicleId = vehicleRef.Id;

            DateTime startDate = StartDate.Get(context);
            DateTime endDate = EndDate.Get(context);

            var query = new QueryExpression("new_tax");

            query.ColumnSet.AddColumns("new_taxid", "new_enddate", "new_startdate", "new_tax", "new_name");

            query.Criteria.AddCondition("new_vehicleid", ConditionOperator.Equal, vehicleId);
            query.Criteria.AddCondition("new_startdate", ConditionOperator.OnOrBefore, endDate);
            query.Criteria.AddCondition("new_enddate", ConditionOperator.OnOrAfter, startDate);

            var entities = service.RetrieveMultiple(query).Entities;



            for (DateTime i = startDate; i <= endDate; i = i.AddDays(1))
            {            
                foreach (var entity in entities)
                {
                    if (entity.GetAttributeValue<DateTime>("new_startdate") <= i && entity.GetAttributeValue<DateTime>("new_enddate") >= i)
                    {
                        decimal tax = entity.GetAttributeValue<Money>("new_tax").Value;
                        idEntities = entity.Id.ToString();
                        taxName = entity.GetAttributeValue<string>("new_name");

                        if (totalDays % 5 == 0 && totalDays != 0)
                        {
                            tax = tax * 0.97m;
                        }
                        if (i.DayOfWeek == DayOfWeek.Sunday || (i.Month == 12 && i.Day == 25) || (i.Month == 1 && i.Day == 1) || (i.Month == 4 && i.Day == 25) || (i.Month == 6 && i.Day == 10) || (i.Month == 8 && i.Day == 15) || (i.Month == 10 && i.Day == 5) || (i.Month == 11 && i.Day == 1) || (i.Month == 12 && i.Day == 1) || (i.Month == 12 && i.Day == 8) || (i.Month == 5 && i.Day == 1))
                        {
                            tax = tax * 1.05m;
                        }
                        totalCoast = totalCoast + tax;

                    }
                }

                totalDays++;

                if (id != idEntities)
                {
                    if (id == "")
                    {
                        taxNameVerify = taxName;
                        id = idEntities;
                        daysAtTax++;
                    }
                    else
                    {
                        response += "Foram usados " + daysAtTax.ToString() + " dias da " + taxNameVerify.ToString() + ". ";
                        daysAtTax = 1;
                        id = idEntities;
                        taxNameVerify = taxName;
                    }
                }
                else
                {
                    daysAtTax++;
                }
            }


            response += "Foram usados " + daysAtTax.ToString() + " dias da " + taxNameVerify.ToString() + ". ";


            if (totalCoast * 0.1m > 100)
            {
                caution = totalCoast * 0.1m;
            }

            totalCoast += caution;

            response += "Este aluguer tem um custo de " + totalCoast.ToString("0.00") + " e uma caução no valor de " + caution.ToString("0.00") + " num total de " + totalDays.ToString() + " dias.";

            Entity new_entity = new Entity(RecordId.Get(context).LogicalName, recordId);

            new_entity["new_taxesresume"] = response;
            new_entity["new_totaltax"] = new Money(totalCoast);
            new_entity["new_caution"] = new Money(caution);
            new_entity["new_diastotais"] = totalDays;

            service.Update(new_entity);

            Result.Set(context, response);

        }
    }
}
