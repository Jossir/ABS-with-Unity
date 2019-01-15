using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraphController2 : MonoBehaviour {


	public Transform graphLinePrefab;
	private gameController gameControllerObject;

	public Transform pointPrefabSun;
	public Transform pointPrefabSoilMoisture;
	public Transform pointPrefabGrowRate;
	public Transform pointPrefabTemp;

	//lines

	Transform[] pointsGrowRate;
	Transform[] pointsSoilMoisture;
	Transform[] pointsSun;
	Transform[] pointsTemp;

	private int lastMonth;
	private int lastYear;


	void Start ()
	{
		lastMonth = 1;
		lastYear = 1;
		gameControllerObject = GameObject.FindWithTag ("GameController").GetComponent<gameController> ();


		//create graphs here
		CreateGraphs ();

	}

	private float elapsed = 0f;
	private float elapsed1 = 0f;


	void Update ()
	{

		elapsed += Time.deltaTime;
		float waitTime = 4f ;//1 hour
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

	private void CreateGraphs ()
	{
		

			int numOfPoints = 168;
			float step = 2f / numOfPoints;//7*24
			float step2 = 2f / 100f;//size of points
			Vector3 scale = Vector3.one * step2;
			Vector3 position;
			position.y = 0f;
			position.z = -1f;

			pointsGrowRate = new Transform[numOfPoints];
			for (int i = 0; i < pointsGrowRate.Length; i++) {
			Transform point = Instantiate (pointPrefabGrowRate);
				position.x = (i + 0.5f) * step - 1f;

				point.localPosition = position;
				point.localScale = scale;
				point.SetParent (transform, false);
				pointsGrowRate [i] = point;
			}

			pointsSoilMoisture = new Transform[numOfPoints];
			position.x = 0f;
			for (int i = 0; i < pointsSoilMoisture.Length; i++) {
			Transform point = Instantiate (pointPrefabSoilMoisture);
				position.x = (i + 0.5f) * step - 1f;
				point.localPosition = position;
				point.localScale = scale;
				point.SetParent (transform, false);
				pointsSoilMoisture  [i] = point;
			}
			position.x = 0f;
			pointsSun = new Transform[numOfPoints];
			for (int i = 0; i < pointsSun.Length; i++) {
			Transform point = Instantiate (pointPrefabSun);
				position.x = (i + 0.5f) * step - 1f;
				point.localPosition = position;
				point.localScale = scale;
				point.SetParent (transform, false);
				pointsSun  [i] = point;
			}
			position.x = 0f;
			pointsTemp = new Transform[numOfPoints];
			for (int i = 0; i < pointsTemp.Length; i++) {
			Transform point = Instantiate (pointPrefabTemp);
				position.x = (i + 0.5f) * step - 1f;
				point.localPosition = position;
				point.localScale = scale;
				point.SetParent (transform, false);
				pointsTemp  [i] = point;
			}
			position.x = 0f;
			//create line
			CreateLine();
			//lineV.position.Set(0,-20,0);


			//text.transform.SetParent(transform,false);




	}


	private void DeleteGraph ()
	{
		

			foreach (var element in pointsGrowRate) {
				Destroy (element.gameObject);
			}


	}

	private void UpdateGraph ()
	{
		

			int numOfStoredDataPoints = gameControllerObject.soilMoistureRecordedHourly.Count;
			int buffer = 0;
			if (numOfStoredDataPoints >= 168) {
				buffer = numOfStoredDataPoints-168;

			}
		for (int i = 0; i < pointsGrowRate.Length; i++) {


				Transform pointGrowRate = pointsGrowRate [i];
				Vector3 positionGrowRate = pointGrowRate.localPosition;

				Transform pointSoilMoisture = pointsSoilMoisture [i];
				Vector3 positionSoilMoisture = pointSoilMoisture.localPosition;

				Transform pointSun = pointsSun [i];
				Vector3 positionSun = pointSun.localPosition;

				Transform pointTemp = pointsTemp [i];
				Vector3 positionTemperature = pointTemp.localPosition;

				//position.y = Mathf.Sin(Mathf.PI * (position.x + Time.time));
				if (numOfStoredDataPoints > 0) {
				positionGrowRate.z = 0f;

				positionSoilMoisture.z = 0f;

				positionSun.z = 0f;

				positionTemperature.z = 0f;

				positionGrowRate.y = gameControllerObject.growRateRecordedHourly[i+buffer]/2f;
				positionSoilMoisture.y =gameControllerObject.soilMoistureRecordedHourly[i+buffer];
				positionSun.y =gameControllerObject.sunlightRecordedHourly[i+buffer];
				if (gameControllerObject.tempRecordedHourly [i+buffer] > 0) {
					positionTemperature.y = gameControllerObject.tempRecordedHourly [i+buffer] / 50f;
				} else {
					positionTemperature.y = gameControllerObject.tempRecordedHourly [i+buffer];
				}

				} else {
					//to do: change z asis to hide line
					positionGrowRate.y = 0f;
				positionGrowRate.z = 1f;
					positionSoilMoisture.y = 0f;
				positionSoilMoisture.z = 1f;
					positionSun.y = 0f;
				positionSun.z = 1f;
					positionTemperature.y = 0f;
				positionTemperature.z = 1f;
				}

				pointSoilMoisture.localPosition = positionSoilMoisture;
				pointGrowRate.localPosition = positionGrowRate;
				pointSun.localPosition = positionSun;
				pointTemp.localPosition = positionTemperature;
				//pointSoilMoisture.localPosition = positionSoilMoisture;

				numOfStoredDataPoints--;

			}


	}

}
