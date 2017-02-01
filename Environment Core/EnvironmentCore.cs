using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnvironmentCore : OverlordComponent
{
    public MapSlice mapSlice;

    new public void setOverlord(Overlord o)
    {
        this.overlord = o;
        mapSlice.setOverlord(o);
    }

    
}