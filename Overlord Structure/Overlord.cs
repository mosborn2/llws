﻿using UnityEngine;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;
using System.Text;
#endif

public class Overlord : MonoBehaviour
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	[SerializeField] bool debugMode = false;									//switch on to activate debug timer prints

    public EnvironmentCore environmentCore; 				//environment reference
    public ActorCore actorCore;								//actor reference
    public ResourceCore resourceCore;						//resource reference
    public CampCore campCore;               //campCore reference
	Player [] pls; 											//cached list of players for determining a victor

	public float gameLength = 5.0f; 						//length of the game, in minutes
	public float endTime;									//Time.time at the end of the game
	private float startupTime;								//Time.time at the start of the game

	bool gameRunning = false;								//state of the game. Terminates when there's a winner. 
	bool playersCached = false;								//whether or not the player cache has been built

  public OverlordAudio audio;        //audioController ref
	float nextDebugTick;									//time of the next Debug Tick
	//-----------------------------------------------------------------------------------------------------------------------------

 	public void EditorInit() { environmentCore.setOverlord(this); campCore.setOverlord(this); }

    void Start()
    {
        if (debugMode) Debug.Log("overlord start");

		initGameCoreFunctions();
		cachePlayers();
    	resourceCore.activate();
    	campCore.activate();

		gameTimerInit();
		gameRunning = true;
    audio = this.GetComponentInParent<OverlordAudio>();    //if broken, add OverlordAudio script
    }  

	void Update()
	{
		//if the game is still running...
		if (gameRunning)
		{
			debugInput();
			gameTimer();
		}
	}

	void gameTimer()
	{
		//normal timeout
		if (Time.time > endTime) gameTimeout();

		//debug
		else if (debugMode && nextDebugTick < Time.time)
		{
			if (debugMode) Debug.Log ("SECONDS LEFT = " + (endTime - Time.time));
			nextDebugTick = Time.time + 10.0f;
		}
	}	

	//this is only called by the resource slice to inform overlord of a victor
	public void hubsWon(Player pl)
	{
		Debug.Log ("GAME OVER - BOTH HUBS CAPTURED BY PLR " + pl.playerID);
		gameRunning = false;
		moveToResultsScene(pl);
	}

	//this fires when the game timer has expired
	private void gameTimeout()
	{
		//find the winner
		int largestResources = 0;
		int winnerID = -1;
		for(int i=0;i<pls.Length;i++)
		{
			if (pls[i].stats.resources > largestResources)
			{
				largestResources = pls[i].stats.resources;
				winnerID = i;
			}
		}
		Debug.Log ("GAME OVER - TIMEOUT WIN BY PLR " + winnerID);
		this.resourceCore.shutdown();
		gameRunning = false;
		moveToResultsScene(pls[winnerID]);
	}

	//set up the game timer
	void gameTimerInit()
	{
		startupTime = Time.time;
		endTime = startupTime + (60.0f * gameLength);
		nextDebugTick = Time.time;
	}
	
	//set up the overlord components
	void initGameCoreFunctions()
	{
		environmentCore.setOverlord(this);
		actorCore.setOverlord(this);
		resourceCore.setOverlord(this);
    	campCore.setOverlord(this);
	}

	void cachePlayers()
	{
		//avoid rework
		if (playersCached) return;
		
		//get references to all players in the scene
		GameObject [] gos = GameObject.FindGameObjectsWithTag("Player");
		
		//get the player script for all players in the scene, and cache the pointers
		pls = new Player[gos.Length];
		for(int i=0;i<pls.Length;i++) pls[i] = gos[i].GetComponent<Player>();
		
		//mark caching as complete
		playersCached = true;
	}
	
	//give a list of player references to requesting classes
	public Player[] requestPlayerList() 
	{ 
		if (playersCached) return pls;
		else
		{
			//this means the cache has been requested before it was built.
			cachePlayers();
			return pls;
		}
	}

	void debugInput()
	{
		if (Input.GetKeyDown(KeyCode.F1))
		{
			Debug.LogWarning ("players are being respawned at itreasure spawns?");
			for (int i = 0; i < 4; i++) actorCore.playerSlice.respawnPlayer(actorCore.playerSlice.players[i]);
		}
	}
	
	void moveToResultsScene(Player winner){
		//Dont destory players on load
		Object.DontDestroyOnLoad (winner);
		
		//load the next scene
		Application.LoadLevel("Results");
	}
}

//-----------------------------------------------------------------------------------------------------------------------------
//EDITOR RELATED CODE BELOW
//-----------------------------------------------------------------------------------------------------------------------------
#if UNITY_EDITOR
[CustomEditor(typeof(Overlord))]
public class OverlordEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(30);
        GUILayout.Label("Functions for use in editor");
        Overlord overlord = (Overlord)target;

        overlord.EditorInit();

        MapSlice mSlice = overlord.environmentCore.mapSlice;
                
        if (GUILayout.Button("Generate map from file"))
        {
            mSlice.InitMap();
			Debug.LogError ("[WARNING - Game Breaker] Don't forget to set center hub as uncapturable!");
        }

        if (GUILayout.Button("Clear Tiles"))
        {
            Transform[] res = mSlice.mapRoot.GetComponentsInChildren<Transform>();
            foreach (Transform t in res)
            {
                if (t != mSlice.mapRoot.transform && t != null)
                {
                    DestroyImmediate(t.gameObject);
                }
            }

        }

        if (GUILayout.Button("Toggle Hidden Tiles"))
        {
            mSlice.LoadTiles();
            for (int x = 0; x < 35; x++)
            {
                for (int y = 0; y < 35; y++)
                {
                    Tile t = mSlice.map.tMap[x, y];
                    if (t.hiddenBody)
                    {
                        if (t.bodyObj.activeInHierarchy)
                        {
                            t.bodyObj.SetActive(false);
                        }
                        else
                        {
                            t.bodyObj.SetActive(true);
                        }
                    }
                }
            }

        }
    }
}
#endif