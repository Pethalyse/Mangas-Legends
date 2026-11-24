using kcp2k;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private const string playerNamePrefix = "Player";
    private const string objetNamePrefix = "Objet";

    private static Dictionary<string, ChampionControleur> players = new Dictionary<string, ChampionControleur>();
    private static Dictionary<string, StatsManager> objets = new Dictionary<string, StatsManager>();

    public static GameManager instance;

    private void Start()
    {
        if(instance == null)
        {
            instance = this;
        } 
    }

    public static void RegisterPlayer(string netID, ChampionControleur cp)
    {
        string playerName = playerNamePrefix + netID;
        players.Add(playerName, cp);
        cp.transform.name = playerName;
    }


    public static void UnregisterPlayer(string PlayerId)
    {
        if(players.ContainsKey(PlayerId))
            players.Remove(PlayerId);
    }

    public static void RegisterObjet(string netID, StatsManager sm)
    {
        if(sm is ChampionControleur)
        {
            Debug.LogWarning("On ne peut pas enregistrer un ChampionControleur en tant qu'objet");
            return;
        }

        string objetName = objetNamePrefix + netID;
        objets.Add(objetName, sm);
        sm.transform.name = objetName;
    }

    public static void UnregisterObjet(string objetId) 
    {
        if(objets.ContainsKey(objetId))
            objets.Remove(objetId);
    }

    public static ChampionControleur GetPlayer(string name)
    {
        if(players.ContainsKey(name))
            return players[name];
        return null;
    }
    
    public static StatsManager GetObjet(string name)
    {
        if (objets.ContainsKey(name))
            return objets[name];
        return null;
    }
    
    public static StatsManager GetFromAll(string name)
    {
        if (objets.ContainsKey(name))
        {
            return objets[name];
        }
        else if(players.ContainsKey(name))
        {
            return players[name];
        }

        return null;
    }

    public static Dictionary<string, ChampionControleur> GetPlayers() { return new Dictionary<string, ChampionControleur>(players); }

    public static ChampionControleur GetLocalPlayer()
    {
        foreach(ChampionControleur cc in players.Values)
        {
            if(cc.isLocalPlayer)
            {
                return cc;
            }
        }

        return null;
    }
}
