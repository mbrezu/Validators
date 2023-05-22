﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Validators.NewtonsoftJson;

var schema = ValidationSchema.FromType(typeof(Team), ValidationSchemaOptions.Empty with
{
    AllowExtras = true,
    Required = new TypeAndProperty[]
    {
        new TypeAndProperty(typeof(Person).FullName!, nameof(Person.DateOfBirth))
    },
    Optional = new TypeAndProperty[]
    {
        new TypeAndProperty(typeof(Team).FullName!, nameof(Team.CreationDate))
    }
});
var validator = schema.GetValidator(true);
var json = JsonConvert.SerializeObject(schema, Formatting.Indented);
Console.WriteLine(json);

var doc = JObject.Parse("""
    {
        "Extra": 0,
        "Lead": {
            "Name": "John",
            "Age": 35,
            "Kind": "firstkind",
            "DateOfBirth": "aaa"
        },
        "Members": [
            {
                "Name": "Henry",
                "Age": 30,
                "Kind": "secondKind",
                "DateOfBirth": "yyyy"
            }
        ],
        "Budget": 100,
        "CreationDate": "xxx"
    }
    """);
Console.WriteLine(doc);
var errors = validator.Validate(doc).ToList();
foreach (var error in errors)
{
    Console.WriteLine(error);
}
if (!errors.Any())
{
    Console.WriteLine("All good!");
}

enum PersonKind { FirstKind, SecondKind }
record Person(string Name, PersonKind Kind, int Age, bool? IsAdmin, DateTime? DateOfBirth);

record Team(Person Lead, IEnumerable<Person> Members, decimal? Budget, DateTime CreationDate);
