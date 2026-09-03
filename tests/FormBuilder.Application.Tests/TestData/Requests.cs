using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Application.Tests.TestData;

internal static class Requests
{
    public static CreateFormTemplateRequest ValidCreate(
        string name = "Vacation Request",
        string? description = "Annual leave for all staff")
        => new(
            name,
            description,
            [
                new CreateFormFieldInput("Employee name", FieldType.Text, IsRequired: true, "e.g. Dana Levi", null),
                new CreateFormFieldInput("Start date", FieldType.Date, IsRequired: true, null, null),
            ],
            [
                new CreateApprovalStepInput("Direct manager", ApprovalActionType.Approve, null),
                new CreateApprovalStepInput("HR verification", ApprovalActionType.Sign, "hr@tax.gov.il"),
            ]);
}
