using MangaLegends.Core.Abilities;
using System.Collections.Generic;
using MangaLegends.Core.Combat;
using MangaLegends.Core.Content;
using MangaLegends.Data.Serialization;
using UnityEngine;

namespace MangaLegends.Data.Definitions
{
    [CreateAssetMenu(menuName = "MangaLegends/Game Rules", fileName = "RULES_Default")]
    public sealed class GameRulesConfig : ScriptableObject
    {
        [Header("Simulation")]
        [SerializeField, Range(10, 128)] private int _tickRate = 30;
        [SerializeField, Range(1, 5)] private int _playersPerTeam = 5;

        [Header("Combat")]
        [SerializeField, Min(1f)] private float _resistanceScale = 100f;
        [SerializeField, Min(0f)] private float _minimumDamage;
        [SerializeField, Min(1f)] private float _abilityHasteScale = 100f;
        [SerializeField, Min(0f)] private float _outOfCombatSeconds = 5f;
        [SerializeField, Range(0f, 1f)] private float _inCombatRegenRatio = 0.25f;
        [SerializeField, Range(0f, 1f)] private float _minionDamageToChampionsRatio = 0.6f;
        [SerializeField, Min(0f)] private float _maxAttackSpeed = 2.5f;

        [Header("Progression")]
        [SerializeField, Range(1, 30)] private int _maxLevel = 18;
        [SerializeField] private int[] _experienceToNextLevel = { 280, 380, 480, 580, 680, 780, 880, 980, 1080, 1180, 1280, 1380, 1480, 1580, 1680, 1780, 1880 };
        [SerializeField] private int[] _ultimateUnlockLevels = { 6, 11, 16 };
        [SerializeField, Range(1, 6)] private int _basicAbilityMaxRank = 5;
        [SerializeField] private float[] _respawnSecondsByLevel = { 10f };
        [SerializeField, Min(0f)] private float _experienceShareRadius = 15f;
        [SerializeField] private float[] _experienceShareRatios = { 1f, 0.65f, 0.433f, 0.325f, 0.26f, 0.217f };
        [SerializeField, Min(0)] private int _killExperienceBase = 140;
        [SerializeField, Min(0)] private int _killExperiencePerLevel = 20;

        [Header("Economy")]
        [SerializeField, Min(0)] private int _startingGold = 500;
        [SerializeField, Min(0f)] private float _passiveGoldPerSecond = 2f;
        [SerializeField, Min(0f)] private float _passiveGoldStartSeconds = 65f;
        [SerializeField, Range(1, 8)] private int _inventorySlots = 6;
        [SerializeField, Range(0f, 1f)] private float _sellRatio = 0.7f;
        [SerializeField, Min(0)] private int _championKillGold = 300;
        [SerializeField, Min(0)] private int _firstBloodBonusGold = 100;
        [SerializeField, Range(0f, 1f)] private float _assistGoldShare = 0.5f;
        [SerializeField, Min(0f)] private float _assistWindowSeconds = 10f;
        [SerializeField, Min(0f)] private float _shopRadius = 8f;

        [Header("Jungle")]
        [SerializeField, Min(0f)] private float _monsterKillHealBase = 90f;
        [SerializeField, Min(0f)] private float _monsterKillHealPerLevel = 20f;
        [SerializeField, Min(0f)] private float _monsterKillHealMax = 250f;
        [SerializeField, Min(0f)] private float _monsterLeashResetSeconds = 4f;

        [Header("Base")]
        [SerializeField, Min(0f)] private float _fountainRadius = 8f;
        [SerializeField, Range(0f, 1f)] private float _fountainRegenPerSecond = 0.1f;
        [SerializeField] private AbilityDefinition _recallAbility;

        [Header("Summoner spells")]
        [SerializeField, Range(0, 4)] private int _summonerSpellSlots = 2;
        [SerializeField] private List<SummonerSpellDefinition> _defaultSummonerSpells = new List<SummonerSpellDefinition>();

        [Header("Structures")]
        [SerializeField, Min(0f)] private float _earlyTowerDefenseSeconds;
        [SerializeField] private List<StatModifierData> _earlyTowerModifiers = new List<StatModifierData>();
        [SerializeField, Min(0f)] private float _inhibitorRespawnSeconds = 300f;
        [SerializeField, Min(0f)] private float _towerDamageRampPerShot = 0.4f;
        [SerializeField, Min(0f)] private float _towerDamageRampMax = 1.2f;

        [Header("Minion waves")]
        [SerializeField] private UnitDefinition _meleeMinion;
        [SerializeField] private UnitDefinition _casterMinion;
        [SerializeField] private UnitDefinition _siegeMinion;
        [SerializeField] private UnitDefinition _megaMinion;
        [SerializeField, Range(0, 6)] private int _meleePerWave = 3;
        [SerializeField, Range(0, 6)] private int _casterPerWave = 3;
        [SerializeField, Min(0f)] private float _firstWaveSeconds = 30f;
        [SerializeField] private TimedValue[] _waveIntervalSeconds = { new TimedValue(0f, 30f), new TimedValue(840f, 25f), new TimedValue(1800f, 20f) };
        [SerializeField] private TimedValue[] _siegeEveryNWaves = { new TimedValue(0f, 3f), new TimedValue(840f, 2f), new TimedValue(1500f, 1f) };
        [SerializeField, Min(1)] private int _firstSiegeWave = 3;
        [SerializeField, Min(0f)] private float _minionAggroRange = 5f;
        [SerializeField, Min(0f)] private float _minionSpawnSpacing = 1.2f;

