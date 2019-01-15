using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class Boundary
{
	public float xMin, xMax, yMin, yMax;

}

public class sheepController : MonoBehaviour
{
	
	private Rigidbody2D rb;
	private SpriteRenderer sr;

	private Vector2 movePosition;

	private Vector2 randomVelocity;

	public int reproductionRate;
	public int energyFromFood;
	public Boundary boundary;

	public GameObject sheepBaby;

	public gameController gameControllerObject;

	private Transform SpriteTransform;
	private static Transform SpriteTransformOriginal;
	public Sprite eat;
	public Sprite eat1;
	public Sprite stand;
	public Sprite sleep;
	public Sprite rest;
	public Sprite die;
	public float maxVision;
	public float stomach;
	//in grams how much a sheep eats
	//short term
	public float speed;

	public float weight;
	//kg
	//long term
	public float maxWeight;

	public float sleepNeededToday;

	public float ageInYears;
	public bool isRam;
	public bool isLeader;
	public bool fertile;

	public bool pregnant;
	public float hoursPregnant;
	public float hoursNeededPregnant;
	//140 * 24 for realistic value

	public float energy;
	//out of a 1000;


	public bool busy;
	//if the sheep is busy with a current task

	public List<Rigidbody2D> sheepInVision = new List<Rigidbody2D> ();
	public List<Rigidbody2D> leaderSheepInVision = new List<Rigidbody2D> ();

	public List<Rigidbody2D> predatorInVision = new List<Rigidbody2D> ();
	public List<Rigidbody2D> tilesInVision = new List<Rigidbody2D> ();


	//0=moving away, 1=move towards, 2=other actions
	private Collider2D grassTarget;

	private List<Collider> grass;
	//http://www.sheep101.info/sheepandgoats.html - for all sheep details

	public Vector2 averagePositionOfSheep;

	public GameObject groundUnderAnimal;

	public bool full;

	public bool traveling;
	public bool fighting;
	//so other sheep dont follow fighting bulls
	private Vector2 destination;
	public Rigidbody2D contestingRam;

	public bool readyToSleep;
	public int sheepReadyToSleepNearby;

	private float sun;
	private bool sleeping;
	public float timeLeftToSleep;
	private bool dead;
	public float repelDistance;

	public float days;


	private IEnumerator coroutine;

	public Vector2 spriteOriginalScale;

	public float grownPercentage;
	public bool notLamb;
	public float radiousOfNewLands;

	public float leaderVisionScale;

	//public Rigidbody2D mom;

	//public int drinksMilk;
	//private int milkLeftToDrink;
	//1,2,3,4 every 2 hours depending on age

	public int noLeader;

	public CircleCollider2D sheepVisionCollider;

	public float starvationWeightLoss;
	//body weight in g sheep can transfer to energy before the sheep starves(20%)
	public float maxWeightReached;

	public float chancesOfLambs;

	//done: only Explore when leader is not around for 2 hours
	//if stored energy gets very low, transfer a bit of body weight to stored energy(sheep getting skinny)
	//pregant sheep use energy(especially last month) and when sheep drinks milk

	//To do
	//fx: leader leading when all other sheep sleeping
	//show femal and male sheep
	//ocacional two leader sheep stuck on top of each other
	void Start ()
	{
		hoursNeededPregnant = Random.Range (3000f,3456f);//between 130 and 140 days pregnant
		starvationWeightLoss = 0f;
		
		hoursPregnant = 0f;
		fertile = false;
		dead = false;
		sleeping = false;
		timeLeftToSleep = 0f;
		days = 0f;
		gameControllerObject = GameObject.FindWithTag ("GameController").GetComponent<gameController> ();
		sun = gameControllerObject.sunlight;

		readyToSleep = false;
		float t = Random.Range (0f, 1f);

		if (chancesOfLambs > t) {
			notLamb = true;
		}

		if (notLamb) {
			//adult sheep
			if (t < 0.5f) {
				isRam = true; //50% chance of male
				maxWeight = 140f + t * 20f;
				weight = Random.Range (80f, 100f);


			} else {
				maxWeight = 100f;
				weight = Random.Range (60f, 80f);
			}
			grownPercentage = weight / maxWeight;

			ageInYears = Random.Range (2f, 4f);


		} else {
			//lamb
			//drinksMilk = 4;
			//milkLeftToDrink = drinksMilk;
			if (t < 0.5f) {
				
				isRam = true; //50% chance of male
				maxWeight = 140f + t * 20f;//add a 10kg randomness max(different genes)

			} else {
				maxWeight = 100f;
			}
			weight = Random.Range (3f, 5f);
			grownPercentage = weight / maxWeight;

			//temp
			//repelDistance =repelDistance* 0.5f;

			ageInYears = 0f;

		}
		isLeader = false;

		full = false;
		busy = false;
		stomach = (weight * 1000f * 0.025f) / 5;//20% fullness of daily requirement

		rb = GetComponent<Rigidbody2D> ();
		sheepVisionCollider = rb.GetComponent<CircleCollider2D> ();
		sr = rb.gameObject.GetComponentInChildren<SpriteRenderer> ();
		grass = new List<Collider> ();
		fighting = false;



		UpdateLeadersAndSleepersList ();
		SpriteTransform = GetComponentInChildren<Transform> ();
		SpriteTransformOriginal = SpriteTransform;
//		foreach (var item in GetComponentsInChildren<Transform> ()) {
//			if (item.tag == "New Sprite") {
//				SpriteTransform = item;
//			}
//		}

		UpdateSpriteSizeAndCircleCollider ();
		maxWeightReached = weight;
	}

