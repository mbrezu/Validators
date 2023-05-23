# Validators

A simple library of "validator combinators" with some useful primitives defined for the [NewtonSoft JSON](https://www.newtonsoft.com/json) [DOM](https://www.newtonsoft.com/json/help/html/N_Newtonsoft_Json_Linq.htm).

## Getting Started

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
```

The content above is from [`Program.cs`](Validators.Console/Program.cs).
See also:
* [A C# Notebook](Notebook.dib)
* [tests](Validators.Test)
  * [Basic validators](Validators.Test\JsonBasic.cs)
  * [Automatic validator generation for non-trivial objects](Validators.Test\JsonValidationSchema\NestedTwo.cs)
  * [Extract the invalid content](Validators.Test\ContentExtraction.cs)
  * [Generic dictionaries](Validators.Test\JsonDictionary.cs)

## License

[License](LICENSE) is BSD 2-clause.