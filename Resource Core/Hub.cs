using UnityEngine;
using System.Collections;

public class Hub : MonoBehaviour 
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	private bool debugMode = false;										//set to false to disable debug log statements

	private Overlord overlord;											//overlord reference
	private ResourceCore resCore;										//resource core reference
	private ResourceSlice resMgr;										//resource slice reference
	private Player [] pls;												//player reference list
	private ItemSlice itemMagr;

	private float [] timeStayed;										//ammount of time a player is within the capture radius
	private float captureTime = 10.0f; 									//How long the player must be in a hub to capture it

	[SerializeField] private int ownerID; 								//playerID that owns this hub. negative if no owner.
	[SerializeField] private int hubID; 								//"The ID of this hub. Assigned by resource slice."

	public  bool uncapturable  = false;									//set to true to prevent any capture of this hub
	private bool captureLocked = false;									//for blocking captures during important operations
	//-----------------------------------------------------------------------------------------------------------------------------

	// Use this for initialization
	void Start() 
	{
		//cache references to all the things
		updateReferences();

		//keep track of how long each player has stayed inside the capture zone.
		timeStayed = new float[pls.Length];

		//default configurations
		ownerID = -1;

		//request a HUB ID from the resource manager.
		if (!uncapturable) 
		{
			hubID = resMgr.requestHubID();
			if (debugMode) Debug.Log ("HUB ID " + hubID + " initialized.");

      //if (hubID == 2) uncapturable = true;  //THIS IS SOME DEBUG BULLSHIT
		}
	}

	//locate and cache a pointer to the resource manager
	void updateReferences()
	{
		overlord = GameObject.FindGameObjectWithTag("master").GetComponent<Overlord>();
		resCore = overlord.resourceCore;
		itemMagr = overlord.resourceCore.itemSlice;
		resMgr = resCore.resourceSlice;
		pls = overlord.requestPlayerList();

	}

	void OnTriggerStay(Collider col)
	{
		//if we can't capture this hub, don't bother processing further.
		if (captureLocked || uncapturable || !resMgr.isActive()) return;

		if (col.GetComponent<Collider>().gameObject.tag == "Player")
		{
			//grab the player
			Player player = col.gameObject.GetComponent<Player>();
			
			//safety check, and if we already own this tile, don't waste any more time
			if (player == null || player.playerID == ownerID) return;

			//update the time stayed inside the hub
			timeStayed[player.playerID - 1] += Time.deltaTime;

			//stop here if the player hasn't been here long enough to perform a capture.
			if (timeStayed[player.playerID - 1] < captureTime) return;

			//immediately lock this hub while capturing is being processed, to avoid problems related by the async calling of this function.
			captureLocked = true;

			Debug.Log ("HUB ID " + hubID + " Captured By Player " + (player.playerID));

			//zero timestayed for all players - so no one already in the hub can quickly recapture
			for(int i=0;i<timeStayed.Length;i++) timeStayed[i] = 0.0f;

			//swap owners
			int oldOwnerID = ownerID;
			ownerID = player.playerID;

			//update the player's stats
			for(int i=0;i<pls.Length;i++)
			{
				if (pls[i].playerID == ownerID) pls[i].stats.numHubsOwned++;
				else if (pls[i].playerID == oldOwnerID) pls[i].stats.numHubsOwned--;
			}

			//perform a UI update
      if (ownerID > -1)
      {
        pls[ownerID - 1].hud.updateTilesOwned();
        if (oldOwnerID >= 0) pls[oldOwnerID - 1].hud.updateTilesOwned();
      }

			resMgr.checkWinPotential();

			//unlock capturing
			captureLocked = false;

		}
	}

	//zero the player's timeStayed if they leave.
	void OnTriggerExit(Collider col)
	{
		if (col.GetComponent<Collider>().gameObject.tag == "Player")
		{
			//grab the player
			Player player = col.gameObject.GetComponent<Player>();
			timeStayed[player.playerID - 1] = 0.0f;

			if (debugMode) Debug.Log ("PlayerID " + player.playerID + " exited hub " + hubID);
		}
	}
}
