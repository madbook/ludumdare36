using UnityEngine;
using System.Collections;

public class InputManager : MonoBehaviour {
    public int brushMagnitude = 10;

    public GameManager gameManager;
    public GameManager.Action currentAction;

    const float PAINT_INTERVAL = .2f;

    bool wasCollidingLastUpdate = false;
    bool isPaintEnabled = true;

    void Start () {
        gameManager.UpdateInput (currentAction, brushMagnitude);
    }

    void FixedUpdate () {
        if (Input.GetKeyDown (KeyCode.W)) {
            SetAction (GameManager.Action.Wet);
        } else if (Input.GetKeyDown (KeyCode.D)) {
            SetAction (GameManager.Action.Dry);
        } else if (Input.GetKeyDown (KeyCode.R)) {
            SetAction (GameManager.Action.Raise);
        } else if (Input.GetKeyDown (KeyCode.L)) {
            SetAction (GameManager.Action.Lower);
        } else if (Input.GetKeyDown (KeyCode.H)) {
            SetAction (GameManager.Action.Hot);
        } else if (Input.GetKeyDown (KeyCode.C)) {
            SetAction (GameManager.Action.Cold);
        } else if (Input.GetKeyDown (KeyCode.I)) {
            SetAction (GameManager.Action.Inspect);
        } else if (Input.GetKeyDown (KeyCode.Minus)) {
            SetBrushMagnitude (brushMagnitude - 5);   
        } else if (Input.GetKeyDown (KeyCode.Equals)) {
            SetBrushMagnitude (brushMagnitude + 5);
        }

        Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
        RaycastHit hit;
        int mask =  1 << LayerMask.NameToLayer("Terrain");
        bool isCollision = Physics.Raycast(ray, out hit, 100, mask);

        if (isCollision) {
            float x = hit.point.x;
            float z = hit.point.z;
            GameManager.Size boardSize = gameManager.GetBoardSize ();
            int row = Mathf.Clamp ((int)(x + boardSize.width/2), 0, boardSize.width - 1);
            int col = Mathf.Clamp ((int)(z + boardSize.height/2), 0, boardSize.height - 1);

            if (!wasCollidingLastUpdate) {
                wasCollidingLastUpdate = true;
                gameManager.CursorEnabled (row, col);
            } else {
                gameManager.CursorUpdate (row, col);
            }

            if (isPaintEnabled && Input.GetMouseButton (0)) {
                gameManager.ApplyActionAtCursor (currentAction, brushMagnitude);
                StartCoroutine (TemporarilyDisablePaint (PAINT_INTERVAL));
            }
        } else if (wasCollidingLastUpdate) {
            wasCollidingLastUpdate = false;
            gameManager.CursorDisabled ();
        }
    }

    void SetAction (GameManager.Action action) {
        currentAction = action;
        gameManager.UpdateInput (currentAction, brushMagnitude);
    }

    void SetBrushMagnitude (int newMagnitude) {
        brushMagnitude = Mathf.Clamp (newMagnitude, 5, 100);
        gameManager.UpdateInput (currentAction, brushMagnitude);
    }

    IEnumerator TemporarilyDisablePaint (float interval) {
        isPaintEnabled = false;
        yield return new WaitForSeconds (interval);
        isPaintEnabled = true;
    }
}
