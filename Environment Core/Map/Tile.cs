using UnityEngine;
using System.Collections.Generic;

public class Tile
{
    //Overlord
    public Overlord overlord;

    //From map file
    public byte bodyType;
    public byte region;
    public byte value;

    //From loading
    public int xPos;
    public int yPos;
    public bool hiddenBody = false;

    //From analysis
    public int spacialVal = 1;
    public int degree = 0;

    //From detection and loading
    public DefaultTiles.tType tileType = DefaultTiles.tType.error_error;
    public byte orientation = 0;
    public bool isFloor = false;
    public bool isWilderness = false;
    public bool isFloorCapturable = false;
    public bool respawnPoint = false;

    //From initializing
    public GameObject holder;
    public GameObject floorObj;
    public GameObject bodyObj;
    public GameObject rewardObj;
    public List<GameObject> artifacts;

    //Data
    public int Owner = -1;

    public void updateColor(Color cl)
    {
        MeshRenderer m = floorObj.GetComponent<MeshRenderer>();
        m.material.SetColor("EmissionColor", cl);
    }

}