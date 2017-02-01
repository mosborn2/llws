using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class DefaultTiles : MonoBehaviour
{
    public enum tType
    {
        error_error = -1,

        struct_camp,
        struct_hub,

        floor,

        wall_connector,
        wall_connector_dec,
        wall_corner,
        wall_corner_dec,
        wall_cross,
        wall_cross_dec,
        wall_endcap,
        wall_endcap_dec,
        wall_single,
        wall_single_dec,
        wall_tee,
        wall_tee_dec,
    }

    //public GameObject TileHolder;

    public GameObject error_error;

    public GameObject detail_grass;
    public GameObject detail_rubble;
    public GameObject floor_path;

    public GameObject capt_hub;
    public GameObject mb_hub;

    public GameObject camp_floor;
    public GameObject camp_chest;
    public GameObject camp_turret;

    public GameObject capt_connector;
    public GameObject capt_corner;
    public GameObject capt_cross;
    public GameObject capt_endcap;
    public GameObject capt_single;
    public GameObject capt_single_alt;
    public GameObject capt_tee;
    public GameObject capt_floor;

    public GameObject neut_connector;
    public GameObject neut_connector_alt;
    public GameObject neut_connector_dec;
    public GameObject neut_corner;
    public GameObject neut_corner_alt;
    public GameObject neut_cross;
    public GameObject neut_endcap;
    public GameObject neut_endcap_alt;
    public GameObject neut_single;
    public GameObject neut_single_alt;
    public GameObject neut_tee;
    public GameObject neut_floor;

    public GameObject wild_connector;
    public GameObject wild_corner;
    public GameObject wild_cross;
    public GameObject wild_single_dec;
    public GameObject wild_endcap;
    public GameObject wild_single;
    public GameObject wild_tee;
    public GameObject wild_floor;

    

    void Start()
    {
        

    }

    public tType getType(Tile t)
    {
        switch (t.bodyType)
        {
            case (byte)'.':
                t.isFloorCapturable = true;
                t.isFloor = true;
                return tType.floor;

            case (byte)'%':
                t.isFloorCapturable = true;
                t.isFloor = true;
                return tType.floor;

            case (byte)'&':
                t.isFloorCapturable = false;
                t.isFloor = true;
                return tType.floor;

            case (byte)'#':
                if (t.degree == 0)
                    return tType.wall_single;
                if (t.degree == 1)
                    return tType.wall_endcap;
                if (t.degree == 2)
                    if ((t.spacialVal % 10 == 0) || (t.spacialVal % 21 == 0))
                        return tType.wall_connector;
                    else
                        return tType.wall_corner;
                if (t.degree == 3)
                    return tType.wall_tee;
                if (t.degree == 4)
                    return tType.wall_cross;
                else
                    return tType.error_error;
            case (byte)'@':
                if (t.degree == 0)
                    return tType.wall_single_dec;
                if (t.degree == 1)
                    return tType.wall_endcap_dec;
                if (t.degree == 2)
                    if ((t.spacialVal % 10 == 0) || (t.spacialVal % 21 == 0))
                        return tType.wall_connector_dec;
                    else
                        return tType.wall_corner_dec;
                if (t.degree == 3)
                    return tType.wall_corner_dec;
                if (t.degree == 4)
                    return tType.wall_cross_dec;
                else
                    return tType.error_error;
            case (byte)'^':
                return tType.struct_hub;
            case (byte)'$':
                t.isFloor = true;
                return tType.struct_camp;
            default:
                return tType.error_error;
        }
    }

    public void unloadTile(Tile t)
    {
        if (t.holder != null)
        {
            DestroyObject(t.holder);
        }

        if (t.bodyObj != null)
        {
            DestroyObject(t.bodyObj);
        }

        if (t.floorObj != null)
        {
            DestroyObject(t.floorObj);
        }
        if (t.rewardObj != null)
        {
          DestroyObject(t.rewardObj);
        }
        if (t.artifacts != null)
        {
            if (t.artifacts.Count > 0)
            {
                foreach (GameObject o in t.artifacts)
                {
                    DestroyObject(o);
                }
            }
        }
        else
        {
            t.artifacts = new List<GameObject>();
        }
    }

    public bool initTile(Tile tile, GameObject mapRoot)
    {

        unloadTile(tile);

        tile.holder = new GameObject();
        tile.holder.transform.parent = mapRoot.transform;


        switch (tile.tileType)
		{
            case tType.error_error:
                {
                    tile.bodyObj = Instantiate(error_error);
                    break;
                }
            case tType.struct_hub:
                {
                    if (tile.region < 4)
                    {
                        tile.bodyObj = Instantiate(capt_hub);
                    }
                    else
                    {
                        tile.bodyObj = Instantiate(mb_hub);
                        tile.bodyObj.tag = "mbHub";
                        tile.rewardObj = Instantiate(camp_chest);
                        tile.rewardObj.tag = "mbChest";
                    }
                    break;
                }
            case tType.struct_camp:
                {
                    tile.floorObj = Instantiate(camp_floor);
                    tile.bodyObj = Instantiate(camp_turret);
                    tile.rewardObj = Instantiate(camp_chest);
                    break;
                }
            case tType.floor:
                {
                    if (tile.region >= 2 && tile.region <= 5)
                    {
                        tile.floorObj = Instantiate(capt_floor);
                    }
                    else if (tile.region == 9)
                    {
                        tile.floorObj = Instantiate(wild_floor);
                    }
                    else if (tile.region == 6)
                    {
                        tile.floorObj = Instantiate(floor_path);
                    }
                    else
                    {
                        tile.floorObj = Instantiate(neut_floor);
                    }
                    break;
                }
            case tType.wall_connector:
                {
                    if (tile.region >= 2 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_connector);
                        tile.orientation += 1;
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_connector);
                        tile.orientation += 1;
                    }
                    else
                    {
                        if (UnityEngine.Random.value > 0.75)
                        {
                            tile.bodyObj = Instantiate(neut_connector_alt);
                            if (UnityEngine.Random.value > 0.5)
                            {
                                tile.orientation += 2;
                            }
                        }
                        else
                        {
                            tile.bodyObj = Instantiate(neut_connector);
                            tile.orientation += 1;
                        }
                    }
                    break;
                }
            case tType.wall_connector_dec:
                {
                    tile.bodyObj = Instantiate(neut_connector_dec);
                    break;
                }
            case tType.wall_corner_dec:
            case tType.wall_corner:
                {
                    if (tile.region >= 2 && tile.region <=5)
                    {
                        tile.bodyObj = Instantiate(capt_corner);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_corner);
                        tile.bodyObj.transform.position = new Vector3(0, 1, 0);
                    }
                    else
                    {
                        if (UnityEngine.Random.value > 0.75)
                        {
                            tile.bodyObj = Instantiate(neut_corner_alt);
                        }
                        else
                        {
                            tile.bodyObj = Instantiate(neut_corner);
                        }
                    }
                    break;
                }
            case tType.wall_cross_dec:
            case tType.wall_cross:
                {
                    if (tile.region >= 2 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_cross);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_cross);
                    }
                    else
                    {
                        tile.bodyObj = Instantiate(neut_cross);   
                    }
                    break;
                }
            case tType.wall_endcap_dec:
            case tType.wall_endcap:
                {
                    if (tile.region >= 2 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_endcap);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_endcap);
                    }
                    else
                    {
                        if (UnityEngine.Random.value > 0.75)
                        {
                            tile.bodyObj = Instantiate(neut_endcap_alt);
                            tile.orientation += 3;
                        }
                        else
                        {
                            tile.bodyObj = Instantiate(neut_endcap);
                        }
                    }
                    break;
                }
            case tType.wall_single_dec:
                {
                    if (tile.region >= 2 && tile.region <= 3)
                    {
                        tile.bodyObj = Instantiate(capt_single_alt);
                    }
                    else if (tile.region >= 4 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_single);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_single_dec);
                    }
                    else
                    {
                        tile.bodyObj = Instantiate(neut_single_alt);
                    }
                    break;
                }
            case tType.wall_single:
                {
                    if (tile.region >= 2 && tile.region <= 3)
                    {
                        tile.bodyObj = Instantiate(capt_single_alt);
                    }
                    else if (tile.region >= 4 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_single);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_single);
                    }
                    else
                    {
                        tile.bodyObj = Instantiate(neut_single);
                    }
                    break;
                }

            case tType.wall_tee_dec:
            case tType.wall_tee:
                {
                    if (tile.region >= 2 && tile.region <= 5)
                    {
                        tile.bodyObj = Instantiate(capt_tee);
                    }
                    else if (tile.region == 9)
                    {
                        tile.bodyObj = Instantiate(wild_tee);
                    }
                    else
                    {
                        tile.bodyObj = Instantiate(neut_tee);
                    }
                    break;
                }
            default: 
                {
                    tile.bodyObj = Instantiate(error_error);
                    break;
                }

        }

        if (tile.bodyObj != null)
        {
            tile.bodyObj.transform.parent = tile.holder.transform;
            if (tile.bodyObj.tag != "hub" && tile.bodyObj.tag != "CampTurret" ) tile.bodyObj.tag = "BodyObj";
        }

        if (tile.floorObj != null)
        {
            tile.floorObj.transform.parent = tile.holder.transform;
            tile.floorObj.tag = "FloorObj";
            tile.floorObj.transform.position = new Vector3(0, 1, 0);
        }

        if (tile.rewardObj != null)
        {
          tile.rewardObj.transform.parent = tile.holder.transform;
          tile.rewardObj.tag = "CampChest";
        }

        if (tile.artifacts != null)
        {
          foreach (GameObject g in tile.artifacts)
          {
            g.transform.parent = tile.holder.transform;
            g.tag = "ArtifactObj";
          }
        }

        /*
        if (tile.hiddenBody)
        {
            tile.bodyObj.SetActive(false);
        }
         * */

        tile.holder.name = "Tile(" + tile.xPos + "," + tile.yPos + ")";
        tile.holder.tag = "TileHolder";
        tile.holder.transform.rotation = Quaternion.Euler(0, 90 * tile.orientation, 0);
        tile.holder.transform.position = new Vector3(tile.xPos * 2, 0, tile.yPos * 2);

        return true;
    }

}



