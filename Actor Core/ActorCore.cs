using UnityEngine;
using System.Collections;

public class ActorCore : OverlordComponent
{
	public PlayerSlice playerSlice;

    new public void setOverlord(Overlord o)
    {
        this.overlord = o;
        playerSlice.setOverlord(o);
    }
}