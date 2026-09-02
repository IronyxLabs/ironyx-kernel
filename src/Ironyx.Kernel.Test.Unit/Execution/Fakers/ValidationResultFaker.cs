using AutoBogus;
using FluentValidation.Results;

namespace Ironyx.Kernel.Test.Unit.Execution.Fakers
{
    public class ValidationResultFaker : AutoFaker<ValidationResult>
    {
        public ValidationResultFaker Valid()
        {
            RuleFor(v => v.IsValid, true);

            return this;
        }
    }
}
