using FluentValidation;
using MediatR;

namespace RydrSafe.Application.Common.Behaviors;

/// <summary>
/// Runs every registered <see cref="IValidator{T}"/> for a request before its handler.
///
/// Without this, <c>AddValidatorsFromAssembly</c> puts the validators in the container and
/// nothing ever resolves them — so every <c>AbstractValidator</c> in the codebase is dead code
/// that reads as though it were enforcing something. That is how the 18+ age gate and the
/// Part D consent requirements came to be documented as enforced while registration would
/// happily accept a child with no boxes ticked (issue #39).
///
/// Failures are collected across all validators for the request, so a form gets every problem
/// at once instead of one per round trip.
/// </summary>
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var applicable = validators.ToList();

        if (applicable.Count == 0)
            return await next(cancellationToken);

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            applicable.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count > 0)
            throw new ValidationException(failures);

        return await next(cancellationToken);
    }
}
