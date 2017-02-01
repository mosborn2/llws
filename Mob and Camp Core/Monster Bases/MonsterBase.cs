using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterBase
{
    public int region;
    public Camp c1;
    public Camp c2;
    public GameObject dec;
    public GameObject chest;

    public void init()
    {

    }

    bool active = false;

    public void Update()  //Called in CampCore slice
    {
        if(active)
        {

        } else
        {
            if(c1 != null && c2 != null && dec != null && chest != null)
            {

            }
        }
    }
}
