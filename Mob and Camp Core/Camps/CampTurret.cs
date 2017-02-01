using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

class Potential
{
   public Player player;
   public float distance;
   public float timeInRange;
   public float lastTimeSeen;
   public Boolean inRange;
   public float damageDone;
}

class CampTurret : Entity
{
    bool debugMode = false;                                             //switch to control debug print statements
	public CombatSlice cSlice;											//combat slice
	public EntityStats stats;

    public enum TurretState {
        NULL,
        WAITING,
        TARGETING,
        TURNING,
        PREPARING,
        FIRING,
        COOLING,
    }

    //Fields that are set
    public GameObject barrel;
    public GameObject projectile;
    public GameObject bPart;
    private Camp camp;
	private bool active = false;
    //public GameObject turretPrefab;
    //public Overlord overlord;

    /*
     * Private Variables
     * Some of these you may want to sync with entity stats, but I wanted them to be visible here.
     * 
     */
    public TurretState currentState = TurretState.NULL;    //Starts from the NULL state, which will set these values as soon as the overlord isn't null.
    private float targetUpdateTime;                 //How often does the turret recalculate the targets. Very math heavy.
    private float targetingRange;                   //How far away should the turret consider targets
    private float coolingTime;                       //How long before the turret goes back to waiting.
    private float cooldownTime;
    private float gracePeriod;                      //How long before the turret tries to attack you.
    private float persistTime;                      //How long after leaving range will the turret attack you.
    private float maxTurnSpeed;                     //Max degrees per second
    private float maxTurnSpeedWhenTargetting;       //Max degrees per second, after the inital turning has occured
    private float maxTargetAhead;                   //Max "time" to fire ahead of a player. Could cause targetting issue if too large.
    

    //Damage & Time determine agression
    private Potential[] potentials;
    private Potential target;

    private TurretAudio audio;    //turret audio controller, make sure prefab has script 

    public void reset()
    {
        nullState();
    }

    public void respawn()
    {
        camp.setState(CampState.CHEST);
        return;
    }

    public void init(Camp camp, Overlord ol)
    {
        this.camp = camp;
        this.overlord = ol;

		stats = new EntityStats();
		stats.attack = 10000;
		cSlice = this.gameObject.AddComponent<CombatSlice>();
        //cSlice = new CombatSlice();
		cSlice.init(stats, this, null, null);
		active = true;
    }

    new public void Start()
    {
		cSlice = this.gameObject.AddComponent<CombatSlice>();
    audio = this.GetComponentInParent<TurretAudio>();
        base.Start();

    }

    public void Update()
    {
		if (active)
		{
	        if (stats.health < 0)
	        {
	            camp.setState(CampState.CHEST);
	            return;
	        }
	        if (currentState == TurretState.NULL)
	        {
	            nullState();
	        }
	        else
	        {
	            updateState();
	        }
		}
    }


    //State Stuff
    int framesInState = 0;
    float timeInState = 0.0f;

    void updateState()
    {

        switch (currentState)
        {
            case TurretState.WAITING:
                waitingState();
                break;
            case TurretState.TARGETING:
                targetingState();
                break;
            case TurretState.TURNING:
                turningState();
                break;
            case TurretState.PREPARING:
                preparingState();
                break;
            case TurretState.FIRING:
                firingState();
                break;
            case TurretState.COOLING:
                coolingState();
                break;
            case TurretState.NULL:
                nullState();
                break;
        }
        framesInState++;
        timeInState += Time.deltaTime;
    }


    /*
     * 
     * ---------------------------------------------------------------------------------
     * Null State
     * 
     */

