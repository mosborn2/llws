using UnityEngine;
using System.Collections;

public class Projectile : MonoBehaviour 
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	bool active = false;												//lifetime limiting is disabled while this is false
	float lifetime = 99999; 											//how long before projectile dissipates
	float damage = 0;													//damage this projectile inflicts
  EntityStats owner;
  Collider cOwner;
	//-----------------------------------------------------------------------------------------------------------------------------

  public void setAttributes(float l, float d, EntityStats e, Collider c) { lifetime = Time.time + l; damage = d; active = true; owner = e; cOwner = c; Physics.IgnoreCollision(this.GetComponent<Collider>(), c); }

	void OnCollisionEnter(Collision col)
	{
		if (!active) return; //dont remove this
		if      (col.gameObject.tag == "Projectile") { Physics.IgnoreCollision(GetComponent<Collider>(), col.collider); return; }
		else if (col.gameObject.tag == "Player")
        {
			Player pl = col.gameObject.GetComponent<Player>();

			//deflect projectiles
			if (pl.GetComponent<CombatSlice>().getState() == CombatSlice.combatState.ATTACK)
			{
				this.GetComponent<Rigidbody>().velocity = -this.GetComponent<Rigidbody>().velocity;
				this.GetComponent<Rigidbody>().velocity *= 2;
				Physics.IgnoreCollision(this.GetComponent<Collider>(), col.collider);
                Physics.IgnoreCollision(this.GetComponent<Collider>(), cOwner, false);
				damage *= 2;
				return;
			}
			//regular hit

			else pl.cSlice.damage(damage, owner); 	//hit player, second arg may be wrong
			Debug.Log ("DAMGE=" + damage);

		}
		else if (col.gameObject.tag == "Enemy")
		{
			col.gameObject.GetComponent<FollowTest>().gotoBase();
		}
        else if (col.gameObject.tag == "CampTurret")
        {
            if (col.gameObject.GetComponent<CampTurret>().stats == owner)
            {
                Physics.IgnoreCollision(GetComponent<Collider>(), col.collider); return;
            }

            Debug.LogWarning("Turret hit unimplemented");					//hit turret
        }

		Destroy(gameObject);																					//hit wall or unknown obj
	}

	void Update() { if (active && lifetime < Time.time) Destroy(gameObject); } 									//ran out of life time
}