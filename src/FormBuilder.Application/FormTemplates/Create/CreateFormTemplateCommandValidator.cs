using FluentValidation;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Application.FormTemplates.Create;

public sealed class CreateFormTemplateCommandValidator : AbstractValidator<CreateFormTemplateCommand>
{
    public CreateFormTemplateCommandValidator()
    {
        RuleFor(command => command.Request.Name)
            .NotEmpty().WithMessage("Form name is required.")
            .MaximumLength(FormTemplate.MaxNameLength);

        RuleFor(command => command.Request.Description)
            .MaximumLength(FormTemplate.MaxDescriptionLength);

        RuleFor(command => command.Request.Fields)
            .NotNull()
            .NotEmpty().WithMessage("Add at least one field.");

        RuleForEach(command => command.Request.Fields).ChildRules(field =>
        {
            field.RuleFor(f => f.Label)
                .NotEmpty().WithMessage("Field label is required.")
                .MaximumLength(FormField.MaxLabelLength);

            field.RuleFor(f => f.FieldType).IsInEnum().WithMessage("Unknown field type.");

            field.RuleFor(f => f.Placeholder).MaximumLength(FormField.MaxPlaceholderLength);
            field.RuleFor(f => f.Options).MaximumLength(FormField.MaxOptionsLength);
        });

        RuleFor(command => command.Request.ApprovalSteps)
            .NotNull()
            .NotEmpty().WithMessage("Add at least one approval step.");

        RuleForEach(command => command.Request.ApprovalSteps).ChildRules(step =>
        {
            step.RuleFor(s => s.Name)
                .NotEmpty().WithMessage("Step name is required.")
                .MaximumLength(ApprovalStep.MaxNameLength);

            step.RuleFor(s => s.ActionType).IsInEnum().WithMessage("Unknown action type.");

            step.RuleFor(s => s.ApproverId).MaximumLength(ApprovalStep.MaxApproverIdLength);
        });
    }
}