    void nullState()
    {
        if (overlord == null)
        {
            if (debugMode) Debug.Log("Turret: Overlord not set. Searching for overlord!");
			overlord = GameObject.FindWithTag("master").GetComponent<Overlord>();
        }
        else
        {
            barrel.transform.rotation = Quaternion.Euler(270, 0, 0);
            Player[] ps = overlord.actorCore.playerSlice.players;
            if (ps == null)
            {
                if (debugMode) Debug.Log("Turret says players are null!");
            }
            else
            {
                targetUpdateTime = 0.25f;
                targetingRange = 5.0f;  //Remember that tiles are 2x2
                coolingTime = 0.25f; //Because scaled time is slower than real time
                gracePeriod = 0.5f; //Because scaled time is slower than real time
                cooldownTime = 2.5f / camp.tile.value;
                persistTime = 0.1f;
                maxTurnSpeed = 35f * camp.tile.value;
                maxTurnSpeedWhenTargetting = 720f;
                maxTargetAhead = 2.5f;

                stats.attack = 15f * camp.tile.value;
                stats.projectileSpeed = 0.5f * camp.tile.value;
                stats.attackDelay = 0;

                potentials = new Potential[ps.GetLength(0)];

                foreach (Player p in ps)
                {
                    potentials[p.playerID - 1] = new Potential();
                    Potential currentPotential = potentials[p.playerID - 1];
                    currentPotential.player = p;
                    currentPotential.distance = (p.transform.position - this.transform.position).sqrMagnitude;
                    currentPotential.timeInRange = 0.0f;
                    currentPotential.lastTimeSeen = -10.0f;
                }
                waitingState();
            }
        }
    }

    /*
     * 
     * ---------------------------------------------------------------------------------
     * Waiting State
     * 
     */

    float yTPos;
    float maxWanderChange = 1.0f;
    float maxWanderSpeed = 2.0f;
    float turnSpeed = 0.5f;

