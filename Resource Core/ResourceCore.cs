using UnityEngine;
using System.Collections;

public class ResourceCore : OverlordComponent
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
    public ItemSlice itemSlice;					//reference to items manager
    public ResourceSlice resourceSlice;			//reference to resource manager
    public ShopSlice shopSlice;					//reference to shops manager
	public bool active = false;					//state of the resource core
	//-----------------------------------------------------------------------------------------------------------------------------

	//called by the overlord to configure this core
    new public void setOverlord(Overlord o)
    {
		//set the overlord reference to all objects managed in this core
        this.overlord = o;
        itemSlice.setOverlord(o);
        resourceSlice.setOverlord(o);
//        shopSlice.setOverlord(o);
    }

	//called by the overlord to start the game
	public void activate()
	{
		//IN PROGRESS: activate all submanagers
		resourceSlice.activate();
    	itemSlice.activate();

		//mark this core as active
		active = true;
	}

	//called when the game has been won
	public void shutdown() 
	{
		//IN PROGRESS: Shut down all submanagers
		this.resourceSlice.shutdown();	

		//mark this core as inactive
		active = false;
	}
}