using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroStateMachine : MonoBehaviour
{

    private BattleStateMachine BSM;
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

        startPosition = transform.position;
        cur_cooldown = Random.Range(0,2.5f);
        Selector.SetActive(false);
        currentState = TurnState.PROCESSING;
        BSM = GameObject.Find("BattleManager").GetComponent<BattleStateMachine>();

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
            case (TurnState.ADDTOLIST):
            {
                BSM.HerosToManage.Add(this.gameObject);
                currentState = TurnState.WAITING;

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
            currentState = TurnState.ADDTOLIST;
        }

    }

    private IEnumerator TimeForAction()
        {
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

            yield return new WaitForSeconds(0.5f);
            DoDamage();

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
                //reset enemy state
                cur_cooldown = 0f;
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
            float calc_damage = hero.curATK + BSM.PerformList[0].chosenAttack.attackDamage;
            EnemyToAttack.GetComponent<EnemyStateMachine>().TakeDamage(calc_damage);
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
