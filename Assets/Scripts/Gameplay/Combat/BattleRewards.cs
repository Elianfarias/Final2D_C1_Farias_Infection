using System.Collections.Generic;
using UnityEngine;
using RPGCorruption.Data;
using RPGCorruption.Data.Runtime;

namespace RPGCorruption.Combat
{
    public class BattleRewards
    {
        public int TotalExp { get; private set; }
        public int TotalGold { get; private set; }
        public List<ItemData> ItemsDropped { get; private set; }
        public List<CharacterInstance> LeveledUpCharacters { get; private set; }

        /// <summary>
        /// Calcula las recompensas basadas en los enemigos derrotados
        /// </summary>
        public void CalculateRewards(List<EnemyData> defeatedEnemies)
        {
            TotalExp = 0;
            TotalGold = 0;
            ItemsDropped = new List<ItemData>();

            foreach (var enemy in defeatedEnemies)
            {
                // Sumar experiencia
                TotalExp += enemy.ExpReward;

                // Sumar oro
                TotalGold += enemy.GoldReward;

                // Intentar drop de items (si el enemigo tiene)
                if (enemy.DropTable != null && enemy.DropTable.Count > 0)
                    TryDropItem(enemy);
            }

            Debug.Log($"Rewards calculated: {TotalExp} EXP, {TotalGold} Gold, {ItemsDropped.Count} items");
        }

        /// <summary>
        /// Intenta dropear un item del enemigo
        /// </summary>
        private void TryDropItem(EnemyData enemy)
        {
            foreach (var drop in enemy.DropTable)
            {
                if (drop.Item == null) continue;

                float roll = Random.Range(0f, 100f);
                if (roll <= drop.DropChance)
                {
                    ItemsDropped.Add(drop.Item);
                    Debug.Log($"Item dropped: {drop.Item.ItemName}");
                }
            }
        }

        /// <summary>
        /// Otorga las recompensas al grupo
        /// </summary>
        public void GrantRewards(PartyData party)
        {
            if (party == null)
            {
                Debug.LogError("Cannot grant rewards: Party is null!");
                return;
            }

            // Otorgar oro
            party.AddGold(TotalGold);

            // Otorgar experiencia y detectar level ups
            LeveledUpCharacters = party.GainExperienceAll(TotalExp);

            // TODO: Agregar items al inventario cuando esté implementado
            // inventory.AddItems(ItemsDropped);

            Debug.Log($"Rewards granted! {LeveledUpCharacters.Count} character(s) leveled up!");
        }

        /// <summary>
        /// Obtiene un resumen de texto de las recompensas
        /// </summary>
        public string GetRewardsSummary()
        {
            string summary = $"Victory!\n\n";
            summary += $"EXP gained: {TotalExp}\n";
            summary += $"Gold gained: {TotalGold}\n";

            if (ItemsDropped.Count > 0)
            {
                summary += $"\nItems obtained:\n";
                foreach (var item in ItemsDropped)
                {
                    summary += $"- {item.ItemName}\n";
                }
            }

            if (LeveledUpCharacters != null && LeveledUpCharacters.Count > 0)
            {
                summary += $"\nLevel Up!\n";
                foreach (var character in LeveledUpCharacters)
                {
                    summary += $"- {character.Template.CharacterName} reached Level {character.Level}!\n";
                }
            }

            return summary;
        }
    }
}