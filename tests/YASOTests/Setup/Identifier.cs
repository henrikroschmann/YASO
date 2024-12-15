using YASO.Abstractions;

namespace YASOTests.Setup;

internal class Identifier : ISagaIdentifier
{
    public int Id { get; set; }
}