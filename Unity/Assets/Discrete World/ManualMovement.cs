using UnityEngine;
using System.Collections;

public class ManualMovement : MonoBehaviour {

	Grid grid;
	int x;
	int y;

	void Start () {
		grid = GameObject.FindGameObjectWithTag ("Grid").GetComponent<Grid> ();
		Node node = grid.NodeFromWorldPoint (transform.position);
		transform.position = node.worldPosition;
		x = node.gridPosX;
		y = node.gridPosY;

		transform.position = grid.grid[0, 10].worldPosition;
	}

	// Update is called once per frame
	void Update () {
		//print ("Player Position: [" + x + ", " + y + "]");
		if (Input.GetKeyDown (KeyCode.W)) {
			if(y > 0) {
				y--;
				Node node = grid.grid[x, y];
				if(node.walkable)
					transform.position = node.worldPosition;
				else
					y++;
			}
		}
		else if (Input.GetKeyDown (KeyCode.A)) {
			if(x > 0) {
				x--;
				Node node = grid.grid[x, y];
				if(node.walkable) 
					transform.position = node.worldPosition;
				else
					x++;
			}
		}
		else if (Input.GetKeyDown (KeyCode.S)) {
			if(y < 19) {
				y++;
				Node node = grid.grid[x, y];
				if(node.walkable)
					transform.position = node.worldPosition;
				else
					y--;
			}
		}
		else if (Input.GetKeyDown (KeyCode.D)) {
			if(x < 19) {
				x++;
				Node node = grid.grid[x, y];
				if(node.walkable)
					transform.position = node.worldPosition;
				else
					x--;
			}
		}
	}
}
