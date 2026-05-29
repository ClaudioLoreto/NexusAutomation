namespace Nexus.Core.Enums;

/// <summary>
/// Built-in niche typologies. The DB enforces a unique index on this column,
/// so each value identifies exactly one row in the <c>Niches</c> table.
/// Add new typologies here AND in the seeding block of <c>NexusDbContext</c>
/// (with a fresh, unused integer value) so the migration stays additive.
/// </summary>
public enum NicheType
{
    Finance = 1,
    TechAndAi = 2,
    LegalAndCourt = 3,
    StoriaAntica = 4,
    BrainrotFacts = 5,
    WholesomeAnimals = 6
}
