using FormBuilder.Domain.Common;
using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;

namespace FormBuilder.Domain.Tests.FormTemplates;

public class FormTemplateFieldTests
{
    private static FormTemplate NewTemplate() => new("Form", null, "system", DateTime.UnixEpoch);

    [Fact]
    public void AddField_appends_in_call_order_starting_at_zero()
    {
        var template = NewTemplate();

        template.AddField("Employee name", FieldType.Text);
        template.AddField("Start date", FieldType.Date, isRequired: true);

        template.Fields.Select(f => (f.Label, f.Order))
            .Should().Equal([("Employee name", 0), ("Start date", 1)]);
        template.Fields[1].IsRequired.Should().BeTrue();
    }

    [Fact]
    public void AddField_returns_the_created_field_and_normalises_its_placeholder()
    {
        var template = NewTemplate();

        var field = template.AddField("Days", FieldType.Number, placeholder: "  e.g. 5  ");

        field.Order.Should().Be(0);
        field.Placeholder.Should().Be("e.g. 5");
        template.Fields.Should().ContainSingle().Which.Should().BeSameAs(field);
    }

    [Fact]
    public void RemoveField_closes_the_gap_in_the_ordering()
    {
        var template = NewTemplate();
        template.AddField("A", FieldType.Text);
        var b = template.AddField("B", FieldType.Text);
        template.AddField("C", FieldType.Text);

        template.RemoveField(b.Id);

        template.Fields.Select(f => (f.Label, f.Order)).Should().Equal([("A", 0), ("C", 1)]);
    }

    [Fact]
    public void RemoveField_rejects_an_id_that_is_not_on_this_template()
    {
        var template = NewTemplate();

        var act = () => template.RemoveField(Guid.NewGuid());

        act.Should().Throw<DomainException>().WithMessage("*not part of this template*");
    }

    [Fact]
    public void ReorderFields_applies_the_requested_sequence_and_reindexes()
    {
        var template = NewTemplate();
        var a = template.AddField("A", FieldType.Text);
        var b = template.AddField("B", FieldType.Text);
        var c = template.AddField("C", FieldType.Text);

        template.ReorderFields([c.Id, a.Id, b.Id]);

        template.Fields.Select(f => (f.Label, f.Order)).Should().Equal([("C", 0), ("A", 1), ("B", 2)]);
    }

    [Fact]
    public void ReorderFields_rejects_a_sequence_that_does_not_cover_every_field()
    {
        var template = NewTemplate();
        var a = template.AddField("A", FieldType.Text);
        template.AddField("B", FieldType.Text);

        var act = () => template.ReorderFields([a.Id]);

        act.Should().Throw<DomainException>().WithMessage("*exactly once*");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void AddField_rejects_a_missing_label(string? label)
    {
        var template = NewTemplate();

        var act = () => template.AddField(label!, FieldType.Text);

        act.Should().Throw<DomainException>().WithMessage("*Label*");
    }
}
