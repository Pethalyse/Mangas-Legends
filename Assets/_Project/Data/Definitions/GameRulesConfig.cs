using MangaLegends.Core.Abilities;
using MangaLegends.Core.Combat;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Game Rules", fileName = "RULES_Default")]
    public sealed class GameRulesConfig : ScriptableObject
    {
        [Header("Simulation")]
        [SerializeField, Range(10, 128)] private int _tickRate = 30;

        [Header("Combat")]
        [SerializeField, Min(1f)] private float _resistanceScale = 100f;
        [SerializeField, Min(0f)] private float _minimumDamage;
        [SerializeField, Min(1f)] private float _abilityHasteScale = 100f;
        [SerializeField, Min(0f)] private float _outOfCombatSeconds = 5f;
        [SerializeField, Range(0f, 1f)] private float _inCombatRegenRatio = 0.25f;

        [Header("Progression")]
        [SerializeField, Range(1, 30)] private int _maxLevel = 18;
        [SerializeField] private int[] _experienceToNextLevel = { 280, 380, 480, 580, 680, 780, 880, 980, 1080, 1180, 1280, 1380, 1480, 1580, 1680, 1780, 1880 };
        [SerializeField] private int[] _ultimateUnlockLevels = { 6, 11, 16 };
        [SerializeField] private float[] _respawnSecondsByLevel = { 10f };

        [Header("Economy")]
        [SerializeField, Min(0)] private int _startingGold = 500;
        [SerializeField, Min(0f)] private float _passiveGoldPerSecond = 2f;
        [SerializeField, Range(1, 8)] private int _inventorySlots = 6;
        [SerializeField, Range(0f, 1f)] private float _sellRatio = 0.7f;

        public int TickRate => _tickRate;
        public float AbilityHasteScale => _abilityHasteScale;
        public float OutOfCombatSeconds => _outOfCombatSeconds;
        public float InCombatRegenRatio => _inCombatRegenRatio;
        public int MaxLevel => _maxLevel;
        public int[] ExperienceToNextLevel => _experienceToNextLevel;
        public int[] UltimateUnlockLevels => _ultimateUnlockLevels;
        public int StartingGold => _startingGold;
        public float PassiveGoldPerSecond => _passiveGoldPerSecond;
        public int InventorySlots => _inventorySlots;
        public float SellRatio => _sellRatio;

        public CombatRules CombatRules => new CombatRules(_resistanceScale, _minimumDamage);

        public float RespawnSeconds(int level) => RankValues.At(_respawnSecondsByLevel, level);
    }
}
