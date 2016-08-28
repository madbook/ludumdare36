using UnityEngine;
using System.Collections;

public class BoardDisplay : MonoBehaviour {
	[Range(0, 1)]
	public float scale;

	public void DrawBoard (BoardNode[,] board) {
		int width = board.GetLength (0);
		int height = board.GetLength (1);

		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				BoardNode node = board[x,y];
				float scaledAltitude = node.altitude * scale;
				GameObject obj = GameObject.CreatePrimitive (PrimitiveType.Cube);
				obj.transform.localScale = new Vector3 (1, scaledAltitude, 1);
				obj.transform.localPosition = new Vector3 (x, scaledAltitude / 2f, y);
				obj.transform.parent = transform;
			}
		}
	}
}
