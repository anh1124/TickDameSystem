namespace TickCombat.Combat
{
    public enum DamageType
    {
        Physical,
        Magical,
        True,              // bỏ qua def, KHÔNG bỏ qua shield
        PercentCurrentHP   // % trên HP snapshot
    }
}
