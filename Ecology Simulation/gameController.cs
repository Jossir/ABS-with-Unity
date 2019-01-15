using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gameController : MonoBehaviour {

	public GameObject sheep;
	public int startingNumOfSheep;
	public GameObject groundTile;
	public Vector3 spawnValues;

	public Sprite extraGrass;//has extra food adds ramdomness

	public Sprite grass;
	//for visuals
	public Sprite topLeftGrass;
	public Sprite topRightGrass;
	public Sprite bottomLeftGrass;
	public Sprite bottmRightGrass;
	public Sprite leftGrass;
	public Sprite rightGrass;


	public float timeSpeed;//time starts at 12pm, January
	private float totalSecounds;
	public int minutes;
	private int hours;
	private int days;
	public int months;
	private int years;
	private int monthOfYear;//what month is it in the year
	private int dayOfMonth;
	private bool newMonth;

	public float[] averageTemperaturePerMonth;
	public float[] hoursDaylightPerMonth;
	public float[] totalRainfallPerMonthInMm;
	private float rainfallThisMonth;
	private float daylightThisMonth;

	private int[] daysPerMonth;

	public float temperature;
	public float wheatherStandardDeviationOnTemperature;
	public float hoursIntoTheNight;

	public float rain;//mm/h
	public float rainFrequency;//0-1
	public bool rainToday;
	public float rainLeftThisMonth;
	public float rainLeftTodayInmm;
	public float rainfallStrengthInMM;

	public float sunlight;//% of sunlight getting to the ground
	private bool daytime;

	public float totalRain;
	private GameObject cameraObject;

	private int aliveSheep;
	private int sheepDeaths;
	private int sheepBirths;
	private int pregnantSheep;

	public float grassHealth;
	private float grassGrowRate;
	private float grassSoilMoisture;
	private List<GameObject> tiles = new List<GameObject> ();

	public List<float> rainRecordedHourly = new List<float> ();
	public List<float> sunlightRecordedHourly = new List<float> ();
	public List<float> tempRecordedHourly = new List<float> ();
	public List<float> growRateRecordedHourly = new List<float> ();
	public List<float> soilMoistureRecordedHourly = new List<float> ();
	public List<float> grassHealthRecordedHourly = new List<float> ();
	public List<int> sheepAliveRecordedHourly = new List<int> ();

	// Use this for initialization
	//To do: Have a controller to change fised update
	void Start () {
		Application.runInBackground = true;
		sheepBirths = 0;
		sheepDeaths = 0;
		pregnantSheep = 0;
		//timeSpeed = 0.06666666666666666666666666666667f;//set the time back to 15 minute = 1 min (1secound real life sim)
		//timeSpeed = 1f;
		RainToday ();
		CreateGrass ();
		Spawn (sheep,startingNumOfSheep);
		newMonth = true;
		hours = 0;
		days=0;
		months=0;
		years = 0;
		minutes = 0;
		daysPerMonth = new int[] { 31, 28, 31, 30, 31,30,31,31,30,31,30,31};
		rainfallThisMonth = totalRainfallPerMonthInMm [0];
		daylightThisMonth = hoursDaylightPerMonth [0];
		rainLeftThisMonth = rainfallThisMonth;//sets the amount of rainfall for the rest of the month

		UpdateRainfall();
		UpdateDayNight ();
		UpdateTemperature ();
		UpdateCurruntAvgGrassData ();
		cameraObject = GameObject.FindWithTag ("MainCamera");

		aliveSheep = startingNumOfSheep;
		rainRecordedHourly.Add (0f);
		sunlightRecordedHourly.Add (0f);
		tempRecordedHourly.Add (temperature);
		growRateRecordedHourly.Add (0f);
		soilMoistureRecordedHourly.Add (0.6f);

		sheepAliveRecordedHourly.Add (aliveSheep);
		grassHealthRecordedHourly.Add (grassHealth);


	}

	// Update is called once per frame

	private float elapsed = 0f;
	private float elapsed1 = 0f;

	void Update () {
		
		//sheep.GetComponentInChildren
		Time.timeScale = timeSpeed;

		PopulateTimes ();
		//run every secound(15min) for 1s of waitTime
		//set:1 hour
		elapsed += Time.deltaTime;
		float waitTime = 4f;
		if (elapsed >= waitTime) {
			elapsed = elapsed % waitTime;
			//call methodes here
			totalRain=totalRain+rain;
			UpdateRainfall();
			UpdateDayNight ();
			UpdateSunlight ();
			UpdateTemperature ();
			UpdateCurruntAvgGrassData ();
			int hourOfDay = hours % 24;
//			if (hourOfDay == 4 || hourOfDay == 12 || hourOfDay == 20) {
//				StoreDataThreeTimesPerDay();
//			}
			StoreDataHourly ();
		}
		//set:24 hour/1 day
		elapsed1 += Time.deltaTime;
		float waitTime1 = 96f;
		if (elapsed1 >= waitTime1) {
			elapsed1 = elapsed1 % waitTime1;
			//call methodes here

			RainToday();//determines whether tomorrow will rain
			float tomorrowsRainInMM;
			if (rainLeftThisMonth < 1||daysPerMonth [months % 12]-(dayOfMonth+1)==0) {
				tomorrowsRainInMM = rainLeftThisMonth;

			} else {
				//how much will it rain tomorrow?
				tomorrowsRainInMM = (rainLeftThisMonth / ((daysPerMonth [months % 12] - (dayOfMonth + 1))-((daysPerMonth [months % 12]-dayOfMonth)*(1-rainFrequency))));
				//Adjust to rain more at the beginning of the month to avoide catch up
			}


			//times by rainfrequency to adjust rainfall
			//(1+rainFrequency-(rainFrequency*((dayOfMonth+1)/daysPerMonth[months%12]))
			if (rainToday) {
				rainLeftThisMonth = rainLeftThisMonth - tomorrowsRainInMM+rainLeftTodayInmm;//make up for overflow of rain from last month
				rainLeftTodayInmm = tomorrowsRainInMM;

			} 


		}

		//set:1 month)
		if (dayOfMonth == 0&&!newMonth) {//new month
			rainfallThisMonth = totalRainfallPerMonthInMm [months%12];
			daylightThisMonth = hoursDaylightPerMonth [months%12];
			if(rainLeftThisMonth<1&&rainLeftThisMonth>-1)
			{
				rainLeftThisMonth=0f;//reset to avoid Nad error
			}
			rainLeftThisMonth = rainfallThisMonth+rainLeftThisMonth;//sets the amount of rainfall for the rest of the month

			newMonth = true;
			//call graph controller here!



		} else if(dayOfMonth != 0){
			newMonth = false;
		}
			

	}

	private void StoreDataHourly()
	{
		//called every hour
		rainRecordedHourly.Add (rain);
		sunlightRecordedHourly.Add (sunlight);
		tempRecordedHourly.Add (temperature);
		growRateRecordedHourly.Add (grassGrowRate);
		soilMoistureRecordedHourly.Add (grassSoilMoisture);
		sheepAliveRecordedHourly.Add (aliveSheep);
		grassHealthRecordedHourly.Add (grassHealth);

	}



	void PopulateTimes()//total in time in each individual catogory
	{
		float timeStart = Time.time;

		//15 min ingame is 1 secound real life
		minutes = Mathf.RoundToInt(timeStart*15f);
		hours = minutes / 60;
		days = hours / 24;
		months = 0;//reset
		int daysLeft = days;
		monthOfYear = 0;
		if (daysLeft >= daysPerMonth [0]) {
			//enough days to make atleast one month
			while (daysLeft - daysPerMonth [monthOfYear] >= 0) {//enough days to make another month

			    daysLeft =daysLeft-daysPerMonth [monthOfYear];
				monthOfYear++;
				months++;
				if (monthOfYear == 12) {//last month
					monthOfYear=0;
				}
			}

		}

		dayOfMonth = daysLeft;
		years = months/12;

	}

	void OnGUI() {
		
		GUI.Label(new Rect(10, 20, 200, 20), (dayOfMonth+1)+" "+Month());
		GUI.Label(new Rect(10, 40, 200, 20), "Year "+(years+1).ToString());
		GUI.Label(new Rect(10, 60, 200, 20), (hours%24).ToString("00")+ ":"+Mathf.RoundToInt(minutes%60).ToString("00"));

		GUI.Label(new Rect(10, 100, 200, 20), "Temperature: "+temperature.ToString("0.0")+"°C");
		GUI.Label(new Rect(10, 120, 200, 20), "Sunlight:"+sunlight.ToString("P"));

		GUI.Label(new Rect(10, 140, 200, 20), "Wheather:"+rain.ToString("0.00")+"mm");
		GUI.Label(new Rect(10, 180, 300, 20), "Rain left today: "+rainLeftTodayInmm.ToString("0.0")+"mm");
		GUI.Label(new Rect(10, 200, 300, 20), "Total rain: "+totalRain.ToString("0.0")+"mm");

		GUI.Label(new Rect(10, 240, 200, 20), "Total days: "+days.ToString());

		GUI.Label(new Rect(10, 280, 200, 20), "Total sheep alive: "+aliveSheep.ToString());
		GUI.Label(new Rect(10, 295, 200, 20), "Total sheep deaths: "+sheepDeaths);
		GUI.Label(new Rect(10, 310, 200, 20), "Total sheep births: "+sheepBirths);
		GUI.Label(new Rect(10, 325, 200, 20), "Total sheep pregnant: "+pregnantSheep);
	}

	private void RainToday(){//Randomize days it rains on based on probability

		float r1 = Random.value;
		if ((r1 < rainFrequency))
		{
			rainToday = true;
		} else {
			rainToday = false;
		}

	}

	private void UpdateRainfall(){
		

			if (rainLeftTodayInmm > 0f) {
				//to do different rain strength, for now 4mm/hour of rain
				float r = Random.value;
			if (r < (rainLeftTodayInmm / ((24 - (hours % 24)) * rainfallStrengthInMM))) {
					
					rain = rainfallStrengthInMM;//make it rain on full rainfall strength
					if (rainLeftTodayInmm / rainfallStrengthInMM < 1) {
						rain = rainLeftTodayInmm;//make it rain on left over rain for the day
					}
					if ((hours % 24) == 23) {//last hour of the day
						rain = rainLeftTodayInmm;//make up potential overflow
						rainLeftTodayInmm = 0;
					} else {
						rainLeftTodayInmm = rainLeftTodayInmm - rain;
					}

				} else {//no rain
					rain = 0f;

				}

			}else {
			rain = 0f;

		}



		//4.0 mm/hour




//			Very light rain	precipitation rate is < 0.25 mm/hour
//			Light rain	precipitation rate is between 0.25mm/hour and 1.0mm/hour
//			Moderate rain	precipitation rate is between 1.0 mm/hour and 4.0 mm/hour
//			Heavy rain	recipitation rate is between 4.0 mm/hour and 16.0 mm/hour
//			Very heavy rain   	precipitation rate is between 16.0 mm/hour and 50 mm/hour
//			Extreme rain	recipitation rate is > 50.0 mm/hour

	}




	public string Month(){
		int t = months % 12;
		switch (t) {

		case 0:return"January";
		case 1:return"February";
		case 2:return"March";
		case 3:return"April";
        case 4:return"May";
		case 5:return"June";
		case 6:return"July";
		case 7:return"August";
		case 8:return"September";
		case 9:return"October";
		case 10:return"November";
		case 11:return"December";
}
		return"Error -monthfunction()";
	}

	public int monthLength(){
		int t = months % 12;
		switch (t) {
		//daysPerMonth = new int[] { 31, 28, 31, 30, 31,30,31,31,30,31,30,31};
		case 0:return 31;
		case 1:return 28;
		case 2:return 31;
		case 3:return 30;
		case 4:return 31;
		case 5:return 30;
		case 6:return 31;
		case 7:return 31;
		case 8:return 30;
		case 9:return 31;
		case 10:return 30;
		case 11:return 31;
		}
		return -1;
	}


	private void UpdateDayNight(){
		

		float sunRise = (24-daylightThisMonth)/2;
		if (hours%24 >= sunRise && hours%24 < 24 - sunRise) {
			daytime = true;
			hoursIntoTheNight = 0;//reset
		} else {
			daytime = false;
			hoursIntoTheNight++;
		}
		//to do:sunlight lighting
//		Gradient g;
//		GradientColorKey[] gck;
//		GradientAlphaKey[] gak;
//		g = new Gradient();
//		gck = new GradientColorKey[2];
//		gck[0].color = Color.red;
//		gck[0].time = 0.0F;
//		gck[1].color = Color.blue;
//		gck[1].time = 1.0F;
//		gak = new GradientAlphaKey[2];
//		gak[0].alpha = 1.0F;
//		gak[0].time = 0.0F;
//		gak[1].alpha = 0.0F;
//		gak[1].time = 1.0F;
//		g.SetKeys(gck, gak);
//		cameraObject.GetComponent <Camera> ().backgroundColor = (g.Evaluate(0.75F));

		//or
		//public float step = 0;
		//Color.Lerp(colorStart, colorEnd, step);
		//step += Time.deltaTime / duration;


	}

	private void UpdateSunlight(){
		//called every hour
		float sunRise = (24-daylightThisMonth)/2;
		if (daytime) {
			//daytime
				float hoursSinceSunRise =hours%24-sunRise;
				float hoursTillSunSet = daylightThisMonth - hoursSinceSunRise;
				if (hoursSinceSunRise < 1||hoursTillSunSet < 1) {
					sunlight = 0.15f;
				} else if (hoursSinceSunRise < 2||hoursTillSunSet < 2) {
				sunlight = 0.35f;
				} else if (hoursSinceSunRise < 4||hoursTillSunSet < 4) {
				sunlight = 0.50f;
				} else if (hoursSinceSunRise < 6||hoursTillSunSet < 6) {
				sunlight = 0.85f;
				} else {
					sunlight = 1f;
				}
		} else {
			//night
			sunlight=0f;
		}
		if (rain > 0) {
			sunlight = sunlight * 0.5f;
			//ADD more variations 
		}

	}

	private void UpdateTemperature()
	{
		//called every hour

		float avgTemperature = averageTemperaturePerMonth [months%12];


		float temparetureDaytimeModifier = sunlight * wheatherStandardDeviationOnTemperature;

		float temparetureNighttimeModifier = (wheatherStandardDeviationOnTemperature)*((hoursIntoTheNight+1)/(24-daylightThisMonth));

		if (daytime) {
			//day
			temperature = avgTemperature+temparetureDaytimeModifier;
			
		} else {
			//night
			if ((hoursIntoTheNight - (24 - daylightThisMonth)) >= 0) {
				//last hour of night, start warming up
				temperature = temperature+ wheatherStandardDeviationOnTemperature/2;
			} else {
				temperature = avgTemperature-temparetureNighttimeModifier;
			}

		}



	}




	void Spawn(GameObject animal, int numOFAnimals)
	{
	for (int i = 0; i < numOFAnimals; i++) {
			
			Vector3 spawnPosition = new Vector3(Random.Range(-spawnValues.x, spawnValues.x),Random.Range(-spawnValues.y, spawnValues.y),-1);
			//Quaternion spawnRotation = Quaternion.Euler(0,0,0);
			Instantiate (animal, spawnPosition, Quaternion.identity);
		}

	}

	private void CreateGrass()
	{
		Vector3 offset = new Vector3(0.5f, 0.5f, 0);

		for(int i = (int)-spawnValues.x;i<(int)spawnValues.x;i=i+2)
			{
			for(int j = (int)-spawnValues.y;j<(int)spawnValues.y;j=j+2)
				{
				GameObject tempGrass = Instantiate(groundTile,new Vector3(i, j, 0)+offset,Quaternion.identity);
				tempGrass.transform.SetParent(this.transform,false);
				tiles.Add (tempGrass);
				}
			}
	}


	private void UpdateCurruntAvgGrassData()
	{
		float attractivness = 0f;
		float soilMoisture = 0f;
		float growRate = 0f;
		foreach (var item in tiles) {
			attractivness =attractivness+ item.GetComponent<GrassController> ().attractiveness;
			soilMoisture = soilMoisture + item.GetComponent<GrassController> ().soilMoisture;
			growRate = growRate + item.GetComponent<GrassController> ().growRate;
		}
		float numTiles = tiles.Count;
		grassHealth= attractivness / numTiles;
		grassSoilMoisture = soilMoisture / numTiles;
		grassGrowRate = growRate / numTiles;

	}



	public void SheepDied()
	{
		aliveSheep--;
		sheepDeaths++;
	}
	public void SheepBorn()
	{
		aliveSheep++;
		sheepBirths++;
		pregnantSheep--;
	}
	public void SheepBorn(int num)
	{
		aliveSheep= aliveSheep+num;
	}
	public void PregnantSheep()
	{
		pregnantSheep++;
	}
}
