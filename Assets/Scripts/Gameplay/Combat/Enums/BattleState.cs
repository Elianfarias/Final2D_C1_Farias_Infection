namespace RPGCorruption.Combat
{
    public enum BattleState
    {
        Start,          // Iniciando batalla
        PlayerTurn,     // Turno del jugador (eligiendo acción)
        EnemyTurn,      // Turno del enemigo
        Busy,           // Ejecutando una acción (animación, daño, etc)
        Victory,        // Batalla ganada
        Defeat,         // Batalla perdida
        Escaped         // Jugador escapó
    }
}