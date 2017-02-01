using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapSlice : OverlordComponent {
    
    public DefaultTiles dTiles;
    public GameObject mapRoot;
    public TextAsset mapText;
    public Map map;
    public List<Camp> camps;  

	//Initialize the Map Object
    void Start()
    {
        
	}

    new public void setOverlord(Overlord o)
    {
        overlord = o;
        LoadTiles();
    }

    void Update()
    {
        UpdateTileOwners();
    }

    void UpdateTileOwners()
    {
        Player[] ps = overlord.actorCore.playerSlice.players;
        for (int i = 0; i < ps.Length; i++ )
        {
            Player p = ps[i];
            if (p != null)
            {
                int x = 0;
                int z = 0;


                if ((x % 4 == 1 || x % 4 == 2) && (z % 4 == 1 || z % 4 == 2))
                {
                    //map.tMap[x / 4, z / 4].claimTile(p.playerID);
                }
            }
        }
    }

    public void LoadTiles()
    {
        map = new Map(dTiles);
        map.mapRoot = mapRoot;
        map.loadMap(mapText.text);
        overlord.campCore.mb1 = new MonsterBase();
        overlord.campCore.mb2 = new MonsterBase();


        if (overlord != null)
        {
            camps = new List<Camp>();
        }

        int tileHolderCount = 0;

        Transform[] tiles = map.mapRoot.GetComponentsInChildren<Transform>();
        foreach (Transform t in tiles)
        {
            if (t.tag == "TileHolder")
            {
                tileHolderCount++;

                string name = t.name;
                int comma = name.IndexOf(',');
                if (comma > 0)
                {
                    int x = Convert.ToInt32(name.Substring(5, (comma - 5)));
                    int y = Convert.ToInt32(name.Substring(comma + 1, name.Length - comma - 2));
                    map.tMap[x, y].holder = t.gameObject;

                    Transform[] children = t.GetComponentsInChildren<Transform>();
                    //bool isCamp = false;
                    //Camp camp;
                    Tile tile = map.tMap[x, y];
                    foreach (Transform k in children)
                    {
                        if (k.tag == "BodyObj")
                        {
                            tile.bodyObj = k.gameObject;
                        }
                        else if (k.tag == "FloorObj")
                        {
                            tile.floorObj = k.gameObject;
                            //This code breaks the editor.
                           /* if (tile.isFloorCapturable)
                            {
                                tile.updateColor(overlord.actorCore.playerSlice.playerColors[0]);
                            }*/

                            if (tile.tileType == DefaultTiles.tType.struct_camp)
                            {

                                if (overlord != null)
                                {
                                  tile.overlord = overlord;
                                    if(tile.region == 5)
                                    {
                                        if(overlord.campCore.mb2.c1 == null)
                                        {
                                            overlord.campCore.mb2.c1 = new Camp(tile);
                                        } else
                                        {
                                            overlord.campCore.mb2.c2 = new Camp(tile);
                                        }
                                    }  else if(tile.region == 4)
                                    {
                                        if (overlord.campCore.mb1.c1 == null)
                                        {
                                            overlord.campCore.mb1.c1 = new Camp(tile);
                                        }
                                        else
                                        {
                                            overlord.campCore.mb1.c2 = new Camp(tile);
                                        }

                                    } else
                                    {
                                        camps.Add(new Camp(tile));
                                    }
                                }
                            }
                        } else if(k.tag == "mbHub")
                        {
                            if (tile.region == 5)
                            {
                                overlord.campCore.mb2.dec = k.gameObject;
                            }
                            else if (tile.region == 4)
                            {
                                overlord.campCore.mb1.dec = k.gameObject;

                            }
                        } else if(k.tag == "mbChest")
                        {
                            if (tile.region == 5)
                            {
                                overlord.campCore.mb2.chest = k.gameObject;
                            }
                            else if (tile.region == 4)
                            {
                                overlord.campCore.mb1.chest = k.gameObject;

                            }
                        }
                        //else if (k.tag == "ArtifactObj")
                        //{
                        //    map.tMap[x, y].artifacts.Add(k.gameObject);
                        //}
                        //NO ELSE CASE ON PURPOSE!
                    } 

                 //Loop after if it is a camp
                    foreach (Transform k in children)
                    {
                        if (k.tag == "CampTurret")
                        {
                            if (k.gameObject.GetComponent<CampTurret>() != null)
                            {
                                tile.bodyObj = k.gameObject;
                            }
                        }
                        else if (k.tag == "CampChest")
                        {
                            if (k.gameObject.GetComponent<CampChest>() != null)
                            {
                                tile.rewardObj = k.gameObject;
                            }
                        }
                        else if (k.tag == "CampHub")
                        {

                        }
                        //else if (k.tag == "ArtifactObj")
                        //{
                        //  map.tMap[x, y].artifacts.Add(k.gameObject);
                        //}
                    }

                }
            }
        }

        //for (int i = 0; i < map.xSize; i++ )
        //{
        //    for (int j = 0; j < map.ySize; j++)
        //    {
        //        if (map.tMap[i, j].bodyObj != null)
        //        {
        //            if (map.tMap[i, j].bodyObj.tag == "CampTurret")
        //            {
        //                camps.Add(new Camp(map.tMap[i, j]));
        //                camps[camps.Count - 1].init();
        //                Debug.Log("Camp added.");
        //            }
        //        }
        //    }
        //}


        //Do some tile loading.
    }

    public void InitMap()
    {
        map = new Map(dTiles);
        map.mapRoot = mapRoot;
        map.loadMap(mapText.text);
        map.initializeMap();
    }
   

}
