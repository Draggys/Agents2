﻿using UnityEngine;
using System.Collections;
using System;

public class PolyMapLoaderT3 {

	private string prefix = Application.dataPath+"/Data/Polygonal/";
	private string postfix = ".txt";

	public PolyData polyData;

	public PolyMapLoaderT3(string x, string y, string goal, string start, string buttons, string customers) {
		polyData = new PolyData();
		string xFile = prefix + x + postfix;
		string yFile = prefix + y + postfix;
		string goalFile = prefix + goal + postfix;
		string startFile = prefix + start + postfix;
		string buttonsFile = prefix + buttons + postfix;
		string customerFile = prefix + customers + postfix;

		System.IO.StreamReader xReader = new System.IO.StreamReader (xFile);
		System.IO.StreamReader yReader = new System.IO.StreamReader (yFile);
		System.IO.StreamReader buttonReader = new System.IO.StreamReader (buttonsFile);

		string xpos, ypos;
		PolyNode curFigure = new PolyNode ();
		int curFirstPoint = 0;
		int index = 0;
		bool newOne = true;
		while ((xpos = xReader.ReadLine ()) != null) {
			ypos = yReader.ReadLine (); // xFile and yFile matches each other

			int curButton=int.Parse(buttonReader.ReadLine());

			// Här funkar det inte
			float xfloat;
			float yfloat;

			xpos = xpos.Replace (",", ".");
			ypos = ypos.Replace (",", ".");

			Vector3 curVec=new Vector3(float.Parse(xpos), 1f, float.Parse (ypos));


			curFigure.vertices.Add(curVec);

			polyData.buttons.Add (curButton);

			//New Figure
			if(curButton==3){
				polyData.figures.Add(curFigure);
				curFigure=new PolyNode();

				Line newLine1= new Line(curVec,polyData.nodes[curFirstPoint]);
				Line newLine2=new Line(polyData.nodes[index-1],curVec);

				polyData.lines.Add(newLine1);
				polyData.lines.Add(newLine2);
				curFirstPoint=index+1;
				newOne=true;
			}
			else{
				if(polyData.nodes.Count!=0 && !newOne){
					Line newLine=new Line(polyData.nodes[index-1],curVec);
					polyData.lines.Add(newLine);
				}
				newOne=false;
			}
			polyData.nodes.Add (curVec);
			index++;

		}

		xReader.Close ();
		yReader.Close ();
		buttonReader.Close ();

		string line;
		// read start
		System.IO.StreamReader file;
		file = new System.IO.StreamReader (startFile);
		while ((line = file.ReadLine ()) != null) {
			string[] pos = line.Split(' ');
			polyData.start.Add (new Vector3(float.Parse (pos[0])-1, 1, float.Parse (pos[1])-1));
		}
		file.Close ();

		// read end
		file = new System.IO.StreamReader (goalFile);
		while ((line = file.ReadLine ()) != null) {
			string[] pos = line.Split(' ');
			polyData.end.Add (new Vector3(float.Parse (pos[0])-1, 1, float.Parse (pos[1])-1));
		}
		file.Close ();

		// read customers
		file = new System.IO.StreamReader (customerFile);
		while ((line = file.ReadLine()) != null) {
			string[] pos = line.Split(' ');
			polyData.customers.Add (new Vector3(float.Parse (pos[0])-1, 1, float.Parse (pos[1])-1));
		}

		/*System.IO.StreamReader buttonReader = new System.IO.StreamReader (buttonsFile);
		string button;
		while ((button = buttonReader.ReadLine ()) != null) {
			polyData.buttons.Add (Convert.ToInt32(button));
		}
		buttonReader.Close ();*/
	}
}
