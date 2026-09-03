using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Domain.Tests.FormTemplates;

public class FormTemplatePublishTests
{
    private static FormTemplate NewTemplate() => new("Form", null, "system", DateTime.UnixEpoch);

    [Fact]
    public void Publish_promotes_a_template_that_has_at_least_one_field_and_one_step()
    {
        var template = NewTemplate();
        template.AddField("Employee name", FieldType.Text);
        template.AddApprovalStep("Direct manager", ApprovalActionType.Approve);

        template.Publish();

        template.Status.Should().Be(TemplateStatus.Published);
    }

    [Fact]
    public void Publish_rejects_a_template_with_no_fields()
    {
        var template = NewTemplate();
        template.AddApprovalStep("Direct manager", ApprovalActionType.Approve);

        var act = () => template.Publish();

        act.Should().Throw<DomainException>().WithMessage("*at least one field*");
    }

    [Fact]
    public void Publish_rejects_a_template_with_no_approval_steps()
    {
        var template = NewTemplate();
        template.AddField("Employee name", FieldType.Text);

        var act = () => template.Publish();

        act.Should().Throw<DomainException>().WithMessage("*at least one approval step*");
    }
}
