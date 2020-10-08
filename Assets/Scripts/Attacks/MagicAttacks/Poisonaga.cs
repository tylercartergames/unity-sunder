using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poisonaga : BaseAttack
{
   public Poisonaga()
   {
    attackName= "Poisonaga";
    attackDescription = "A simple aoe poison spell";
    attackDamage = 30;
    attackCost = 20;
    hitNumber = 1;
   }
}
