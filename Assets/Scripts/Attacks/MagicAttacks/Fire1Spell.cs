using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fire1Spell : BaseAttack
{
    public Fire1Spell()
    {
        attackName = "Fire1";
        attackDescription = "A basic fire ball. Weak.";
        attackDamage = 10f;
        attackCost = 5f;
        hitNumber = 1;
    } 
}