	private float elapsed = 0f;
	private float elapsed1 = 0f;
	private float elapsed0 = 0f;
	private float elapsed2 = 0f;







	void Update ()
	{
		//if the sheep is dead it will start decaying for 1 day before disapearing
		if (!dead) {



			//call loop handles all the movement and actions of sheep
			elapsed0 += Time.deltaTime;
			float waitTime0 = 4f / 60f;//1 min
			if (elapsed0 >= waitTime0) {
				elapsed0 = elapsed0 % waitTime0;
				//call here
				UpdateLeadersAndSleepersList ();
				if (!traveling) {//allow the sheap to reach destination
					
					rb.velocity = new Vector2 ();
					groundUnderAnimal = FindClosestTile (tilesInVision).gameObject;

					if (sheepInVision.Count != 0) {
						
						if (!sleeping) {
							
							if (isLeader) {
								//leader
								FightLeader ();
								//fight leader
								if (groundUnderAnimal.gameObject.GetComponent<GrassController> ().vegitationMass < 750 && (sheepReadyToSleepNearby < (sheepInVision.Count * 0.5f))) {//if ground under leader is less than expected
									destination = NewLands (tilesInVision, radiousOfNewLands, 0.3f);//returns the current position of sheep if satisfied in current spot
									MoveToInputAllTheWay ();//this pauses the 1 min loop, until destination reached


								}

							}//keep minmum distance from other sheep

							float tMin = 0.3f;
							if (grownPercentage > tMin) {
								tMin = grownPercentage;
							}

							Rigidbody2D closestSheep = GetClosestSheep (repelDistance - (1 - tMin) * repelDistance);
							//repel away from other sheep otherwise do other actions
							if (closestSheep != null) {
								movePosition = Vector2.MoveTowards (rb.position, closestSheep.position, -0.10f);
								rb.position = movePosition;

							} else {
								//follows leader at different speed acording to proximity?
								//if not moveing away then following leader and eating Veg
								if (!full && groundUnderAnimal.gameObject.GetComponent<GrassController> ().vegitationMass > 10f) {
									EatingVegitaion ();
								} else {
									Waiting ();
								}


//								int increasedRange = 0;
//
//								if (isLeader) {//so leader doesnt back track
//									increasedRange = 5;
//								} 

								averagePositionOfSheep = AveragePositionSheep (sheepInVision);
								if (averagePositionOfSheep != null) {
									int bufferSpace = 4;
									averagePositionOfSheep.x = Mathf.Clamp (averagePositionOfSheep.x, boundary.xMin + bufferSpace, boundary.xMax - bufferSpace);
									averagePositionOfSheep.y = Mathf.Clamp (averagePositionOfSheep.y, boundary.yMin + bufferSpace, boundary.yMax - bufferSpace);
									//this forces the average sheep position to be within the tiles
									//go to center of group
									//if (Vector2.Distance (rb.position, averagePositionOfSheep) > Random.Range (1.5f, 5f) + (sheepInVision.Count / 3) + increasedRange) {//sheepInVision.Count/5 = 1 extra unit of distance every 5 sheep
									sr.sprite = stand;

									float stepSize = Vector2.Distance (rb.position, averagePositionOfSheep);

									//stepSize = stepSize - ((sheepInVision.Count / 3f));
									stepSize = stepSize/((sheepInVision.Count / 3f));

									stepSize = (0.008f * Mathf.Abs (stepSize));

									if (notLamb) {
										MoveToInput (averagePositionOfSheep, stepSize);
									} else {
										MoveToInput (averagePositionOfSheep, stepSize * 0.05f);
									}
								}
								//MoveToInput (averagePositionOfSheep, 0.025f);
								//}


								//follow leader
								if (!isLeader) {
									//notleader

									if (leaderSheepInVision.Count > 0) {//has to be leader to follow
										noLeader = 0;

										//float t2 = Vector2.Distance (rb.position, destination) - 1;
										float stepSize2 = Vector2.Distance (rb.position, leaderSheepInVision [0].position) - 1;


										FollowLeader (0.008f * Mathf.Abs (stepSize2));


									} else {
										//explore on own until found leader
										noLeader++;
										if (noLeader >= 720) {//no leader around for 360min, start searching
											Explore (0.04f);
										}

									}
								} 

							}



								
						} else {
							timeLeftToSleep = timeLeftToSleep - 1;
							if (timeLeftToSleep <= 0f) {

								sleeping = false;
								readyToSleep = false;
								sr.sprite = stand;
							}

//							if (milkLeftToDrink > 0) {//till 6 weeks old drinks milk
//
//								if (!FollowParent (1)) {
//									if (!full && groundUnderAnimal.gameObject.GetComponent<GrassController> ().vegitationMass > 10f) {
//										EatingVegitaion ();
//									} else {
//										Waiting ();
//									}
//								}
//
//							} else {


						}

					} else {
//					no sheep nearby
						Explore (0.02f);
					}
				
				} else {
					//busy travelling
					MoveToInputAllTheWay ();

				}
			}
			elapsed += Time.deltaTime;
			float waitTime = 1f;//15 min
			if (elapsed >= waitTime) {
				elapsed = elapsed % waitTime;
				//call here
				EnergyConverstion ();
				sun = gameControllerObject.sunlight;//update the sunlight


				if (!traveling) {
					
					if (!isLeader) {
						AssignLeader ();
					}
						
					Sleep ();
				} else {
					MoveToInputAllTheWay ();
				}
			}
			
			elapsed1 += Time.deltaTime;
			float waitTime1 = 8f;//2 hour
			if (elapsed1 >= waitTime1) {
				elapsed1 = elapsed1 % waitTime1;

				WeightUpdate ();
				Breeding ();
				Reproduce ();

//				if (drinksMilk > 0) {
//					milkLeftToDrink = drinksMilk;
//				}

			}
			elapsed2 += Time.deltaTime;
			float waitTime2 = 96f;//1 day
			if (elapsed2 >= waitTime2) {
				elapsed2 = elapsed2 % waitTime2;

				UpdateSpriteSizeAndCircleCollider ();
				//reset daily sleeping 6 hours
				timeLeftToSleep = 360f;
				//increase age
				ageInYears = ageInYears + (1 / 365);
				if (ageInYears > 10 + (3 * (energy / 1000))) {
					//kill this sheep of old age between 10 13 years depending on health(energy) i.e. extreme wheather
					Kill ();
				}

				//UpdateMilkConsumption ();

			}

		} else {
			
			elapsed += Time.deltaTime;
			float waitTime = 4f * 24f;//24 hour
			//show decaying
			if (elapsed >= waitTime) {
				elapsed = elapsed % waitTime;
				//call methodes here
				Destroy(rb);
			}

		}
		

	}


