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
    public sealed class HudPanel
    {
        private static readonly CasterSlot[] SlotOrder = { CasterSlot.Passive, CasterSlot.A, CasterSlot.Z, CasterSlot.E, CasterSlot.R, CasterSlot.Summoner1, CasterSlot.Summoner2, CasterSlot.Recall };
        private static readonly string[] Keys = { "P", "A", "Z", "E", "R", "D", "F", "B" };

        private readonly GameObject _root;
        private readonly Image _health;
        private readonly Text _healthText;
        private readonly Image _mana;
        private readonly Text _manaText;
        private readonly Image _xp;
        private readonly Text _level;
        private readonly Text _gold;
        private readonly Text _stats;
        private readonly Text _clock;
        private readonly Text _center;
        private readonly Image _channel;
        private readonly GameObject _channelRoot;
        private readonly SlotView[] _slots = new SlotView[SlotOrder.Length];
        private readonly Image[] _items = new Image[Inventory.Capacity];
        private MatchServices _services;

        public HudPanel(Transform canvas)
        {
            RectTransform root = UiFactory.Anchored(canvas, "Hud", new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), Vector2.zero, new Vector2(1100f, 210f));
            _root = root.gameObject;
            UiFactory.Image(root, "Background", UiFactory.Panel);

            for (int i = 0; i < SlotOrder.Length; i++) _slots[i] = new SlotView(root, i, Keys[i], this);

            UiFactory.Bar(root, "Health", new Vector2(0f, 0f), Vector2.zero, new Vector2(140f, 40f), new Vector2(620f, 26f), UiFactory.Health, out _health);
            _healthText = UiFactory.Text(_health.transform.parent, "Value", "", 18, TextAnchor.MiddleCenter, Color.white);
            UiFactory.Bar(root, "Mana", new Vector2(0f, 0f), Vector2.zero, new Vector2(140f, 10f), new Vector2(620f, 24f), UiFactory.Mana, out _mana);
            _manaText = UiFactory.Text(_mana.transform.parent, "Value", "", 16, TextAnchor.MiddleCenter, Color.white);

            Image portrait = UiFactory.Box(root, "Level", Vector2.zero, Vector2.zero, new Vector2(10f, 10f), new Vector2(120f, 120f), new Color(0f, 0f, 0f, 0.5f));
            _level = UiFactory.Text(portrait.transform, "Text", "1", 48, TextAnchor.MiddleCenter, UiFactory.Accent);
            UiFactory.Bar(root, "Xp", Vector2.zero, Vector2.zero, new Vector2(10f, 134f), new Vector2(120f, 10f), new Color(0.7f, 0.4f, 1f), out _xp);

            _gold = UiFactory.Text(root, "Gold", "", 26, TextAnchor.MiddleLeft, UiFactory.Accent);
            UiFactory.Place(_gold, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-10f, 150f), new Vector2(300f, 40f));
            for (int i = 0; i < _items.Length; i++)
            {
                int slot = i;
                Image item = UiFactory.Box(root, "Item" + i, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-250f + (i % 3) * 82f, 90f - (i / 3) * 82f), new Vector2(76f, 76f), new Color(0f, 0f, 0f, 0.6f));
                item.gameObject.AddComponent<Button>().onClick.AddListener(() => _services?.Commands.RequestSell(slot));
                _items[i] = UiFactory.Image(item.transform, "Icon", Color.white);
                _items[i].rectTransform.offsetMin = new Vector2(4f, 4f);
                _items[i].rectTransform.offsetMax = new Vector2(-4f, -4f);
                if (i >= 6) item.gameObject.SetActive(false);
            }

            Image top = UiFactory.Box(canvas, "Top", new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(520f, 50f), UiFactory.Panel);
            _clock = UiFactory.Text(top.transform, "Clock", "00:00", 26, TextAnchor.MiddleLeft, Color.white);
            _clock.rectTransform.offsetMin = new Vector2(16f, 0f);
            _stats = UiFactory.Text(top.transform, "Stats", "", 24, TextAnchor.MiddleRight, Color.white);
            _stats.rectTransform.offsetMax = new Vector2(-16f, 0f);
            top.transform.SetParent(root, true);

            _center = UiFactory.Text(canvas, "Center", "", 40, TextAnchor.MiddleCenter, Color.white);
            _center.transform.SetParent(root, true);
            UiFactory.Place(_center, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 340f), new Vector2(900f, 80f));

            UiFactory.Bar(root, "Channel", new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 120f), new Vector2(360f, 18f), UiFactory.Accent, out _channel);
            _channelRoot = _channel.transform.parent.gameObject;
        }

        public MatchServices Services => _services;

        public void Bind(MatchServices services) => _services = services;

        public void Refresh(MatchController match, Unit champion)
        {
            bool visible = match != null && match.Phase == MatchPhase.Playing && champion != null;
            _root.SetActive(visible);
            if (!visible) return;

            Health health = champion.Health;
            UiFactory.SetFill(_health, health.Ratio);
            _healthText.text = $"{Mathf.CeilToInt(health.Current)} / {Mathf.CeilToInt(health.Max)}" + (health.ShieldTotal > 0f ? $"  (+{Mathf.CeilToInt(health.ShieldTotal)})" : "");
            if (champion.Resource != null && champion.Resource.UsesResource)
            {
                UiFactory.SetFill(_mana, champion.Resource.Ratio);
                _manaText.text = $"{Mathf.FloorToInt(champion.Resource.Current)} / {Mathf.FloorToInt(champion.Resource.Max)}";
            }

            ChampionProgress progress = champion.GetComponent<ChampionProgress>();
            _level.text = champion.Stats.Level.ToString();
            UiFactory.SetFill(_xp, progress.LevelProgress);
            _gold.text = $"{progress.GoldAmount} or";
            _stats.text = $"{progress.Kills}/{progress.Deaths}/{progress.Assists}   CS {progress.MinionKills}   AD {Mathf.RoundToInt(champion.Stats.Get(StatType.AttackDamage))}  AP {Mathf.RoundToInt(champion.Stats.Get(StatType.AbilityPower))}";
            int seconds = Mathf.FloorToInt(match.ElapsedSeconds);
            _clock.text = $"{seconds / 60:00}:{seconds % 60:00}";

            _center.text = champion.IsAlive ? "" : $"Réapparition dans {Mathf.CeilToInt(progress.RespawnRemaining)} s";
            for (int i = 0; i < _slots.Length; i++) _slots[i].Refresh(champion, SlotOrder[i]);

            Inventory inventory = champion.GetComponent<Inventory>();
            for (int i = 0; i < _items.Length; i++)
            {
                ItemDefinition item = inventory.ItemAt(i);
                _items[i].enabled = item != null;
                if (item != null) _items[i].sprite = item.Icon;
            }

            AbilityCaster caster = champion.Caster;
            _channelRoot.SetActive(caster.IsChanneling);
            if (caster.IsChanneling)
            {
                float total = Mathf.Max(1, caster.ChannelEndTick - caster.ChannelStartTick);
                UiFactory.SetFill(_channel, (champion.Runner.Tick - caster.ChannelStartTick) / total);
            }
        }

        private sealed class SlotView
        {
            private readonly Image _icon;
            private readonly Image _cooldown;
            private readonly Text _timer;
            private readonly Text _rank;
            private readonly GameObject _levelUp;
            private readonly int _slotIndex;
            private readonly HudPanel _hud;

            public SlotView(Transform root, int index, string key, HudPanel hud)
            {
                _hud = hud;
                _slotIndex = index;
                bool small = index == 0 || index >= 5;
                float size = small ? 56f : 80f;
                float x = index == 0 ? 140f : index <= 4 ? 210f + (index - 1) * 92f : 590f + (index - 5) * 66f;
                Image frame = UiFactory.Box(root, "Slot" + index, Vector2.zero, Vector2.zero, new Vector2(x, 80f), new Vector2(size, size), new Color(0f, 0f, 0f, 0.6f));
                _icon = UiFactory.Image(frame.transform, "Icon", Color.white);
                _cooldown = UiFactory.Image(frame.transform, "Cooldown", new Color(0f, 0f, 0f, 0.7f));
                _timer = UiFactory.Text(frame.transform, "Timer", "", small ? 20 : 28, TextAnchor.MiddleCenter, Color.white);
                Text label = UiFactory.Text(frame.transform, "Key", key, 16, TextAnchor.LowerRight, UiFactory.Accent);
                label.rectTransform.offsetMax = new Vector2(-3f, 0f);
                _rank = UiFactory.Text(frame.transform, "Rank", "", 16, TextAnchor.UpperLeft, Color.white);
                _rank.rectTransform.offsetMin = new Vector2(3f, 0f);
                Button plus = UiFactory.Button(frame.transform, "LevelUp", "+", () => _hud.Services?.Commands.RequestLevelUp((int)SlotOrder[_slotIndex]), 26);
                UiFactory.Place(plus, new Vector2(0.5f, 1f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(36f, 30f));
                _levelUp = plus.gameObject;
            }

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
                bool learned = rank > 0 || slot == CasterSlot.Passive;
                float remaining = caster.CooldownRemaining(slot);
                float total = Mathf.Max(0.01f, caster.CooldownTotal(slot));
                bool cooling = learned && !caster.IsReady(slot) && slot != CasterSlot.Passive;
                RectTransform overlay = _cooldown.rectTransform;
                overlay.anchorMin = Vector2.zero;
                overlay.anchorMax = new Vector2(1f, !learned ? 1f : cooling ? Mathf.Clamp01(remaining / total) : 0f);
                overlay.offsetMin = Vector2.zero;
                overlay.offsetMax = Vector2.zero;
                _timer.text = cooling && remaining > 0f ? Mathf.CeilToInt(remaining).ToString() : "";
                _rank.text = slot >= CasterSlot.A && slot <= CasterSlot.R && rank > 0 ? rank.ToString() : "";
                _levelUp.SetActive(caster.CanLevelUp(slot));
            }
        }
    }
}
