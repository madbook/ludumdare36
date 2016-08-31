using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardDisplay : MonoBehaviour {
    public enum DisplayMode {Mesh, Cube, MeshWithBillboards, MeshWithDoodads};
    public DisplayMode displayMode;
    public enum ColorMode {Biome, Debug};
    public ColorMode colorMode;
    public Transform camera;
    public bool hdColorMap;

    // These could be set up in the inspector UI, but for now I'll build them here.
    Color desertColor = new Color (1, .85f, .5f);
    Color iceShelfColor = new Color (1, 1, 1);
    Color tundraColor = new Color (.7f, .65f, .75f);
    Color rainForestColor = new Color (.25f, .5f, 0);
    Color deepWaterColor = new Color (0, .25f, 1);
    Color shallowWaterColor = new Color (0, .5f, 1);
    Color forestColor = new Color (0f, .75f, .25f);
    Color mountainForestColor = new Color (.5f, .5f, .25f);
    Color swampColor = new Color (.25f, .25f, .15f);
    Color plainsColor = new Color (.5f, .75f, .25f);
    Color borealColor = new Color (.25f, .5f, .5f);
    Color mountainBorealColor = new Color (.65f, .75f, .75f);

    // To help spot holes in biome coverage.
    Color defaultColor = Color.magenta;

    // Any temperature, any altitude.
    Biome desertBiome = new Biome (MoistureBiome.Dry, TemperatureBiome.Any, AltitudeBiome.Any);
    Biome iceShelfBiome = new Biome (MoistureBiome.Water, TemperatureBiome.Any, AltitudeBiome.Any);

    // Any altitude.
    Biome tundraBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Cold, AltitudeBiome.Any);
    Biome rainForestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Tropical, AltitudeBiome.Any);
 
    // Any temperature.
    Biome deepWaterBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Valley);
    Biome shallowWaterBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Plain);
    Biome forestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Hill);
    Biome mountainForestBiome = new Biome (MoistureBiome.Wet, TemperatureBiome.Any, AltitudeBiome.Mountain);
    Biome swampBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Valley);
    Biome plainsBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Plain);
    Biome borealBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Hill);
    Biome mountainBorealBiome = new Biome (MoistureBiome.Moist, TemperatureBiome.Any, AltitudeBiome.Mountain);

    Dictionary<AltitudeBiome,int> altitudeBiomeIndices = new Dictionary<AltitudeBiome,int> ();

    // TODO - Can simplify the logic that that selects the biome by creating a dictionary
    // that ties biomes to colors, but I don't think that works with the "Any" types.  Will
    // need to enumrate all the biome combinations for that to work (I think);

    // Snap altitude rendering to n discrete levels
    const int quantizationLevels = 5;
    const int verticalScale = 4;

    // Because the current value of altitude is in the range of 0-100; 
    const float MAX_ALTITUDE = 100;

    MeshFilter meshFilter;
    MeshRenderer meshRenderer;
    public MeshFilter bottomMeshFilter;
    MeshCollider meshCollider;

    public Transform waterCube;
    public Material cloudMaterial;

    List<GameObject> doodads = new List<GameObject> ();

    void Start () {
        meshFilter = GetComponent<MeshFilter> ();
        meshRenderer = GetComponent<MeshRenderer> ();
        MeshCollider meshCollider = meshFilter.gameObject.AddComponent<MeshCollider>();

        altitudeBiomeIndices.Add(AltitudeBiome.Valley, 0);
        altitudeBiomeIndices.Add(AltitudeBiome.Plain, 1);
        altitudeBiomeIndices.Add(AltitudeBiome.Hill, 2);
        altitudeBiomeIndices.Add(AltitudeBiome.Mountain, 3);
    }

    public void DrawBoard (BoardNode[,] board) {
        foreach (GameObject obj in doodads) {
            Destroy (obj);
        }
        doodads.Clear ();

        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = GenerateHeightMap (board);
        Color[] colorMap;
        Biome[,] biomeMap = BiomeGenerator.GenerateBiomeData (board);

        if (colorMode == ColorMode.Biome) {
            colorMap = GenerateBiomeColorMap (board, heightMap, biomeMap, hdColorMap);
        } else {
            colorMap = GenerateDebugColorMap (board);
        }

        if (displayMode == DisplayMode.Mesh) {
            DrawMesh (heightMap, colorMap, hdColorMap);
            DrawClouds (board, heightMap);
        } else if (displayMode == DisplayMode.Cube) {
            DrawBoardCubes (heightMap, colorMap);
        } else if (displayMode == DisplayMode.MeshWithBillboards) {
            DrawMesh (heightMap, colorMap, hdColorMap);
            DrawBillboards (board, heightMap);
        } else if (displayMode == DisplayMode.MeshWithDoodads) {
            DrawMesh (heightMap, colorMap, hdColorMap);
            DrawDoodads (heightMap, biomeMap);
            DrawClouds (board, heightMap);
        }

        if (waterCube != null) {
            // Adjust the water level;
            float waterLevel = GetWaterLevel (board, heightMap);
            waterCube.transform.localScale = new Vector3 (width - 1, waterLevel, height - 1);
            waterCube.transform.localPosition = new Vector3 (0, waterLevel/2, 0); 
        }
    }

    float GetWaterLevel (BoardNode[,] board, float[,] heightMap) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        int waterLevels = 4;
        float totalWater = 0;
        float waterLevel = 0f;
        int[] numberOfTilesPerLevel = new int[waterLevels];


        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                int index = altitudeBiomeIndices[BiomeGenerator.GetAltitudeBiome(node.altitude)];
                totalWater += node.moisture;
                // Add volume for each level at or above this altitude.
                for (int i = index; i < waterLevels; i++) {
                    numberOfTilesPerLevel[i] += 1;
                }
            }
        }

        for (int i = 0; i < waterLevels; i++) {
            int tiles = numberOfTilesPerLevel[i];
            int availableVolumeOnThisLevel = tiles * 100;
            float waterOnThisLevel = totalWater / availableVolumeOnThisLevel;

            if (waterOnThisLevel > 1f) {
                waterLevel += 1f;
                totalWater = totalWater - availableVolumeOnThisLevel;
            } else {
                waterLevel += waterOnThisLevel;
                break;
            }
        }

        return waterLevel;
    }

    float[,] GenerateHeightMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);
        float[,] heightMap = new float[width, height];

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                float altitude = node.altitude / MAX_ALTITUDE;
                if (quantizationLevels > 1) {
                    altitude *= quantizationLevels - 1;
                    altitude = Mathf.Round(altitude);
                    altitude /= quantizationLevels - 1;
                } else if (quantizationLevels == 1) {
                    altitude = 0f;
                }
                heightMap[x,y] = altitude * verticalScale;
            }
        }

        return heightMap;
    }

    Color[] GenerateDebugColorMap (BoardNode[,] board) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        Color[] colorMap = new Color[width*height];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                colorMap[x + width*y] = new Color ((float)(node.temperature)/50f -.5f, 0, (float)(node.moisture) / 50f - .5f);
            }
        }

        return colorMap;
    }

    Color[] GenerateBiomeColorMap (BoardNode[,] board, float[,] heightMap, Biome[,] biomeMap, bool hdColorMap) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        int colorMapLength = (hdColorMap) ? width*height*4 : width*height;
        Color[] colorMap = new Color[colorMapLength];
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                Biome biome = biomeMap[x,y];
                Color baseColor = ColorFromBiome (biome);
                Color altitudeColor = Color.Lerp (Color.black, Color.white, node.altitude / MAX_ALTITUDE);
                float altitude = heightMap[x,y];

                // Original thought was to have this be on a biome-basis, e.g. to only apply it to "fluid" biomes.
                // Each tile has a 2x2 pixel texture. If any quadrant of that texture has a neighboring tile with a
                // greater altitude, that tile's texture is blended into that pixel.
                if (hdColorMap) {
                    Color colorA = baseColor;
                    Color colorB = baseColor;
                    Color colorC = baseColor;
                    Color colorD = baseColor;

                    bool nIsHigher = y > 0 && heightMap[x,y-1] > heightMap[x,y];
                    bool neIsHigher = y > 0 && x < width - 1 && heightMap[x+1,y-1] > heightMap[x,y];
                    bool eIsHigher = x < width - 1 && heightMap[x+1,y] > heightMap[x,y];
                    bool seIsHigher = x < width - 1 && y < height - 1 && heightMap[x+1,y+1] > heightMap[x,y];
                    bool sIsHigher = y < height - 1 && heightMap[x,y+1] > heightMap[x,y];
                    bool swIsHigher = y < height - 1 && x > 0 && heightMap[x-1,y+1] > heightMap[x,y];
                    bool wIsHigher = x > 0 && heightMap[x-1,y] > heightMap[x,y];
                    bool nwIsHigher = x > 0 && y > 0 && heightMap[x-1,y-1] > heightMap[x,y];

                    if (nwIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y-1]), .5f); }
                    else if (nIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y-1]), .5f); }
                    else if (wIsHigher) { colorA = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y]), .5f); }

                    if (neIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y-1]), .5f); }
                    else if (nIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y-1]), .5f); }
                    else if (eIsHigher) { colorB = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y]), .5f); }

                    if (swIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y+1]), .5f); }
                    else if (sIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y+1]), .5f); }
                    else if (wIsHigher) { colorC = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x-1,y]), .5f); }

                    if (seIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y+1]), .5f); }
                    else if (sIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x,y+1]), .5f); }
                    else if (eIsHigher) { colorD = Color.Lerp(baseColor, ColorFromBiome (biomeMap[x+1,y]), .5f); }

                    colorMap[x*2 + width*y*4] = Color.Lerp (colorA, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 1] = Color.Lerp (colorB, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 2*width] = Color.Lerp (colorC, altitudeColor, .33f);
                    colorMap[x*2 + width*y*4 + 2*width + 1] = Color.Lerp (colorD, altitudeColor, .33f);
                } else {
                    Color finalColor = Color.Lerp (baseColor, altitudeColor, .33f);
                    // Debug.Log ("assigning for (" + x + "," + y + ")");
                    colorMap[x + width*y] = finalColor;
                }
            }
        }

        return colorMap;
    }

    public bool IsBiomeFlat (Biome biome) {
        return true;
        // return biome == deepWaterBiome || biome == shallowWaterBiome || biome == swampBiome;
    }

    public Color ColorFromBiome (Biome biome) {
        if (biome == desertBiome) {
            return desertColor;
        } else if (biome == iceShelfBiome) {
            return iceShelfColor;
        } else if (biome == tundraBiome) {
            return tundraColor;
        } else if (biome == rainForestBiome) {
            return rainForestColor;
        } else if (biome == deepWaterBiome) {
            return deepWaterColor;
        } else if (biome == shallowWaterBiome) {
            return shallowWaterColor;
        } else if (biome == forestBiome) {
            return forestColor;
        } else if (biome == mountainForestBiome) {
            return mountainForestColor;
        } else if (biome == swampBiome) {
            return swampColor;
        } else if (biome == plainsBiome) {
            return plainsColor;
        } else if (biome == borealBiome) {
            return borealColor;
        } else if (biome == mountainBorealBiome) {
            return mountainBorealColor;
        } else {
            return defaultColor;
        }
    }

    public void DrawMesh (float[,] heightMap, Color[] colorMap, bool hdColorMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        MeshData meshData = MeshGenerator.GenerateMeshData (heightMap);
        Mesh mesh = meshData.GenerateMesh ();
        meshFilter.sharedMesh = meshData.GenerateMesh ();

        int textureMultiplier = (hdColorMap) ? 2 : 1;
        Texture2D texture = TextureGenerator.GenerateTexture (colorMap, width * textureMultiplier, height * textureMultiplier);
        meshRenderer.material.mainTexture = texture;

        MeshData bottomMeshData = MeshGenerator.GenerateBottomMeshData (heightMap);
        bottomMeshFilter.sharedMesh = bottomMeshData.GenerateMesh ();

        //boop the collider into updating
        GetComponent<MeshCollider>().sharedMesh = null;
        GetComponent<MeshCollider>().sharedMesh = meshFilter.mesh;
    }

    public void DrawBoardCubes (float[,] heightMap, Color[] colorMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                float scaledAltitude = heightMap[x,y];
                GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                obj.transform.localScale = new Vector3 (1, scaledAltitude, 1);
                obj.transform.localPosition = new Vector3 (x - width/2 + .5f, scaledAltitude / 2f, y - width/2 + .5f);
                obj.transform.parent = transform;
                obj.GetComponent<Renderer>().material.color = colorMap[x + width*y];
                doodads.Add (obj);
            }
        }
    }

    public void DrawClouds (BoardNode[,] board, float[,] heightMap) {
        int width = board.GetLength (0);
        int height = board.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                Biome biome = BiomeGenerator.GetBiome (node);

                if (biome.moisture == MoistureBiome.Dry || biome.moisture == MoistureBiome.Moist) {
                    continue;
                }

                Vector3 scale;

                if (biome.moisture == MoistureBiome.Wet) {
                    scale = new Vector3 (.75f, .25f, .75f);
                } else {
                    scale = new Vector3 (1f, .5f, 1f);
                }
                float nodeHeight = heightMap[x,y];
                Vector3 position = new Vector3 (x - width/2 + .5f, nodeHeight/2 + 5, y - width/2 + .5f);

                GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer> ();
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                obj.transform.localScale = scale;
                obj.transform.localPosition = position;
                obj.transform.parent = transform;
                obj.GetComponent<MeshRenderer> ().sharedMaterial = cloudMaterial;
                doodads.Add (obj);
            }
        }
    }

    public void DrawBillboards (BoardNode[,] board, float[,] heightMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BoardNode node = board[x,y];
                if (node.temperature > 50 && node.moisture > 50) {
                    // GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
                    GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Quad);
                    obj.AddComponent<Billboard> ();
                    obj.GetComponent<Billboard> ().camera = camera;
                    // Some of this won't be necessary, since we can set up textures properly.
                    MeshRenderer meshRenderer = obj.GetComponent<MeshRenderer> ();
                    meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                    meshRenderer.receiveShadows = false;

                    obj.transform.localScale = new Vector3 (0.25f, 0.5f, 0.25f);
                    obj.transform.localPosition = new Vector3 (x - width/2 + .5f, heightMap[x,y] + .25f, y - width/2 + .5f);
                    obj.transform.parent = transform;
                    obj.GetComponent<Renderer>().material.color = Color.Lerp (Color.black, Color.red, .66f);
                    doodads.Add (obj);
                }
            }
        }
    }

    public void DrawDoodads (float[,] heightMap, Biome[,] biomeMap) {
        int width = heightMap.GetLength (0);
        int height = heightMap.GetLength (1);

        Material material = GetComponent<MeshRenderer> ().material;
        Mesh normalBoreal = MeshGenerator.GeneratePyramid (1f, 0.3f).GenerateMesh ();
        Mesh tallBoreal = MeshGenerator.GeneratePyramid (1.5f, 0.28f).GenerateMesh ();
        Mesh shortBoreal = MeshGenerator.GeneratePyramid (.7f, .32f).GenerateMesh ();

        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                Biome biome = biomeMap[x,y];
                if (biome == borealBiome || biome == mountainBorealBiome || biome == mountainForestBiome) {
                    GameObject obj = new GameObject ("Tree");
                    obj.AddComponent<MeshRenderer> ();
                    obj.AddComponent<MeshFilter> ();

                    float heightOffset;
                    if (biome == borealBiome) {
                        heightOffset = .75f;
                        obj.GetComponent<MeshFilter> ().sharedMesh = tallBoreal;
                        obj.GetComponent<Renderer>().material = material;
                        obj.GetComponent<Renderer>().material.color = borealColor;
                    } else if (biome == mountainBorealBiome) {
                        heightOffset = .5f;
                        obj.GetComponent<MeshFilter> ().sharedMesh = normalBoreal;
                        obj.GetComponent<Renderer>().material = material;
                        obj.GetComponent<Renderer>().material.color = mountainBorealColor;
                    } else {
                        heightOffset = .35f;
                        obj.GetComponent<MeshFilter> ().sharedMesh = shortBoreal;
                        obj.GetComponent<Renderer>().material = material;
                        obj.GetComponent<Renderer>().material.color = mountainForestColor;
                    }

                    obj.transform.localPosition = new Vector3 (x - width/2 + .5f, heightMap[x,y] + heightOffset, y - width/2 + .5f);
                    obj.transform.parent = transform;
                    doodads.Add (obj);
                }
            }
        }
    }
}
