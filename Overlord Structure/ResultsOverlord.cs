using UnityEngine;
using System.Collections;

public class ResultsOverlord : MonoBehaviour {
	Player winner;
	float timeUntilNextMatch = 30.0f; //in seconds
	[SerializeField] private Transform playerSpawnLocation;
	
	// Use this for initialization
	void Start () {
		//Find the player
		GameObject temp = GameObject.FindGameObjectWithTag("Player");
		
		//put on our seat belts
		if(temp == null){
			Debug.LogError("No player detected in scene.");
			return;
		}
		
		winner = temp.GetComponent<Player>();
		
		if(winner == null){
			Debug.LogError("Detected player is corrupt.");
			return;
		}

		//move him to center screen
		winner.gameObject.transform.position = playerSpawnLocation.position;
		
		//Ditch his camera
		Camera cam = winner.GetComponentInChildren<Camera>();
		if(cam == null){
			Debug.LogError("No cameara attatched to player.");
			return;
		}
		cam.enabled = false;
		
	}
	
	// Update is called once per frame
	void Update () {
		timeUntilNextMatch -= Time.deltaTime;
		if(timeUntilNextMatch < 0.0f || Input.anyKey){
			Object.DestroyImmediate(winner);
			Application.LoadLevel("Menu");
		}
	}
}
