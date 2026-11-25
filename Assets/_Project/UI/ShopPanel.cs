using System.Collections.Generic;
using System.Linq;
using MangaLegends.Core.Economy;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    // LoL-style shop: items by category on the left (left click selects, right click buys), the selected item on the
    // right with what it builds into, its recipe tree, its stats and the Buy / Sell buttons.
    public sealed class ShopPanel
    {
        private const float Cell = 58f;
        private const float GridWidth = 700f;
        private static readonly Color Border = new Color(0.78f, 0.67f, 0.43f);
        private static readonly Color Back = new Color(0.02f, 0.06f, 0.08f, 1f);
        private static readonly (ItemTier tier, string title)[] Sections =
        {
            (ItemTier.Basic, "DÉPART ET BASIQUES"), (ItemTier.Component, "ÉPIQUES"), (ItemTier.Complete, "LÉGENDAIRES"), (ItemTier.Consumable, "CONSOMMABLES")
        };

        private readonly GameObject _root;
        private readonly RectTransform _grid;
        private readonly RectTransform _detail;
        private readonly Text _status;
        private readonly Text _name;
        private readonly Text _price;
        private readonly Text _body;
        private readonly Button _buy;
        private readonly Button _sell;
        private readonly InputField _search;
        private readonly List<(ItemDefinition item, Text price, Image frame)> _entries = new List<(ItemDefinition, Text, Image)>();
        private readonly List<GameObject> _detailNodes = new List<GameObject>();
        private readonly List<Text> _trinketLabels = new List<Text>();
        private MatchServices _services;
        private ItemDefinition _selected;
        private Unit _champion;
        private bool _open;
        private string _builtFilter;

        public ShopPanel(Transform canvas)
        {
            Image border = UiFactory.Box(canvas, "Shop", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 40f), new Vector2(1200f, 700f), Border);
            _root = border.gameObject;
            Image panel = UiFactory.Image(border.transform, "Back", Back);
            panel.rectTransform.offsetMin = new Vector2(2f, 2f);
            panel.rectTransform.offsetMax = new Vector2(-2f, -2f);
            Transform root = panel.transform;

            Text title = UiFactory.Text(root, "Title", "TOUS LES OBJETS", 22, TextAnchor.MiddleLeft, Border);
            UiFactory.Place(title, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f, -10f), new Vector2(300f, 34f));
            _search = UiFactory.Input(root, "Search", "", "Rechercher…");
            UiFactory.Place(_search, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(320f, -10f), new Vector2(400f, 34f));
            Button close = UiFactory.Button(root, "Close", "X", () => _services?.View.ToggleShop(), 22);
            UiFactory.Place(close, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-10f, -10f), new Vector2(40f, 34f));

            _grid = UiFactory.Rect(root, "Grid", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(16f, 70f), new Vector2(16f + GridWidth, -54f));
            Image divider = UiFactory.Box(root, "Divider", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(GridWidth + 30f, 0f), new Vector2(2f, 660f), new Color(Border.r, Border.g, Border.b, 0.4f));
            divider.raycastTarget = false;
            _detail = UiFactory.Rect(root, "Detail", new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(GridWidth + 44f, 70f), new Vector2(-16f, -54f));

            _name = UiFactory.Text(_detail, "Name", "", 22, TextAnchor.UpperLeft, Color.white);
            UiFactory.Place(_name, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 250f), new Vector2(300f, 30f));
            _price = UiFactory.Text(_detail, "Price", "", 20, TextAnchor.UpperRight, Border);
            UiFactory.Place(_price, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 250f), new Vector2(140f, 30f));
            _body = UiFactory.Text(_detail, "Body", "", 16, TextAnchor.UpperLeft, new Color(0.85f, 0.87f, 0.9f));
            _body.supportRichText = true;
            UiFactory.Place(_body, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 56f), new Vector2(420f, 190f));
            _buy = UiFactory.Button(_detail, "Buy", "ACHETER", () => Buy(_selected), 20);
            UiFactory.Place(_buy, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(260f, 44f));
            _sell = UiFactory.Button(_detail, "Sell", "VENDRE", Sell, 18);
            UiFactory.Place(_sell, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(150f, 44f));

            _status = UiFactory.Text(root, "Status", "", 16, TextAnchor.MiddleLeft, new Color(1f, 1f, 1f, 0.7f));
            UiFactory.Place(_status, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 12f), new Vector2(380f, 24f));
            _root.SetActive(false);
        }

        public void Bind(MatchServices services)
        {
            if (_services == services) return;
            _services = services;
            BuildTrinkets();
        }

        public void Refresh(Unit champion)
        {
            if (_services == null) return;
            if (_services.View.TakeShopToggle()) _open = !_open;
            bool visible = _open && champion != null;
            _root.SetActive(visible);
            _services.View.ShopOpen = visible;
            if (!visible) return;
            _champion = champion;
            if (_builtFilter != _search.text) BuildGrid(_search.text);

            Inventory inventory = champion.GetComponent<Inventory>();
            bool canShop = inventory.CanShop;
            _status.text = canShop ? "Clic gauche : choisir.  Clic droit : acheter.  Échap : fermer." : "Retournez à la fontaine pour acheter.";
            var choices = _services.Rules.TrinketChoices;
            var current = champion.Caster.AbilityAt(Simulation.Abilities.CasterSlot.Trinket);
            for (int i = 0; i < _trinketLabels.Count && i < choices.Count; i++)
                _trinketLabels[i].color = choices[i] == current ? UiFactory.Accent : canShop ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            foreach (var entry in _entries)
            {
                PurchaseQuote quote = inventory.Quote(entry.item);
                entry.price.text = quote.Cost.ToString();
                entry.price.color = quote.CanBuy && canShop ? UiFactory.Accent : new Color(0.7f, 0.45f, 0.45f);
                entry.frame.color = entry.item == _selected ? new Color(0.2f, 0.45f, 0.5f) : new Color(0f, 0f, 0f, 0.6f);
            }
            if (_selected != null)
            {
                PurchaseQuote selectedQuote = inventory.Quote(_selected);
                _price.text = $"{selectedQuote.Cost} or";
                _buy.interactable = canShop && selectedQuote.CanBuy;
                _sell.interactable = canShop && SlotOf(inventory, _selected) >= 0;
            }
        }

        private void BuildTrinkets()
        {
            Transform root = _root.transform.GetChild(0);
            var trinkets = _services.Rules.TrinketChoices;
            Text caption = UiFactory.Text(root, "TrinketCaption", "Bijou :", 18, TextAnchor.MiddleLeft, Border);
            UiFactory.Place(caption, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(420f, 10f), new Vector2(70f, 30f));
            for (int i = 0; i < trinkets.Count; i++)
            {
                int choice = i;
                Button button = UiFactory.Button(root, "Trinket" + i, trinkets[i] != null ? trinkets[i].DisplayName : "?", () => _services.Commands.RequestTrinket(choice), 15);
                UiFactory.Place(button, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(490f + i * 160f, 8f), new Vector2(150f, 34f));
                _trinketLabels.Add(button.GetComponentInChildren<Text>());
            }
        }

        private void BuildGrid(string filter)
        {
            _builtFilter = filter;
            foreach (Transform child in _grid) Object.Destroy(child.gameObject);
            _entries.Clear();
            var items = _services.Content.Items;
            float y = 0f;
            int perRow = Mathf.FloorToInt(GridWidth / (Cell + 10f));
            foreach (var section in Sections)
            {
                int column = 0;
                bool any = false;
                for (int i = 0; i < items.Count; i++)
                {
                    ItemDefinition item = items[i];
                    if (item.Tier != section.tier || item.IsChampionExclusive || (item.Tier == ItemTier.Consumable && item.RecipeCost <= 0) || !Matches(item, filter)) continue;
                    if (!any)
                    {
                        Text header = UiFactory.Text(_grid, "Section", section.title, 15, TextAnchor.MiddleLeft, Border);
                        UiFactory.Place(header, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, -y), new Vector2(300f, 22f));
                        y += 26f;
                        any = true;
                    }
                    if (column == perRow)
                    {
                        column = 0;
                        y += Cell + 26f;
                    }
                    Image frame = ItemIcon(_grid, item, new Vector2(column * (Cell + 10f), -y), Cell);
                    Text price = UiFactory.Text(frame.transform, "Price", "", 15, TextAnchor.UpperCenter, UiFactory.Accent);
                    UiFactory.Place(price, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(Cell + 10f, 20f));
                    price.raycastTarget = false;
                    _entries.Add((item, price, frame));
                    column++;
                }
                if (any) y += Cell + 34f;
            }
        }

        private Image ItemIcon(Transform parent, ItemDefinition item, Vector2 position, float size)
        {
            Image frame = UiFactory.Box(parent, item.Id, new Vector2(0f, 1f), new Vector2(0f, 1f), position, new Vector2(size, size), new Color(0f, 0f, 0f, 0.6f));
            Image icon = UiFactory.Image(frame.transform, "Icon", Color.white);
            icon.rectTransform.offsetMin = new Vector2(3f, 3f);
            icon.rectTransform.offsetMax = new Vector2(-3f, -3f);
            icon.sprite = item.Icon;
            icon.raycastTarget = false;
            if (item.Icon == null)
            {
                icon.color = new Color(0.2f, 0.25f, 0.35f);
                Text name = UiFactory.Text(frame.transform, "Name", item.DisplayName, 11, TextAnchor.MiddleCenter, Color.white);
                name.raycastTarget = false;
            }
            if (item.Active.Enabled) UiFactory.Border(frame, UiFactory.Accent, 2f);
            EventTrigger trigger = frame.gameObject.AddComponent<EventTrigger>();
            var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener(data =>
            {
                if (((PointerEventData)data).button == PointerEventData.InputButton.Right) Buy(item);
                else Select(item);
            });
            trigger.triggers.Add(click);
            return frame;
        }

        private void Select(ItemDefinition item)
        {
            _selected = item;
            foreach (GameObject node in _detailNodes) Object.Destroy(node);
            _detailNodes.Clear();
            if (item == null) return;
            _name.text = item.DisplayName;
            _body.text = TooltipText.Item(item, _champion, "", true).Body;

            Text builds = UiFactory.Text(_detail, "BuildsInto", "SE CONSTRUIT EN", 14, TextAnchor.UpperLeft, Border);
            UiFactory.Place(builds, new Vector2(0f, 1f), new Vector2(0f, 1f), Vector2.zero, new Vector2(300f, 20f));
            _detailNodes.Add(builds.gameObject);
            int column = 0;
            var items = _services.Content.Items;
            for (int i = 0; i < items.Count && column < 7; i++)
                if (items[i].ComponentDefinitions.Contains(item)) _detailNodes.Add(ItemIcon(_detail, items[i], new Vector2(column++ * 50f, -24f), 44f).gameObject);

            AddTreeNode(item, 210f, -100f, 0);
        }

        // Recipe tree: the item, its components below it, and theirs one row further down.
        private void AddTreeNode(ItemDefinition item, float x, float y, int depth)
        {
            Image node = ItemIcon(_detail, item, new Vector2(x - 24f, y), 48f);
            Text price = UiFactory.Text(node.transform, "Price", item.TotalCost.ToString(), 13, TextAnchor.UpperCenter, UiFactory.Accent);
            UiFactory.Place(price, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(60f, 18f));
            _detailNodes.Add(node.gameObject);
            var components = item.ComponentDefinitions;
            if (depth >= 2 || components.Count == 0) return;
            float spread = depth == 0 ? 140f : 60f;
            for (int i = 0; i < components.Count; i++)
            {
                if (components[i] == null) continue;
                float childX = x + (components.Count == 1 ? 0f : Mathf.Lerp(-spread, spread, i / (float)(components.Count - 1)));
                AddTreeNode(components[i], childX, y - 78f, depth + 1);
            }
        }

        private void Buy(ItemDefinition item)
        {
            if (item == null || _services == null) return;
            Select(item);
            _services.Commands.RequestBuy(_services.Content.Items.IndexOf(item.Id));
        }

        private void Sell()
        {
            if (_selected == null || _champion == null) return;
            int slot = SlotOf(_champion.GetComponent<Inventory>(), _selected);
            if (slot >= 0) _services.Commands.RequestSell(slot);
        }

        private static int SlotOf(Inventory inventory, ItemDefinition item)
        {
            for (int i = 0; i < Inventory.Capacity; i++)
                if (inventory.ItemAt(i) == item) return i;
            return -1;
        }

        private static bool Matches(ItemDefinition item, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter)) return true;
            string text = filter.Trim().ToLowerInvariant();
            if ((item.DisplayName ?? "").ToLowerInvariant().Contains(text) || (item.Description ?? "").ToLowerInvariant().Contains(text)) return true;
            foreach (var stat in item.Stats)
                if (TooltipText.StatName(stat.Stat).ToLowerInvariant().Contains(text)) return true;
            return false;
        }
    }
}
