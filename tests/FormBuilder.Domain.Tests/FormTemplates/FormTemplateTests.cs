using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Domain.Tests.FormTemplates;

public class FormTemplateTests
{
    private static readonly DateTime CreatedAt = new(2026, 1, 15, 9, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Constructor_populates_the_envelope_and_starts_as_a_draft()
    {
        var template = new FormTemplate("Vacation Request", "Annual leave for all staff", "user@tax.gov.il", CreatedAt);

        template.Name.Should().Be("Vacation Request");
        template.Description.Should().Be("Annual leave for all staff");
        template.CreatedBy.Should().Be("user@tax.gov.il");
        template.CreatedAtUtc.Should().Be(CreatedAt);
        template.Status.Should().Be(TemplateStatus.Draft);
        template.Fields.Should().BeEmpty();
        template.ApprovalSteps.Should().BeEmpty();
    }

    [Fact]
    public void Constructor_trims_the_name_and_treats_a_blank_description_as_absent()
    {
        var template = new FormTemplate("  Sick Leave  ", "   ", "system", CreatedAt);

        template.Name.Should().Be("Sick Leave");
        template.Description.Should().BeNull();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_rejects_a_missing_name(string? name)
    {
        var act = () => new FormTemplate(name!, null, "system", CreatedAt);

        act.Should().Throw<DomainException>().WithMessage("*Name*");
    }

    [Fact]
    public void Constructor_rejects_a_name_longer_than_the_limit()
    {
        var name = new string('x', FormTemplate.MaxNameLength + 1);

        var act = () => new FormTemplate(name, null, "system", CreatedAt);

        act.Should().Throw<DomainException>().WithMessage($"*{FormTemplate.MaxNameLength}*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("  ")]
    public void Constructor_rejects_a_missing_author(string? createdBy)
    {
        var act = () => new FormTemplate("Form", null, createdBy!, CreatedAt);

        act.Should().Throw<DomainException>().WithMessage("*CreatedBy*");
    }
}
