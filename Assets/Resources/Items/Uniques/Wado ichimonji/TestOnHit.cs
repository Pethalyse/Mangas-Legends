using UnityEngine;

[CreateAssetMenu(fileName = "TestOnHit", menuName = "MangaLegends/Wado ichimonji/TestOnHit")]
public class TestOnHit : OnHitPassif
{
    override public void itemInteraction(ChampionControleur self = null, StatsManager target = null)
    {
        Debug.Log("passif");
    }
}
