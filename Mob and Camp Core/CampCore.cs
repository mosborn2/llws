using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CampCore : OverlordComponent
{

    public CampSlice campSlice;
    public List<Camp> camps;

    public MonsterBase mb1;
    public MonsterBase mb2;

    new public void setOverlord(Overlord o)
    {
        this.overlord = o;
        this.camps = o.environmentCore.mapSlice.camps;
        this.campSlice.setOverlord(o);
        mb1.region = 4;
        mb2.region = 5;
    }

    public void activate()
    {
        campSlice.spawnCamps(camps);
    }

    public void Update()
    {
        if (mb1 != null)
            mb1.Update();
        if (mb2 != null)
            mb2.Update();
    }
}
