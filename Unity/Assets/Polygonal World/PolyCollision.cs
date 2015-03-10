using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PolyCollision {

	List<Line> lines;

	public PolyCollision(List<Line> lines) {
		this.lines = lines;
	}
	
	public bool IntersectsWithObstacle(Line newLine){
		foreach (Line line in lines) {
			if(newLine.intersect(line)){
				return true;
			}
		}
		return false;
	}
}
