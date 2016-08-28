using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public int width;
	public int height;
	public int seed;

	BoardNode[,] board;

	void Start () {
		board = BoardGenerator.GenerateBoard (width, height, seed);
		BoardDisplay display = FindObjectOfType<BoardDisplay> ();
		if (display != null) {
			display.DrawBoard (board);
		} 		
	}
}
