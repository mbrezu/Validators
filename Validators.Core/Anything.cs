namespace Validators.Core
{
    public class Anything<T> : IValidator<T>
    {
        public string ObjectName => "anything";

        public IEnumerable<IValidationError> Validate(T target)
        {
            yield break;
        }
    }
}
