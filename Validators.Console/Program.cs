using Newtonsoft.Json.Linq;
using static Validators.Json.Newtonsoft.Library;

var target = JObject.Parse("""
    {
        "a": 1,
        "b": 2
    }
    """);
var validator = And(
    IsObject,
    HasRequiredKeys("a", "b"),
    HasValidKeys("a", "b"),
    DiveInto("a", IsNumber),
    DiveInto("b", IsNumber));

var errors = validator.Validate(target);

if (!errors.Any())
{
    Console.WriteLine("All good!");
}
else
{
    foreach (var error in errors)
    {
        Console.WriteLine(error);
    }
}