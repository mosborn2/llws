using UnityEngine;
using System.Collections;

//testin class as object

public class Item : Object
{

    /*
    public enum iType : short
      {
          mainHand, offHand, armor, trinket
      }
     */

    //used for damage on offensive weapons, resistance on defensive items, or a scalar for trinkets
    [SerializeField]
    private int power;

    //range of main and offhand weapons, or trinket ability
    [SerializeField]
    private int range;

    //speed of main weapons, or parry speed, or trinket ability
    [SerializeField]
    private int speed;

    //name and ID of the item
    [SerializeField]
    private string itemName;
    [SerializeField]
    private int id;
    
    private static int IDpool = 1; //for unique IDs
  
    
    //cost of item
    [SerializeField]
    private int cost;

    //item type
    [SerializeField]
    private short type; //0 mainHand, 1 offHand, 2 armor, 3 trinket


    //may contain fields for unique audio, mesh, and method(for trinkets) profiles

    public void empty()
    {
        power = 1;
        range = 1;
        speed = 1;
        itemName = "Cat Wrangler";
        id = genID();
        type = 0;
        cost = 1;
    }

    
    public void set(int p, int r, int s, short t)
    {
        power = p;
        range = r;
        speed = s;
        id = genID();
        itemName = nameGen(p, s, r, t) + " " + id.ToString();
        type = t;
        cost = genCost(p, r, s);
        if (t == 3) cost *= 3;   //triple trinket cost
        
    }

    public int getPower() { return power; }
    public int getRange() { return range; }
    public int getSpeed() { return speed; }
    public string getName() { return itemName; }
    public int getCost() { return cost; }
    public int getid() { return id; }
    public short getType() { return type; } 


    //currently simple, should be tweaked
    private static int genCost(int p, int s, int r)
    {
      int cost = p + s + r;
      return cost;
    }


    private static string nameGen(int p, int s, int r, int t){
      string str = "";
      int max = findMax(p, r, s);
      
      switch(max)
      {
        case 1:
          str += "Strong ";
          break;
        case 2:
          str += "Massive ";
          break;
        case 3:
          str += "Fast ";
          break;
        default:
          str += "Sloth ";
          break;
      }

      switch (t)
      {
        case 0:
          str += "Sword";
          break;
        case 1:
          str += "Shield";
          break;
        case 2:
          str += "Plate";
          break;
        case 3:
          str += "Totem";
          break;
        default:
          break;
      }
      
      return str;
    }


    public static int findMax(int p, int r, int s)
    {
      if (p > r && p > s)  //power
      {
        return 1;
      }
      else if (r > p && r > s) //range
      {
        return 2;
      }
      else if ( s > p && s > r) //speed
      {
        return 3;
      }
      else
      {
        return 4;
      }
    }

    private static int genID()
    {
      return IDpool++;
    }
    void Start()
    {

    }

}