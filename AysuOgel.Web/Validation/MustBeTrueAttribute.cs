using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace AysuOgel.Web.Validation;

[AttributeUsage(AttributeTargets.Property)]
public class MustBeTrueAttribute : ValidationAttribute, IClientModelValidator
{
    public override bool IsValid(object? value) => value is true;

    public void AddValidation(ClientModelValidationContext context)
    {
        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-mustbetrue"] =
            ErrorMessage ?? "Devam etmek için onay vermelisiniz.";
    }
}