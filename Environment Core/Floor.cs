using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

public class Floor : MonoBehaviour 
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	[SerializeField] bool active;										//switches to false if an Overlord is not in the scene
	[SerializeField] private float captureTimeThreshold = 0.0f; 		//default to instant capture, when no one owns this tile.
	[SerializeField] private float fullResistance = 0.0f;				//how long a captured tile resists re-capture
	[SerializeField] private float timeStayed = 0.0f; 					//time a player has stayed inside the tile.
	[SerializeField] private bool capturableFloor = true; 				//whether or not this tile can be captured

	private Player [] pls; 												//array of references to players in the scene
	int ownerID = -1; 													//which playerID owns this tile
	[SerializeField] private Tile tile;									//Tile data

	private Overlord overlord;											//reference to the overlord
	private ResourceSlice resMgr;										//resource slice reference
	private ResourceCore resCore;										//resource core reference
	private new Renderer renderer;										//renderer cache reference
	private Color myColor;												//emission color of the tile, tied to the player
	[SerializeField] Color fadeToColor;									//color to fade or lerp to
	//-----------------------------------------------------------------------------------------------------------------------------

	void Start()
	{
		fullResistance = 0;
		renderer = this.GetComponent<Renderer>(); 						//get a reference to the renderer
		myColor = renderer.material.GetColor("_EmissionColor");			//grab the emission color from the renderer

		//locate and cache important object pointers
		try
		{
			overlord = GameObject.FindGameObjectWithTag("master").GetComponent<Overlord>();
			resCore = overlord.resourceCore;
			resMgr = resCore.resourceSlice;
			pls = overlord.requestPlayerList();
			active = true;
		}
		catch (System.Exception e)
		{
			//failed to find an overlord in the scene, sp we'll deactivate this tile's logic.
			//this will happen on the menu scene every time
			e.ToString();
			active = false;
		}
	}

	//pulsate floor tile colors
	void Update() {	renderer.material.SetColor("_EmissionColor", Color.Lerp(myColor, fadeToColor, Mathf.Sin(Time.time * 2) / 1.2f)); }

	//useful for base mechanics later.
	public void setTile(Tile tyle) { tile = tyle; }

	//this function handles collisions with players
	void OnTriggerStay(Collider col)
	{
		if (active && capturableFloor && resMgr.isActive() && col.gameObject.tag == "Player")
		{
			Player player = col.gameObject.GetComponent<Player>();

			//if we already own this tile, restore its capture resistance 'HP' to full power.
			if (player.playerID == ownerID) 
			{
				timeStayed = 0.0f;
				captureTimeThreshold = fullResistance;
				return;
			}

			//update the time stayed inside the tile
			timeStayed += Time.deltaTime;

			//if the capture time threshhold has been met, capture this tile
			if (timeStayed > captureTimeThreshold) changeOwner(player);
		}
	}

	public void addStructure(Entity en)
	{
		//prototype code. modify me.
		if (tile != null) 
		{
			GameObject go = Instantiate(en, this.transform.position, this.transform.rotation) as GameObject;
			go.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + 1, this.transform.position.z);
		}
		else
		{
			Debug.LogWarning("can't add structure to tile: Cannot determine if tileType is floor.");
		}
	}

	//capture a tile / replace owner
	void changeOwner(Player player)
	{
		//swap owners
		int oldOwnerID = ownerID;
		ownerID = player.playerID;
		
		//once a tile is captured, it should resist future captures
		//the resistance will degrade the more it is stomped on.
		captureTimeThreshold = fullResistance;
		
		//update the counts of owned tiles
		for(int i=0;i<pls.Length;i++)
		{
			if (pls[i].playerID == ownerID)     
			{
				pls[i].stats.numTilesOwned++;
				pls[i].hud.updateTilesOwned();
			}
			else if (pls[i].playerID == oldOwnerID)  pls[i].stats.numTilesOwned--;
		}
		
		myColor = player.getColor();
		timeStayed = 0;
	}
}
