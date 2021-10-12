using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace U2UActivities
{
    public class CalculateCreditLimit : CodeActivity
    {
        protected override void Execute(CodeActivityContext context)
        {
            string country = Country.Get(context);
            EntityReference er = ContactReference.Get(context);
            Guid contactId = er.Id;

            IWorkflowContext ctx = context.GetExtension<IWorkflowContext>();
            IOrganizationServiceFactory factory = context.GetExtension<IOrganizationServiceFactory>();
            ITracingService tracing = context.GetExtension<ITracingService>();

            IOrganizationService service = factory.CreateOrganizationService(Guid.Empty);
            if (ctx.PrimaryEntityName == "contact")
            {
                Guid contactIdAlternate = ctx.PrimaryEntityId;
              
                tracing.Trace("Custom Trace: calculating credit limit.");
                if (country == "Belgium")
                    CreditLimit.Set(context, new Money(1000));
                else
                    CreditLimit.Set(context, new Money(500));

            }
            else
            {
                tracing.Trace("Custom Trace: I can only work with the contact entity");
                throw new InvalidPluginExecutionException(OperationStatus.Failed, "I can only work with the contact entity");
            }

        }

        [Input("Country of residence")]
        [ArgumentRequired()]
        [Default("Belgium")]
        public InArgument<string> Country { get; set; }

        [Input("Contact")]
        [ArgumentRequired()]
        [ReferenceTarget("contact")]
        public InArgument<EntityReference> ContactReference { get; set; }

        [Output("Credit Limit")]
        public OutArgument<Money> CreditLimit { get; set; }

    }
}
