using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poison1Spell : BaseAttack
{
    public Poison1Spell()
    {
        attackName = "Poison1";
        attackDescription = "Poison from a nonvenomous snake. ?????.";
        attackDamage = 4f;
        attackCost = 1f;
        hitNumber = 1;
        hasStatusEffect = true;
        debuffTickAmount = 1;
        debuffRoundDuration = 5;
        debuffName = "Poison1 DoT";
    } 
}
