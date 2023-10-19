using Mirror;
using UnityEngine;

public class TeamManager : NetworkBehaviour
{
    [Header("Team")]
    [SerializeField] Teams team;
    internal Teams Team { get => team; set => team = value; }

    public bool inSameTeam(GameObject hit)
    {
        return Team.Equals(hit.GetComponent<TeamManager>()?.Team);
    }

    static public bool the2InSameTeam(GameObject team1,  GameObject team2)
    {
        Teams? t1 = team1.GetComponent<TeamManager>()?.Team;
        Teams? t2 = team2.GetComponent<TeamManager>()?.Team;

        if(t1 != null && t2 != null)
        {
            return t1.Equals(t2);
        }
        else
        {
            return false;
        }
        
    }
}
