using System.Net;
using System.Net.Http.Json;
using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Domain.Enums;

namespace FormBuilder.Api.IntegrationTests;

public sealed class FormsEndpointsTests(FormBuilderApiFactory factory) : IClassFixture<FormBuilderApiFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    private static CreateFormTemplateRequest ValidRequest(string name = "בקשת חופשה")
        => new(
            name,
            "חופשה שנתית לכלל העובדים",
            [
                new CreateFormFieldInput("שם העובד", FieldType.Text, IsRequired: true, "דנה לוי", null),
                new CreateFormFieldInput("תאריך התחלה", FieldType.Date, IsRequired: true, null, null),
            ],
            [
                new CreateApprovalStepInput("אישור מנהל ישיר", ApprovalActionType.Approve, null),
                new CreateApprovalStepInput("אימות משאבי אנוש", ApprovalActionType.Sign, "hr@tax.gov.il"),
            ]);

    [Fact]
    public async Task Post_valid_returns_201_with_id_and_location()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/forms", ValidRequest(), FormBuilderApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await response.Content.ReadFromJsonAsync<CreateResponse>(FormBuilderApiFactory.Json);
        body!.Id.Should().NotBeEmpty();
        response.Headers.Location!.ToString().Should().Contain(body.Id.ToString());
    }

    [Fact]
    public async Task Post_then_get_by_id_returns_the_full_ordered_graph()
    {
        var id = await CreateAsync(ValidRequest("דיווח ימי מחלה"));

        var detail = await _client.GetFromJsonAsync<FormTemplateDetailDto>(
            $"/api/v1/forms/{id}", FormBuilderApiFactory.Json);

        detail.Should().NotBeNull();
        detail!.Name.Should().Be("דיווח ימי מחלה");
        detail.Status.Should().Be(TemplateStatus.Draft);
        detail.Fields.Select(field => (field.Label, field.Order, field.FieldType))
            .Should().Equal([("שם העובד", 0, FieldType.Text), ("תאריך התחלה", 1, FieldType.Date)]);
        detail.ApprovalSteps.Select(step => (step.Name, step.Order, step.ActionType))
            .Should().Equal(
            [
                ("אישור מנהל ישיר", 0, ApprovalActionType.Approve),
                ("אימות משאבי אנוש", 1, ApprovalActionType.Sign),
            ]);
    }

    [Fact]
    public async Task Get_list_returns_created_templates_newest_first_with_counts()
    {
        var older = await CreateAsync(ValidRequest("טופס ישן"));
        var newer = await CreateAsync(ValidRequest("טופס חדש"));

        var summaries = await _client.GetFromJsonAsync<List<FormTemplateSummaryDto>>(
            "/api/v1/forms", FormBuilderApiFactory.Json);

        summaries.Should().NotBeNull();
        var mine = summaries!.Where(summary => summary.Id == older || summary.Id == newer).ToList();
        mine.Select(summary => summary.Id).Should().Equal(newer, older);
        mine[0].FieldCount.Should().Be(2);
        mine[0].ApprovalStepCount.Should().Be(2);
    }

    [Fact]
    public async Task Post_invalid_returns_400_with_field_errors()
    {
        var invalid = new CreateFormTemplateRequest("", null, [], []);

        var response = await _client.PostAsJsonAsync("/api/v1/forms", invalid, FormBuilderApiFactory.Json);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problem = await response.Content.ReadFromJsonAsync<ValidationProblem>(FormBuilderApiFactory.Json);
        problem!.Errors.Keys.Should().Contain(["Request.Name", "Request.Fields", "Request.ApprovalSteps"]);
    }

    [Fact]
    public async Task Get_unknown_id_returns_404()
    {
        var response = await _client.GetAsync($"/api/v1/forms/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private async Task<Guid> CreateAsync(CreateFormTemplateRequest request)
    {
        var response = await _client.PostAsJsonAsync("/api/v1/forms", request, FormBuilderApiFactory.Json);
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<CreateResponse>(FormBuilderApiFactory.Json);
        return body!.Id;
    }

    private sealed record CreateResponse(Guid Id);

    private sealed record ValidationProblem(Dictionary<string, string[]> Errors);
}
