using Asp.Versioning;
using FormBuilder.Api.Contracts;
using FormBuilder.Api.Extensions;
using FormBuilder.Application.FormTemplates.Contracts;
using FormBuilder.Application.FormTemplates.Create;
using FormBuilder.Application.FormTemplates.GetById;
using FormBuilder.Application.FormTemplates.GetList;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FormBuilder.Api.Controllers;

/// <summary>Create, list, and read organizational form templates and their approval routes.</summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/forms")]
[Produces("application/json")]
public sealed class FormsController(ISender sender) : ControllerBase
{
    /// <summary>Creates a form template with all of its fields and approval steps in one call.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(CreateFormTemplateResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateFormTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateFormTemplateCommand(request), cancellationToken);

        return result.ToActionResult(this, id =>
            Created($"/api/v1/forms/{id}", new CreateFormTemplateResponse(id)));
    }

    /// <summary>Returns every form template as a summary row, newest first.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<FormTemplateSummaryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<FormTemplateSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var summaries = await sender.Send(new GetFormTemplatesQuery(), cancellationToken);
        return Ok(summaries);
    }

    /// <summary>Returns a single form template with its full field and approval-step detail.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(FormTemplateDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetFormTemplateByIdQuery(id), cancellationToken);

        return result.ToActionResult(this, detail => Ok(detail));
    }
}