	private void FightLeader ()
	{
		if (leaderSheepInVision.Count > 0) {//look for other leaders, if so must compete for dominance

			float tR = Random.Range (0f, 1f);


			contestingRam = FindClosestTile (leaderSheepInVision);//get the cosest ram

			float contestingRamsDistance = Vector2.Distance (rb.position, contestingRam.position);
			float chance = (contestingRamsDistance / (maxVision * leaderVisionScale));//will heighten chances of fighting the closer the Ram gets

			if (tR > chance) {
				fighting = true;
				//Vector2 midWay = Vector2.(rb.position,contestingRam.position)/2f;
				//destination = midWay;
				destination = contestingRam.position;
				MoveToInputAllTheWay ();
			}


		} 

	}

	private void Waiting ()
	{
		randomVelocity = new Vector2 (Random.Range (-1f, 1f), Random.Range (-1f, 1f));
		MoveToInput (rb.position + randomVelocity, 0.025f);
		sr.sprite = stand;
	}



	public int steps = 0;
	public int maxSteps = 20;
	private Vector2 explorationDestination;

	private void Explore (float speed)
	{
		
		//looks around map for leader
		steps++;

		if (steps > maxSteps) {//new direction
			bool directionChosen = false;
			float bufferSpace = 1f;//not going to corners

			float maxDistance = Mathf.Max (boundary.xMax - boundary.xMin, boundary.yMax - boundary.yMin);

			int rN2 = Random.Range (0, 5);//all 5 options
			//cant ever pick closest location
			float rN = Random.Range (0f, 1f);
			Vector2 destinationBeingAssest = new Vector2 (0, 0);
			if (rN2 == 0) {
				if (rN < Vector2.Distance (rb.position, destinationBeingAssest) / maxDistance) {
					//middle
					explorationDestination = destinationBeingAssest;
					directionChosen = true;
				}
			} else if (rN2 == 1) {
				destinationBeingAssest = new Vector2 (boundary.xMin + bufferSpace, boundary.yMax - bufferSpace);
				if (rN < Vector2.Distance (rb.position, destinationBeingAssest) / maxDistance) {
					//Tl
					explorationDestination = destinationBeingAssest;
					directionChosen = true;
				}
			} else if (rN2 == 2) {
				destinationBeingAssest = new Vector2 (boundary.xMax - bufferSpace, boundary.yMax - bufferSpace);
				if (rN < Vector2.Distance (rb.position, destinationBeingAssest) / maxDistance) {
					//TR
					explorationDestination = destinationBeingAssest;
					directionChosen = true;
				}

			} else if (rN2 == 3) {
				destinationBeingAssest = new Vector2 (boundary.xMin + bufferSpace, boundary.yMin + bufferSpace);
				if (rN < Vector2.Distance (rb.position, destinationBeingAssest) / maxDistance) {
					//BL
					explorationDestination = destinationBeingAssest;
					directionChosen = true;
				}
			} else if (rN2 == 4) {
				destinationBeingAssest = new Vector2 (boundary.xMax - bufferSpace, boundary.yMin + bufferSpace);
				if (rN < Vector2.Distance (rb.position, destinationBeingAssest) / maxDistance) {
					//BR
					explorationDestination = destinationBeingAssest;
					directionChosen = true;
				}
			}


			if (directionChosen) {
				steps = 0;

				maxSteps = Random.Range (8, Mathf.RoundToInt (Vector2.Distance (rb.position, destinationBeingAssest) * 40f));
			}

		}

		MoveToInput (explorationDestination, speed);



	}

