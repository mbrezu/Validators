using System.Collections.Immutable;

namespace Validators.Core
{
    public interface IValidationError
    {
        public string Message { get; }
        public IEnumerable<string> ReversePath { get; }
    }

    public class DefaultValidationError : IValidationError
    {
        public string Message { get; init; } = "";
        public IEnumerable<string> ReversePath { get; init; } = ImmutableList<string>.Empty;

        public override string ToString()
        {
            if (ReversePath.Any())
            {
                return $"{string.Join(".", ReversePath.Reverse())}: {Message}";
            }
            else
            {
                return Message;
            }
        }
    }
}
