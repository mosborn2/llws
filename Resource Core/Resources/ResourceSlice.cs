using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceSlice : OverlordComponent
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	[SerializeField] private bool active = false; 		//state of the resource slice
	[SerializeField] private bool debugMode = true;		//set to false to turn off debug prints

	private bool bothHubsHeld = false;					//true if a player holds both hubs
	private int potentialWinnerID;						//player ID of the person holding both hubs

	[SerializeField] private int multiplier;			//modify the rate at which resources are awarded
	[SerializeField] private int difficulty;			//we can increase the ammount of resources given during sudden death, etc.
	[SerializeField] private float tickLength = 3.0f;   //ammount of time in seconds between each resource update
	[SerializeField] private float nextTick;			//time when the next tick will occur
	[SerializeField] private int HUB_BONUS = 100; 		//ammount of resources awarded per tick for capturing a hub

	private ItemSlice itemMgr;							//reference to the item slice
	public Player [] pls;								//cached references to scene players
	public GameObject item;								//the generic item for spawning. Modify its attributes in code.
	private int nextHubID = 0;							//this is given to a requesting hub for keeping their ID's unique.
	float [] hubHoldTimes;								//for each player, track how long they hold both hubs
	float timeToHoldHubs = 10;								//how long both hubs must be held before a victor is declared

	public int initialItemsSeeded;						//[DEPRECATED] how many items to place in the map
	//-----------------------------------------------------------------------------------------------------------------------------

	new void setOverlord(Overlord o) { this.overlord = o; }					//set the overlord object [REQUIRED]
	public void Update()             { if (active && Time.time > nextTick) tick(); }
	public bool isActive()           { return active; }						//return state of the resource slice
	public void shutdown()           { active = false; }					//called by overlord to shut down resource tracking
	private void zeroHubTimes()      { for(int i=0;i<hubHoldTimes.Length;i++) hubHoldTimes[i] = 0.0f; }
	Player getPlayer(int id)         { return itemMgr.getPlayerByID(id); }	//return the requested player script via id
	
	//the resource manager will be dormant until this is called.
	public void activate()
	{
		//TEMPORARY: configure rate settings here
		multiplier = 1;
		difficulty = 1;

		//set the item manager
		itemMgr = overlord.resourceCore.itemSlice;	

		//initialize and zero the float holding array time
		hubHoldTimes = new float[pls.Length];
		zeroHubTimes();

		//set the first tick and activate
		nextTick = Time.time + tickLength;
		active = true;
	}



	//hubs call this to request a unique ID from this slice
	public int requestHubID()
	{ 
		if (nextHubID < 2)
		{
			nextHubID++;
			return nextHubID;
		}
		else
		{
			//do not remove these lines - to fix this error, check "Uncapturable" on the Hub script for the center hub.
			Debug.LogError ("More than 2 capturable hubs in the scene. Fix via the inspector.");
			Debug.DebugBreak();
			return -1;
		}
	} 	

	//called only when hub ownership changes from HUB
	public void checkWinPotential()
	{
		zeroHubTimes();
		for(int i=0;i<pls.Length;i++)
		{
			if (pls[i].stats.numHubsOwned >= 2)
			{
				Debug.Log ("Player " + pls[i].playerID + " is a potential winner");
				bothHubsHeld = true;
				potentialWinnerID = i;
				hubHoldTimes[potentialWinnerID] = Time.time + timeToHoldHubs;
				return;
			}
		}
		//no potential winner found.
		bothHubsHeld = false;
	}

	public int getHubBonus() { return HUB_BONUS; }

	//update resource counts
	private void tick()
	{
		if (active)
		{
			for(int i=0;i<pls.Length;i++)
		    {
				pls[i].stats.resources += pls[i].stats.numTilesOwned * multiplier * difficulty;
				pls[i].stats.resources += pls[i].stats.numHubsOwned * HUB_BONUS * multiplier * difficulty;
				pls[i].hud.updateResourceText();
		    }
		    
			//check for HUB win condition
			if (bothHubsHeld && hubHoldTimes[potentialWinnerID] < Time.time)
			{
				//inform the overlord.

				//shut down this module
				active = false;
				overlord.hubsWon(pls[potentialWinnerID]);
			}
		    nextTick = Time.time + tickLength;
		    //if (debugMode) DEBUG_printResourceData();
		}
	}
 
	//dump the current state of resources to the console.
	public void DEBUG_printResourceData()
	{
		string outStr = "Res Tick:   ";
		for (int i = 0; i < pls.Length; i++) outStr += "P" + pls[i].playerID + "(Reso=" + pls[i].stats.resources + " Tils=" + pls[i].getTilesOwned()+ " Hubs=" + pls[i].stats.numHubsOwned + ")   ";
		Debug.Log(outStr);
	}

	//itemNum refers to position in shop's inventory, should correspond visually in GUI storefront
	public bool buyItem(int plr_id, int itemNum)
	{
		if (debugMode) Debug.Log ("Rslice: buyItem() called");
		Player p = getPlayer(plr_id);
		
		//here, the value of the item can be determined by its ID in an item system. this line is placeholder.
		int itemValue = itemMgr.getPrice(itemNum);
		
		//can we buy this item or not?
		if (p.stats.resources > itemValue)
		{
			p.stats.resources -= itemValue;
      p.hud.updateResourceText();  //update resources on HUD
			itemMgr.transferItem(plr_id, itemNum);
			//tell the shop system to give the player this item
			//Debug.Log ("Player " + plr_id + " Bought itemID " + itemNum);
			return true;
		}
		else
		{
			//tell the shop system the card was declined
			if (debugMode) Debug.Log("Insufficient Resources :(");
			return false;
		}

	}

	//disabled for the demo.
	public void seedMapWithItems(int numItems)
	{
		Debug.LogWarning("SeedMapWithItems() called, but is no longer correctly implemented");

		//find the map manager
		Map mapData = overlord.environmentCore.mapSlice.map;

		//temporaries for map dimesions
		int tx = mapData.xSize;
		int ty = mapData.ySize;
		
		//randomly plop down some item cubes in the wilderness claimable tiles.
		//this currently ignores the region bits.
		short misses = 0;
		while (numItems > 0)
			for (int i = 0; i < tx; i++)
			{
				for (int j = 0; j < ty; j++)
				{
					Tile t = mapData.getTile(i, j);
					if (t.isFloor && t.isWilderness && numItems > 0)
					{
						if (Random.Range(1, 30) == 5)
						{
							item = (GameObject)Instantiate(item, new Vector3((float)(0), (float)0.7, (float)(0)), Quaternion.Euler(0, 0, 0));
							item.transform.SetParent(t.holder.transform);
							t.artifacts.Add(item);
							numItems--;
						}
					}
				}
			}
		//we will limit recycling to 3. If the map object is not found, this prevents an infinite loop condition.
		misses++;
		if (misses == 3) numItems = 0;
	}
}
 