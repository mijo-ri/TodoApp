using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoApp.Api.Contracts;
using TodoApp.Api.Errors;
using TodoApp.Application.Todos;
using TodoApp.Application.Todos.Complete;
using TodoApp.Application.Todos.Create;
using TodoApp.Application.Todos.Delete;
using TodoApp.Application.Todos.GetById;
using TodoApp.Application.Todos.List;
using TodoApp.Application.Todos.Reopen;
using TodoApp.Application.Todos.Update;

namespace TodoApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TodosController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetTodoByIdQuery(id), cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet]
    public async Task<ActionResult<List<TodoDto>>> List(
        [FromQuery] bool? isCompleted,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new ListTodosQuery(isCompleted), cancellationToken);

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateTodoCommand(
            request.Title,
            request.Notes,
            request.DueDate);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToCreatedResult(
            this,
            nameof(GetById),
            created => new { id = created.Id });
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTodoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateTodoCommand(
            id,
            request.Title,
            request.Notes,
            request.DueDate);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteTodoCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToNoContentResult(this);
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new CompleteTodoCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/reopen")]
    public async Task<IActionResult> Reopen(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new ReopenTodoCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }
}