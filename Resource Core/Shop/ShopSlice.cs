using UnityEngine;
using System.Collections;

public class ShopSlice : OverlordComponent
{
    
    
    public bool inShop = false;
    public bool active = false;
    int customer = 0;                 //number of customers in shop area
    int shopPtr = 0;                  //ptr to current item being looked at
    bool occupied = false;


    public GameObject UI;
    public GameObject myUI;
    HubAudio audio;

    void overlordSetup()
    {
        try
        {
            overlord = GameObject.FindWithTag("master").GetComponent<Overlord>();   //CRAP, FIXME
            myUI.GetComponent<InGameItemUI>().init();
            myUI.transform.SetParent(this.transform);
            myUI.transform.position = this.transform.position;
            myUI.transform.Translate(0, 0, -3f);
            myUI.SetActive(false);
            audio = this.GetComponentInParent<HubAudio>();
            active = true;
        }
        catch (System.Exception e)
        {
            active = false;
        }
    }

    public void Start()
    {
      myUI = Instantiate(UI);
      overlordSetup(); 
    }

    IEnumerator wait()
    {
      occupied = true;
      yield return new WaitForSeconds(.12f);
      occupied = false;
    }


    void OnTriggerEnter(Collider col)
    {
      if (!active) overlordSetup();

      if (col.GetComponent<Collider>().gameObject.tag == "Player")
      {
        ++customer;
        if (customer > 1)
        {
          myUI.SetActive(false);
          shopPtr = 0;
          inShop = false;
          //close store if two or more
          //indicate contested
        }
      }

    }

    void OnTriggerExit(Collider col)
    {
      if (!active) overlordSetup();
      if (col.GetComponent<Collider>().gameObject.tag == "Player")
      {
        --customer;
        if (inShop) audio.playSound(1);
        inShop = false;
        shopPtr = 0;
        myUI.SetActive(false);
      }
    }

    
    void OnTriggerStay(Collider col)
    {
        if (!active) overlordSetup();
        if (customer > 1) {  return; }   //return if too many customers
        if (col.GetComponent<Collider>().gameObject.tag == "Player")
        {

            Player p1 = col.GetComponent<Collider>().gameObject.GetComponentInParent<Player>();
            
            if (!occupied)
            {
              if (p1.gamepad.Y() && !inShop)   //open shop, disp 1st item
              {
                inShop = true;
                myUI.SetActive(true);
                shopPtr = 0;
                myUI.GetComponent<InGameItemUI>().loadItem(overlord.resourceCore.itemSlice.shop_inventory[shopPtr]);
                audio.playSound(0);
                StartCoroutine(wait());
              }
              else if (inShop) {

                if ( p1.gamepad.D_left() > 0)  //cycle through items while shop open,!!! D_left and right are system dependent?!
                {
                  if (shopPtr > 8) shopPtr = 0;
                  shopPtr++;
                  myUI.GetComponent<InGameItemUI>().loadItem(overlord.resourceCore.itemSlice.shop_inventory[shopPtr]);
                  audio.playSound(4);
                  StartCoroutine(wait());
                }
                else if (p1.gamepad.A())   //puchase item, redisplay comparisons
                {
                  overlord.resourceCore.resourceSlice.buyItem(col.GetComponent<Collider>().gameObject.GetComponentInParent<Player>().playerID, shopPtr);
                  myUI.GetComponent<InGameItemUI>().loadItem(overlord.resourceCore.itemSlice.shop_inventory[shopPtr]);
                  audio.playSound(2);
                  StartCoroutine(wait());
                }

                else if (p1.gamepad.B())   //close shop window
                {
                  inShop = false;
                  shopPtr = 0;
                  myUI.SetActive(false);
                  audio.playSound(1);
                  StartCoroutine(wait());
                }
                else if (p1.gamepad.D_down() > 0)  //buy atk bonus
                {
                  if (!overlord.resourceCore.itemSlice.buyBonus(p1, 0)) audio.playSound(1);
                  else audio.playSound(3);
                  StartCoroutine(wait());
                }
                else if (p1.gamepad.D_right() > 0)  //buy spd bonus
                {
                  if (!overlord.resourceCore.itemSlice.buyBonus(p1, 2)) audio.playSound(1);
                  else audio.playSound(3);
                  StartCoroutine(wait());
                }
                else if (p1.gamepad.D_up() > 0)  //buy armr bonus
                {
                  if (!overlord.resourceCore.itemSlice.buyBonus(p1, 1)) audio.playSound(1);
                  else audio.playSound(3);
                  StartCoroutine(wait());
                }
              }
            
            }
         
        
        }
        
    }
}
