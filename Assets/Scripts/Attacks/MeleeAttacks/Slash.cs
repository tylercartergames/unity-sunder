using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slash : BaseAttack
{
    public Slash()
    {
        attackName = "Slash";
        attackDescription = "A quick sword slash.";
        attackDamage = 10f;
        attackCost = 0;
        hitNumber = 1;
    }
}
