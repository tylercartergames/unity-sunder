using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseUnit
{
    public string theName;
    public int charTurnNum;
    public bool isHasted;
    public float hasteMod;
    public float baseSPD;
    public float curSPD;
    public float baseHP;
    public float curHP;
    public float baseMP;
    public float curMP;
    public float baseATK;
    public float curATK;
    public float baseDEF;
    public float curDEF;

    //meeting Notes variables for buff/debuffs and other flags
    public bool isDamageTakenIncreased;
    public float damageTakenMod;
    public bool isDamageBuffed;
    public float damageBuffMod;
    public bool isDebuffed;




    public List<BaseAttack> attacks = new List<BaseAttack>();
    public List<BaseStatusEffect> debuffs = new List<BaseStatusEffect>();
}
