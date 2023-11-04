
using UnityEngine;

public class AinzOoalGownControleur : ChampionControleur
{

    //PASSIF => REECRITURE DU REGENERATION MANA
    protected override void CmdRegenerationMana()
    {
        if (Time.time >= nextRegenManaTime)
        {
            nextRegenManaTime = Time.time + 1f / 100;

            if (mana < Stats.manaMax.GetValue())
            {
                if (!inBattle)
                {
                    mana += (Stats.manaRegen.GetValue() + Stats.manaRegen.GetBaseValue() * 0.01f) / 100;
                }
                else
                {
                    mana += Stats.manaRegen.GetValue() / 100 / 4;
                }

                if (mana > Stats.manaMax.GetValue())
                {
                    mana = Stats.manaMax.GetValue();
                }
            }
        }
    }

    private void aaCreation()
    {   

        if (aaHit == null)
        {
            animations.stopAaAnimation();
            return;
        }
        aaInit(aaHit.gameObject);
        
    }

    protected void aaInit(GameObject hit)
    {
        if (aaHit != null && isLocalPlayer)
        {
            GameObject bouleDeFeu = Instantiate(aaVisuel, spawnPoint.transform.position, spawnPoint.transform.rotation);
            Projectile principalClass = bouleDeFeu.GetComponent<Projectile>();

            if (principalClass != null)
            {
                if (Random.Range(0, 101) <= Stats.critChance.GetValue())
                {
                    principalClass.setValue(Stats.ad.GetValue() * (Stats.critDamage.GetValue() / 100f));
                }
                else
                {
                    principalClass.setValue(Stats.ad.GetValue());
                }

                principalClass.send = this;
                principalClass.setTarget(hit.transform);
            }

            aaHit = null;

            //nextAttackTime = Time.time + attackInterval;
            canAuto = true;
            //isAttack = false;
        }
    }
}
