using UnityEngine;

[CreateAssetMenu(fileName = "Eclipse", menuName = "MangaLegends/Sceptre de la lune/Eclipse")]
public class Eclipse : HealBoostPassif
{
    override public void itemInteraction(ChampionControleur self = null, StatsManager target = null)
    {
        Debug.Log("passif");
    }
}