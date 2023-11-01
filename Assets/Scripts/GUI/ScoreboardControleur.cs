using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ScoreboardControleur : MonoBehaviour
{

    public static ScoreboardControleur instance;

    [SerializeField] private GameObject blueTeam;
    [SerializeField] private GameObject redTeam;

    private void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    private void Update()
    {
        for(int i = 0; i<GameManager.GetPlayers().Count; i++)
        {
            ChampionControleur player = GameManager.GetPlayers().ToList()[i].Value;
            Transform child = blueTeam.transform.GetChild(i);

            child.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = player.NbKills + "/" + player.NbDeath + "/" + player.NbAssist;
            child.GetChild(1).GetChild(1).GetComponent<TextMeshProUGUI>().text = player.NbMinions.ToString();

            foreach(Transform t in child.GetChild(2))
            {
                t.GetComponent<Image>().sprite = null;
            }

            for(int l=0; l<player.getItems().Count; l++)
            {
                child.GetChild(2).GetChild(l).GetComponent<Image>().sprite = player.getItems()[l].icone;
            }
        }
    }
}
