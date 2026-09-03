using FluentValidation.TestHelper;
using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Application.FormTemplates.Create;
using FormBuilder.Application.Tests.TestData;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Application.Tests.FormTemplates;

public class CreateFormTemplateCommandValidatorTests
{
    private readonly CreateFormTemplateCommandValidator _validator = new();

    private static CreateFormTemplateCommand Command(CreateFormTemplateRequest request) => new(request);

    [Fact]
    public void Accepts_a_well_formed_request()
    {
        var result = _validator.TestValidate(Command(Requests.ValidCreate()));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Rejects_a_missing_form_name()
    {
        var result = _validator.TestValidate(Command(Requests.ValidCreate(name: "   ")));

        result.ShouldHaveValidationErrorFor(command => command.Request.Name);
    }

    [Fact]
    public void Rejects_a_request_with_no_fields()
    {
        var request = Requests.ValidCreate() with { Fields = [] };

        var result = _validator.TestValidate(Command(request));

        result.ShouldHaveValidationErrorFor(command => command.Request.Fields);
    }

    [Fact]
    public void Rejects_a_request_with_no_approval_steps()
    {
        var request = Requests.ValidCreate() with { ApprovalSteps = [] };

        var result = _validator.TestValidate(Command(request));

        result.ShouldHaveValidationErrorFor(command => command.Request.ApprovalSteps);
    }

    [Fact]
    public void Rejects_a_field_with_a_blank_label()
    {
        var request = Requests.ValidCreate() with
        {
            Fields = [new CreateFormFieldInput("  ", FieldType.Text, IsRequired: false, null, null)],
        };

        var result = _validator.TestValidate(Command(request));

        result.ShouldHaveValidationErrorFor("Request.Fields[0].Label");
    }

    [Fact]
    public void Rejects_an_approval_step_with_a_blank_name()
    {
        var request = Requests.ValidCreate() with
        {
            ApprovalSteps = [new CreateApprovalStepInput("  ", ApprovalActionType.Approve, null)],
        };

        var result = _validator.TestValidate(Command(request));

        result.ShouldHaveValidationErrorFor("Request.ApprovalSteps[0].Name");
    }
}
