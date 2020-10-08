using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PyroblastSpell : BaseAttack
{

    public PyroblastSpell()
    {
        attackName = "Pyroblast";
        attackDescription = "Large fireball.";
        attackDamage = 40f;
        attackCost = 25f;
        hitNumber = 1;
    } 

}
