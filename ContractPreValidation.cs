using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;

namespace Rent_a_car
{
    public class ContractPreValidation : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            IPluginExecutionContext context = (IPluginExecutionContext)
                serviceProvider.GetService(typeof(IPluginExecutionContext));

            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);
            try
            {
                Entity entity = (Entity)context.InputParameters["Target"];

                if (entity.Contains("new_startdate") && entity.Contains("new_enddate"))
                {
                    DateTime startDate = (DateTime)entity["new_startdate"];
                    DateTime endDate = (DateTime)entity["new_enddate"];
                    Guid entityId = entity.Id;

                    // Set Condition Values

                    // Instantiate QueryExpression query
                    var query = new QueryExpression("new_movement");
                    query.TopCount = 50;
                    // Add columns to query.ColumnSet
                    query.ColumnSet.AddColumn("activityid");

                    // Add conditions to query.Criteria
                    query.Criteria.AddCondition("scheduledstart", ConditionOperator.Between, startDate, endDate);

                    var results = service.RetrieveMultiple(query).Entities;
                    if (results.Count > 0)
                    {
                        throw new InvalidPluginExecutionException("Já existem movimentos nestes dias. Por favor tente outra data");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException($"ContractPreValidation: {ex.Message}");
            }
        }

    }
}
