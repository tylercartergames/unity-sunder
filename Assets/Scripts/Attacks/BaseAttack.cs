using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BaseAttack : MonoBehaviour
{
    public string attackName;
    public string attackDescription;
    public float attackDamage;
    public float attackCost;
    public int hitNumber;
    public float baseAttackAnimation;
    public bool hasStatusEffect;
    public int debuffTickAmount;
    public int debuffRoundDuration;
    public string debuffName;

}
