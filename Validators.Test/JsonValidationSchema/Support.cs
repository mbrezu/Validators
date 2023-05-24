namespace Validators.Test.JsonValidationSchema
{
    public enum PersonKind { FirstKind, SecondKind }
    public record Person(
        string Name,
        PersonKind Kind,
        int Age,
        bool? IsAdmin,
        DateTime? DateOfBirth);
    public record Property(string Name, object Value, Person[] People);
    public record Team(
        Person Lead,
        Person[] Members,
        decimal? Budget,
        DateTime CreationDate,
        Dictionary<string, Property> Properties);

    public record SelfCycle(SelfCycle? Parent, string Name, int Size);

    public record MutualCycle1(MutualCycle2? Other2, string Name);
    public record MutualCycle2(MutualCycle1? Other1, int Size);
}
