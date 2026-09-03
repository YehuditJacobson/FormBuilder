using FormBuilder.Domain.Enums;
using FormBuilder.Domain.FormTemplates;
using FormBuilder.Infrastructure.Persistence;
using FormBuilder.Infrastructure.Persistence.Queries;
using FormBuilder.Infrastructure.Persistence.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FormBuilder.Infrastructure.Tests.Persistence;

public sealed class AppDbContextPersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public AppDbContextPersistenceTests()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    private AppDbContext NewContext() => new(_options);

    private static FormTemplate SampleTemplate()
    {
        var template = new FormTemplate(
            "Vacation Request", "Annual leave", "dana@tax.gov.il",
            new DateTime(2026, 1, 5, 8, 0, 0, DateTimeKind.Utc));

        template.AddField("Employee name", FieldType.Text, isRequired: true, placeholder: "e.g. Dana");
        template.AddField("Start date", FieldType.Date, isRequired: true);
        template.AddApprovalStep("Direct manager", ApprovalActionType.Approve);
        template.AddApprovalStep("HR verification", ApprovalActionType.Sign, "hr@tax.gov.il");
        return template;
    }

    [Fact]
    public async Task Repository_persists_the_template_with_its_fields_and_steps()
    {
        var template = SampleTemplate();
        await using (var write = NewContext())
        {
            await new FormTemplateRepository(write).AddAsync(template, CancellationToken.None);
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var stored = await read.FormTemplates
            .Include(t => t.Fields)
            .Include(t => t.ApprovalSteps)
            .SingleAsync();

        stored.Name.Should().Be("Vacation Request");
        stored.CreatedBy.Should().Be("dana@tax.gov.il");
        stored.Fields.OrderBy(f => f.Order).Select(f => f.Label)
            .Should().Equal("Employee name", "Start date");
        stored.ApprovalSteps.OrderBy(s => s.Order).Select(s => (s.Name, s.Order))
            .Should().Equal([("Direct manager", 0), ("HR verification", 1)]);
    }

    [Fact]
    public async Task GetDetail_projects_the_graph_with_children_in_order()
    {
        var template = SampleTemplate();
        await using (var write = NewContext())
        {
            write.FormTemplates.Add(template);
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var queries = new FormTemplateQueries(read);

        var detail = await queries.GetDetailAsync(template.Id, CancellationToken.None);

        detail.Should().NotBeNull();
        detail!.Status.Should().Be(TemplateStatus.Draft);
        detail.Fields.Select(f => (f.Label, f.Order, f.FieldType))
            .Should().Equal([("Employee name", 0, FieldType.Text), ("Start date", 1, FieldType.Date)]);
        detail.ApprovalSteps.Select(s => (s.Name, s.ActionType))
            .Should().Equal(
            [
                ("Direct manager", ApprovalActionType.Approve),
                ("HR verification", ApprovalActionType.Sign),
            ]);
    }

    [Fact]
    public async Task GetDetail_returns_null_for_an_unknown_id()
    {
        await using var read = NewContext();

        var detail = await new FormTemplateQueries(read).GetDetailAsync(Guid.NewGuid(), CancellationToken.None);

        detail.Should().BeNull();
    }

    [Fact]
    public async Task GetSummaries_returns_child_counts_newest_first()
    {
        await using (var write = NewContext())
        {
            var older = SampleTemplate();
            var newer = new FormTemplate("Sick Leave", null, "system", new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
            newer.AddField("Reason", FieldType.Text);
            newer.AddApprovalStep("Manager", ApprovalActionType.Approve);
            write.FormTemplates.AddRange(older, newer);
            await write.SaveChangesAsync();
        }

        await using var read = NewContext();
        var summaries = await new FormTemplateQueries(read).GetSummariesAsync(CancellationToken.None);

        summaries.Select(s => s.Name).Should().Equal("Sick Leave", "Vacation Request");
        summaries[0].FieldCount.Should().Be(1);
        summaries[0].ApprovalStepCount.Should().Be(1);
        summaries[1].FieldCount.Should().Be(2);
        summaries[1].ApprovalStepCount.Should().Be(2);
    }

    [Fact]
    public async Task Deleting_a_template_cascades_to_its_children()
    {
        var template = SampleTemplate();
        await using (var write = NewContext())
        {
            write.FormTemplates.Add(template);
            await write.SaveChangesAsync();
        }

        await using (var remove = NewContext())
        {
            var stored = await remove.FormTemplates.SingleAsync();
            remove.FormTemplates.Remove(stored);
            await remove.SaveChangesAsync();
        }

        await using var read = NewContext();
        (await read.FormFields.CountAsync()).Should().Be(0);
        (await read.ApprovalSteps.CountAsync()).Should().Be(0);
    }

    public void Dispose() => _connection.Dispose();
}
