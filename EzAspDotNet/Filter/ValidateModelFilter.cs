using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;
using EzAspDotNet.Protocols;

namespace EzAspDotNet.Filter
{
    public class ValidateModelFilter : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (!context.ModelState.IsValid)
            {
                var message = string.Join(" | ", context.ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage));
                context.Result = new BadRequestObjectResult(new ResponseHeader { ResultCode = Code.ResultCode.BadRequest, ErrorMessage = message });
            }
        }
    }
}
