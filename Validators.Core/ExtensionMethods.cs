using System.Collections.Immutable;

namespace Validators.Core
{
    public static class ExtensionMethods
    {
        public static IValidationError Wrap(this IValidationError error, string pathComponent)
        {
            return new DefaultValidationError
            {
                Message = error.Message,
                ReversePath = error.ReversePath.ToImmutableList().Add(pathComponent)
            };
        }
    }
}
