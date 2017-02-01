using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemSlice : OverlordComponent
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
    public Item[] shop_inventory; 				//shop's inventory
	  public bool active = false;					//state of this slice

    public int statBase = 5; 					//base granular stat value, might be pulled from elsewhere
    public int bonus; 							  //size of bonus applied randomly to new items 
    public int gameState = 1; 					//temporary housing for gameState as it relates to scaling items


    public int[] diffTiers = { 100, 10, 5, 3, 2, 1 };  //array for difficulty tiers
    public Player[] playerList;					//list of player references
    //public ShopSlice shopMgr;					//shop slice reference
    public GameObject pickup;            //pickup reference
	//-----------------------------------------------------------------------------------------------------------------------------
   
    new void setOverlord(Overlord o)
    {
        this.overlord = o;
        //shopMgr = overlord.resourceCore.shopSlice;
        activate();
    }
    

  //returns generic starting item, takes a specified item type
    public Item genStdItem(short type)
    {
      int stat = statBase * gameState;
      Item tmp = new Item();
      tmp.set(stat, stat, stat, type);
      return tmp;
    }
  
  //returns specified type of item, consructs based on time and other variable factors
    public Item genItem(short type)    //type should eventually be enum'd   !!  MAY need to pass in random value instead of gen in function
    {
      int mod = (int) (Random.value * 3);         //randomly determines which stat gets boosted/which gets lowered
      int stat = statBase * (gameState + 1);
      bonus = stat / 4;
      //Debug.Log("stat " + stat + " mod " + mod);
      Item tmp = new Item();
      
      switch (mod){
        case 0:
          tmp.set(stat + bonus, stat - bonus, stat, type);
          break;
        case 1:
          tmp.set(stat, stat + bonus, stat - bonus, type);
          break;
        case 2:
          tmp.set(stat - bonus, stat, stat + bonus, type);
          break;
      }         
      
      return tmp;
    }

    public Item genItem(short type, short difficulty)    //type should eventually be enum'd   !!  MAY need to pass in random value instead of gen in function
    {
      int mod = (int)(Random.value * 3);         //randomly determines which stat gets boosted/which gets lowered
      int stat = statBase * (gameState + 1);
      bonus = stat / 4;
      stat += stat/diffTiers[difficulty];    //add the difficulty modifier to the stat base
      
      //Debug.Log("stat " + stat + " mod " + mod);
      Item tmp = new Item();

      switch (mod)
      {
        case 0:
          tmp.set(stat + bonus, stat - bonus, stat, type);
          break;
        case 1:
          tmp.set(stat, stat + bonus, stat - bonus, type);
          break;
        case 2:
          tmp.set(stat - bonus, stat, stat + bonus, type);
          break;
      }

      return tmp;
    }

    //For gearing players upon spawning 
    public void gearSpawnedPlayer(Player p)
    {
      p.stats.mainHand = genStdItem(0);
      p.stats.offHand = genStdItem(1);
      p.stats.armor = genStdItem(2);
      p.stats.trinket = genStdItem(3);
      p.stats.recalculateStats();
    }

   
    public void activate()
    {
        StartCoroutine("itemStaging");
        active = true;
    }

    public GameObject genPickup(short i)
    {
      Item item = genItem(i);
      GameObject pick = Instantiate(pickup);
      //pick.GetComponent<Powerup>().init(item, this.transform, overlord);
      return pick;
    }

    public GameObject genPickup(short i, short difficulty)
    {
      Item item = genItem(i, difficulty);
      GameObject pick = Instantiate(pickup);
      //pick.GetComponent<Powerup>().init(item, this.transform, overlord);
      return pick;

    }

    //builds shop inventory based on game state, variability not yet coded
    public void buildWares()
    {
        //clear shop inventory
      
        shop_inventory = new Item[9];
        int i = 0;

        //build mainHand Wares
        for (; i < 3; i++)
        {
          shop_inventory[i] = genItem(0);
             
        }
        //build offHand Wares
        for (; i < 6; i++)
        {
          shop_inventory[i] = genItem(1);
        }

        //build armor Wares
        for (; i < 9; i++)
        {
          shop_inventory[i] = genItem(2);
        }
    }

    


    //will be called from some shop code
    public bool buyBonus(Player p, short b)
    {

      if (p.stats.resources >= 5) //arbitrary atm, might not be here
      {
        p.stats.resources -= 5;
        p.stats.hud.updateResourceText();
        switch (b)
        {
          case 0:
            StartCoroutine(attackUp(p));
            break;
          case 1:
            StartCoroutine(armorUp(p));
            break;
          case 2:
            StartCoroutine(speedUp(p));
            break;
        }
        return true;
      }
      else
      {
        return false;
      }
    }

    IEnumerator attackUp(Player p)
    {
      p.stats.damageBonus = 1.5f;
      p.stats.recalculateStats();
      p.stats.hud.setAttackBonus(true);
      yield return new WaitForSeconds(10f);
      p.stats.hud.setAttackBonus(false);
      p.stats.damageBonus = 1.0f;
      p.stats.recalculateStats();
    }

    IEnumerator speedUp(Player p)
    {
      p.stats.speedBonus = 1.5f;
      p.stats.recalculateStats();
      p.stats.hud.setSpeedBonus(true);
      yield return new WaitForSeconds(10f);
      p.stats.hud.setSpeedBonus(false);
      p.stats.speedBonus = 1.0f;
      p.stats.recalculateStats();
    }

    IEnumerator armorUp(Player p)
    {
      p.stats.armorBonus = 1.5f;
      p.stats.recalculateStats();
      p.stats.hud.setArmorBonus(true);
      yield return new WaitForSeconds(10f);
      p.stats.hud.setArmorBonus(false);
      p.stats.speedBonus = 1.0f;
      p.stats.recalculateStats();
    }

    
    public int getPrice(int itemNum)
    {
        return shop_inventory[itemNum].getCost();
    }
    
    public void transferItem(int plr_id, int itemNum)
    {
      //Debug.Log(plr_id + " " + itemNum);
      Player p = getPlayerByID(plr_id);
      short type = shop_inventory[itemNum].getType();

      if (type == 0) { DestroyObject(p.stats.mainHand); p.stats.mainHand = shop_inventory[itemNum]; }
      if (type == 1) { DestroyObject(p.stats.offHand); p.stats.offHand = shop_inventory[itemNum]; }
      if (type == 2) { DestroyObject(p.stats.armor); p.stats.armor = shop_inventory[itemNum]; }
      if (type == 3) { DestroyObject(p.stats.trinket); p.stats.trinket = shop_inventory[itemNum]; }
      //shop_inventory[itemNum] = genItem(0); //set to gen mainHand for DEBUG TESTING, should normally be empty after purchase
      p.stats.recalculateStats();
    }

    IEnumerator itemStaging(){
      while (true)
      {
        gameState++;
        //Debug.Log("gameState = " + gameState);
        buildWares();
        yield return new WaitForSeconds(30f);
      }
    }

    public Player getPlayerByID(int pid)
    {
      foreach (Player p in playerList)
      {
        if (p.playerID == pid)
        {
          return p;
        }
      }
      return null; //will gen errors
    }


    //public void DEBUGInventory()
    //{
    //  Debug.Log(plr_inventory[0].mainHand.GetComponent<Item>().getName());
    //}

    public void DEBUGstats()
    {
      //foreach (Player p in playerList)
      //{
      //  Debug.Log("power: " + p.stats.mainHand.getPower() + " damage: " + p.stats.damage + " name: " + p.stats.mainHand.getName() + " ID: " + p.stats.mainHand.getid());
      //}
      Debug.Log("name " + playerList[0].stats.mainHand.getName() + " pow " +  playerList[0].stats.mainHand.getPower() + " ran " + playerList[0].stats.mainHand.getRange() + " spe " + playerList[0].stats.mainHand.getSpeed());
      Debug.Log("name " + playerList[0].stats.offHand.getName() + " pow " + playerList[0].stats.offHand.getPower() + " ran " + playerList[0].stats.offHand.getRange() + " spe " + playerList[0].stats.offHand.getSpeed());
      Debug.Log("name " + playerList[0].stats.armor.getName() + " pow " + playerList[0].stats.armor.getPower() + " ran " + playerList[0].stats.armor.getRange() + " spe " + playerList[0].stats.armor.getSpeed());
      Debug.Log("name " + playerList[0].stats.trinket.getName() + " pow " + playerList[0].stats.trinket.getPower() + " ran " + playerList[0].stats.trinket.getRange() + " spe " + playerList[0].stats.trinket.getSpeed());
    }

    // Use this for initialization
    void Start()
    {
     
    }
    
    // Update is called once per frame
    void Update()
    {
	  if (Input.GetKeyDown(KeyCode.I))
      {
        DEBUGstats();
      }

      if (Input.GetKeyDown(KeyCode.J))
      {
        transferItem(1, 0);
        Debug.Log("transferring item to P1");
        Debug.Log("" + playerList[0].stats.numHubsOwned + playerList[1].stats.numHubsOwned + playerList[2].stats.numHubsOwned + playerList[3].stats.numHubsOwned);
        //remove
        //overlord.environmentCore.mapSlice.spawnCamps(overlord.campCore.camps);
      }

      if (Input.GetKeyDown(KeyCode.P))
      {
        buyBonus(playerList[0], 0);
        Debug.Log("purchasing bonus for P1");
      }
      
       
    }
}
