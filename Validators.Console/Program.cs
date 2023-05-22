using Newtonsoft.Json.Linq;
using static Validators.NewtonsoftJson.Json;

var document = """
    {
        "a": 10,
        "b": "string",
        "c": true,
        //"d": null,
        //"e": [1, 2, "f", true]
    }
    """;

var obj = JObject.Parse(document);

var abValidator = And(
    IsObject,
    ValidKeys("a", "b", "c", "d", "e"),
    RequiredKeys("a", "b", "c"),
    DiveInto("a", IsNumber),
    DiveInto("b", IsString),
    DiveInto("c", IsBoolean),
    DiveInto("d", IsNull),
    DiveInto("e", IsArrayOf(Or(IsNumber, IsString, IsBoolean))));

foreach (var err in abValidator.Validate(obj))
{
    Console.WriteLine(err);
}
