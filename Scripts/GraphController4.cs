using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphController4 : MonoBehaviour {

	public Transform pointPrefabVeg;
	public Transform pointPrefabSheep;
	public Transform graphLinePrefab;
	private gameController gameControllerObject;


	//lines
	Transform[] pointsVeg;
	Transform[] pointsSheep;

	private SpriteRenderer[] sr;

	private int lastMonth;
	private int lastYear;


	void Start ()
	{
		lastMonth = 1;
		lastYear = 1;
		gameControllerObject = GameObject.FindWithTag ("GameController").GetComponent<gameController> ();


		//sr.color 

		//create graphs here
		CreateGraphs ();

	}
	//
	private float elapsed = 0f;
	private float elapsed1 = 0f;


	void Update ()
	{

		elapsed += Time.deltaTime;
		float waitTime = 4f;//1 hour
		if (elapsed >= waitTime) {
			elapsed = elapsed % waitTime;
			//Updates all graphs every hour




		}

		if (lastMonth < gameControllerObject.months) {
			lastMonth++;
			//begining on new month

			if (lastYear < (lastMonth / 12)) {
				lastYear++;
				//begining of new year


			}
		}

		elapsed1 += Time.deltaTime;
		float waitTime1 = 4f * 24f;//1 day
		if (elapsed1 >= waitTime1) {
			elapsed1 = elapsed1 % waitTime1;
			//call methods here

			UpdateGraph ();
		}

	}

	private void CreateGraphs ()
	{


		int numOfPoints = 360;
		float step = 2f / (float)numOfPoints;
		float step2 = 2f / 100f;
		Vector3 scale = Vector3.one * step2;
		Vector3 position;
		position.y = -1f;
		position.z = 1f;

		pointsVeg = new Transform[numOfPoints];
		for (int i = 0; i < pointsVeg.Length; i++) {
			Transform point = Instantiate (pointPrefabVeg);
			position.x = (i + 0.5f) * step - 1f;
			point.localPosition = position;
			point.localScale = scale;
			point.SetParent (transform, false);
			pointsVeg [i] = point;
		}
			
		pointsSheep = new Transform[numOfPoints];
		position.x = 0f;
		for (int i = 0; i < pointsSheep.Length; i++) {
			Transform point = Instantiate (pointPrefabSheep);
			position.x = (i + 0.5f) * step - 1f;
			point.localPosition = position;
			point.localScale = scale;
			point.SetParent (transform, false);
			pointsSheep [i] = point;
		}

		//create line
		CreateLine();




	}
	private void CreateLine()
	{
		Transform lineH = Instantiate (graphLinePrefab);
		lineH.SetParent (transform, false);
		lineH.localPosition = new Vector3(0f, -1f, -1.0f);

		Transform lineV = Instantiate (graphLinePrefab);
		lineV.SetParent (transform, false);
		lineV.rotation = Quaternion.Euler(0,0,90);

		lineV.localPosition = new Vector3(-1f, 0f, -1.0f);

	}

	private void DeleteGraph ()
	{
		foreach (var element in pointsVeg) {
			Destroy (element.gameObject);
		}
	}

	private void UpdateGraph ()
	{

		int numOfStoredDataPoints = gameControllerObject.grassHealthRecordedHourly.Count;
		int buffer = 0;
		if (numOfStoredDataPoints > 360*24) {
			buffer = numOfStoredDataPoints - 360*24;

		}
		for (int i = 0; i < pointsVeg.Length; i++) {



			Transform pointVeg = pointsVeg [i];
			Vector3 positionVeg = pointVeg.localPosition;
		

			Transform pointSheep = pointsSheep [i];
			Vector3 positionSheep = pointSheep.localPosition;


			//position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
			if (numOfStoredDataPoints > 0) {


				positionVeg.z = 0f;
				positionSheep.z = 0f;

				positionVeg.y = gameControllerObject.grassHealthRecordedHourly [i*24+ buffer*24]*2-1;
				positionSheep.y = (gameControllerObject.sheepAliveRecordedHourly [i*24+buffer*24]/200f)*2-1;

			} else {
				positionVeg.y = -1f;
				positionVeg.z = 1f;
				positionSheep.y =-1f;
				positionSheep.z = 1f;
			}

			pointVeg.localPosition = positionVeg;
			pointSheep.localPosition = positionSheep;

			numOfStoredDataPoints--;

		}




	}


}
