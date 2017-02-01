using UnityEngine;
using System.Collections;

public class Powerup : MonoBehaviour
{
    //pointer to the resource manager
    Overlord overlord;
    
    //flag to ensure oncollisionstay does not submit resources to the manager twice
    bool active = false;
    bool used = false;
	int worth;

    //store Item in powerup
    public Item loot;
    public GameObject UI;
    public GameObject myUI;

    //short difficulty = 0;                    //pass in the difficulty modifier on instantiation
    // Use this for initialization



    void overlordSetup()
    {
      if (active == true) return;
      try
      {
        overlord = GameObject.FindWithTag("master").GetComponent<Overlord>();   //CRAP, FIXME
        active = true;
      }
      catch (System.Exception e)
      {
        active = false;
      }
    }


    public void init(Overlord o, int vx)
    {
      overlord = o;
		worth = vx;
      if (overlord == null) overlordSetup();
      else { active = true; }

      myUI = Instantiate(UI);
		try
		{
				
      myUI.GetComponent<InGameItemUI>().init();
      myUI.transform.SetParent(this.transform);
      myUI.transform.position = this.transform.position;
      myUI.transform.Translate(0, 0, -3f);
      myUI.SetActive(false);
		}
		catch(System.Exception e)
		{
			Debug.LogError ("@Morgan - Red alert, This function is busted at the init call!");
		}
    }


    public void loadItem(short difficulty)
    {

		try
		{
	      loot = overlord.resourceCore.itemSlice.genItem(3, difficulty);  //NOT 3 prolly
	      myUI.GetComponent<InGameItemUI>().loadItem(loot);
		}
		catch (System.Exception e)
		{
			e.ToString();
		}

    }

    public void item_destroy(Item item, Player p)
    {
      //here, we can either award a player resources based on what type of item it is.
      //or, we can give a random ammount of resources for any item, or whatnot.
 
		p.stats.resources += 100; //Division currently arbitrary
    }



    //should extend to chests
    public void giveItem(Item item, Player p)
    {
		Debug.LogWarning ("Morgan, need to know if this function is working fully");
		p.stats.resources += 100;
		return;

		//orig
      switch (item.getType())
      {
        case 0:
          p.stats.mainHand = item;
          break;
        case 1:
          p.stats.offHand = item;
          break;
        case 2:
          p.stats.armor = item;
          break;
        case 3:
          p.stats.trinket = item;
          break;
      }
      p.stats.recalculateStats();
      
    }
    void OnTriggerEnter(Collider col)
    {
      if (!active) overlordSetup();

      if (col.GetComponent<Collider>().gameObject.tag == "Player")
      {
          myUI.SetActive(true);
          
      }

    }

    void OnTriggerExit(Collider col)
    {
      if (!active) overlordSetup();
      if (col.GetComponent<Collider>().gameObject.tag == "Player")
      {
        
        myUI.SetActive(false);
      }
    }
  //this is killing me
  //void Update()
  //{
  //  this.transform.Rotate(new Vector3(0, 200, 0) * Time.deltaTime);
  //}

    void OnTriggerStay(Collider col)
    {
      if (!active) overlordSetup();
      if (used) return;
      if (col.GetComponent<Collider>().gameObject.tag == "Player")
      {

        Player p1 = col.GetComponent<Collider>().gameObject.GetComponentInParent<Player>();

        //Debug.Log("Player detected");
        //button handling: destory into resources
        if (p1.gamepad.B())
        {
          //destroy object into resources
          Debug.Log("Item destroyed into resources");
				p1.stats.resources += (200 * worth);

          //active = false;
          //item_destroy(loot, p1);
          //SFX
          Destroy(gameObject);
          used = true;
        }

        //button handling: pick up this item
        else if (p1.gamepad.A())
        {
          //for picking up items.
          //giveItem(loot, p1);
          Debug.Log("Item picked up");
          //SFX
				p1.stats.resources += (200 * worth);
          Destroy(gameObject);
          used = true;
        }
        

        
      }

    }
}
