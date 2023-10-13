// Mère //
using System;

abstract public class ItemInteraction : ItemsInteractionsBase, IItemsIteractions
{
    virtual public void itemInteraction(ChampionControleur self, StatsManager target)
    {
        throw new System.NotImplementedException();
    }
}

// Grandes Soeurs // 
abstract public class Passif : ItemInteraction { }
abstract public class Actif : ItemInteraction { }

// Petites Soeurs //

// Passifs
abstract public class OnHitPassif : Passif { }
abstract public class SpeedBoostPassif : Passif { }
abstract public class HealBoostPassif : Passif { }

// Actifs

abstract public class PointAndClickActif : Actif { }