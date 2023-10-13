using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GUIControleur : MonoBehaviour
{

    [SerializeField] private Slider vie;
    [SerializeField] private Slider mana;
    [SerializeField] private TextMeshProUGUI golds;
    [SerializeField] private TextMeshProUGUI Kda;
    [SerializeField] private TextMeshProUGUI minions;

    public void miseAJour(ChampionControleur player)
    {
        vie.value = (float)player.getVie() / player.getVieMax();
        mana.value = (float)player.getMana() / player.getManaMax();
        golds.text = player.getGolds().ToString();
        Kda.text = player.NbKills + "/" + player.NbDeath + "/" + player.NbAssist;
        minions.text = player.NbMinions.ToString();
    }
}
