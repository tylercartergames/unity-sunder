using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{

    private BattleStateMachine BSM;
    private BattleLog BL;

    public BaseEnemy enemy;
    public enum TurnState
    {
        PROCESSING,
        CHOOSEACTION,
        WAITING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    //progressBar
    public float cur_cooldown = 0f;
    private float max_cooldown = 5f;
    
    //this gameobject
    private Vector3 startPosition;

    //ienumerator
    private bool actionStarted = false;
    public GameObject HeroToAttack;
    private float animSpeed = 10f;

    //alive
    private bool alive = true;

    public GameObject Selector;

    // Start is called before the first frame update
    void Start()
    {
        // cur_cooldown = 0f; //was just cur_cooldown = 0f;
        currentState = TurnState.PROCESSING;
        Selector.SetActive(false);
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        startPosition = transform.position;
        enemy.charTurnNum = 1;
        BL = GameObject.Find("BattleManager").GetComponent<BattleLog>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
            {
                UpgradeProgressBar();

                break;
            }
            case (TurnState.CHOOSEACTION):
            {
                ChooseAction();
                currentState = TurnState.WAITING;

                break;
            }
            case (TurnState.WAITING):
            {
                //idle

                break;
            }
            case (TurnState.ACTION):
            {
                StartCoroutine(TimeForAction());

                break;
            }
            case (TurnState.DEAD):
            {
                if (!alive)
                {
                    return;
                } else
                {
                    //change tag
                    this.gameObject.tag = "DeadEnemy";
                    //not attackable
                    BSM.EnemysInBattle.Remove(this.gameObject);
                    //disable selector
                    Selector.SetActive(false);
                    //remove all inputs heroattks
                    if (BSM.EnemysInBattle.Count > 0)
                    {
                        for (int i=0; i < BSM.PerformList.Count; i++)
                        {
                            if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }

                            if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.EnemysInBattle[Random.Range(0,BSM.EnemysInBattle.Count)];
                            }
                        }   
                    }
                    
                    //chagnge color/play dead animations i guess
                    this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(105,105,105,255);
                    //set alive
                    alive = false;
                    //reset enemybuttons
                    BSM.EnemyButtons();
                    //check if battle is won/lost already
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                }

                break;
            }

        }
    }

    void UpgradeProgressBar()
    {   
        cur_cooldown = cur_cooldown + Time.deltaTime;

        if (cur_cooldown >= max_cooldown)
        {

                for (int i=0;i<BSM.HerosInBattle.Count;i++){ //NEW0707 
                    BSM.HerosInBattle[i].gameObject.GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING; //NEW0707 
                } //NEW0707 
                for (int j=0;j<BSM.EnemysInBattle.Count;j++){ //NEW0707 
                    BSM.EnemysInBattle[j].gameObject.GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.WAITING; //NEW0707 
                }      //NEW0707  
            currentState = TurnState.CHOOSEACTION;
        }

    }

    void ChooseAction()
    {
        HandleTurn myAttack = new HandleTurn();
        myAttack.Attacker = enemy.theName;
        myAttack.Type = "Enemy";
        myAttack.AttackersGameObject = this.gameObject;
        myAttack.AttackersTarget = BSM.HerosInBattle[Random.Range(0,BSM.HerosInBattle.Count)];

        int num = Random.Range(0,enemy.attacks.Count);
        myAttack.chosenAttack = enemy.attacks[num];

        BSM.CollectActions(myAttack);
    }

    private IEnumerator TimeForAction()
    {
        if (actionStarted)
        {
            yield break;
        }

        actionStarted = true;
        Vector3 heroPosition = new Vector3(HeroToAttack.transform.position.x - 1.5f, HeroToAttack.transform.position.y, HeroToAttack.transform.position.z);
        while(MoveTowardsEnemy(heroPosition))
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        
        DoDamage();

        Vector3 firstPosition = startPosition;
        while(MoveTowardsStart(firstPosition))
        {
            yield return null;
        }

        BSM.PerformList.RemoveAt(0);
        BSM.battleStates = BattleStateMachine.PerformAction.WAIT;

        actionStarted = false;

        cur_cooldown = 0f; //was just cur_cooldown = 0f;

                for (int i=0;i<BSM.HerosInBattle.Count;i++){ //NEW0707 
                    BSM.HerosInBattle[i].gameObject.GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.PROCESSING; //NEW0707 
                } //NEW0707 
                for (int j=0;j<BSM.EnemysInBattle.Count;j++){ //NEW0707 
                    BSM.EnemysInBattle[j].gameObject.GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.PROCESSING; //NEW0707 
                }      //NEW0707  

        currentState = TurnState.PROCESSING;
    }

    private bool MoveTowardsEnemy(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }
    private bool MoveTowardsStart(Vector3 target)
    {
        return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
    }

    void DoDamage()
    {
        float calc_damage = enemy.curATK + BSM.PerformList[0].chosenAttack.attackDamage;
        HeroToAttack.GetComponent<HeroStateMachine>().TakeDamage(calc_damage);
        BL.CreateText("1"+","+
                      "0"+","+
                      enemy.charTurnNum+","+
                      "Enemy"+","+
                      enemy.theName+","+
                      HeroToAttack.GetComponent<HeroStateMachine>().hero.theName+","+
                      BSM.PerformList[0].chosenAttack.attackName+","+
                      BSM.PerformList[0].chosenAttack.hitNumber+","+
                      BSM.PerformList[0].chosenAttack.attackDamage+","+
                      HeroToAttack.GetComponent<HeroStateMachine>().hero.curHP+","+
                      HeroToAttack.GetComponent<HeroStateMachine>().hero.baseHP+","+
                      enemy.isHasted+","+
                      enemy.hasteMod+","+
                      enemy.isDamageBuffed+","+
                      enemy.damageBuffMod+","+
                      enemy.isDamageTakenIncreased+","+
                      enemy.damageTakenMod+"\n");
        enemy.charTurnNum++;
    }

    public void TakeDamage(float getDamageAmount)
    {
       enemy.curHP -= getDamageAmount;
       if (enemy.curHP <= 0)
       {
           enemy.curHP = 0;
           currentState = TurnState.DEAD;
       } 
    }

}
