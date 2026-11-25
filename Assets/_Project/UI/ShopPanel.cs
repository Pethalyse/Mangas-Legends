using System.Collections.Generic;
using MangaLegends.Core.Economy;
using MangaLegends.Data.Definitions;
using MangaLegends.Simulation.Champions;
using MangaLegends.Simulation.Services;
using MangaLegends.Simulation.Units;
using UnityEngine;
using UnityEngine.UI;

namespace MangaLegends.UI
{
    public sealed class ShopPanel
    {
        private const float Cell = 96f;

        private readonly GameObject _root;
        private readonly Text _status;
        private readonly List<(ItemDefinition item, Image frame, Text price)> _entries = new List<(ItemDefinition, Image, Text)>();
        private MatchServices _services;
        private bool _open;
        private readonly List<Text> _trinketLabels = new List<Text>();

        public ShopPanel(Transform canvas)
        {
            Image panel = UiFactory.Box(canvas, "Shop", new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(20f, 60f), new Vector2(560f, 700f), UiFactory.Panel);
            _root = panel.gameObject;
            Text title = UiFactory.Text(panel.transform, "Title", "Boutique (P)", 30, TextAnchor.UpperCenter, UiFactory.Accent);
            title.rectTransform.offsetMax = new Vector2(0f, -10f);
            _status = UiFactory.Text(panel.transform, "Status", "", 20, TextAnchor.LowerCenter, Color.white);
            _status.rectTransform.offsetMin = new Vector2(0f, 10f);
            _root.SetActive(false);
        }

        public void Bind(MatchServices services)
        {
            if (_services == services) return;
            _services = services;
            Build();
        }

        public void Refresh(Unit champion)
        {
            if (_services == null) return;
            if (_services.View.TakeShopToggle()) _open = !_open;
            bool visible = _open && champion != null;
            _root.SetActive(visible);
            if (!visible) return;

            Inventory inventory = champion.GetComponent<Inventory>();
            bool canShop = inventory.CanShop;
            _status.text = canShop ? "Clic : acheter. Clic sur un objet de l'inventaire : vendre." : "Retournez à la fontaine pour acheter.";
            var choices = _services.Rules.TrinketChoices;
            var current = champion.Caster.AbilityAt(Simulation.Abilities.CasterSlot.Trinket);
            for (int i = 0; i < _trinketLabels.Count && i < choices.Count; i++)
                _trinketLabels[i].color = choices[i] == current ? UiFactory.Accent : canShop ? Color.white : new Color(1f, 1f, 1f, 0.4f);
            foreach (var entry in _entries)
            {
                PurchaseQuote quote = inventory.Quote(entry.item);
                entry.price.text = quote.Cost.ToString();
                entry.price.color = quote.CanBuy && canShop ? UiFactory.Accent : new Color(0.7f, 0.4f, 0.4f);
            }
        }

        private void Build()
        {
            var items = _services.Content.Items;
            Transform root = _root.transform;
            Text caption = UiFactory.Text(root, "TrinketCaption", "Bijou :", 20, TextAnchor.MiddleLeft, Color.white);
            UiFactory.Place(caption, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(20f, 44f), new Vector2(80f, 40f));
            var trinkets = _services.Rules.TrinketChoices;
            for (int i = 0; i < trinkets.Count; i++)
            {
                int choice = i;
                Button button = UiFactory.Button(root, "Trinket" + i, trinkets[i] != null ? trinkets[i].DisplayName : "?", () => _services.Commands.RequestTrinket(choice), 18);
                UiFactory.Place(button, new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(100f + i * 220f, 44f), new Vector2(210f, 40f));
                _trinketLabels.Add(button.GetComponentInChildren<Text>());
            }
            int column = 0;
            int row = 0;
            for (int i = 0; i < items.Count; i++)
            {
                ItemDefinition item = items[i];
                if (item.Tier == ItemTier.Consumable && item.RecipeCost <= 0) continue;
                int index = i;
                Image frame = UiFactory.Box(root, item.Id, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20f + column * (Cell + 8f), -60f - row * (Cell + 30f)), new Vector2(Cell, Cell), new Color(0f, 0f, 0f, 0.6f));
                frame.gameObject.AddComponent<Button>().onClick.AddListener(() => _services.Commands.RequestBuy(index));
                Image icon = UiFactory.Image(frame.transform, "Icon", Color.white);
                icon.sprite = item.Icon;
                if (item.Icon == null)
                {
                    icon.color = new Color(0.3f, 0.3f, 0.4f);
                    Text name = UiFactory.Text(frame.transform, "Name", item.DisplayName, 14, TextAnchor.MiddleCenter, Color.white);
                    name.raycastTarget = false;
                }
                Text price = UiFactory.Text(frame.transform, "Price", "", 18, TextAnchor.UpperCenter, UiFactory.Accent);
                UiFactory.Place(price, new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), Vector2.zero, new Vector2(Cell, 26f));
                _entries.Add((item, frame, price));
                column++;
                if (column == 5)
                {
                    column = 0;
                    row++;
                }
            }
        }
    }
}
