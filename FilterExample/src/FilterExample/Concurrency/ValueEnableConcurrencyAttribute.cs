using FilterExample.Extensions;
using FilterExample.Models;
using FilterExample.Repositories;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FilterExample.Concurrency
{
    public class ValueEnableConcurrencyAttribute : EnableConcurrenyAttributeBase
    {
        private const string IdActionArgumentName = "id";

        public ValueEnableConcurrencyAttribute(bool PreconditionRequired = false) : base(PreconditionRequired)
        { }
        
        protected override async Task<object> FindExistingAsync(ActionExecutingContext context)
        {
            var repository = context.HttpContext.RequestServices.GetService<IRepository<Value>>();

            if (context.ActionArguments.ContainsKey(IdActionArgumentName))
            {
                ResourceType = typeof(Value);
                return await repository.FindAsync(context.ActionArguments[IdActionArgumentName]);
            }
            else
            {
                ResourceType = typeof(IEnumerable<Value>);
                return await repository.GetAsync();
            }
        }
    }
}
