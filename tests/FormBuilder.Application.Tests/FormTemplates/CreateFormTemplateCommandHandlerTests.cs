using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Application.FormTemplates.Create;
using FormBuilder.Application.Tests.Fakes;
using FormBuilder.Application.Tests.TestData;
using FormBuilder.Domain.Enums;
using NSubstitute;

namespace FormBuilder.Application.Tests.FormTemplates;

public class CreateFormTemplateCommandHandlerTests
{
    private static readonly DateTime Now = new(2026, 2, 1, 12, 0, 0, DateTimeKind.Utc);

    private readonly FakeFormTemplateRepository _repository = new();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly CreateFormTemplateCommandHandler _handler;

    public CreateFormTemplateCommandHandlerTests()
    {
        _handler = new CreateFormTemplateCommandHandler(
            _repository,
            _unitOfWork,
            new FixedDateTimeProvider(Now),
            new StubCurrentUser("dana@tax.gov.il"));
    }

    [Fact]
    public async Task Builds_the_aggregate_from_the_request_and_returns_its_id()
    {
        var command = new CreateFormTemplateCommand(Requests.ValidCreate());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _repository.Added.Should().NotBeNull();

        var template = _repository.Added!;
        result.Value.Should().Be(template.Id);
        template.Name.Should().Be("Vacation Request");
        template.CreatedBy.Should().Be("dana@tax.gov.il");
        template.CreatedAtUtc.Should().Be(Now);
        template.Fields.Select(f => (f.Label, f.Order))
            .Should().Equal([("Employee name", 0), ("Start date", 1)]);
        template.ApprovalSteps.Select(s => (s.Name, s.Order, s.ActionType))
            .Should().Equal(
            [
                ("Direct manager", 0, ApprovalActionType.Approve),
                ("HR verification", 1, ApprovalActionType.Sign),
            ]);
    }

    [Fact]
    public async Task Commits_exactly_once_through_the_unit_of_work()
    {
        await _handler.Handle(new CreateFormTemplateCommand(Requests.ValidCreate()), CancellationToken.None);

        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Returns_a_validation_failure_when_the_aggregate_rejects_the_data()
    {
        var request = Requests.ValidCreate() with
        {
            Fields = [new CreateFormFieldInput("   ", FieldType.Text, IsRequired: false, null, null)],
        };

        var result = await _handler.Handle(new CreateFormTemplateCommand(request), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        _repository.Added.Should().BeNull();
        await _unitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
