using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RPGCorruption.Data.Runtime
{
    /// <summary>
    /// Inventario runtime del jugador.
    /// Gestiona items con cantidades y limitaciones de espacio.
    /// </summary>
    [Serializable]
    public class InventoryInstance
    {
        [Header("Almacenamiento")]
        [SerializeField] private List<ItemStack> items = new List<ItemStack>();

        // Properties
        public List<ItemStack> Items => items;
        public int UsedSlots => items.Count;
        public int FreeSlots => GameConfig.Instance.MaxInventorySlots - UsedSlots;
        public bool IsFull => UsedSlots >= GameConfig.Instance.MaxInventorySlots;
        public bool IsEmpty => items.Count == 0;

        /// <summary>
        /// Constructor por defecto
        /// </summary>
        public InventoryInstance()
        {
            items = new List<ItemStack>();
        }

        #region Item Management

        /// <summary>
        /// Añade un item al inventario
        /// </summary>
        public bool AddItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

            // Buscar si el item ya existe en el inventario
            var existingStack = items.Find(i => i.ItemId == itemId);

            if (existingStack != null)
            {
                // El item ya existe, añadir a la pila
                existingStack.Quantity += quantity;
                Debug.Log($"Added {quantity}x {itemId}. Total: {existingStack.Quantity}");
                return true;
            }
            else
            {
                // Verificar si hay espacio
                if (IsFull)
                {
                    Debug.LogWarning("Inventory is full!");
                    return false;
                }

                // Crear nueva pila
                items.Add(new ItemStack(itemId, quantity));
                Debug.Log($"Added {quantity}x {itemId} to inventory.");
                return true;
            }
        }

        /// <summary>
        /// Remueve un item del inventario
        /// </summary>
        public bool RemoveItem(string itemId, int quantity = 1)
        {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) return false;

            var stack = items.Find(i => i.ItemId == itemId);
            if (stack == null)
            {
                Debug.LogWarning($"Item {itemId} not found in inventory!");
                return false;
            }

            if (stack.Quantity < quantity)
            {
                Debug.LogWarning($"Not enough {itemId}! Need {quantity}, have {stack.Quantity}");
                return false;
            }

            stack.Quantity -= quantity;
            Debug.Log($"Removed {quantity}x {itemId}. Remaining: {stack.Quantity}");

            // Si la cantidad llega a 0, remover el stack
            if (stack.Quantity <= 0)
            {
                items.Remove(stack);
            }

            return true;
        }

        /// <summary>
        /// Obtiene la cantidad de un item específico
        /// </summary>
        public int GetItemCount(string itemId)
        {
            var stack = items.Find(i => i.ItemId == itemId);
            return stack?.Quantity ?? 0;
        }

        /// <summary>
        /// Verifica si el inventario contiene un item
        /// </summary>
        public bool HasItem(string itemId, int minimumQuantity = 1)
        {
            return GetItemCount(itemId) >= minimumQuantity;
        }

        /// <summary>
        /// Ordena el inventario (por nombre, rareza, tipo, etc.)
        /// </summary>
        public void SortInventory(SortType sortType = SortType.ByName)
        {
            switch (sortType)
            {
                case SortType.ByName:
                    items.Sort((a, b) => string.Compare(a.ItemId, b.ItemId, StringComparison.Ordinal));
                    break;
                case SortType.ByQuantity:
                    items.Sort((a, b) => b.Quantity.CompareTo(a.Quantity));
                    break;
                    // Otros tipos de ordenamiento requieren acceso a ItemData
            }
        }

        /// <summary>
        /// Limpia items con cantidad 0 o inválidos
        /// </summary>
        public void CleanupInventory()
        {
            items.RemoveAll(i => i.Quantity <= 0 || string.IsNullOrEmpty(i.ItemId));
        }

        #endregion

        #region Filtering

        /// <summary>
        /// Obtiene todos los consumibles
        /// </summary>
        public List<ItemStack> GetConsumables(Dictionary<string, ItemData> itemDatabase)
        {
            return items.Where(stack =>
            {
                if (itemDatabase.TryGetValue(stack.ItemId, out ItemData data))
                {
                    return data.Type == ItemType.Consumable;
                }
                return false;
            }).ToList();
        }

        /// <summary>
        /// Obtiene todo el equipamiento
        /// </summary>
        public List<ItemStack> GetEquipment(Dictionary<string, ItemData> itemDatabase)
        {
            return items.Where(stack =>
            {
                if (itemDatabase.TryGetValue(stack.ItemId, out ItemData data))
                {
                    return data.Type == ItemType.Weapon ||
                           data.Type == ItemType.Armor ||
                           data.Type == ItemType.Accessory;
                }
                return false;
            }).ToList();
        }

        /// <summary>
        /// Obtiene items clave
        /// </summary>
        public List<ItemStack> GetKeyItems(Dictionary<string, ItemData> itemDatabase)
        {
            return items.Where(stack =>
            {
                if (itemDatabase.TryGetValue(stack.ItemId, out ItemData data))
                {
                    return data.Type == ItemType.KeyItem;
                }
                return false;
            }).ToList();
        }

        #endregion

        #region Batch Operations

        /// <summary>
        /// Añade múltiples items al inventario
        /// </summary>
        public int AddMultipleItems(Dictionary<string, int> itemsToAdd)
        {
            int addedCount = 0;

            foreach (var kvp in itemsToAdd)
            {
                if (AddItem(kvp.Key, kvp.Value))
                {
                    addedCount++;
                }
            }

            return addedCount;
        }

        /// <summary>
        /// Añade drops de batalla
        /// </summary>
        public void AddLoot(List<ItemData> lootItems)
        {
            if (lootItems == null || lootItems.Count == 0) return;

            foreach (var item in lootItems)
            {
                if (item != null)
                {
                    AddItem(item.ItemId, 1);
                }
            }
        }

        #endregion

        #region Sell/Buy

        /// <summary>
        /// Calcula el valor de venta total del inventario
        /// </summary>
        public int CalculateTotalValue(Dictionary<string, ItemData> itemDatabase)
        {
            int totalValue = 0;

            foreach (var stack in items)
            {
                if (itemDatabase.TryGetValue(stack.ItemId, out ItemData data))
                {
                    totalValue += data.SellPrice * stack.Quantity;
                }
            }

            return totalValue;
        }

        #endregion

        #region Serialization Helper

        /// <summary>
        /// Valida el inventario contra la base de datos de items
        /// </summary>
        public void ValidateInventory(Dictionary<string, ItemData> itemDatabase)
        {
            items.RemoveAll(stack =>
            {
                if (string.IsNullOrEmpty(stack.ItemId))
                {
                    Debug.LogWarning("Found item with empty ID, removing...");
                    return true;
                }

                if (!itemDatabase.ContainsKey(stack.ItemId))
                {
                    Debug.LogWarning($"Item {stack.ItemId} not found in database, removing...");
                    return true;
                }

                return false;
            });
        }

        #endregion
    }

    /// <summary>
    /// Representa una pila de items del mismo tipo
    /// </summary>
    [Serializable]
    public class ItemStack
    {
        [SerializeField] private string itemId;
        [SerializeField] private int quantity;

        public string ItemId => itemId;
        public int Quantity
        {
            get => quantity;
            set => quantity = Mathf.Max(0, value);
        }

        public ItemStack(string itemId, int quantity)
        {
            this.itemId = itemId;
            this.quantity = Mathf.Max(0, quantity);
        }
    }
}