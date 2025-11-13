namespace RPGCorruption.Data
{
    public enum AIBehavior
    {
        Aggressive,       // Ataca siempre al más débil
        Defensive,        // Prioriza buffear aliados
        Balanced,         // Mix de ataque y defensa
        Random,           // Acciones aleatorias
        Scripted          // Comportamiento especial (para bosses)
    }
}