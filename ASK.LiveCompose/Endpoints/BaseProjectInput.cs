using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace ASK.LiveCompose.Endpoints;

public class BaseProjectInput : IValidatableObject
{
    [FromRoute(Name = "projectName")]
    public required string ProjectName { get; set; }

    [FromRoute(Name = "service")]
    public string? ServiceName { get; set; }

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if(!ProjectName.IsValidateServiceOrProjectName())
            yield return new ValidationResult("Invalid Project Id");
        if(ServiceName is not null && !ServiceName.IsValidateServiceOrProjectName())
            yield return new ValidationResult("Service name is invalid");
    }
}