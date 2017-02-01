using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum CampState
{
    EMPTY = 0,
    TURRET,
    CHEST,
}

public class Camp
{

    //ENV Stuff
    public Tile tile;
    public bool respawnPoint;
	bool debugMode = false;

    //Camp Stuff
    GameObject turret;
    GameObject chest;
    public bool isEmpty = true;

    public CampState campState;

    public Camp(Tile t)
    {
        tile = t;
        respawnPoint = t.value < 3;
    }

    public void init()
    {
        turret = tile.bodyObj;
        chest = tile.rewardObj;

//        turret.GetComponent<CampTurret>().init(this, tile.overlord);
        turret.SetActive(false);

        chest.SetActive(false);
        turret.GetComponent<CampTurret>().init(this, tile.overlord);
        if (tile.region == 4 || tile.region == 5)
        {
            if (debugMode) Debug.Log("Found Base Turret!");
        }

    }

	//call this again if the powerup failed to init
	void initPowerup()
	{
		try
		{
			chest.GetComponent<Powerup>().init(tile.overlord, tile.value);
		}
		catch(System.Exception e)
		{
			Debug.LogWarning("cound not initialize a powerup");
		}
	}

    public void setState(CampState state)
    {
        /*
         * States:
         *   Environment Core should call to set the TURRET State
         *   CampTurret should call to set the CHEST State
         *   CampChest should call to set the EMPTY State
         * 
         * Camp needs to first be fully integrated into the environment, look at the existing code and I will help you with this. 
         * We will use a prefab for the chest and turret, with stubbed and incomplete code. I will be fleshing out Camp Turret to have ready for you tomorrow. 
         * You will need to add both to the DefaultTiles and Map object. Spawn them as children to the Tile Holder and Tag them as shown in MapSlice.
         * -Craig
         * 
         */

        this.campState = state;
        switch (state)
        {
            case CampState.TURRET:
                turret.GetComponent<CampTurret>().stats.health = 10 * tile.value;
                turret.GetComponent<CampTurret>().reset();
                turret.SetActive(true);
                chest.SetActive(false);
                isEmpty = false;
                if (debugMode) Debug.Log("Turret made");
                respawnPoint = false;
                break;
            case CampState.CHEST:
                turret.SetActive(false);
                chest.SetActive(true);
                chest.GetComponent<Powerup>().init(tile.overlord, tile.value);
                chest.GetComponent<Powerup>().loadItem(tile.value);     //may not be tile.value
                isEmpty = false;
                if (debugMode) Debug.Log("Chest made");
                respawnPoint = false;
                break;
            case CampState.EMPTY:
                turret.SetActive(false);
                chest.SetActive(false);
                isEmpty = true;
                if (debugMode) Debug.Log("Empty Camp made");
                respawnPoint = true;
                //set off some sort of respawn timer
                break;
        }





    }
}
