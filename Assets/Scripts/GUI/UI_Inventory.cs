using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Inventory : MonoBehaviour
{

    public GameObject itemPrefab;
    public Transform conteneurObjets;

    private ChampionControleur player;
    private List<Item> items = new List<Item>();

    // Update is called once per frame
    void Update()
    {
        if (!player)
        {
            player = GameManager.GetLocalPlayer();
        }
        else
        {
            if (items.Equals(player.getItems())) { return; }
            items = new List<Item>(player.getItems());

            foreach(Transform t in conteneurObjets.transform)
            {
                Destroy(t.gameObject);
            }

            foreach (Item i in player.getItems())
            {
                var item = Instantiate(itemPrefab, conteneurObjets);
                item.GetComponent<Image>().sprite = i.icone;

                string textFinal = "";
                i.stats.ForEach(x => textFinal += x.ToString() + " ");

                textFinal += "\n";

                string textF = "";

                textFinal += textF;

                item.GetComponentInChildren<TextMeshProUGUI>().text = textFinal;
            }
        }
    }
}
