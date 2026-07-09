using Crm.Application.DTOs.DynamicRecords;
using FluentValidation;

namespace Crm.Application.Validators
{
    public class CreateDynamicRecordValidator : AbstractValidator<CreateDynamicRecordDto>
    {
        public CreateDynamicRecordValidator()
        {
            RuleFor(x => x.EntityId)
                .NotEmpty().WithMessage("EntityId is required.");

            RuleForEach(x => x.FieldValues)
                .ChildRules(values =>
                {
                    values.RuleFor(v => v.FieldId).NotEmpty();
                    values.RuleFor(v => v.Value)
                        .NotNull().WithMessage("Value cannot be null");
                });
        }
    }

    public class UpdateDynamicRecordValidator : AbstractValidator<UpdateDynamicRecordDto>
    {
        public UpdateDynamicRecordValidator()
        {
            RuleFor(x => x.Id).NotEmpty();
            RuleForEach(x => x.FieldValues)
                .ChildRules(values =>
                {
                    values.RuleFor(v => v.FieldId).NotEmpty();
                    values.RuleFor(v => v.Value)
                        .NotNull().WithMessage("Value cannot be null");
                });
        }
    }
}
