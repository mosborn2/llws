using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine.UI;

public class Map
{
	//-----------------------------------------------------------------------------------------------------------------------------
	//ATTRIBUTES
	//-----------------------------------------------------------------------------------------------------------------------------
	private bool debugMode = false;									//set to false to deactivate Debug.Log statements
	public DefaultTiles dTiles;

    public int xSize;												//horizontal size of the map
    public int ySize;												//vertical size of the map

	public GameObject mapRoot;										//map container object
    public Tile[,] tMap;											//the actual map (tile array)    
	//-----------------------------------------------------------------------------------------------------------------------------


    public struct Point
    {
        /*
         * 
         * Yes, this is terrible, but it is only used during the loading sequence. 
         * 
         */
        public int x;
        public int y;
        public Point(int _x, int _y)
        {
            x = _x;
            y = _y;
        }
        public Point up()    { return new Point(x, y + 1); }
        public Point down()  { return new Point(x, y - 1); }
        public Point left()  { return new Point(x - 1, y); }
        public Point right() { return new Point(x + 1, y); }
    }

    private bool validP(Point p)       { return !(p.x < 0 || p.x >= xSize || p.y < 0 || p.y >= ySize); }
	public Tile getTile(Point p)       { return tMap[p.x, p.y]; }
	public Tile getTile(int x, int y)  { return tMap[x, y]; }

    /*
     * 
     * Constructors
     * 
     */
    public Map()               { }
	public Map(string mapName) { }
	public Map(DefaultTiles dTiles) { this.dTiles = dTiles; }

    /*
     * 
     * Class Functions
     * 
     */

    /*
     *I left this with a generic string so we could either use a TextAsset, or load text from a file. 
     */
    public bool loadMap(string mapText)
    {
        string[] mText = mapText.Split('\n');
        xSize = int.Parse(mText[0]);
        ySize = int.Parse(mText[1]);
        //Debug.Log("Map width: " + xSize.ToString() + " Map Height: " + ySize.ToString());

        tMap = new Tile[xSize, ySize];
        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                tMap[i, j] = new Tile();
                tMap[i, j].xPos = i;
                tMap[i, j].yPos = j;
            }
        }


        byte[][] values = new byte[mText.Length][];
        for (int i = 0; i < mText.Length; i++)
        {
            values[i] = Encoding.GetEncoding("UTF-8").GetBytes(mText[i].ToCharArray());
        }

        for (int j = 0; j < ySize; j++)
        {
            for (int i = 0; i < xSize; i++)
            {
                Point p = new Point(i, j);
                Tile t = getTile(p);

                t.bodyType = values[i + 2][j];
                t.region = (byte)(values[i + 2 + ySize][j] - 48);
                if (t.region == 9) { t.isWilderness = true; }
                t.value = (byte)(values[i + 2 + ySize * 2][j] - 48);

            }
        }

        if (debugMode) Debug.Log("Map Read. Processing Map");

        for (Point p = new Point(0, 0); p.x < xSize; p.x++)
        {
            for (p.y = 0; p.y < ySize; p.y++)
            {
                Tile t = getTile(p);

                if (validP(p.right()))
                {
                    if (getTile(p.right()).bodyType == t.bodyType && getTile(p.right()).region == t.region)
                    {
                        t.spacialVal *= 2;
                        t.degree++;
                    }
                }

                if (validP(p.up()))
                {
                    if (getTile(p.up()).bodyType == t.bodyType && getTile(p.up()).region == t.region)
                    {
                        t.spacialVal *= 3;
                        t.degree++;
                    }
                }

                if (validP(p.left()))
                {
                    if (getTile(p.left()).bodyType == t.bodyType && getTile(p.left()).region == t.region)
                    {
                        t.spacialVal *= 5;
                        t.degree++;
                    }
                }

                if (validP(p.down()))
                {
                    if (getTile(p.down()).bodyType == t.bodyType && getTile(p.down()).region == t.region)
                    {
                        t.spacialVal *= 7;
                        t.degree++;
                    }
                }

                //We get the orientation for all tiles out of simplicity.
                if (t.degree == 0)
                {
                    //Tile is completely isolated.
                    //Orientation doesn't matter for now.
                    t.orientation = 0;
                }
                else if (t.degree == 1)
                {
                    if (t.spacialVal % 2 == 0)
                        t.orientation = 0;
                    else if (t.spacialVal % 3 == 0)
                        t.orientation = 3;
                    else if (t.spacialVal % 5 == 0)
                        t.orientation = 2;
                    else if (t.spacialVal % 7 == 0)
                        t.orientation = 1;
                    else
                        t.orientation = 0;
                }
                else if (t.degree == 2)
                {
                    if (t.spacialVal % 10 == 0)
                        t.orientation = 0;
                    else if (t.spacialVal % 21 == 0)
                        t.orientation = 1;
                    else
                    {
                        if (t.spacialVal % 2 == 0)
                        {
                            if (t.spacialVal % 3 == 0)
                                t.orientation = 2;
                            if (t.spacialVal % 7 == 0)
                                t.orientation = 3;
                        }
                        else
                        {
                            if (t.spacialVal % 3 == 0)
                                t.orientation = 1;
                            if (t.spacialVal % 7 == 0)
                                t.orientation = 0;
                        }
                    }

                }
                else if (t.degree == 3)
                {
                    if (t.spacialVal % 10 == 0)
                        if (t.spacialVal % 3 == 0)
                            t.orientation = 2;
                        else
                            t.orientation = 0;
                    if (t.spacialVal % 21 == 0)
                        if (t.spacialVal % 2 == 0)
                            t.orientation = 3;
                        else
                            t.orientation = 1;
                }
                else if (t.degree == 4)
                {
                    t.orientation = 0;
                }
                else
                {
                    t.orientation = 0;
                }

                t.tileType = dTiles.getType(t);
            }
        }

        if (debugMode) Debug.Log("Done processing map");
        return true;
    }

    public bool initializeMap()
    {
        if (mapRoot == null)
        {
            Debug.LogError("Map root must be set before initializing.");
            return false;
        }
        else
        {
            for (Point p = new Point(0, 0); p.x < xSize; p.x++)
            {
                for (p.y = 0; p.y < ySize; p.y++)
                {
                    Tile t = getTile(p);
                    dTiles.initTile(t, mapRoot);
                }
            }
        }
        if (debugMode) Debug.Log("Done initializing Map");
        return true;
    }
}
