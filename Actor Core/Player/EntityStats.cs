using UnityEngine;
using System.Collections;

public class EntityStats
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
  	public HUD hud; 												//heads up display reference 
  
    public int resources = 0;											//total resources or score
    public int numTilesOwned = 0;										//number of tiles owned. very important for resource slice
	public int numHubsOwned = 0;										//number of hubs owned. very important for resource slice

    public float health;												//hit points
	public float attack;												//attack power
	public float defense;												//damage resistance
	public float speed;													//speed of all actions
	public float luck;													//luck factor

	//BONUSES AND MODIFIERS FOLLOW - UNUSED FOR COMBAT RIGHT NOW
    public float maxHealth = 10;									    //max hit points
    public float healthChargeRate = 0; 									//HP per Second up to max
    public float healthChargeDelay = 1; 								//Time since last atttack
    public float damageModifier = 1;									//UNDOCUMENTED
    public float armorBonus = 1;  										//UNDOCUMENTED
	public float currentSpeed = 0;										//the current speed of the player
	public float topSpeed = 10; 										//movement top speed
	public float acceleration = 1; 										//movement acceleration
	public float speedBonus = 1; 										//UNDOCUMENTED
	public float maxChargeTime = 1;										//COMBAT RELATED
	public float attackTime = 1;										//UNDOCUMENTED
	public float attackDelay = 1;										//UNDOCUMENTED
	public float attackStaggerTime = 1;									//UNDOCUMENTED
	public float parryTime = 1;											//UNDOCUMENTED
	public float parryStaggerTime = 1;									//UNDOCUMENTED
	public float damageBonus = 1;										//damage bonus modifier
	public bool  rangedDamageType = false;								//UNDOCUMENTED - DEPRECATED
	public float meleeRange; 											//Should this be initialized?
	//public int   combatState = 0;									    //Combat State - DEPRECATED [CHANGES SOON]
	public float projectileSpeed = 1;									//ranged combat projectile speed
	public float projectileLifetime = 5;								//ranged combat projectile lifetime
	public bool invincible = false;										// if true player cannot be hit
	public bool canMove = true											//true if player should be able to move

	public Item mainHand;												//mainhand item ref
	public Item offHand;												//offhand item ref
	public Item armor;													//armor item ref
	public Item trinket;												//trinkey item ref

	//-----------------------------------------------------------------------------------------------------------------------------

	public float getDefense() { return defense; }

	public void statsInit()
	{
		health  = 5;													//set as a percentile for now. should make UI drawing easier.
		try
		{
			attack  = this.mainHand.getPower();
			defense = this.armor.getPower() + this.offHand.getPower();
		}
		catch(System.Exception e)
		{
			e.ToString();
			defense = 5;
			attack = 5;
			//update the UI here, probably.
		}
	}

	public enum ItemSlot
	{
		MAIN_HAND = 1,
		OFF_HAND,
		ARMOR,
		TRINKET,
	}

	public Item swapItem(Item i, ItemSlot slot)
    {
        switch (slot)
        {
            case ItemSlot.MAIN_HAND:
                i = mainHand;
                mainHand = i;
                break;
            case ItemSlot.OFF_HAND:
                i = offHand;
                offHand = i;
                break;
            case ItemSlot.ARMOR:
                i = armor;
                armor = i;
                break;
            case ItemSlot.TRINKET:
                i = trinket;
                trinket = i;
                break;
			default:
				Debug.LogError ("Swap Item attempt failure!");
				break;
        }
        recalculateStats();
        return i; // BEWARE, may return null;
    }

	//-----------------------------------------------------------------------------------------------------------------------------
	//EXPERIMENTAL CODE BELOW
	//-----------------------------------------------------------------------------------------------------------------------------
    public void addPowerup(Powerup p)
    {
		Debug.LogWarning("adding powerup");
        recalculateStats();        
    }

    public void recalculateStats()    //currently proof of concept, not at all balanced
    {
		//Debug.LogWarning("Recalculating stats for player");
      	//maxHealth 		 = ;
      	speed 			  = 5 + armor.getSpeed()/10;
      	topSpeed 		  = 5 + armor.getSpeed()/10;
      	acceleration 	  = 5 + armor.getSpeed()/10;    
      	attack 			  = 1 + mainHand.getPower() * damageBonus;
      	maxChargeTime     = 1 + mainHand.getPower();
      	attackTime        = 1 + mainHand.getSpeed() * speedBonus;
      	attackDelay       = 1 - mainHand.getSpeed() * speedBonus;
      	attackStaggerTime = 1 - mainHand.getSpeed() * speedBonus;
      	parryTime         = 1 + offHand.getSpeed()  /10;
      	parryStaggerTime  = 1 + offHand.getPower()  /10;
      	meleeRange        = 1 + mainHand.getRange();
        defense           = 1 + armor.getPower() * armorBonus;

      	this.hud.pullStats();
    }
}
