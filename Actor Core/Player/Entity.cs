using UnityEngine;
using System.Collections;

public class Entity : MonoBehaviour
{
    //public CombatSystem combatSystem;
    protected Overlord overlord;
	public Vector3 respawnPoint;

    public void Start()
    {

    }

	public void respawn()
	{
        //this.transform.position = respawnPoint;
    // overlord.resourceCore.itemSlice.gearSpawnedPlayer(this.GetComponentInChildren<Player>());
	}

    //A value between 0 and 3. Should be overridden in the player class.
    //0-1 is charging
    //1-2 is attacking
    //2-3 is recovering
    //This may be replaced by animations
}

