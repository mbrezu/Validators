namespace Validators.Core
{
    public class And<T> : IValidator<T>
    {
        private readonly IValidator<T>[] _children;

        public And(params IValidator<T>[] children)
        {
            _children = children;
        }

        public string ObjectName => $"all of ({string.Join(", ", _children.Select(x => x.ObjectName))})";

        public IEnumerable<IValidationError> Validate(T target) 
            => _children.SelectMany(x => x.Validate(target));
    }
}
