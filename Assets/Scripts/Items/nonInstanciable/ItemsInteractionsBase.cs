using UnityEngine;
public abstract class ItemsInteractionsBase: ScriptableObject
{
    public string iteractionNom;
    public string iteractionDescription;
    public float cooldown;

    public string description()
    {
        string textFinal = "";

        textFinal += "<color=#FFD700>-" + iteractionNom + "-</color>";
        
        if(cooldown > 0)
        {
            textFinal += "<color=#FFD700> (" + cooldown + "s)</color>";
        }
        textFinal += "\n";

        textFinal += iteractionDescription;

        return textFinal;
    }
}

public interface IItemsIteractions
{
    public void itemInteraction(ChampionControleur self, StatsManager target);
    
}