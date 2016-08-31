using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public int width;
    public int height;
    public enum Action {Wet, Dry, Cold, Hot, Raise, Lower, Inspect};
    public Action currentAction;
    public bool upateEachTick = true;
    public int brushMagnitude = 10;
    private float atmosphericDiffusion = .01f; //The amount adjacent blocks "blur" their props per tick.  Magnified by 4, since 4 cardinal neighbors influence you.
    public bool useTemplateNode = false;
    public BoardNode templateNode;

    BoardNode[,] board;
    BoardDisplay display;

    const float paintInterval = .2f;
    bool isPaintEnabled = true;

    void Start()
    {
        if (useTemplateNode) {
            board = BoardGenerator.CreateUniformBoard (width, height, templateNode);
        } else {
            board = BoardGenerator.GenerateBoard(width, height);
        }

        display = FindObjectOfType<BoardDisplay>();
        if (display != null) {
            display.DrawBoard(board);
        }
    }
   
    void FixedUpdate()
    {
        if (!upateEachTick) {
            return;
        }

        int width = board.GetLength(0);
        int height = board.GetLength(1);

        BoardNode[, ] board_after = new BoardNode[width, height];

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                BoardNode old = board[x, y];

                // If a tile is on the edge, it's starting condition is weighed more (since there isn't diffusion from off the board).
                float edges = 0;
                if(x == 0 || x == width - 1)
                {
                    edges++;
                }
                if (y == 0 || y == height - 1)
                {
                    edges++;
                }
                
                                                        // I don't understand why this isn't 4.
                float newMoisture = old.moisture * (1f - (3.5f-edges) * atmosphericDiffusion);
                float newTemperature = old.temperature * (1f - (3.5f - edges) * atmosphericDiffusion);
                
                 if (x > 0)
                 {
                    newMoisture += board[x - 1, y].moisture * atmosphericDiffusion;
                    newTemperature += board[x - 1, y].temperature * atmosphericDiffusion;
                 }
                 if (x < width-1)
                 {
                    newMoisture += board[x + 1, y].moisture * atmosphericDiffusion;
                    newTemperature += board[x + 1, y].temperature * atmosphericDiffusion;
                 }
                 if (y > 0)
                 {
                    newMoisture +=board[x, y-1].moisture * atmosphericDiffusion;
                    newTemperature += board[x, y-1].temperature * atmosphericDiffusion;
                 }
                if (y < height-1)
                 {
                    newMoisture += board[x, y + 1].moisture * atmosphericDiffusion;
                    newTemperature += board[x, y + 1].temperature * atmosphericDiffusion;
                 }
                board_after[x, y] = new BoardNode(old.altitude, (int)newMoisture, (int)newTemperature, old.wind);
            }
            
        }
        board = board_after;

        if (display != null) {
            display.DrawBoard(board);
        }
    }
    
    void Update()
    {
        int width = board.GetLength(0);
        int height = board.GetLength(1);

        //key presses:

        // Select Action
        if (Input.GetKeyDown (KeyCode.W)) {
            currentAction = Action.Wet;
        } else if (Input.GetKeyDown (KeyCode.D)) {
            currentAction = Action.Dry;
        } else if (Input.GetKeyDown (KeyCode.R)) {
            currentAction = Action.Raise;
        } else if (Input.GetKeyDown (KeyCode.L)) {
            currentAction = Action.Lower;
        } else if (Input.GetKeyDown (KeyCode.H)) {
            currentAction = Action.Hot;
        } else if (Input.GetKeyDown (KeyCode.C)) {
            currentAction = Action.Cold;
        } else if (Input.GetKeyDown (KeyCode.I)) {
            currentAction = Action.Inspect;
        }

        if (isPaintEnabled && Input.GetMouseButton(0))
        {
            // Debug.Log("click");

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerMask.NameToLayer("Terrain")))
            {
                Debug.Log(hit.point);

                float x = hit.point.x;
                float z = hit.point.z;

                int row = (int)(x + width / 2 - .5);
                int col = (int)(z + height / 2 - .5);
                Debug.Log("row: " + row + " col: " + col);

                BoardNode node = board[row, col];
                if (currentAction == Action.Wet) {
                    node.moisture = Mathf.Clamp (node.moisture + brushMagnitude, 0, 100);
                } else if (currentAction == Action.Dry) {
                    node.moisture = Mathf.Clamp (node.moisture - brushMagnitude, 0, 100);
                } else if (currentAction == Action.Raise) {
                    node.altitude = Mathf.Clamp (node.altitude + brushMagnitude, 0, 100);
                } else if (currentAction == Action.Lower) {
                    node.altitude = Mathf.Clamp (node.altitude - brushMagnitude, 0, 100);
                } else if (currentAction == Action.Hot) {
                    node.temperature = Mathf.Clamp (node.temperature + brushMagnitude, 0, 100);
                } else if (currentAction == Action.Cold) {
                    node.temperature = Mathf.Clamp (node.temperature - brushMagnitude, 0, 100);
                } else if (currentAction == Action.Inspect) {
                    Biome biome = BiomeGenerator.GetBiome (node);
                    Debug.Log ("<Node (" + row + "," + col + ") moisture=" + node.moisture + " altitude=" + node.altitude + " temperature=" + node.temperature + ">");
                    Debug.Log ("<Biome (" + row + "," + col + ") moisture=" + biome.moisture + " altitude=" + biome.altitude + " temperature=" + biome.temperature + ">");
                }

                board[row, col] = node;

                if (display != null) {
                    display.DrawBoard(board);
                }
            }

            StartCoroutine (TemporarilyDisablePaint (paintInterval));
        }    
    }

    IEnumerator TemporarilyDisablePaint (float interval) {
        isPaintEnabled = false;
        yield return new WaitForSeconds (interval);
        isPaintEnabled = true;
    }
}
