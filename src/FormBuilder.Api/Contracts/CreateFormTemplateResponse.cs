namespace FormBuilder.Api.Contracts;

/// <summary>Body returned by <c>POST /api/v1/forms</c>: the id of the newly created template.</summary>
public sealed record CreateFormTemplateResponse(Guid Id);