	//	private void UpdateMilkConsumption ()
	//	{
	//		if (ageInYears > 0.13f) {
	//			drinksMilk = 0;
	//		} else if (ageInYears > 0.11f) {//end of 5th week
	//			drinksMilk = 1;
	//		} else if (ageInYears > 0.086f) {//4-5week
	//			drinksMilk = 2;
	//		} else if (ageInYears > 0.067f) {//3-4week
	//			drinksMilk = 3;
	//		} else if (ageInYears > 0.04f) {//first couple weeks
	//			drinksMilk = 4;
	//		}
	//
	//		//once a day
	//	}

	//	private bool FollowParent (float distance)//1f = 15min in game
	//	{
	//		float distanceFromParent = Vector2.Distance (rb.position, mom.position);
	//		if (distanceFromParent > distance) {
	//			destination = mom.position;
	//			MoveToInputAllTheWay ();
	//			return true;
	//
	//		}
	//
	//		return false;
	//
	//
	//	}

	//	private void DrinkMilk ()
	//	{
	//		destination = mom.position;
	//		MoveToInputAllTheWay ();//this pauses the 1 min loop, until destination reached
	//	}


	private void UpdateSpriteSizeAndCircleCollider ()
	{//called every day
		//make sprite smaller
		float size = (grownPercentage / 2) + 0.5f;

		SpriteTransform.localScale = new Vector3 (spriteOriginalScale.x * size, spriteOriginalScale.y * size, SpriteTransform.localScale.z);

		//sheepVisionCollider.radius = maxVision * (spriteOriginalScale.x-(size*spriteOriginalScale.x));
		if (isLeader) {
			sheepVisionCollider.radius = leaderVisionScale * maxVision * (1 + (1f - (SpriteTransform.localScale.x / spriteOriginalScale.x)));
		} else {
			
			sheepVisionCollider.radius = maxVision * (1 + (1f - (SpriteTransform.localScale.x / spriteOriginalScale.x)));
		}
		//fix radious change

		//change the push distance
	}

