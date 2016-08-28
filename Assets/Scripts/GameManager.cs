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
