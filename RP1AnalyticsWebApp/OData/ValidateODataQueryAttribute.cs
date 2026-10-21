using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Query.Validator;
using Microsoft.OData;
using System.Linq;

namespace RP1AnalyticsWebApp.OData
{
    /// <summary>
    /// Validates the action's OData query options before it runs.
    /// The options get applied manually to the in-memory career cache and to MongoDB queries,
    /// so without this nothing limits how expensive a client-supplied $filter can be.
    /// </summary>
    public class ValidateODataQueryAttribute : ActionFilterAttribute
    {
        private static readonly ODataValidationSettings ValidationSettings = new ODataValidationSettings
        {
            // Only $filter is enabled in Startup; $compute could otherwise smuggle expressions past the function check
            AllowedQueryOptions = AllowedQueryOptions.Filter | AllowedQueryOptions.Format,
            // matchesPattern runs a client-supplied regex with no timeout
            AllowedFunctions = AllowedFunctions.AllFunctions & ~AllowedFunctions.MatchesPattern,
            // Nested any/all over career periods gets polynomially expensive
            MaxAnyAllExpressionDepth = 1,
            // Leaves room for the career filters with lots of players/races selected
            MaxNodeCount = 500
        };

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            foreach (ODataQueryOptions queryOptions in context.ActionArguments.Values.OfType<ODataQueryOptions>())
            {
                try
                {
                    queryOptions.Validate(ValidationSettings);
                }
                catch (ODataException ex)
                {
                    context.Result = new BadRequestObjectResult(ex.Message);
                    return;
                }
            }
        }
    }
}
