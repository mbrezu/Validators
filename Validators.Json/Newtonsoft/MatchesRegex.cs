using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using Validators.Core;

namespace Validators.Json.Newtonsoft
{
    class MatchesRegex : IValidator<JToken>
    {
        private readonly Regex _regex;

        public MatchesRegex(Regex regex)
        {
            _regex = regex;
        }

        public string ObjectName => $"match for regex {_regex}";

        public IEnumerable<IValidationError> Validate(JToken target)
        {
            var match = _regex.Match(target.Value<string>() ?? string.Empty);
            if (!match.Success)
            {
                yield return new DefaultValidationError
                {
                    Message = $"Not a {ObjectName}."
                };
            }
        }
    }
}
