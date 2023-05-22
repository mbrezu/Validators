namespace Validators.Core
{
    public interface IValidator<T>
    {
        public IEnumerable<IValidationError> Validate(T target);
        public string ObjectName { get; }
    }
}
