namespace RPGCorruption.Data
{
    public enum StatusEffect
    {
        None,
        Infected,         // Reduce defensa, aumenta daño
        Paralyzed,        // Chance de perder turno
        Poisoned,         // Daño por turno
        Burning,          // Daño por turno mayor
        Frozen,           // No puede actuar
        Stunned,          // Pierde 1 turno
        Purified,         // Inmune a infección temporal
        Strengthened,     // +Attack
        Weakened          // -Attack
    }
}