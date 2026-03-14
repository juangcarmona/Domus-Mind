namespace DomusMind.Application.Abstractions.Validation;

public interface IValidator<T>
{
    Task<ValidationResult> Validate(
        T instance,
        CancellationToken cancellationToken);
}
