using System.Collections.Generic;
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

    [SerializeField] private List<Behaviour> passifs;
    [SerializeField] private List<Behaviour> ab1;
    [SerializeField] private List<Behaviour> ab2;
    [SerializeField] private List<Behaviour> ab3;
    [SerializeField] private List<Behaviour> ab4;

    public static GUIControleur instance;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        }  
    }

    public void MiseAJour(ChampionControleur player)
    {
        vie.value = (float)player.getVie() / player.getVieMax();
        mana.value = (float)player.getMana() / player.getManaMax();
        golds.text = player.getGolds().ToString();
        Kda.text = player.NbKills + "/" + player.NbDeath + "/" + player.NbAssist;
        minions.text = player.NbMinions.ToString();
    }

    public List<Behaviour> GetAbGui(int i)
    {
        switch (i)
        {
            case 0: return passifs;
            case 1: return ab1;
            case 2: return ab2;
            case 3: return ab3;
            case 4: return ab4;
        }

        return null;
    }
}