    void waitingState()
    {
        //Initialize the state if I'm just now entering.
        if (currentState != TurretState.WAITING)
        {
            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered Waiting State");
            currentState = TurretState.WAITING;
            
            //This is so we aren't calculating all of the turret info on the same frames. Yes it is silly, but it will spread out the burden.
            //It'd be best to do it once, but once per entering the state is good enough.
        }        
        else
        {
            Quaternion trot = Quaternion.Euler(270f, barrel.transform.rotation.eulerAngles.y + turnSpeed, 0f);
            barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, trot, maxTurnSpeed * Time.smoothDeltaTime);

            framesInState++;
            timeInState += Time.smoothDeltaTime;

            bool switchStateFlag = false;
            foreach (Potential p in potentials)
            {
                try
                {
                    p.distance = (p.player.transform.position - this.transform.position).sqrMagnitude;
                    if (p.distance < targetingRange * targetingRange)
                    {
                        p.inRange = true;
                        p.lastTimeSeen = Time.time;
                        p.timeInRange += Time.deltaTime;
                        if (p.timeInRange > gracePeriod)
                        {
                            switchStateFlag = true;
                        }
                    }
                    else
                    {
                        p.inRange = false;
                        p.timeInRange = 0;
                    }
                }
                catch(System.Exception e)
                {
                    e.ToString();
                }
            }

            if (switchStateFlag)
            {
                targetingState();
            }

        }
    }


    void wanderTurret()
    {
        //To give them a bit of personality!
        Quaternion to = Quaternion.Euler(0,barrel.transform.rotation.eulerAngles.y + UnityEngine.Random.Range(-1*maxWanderSpeed,maxWanderSpeed),0);
        barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, to, maxWanderChange);
    }

    /*
     * 
     * ---------------------------------------------------------------------------------
     * Targeting State
     * 
     */

    void targetingState()
    {
        if (currentState != TurretState.TARGETING)
        {
            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered targeting State");
            currentState = TurretState.TARGETING;
            target = null;
        }
        else
        {
            framesInState++;
            timeInState += Time.smoothDeltaTime;

            bool noTargets = true;
            foreach (Potential p in potentials)
            {
                try
				{
					p.distance = (p.player.transform.position - this.transform.position).sqrMagnitude;
	                if (p.distance < targetingRange * targetingRange)
	                {
	                    p.inRange = true;
	                    p.lastTimeSeen = Time.time;
	                    p.timeInRange += Time.deltaTime;
	                    noTargets = false;
	                }
	                else
	                {
	                    p.inRange = false;
	                }
				}
				catch (System.Exception e)
				{
					e.ToString();
				}
            }

            if (noTargets && timeInState > coolingTime)
            {
                waitingState();
            }
            else if (noTargets)
            {
                wanderTurret();
            }
            else
            {
                foreach (Potential p in potentials)
                {
                    if (p.inRange)
                    {
                        if (target == null)
                        {
                            target = p;
                        }
                        else if (p.timeInRange * UnityEngine.Random.Range(0f, 1f) > target.timeInRange * UnityEngine.Random.Range(0f, 1f))
                        {
                            target = p;
                        }
                    }
                }
                turningState();
            }
        }

            
    }


    /*
     * 
     * ---------------------------------------------------------------------------------
     * Turning State
     * 
     */
    

    void turningState()
    {
        //Initialize the state if I'm just now entering.
        if (currentState != TurretState.TURNING)
        {
            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered turning State");
            currentState = TurretState.TURNING;
            return;
        }
        else
        {
            target.distance = (target.player.transform.position - this.transform.position).sqrMagnitude;
            target.inRange = target.distance < targetingRange * targetingRange;


            //Check if target is in sight range
            if (!target.inRange && (Time.time - target.lastTimeSeen) > persistTime)
            {
                targetingState();
                return;
            }
            else
            {
                if (debugMode) Debug.Log("Turning... " + maxTurnSpeed + " : " + camp.tile.value);
                Quaternion look = Quaternion.LookRotation(target.player.transform.position - this.transform.position);
                target.lastTimeSeen = Time.time;
                barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, look, maxTurnSpeed * Time.smoothDeltaTime);

                if (timeInState > 2.5f)
                {
                    if ( Quaternion.Angle(barrel.transform.rotation, look) < 0.5f)
                    {
                        firingState();
                        return;
                    }
                }
            }
        }

    }

    //THIS STATE IS NOT USED
    void preparingState()
    {
        //Vector3 prot = Quaternion.LookRotation((p.player.transform.position - this.transform.position)).eulerAngles;        
        //Initialize the state if I'm just now entering.
        if (currentState != TurretState.PREPARING)
        {
            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered preparing State");
            currentState = TurretState.PREPARING;
        }
        else
        {
            //Should do a raycast check here.        
            
        }
    }

    void firingState()
    {
        //Initialize the state if I'm just now entering.
        if (currentState != TurretState.FIRING)
        {

            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered firing State");
            currentState = TurretState.FIRING;
            return;
        }
        else
        {
           cSlice.shoot(projectile, barrel.gameObject.transform.position, barrel.gameObject.transform.forward, barrel.gameObject.transform.rotation, false);
           coolingState();
           return;
        }

    }

    void coolingState()
    {
        //Initialize the state if I'm just now entering.
        if (currentState != TurretState.COOLING)
        {

            framesInState = 0;
            timeInState = 0.0f;
            if (debugMode) Debug.Log("Entered cooling State");
            currentState = TurretState.COOLING;
            return;
        }
        else
        {
            if (timeInState < 0.1)
            {
                barrel.transform.LookAt(target.player.transform);
            }
            else if (timeInState > cooldownTime)
            {
                turningState();
            }
            else
            {
                Quaternion trot = Quaternion.Euler(270f, barrel.transform.rotation.eulerAngles.y + turnSpeed * 3.0f, 0f);
                barrel.transform.rotation = Quaternion.RotateTowards(barrel.transform.rotation, trot, maxTurnSpeed * 3.0f * Time.smoothDeltaTime);
                return;
            }
        }

    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Weapon")
        {
            if (debugMode) Debug.Log("Turret hit!");
            Player p = col.GetComponentInParent<Player>();
            potentials[p.playerID - 1].damageDone += p.stats.attack;
            this.cSlice.damage(p.stats.attack, stats);
        }
    }

}

