using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{

    private BattleStateMachine BSM;
    private BattleLog BL;
    public BaseHero hero;

    public enum TurnState
    {
        PROCESSING,
        ADDTOLIST,
        WAITING,
        SELECTING,
        ACTION,
        DEAD
    }

    public TurnState currentState;

    //progressBar
    private float cur_cooldown = 0f;
    private float max_cooldown = 5f;
    private Image ProgressBar;

    public GameObject Selector;

    //ienumerator
    public GameObject EnemyToAttack;
    private bool actionStarted = false;
    private Vector3 startPosition;
    private float animSpeed = 10f;

    //dead
    private bool alive = true;

    //hero panel
    private HeroPanelStats stats;
    public GameObject HeroPanel;
    private Transform HeroPanelSpacer;


    // Start is called before the first frame update
    void Start()
    {   
        //find spacer object. make connection
        HeroPanelSpacer = GameObject.Find("BattleCanvas").transform.Find("HeroPanel").transform.Find("HeroPanelSpacer");

        //create panel, fill in info for corresponding hero
        CreateHeroPanel();

        hero.charTurnNum = 1;
        startPosition = transform.position;
        cur_cooldown = Random.Range(0,2.5f);
        Selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();
        BL = GameObject.Find("BattleManager").GetComponent<BattleLog>();

    }

    // Update is called once per frame
    void Update()
    {
        switch (currentState)
        {
            case (TurnState.PROCESSING):
            {

                UpgradeProgressBar(); //only line originally in this case

                break;
            }
            case (TurnState.ADDTOLIST):
            {
                    BSM.HerosToManage.Add(this.gameObject); 

            currentState = TurnState.WAITING; //keep
                break;
            }
            case (TurnState.WAITING):
            {   //idle state
                 
                break;
            }
            case (TurnState.SELECTING):
            {


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
                    //change tag of hero
                    this.gameObject.tag = "DeadHero";

                    //not attackable by enemy
                    BSM.HerosInBattle.Remove(this.gameObject);

                    //not manageable
                    BSM.HerosToManage.Remove(this.gameObject);

                    //deactivate selector
                    Selector.SetActive(false);

                    //reset gui
                    BSM.AttackPanel.SetActive(false);
                    BSM.EnemySelectPanel.SetActive(false);

                    //remove item from performList
                    if (BSM.HerosInBattle.Count > 0)
                    {
                        for(int i=0; i<BSM.PerformList.Count; i++)
                        {
                            if (BSM.PerformList[i].AttackersGameObject == this.gameObject)
                            {
                                BSM.PerformList.Remove(BSM.PerformList[i]);
                            }

                            if (BSM.PerformList[i].AttackersTarget == this.gameObject)
                            {
                                BSM.PerformList[i].AttackersTarget = BSM.HerosInBattle[Random.Range(0,BSM.HerosInBattle.Count)];
                            }
                        }    
                    }
                    

                    //change color/playanimation
                    this.gameObject.GetComponent<SpriteRenderer>().color = new Color32(105,105,105,255);

                    //reset Heroinput
                    BSM.battleStates = BattleStateMachine.PerformAction.CHECKALIVE;
                    alive = false;
                }

                break;
            }

        }
    }

    void UpgradeProgressBar()
    {
        cur_cooldown = cur_cooldown + Time.deltaTime;
        float calc_cooldown = cur_cooldown / max_cooldown;
        ProgressBar.transform.localScale = new Vector3(Mathf.Clamp(calc_cooldown,0,1), ProgressBar.transform.localScale.y, ProgressBar.transform.localScale.z);
        if (cur_cooldown >= max_cooldown)
        {
            for (int i=0;i<BSM.HerosInBattle.Count;i++){
                BSM.HerosInBattle[i].gameObject.GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.WAITING;
            }
            for (int j=0;j<BSM.EnemysInBattle.Count;j++){
                BSM.EnemysInBattle[j].gameObject.GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.WAITING;
            }


            currentState = TurnState.ADDTOLIST; //keep
        }
    }

    private IEnumerator TimeForAction()
        {
            //check if they have a status
            //do damage from any statuses
            // yield return new WaitForSeconds(.5f);



            if (actionStarted)
            {
                yield break;
            }

            actionStarted = true;
            Vector3 enemyPosition = new Vector3(EnemyToAttack.transform.position.x + 1.5f, EnemyToAttack.transform.position.y, EnemyToAttack.transform.position.z);
            while(MoveTowardsEnemy(enemyPosition))
            {
                yield return null;
            }

            yield return new WaitForSeconds(.5f);

            //Attack function
            DoDamage();

            //Check/Do Status Effects
            
            //Do Animation
            

            Vector3 firstPosition = startPosition;
            while(MoveTowardsStart(firstPosition))
            {
                yield return null;
            }

            BSM.PerformList.RemoveAt(0);
            //reset bsm ~> wait
            if (BSM.battleStates != BattleStateMachine.PerformAction.WIN && BSM.battleStates != BattleStateMachine.PerformAction.LOSE)
            {
                BSM.battleStates = BattleStateMachine.PerformAction.WAIT;
                //reset hero state
                cur_cooldown = 0f;

                for (int i=0;i<BSM.HerosInBattle.Count;i++){ 
                    BSM.HerosInBattle[i].gameObject.GetComponent<HeroStateMachine>().currentState = HeroStateMachine.TurnState.PROCESSING; 
                } 
                for (int j=0;j<BSM.EnemysInBattle.Count;j++){ 
                    BSM.EnemysInBattle[j].gameObject.GetComponent<EnemyStateMachine>().currentState = EnemyStateMachine.TurnState.PROCESSING;
                }           


                currentState = TurnState.PROCESSING;
            } else 
            {
                currentState = TurnState.WAITING;
            }
            actionStarted = false;
        }

        private bool MoveTowardsEnemy(Vector3 target)
        {
            return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
        }
        private bool MoveTowardsStart(Vector3 target)
        {
            return target != (transform.position = Vector3.MoveTowards(transform.position, target, animSpeed * Time.deltaTime));
        }

        public void TakeDamage(float getDamageAmount)
        {
            hero.curHP -= getDamageAmount;
            if (hero.curHP<=0)
            {
                hero.curHP = 0;
                currentState = TurnState.DEAD;
            }
            UpdateHeroPanel();
        }

        void DoDamage()
        {
            if (EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.Count == 0){
                EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.isDebuffed = false;
            }
            float calc_damage = hero.curATK + BSM.PerformList[0].chosenAttack.attackDamage;
            EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);

            //check if enemy is alive after initial ability hit before worrying about dots on the enemy
            if (EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.curHP > 0){

                print (BSM.PerformList[0].chosenAttack.debuffRoundDuration);

                if (BSM.PerformList[0].chosenAttack.hasStatusEffect || EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.isDebuffed){

                    BaseStatusEffect newStatusEffect = new BaseStatusEffect();
                    newStatusEffect.effectName = BSM.PerformList[0].chosenAttack.debuffName;
                    newStatusEffect.effectRoundDuration = BSM.PerformList[0].chosenAttack.debuffRoundDuration;
                    newStatusEffect.effectTickAmount = BSM.PerformList[0].chosenAttack.debuffTickAmount;

                    for (int j=0;j<EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.Count;j++){
                        if (string.Equals(EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[j].effectName,newStatusEffect.effectName)){
                            //Remove so we can apply with new calc
                            EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.Remove(newStatusEffect);
                        }
                    }
                    
                    ApplyStatus(newStatusEffect);
                    DoStatus();
                    
                }
            }
            BL.CreateText("1"+","+
                          "0"+","+
                          hero.charTurnNum+","+
                          "Player"+","+
                          hero.theName+","+
                          EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.theName+","+
                          BSM.PerformList[0].chosenAttack.attackName+","+
                          BSM.PerformList[0].chosenAttack.hitNumber+","+
                          BSM.PerformList[0].chosenAttack.attackDamage+","+
                          EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.curHP+","+
                          EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.baseHP+","+
                          hero.isHasted+","+
                          hero.hasteMod+","+
                          hero.isDamageBuffed+","+
                          hero.damageBuffMod+","+
                          hero.isDamageTakenIncreased+","+
                          hero.damageTakenMod+"\n");
            hero.charTurnNum++;
        }

        void ApplyStatus(BaseStatusEffect status)
        {
            status.statusEffectCaster = GameObject.Find(hero.theName);
            
            EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.Add(status);
            EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.isDebuffed = true;
        }

        void DoStatus()
        {
            int tempDoT;
            GameObject caster = GameObject.Find(hero.theName);

            for (int i=0; i<EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.Count;i++){

                if (EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[i].statusEffectCaster == caster){
                    tempDoT = EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[i].effectTickAmount;
                    if (EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[i].effectRoundDuration == 0){
                        EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs.RemoveAt(i);
                    }
                    else {
                        EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(tempDoT);
                        EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[i].effectRoundDuration--;
                        print("Dot Duration: " + EnemyToAttack.GetComponent<EnemyStateMachine>().enemy.debuffs[i].effectRoundDuration);
                    }

                }
            }
        }


        void CreateHeroPanel()
        {
            HeroPanel = Instantiate(HeroPanel) as GameObject;
            stats = HeroPanel.GetComponent<HeroPanelStats>();
            stats.HeroName.text = hero.theName;
            stats.HeroHP.text = "HP: " + hero.curHP;
            stats.HeroMP.text = "MP: " + hero.curMP;
            
            ProgressBar = stats.ProgressBar;

            HeroPanel.transform.SetParent(HeroPanelSpacer, false);
        }

        void UpdateHeroPanel()
        {
            stats.HeroHP.text = "HP: " + hero.curHP;
            stats.HeroMP.text = "MP: " + hero.curMP;
        }

}
