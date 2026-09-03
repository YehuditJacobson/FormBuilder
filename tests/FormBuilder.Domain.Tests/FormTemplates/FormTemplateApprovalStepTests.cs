using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Domain.Tests.FormTemplates;

public class FormTemplateApprovalStepTests
{
    private static FormTemplate NewTemplate() => new("Form", null, "system", DateTime.UnixEpoch);

    [Fact]
    public void AddApprovalStep_appends_in_call_order_starting_at_zero()
    {
        var template = NewTemplate();

        template.AddApprovalStep("Direct manager", ApprovalActionType.Approve);
        template.AddApprovalStep("HR verification", ApprovalActionType.Sign, approverId: "hr-01");

        template.ApprovalSteps.Select(s => (s.Name, s.Order, s.ActionType))
            .Should().Equal(
            [
                ("Direct manager", 0, ApprovalActionType.Approve),
                ("HR verification", 1, ApprovalActionType.Sign),
            ]);
        template.ApprovalSteps[1].ApproverId.Should().Be("hr-01");
    }

    [Fact]
    public void AddApprovalStep_treats_a_blank_approver_as_absent()
    {
        var template = NewTemplate();

        var step = template.AddApprovalStep("Review", ApprovalActionType.Acknowledge, approverId: "   ");

        step.ApproverId.Should().BeNull();
    }

    [Fact]
    public void RemoveApprovalStep_closes_the_gap_in_the_ordering()
    {
        var template = NewTemplate();
        template.AddApprovalStep("A", ApprovalActionType.Approve);
        var b = template.AddApprovalStep("B", ApprovalActionType.Approve);
        template.AddApprovalStep("C", ApprovalActionType.Approve);

        template.RemoveApprovalStep(b.Id);

        template.ApprovalSteps.Select(s => (s.Name, s.Order)).Should().Equal([("A", 0), ("C", 1)]);
    }

    [Fact]
    public void RemoveApprovalStep_rejects_an_id_that_is_not_on_this_template()
    {
        var template = NewTemplate();

        var act = () => template.RemoveApprovalStep(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*not part of this template*");
    }

    [Fact]
    public void ReorderApprovalSteps_applies_the_requested_sequence_and_reindexes()
    {
        var template = NewTemplate();
        var a = template.AddApprovalStep("A", ApprovalActionType.Approve);
        var b = template.AddApprovalStep("B", ApprovalActionType.Approve);
        var c = template.AddApprovalStep("C", ApprovalActionType.Approve);

        template.ReorderApprovalSteps([b.Id, c.Id, a.Id]);

        template.ApprovalSteps.Select(s => (s.Name, s.Order)).Should().Equal([("B", 0), ("C", 1), ("A", 2)]);
    }

    [Fact]
    public void ReorderApprovalSteps_rejects_a_sequence_containing_a_foreign_id()
    {
        var template = NewTemplate();
        var a = template.AddApprovalStep("A", ApprovalActionType.Approve);
        template.AddApprovalStep("B", ApprovalActionType.Approve);

        var act = () => template.ReorderApprovalSteps([a.Id, Guid.NewGuid()]);

        act.Should().Throw<DomainException>().WithMessage("*exactly once*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void AddApprovalStep_rejects_a_missing_name(string? name)
    {
        var template = NewTemplate();

        var act = () => template.AddApprovalStep(name!, ApprovalActionType.Approve);

        act.Should().Throw<DomainException>().WithMessage("*Name*");
    }
}