	private void Sleep ()
	{
		//cant be sun to sleep
		//if (sun == 0f) {
		if (!readyToSleep && full && timeLeftToSleep > 0f) {

			readyToSleep = true;

		}
		if (sheepReadyToSleepNearby > sheepInVision.Count - sheepInVision.Count / 2 && !sleeping && timeLeftToSleep > 0f) {
			readyToSleep = true;
			//timeLeftToSleep = 240f;
			traveling = false;
			sleeping = true;
			sr.sprite = sleep;
		}
		//}
	}

	private void AssignLeader ()
	{
		if (isRam) {
			if (leaderSheepInVision.Count == 0) {//no other leader sheep in range

				if (Random.Range (0f, 100f) < (0.05 + (grownPercentage))) {
					MakeLeader (true);

				}
			}
		}
	}



	private void MakeLeader (bool t)
	{
		
			
		if (!isLeader && t) {
			isLeader = true;
			sr.color = Color.gray;
			rb.mass = 10f;
			//sheepVisionCollider.radius = maxVision * leaderVisionScale;//leader sheep can see further
			repelDistance = repelDistance - 0.3f;
			UpdateSpriteSizeAndCircleCollider ();

		} else if (isLeader && !t) {
			isLeader = false;
			sr.color = Color.white;
			//sheepVisionCollider.radius = maxVision;
			rb.mass = 0.25f;
			repelDistance = repelDistance + 0.3f;
			UpdateSpriteSizeAndCircleCollider ();
		}


	}

	private void FollowLeader (float speed)
	{
		Vector2 tempDestination = new Vector2 ();
		bool gotDestination = false;
		foreach (Rigidbody2D element in leaderSheepInVision) {

			if (!element.GetComponent<sheepController> ().fighting && Vector2.Distance (rb.position, element.position) > 1) {
				tempDestination = element.position;
				gotDestination = true;
			}
				
			break;

		}

		if (gotDestination) {
			destination = tempDestination;
			MoveToInput (destination, speed);//move to sheep leader 
		}

	}
		
	//Priorites - levels can change if urgent? i.e. starving
	//1.run away from predator
	//2.move away from other sheep
	//if not busy choose from following
	//3.stick with flock/sheep leader
	//4 sleep/rest
	//5 eat
	//6 mate


	private void EnergyConverstion ()//turns stomach into energy and other data points
	{
		//every 15min
		float foodRequirementEveryFifteenMin = (weight * 1000f * 0.025f) / 96f;


		if (stomach > 0 && energy < 1000) {
			energy = energy + foodRequirementEveryFifteenMin;
			stomach = stomach - foodRequirementEveryFifteenMin;
		}
		if (stomach < (weight * 1000f * 0.025f) * 0.75f) {//less thank half full
			full = false;
		} 

		if (energy < 50 && (maxWeightReached - weight) < (maxWeightReached / 4)) { //can only be starving for so long(30kg*how large sheep is determines how much weight it can loose
			
			energy = energy + foodRequirementEveryFifteenMin * 0.375f;//check this value
			starvationWeightLoss = starvationWeightLoss + 0.005f;//(half kg per day if starving all day)

		}


		if (energy < 0) {
			Kill ();
		}

		energy = energy - foodRequirementEveryFifteenMin * 0.375f;//9 hours, needs to eat third of the time unless extra energy is needed

			
	}

	private void WeightUpdate ()
	{
		//called every 2 hours
		//convert energy into weight
		if (starvationWeightLoss <= 0) {//cant be starving
			//highest is 0.032 for growth 46kg/(120*12)
			grownPercentage = weight / maxWeight;

			if (grownPercentage < 0.5f) {
				//weight = weight + 0.01f;//46kg/(365*12)
				weight = weight + 0.01f + (0.022f * (energy / 1000f));
			} else {
				//grow more slowly for adults until maxed reached
				weight = weight + 0.01f * (1f - grownPercentage);
			}
			//update the largest the sheep has ever gotten
			if (weight > maxWeightReached) {
				maxWeightReached = weight;
			}
		} else {
			

			weight = weight - starvationWeightLoss;
			starvationWeightLoss = 0f;

		}


	}




	private void Kill ()
	{
		sr.sprite = die;
		MakeLeader (false);
		dead = true;
		Destroy (sheepVisionCollider);
		Destroy (rb.GetComponent<BoxCollider2D> ());
		gameControllerObject.SheepDied ();

	}

