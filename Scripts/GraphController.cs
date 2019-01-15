using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphController : MonoBehaviour
{
	public Transform pointPrefabVeg;
	public Transform pointPrefabSheep;
	public Transform pointPrefabGrowRate;
	public Transform graphLinePrefab;
	private gameController gameControllerObject;


	//lines
	Transform[] pointsVeg;
	Transform[] pointsGrowRate;
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

			UpdateGraph ();


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
		float waitTime1 = 4f * 24f * 10f;//10 days
		if (elapsed1 >= waitTime1) {
			elapsed1 = elapsed1 % waitTime1;
			//call methods here


		}

	}

	private void CreateGraphs ()
	{
		

		int numOfPoints = 720;
		float step = 2f / (float)numOfPoints;
		float step2 = 2f / 100f;
		Vector3 scale = Vector3.one * step2;
		Vector3 position;
		position.y = 0f;
		position.z = -1f;

		pointsVeg = new Transform[numOfPoints];
		for (int i = 0; i < pointsVeg.Length; i++) {
			Transform point = Instantiate (pointPrefabVeg);
			position.x = (i + 0.5f) * step - 1f;
			point.localPosition = position;
			point.localScale = scale;
			point.SetParent (transform, false);
			pointsVeg [i] = point;
		}

		pointsGrowRate = new Transform[numOfPoints];
		position.x = 0f;
		for (int i = 0; i < pointsGrowRate.Length; i++) {
			Transform point = Instantiate (pointPrefabGrowRate);
			position.x = (i + 0.5f) * step - 1f;
			point.localPosition = position;
			point.localScale = scale;
			point.SetParent (transform, false);
			pointsGrowRate [i] = point;
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
		lineH.localPosition = new Vector3(0f, 0f, -1.0f);

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
		foreach (var element in pointsGrowRate) {
			Destroy (element.gameObject);
		}


	}

	private void UpdateGraph ()
	{
		
		int numOfStoredDataPoints = gameControllerObject.grassHealthRecordedHourly.Count;
		int buffer = 0;
		if (numOfStoredDataPoints > 720) {
			buffer = numOfStoredDataPoints - 720;

		}
		for (int i = 0; i < pointsGrowRate.Length; i++) {
			


			Transform pointVeg = pointsVeg [i];
			Vector3 positionVeg = pointVeg.localPosition;

			Transform pointGrowRate = pointsGrowRate [i];
			Vector3 positionGrowRate = pointGrowRate.localPosition;

			Transform pointSheep = pointsSheep [i];
			Vector3 positionSheep = pointSheep.localPosition;


			//position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
			if (numOfStoredDataPoints > 0) {

				positionGrowRate.z = 0f;
				positionVeg.z = 0f;
				positionSheep.z = 0f;
				
				positionVeg.y = gameControllerObject.grassHealthRecordedHourly [i+ buffer];
				positionGrowRate.y = gameControllerObject.growRateRecordedHourly [i+ buffer]/2f;
				positionSheep.y = gameControllerObject.sheepAliveRecordedHourly [i+ buffer]/200f;

			} else {
				//to do: change z asis to hide line
				positionVeg.y = 0f;
				positionVeg.z = 1f;
				positionGrowRate.y = 0f;
				positionGrowRate.z = 1f;
				positionSheep.y =0f;
				positionSheep.z = 1f;
			}

			pointVeg.localPosition = positionVeg;
			pointGrowRate.localPosition = positionGrowRate;
			pointSheep.localPosition = positionSheep;

			numOfStoredDataPoints--;

		}

		
		

	}




}