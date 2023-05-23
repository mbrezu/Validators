# Validators

A simple library of "validator combinators" with some useful primitives defined for the [NewtonSoft JSON](https://www.newtonsoft.com/json) [DOM](https://www.newtonsoft.com/json/help/html/N_Newtonsoft_Json_Linq.htm).

## Getting Started

See [`Program.cs`](Validators.Console/Program.cs) (and [tests](Validators.Test)):

```
using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

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

foreach (var error in errors)
{
    Console.WriteLine(error);
}
if (!errors.Any())
{
    Console.WriteLine("All good!");
}
```