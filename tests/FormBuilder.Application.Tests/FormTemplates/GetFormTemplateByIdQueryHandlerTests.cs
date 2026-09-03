using FormBuilder.Application.Common;
using FormBuilder.Application.FormTemplates.Abstractions;
using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Application.FormTemplates.GetById;
using FormBuilder.Domain.Enums;
using NSubstitute;

namespace FormBuilder.Application.Tests.FormTemplates;

public class GetFormTemplateByIdQueryHandlerTests
{
    private readonly IFormTemplateQueries _queries = Substitute.For<IFormTemplateQueries>();
    private readonly GetFormTemplateByIdQueryHandler _handler;

    public GetFormTemplateByIdQueryHandlerTests()
        => _handler = new GetFormTemplateByIdQueryHandler(_queries);

    [Fact]
    public async Task Returns_the_detail_when_the_template_exists()
    {
        var id = Guid.NewGuid();
        var detail = new FormTemplateDetailDto(
            id, "Vacation Request", null, DateTime.UnixEpoch, "system", TemplateStatus.Draft, [], []);
        _queries.GetDetailAsync(id, Arg.Any<CancellationToken>()).Returns(detail);

        var result = await _handler.Handle(new GetFormTemplateByIdQuery(id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeSameAs(detail);
    }

    [Fact]
    public async Task Returns_a_not_found_failure_when_the_template_is_missing()
    {
        var id = Guid.NewGuid();
        _queries.GetDetailAsync(id, Arg.Any<CancellationToken>()).Returns((FormTemplateDetailDto?)null);

        var result = await _handler.Handle(new GetFormTemplateByIdQuery(id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }
}
