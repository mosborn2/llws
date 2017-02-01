using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampSlice : OverlordComponent
{

    public int spawnPtFree = 0;

    new public void setOverlord(Overlord o)
    {
        overlord = o;
    }

    bool initCamps = false;

    //goes through camp tile list and sets them up according to Tile.value and (current Campstate !!Not yet imp'd). Monster Camps not imp'd
    public void spawnCamps(List<Camp> camps)
    {
        float rando;
        spawnPtFree = 0;
        bool[] cTest = new bool[camps.Count];
        //init and data retrieval loop  (Only do this once if you can lock down inc and dec on change of camp state)

        if (!initCamps)
        {
            foreach (Camp c in camps)
            {
                c.init();
            }
        }

        for (int i = 0; i < camps.Count; i++)
        {
            if (camps[i].campState == CampState.EMPTY)
            {
                cTest[i] = true;
                spawnPtFree++;
            }
            else
            {
                cTest[i] = false;
            }
        }

        int maxIterations = 100;

        while (spawnPtFree > 4)
        {
            int test = UnityEngine.Random.Range(0, camps.Count);
            while (!cTest[test]) { test = ++test % camps.Count; }
            cTest[test] = false;
            spawnPtFree--;
            if (UnityEngine.Random.value < 0.1)
            {
                camps[test].setState(CampState.CHEST);
            }
            else
            {
                camps[test].setState(CampState.TURRET);
            }
            if ((28 - spawnPtFree) / 24 > UnityEngine.Random.Range(0.4f, 1.0f)) break;
            if (--maxIterations < 0) break;
        }


        /*
      //set new camp states
      foreach (Camp c in camps)
      {
        if (spawnPtFree > 4)         //make sure there are atleast 4 empty spaces for respawns
        {
          rando = UnityEngine.Random.value;

          if (rando < 0.7) {                  //arbitrary two
            c.setState(CampState.TURRET); 
            if (c.respawnPoint == true) 
              --spawnPtFree; 
          }
          else if (rando < .10) { 
            c.setState(CampState.CHEST);
            if (c.respawnPoint == true) 
              --spawnPtFree; 
          }
          else 
            c.setState(CampState.EMPTY);


        }
        else
        {
            break;
        }
      } 
         */
    }
}
