namespace Validators.Core
{
    public class DelayedValidator<T> : IValidator<T>
    {
        private readonly Func<IValidator<T>> _validatorProvider;

        public DelayedValidator(Func<IValidator<T>> validatorProvider)
        {
            _validatorProvider = validatorProvider;
        }

        public string ObjectName => _validatorProvider().ObjectName;

        public IEnumerable<IValidationError> Validate(T target) 
            => _validatorProvider().Validate(target);
    }
}