	private void MoveToInput (Vector2 destination, float speed)
	{
		//rb.position = Vector2.MoveTowards (rb.position, averagePosOfSheep, Time.deltaTime);//move sheep towards sheep center of mass

		//rb.MovePosition(Vector2.MoveTowards (rb.position, averagePosOfSheep, 15*Time.deltaTime));
		//rb.position =Vector2.MoveTowards (rb.position, averagePosOfSheep, Time.deltaTime);
		if (destination != null) {
			movePosition = Vector2.MoveTowards (rb.position, destination, speed);

			rb.position = movePosition;
		}

	}

	private void MoveToInputAllTheWay ()
	{
		if (contestingRam == null) {
			contestingRam = rb;
		}

		//like MoveToInput but all the way
		float minDistance = 0.4f;

		traveling = true;
		if (fighting) {//handles when lamb collides

			 

			float d = Vector2.Distance (rb.position, contestingRam.position);

			if (d < 1f) {//collision of Rams
					

				Vector2 direction = rb.position - contestingRam.position;
				direction.Normalize ();
				movePosition = Vector2.MoveTowards (rb.position, direction * -1, 0.1f);//bump from lamb
				movePosition = Vector2.MoveTowards (rb.position, contestingRam.position, 0.1f);
				rb.position = movePosition;
			} else {
				movePosition = Vector2.MoveTowards (rb.position, contestingRam.position, 0.1f);

				rb.position = movePosition;
			}
			
			if (leaderSheepInVision.Count == 0) {
				traveling = false;
				fighting = false;
			} else if (!contestingRam.GetComponent<sheepController> ().isLeader) {
				traveling = false;
				fighting = false;

			}

		} else {
			if (Vector2.Distance (destination, rb.position) < minDistance || (sheepReadyToSleepNearby > (sheepInVision.Count * 0.5f))) {
				traveling = false;

			} else {

				movePosition = Vector2.MoveTowards (rb.position, destination, 0.05f);

				rb.position = movePosition;
			}
		}




	}

	private void EatingVegitaion ()
	{
		//called every 1 min

		randomVelocity = new Vector2 (Random.Range (-1f, 1f), Random.Range (-1f, 1f));
		MoveToInput (rb.position + randomVelocity, 0.025f);
		//rb.AddForce (randomVelocity * 5f);


		//float eatingRate = weight / maxWeight * 5f;//grams per minute of ingame
		float eatingRate = (weight * 0.025f * 1000f) / (60 * 9);

//		velocity = new Vector2 (Random.Range (-1, 2), Random.Range (-1, 2));
//		velocity = velocity ;
		//rb.AddForce (velocity * Time.deltaTime*10f);
		if (stomach > 0.025 * weight * 1000) {//eats 2.5%
			full = true;
		} 

		if (stomach < 0.03 * weight * 1000) {//max3%
			

			groundUnderAnimal.GetComponent<GrassController> ().Gazed (eatingRate / 5);
			stomach = stomach + eatingRate;
			if (sr.sprite == eat1) {
				sr.sprite = eat;
			} else {
				sr.sprite = eat1;
			}

		}

//		//baby sheep drinking milk
//		if (drinksMilk > 0) {
//			float momsEnergy = mom.gameObject.GetComponent<sheepController> ().energy;
//			if (momsEnergy > 100) {
//				stomach = stomach + drinksMilk;
//				mom.gameObject.GetComponent<sheepController> ().energy = momsEnergy - drinksMilk;
//			}
//		}
	

	}






	//ruturn postition of center of mass
	//To DO: weigh the importance of each sheep according their weight(follow elders)
	private Vector2 AveragePositionSheep (List<Rigidbody2D> sheep)
	{


		float x = 0f;
		float y = 0f;
		float xB = 0f;
		float yB = 0f;
		float counter = 0;
		float counterB = 0;
		foreach (Rigidbody2D rb in sheep) {
			if (!rb.GetComponent<sheepController> ().notLamb) {//take baby sheep in consideration
				xB += rb.position.x;
				yB += rb.position.y;
				counterB++;
			} else {
				x += rb.position.x;
				y += rb.position.y;
				counter++;
			}

		}
//		//makes baby sHeep worth half as much
//		if (counterB >= 4f) {
//			xB = xB / (4f);
//			yB = yB / (4f);
//			counterB = counterB / 4f;
//
//			x = x + xB;
//			y = y + yB;
//			counter = counter + counterB;
//		}

		x = x / counter;
		y = y / counter;
		return new Vector2 (x, y);

	}

