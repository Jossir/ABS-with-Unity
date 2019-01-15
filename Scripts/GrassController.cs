using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrassController : MonoBehaviour
{
	private BoxCollider2D BC2D;
	private SpriteRenderer SR;
	public Sprite grownGrass;
	public Sprite eatenGrass;
	public Sprite grownGrass95;
	public Sprite grownGrass80;

	public GameObject gameControllerObject;

	//do check here for what type of grass, by looking at sprite

	public float growRate;
//% of max reachable grow rate

	public float vegitationMass = 600f;
	//in grams
	public float vegitationMassMax;
	public float growRateMultiplier;
	public float vegitationEnergy;
	//energy stored from the sun. out of a 1000;

	private bool eaten;

	public float attractiveness;
//To DO: look at grass around

	//how fast soil drains
	public float soilMoisture;
	//% Volumetric Water Content

	private float rain;
	// Use this for initialization
	void Start ()
	{
		BC2D = GetComponent<BoxCollider2D> ();
		SR = GetComponent<SpriteRenderer> ();
		eaten = false;
		grownGrass = GetComponent<SpriteRenderer> ().sprite;
		growRateMultiplier = 1f;
		vegitationEnergy = 600f;
		soilMoisture = 0.6f;//(1-0) 60%


		gameControllerObject = GameObject.FindWithTag ("GameController");
		growRate = 0f;
		rain = gameControllerObject.GetComponent <gameController> ().rain;
		//if(Sr
		//SR.sprite = eatenGrass;
		vegitationMassMax = 1000f;
		attractiveness = vegitationMass / vegitationMassMax;
	}

	//To do: grass different energy states



	// Update is called once per frame
	private float elapsed = 0f;

	void Update ()
	{
		
		//set:1 hour
		elapsed += Time.deltaTime;
		float waitTime = 4f;
		if (elapsed >= waitTime) {
			elapsed = elapsed % waitTime;

			UpdateSoilMoisture ();
			UpdateStoredVegitationEnergy ();
			UpdateGrowRate ();
			attractiveness = vegitationMass / vegitationMassMax;

		}
		UpdateSprite ();
	
	}

	void changeState (Sprite p)
	{
		if (!(SR.sprite == p)) {
			SR.sprite = p;
		}
	}
	//https://observant.zendesk.com/hc/en-us/articles/208067926-Monitoring-Soil-Moisture-for-Optimal-Crop-Growth
	//
	private void UpdateSoilMoisture ()
	{
		//rain, sun, temp, soilPermeability
		//called every hour
		float rain = gameControllerObject.GetComponent <gameController> ().rain;
		float temperature = gameControllerObject.GetComponent <gameController> ().temperature;
		float sun = gameControllerObject.GetComponent <gameController> ().sunlight;


		float moistureModifier = 0f;

		if (rain > 0) {
			//raining
			moistureModifier = moistureModifier + 0.05f * rain;//+5% each mm of rain

		}
		if (temperature >= 0) {
			//eveporation
			//done: less eveopration when theres less water avaliable
			//from temp
			moistureModifier = moistureModifier - (0.0005f * temperature) * soilMoisture;//-0.05% each temperature degree, when 100% soil moisture


			//evep from direct sunlight
			moistureModifier = moistureModifier - (0.01f * (sun)) * soilMoisture;//-1% for full sun, when 100% soil moisture

		} else {
			//evep from direct sunlight SNOW SUBLIMATION(slower)when ice/snow is coverted straight to vapour
			moistureModifier = moistureModifier - (0.005f * (sun)) * soilMoisture;//-0.5% for full sun, when 100% soil moisture
		}

		//plants water use
		moistureModifier = moistureModifier - (vegitationMass / vegitationMassMax) / 200f;//-0.5% moisture per hour when vegitaion is at 100%



		//runnoff
		moistureModifier = moistureModifier - soilMoisture * 0.001f;//0.1% less moisture per hour when 100% soil moisture. more mositure = greater runnoff
		//flood senerio
		if (soilMoisture > 0.90f) {
			//flood, soil looses lots of moisture from runnoff
			moistureModifier = moistureModifier - 0.03f;//-3%
		} else if (soilMoisture > 0.96f) {
			//flood, soil looses lots of moisture from runnoff
			moistureModifier = moistureModifier - 0.03f;//-3%
		} else if (soilMoisture > 0.99f) {
			//flood, soil looses lots of moisture from runnoff
			moistureModifier = moistureModifier - 0.09f;//-9%
		}

		soilMoisture = soilMoisture + moistureModifier;




	}

	private void UpdateStoredVegitationEnergy ()
	{
		//called every hour
		//sun=energy
		float sun = gameControllerObject.GetComponent <gameController> ().sunlight;
		float storeRate = ((vegitationMass * 0.9f) / vegitationMassMax);//% of avliable plant for

		float vegitationEnergyModifier = sun * storeRate * 20;

		if (sun > 0 && vegitationEnergy < 1000) {
			

			vegitationEnergy = vegitationEnergy + vegitationEnergyModifier;
		} 

	}


	private void UpdateGrowRate ()
	{
		//Sunlight, soil moisture, temp, Fertile, Time
		//called every hour
		float temperature = gameControllerObject.GetComponent <gameController> ().temperature;


		float growModifier = 0f;//in grams per hour
		if (vegitationEnergy > 0) {
			growModifier = 10 * growRateMultiplier;//max it can grow g/hour - set 10g per hour max
		}

		//use up stored energy from sun
		//to do: 
		if (vegitationEnergy <= 0) {
			//start dying, not enugh stored sunlight
			growModifier = -5f;
			vegitationEnergy = 0;//reset overflow
		} else if (vegitationEnergy < 25) {
			growModifier = growModifier * 0.1f;
		} else if (vegitationEnergy < 50) {
			growModifier = growModifier * 0.2f;
		} else if (vegitationEnergy < 75) {
			growModifier = growModifier * 0.4f;
		} else if (vegitationEnergy < 100) {
			growModifier = growModifier * 0.6f;
		} 


		//use soil moisture
		//also deducts energy in extreme whether
		if (soilMoisture > 0.8f) {//flood - not enough oxygen in soil, thefore dying
			growModifier = growModifier - 5f;

		} else if (soilMoisture > 0.7f) {//extreme wet
			growModifier = growModifier * 0.1f;
		} else if (soilMoisture > 0.6f) {//Too wet
			growModifier = growModifier * 0.3f;
		} else if (soilMoisture > 0.4f) {//optimal
			growModifier = growModifier * 1f;
		} else if (soilMoisture > 0.2f) {//Too dry
			growModifier = growModifier * 0.3f;
		} else if (soilMoisture > 0.05f) {//extremely dry
			growModifier = growModifier * 0.1f;
		} else if (soilMoisture > 0.02f) {//extremely dry
			growModifier = growModifier * 0.01f;
		} else {//completely dry
			growModifier = growModifier - 25f;
			//vegitation dying - lack of water
		}


		//Temperature modifier
		//can't be freezing to grow
		//to do: show ice when freezing

		if (temperature > -2) {

			if (temperature >= 18 && temperature <= 27) {
				//perfect temp, reduce nothing

			} else if (temperature > 55) {//way too hot
				//-50g
				growModifier = growModifier - 50f;
			} else if (temperature > 45) {//too hot
				//-2g
				growModifier = growModifier - 2f;
			} else if (temperature < 5 || temperature > 39) {
				//10%
				growModifier = growModifier * 0.1f;
			} else if (temperature < 7 || temperature > 37) {
				//20%
				growModifier = growModifier * 0.2f;
			} else if (temperature < 9 || temperature > 35) {
				//35%
				growModifier = growModifier * 0.35f;
			} else if (temperature < 11 || temperature > 32) {
				//50%
				growModifier = growModifier * 0.5f;
			} else if (temperature < 15 || temperature > 30) {
				//70%
				growModifier = growModifier * 0.7f;
			} else if (temperature < 15 || temperature > 28) {
				//90%
				growModifier = growModifier * 0.9f;
			}

		} else {
			//freezing
			growModifier = growModifier - 50f;
		}

		if (vegitationMass <= vegitationMassMax) {
			vegitationMass = Mathf.Abs (vegitationMass + growModifier);

		} else {
			vegitationMass = vegitationMassMax;
			//control for overflow
		}
		growRate = growModifier / (10 * growRateMultiplier);
		float energyUsedFromGrowing = (vegitationMass / vegitationMassMax) * 20f;
		vegitationEnergy = vegitationEnergy - energyUsedFromGrowing * Mathf.Abs (growModifier / 10);//use up stored energy


	}



	private void UpdateSprite ()
	{
		//change sprite
		if (vegitationMass <= 50 && eaten == false) {
			changeState (eatenGrass);
			eaten = true;
		} else if (vegitationMass > 750) {
			changeState (grownGrass);
			eaten = false;
		} else if (vegitationMass > 550) {
			changeState (grownGrass95);
			eaten = false;
		} else if (vegitationMass > 300) {
			changeState (grownGrass80);
			eaten = false;
		}



	}


	public void Gazed (float t)
	{
		vegitationMass = vegitationMass - t;

	}



}
