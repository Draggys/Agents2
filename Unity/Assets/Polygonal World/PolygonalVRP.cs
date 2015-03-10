using UnityEngine;
using System.Collections;

public class PolygonalVRP : MonoBehaviour {

	private PolyMapLoader map;

	// Use this for initialization
	void Start () {
		map = new PolyMapLoader ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button" , "polygMap1/customerPos");
        
    }
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnDrawGizmos() {
		map = new PolyMapLoader ("polygMap1/x", "polygMap1/y", "polygMap1/goalPos", "polygMap1/startPos", 
		                         "polygMap1/button" , "polygMap1/customerPos");
		if (map.polyData.nodes != null) {
			foreach(Vector3 node in map.polyData.nodes) {
				Gizmos.color = Color.blue;
				Gizmos.DrawCube (node, Vector3.one);
			}
			foreach(Line line in map.polyData.lines){
				Gizmos.color=Color.black;
                Gizmos.DrawLine(line.point1,line.point2);
			}
		}
    }
}