        public int TickRate => _tickRate;
        public int PlayersPerTeam => _playersPerTeam;
        public float AbilityHasteScale => _abilityHasteScale;
        public float OutOfCombatSeconds => _outOfCombatSeconds;
        public float InCombatRegenRatio => _inCombatRegenRatio;
        public float MinionDamageToChampionsRatio => _minionDamageToChampionsRatio;
        public float MaxAttackSpeed => _maxAttackSpeed;
        public int MaxLevel => _maxLevel;
        public int[] ExperienceToNextLevel => _experienceToNextLevel;
        public int[] UltimateUnlockLevels => _ultimateUnlockLevels;
        public int BasicAbilityMaxRank => _basicAbilityMaxRank;
        public float ExperienceShareRadius => _experienceShareRadius;
        public int KillExperienceBase => _killExperienceBase;
        public int KillExperiencePerLevel => _killExperiencePerLevel;
        public int StartingGold => _startingGold;
        public float PassiveGoldPerSecond => _passiveGoldPerSecond;
        public float PassiveGoldStartSeconds => _passiveGoldStartSeconds;
        public int InventorySlots => _inventorySlots;
        public float SellRatio => _sellRatio;
        public int ChampionKillGold => _championKillGold;
        public int FirstBloodBonusGold => _firstBloodBonusGold;
        public float AssistGoldShare => _assistGoldShare;
        public float AssistWindowSeconds => _assistWindowSeconds;
        public float ShopRadius => _shopRadius;
        public float MonsterLeashResetSeconds => _monsterLeashResetSeconds;
        public float MonsterKillHeal(int level) => Mathf.Min(_monsterKillHealMax, _monsterKillHealBase + _monsterKillHealPerLevel * Mathf.Max(0, level - 1));
        public float FountainRadius => _fountainRadius;
        public float FountainRegenPerSecond => _fountainRegenPerSecond;
        public AbilityDefinition RecallAbility => _recallAbility;
        public int SummonerSpellSlots => _summonerSpellSlots;
        public IReadOnlyList<SummonerSpellDefinition> DefaultSummonerSpells => _defaultSummonerSpells;
        public float EarlyTowerDefenseSeconds => _earlyTowerDefenseSeconds;
        public IReadOnlyList<StatModifierData> EarlyTowerModifiers => _earlyTowerModifiers;
        public float InhibitorRespawnSeconds => _inhibitorRespawnSeconds;
        public float TowerDamageRampPerShot => _towerDamageRampPerShot;
        public float TowerDamageRampMax => _towerDamageRampMax;
        public UnitDefinition MeleeMinion => _meleeMinion;
        public UnitDefinition CasterMinion => _casterMinion;
        public UnitDefinition SiegeMinion => _siegeMinion;
        public UnitDefinition MegaMinion => _megaMinion;
        public int MeleePerWave => _meleePerWave;
        public int CasterPerWave => _casterPerWave;
        public float FirstWaveSeconds => _firstWaveSeconds;
        public int FirstSiegeWave => _firstSiegeWave;
        public float MinionAggroRange => _minionAggroRange;
        public float MinionSpawnSpacing => _minionSpawnSpacing;
        public float WaveIntervalAt(float elapsedSeconds) => TimedValue.At(_waveIntervalSeconds, elapsedSeconds);
        public int SiegeEveryNWavesAt(float elapsedSeconds) => System.Math.Max(1, (int)System.Math.Round(TimedValue.At(_siegeEveryNWaves, elapsedSeconds)));

        public CombatRules CombatRules => new CombatRules(_resistanceScale, _minimumDamage);

        public float RespawnSeconds(int level) => RankValues.At(_respawnSecondsByLevel, level);

        public float ExperienceShareRatio(int championsInRange) => RankValues.At(_experienceShareRatios, championsInRange);

        public int ExperienceToReach(int level)
        {
            int total = 0;
            for (int i = 0; i < level - 1 && i < _experienceToNextLevel.Length; i++) total += _experienceToNextLevel[i];
            return total;
        }

        public void Validate(ValidationReport report)
        {
            if (_meleeMinion == null || _casterMinion == null) report.Warning(name, "Melee or caster minion is not assigned.");
            if (_siegeMinion == null) report.Warning(name, "No siege minion assigned.");
            if (_megaMinion == null) report.Warning(name, "No mega minion assigned.");
            if (_recallAbility == null) report.Warning(name, "No recall ability assigned.");
            if (_inhibitorRespawnSeconds <= 0f) report.Warning(name, "Inhibitor respawn time is not set.");
            if (_experienceToNextLevel.Length < _maxLevel - 1) report.Error(name, "Experience table is shorter than the level cap.");
        }
    }
}
