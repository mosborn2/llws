using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerSlice : OverlordComponent
{
    public Player[] players;

    [Tooltip("Minimum respawn distance in tiles")]
    [SerializeField] int respawnDistance;
    

    [Tooltip("The color to set to each of the players.")]
    public List<Color> playerColors;

    [Tooltip("The prefab the player will use to be spawned in")]
    [SerializeField] GameObject playerPrefab;
    
    [SerializeField] FillScript p1HealthBar;
	[SerializeField] FillScript p2HealthBar;
	[SerializeField] FillScript p3HealthBar;
	[SerializeField] FillScript p4HealthBar;

    [Tooltip("The prefab the player will use to be spawned in")]
	private FillScript[] healthBars;
	
    public int TileCount;

    public List<Tile> respawnPoints; //EMPTY ATM. DO NOT USE. -Craig
	
	void Start(){
		healthBars = new FillScript[] {p1HealthBar,p2HealthBar,p3HealthBar,p4HealthBar};
        respawnPoints = new List<Tile>();
	}
	
    new public void setOverlord(Overlord o){
        this.overlord = o;
        loadUserPlayer();
    }
	
	//Load all players in through here. 
    //Instead set the players in the editor.
    void loadUserPlayer(){
        
        healthBars = new FillScript[] { p1HealthBar, p2HealthBar, p3HealthBar, p4HealthBar };

        for(int i=0; i < players.Length; i++){
    		players[i].init(i+1, 4, playerColors[i], healthBars[i], this.overlord);
        
    	}

      //hand out player list to those who need it after it's built
      overlord.resourceCore.itemSlice.playerList = players;
      overlord.resourceCore.resourceSlice.pls = players;
    }

    public void respawnPlayer(Player p)
    {
        if (overlord != null)
        {
            //try and place randomly.
            List<Camp> camps = overlord.campCore.camps;
			Debug.Log ("EMPTY CAMPS === " + camps.Count);
            int initial = Random.Range(0, camps.Count);
            int attempt = initial;
            do
            {
                attempt++;
                attempt %= camps.Count;

                int pCheck = 0;
                if (camps[attempt].respawnPoint)
                {
                    for (pCheck = 0; pCheck < players.Length; pCheck++)
                    {
                        if (players[pCheck] != p)
                        {
                            if (Vector3.Distance(players[pCheck].transform.position, camps[attempt].tile.floorObj.transform.position) < respawnDistance * 2.0)
                            {
                                break;
                            }
                        }
                    }
                }

                if (pCheck == players.Length)
                {
                    p.transform.position = camps[attempt].tile.floorObj.transform.position;
					p.transform.position = new Vector3(p.transform.position.x, 0, p.transform.position.z);
                    overlord.resourceCore.itemSlice.gearSpawnedPlayer(p);   //spawn with new standard gear
                    Debug.Log("Player " + p.playerID + " respawned at value " + camps[attempt].tile.value +  " with initial " + initial + " and attempt " + attempt);
                    break;
                }
            } while (attempt != initial);

            if (attempt == initial)
            {
                Debug.Log("No suitable respawn points");
                //Insert failsafe.
				p.transform.position = p.respawnPoint;
            } 

        }
    }
}