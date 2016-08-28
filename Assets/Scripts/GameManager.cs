using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour
{

    public int width;
    public int height;

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

        if (Input.GetMouseButtonDown(0) && Clicked == false)
        {
            /*
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {

                }
            }
            */
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, 100, LayerMask.NameToLayer("Terrain")))
            {
                Debug.Log(hit.point);
            }
        }
    }

}
