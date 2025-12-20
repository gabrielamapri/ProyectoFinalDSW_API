using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace CentroTerapia.API.Middleware
{
    public class ValidatorFilter : IAsyncActionFilter
    {
        private readonly IServiceProvider _serviceProvider;

        public ValidatorFilter(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {   
            foreach( var argument in context.ActionArguments.Values)
            {
                if(argument == null) continue;

                var argumentType = argument.GetType();
                var validatorType = typeof(IValidator<>).MakeGenericType(argumentType);

                var validator = _serviceProvider.GetService(validatorType) as IValidator;

                if (validator != null)
                {
                    ValidationResult result = await validator.ValidateAsync(new ValidationContext<object>(argument));
                    if (!result.IsValid)
                    {
                        var problemDetails = new ValidationProblemDetails();
                        foreach (var error in result.Errors)
                        {
                            if (problemDetails.Errors.ContainsKey(error.PropertyName))
                            {
                                var errors = problemDetails.Errors[error.PropertyName].ToList();
                                errors.Add(error.ErrorMessage);
                                problemDetails.Errors[error.PropertyName] = errors.ToArray();
                            }
                            else
                            {
                                problemDetails.Errors.Add(error.PropertyName, new[] { error.ErrorMessage });
                            }
                        }

                        context.Result = new BadRequestObjectResult(problemDetails);
                        return;
                    }
                }
            }

            await next();
        }
    }
}
