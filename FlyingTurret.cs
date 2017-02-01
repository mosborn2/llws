using UnityEngine;
using System.Collections;

public class FlyingTurret : Entity
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	bool active = false;																	//status of this script
	public int ownerID = 2;																			//playerID of owner
	float deathTime;																		//the time we run out of fuel and die
	float lifetime = 90.0f;																	//how long turrets live

	public GameObject thrusters;															//thrusters
	CombatSlice cSlice;																		//combat code
	public EntityStats stats;																//stats
	public GameObject projectile;															//object for shooting
	private GameObject target = null;														//target object
	//-----------------------------------------------------------------------------------------------------------------------------

	new void Start () 
	{
		if (active) return;
		cSlice = this.gameObject.AddComponent<CombatSlice>(); 								//instantiate a new combat Slice
		base.Start();																		//fire up the entity
		//turret is idle until init is called.
		active = true;
		init(ownerID);
	}

	public void init(int oid)
	{
		Debug.Log("Player " + oid + "'s Flying turret waking up!");
		ownerID = oid;																		//take in ownerID
		Start();
		stats = new EntityStats();															//instantiate stats
		stats.attack = 10;																//DEBUG NONSENSE - REMOVE ME LATER!!
		cSlice.init (stats, this, null, null);												//fire up combat
		cSlice.rangedAttackLength = 1.0f;
		deathTime = Time.time + lifetime;													//at this time, turret dies
		active = true;																		//turn on Update() since we're ready
	}
	
	void Update () 
	{
		if (active) 
		{
			if (deathTime < Time.time) Destroy(gameObject); 								//goodbye cruel world
			thrusters.transform.Rotate(new Vector3(0, 200, 0) * Time.deltaTime);			//rotate the thrusters

			if (target != null) 
			{
				transform.LookAt(target.transform.position);
				cSlice.shoot(projectile, this.gameObject.transform.position, this.gameObject.transform.forward, this.gameObject.transform.rotation, true);
			}
		}
	}

	void OnTriggerStay(Collider col)
	{
		if (!active) return;
		//find a new target
		if (col.gameObject.tag == "Player" || col.gameObject.tag == "Enemy" || col.gameObject.tag == "CampTurret")
		{
			//check if this player is the owner
			if (col.gameObject.tag == "Player" )
			{
				Player pl = col.GetComponent<Player>();
				if (pl.playerID == ownerID) return;
			}

			//shoot at suspicious characters
			if (col != target)
			{
				if (target == null) target = col.gameObject;
				else if (Vector3.Distance(this.transform.position, target.transform.position) > Vector3.Distance(this.transform.position, col.transform.position))
				     target = col.gameObject;
			}
		}
		if (target != null) Debug.Log(target.ToString());
	}

	void OnCollisionEnter(Collision col)
	{
		if (col.gameObject.tag == "Projectile")
		{
			Destroy(gameObject);
		}
	}

	//release the target
	void OnTriggerExit(Collider col)
	{
		if (col.gameObject == target)
		{
			target = null;
		}
	}
}
