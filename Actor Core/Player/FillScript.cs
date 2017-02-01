using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class FillScript : MonoBehaviour 
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	public float maxHealth = 100;							//maximum health (don't change this for now)
	public float curHealth = 100;							//current health (initialized to max)

	public RectTransform health;						//rect container
	public float healthBarLength;						//length of the heath bar
	public Image healthcolor;							//color of the bar, to match its player
	//-----------------------------------------------------------------------------------------------------------------------------
	void Start () {	healthBarLength = 193; } 			//just a static size

	public void AddjustCurrentHealth(float adj)
	{
        curHealth = adj;

        if (maxHealth < 1) maxHealth = 1;
		
		if      (curHealth <0) curHealth = 0;
		else if (curHealth >   maxHealth)curHealth = maxHealth;

		
		healthBarLength = (193) * (curHealth / (float)maxHealth);
	}

	//Update should be removed, use the event-driven approach instead
	void Update () 
	{
		AddjustCurrentHealth(curHealth);//GETHEALTH FROM WHEREVER.
		health.SetSizeWithCurrentAnchors (RectTransform.Axis.Horizontal, healthBarLength);
	}
}
