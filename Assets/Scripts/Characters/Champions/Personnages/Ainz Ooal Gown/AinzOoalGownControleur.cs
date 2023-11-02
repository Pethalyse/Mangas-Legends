
using UnityEngine;

public class AinzOoalGownControleur : ChampionControleur
{

    //PASSIF => REECRITURE DU REGENERATION MANA
    protected override void CmdRegenerationMana()
    {
        if (Time.time >= nextRegenManaTime)
        {
            nextRegenManaTime = Time.time + 1f / 100;

            if (mana < manaMax)
            {
                if (!inBattle)
                {
                    manaRegen = manaRegenBase + manaRegenLeveling * level + manaMax * 0.01f;
                }
                else
                {
                    manaRegen = (manaRegenBase + manaRegenLeveling * level) / 4;
                }

                mana += manaRegen / 100f;

                if (mana > manaMax)
                {
                    mana = manaMax;
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
                if (Random.Range(0, 101) <= critChance)
                {
                    principalClass.setValue(ad * (critDamage / 100f));
                }
                else
                {
                    principalClass.setValue(ad);
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
