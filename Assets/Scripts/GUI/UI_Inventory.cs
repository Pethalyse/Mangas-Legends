using System.Collections.Generic;
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
                if (i.vie > 0)
                {
                    textFinal += "vie:<color=#FFD700> " + i.vie + "</color> ";
                }

                if (i.ad > 0)
                {
                    textFinal += "ad:<color=#FFD700> " + i.ad + "</color> ";
                }

                if (i.lethality > 0)
                {
                    textFinal += "lethality:<color=#FFD700> " + i.lethality + "</color> ";
                }

                if (i.peneArmor > 0)
                {
                    textFinal += "peneArmor:<color=#FFD700> " + i.peneArmor + "%</color> ";
                }

                if (i.ap > 0)
                {
                    textFinal += "ap:<color=#FFD700> " + i.ap + "</color> ";
                }

                if (i.peneMagic > 0)
                {
                    textFinal += "peneMagic:<color=#FFD700> " + i.peneMagic + "</color> ";
                }

                if (i.mana > 0)
                {
                    textFinal += "mana:<color=#FFD700> " + i.mana + "</color> ";
                }

                if (i.critChance > 0)
                {
                    textFinal += "critChance:<color=#FFD700> " + i.critChance + "%</color> ";
                }

                if (i.critDamage > 0)
                {
                    textFinal += "critDamage:<color=#FFD700> " + i.critDamage + "%</color> ";
                }

                if (i.armor > 0)
                {
                    textFinal += "armor:<color=#FFD700> " + i.armor + "</color> ";
                }

                if (i.magicRes > 0)
                {
                    textFinal += "magicRes:<color=#FFD700> " + i.magicRes + "</color> ";
                }

                if (i.aSpd > 0)
                {
                    textFinal += "aSpd:<color=#FFD700> " + i.aSpd + "%</color> ";
                }

                if (i.regenMana > 0)
                {
                    textFinal += "regenMana:<color=#FFD700> " + i.regenMana + "%</color> ";
                }

                if (i.bonusHeal > 0)
                {
                    textFinal += "bonusHeal:<color=#FFD700> " + i.bonusHeal + "%</color> ";
                }

                if (i.cdr > 0)
                {
                    textFinal += "cdr:<color=#FFD700> " + i.cdr + "</color> ";
                }

                if (i.ms > 0)
                {
                    textFinal += "ms:<color=#FFD700> " + i.ms + "%</color> ";
                }

                if (i.lifeSteal > 0)
                {
                    textFinal += "lifeSteal:<color=#FFD700> " + i.mana + "%</color> ";
                }

                textFinal += "\n";

                string textF = "";
                //string derniereVu;
                //int n = i.description.AllIndexesOf(".").Count();
                //int leng = 0;

                //do
                //{
                //    derniereVu = i.description.Substring(leng, i.description.Substring(leng, i.description.Length - leng).IndexOf('-') + 1);
                //    leng += derniereVu.Length;
                //    textF += "<color=#FFD700>" + derniereVu + "</color>" + "\n";
                //    derniereVu = i.description.Substring(leng, i.description.Substring(leng, i.description.Length - leng).IndexOf('.') + 1);
                //    leng += derniereVu.Length;
                //    textF += derniereVu;
                //    n--;
                //    if (n != 0)
                //    {
                //        textF += "\n\n";
                //    }


                //} while (n > 0);

                textFinal += textF;

                item.GetComponentInChildren<TextMeshProUGUI>().text = textFinal;
            }
        }
    }
}
