using System;
using System.Collections.Generic;

namespace UniQuest.Contracts
{
    /// <summary>Résumé d'un emplacement de sauvegarde, pour l'affichage uniquement.</summary>
    public struct SaveSlotInfo
    {
        public int Index;
        public bool IsEmpty;
        public string ZoneName;
        public DateTime SavedAt;
        public TimeSpan PlayTime;
        public int Seed;
        public string PartySummary;     // ex. "Trochu Niv. 6 · Docteur Bec Niv. 5"
    }

    /// <summary>Sauvegarde/chargement, implémenté côté gameplay (sérialisation JSON).</summary>
    public interface ISaveService
    {
        int SlotCount { get; }
        IReadOnlyList<SaveSlotInfo> GetSlots();
        bool Save(int slotIndex);
        bool Load(int slotIndex);
        bool HasAnySave { get; }
        int LastUsedSlot { get; }       // -1 si aucune
    }
}
