namespace Validators.Core
{
    public class Or<T> : IValidator<T>
    {
        private readonly IValidator<T>[] _children;

        public Or(params IValidator<T>[] children)
        {
            _children = children;
        }

        public string ObjectName => $"one of ({string.Join(", ", _children.Select(x => x.ObjectName))})";

        public IEnumerable<IValidationError> Validate(T target)
        {
            foreach (var validator in _children)
            {
                if (!validator.Validate(target).Any())
                {
                    yield break;
                }
            }
            yield return new DefaultValidationError { Message = $"Not {ObjectName}." };
        }
    }
}
