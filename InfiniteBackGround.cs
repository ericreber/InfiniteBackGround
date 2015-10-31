using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace Com.EricReber.InfiniteBackGround
{
	public class InfiniteBackGround : MonoBehaviour{
		[Tooltip("Camera to used to display this background")]
		public Camera parallaxCamera;
		
		[Tooltip("The elements in the parallax.")]
		public List<GameObject> elements;
		
		[Tooltip("Vertical offset used when positionning the backGround")]
		public float verticalOffset = 0.0f;

		//All the elementsThatCan be displayed on the backGround;
		private List<BackGroundElement> elementsOnScreen = new List<BackGroundElement>();

		private List<PrefabPool> prefabsPool = new List<PrefabPool>();

		//The index of the first (left) element displayed on the screen -1 mean no element on screen
		private int firstElementIndex = -1;

		//The index of the last (right) element displayed on the screen -1 mean no element on screen
		private int lastElementIndex = -1;

		//Retrieve the Camera boundaries in the World Cordinate system using the view port 
		//(Viewport bottom left = 0,0 top right = 1,1
		private float cameraLeftBound{
			get{ return parallaxCamera.ViewportToWorldPoint(Vector3.zero).x;}
		}
		private float cameraRightBound{
			get{ return parallaxCamera.ViewportToWorldPoint(Vector3.right).x;}
		}
		
		//Store the rightest postion displaying a backGround
		private float worldRightBound;

		void Start(){
			//When we start, I add one copy of each prefab in the PrefabPool
			foreach (GameObject obj in elements)
			{
				PrefabPool p = new PrefabPool();
				p.SetPrefab(obj);
				prefabsPool.Add(p);
			}
			fillUpTheViewPort();	
		}
		
		//This methode is used to add the prefab to the scene so it takes all the view port
		private void fillUpTheViewPort(){
			worldRightBound = cameraLeftBound;
			
			while (worldRightBound<cameraRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = instantiateRandomPrefab(initialPosition);
			}

			//When we fillUp the screen for the first time the first element is alway at index 0
			if(firstElementIndex == -1){
				firstElementIndex = 0;
			}

			if(lastElementIndex == -1){
				lastElementIndex = elementsOnScreen.Count;
			}

		}
		
		//Instantiate a Random Prefab and position it at initialPosition return the x coordinate for the right bound
		private float instantiateRandomPrefab(Vector3 initialPosition){
			int randomIndex = 0;
			if(prefabsPool.Count==0){
				Debug.LogError("You need at least one prefab in the elements list.");
			}else if(prefabsPool.Count>1){
				randomIndex = Mathf.Abs(Random.Range(0,prefabsPool.Count));
			}

			//The prefab That will be rendered
			GameObject renderedPrefab = prefabsPool[randomIndex].GerAvailableGameObject();
			//We'll use the renderer to get the refab's width
			float prefabWidth = renderedPrefab.GetComponent<Renderer>().bounds.size.x;
			//We are placing the obect based on his center so we /2.0f the width
			Vector3 instantiatedPosition = new Vector3((initialPosition.x+prefabWidth/2.0f)-renderedPrefab.transform.position.x,initialPosition.y-renderedPrefab.transform.position.y);
			renderedPrefab.transform.Translate(instantiatedPosition,Space.World);
			renderedPrefab.SetActive(true);
			//We add it to the element on screen
			BackGroundElement bgElem = new BackGroundElement();
			bgElem.gameObject = renderedPrefab;
			bgElem.width = prefabWidth;
			//We want the right bound, not the center, that's why we don't divide by 2
			bgElem.rightBound = initialPosition.x+prefabWidth;
			elementsOnScreen.Add(bgElem);

			return bgElem.rightBound;
		}
		
		void Update()
		{
			//If the camera is rendering something without a background we add one
			if(cameraRightBound>worldRightBound){
				Vector3 initialPosition = new Vector3(worldRightBound,verticalOffset);
				worldRightBound = instantiateRandomPrefab(initialPosition);
				
			}

			//If we have element that are not displayed by the camera any more we disable them
			if(elementsOnScreen[firstElementIndex].rightBound<cameraLeftBound){
				elementsOnScreen[firstElementIndex].gameObject.SetActive(false);
				if(firstElementIndex<elementsOnScreen.Count){
					firstElementIndex++;
				}else{
					firstElementIndex = 0;
				}

			}
			//On check si tout le view port est couvert par des gameObject
			//Si c'est pas le cas on ajoute ce qui est nécessaire pour le couvrir
			//On commence par regarder si on a qqch sur la bordure droite
			//Pour chaque elem on regarde si xl est >= pl et xl <= pr
			//Si on trouve qqch on place la nouvelle borne de verif xl' sur pr de l'element trouvé
			
			
			//On regarde si il y a des gameObject qui ne sont plus dans le viewport de la camera.
			//Si c'est le cas on les désactives
			
			
			
			
			//Vector3 bg = clone.transform.position;
			//Renderer r = clone.GetComponent<Renderer>();
			//float objectLeftBound = bg.x-r.bounds.size.x/2.0f;
			//float objectRightBound = bg.x+r.bounds.size.x/2.0f;
			
			//if(worldLeftBound>objectRightBound){
			//	clone.SetActive(false);
			//}else{
			//	clone.SetActive(true);
			//}
			
			//clone.transform.Translate(backGround.transform.position.x,backGround.transform.position.y,backGround.transform.position.z);
		}
	}

	//
	public class BackGroundElement{
		public GameObject gameObject;
		public float width;
		public float rightBound;
	}

	public class PrefabPool{
		private GameObject prefab;
		private List<GameObject> gameObjectPool = new List<GameObject>();

		public void SetPrefab(GameObject gameObject){
			prefab = gameObject;

		}

		public GameObject GerAvailableGameObject(){
			GameObject gameObject =null;
			foreach (GameObject obj in gameObjectPool)
			{
				if(obj.activeSelf==false){
					gameObject = obj;
					break;
				}
			}

			//if we don't have any available gameObject we need to instansiate a new one
			if(gameObject==null){
				gameObject = GameObject.Instantiate(prefab, Vector3.zero, Quaternion.identity) as GameObject;
				gameObjectPool.Add(gameObject);
			}
			return gameObject;
		}
	}
}
