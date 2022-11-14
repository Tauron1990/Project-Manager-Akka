using System.Globalization;
using FluentValidation;

namespace SimpleProjectManager.Shared.Validators;

public class ProjectNameValidator : AbstractValidator<ProjectName>
{
    public ProjectNameValidator()
    {
        RuleFor(jn => jn.Value)
           .NotEmpty().WithMessage("Es muss ein Name angegeben werden")
           .Custom(
                (arg, context) =>
                {
                    if(string.IsNullOrWhiteSpace(arg) || !arg.ToUpper(CultureInfo.InvariantCulture).StartsWith("BM", StringComparison.Ordinal)) return;

                    if(arg.Length != 10)
                    {
                        context.AddFailure("Der name muss 10 zeichen lang sein");

                        return;
                    }

                    if(!arg[2..4].All(char.IsDigit))
                        context.AddFailure("Nach BM muss die Jareszahl folgen");

                    if(arg[4] != '_')
                        context.AddFailure("Nach der Jareszahl kommt ein unterstrich");

                    if(!arg[5..].All(char.IsDigit))
                        context.AddFailure("Nach dem unterstrich kommt ein 5 stelliger Auftragszähler");
                });
    }
}