	private void Breeding ()
	{
		//make fertile when reached 45% of Maximum weight
		if (!pregnant && !fertile && energy > 200) {
			if (0.45f <= weight / maxWeight) {
				fertile = true;		
				if (!notLamb) {
					notLamb = true;
				}
			}
		}

	}

	private void Reproduce ()//try reproduce
	{
		//call every 2 hours
		if (fertile) {
			Rigidbody2D mate = GetClosestSheepMate (2f);

			if (mate != null) {//found a mate
				if (!isRam) {//female
					gameControllerObject.PregnantSheep ();
					pregnant = true;
					coroutine = Pregnant (8.0f);//2 hours in game
					StartCoroutine (coroutine);
				} else {//male
					energy = energy - 50;//energy used
					fertile = false;//need a short break
				}

			}
		}
	}


	private IEnumerator Pregnant (float waitTime)
	{//every 2 hours inGame
		fertile = false;
		while (pregnant) {
			yield return new WaitForSeconds (waitTime);
			energy = energy - 3;
			hoursPregnant = hoursPregnant + waitTime / 4f;
			if (hoursPregnant >= (hoursNeededPregnant)) {
				pregnant = false;
				Birth ();
				StopCoroutine (coroutine);
			} else if (hoursPregnant >= (hoursNeededPregnant - 30 * 24)) {
				//requires more energy on last month
				energy = energy - 5;
			}
		}
	}

	private void Birth ()
	{
		//to do: work out the healthyness of sheep over the pregnancy
		sheepController tsheepController = sheepBaby.GetComponent<sheepController> ();
		int counter = 1;
		tsheepController.notLamb = false;
		//tsheepController.mom = rb;
		float t = Random.Range (0f, 1f);
		float t2 = Random.Range (0.5f, 1f);//cant be underweight 
		Instantiate (sheepBaby, (Vector3)rb.position + new Vector3 (0, 2, -1), Quaternion.identity);



		if (grownPercentage > t2 && 0.60f < (energy / 1000f)) {//does random chance, the heavier the better chance of twins and cant be too tierd

			Instantiate (sheepBaby, (Vector3)rb.position + new Vector3 (2, 0, -1), Quaternion.identity);
			counter++;


			if (ageInYears > 3f && ageInYears < 6f) {//twins if correct age
				if (t > 0.8f) {
					Instantiate (sheepBaby, (Vector3)rb.position + new Vector3 (0, -2, -1), Quaternion.identity);
					counter++;
				}

			}

		}


		weight = weight - 3 * counter;
		energy = energy - 50 * counter;
		gameControllerObject.SheepBorn ();
	}

	Rigidbody2D FindClosestTile (List<Rigidbody2D> tiles)
	{
		Rigidbody2D tMin = null;
		float minDist = Mathf.Infinity;
		Vector2 currentPos = this.transform.position;
		foreach (Rigidbody2D t in tiles) {

			float dist = Vector2.Distance (t.position, currentPos);
			if (dist < minDist) {
				tMin = t;
				minDist = dist;
			}
		}
		return tMin;
	}

	//entering sheeps awerness
	void OnTriggerEnter2D (Collider2D collision)
	{
		if (collision.GetType () == typeof(BoxCollider2D)) {//vision
			if (collision.tag == "Sheep") {
				if (collision.GetComponent<sheepController> ().isLeader) {
					leaderSheepInVision.Add (collision.attachedRigidbody);//also adds leaders
				}

				sheepInVision.Add (collision.attachedRigidbody);
			} else if (collision.tag == "Wolf") {
				predatorInVision.Add (collision.attachedRigidbody);
			} else if (collision.tag == "tile") {
				tilesInVision.Add (collision.attachedRigidbody);
			} 


		} 

	}
	//leaving sheeps awerness
	void OnTriggerExit2D (Collider2D collision)
	{
		if (collision.GetType () == typeof(BoxCollider2D)) {//vision
			if (collision.tag == "Sheep") {
				if (collision.GetComponent<sheepController> ().isLeader) {
					leaderSheepInVision.Remove (collision.attachedRigidbody);//also adds leaders
				}
				sheepInVision.Remove (collision.attachedRigidbody);

			} else if (collision.tag == "Wolf") {
				predatorInVision.Remove (collision.attachedRigidbody);
			} else if (collision.tag == "tile") {
				tilesInVision.Remove (collision.attachedRigidbody);
			} 

		} 

	}

