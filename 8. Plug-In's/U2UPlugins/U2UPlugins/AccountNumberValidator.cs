using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace U2UPlugins
{
  public class AccountNumberValidator : IPlugin
  {
    private string accountNumberRegEx = "[A-Z]{2}-[0-9]{4}";

    public AccountNumberValidator()
    { }

    public AccountNumberValidator(string unsecured)
    {
      if (!String.IsNullOrEmpty(unsecured))
        accountNumberRegEx = unsecured;
    }

    public void Execute(IServiceProvider serviceProvider)
    {
      IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
      IOrganizationServiceFactory factory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
      ITracingService tracing = (ITracingService)serviceProvider.GetService(typeof(ITracingService));

      IOrganizationService service = factory.CreateOrganizationService(context.UserId);

      if (context.MessageName == "Create" || context.MessageName == "Update")
      {
        if (context.PrimaryEntityName == "account")
        {
          if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is Entity)
          {
            Entity e = (Entity)context.InputParameters["Target"];


            if (e.Attributes.Contains("accountnumber"))
            {
              string accountNumber = (string)e.Attributes["accountnumber"];

              Regex validFormat = new Regex(accountNumberRegEx);
              if (!validFormat.IsMatch(accountNumber))
              {
                tracing.Trace("Validation error in AccountNumberValidator Plugin");
                string msg = String.Format("Account number is not in correct format. ({0})", accountNumberRegEx);
                throw new InvalidPluginExecutionException(msg);

              }

            }

          }

        }
      }

    }
  }
}
