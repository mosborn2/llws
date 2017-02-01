using UnityEngine;
using System.Collections;

public class CombatSlice : MonoBehaviour
{	
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	bool debugMode = false;
	Entity owner;								//ref to entity owning this script (player/turret/etc)

	Animator anim;								//ref to animator for this entity
	EntityStats stats;							//reference to this entity's stats
	FillScript healthBar;						//UI health bar reference

	public float nextActionTime;				//when a new non-idle action can be performed
	private combatState state;					//current combat state

	public float normalAttackLength  = 0.5f;	//length of the attack action in seconds
	public float rangedAttackLength  = 0.5f;	//length of the attack action in seconds
	public float projectileVel = 400.0f;
	float parryLength   	  = 1.0f;			//length of the parry action in seconds
	float stunLength          = 2.0f;			//ammount of time you have to wait after being parried
	float trinketLength       = 2.0f;			//length of the trinket action in seconds

	public enum combatState   { NORMAL, ATTACK, CHARGING, PARRY, TRINKET, STUNNED, HIT }
	//-----------------------------------------------------------------------------------------------------------------------------
	public combatState getState() 		{ return state; }
	public void parried()
	{
		nextActionTime = Time.time + stunLength;
		state = combatState.STUNNED;
	}
	public void init(EntityStats nstats, Entity en, Animator nanim, FillScript nHealthBar)
	{
		stats = nstats;											//entity stats container
		owner = en;												//entity that this slice instance is attached to
		if (nanim != null) anim = nanim;						//animator reference
		if (nHealthBar != null) healthBar = nHealthBar;			//UI health bar

		state = combatState.NORMAL;								//idle state
		nextActionTime = 0.0f;									//no previous action has been performed, so zero the cooldown period
	}

	//update the state with respect to time
	void Update()
	{
		if      (state == combatState.NORMAL) return; 		//if no actions are in progress, don't process further.
		else if (nextActionTime > Time.time)  
		{
			return; 		//if an action isn't completed, don't continue.
		}
		
		//reset to normal after any primary action has timed out. timings controlled in attributes area up top.
		else if (state == combatState.ATTACK || state == combatState.PARRY || state == combatState.TRINKET || state == combatState.STUNNED || state == combatState.HIT )
		{
			state = combatState.NORMAL;
            if (anim != null)
            {
                anim.SetBool("Attack", false);
                anim.SetBool("Parry", false);
                anim.SetBool("Hit", false);
            }
		}
	}

	//shoot a projectile forward. useful for turrets and ranged player attacks.
	public void shoot(GameObject projectile, Vector3 pos, Vector3 fwd, Quaternion rot, bool ahead)
	{
        if (debugMode) Debug.Log("Attempting to fire...");
		if (nextActionTime > Time.time) return;															//limits the fire rate
        if (debugMode) Debug.Log("Firing");
		GameObject pro;

		//keep this in here - flying turrets depend on it
		if (ahead)  pro = Instantiate(projectile, pos + (3.0f * fwd), rot) as GameObject;   			//shoot a projectile from ahead of you
		else pro = Instantiate(projectile, pos + (0.1f * fwd), rot) as GameObject;						//shoot a projectile from inches away

		try
		{
		pro.GetComponent<Projectile>().setAttributes(stats.projectileLifetime, stats.attack ,
	                                             stats,this.gameObject.GetComponent<Collider>()); 		//REPLACE THESE DEBUG VALUES LATER
			pro.GetComponent<Rigidbody>().velocity = Time.deltaTime * projectileVel * fwd;				//move projectile in fwd dir. 200 IS A DEBUG VALUE
			nextActionTime = Time.time + rangedAttackLength;											//store the time of the attack
		}
		catch(System.Exception e)
		{
			//when this fires, proj died before we could set its attributes. retarded.
			e.ToString();
			Destroy(pro);
		}
	}

	public void charge() { if (state == combatState.NORMAL) state = combatState.CHARGING; } 			//needs work later.
	public void attack() 
	{ 
		if (state == combatState.CHARGING || state == combatState.NORMAL)								//only attack if idle or charging is over with
		{
			state = combatState.ATTACK; 																//switch to attack state - MUST HAPPEN for attacks to register hits.
			nextActionTime  = Time.time + normalAttackLength;											//melee attack
			if (anim != null) anim.SetBool("Attack", true);																//tell animator to execute attack
		}
	}

	public void parry() 
	{ 
		if (state == combatState.NORMAL) 
		{
			if (debugMode) Debug.Log("PARRY!!");
			state = combatState.PARRY; 
			nextActionTime = Time.time + parryLength;
			anim.SetBool("Parry", true);
		}
	}

	public void trinket() 
	{ 
		if (state == combatState.NORMAL)
		{
			state = combatState.TRINKET;
			nextActionTime = Time.time + trinketLength;
		}
	}

	//take damage. called via projectiles, player scripts, and whatnot. EStats p refs attacking player (for transfers)
	public void damage(float d, EntityStats p)
	{
		//damage value calculation
		float temp = d - stats.getDefense();
		if (temp < 1) temp = 1;

		//the pain train has arrived
		stats.health -= temp;
        if (stats.hud != null)
        {
            stats.hud.updateHealthbar();
        }

        //if (debugMode) Debug.Log("Damage done: " + temp + " to " + stats.health);

        if (stats.health <= 0) death(p);
        else
        {
            //we're hit, so cancel whatever action was going on
            state = combatState.HIT;
            if (anim != null)
            {
                anim.SetBool("Hit", true);
                anim.SetBool("Attack", false);
                anim.SetBool("Parry", false);
            }
            nextActionTime = Time.time + 0.2f;
        }
	}

	//called whenever you need to get parried.
	public void stun()
	{
		state = combatState.STUNNED;
		nextActionTime = Time.time + stunLength;
	}

	//bite the dust, give resources to EStat p
	void death(EntityStats p)
	{
		if (debugMode) Debug.Log ("Player died.");
		//do nothing fancy for now - just respawn. turrets should override respawn and call a destroy or explode func or whatev
		stats.health = stats.maxHealth;
        
    
	    //turrets have no resources
	    if (p.resources != 0)
	    {
			try
			{
		      int transfer = this.stats.resources / 5;   //currently set at 20%
		      p.resources += transfer + 30;  //flatrate of 30 added
		      this.stats.resources -= transfer;
		      p.hud.updateResourceText();
		      this.stats.hud.updateResourceText();
			}
			catch (System.Exception e)
			{
				e.ToString();
			}
	    }


        if (owner.gameObject.GetComponent<Player>() != null)
        {
            owner.gameObject.GetComponent<Player>().respawn();
			stats.hud.updateHealthbar();
        }
        else if (owner.gameObject.GetComponent<CampTurret>() != null)
        {
            owner.gameObject.GetComponent<CampTurret>().respawn();
        }
		else if (owner.gameObject.GetComponent<FollowTest>() != null) owner.gameObject.GetComponent<FollowTest>().gotoBase();
        else
        {
            owner.respawn();
        }
	}
}
