using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public int width;
    public int height;
    public enum WetDry { wet, dry };
    public WetDry makeWetDry;
    public enum ColdHot { cold, hot };
    public ColdHot makeColdHot;
    public enum HighLow { high, low };
    public HighLow makeHighLow;

    private float atmosphericDiffusion = .01f; //The amount adjacent blocks "blur" their props per tick.  Magnified by 4, since 4 cardinal neighbors influence you.

    BoardNode[,] board;

    void Start()
    {
        board = BoardGenerator.GenerateBoard(width, height);
        BoardDisplay display = FindObjectOfType<BoardDisplay>();
        if (display != null)
        {
            display.DrawBoard(board);
        }
    }
   
    void FixedUpdate()
    {
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
        FindObjectOfType<BoardDisplay>().DrawBoard(board);
    }
    
    void Update()
    {
        bool Clicked = false;

        int width = board.GetLength(0);
        int height = board.GetLength(1);

        //key presses:

        // Wet
        if (Input.GetKeyDown(KeyCode.W)) {
            Debug.Log("wetting");
            makeWetDry = WetDry.wet;
        } else if(Input.GetKeyDown(KeyCode.D)) {
            Debug.Log("drying");
            makeWetDry = WetDry.dry;
        }

        if (Input.GetMouseButtonDown(0)/* && Clicked == false*/)
        {
            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                }
            }
            */

            Debug.Log("click");

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

                if(makeWetDry == WetDry.wet) {
                    int old_wet =  board[row, col].moisture;
                    board[row, col].moisture += 10;
                    Debug.Log("wetting " + row + ", " + col + " was: " + old_wet + " now: " + board[row, col].moisture);

                }
                if (makeWetDry == WetDry.dry) {
                    Debug.Log("drying " + row + ", " + col);

                    board[row, col].moisture -= 10;
                }

                BoardDisplay display = FindObjectOfType<BoardDisplay>();
                display.DrawBoard(board);

            }
        }    
    }
}
