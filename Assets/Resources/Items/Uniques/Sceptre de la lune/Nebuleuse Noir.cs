using UnityEngine;

[CreateAssetMenu(fileName = "NebuleuseNoir", menuName = "MangaLegends/Sceptre de la lune/NebuleuseNoir")]
public class NebuleuseNoir : PointAndClickActif
{
    override public void itemInteraction(ChampionControleur self = null, StatsManager target = null)
    {
        Debug.Log("actif");
    }
}