	void OnCollisionEnter2D (Collision2D collision)
	{
		if (collision.collider.tag == "Sheep") {
			if (collision.collider.GetComponent<sheepController> ().isLeader && traveling) {
				traveling = false;
				if (collision.collider.GetComponent<sheepController> ().weight > weight) {//if other Ram weighs less when they collid, dont make it a leader
					
					MakeLeader (false);	

					//makes sure all Rams in the list are still leaders, other wise remove it
					//go to all other sheep in vison, and remove this Ram from their leader list

				}
				fighting = false;
			}
		}
	}

	private void UpdateLeadersAndSleepersList ()
	{
		//called every 15min
		List<Rigidbody2D> t = new List<Rigidbody2D> ();
		int sheepSleepingNearby = 0;
		foreach (var item in sheepInVision) {
			if (item.GetComponent<sheepController> ().isLeader) {
				t.Add (item);
			}
			if (item.GetComponent<sheepController> ().readyToSleep) {
				sheepSleepingNearby++;
			}
		}
		leaderSheepInVision.Clear ();
		leaderSheepInVision = t;
		sheepReadyToSleepNearby = sheepSleepingNearby;

	}
	//fix me
	private Rigidbody2D GetClosestSheep (float minDistance)
	{
		
		Rigidbody2D closestSheep = null;
		float min = minDistance;
		foreach (var item in sheepInVision) {
			
			float t = Vector2.Distance (rb.position, item.position);
//			if ((!item.GetComponent<sheepController> ().notLamb)) {
//				
//				//lamb
//
//				t = t*2f;
//			}

				
			if (t < min) {
				min = t;
				closestSheep = item;
			}
				
			

		}
		return closestSheep;


	}

	private Rigidbody2D GetClosestSheepMate (float minDistance)
	{
		
		Rigidbody2D closestSheepMate = null;
		float min = minDistance;
		foreach (var item in sheepInVision) {
			float t = Vector2.Distance (rb.position, item.position);
			sheepController mateSheepController = item.GetComponent<sheepController> ();
			if (t < min && mateSheepController.fertile && (mateSheepController.isRam == !isRam)) {
				min = t;
				closestSheepMate = item;
			}

		}
		return closestSheepMate;


	}

	private Vector2 NewLands (List<Rigidbody2D> qArray, float radius, float stayingPower)
	{
		//make better
		//sort list of tiles according to dinstance from sheep
		//then work out the average attractiveness of tiles, within the distance
		Vector2 destination = new Vector2 ();
		float bestdestinationAttraction = 0f;
		float points = 0f;
		int counter = 0;

		int bufferSpace = 2;

		foreach (Rigidbody2D tile in qArray) {//loops through all vegitation tiles within sheep vision
			Collider2D[] hitColliders = Physics2D.OverlapCircleAll (tile.position, radius);
			points = 0f;
			counter = 0;
			foreach (Collider2D element in hitColliders) {//loops through the surrounding tiles of tile being assest
				if (element.tag == "tile") {
					float attractivness = element.GetComponent<GrassController> ().attractiveness;
					Vector2 tempPos = element.transform.position;
					if (tempPos.x < boundary.xMin + bufferSpace || tempPos.x > boundary.xMax - bufferSpace || tempPos.y < boundary.yMin + bufferSpace || tempPos.y > boundary.yMax - bufferSpace) {
						//a side block makes the total surrounding block less valuable
						attractivness = 0;
					} 
					points = points + attractivness;
					counter++;
				}
			}
			points = points / hitColliders.Length;

			if (bestdestinationAttraction < points) {

				bestdestinationAttraction = points;
				destination = tile.position;
			}
		}

		//checks the current position attractivness
		Collider2D[] hitColliders1 = Physics2D.OverlapCircleAll (groundUnderAnimal.transform.position, radius);
		points = 0f;
		counter = 0;
		foreach (Collider2D element in hitColliders1) {

			if (element.tag == "tile") {

				float attractivness = element.GetComponent<GrassController> ().attractiveness;
				Vector2 tempPos = element.transform.position;
				if (tempPos.x < boundary.xMin + bufferSpace || tempPos.x > boundary.xMax - bufferSpace || tempPos.y < boundary.yMin + bufferSpace || tempPos.y > boundary.yMax - bufferSpace) {
					attractivness = 0;
				}

				points = points + attractivness;
				counter++;
			}

		}
		points = points / hitColliders1.Length;


//		if (bestdestinationAttraction < points) {//so sheep doesnt look for new land right away
//			return rb.position;
//		}
		if (bestdestinationAttraction < points * (1 + stayingPower)) {//so sheep doesnt look for new land right away
			return rb.position;

		}


		return destination;
	}




}


