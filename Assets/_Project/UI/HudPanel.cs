using System;
using MangaLegends.Core.Stats;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Abilities;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Combat;
using MangaLegends.Simulation.Match;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // Bottom HUD laid out like the LoL client: stats, portrait, spells and bars in the centre, inventory to the right.
    public sealed class HudPanel
    {
        private const int BuffSlots = 10;
        private const int RankPips = 9;

        private static readonly CasterSlot[] SlotOrder = { CasterSlot.Passive, CasterSlot.A, CasterSlot.Z, CasterSlot.E, CasterSlot.R, CasterSlot.Summoner1, CasterSlot.Summoner2, CasterSlot.Recall, CasterSlot.Trinket };
        private static readonly string[] KeyActions = { "", "AbilityA", "AbilityZ", "AbilityE", "AbilityR", "Summoner1", "Summoner2", "Recall", "Trinket" };
        private static readonly string[] ItemActions = { "Item1", "Item2", "Item3", "Item4", "Item5", "Item6" };
        private static readonly (StatType stat, string label)[] StatLines =
        {
            (StatType.AttackDamage, "AD"), (StatType.AbilityPower, "AP"),
            (StatType.Armor, "ARM"), (StatType.MagicResist, "RM"),
            (StatType.AttackSpeed, "VA"), (StatType.AbilityHaste, "ACC"),
            (StatType.CritChance, "CRIT"), (StatType.MoveSpeed, "VD")
        };
        private static readonly Color Frame = new Color(0.03f, 0.04f, 0.06f, 0.92f);
        private static readonly Color Gold = new Color(0.78f, 0.67f, 0.43f);

        private readonly GameObject _root;
        private readonly GameObject _itemsRoot;
        private readonly Image _health;
        private readonly Text _healthText;
        private readonly Text _healthRegen;
        private readonly Image _mana;
        private readonly Text _manaText;
        private readonly Text _manaRegen;
        private readonly Image _xp;
        private readonly Text _level;
        private readonly Text _portrait;
        private readonly Text _gold;
        private readonly Text[] _statValues = new Text[StatLines.Length];
        private readonly Text _center;
        private readonly Image _channel;
        private readonly GameObject _channelRoot;
        private readonly SlotView[] _slots = new SlotView[SlotOrder.Length];
        private readonly Image[] _items = new Image[Inventory.Capacity];
        private readonly Text[] _itemKeys = new Text[ItemActions.Length];
        private readonly Image[] _itemCooldowns = new Image[ItemActions.Length];
        private readonly Outline[] _itemBorders = new Outline[ItemActions.Length];
        private readonly string[] _keyLabels = new string[SlotOrder.Length];
        private readonly string[] _itemKeyLabels = new string[ItemActions.Length];
        private readonly TooltipPanel _tooltip;
        private int _hoverSlot = -1;
        private int _hoverItem = -1;
        private readonly BuffView[] _buffs = new BuffView[BuffSlots];
        private MatchServices _services;

        public HudPanel(Transform canvas)
        {
            RectTransform root = UiFactory.Anchored(canvas, "Hud", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-90f, 0f), new Vector2(860f, 176f));
            _root = root.gameObject;
            UiFactory.Image(root, "Background", Frame);

            Image stats = UiFactory.Box(root, "Stats", Vector2.zero, Vector2.zero, new Vector2(6f, 6f), new Vector2(170f, 164f), new Color(0f, 0f, 0f, 0.55f));
            for (int i = 0; i < StatLines.Length; i++)
            {
                float x = 8f + (i % 2) * 82f;
                float y = -8f - (i / 2) * 38f;
                Text label = UiFactory.Text(stats.transform, "Label", StatLines[i].label, 13, TextAnchor.UpperLeft, Gold);
                UiFactory.Place(label, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y), new Vector2(78f, 16f));
                _statValues[i] = UiFactory.Text(stats.transform, "Value", "0", 18, TextAnchor.UpperLeft, Color.white);
                UiFactory.Place(_statValues[i], new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(x, y - 15f), new Vector2(78f, 22f));
            }

            Image portrait = UiFactory.Box(root, "Portrait", Vector2.zero, Vector2.zero, new Vector2(184f, 34f), new Vector2(116f, 116f), new Color(0.12f, 0.12f, 0.16f));
            _portrait = UiFactory.Text(portrait.transform, "Name", "", 18, TextAnchor.MiddleCenter, Color.white);
            Image badge = UiFactory.Box(portrait.transform, "Level", new Vector2(0.5f, 0f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(40f, 30f), Frame);
            _level = UiFactory.Text(badge.transform, "Text", "1", 20, TextAnchor.MiddleCenter, Gold);
            Image xpBack = UiFactory.Box(root, "Xp", Vector2.zero, Vector2.zero, new Vector2(304f, 34f), new Vector2(8f, 116f), new Color(0f, 0f, 0f, 0.7f));
            _xp = UiFactory.Image(xpBack.transform, "Fill", new Color(0.65f, 0.35f, 1f));

            RectTransform items = UiFactory.Anchored(canvas, "Inventory", new Vector2(0.5f, 0f), new Vector2(0f, 0f), new Vector2(340f, 0f), new Vector2(250f, 176f));
            _itemsRoot = items.gameObject;
            UiFactory.Image(items, "Background", Frame);
            _gold = UiFactory.Text(items, "Gold", "", 22, TextAnchor.MiddleLeft, Gold);
            UiFactory.Place(_gold, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(14f, 6f), new Vector2(170f, 28f));
            for (int i = 0; i < _items.Length; i++)
            {
                int slot = i;
                Image item = UiFactory.Box(items, "Item" + i, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(10f + (i % 3) * 62f, -10f - (i / 3) * 62f), new Vector2(58f, 58f), new Color(0f, 0f, 0f, 0.6f));
                item.gameObject.AddComponent<Button>().onClick.AddListener(() => _services?.Commands.RequestSell(slot));
                UiFactory.Hover(item, () => _hoverItem = slot, () => { if (_hoverItem == slot) _hoverItem = -1; });
                _items[i] = UiFactory.Image(item.transform, "Icon", Color.white);
                _items[i].rectTransform.offsetMin = new Vector2(3f, 3f);
                _items[i].rectTransform.offsetMax = new Vector2(-3f, -3f);
                if (i < ItemActions.Length)
                {
                    _itemCooldowns[i] = UiFactory.Image(item.transform, "Cooldown", new Color(0f, 0f, 0f, 0.7f));
                    _itemKeys[i] = UiFactory.Text(item.transform, "Key", "", 13, TextAnchor.LowerRight, Gold);
                    _itemKeys[i].rectTransform.offsetMax = new Vector2(-3f, 0f);
                    _itemBorders[i] = UiFactory.Border(item, new Color(0.95f, 0.75f, 0.3f), 3f);
                }
                else item.gameObject.SetActive(false);
            }

            for (int i = 0; i < SlotOrder.Length; i++) _slots[i] = new SlotView(SlotOrder[i] >= CasterSlot.Recall ? items : root, i, this);

            UiFactory.Bar(root, "Health", Vector2.zero, Vector2.zero, new Vector2(322f, 30f), new Vector2(530f, 22f), UiFactory.Health, out _health);
            _healthText = UiFactory.Text(_health.transform.parent, "Value", "", 16, TextAnchor.MiddleCenter, Color.white);
            _healthRegen = UiFactory.Text(_health.transform.parent, "Regen", "", 13, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.75f));
            _healthRegen.rectTransform.offsetMax = new Vector2(-6f, 0f);
            UiFactory.Bar(root, "Mana", Vector2.zero, Vector2.zero, new Vector2(322f, 8f), new Vector2(530f, 18f), UiFactory.Mana, out _mana);
            _manaText = UiFactory.Text(_mana.transform.parent, "Value", "", 14, TextAnchor.MiddleCenter, Color.white);
            _manaRegen = UiFactory.Text(_mana.transform.parent, "Regen", "", 12, TextAnchor.MiddleRight, new Color(1f, 1f, 1f, 0.75f));
            _manaRegen.rectTransform.offsetMax = new Vector2(-6f, 0f);

            for (int i = 0; i < BuffSlots; i++) _buffs[i] = new BuffView(root, i);

            _center = UiFactory.Text(canvas, "Center", "", 40, TextAnchor.MiddleCenter, Color.white);
            _center.transform.SetParent(root, true);
            UiFactory.Place(_center, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(90f, 300f), new Vector2(900f, 80f));

            UiFactory.Bar(root, "Channel", new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(90f, 110f), new Vector2(360f, 18f), UiFactory.Accent, out _channel);
            _channelRoot = _channel.transform.parent.gameObject;
            _tooltip = new TooltipPanel(canvas);
        }

        public void HoverSlot(int index, bool on)
        {
            if (on) _hoverSlot = index;
            else if (_hoverSlot == index) _hoverSlot = -1;
        }

        public MatchServices Services => _services;

        public void Bind(MatchServices services) => _services = services;

        public void SetKeyLabels(Func<string, string> labelOf)
        {
            for (int i = 1; i < _slots.Length; i++) _slots[i].SetKey(_keyLabels[i] = labelOf(KeyActions[i]));
            for (int i = 0; i < _itemKeys.Length; i++) _itemKeys[i].text = _itemKeyLabels[i] = labelOf(ItemActions[i]);
        }

        public void Refresh(MatchController match, Unit champion)
        {
            bool visible = match != null && match.Phase == MatchPhase.Playing && champion != null;
            _root.SetActive(visible);
            _itemsRoot.SetActive(visible);
            if (!visible)
            {
                _tooltip.Hide();
                return;
            }

            Health health = champion.Health;
            UiFactory.SetFill(_health, health.Ratio);
            _healthText.text = $"{Mathf.CeilToInt(health.Current)} / {Mathf.CeilToInt(health.Max)}" + (health.ShieldTotal > 0f ? $"  (+{Mathf.CeilToInt(health.ShieldTotal)})" : "");
            _healthRegen.text = $"+{champion.Stats.Get(StatType.HealthRegen):0.0}";
            RefreshResource(champion);

            ChampionProgress progress = champion.GetComponent<ChampionProgress>();
            _portrait.text = champion.Definition.DisplayName;
            _level.text = champion.Stats.Level.ToString();
            Fill(_xp.rectTransform, progress.LevelProgress);
            _gold.text = $"{progress.GoldAmount} or";
            for (int i = 0; i < StatLines.Length; i++) _statValues[i].text = Format(StatLines[i].stat, champion.Stats.Get(StatLines[i].stat));

            _center.text = champion.IsAlive ? "" : $"Réapparition dans {Mathf.CeilToInt(progress.RespawnRemaining)} s";
            for (int i = 0; i < _slots.Length; i++) _slots[i].Refresh(champion, SlotOrder[i]);
            RefreshItems(champion);
            RefreshBuffs(champion);
            RefreshTooltip(champion);

            AbilityCaster caster = champion.Caster;
            _channelRoot.SetActive(caster.IsChanneling);
            if (caster.IsChanneling)
            {
                float total = Mathf.Max(1, caster.ChannelEndTick - caster.ChannelStartTick);
                UiFactory.SetFill(_channel, (champion.Runner.Tick - caster.ChannelStartTick) / total);
            }
        }

        // Mana, or for champions without mana the gauge their data names (Pain's body gauge), or nothing.
        private void RefreshResource(Unit champion)
        {
            bool usesMana = champion.Resource != null && champion.Resource.UsesResource;
            StatusEffectDefinition gauge = champion.Definition is ChampionDefinition definition ? definition.ResourceBarStatus : null;
            _mana.transform.parent.gameObject.SetActive(usesMana || gauge != null);
            if (usesMana)
            {
                _mana.color = UiFactory.Mana;
                UiFactory.SetFill(_mana, champion.Resource.Ratio);
                _manaText.text = $"{Mathf.FloorToInt(champion.Resource.Current)} / {Mathf.FloorToInt(champion.Resource.Max)}";
                _manaRegen.text = $"+{champion.Stats.Get(StatType.ManaRegen):0.0}";
            }
            else if (gauge != null)
            {
                int stacks = champion.Status.StacksOf(gauge);
                _mana.color = new Color(0.95f, 0.55f, 0.15f);
                UiFactory.SetFill(_mana, stacks / (float)Mathf.Max(1, gauge.MaxStacks));
                _manaText.text = $"{stacks} / {gauge.MaxStacks}";
                _manaRegen.text = "";
            }
        }

        private void RefreshTooltip(Unit champion)
        {
            LocalView view = _services != null ? _services.View : null;
            bool detailed = view != null && view.TooltipDetailHeld;
            TooltipText.Content? content = null;
            if (_hoverSlot >= 0)
            {
                AbilityDefinition ability = champion.Caster.AbilityAt(SlotOrder[_hoverSlot]);
                if (ability != null) content = TooltipText.Ability(ability, champion.Caster.RankOf(SlotOrder[_hoverSlot]), champion, _keyLabels[_hoverSlot], detailed);
            }
            else if (_hoverItem >= 0)
            {
                ItemDefinition item = champion.GetComponent<Inventory>().ItemAt(_hoverItem);
                if (item != null) content = TooltipText.Item(item, champion, _hoverItem < _itemKeyLabels.Length ? _itemKeyLabels[_hoverItem] : "", detailed);
            }
            if (view != null) view.HoverRange = content.HasValue ? content.Value.Range : 0f;
            if (content.HasValue) _tooltip.Show(content.Value, detailed);
            else _tooltip.Hide();
        }

        private static string Format(StatType stat, float value)
        {
            switch (stat)
            {
                case StatType.AttackSpeed:
                case StatType.MoveSpeed:
                    return value.ToString("0.00");
                case StatType.CritChance:
                    return $"{Mathf.RoundToInt(value * 100f)} %";
                default:
                    return Mathf.RoundToInt(value).ToString();
            }
        }

        private void RefreshItems(Unit champion)
        {
            Inventory inventory = champion.GetComponent<Inventory>();
            for (int i = 0; i < _items.Length; i++)
            {
                ItemDefinition item = inventory.ItemAt(i);
                _items[i].enabled = item != null;
                if (item != null)
                {
                    _items[i].sprite = item.Icon;
                    _items[i].color = item.Icon != null ? Color.white : new Color(0.45f, 0.4f, 0.3f);
                }
                if (i >= _itemCooldowns.Length) continue;
                float cooldown = item != null && item.Active.Enabled && item.Active.CooldownSeconds > 0f ? inventory.ActiveCooldownRemaining(i) / item.Active.CooldownSeconds : 0f;
                _itemBorders[i].enabled = item != null && item.Active.Enabled;
                Fill(_itemCooldowns[i].rectTransform, cooldown);
            }
        }

        private void RefreshBuffs(Unit champion)
        {
            StatusEffects status = champion.Status;
            int used = 0;
            for (int slot = 0; slot < status.Active.Length && used < _buffs.Length; slot++)
            {
                StatusEffectDefinition definition = status.DefinitionAt(slot);
                if (definition == null) continue;
                NetStatus data = status.Active[slot];
                float remaining = data.ExpiresTick > 0 ? Mathf.Max(0f, (data.ExpiresTick - champion.Runner.Tick) / (float)champion.Runner.TickRate) : -1f;
                _buffs[used++].Show(definition, remaining, data.Stacks);
            }
            for (int i = used; i < _buffs.Length; i++) _buffs[i].Hide();
        }

        private static void Fill(RectTransform rect, float ratio)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = new Vector2(1f, Mathf.Clamp01(ratio));
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private sealed class BuffView
        {
            private readonly GameObject _root;
            private readonly Image _back;
            private readonly Image _icon;
            private readonly Image _sweep;
            private readonly Text _label;
            private readonly Text _stacks;
            private StatusEffectDefinition _shown;
            private float _total;

            public BuffView(Transform parent, int index)
            {
                _back = UiFactory.Box(parent, "Buff" + index, Vector2.zero, Vector2.zero, new Vector2(322f + index * 40f, 182f), new Vector2(36f, 36f), Frame);
                _root = _back.gameObject;
                _icon = UiFactory.Image(_back.transform, "Icon", Color.white);
                _icon.rectTransform.offsetMin = new Vector2(2f, 2f);
                _icon.rectTransform.offsetMax = new Vector2(-2f, -2f);
                _label = UiFactory.Text(_back.transform, "Label", "", 12, TextAnchor.MiddleCenter, Color.white);
                _sweep = UiFactory.Radial(_back.transform, "Sweep", new Color(0f, 0f, 0f, 0.6f));
                _stacks = UiFactory.Text(_back.transform, "Stacks", "", 12, TextAnchor.LowerRight, Gold);
                _root.SetActive(false);
            }

            // A timed effect shows a radial sweep of its remaining time; a permanent one is a plain coloured square.
            public void Show(StatusEffectDefinition definition, float remaining, int stacks)
            {
                _root.SetActive(true);
                if (definition != _shown || remaining > _total) { _shown = definition; _total = Mathf.Max(remaining, 0.01f); }
                bool harmful = definition.IsCrowdControl;
                Color tone = harmful ? new Color(0.85f, 0.3f, 0.3f) : new Color(0.3f, 0.75f, 0.4f);
                _back.color = tone;
                _icon.enabled = definition.Icon != null;
                _icon.sprite = definition.Icon;
                string name = definition.DisplayName ?? "";
                _label.text = definition.Icon != null ? "" : name.Length > 3 ? name.Substring(0, 3) : name;
                _sweep.gameObject.SetActive(remaining >= 0f);
                if (remaining >= 0f) _sweep.fillAmount = 1f - Mathf.Clamp01(remaining / _total);
                _stacks.text = definition.StackDecaySeconds <= 0f && (stacks > 1 || definition.MaxStacks > 1) ? stacks.ToString() : "";
            }

            public void Hide()
            {
                _root.SetActive(false);
                _shown = null;
            }
        }

        private sealed class SlotView
        {
            private readonly Image _icon;
            private readonly Image _cooldown;
            private readonly Text _timer;
            private readonly Text _key;
            private readonly Image[] _pips;
            private readonly GameObject _levelUp;
            private readonly int _slotIndex;
            private readonly HudPanel _hud;
            private readonly Text _charges;
            private readonly Outline _lit;
            private readonly float _size;

            public SlotView(Transform parent, int index, HudPanel hud)
            {
                _hud = hud;
                _slotIndex = index;
                CasterSlot slot = SlotOrder[index];
                Vector2 position;
                float size;
                if (slot == CasterSlot.Passive) { position = new Vector2(322f, 96f); size = 46f; }
                else if (slot <= CasterSlot.R) { position = new Vector2(376f + (index - 1) * 76f, 72f); size = 68f; }
                else if (slot <= CasterSlot.Summoner2) { position = new Vector2(684f + (index - 5) * 56f, 90f); size = 50f; }
                else { position = new Vector2(196f, slot == CasterSlot.Recall ? 50f : 104f); size = 44f; }
                Image frame = UiFactory.Box(parent, "Slot" + index, Vector2.zero, Vector2.zero, position, new Vector2(size, size), new Color(0f, 0f, 0f, 0.6f));
                _icon = UiFactory.Image(frame.transform, "Icon", Color.white);
                _cooldown = UiFactory.Image(frame.transform, "Cooldown", new Color(0f, 0f, 0f, 0.7f));
                _timer = UiFactory.Text(frame.transform, "Timer", "", size > 60f ? 26 : 18, TextAnchor.MiddleCenter, Color.white);
                _key = UiFactory.Text(frame.transform, "Key", "", 14, TextAnchor.LowerRight, Gold);
                _key.rectTransform.offsetMax = new Vector2(-3f, 0f);

                bool ranked = slot >= CasterSlot.A && slot <= CasterSlot.R;
                _size = size;
                _pips = new Image[ranked ? RankPips : 0];
                for (int i = 0; i < _pips.Length; i++)
                    _pips[i] = UiFactory.Box(frame.transform, "Rank" + i, Vector2.zero, new Vector2(0f, 1f), Vector2.zero, new Vector2(4f, 5f), new Color(1f, 1f, 1f, 0.15f));
                _charges = UiFactory.Text(frame.transform, "Charges", "", 16, TextAnchor.UpperLeft, Color.white);
                _charges.rectTransform.offsetMin = new Vector2(4f, 0f);
                _lit = UiFactory.Border(frame, new Color(1f, 0.85f, 0.35f), 3f);
                _lit.enabled = false;
                UiFactory.Hover(frame, () => _hud.HoverSlot(_slotIndex, true), () => _hud.HoverSlot(_slotIndex, false));

                Button plus = UiFactory.Button(frame.transform, "LevelUp", "+", () => _hud.Services?.Commands.RequestLevelUp((int)SlotOrder[_slotIndex]), 24);
                UiFactory.Place(plus, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(34f, 28f));
                _levelUp = plus.gameObject;
            }

            public void SetKey(string key) => _key.text = key;

            public void Refresh(Unit champion, CasterSlot slot)
            {
                AbilityCaster caster = champion.Caster;
                AbilityDefinition ability = caster.AbilityAt(slot);
                _icon.enabled = ability != null;
                if (ability != null)
                {
                    _icon.sprite = ability.Icon;
                    _icon.color = ability.Icon != null ? Color.white : new Color(0.4f, 0.4f, 0.5f);
                }
                int rank = caster.RankOf(slot);
                if (slot == CasterSlot.Passive)
                {
                    RefreshPassive(caster, ability != null);
                    return;
                }
                bool learned = rank > 0;
                float remaining = caster.CooldownRemaining(slot);
                float total = Mathf.Max(0.01f, caster.CooldownTotal(slot));
                bool cooling = learned && !caster.IsReady(slot) && remaining > 0f;
                Fill(_cooldown.rectTransform, !learned ? 1f : cooling ? remaining / total : 0f);
                _timer.text = cooling && remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : "";
                int maxCharges = ability != null && rank > 0 ? ability.ChargesAt(rank) : 1;
                _charges.text = maxCharges > 1 ? caster.Slots[(int)slot].Charges.ToString() : "";
                int maxRank = ability != null ? Mathf.Min(RankPips, ability.MaxRank) : 0;
                float pipWidth = maxRank > 0 ? (_size - 8f) / maxRank : 0f;
                for (int i = 0; i < _pips.Length; i++)
                {
                    _pips[i].gameObject.SetActive(i < maxRank);
                    if (i >= maxRank) continue;
                    _pips[i].rectTransform.anchoredPosition = new Vector2(4f + i * pipWidth, -3f);
                    _pips[i].rectTransform.sizeDelta = new Vector2(pipWidth - 2f, 5f);
                    _pips[i].color = i < rank ? Gold : new Color(1f, 1f, 1f, 0.15f);
                }
                _levelUp.SetActive(caster.CanLevelUp(slot));
            }

            // The passive glows while it is active and otherwise counts down to the moment it will be.
            private void RefreshPassive(AbilityCaster caster, bool hasPassive)
            {
                bool ready = caster.PassiveTimer(out float remaining, out float total);
                _lit.enabled = hasPassive && ready;
                bool counting = !ready && remaining > 0f && total > 0f;
                Fill(_cooldown.rectTransform, counting ? remaining / total : 0f);
                _timer.text = counting ? Mathf.CeilToInt(remaining).ToString() : "";
                _charges.text = "";
                _levelUp.SetActive(false);
            }
        }
    }
}
