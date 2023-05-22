using System.Collections.Immutable;

namespace Validators.Core
{
    public interface IValidationError
    {
        public string Message { get; }
        public IEnumerable<string> Path { get; }
    }

    public class DefaultValidationError : IValidationError
    {
        public string Message { get; init; } = "";
        public IEnumerable<string> Path { get; init; } = ImmutableList<string>.Empty;

        public override string ToString()
        {
            if (Path.Any())
            {
                return $"{string.Join(".", Path)}: {Message}";
            }
            else
            {
                return Message;
            }
        }
    }
}
