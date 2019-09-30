using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEditor;


[System.Serializable]
public class GameController : MonoBehaviour
{
    // Start is called before the first frame update
    [Range(2,100)]
    public int xGridSize = 8;
    [Range(2, 100)]
    public int yGridSize = 9;


    public List<Color> GemColors;
   
    public GameObject GemPrefab;
    
    public float normalXOffset = 1.5f;
    public float normalYOffset = 1.5f;
    public float oddLineOffset = 0.5f;

    public Camera MainCamera;

    public GameObject[,] gemArray;
    private List<GameObject> matchedGems;
    private bool isThereMatchedGems = false;
    public UnityEvent goOrder;
    public UnityEvent bombTick;

    private bool isThereNull = false;
    private bool gemRotation;
    public GameObject BackTouchPlate;
    public GameObject[] selectedGems;
    public GameObject[] closeGems;
    public GameObject[] selectedCloseGems;
    public GameObject centerGem;
    public GameObject farAwayObject;
    public bool playerCanMove = false;
    public int score;
    public int scorePerGem = 5;
    public TextMeshProUGUI scoreDisp;
    public int moveCount;
    public TextMeshProUGUI moveCountDisp;
    public int bombThreshhold = 1000;
    public int bombCount = 0;
    public int bombTimerStart = 9;
    public bool isBombExploded = false;
    public TextMeshProUGUI gameOverScore;
    private int emptySpots = 0;
    private int loadStart = 20;
    public Slider loadingSlider;
    public GameObject LoadingScreen;
    public GameObject GameOverMenu;
    public bool isGameOver;










    void Start() 
    {
       /* selectedGems = new GameObject[3];
        closeGems = new GameObject[6];
        GemStart();
        SetCamPosition();*/
        playerCanMove = false;
        
    }

   
    public void GameOver()// Gameover sequence
    {
        isGameOver = true;
        playerCanMove = false;
        isBombExploded = false;
        GameOverMenu.SetActive(true);
        gameOverScore.text = "Game Over!"+"\n"+"Score:"+score.ToString();
        gameOverScore.gameObject.SetActive(true);
        clearGems();
        moveCount = 0;
        bombCount = 0;
        

    }
    private void clearGems()//clears the game field
    {
        GameObject[] findGems = GameObject.FindGameObjectsWithTag("gem");
        foreach (GameObject gem in findGems)
        {
            Destroy(gem);
        }
        score = 0;
        moveCount = 0;
    }
    public void GameStart()//start sequence
    {
        isGameOver = false;
        LoadingScreen.SetActive(true);
        bombCount = 0;
        gameOverScore.gameObject.SetActive(false);
        clearGems();
        // playerCanMove = true;
     
        selectedGems = new GameObject[3];
        closeGems = new GameObject[6];
        GemStart();
        SetCamPosition();
        score = 0;
        scoreDisp.text = "Score"+ "\n" + score.ToString();
        moveCountDisp.text = "Moves" + "\n" + moveCount.ToString(); ;
        if(!isThereAnyMove())
        {
            GameStart();
        }
      

    }
    private void SetCamPosition()//Setting camera position which depends on the grid size
    {
        Vector3 rightUpPoint = new Vector3(0, 0, 0);

        int upX = gemArray[xGridSize-1, yGridSize-1].GetComponent<GemProps>().gemIdx;
        int upY = gemArray[xGridSize-1, yGridSize-1].GetComponent<GemProps>().gemIdy;

        if (xGridSize % 2 == 1)
        {
            rightUpPoint = new Vector3((upX * normalXOffset), (upY * normalYOffset) + oddLineOffset, 0);
        }
        else
        {
            rightUpPoint = new Vector3((upX * normalXOffset), (upY * normalYOffset), 0);
        }
      
        float width = (rightUpPoint.x+6);
        float height = (rightUpPoint.y);
        BackTouchPlate.transform.localScale = new Vector3(width*3, height*3, 1);
        BackTouchPlate.transform.position = new Vector3(0, 0, 0.5f);
        float cameraDistance = 10f;

        float cameraDistanceForWidth  = (3 * (width/2));
        float cameraDistanceForHeigth = (height/2)*2.3f;

        if (cameraDistanceForHeigth > cameraDistanceForWidth)
        {
            cameraDistance = cameraDistanceForHeigth;
        }
        else { cameraDistance = cameraDistanceForWidth; }
      
        MainCamera.transform.position = new Vector3(rightUpPoint.x / 2, (rightUpPoint.y / 2)+(yGridSize/4), -cameraDistance);

    } 
    void GemStart()//Setting the grid
    {
        gemArray = new GameObject[xGridSize, yGridSize];

        

        for (int x = 0; x < xGridSize; x++)
        {
            
            for (int y = 0; y < yGridSize; y++)
            {
            
                SpawnGem(x, y);  
                
            }
            
        }
        
        goOrder.Invoke();
        
        StartCoroutine(gemDestroyWait());
    }
    public void SpawnGem(int x, int y)// spawn gems on passed grid
    {
        GameObject Gem_GO = (GameObject)Instantiate(GemPrefab, new Vector3(x* normalXOffset, yGridSize*normalYOffset+10 , 0), Quaternion.identity);

        Gem_GO.transform.SetParent(this.transform);
        Gem_GO.name = "Gem" + "x" + x + "y" + y;
        int colorIndex = Random.Range(0, GemColors.Count);
        Gem_GO.gameObject.transform.GetChild(0).gameObject.GetComponent<Renderer>().material.color = GemColors[colorIndex];
        Gem_GO.gameObject.GetComponent<GemProps>().gemColor = colorIndex;
        gemArray[x, y] = Gem_GO;
        Gem_GO.GetComponent<GemProps>().gemIdx = x;
        Gem_GO.GetComponent<GemProps>().gemIdy = y;
        Gem_GO.GetComponent<GemProps>().gameControl = this.gameObject.GetComponent<GameController>(); ;
        Gem_GO.GetComponent<GemProps>().normalXOffset = normalXOffset;
        Gem_GO.GetComponent<GemProps>().normalYOffset = normalYOffset;
        Gem_GO.GetComponent<GemProps>().oddLineOffset = oddLineOffset;
        if ((score / bombThreshhold) > bombCount)
        {
            Gem_GO.GetComponent<GemProps>().isBomb = true;
            Gem_GO.GetComponent<GemProps>().bombSpawnTurn = moveCount;
            Gem_GO.GetComponent<GemProps>().bombTimerStart = bombTimerStart;
            bombTick.AddListener(Gem_GO.GetComponent<GemProps>().tickTheBomb);
            bombCount++;
        }
        
        goOrder.AddListener(Gem_GO.GetComponent<GemProps>().goToSpot);
    } 
    IEnumerator gemDestroyWait() //destroy gems with some delay
    {     
        yield return new WaitForSeconds(0.4f);

        if (!isGameOver)
        {
            getMatched();
            DestroyMatchedGems();
            GetEmptySpot();
        }
      

    } 

    public void DestroyMatchedGems()//destroy all marked gems
    {
        
        for (int x = 0; x < gemArray.GetLength(0); x++)
        {
            for (int y = 0; y < gemArray.GetLength(1); y++)
            {
                if (gemArray[x, y] != farAwayObject && gemArray[x, y].GetComponent<GemProps>().isMatched)
                {
             
                    Destroy(gemArray[x, y]);
                    if (moveCount > 0)
                        
                    {
                        score += scorePerGem;
                        scoreDisp.text = "Score" + "\n" + score.ToString();
                        moveCountDisp.text = "Moves" + "\n" + moveCount.ToString(); ;
                    }
                    gemArray[x, y] = farAwayObject;

                }
               
            }
        }
        GetEmptySpot();
        if (isThereNull)
        {
            StartCoroutine(gemDestroyWait());
          
            isThereNull = false;
         
        }
        else if (isThereMatchedGems)
        {
            StartCoroutine(gemDestroyWait());
            isThereMatchedGems = false;
           

        }
        else {
            playerCanMove = true;

            LoadingScreen.SetActive(false);
            checkExploded();


            if ( !isThereAnyMove() )
            {
                if(moveCount <= 0 && xGridSize > 1 && yGridSize > 1)
                { 

                    GameStart();
                }
                else
                {
                    transform.GetComponent<exitController>().GameOver(true);
                }

                
            }

           
        }
        if(moveCount < 1)
        {
            loadingBar();
        }
       
       


    } 
    public void GetEmptySpot()//mark empty spots on grid and fill if there is an empty spot on top
    {
        emptySpots = 0;
        for (int x = 0; x < gemArray.GetLength(0); x++)
        {
            for (int y = 0; y < gemArray.GetLength(1); y++)
            {
                if (gemArray[x, y]== farAwayObject)
                {
                    emptySpots++;
                    isThereNull = true;
                    if(y==gemArray.GetLength(1)-1)
                    {
                        SpawnGem(x, y);
                        goOrder.Invoke();
                                  
                    }
                 
                }
            }
        }
        
    } 
    public void getMatched()//check all the grid for matching gems
    {
        


        for (int x = 0; x < gemArray.GetLength(0); x++)
        {
            for (int y = 0; y < gemArray.GetLength(1); y++)
            {
                if (gemArray[x, y] != farAwayObject)
                {
                    checkColors(gemArray[x, y], getCloseCells(gemArray[x, y]), getCloseColor(gemArray[x, y]));                                  
                }
            }
        }
 
    }

    public void checkColors(GameObject centerGem ,GameObject[] gemNeighbours, int[] neighbourColor) //checks if moved gems is a match
    {
     
        int homeColor = centerGem.GetComponent<GemProps>().gemColor;
        for (int i = 0; i < gemNeighbours.Length; i++)
        {
            if (neighbourColor[i] == homeColor)
            {
                if (neighbourColor[(i + 1) % 6] == homeColor)
                {
                    centerGem.GetComponent<GemProps>().isMatched = true;
                    gemNeighbours[i].GetComponent<GemProps>().isMatched = true;
                    gemNeighbours[(i + 1) % 6].GetComponent<GemProps>().isMatched = true;
                    isThereMatchedGems = true;
                    playerCanMove = false;
                
                }
            }
           
        }
      



    }
    
    public void sellectGem(Vector3 hitPoint) // select first gem
    {
        selectedGems = new GameObject[3];
       
        GameObject[] findGems = GameObject.FindGameObjectsWithTag("gem");

        returnBack();

        findGems = findGems.OrderBy(x => Vector3.Distance(x.transform.position, hitPoint)).ToArray();
        centerGem = findGems[0];
        getCloserTwo(centerGem, hitPoint);;
        selectedCloseGems = getCloseCells(centerGem);
    } 

    public void returnBack() //puts all gems back
    {
        for (int x = 0; x < xGridSize; x++)
        {

            for (int y = 0; y < yGridSize; y++)
            {
                if (gemArray[x, y] != farAwayObject)
                {
                    gemArray[x, y].GetComponent<GemProps>().toFront = false;
                    gemArray[x, y].GetComponent<GemProps>().goToSpot();
                }


            }

        }
    }
    public bool checkExploded()//checks if is there any bomb exploded, if there calls game over with cause of bomb explosion.
    {
        int explodedCount = 0;
        GameObject[] findGems = GameObject.FindGameObjectsWithTag("gem");
        foreach (GameObject gem in findGems)
        {
            if(gem.GetComponent<GemProps>().bombTimer <  1)
            {
                explodedCount++;
            }
        }
        if(explodedCount > 0)
        {
            isBombExploded = true;
            transform.GetComponent<exitController>().GameOver(true);

            playerCanMove = false;
            return true;
        }
        else
        {
            return false;
        }

    }


    public GameObject[] getCloseCells(GameObject firstSelected)// get first selected gems neighbours 
    {
       
        int firstX = firstSelected.GetComponent<GemProps>().gemIdx;
        int firstY = firstSelected.GetComponent<GemProps>().gemIdy;

        //closeGems = new GameObject[6];
        int[] neighbourColor = new int[6];
        if (firstX + 0 < gemArray.GetLength(0) && firstY + 1 < gemArray.GetLength(1))
        { closeGems[0] = gemArray[firstX + 0, firstY + 1]; }
        else { closeGems[0] = farAwayObject;
            neighbourColor[0] = -1;
            }

        if (firstX + 1 < gemArray.GetLength(0) && firstY + 0 + firstX % 2 < gemArray.GetLength(1))
        { closeGems[1] = gemArray[firstX + 1, firstY + 0 + firstX % 2]; }
        else { closeGems[1] = farAwayObject;
            neighbourColor[1] = -1;
        }

        if (firstX + 1 < gemArray.GetLength(0) && firstY - 1 + firstX % 2 < gemArray.GetLength(1)&& firstY - 1 + firstX % 2 > -1)
        { closeGems[2] = gemArray[firstX + 1, firstY - 1 + firstX % 2]; }
        else { closeGems[2] = farAwayObject;
            neighbourColor[2] = -1;
        }

        if (firstX + 0 < gemArray.GetLength(0) && firstY - 1 > -1 )
        { closeGems[3] = gemArray[firstX + 0, firstY - 1]; }
        else { closeGems[3] = farAwayObject;
            neighbourColor[3] = -1;
        }

        if (firstX - 1 > -1 && firstY - 1 + firstX % 2 < gemArray.GetLength(1)&& firstY - 1 + firstX % 2 > -1)
        { closeGems[4] = gemArray[firstX - 1, firstY - 1 + firstX % 2]; }
        else { closeGems[4] = farAwayObject;
            neighbourColor[4] = -1;
        }

        if (firstX - 1 > -1 && firstY + 0 + firstX % 2 < gemArray.GetLength(1))
        { closeGems[5] = gemArray[firstX - 1, firstY + 0 + firstX % 2]; }
        else { closeGems[5] = farAwayObject;
            neighbourColor[5] = -1;
        }

        return  closeGems;
      
      
    }
    public int[] getCloseColor(GameObject firstSelected)// gets neighbour gems  color 
    {
        //int firstX = firstSelected.GetComponent<GemProps>().gemIdx;
       // int firstY = firstSelected.GetComponent<GemProps>().gemIdy;
        GameObject[] closeCells = getCloseCells(firstSelected);
        //closeGems = new GameObject[6];
        int[] neighbourColor = new int[6];
        for (int i = 0; i < 6; i++)
        {
            neighbourColor[i] = closeCells[i].GetComponent<GemProps>().gemColor;
        }


        #region delete this after
        /*
        if (firstX + 0 < gemArray.GetLength(0) && firstY + 1 < gemArray.GetLength(1) && gemArray[firstX + 0, firstY + 1] != farAwayObject)
        {
            neighbourColor[0] = closeGems[0].GetComponent<GemProps>().gemColor;
        }
        else
        {
            
            neighbourColor[0] = -1;
        }

        if (firstX + 1 < gemArray.GetLength(0) && firstY + 0 + firstX % 2 < gemArray.GetLength(1) && gemArray[firstX + 1, firstY + 0 + firstX % 2] != farAwayObject)
        { 
            neighbourColor[1] = closeGems[1].GetComponent<GemProps>().gemColor;
        }
        else
        {
           
            neighbourColor[1] = -1;
        }

        if (firstX + 1 < gemArray.GetLength(0) && firstY - 1 + firstX % 2 < gemArray.GetLength(1) && firstY - 1 + firstX % 2 > -1 && gemArray[firstX + 1, firstY - 1 + firstX % 2] != farAwayObject)
        { 
            neighbourColor[2] = closeGems[2].GetComponent<GemProps>().gemColor;
        }
        else
        {
            
            neighbourColor[2] = -1;
        }

        if (firstX + 0 < gemArray.GetLength(0) && firstY - 1 > -1 && gemArray[firstX + 0, firstY - 1] != farAwayObject)
        { 
            neighbourColor[3] = closeGems[3].GetComponent<GemProps>().gemColor;
        }
        else
        {
            
            neighbourColor[3] = -1;
        }

        if (firstX - 1 > -1 && firstY - 1 + firstX % 2 < gemArray.GetLength(1) && firstY - 1 + firstX % 2 > -1 && gemArray[firstX - 1, firstY - 1 + firstX % 2] != farAwayObject)
        { 
            neighbourColor[4] = closeGems[4].GetComponent<GemProps>().gemColor;
        }
        else
        {
           
            neighbourColor[4] = -1;
        }

        if (firstX - 1 > -1 && firstY + 0 + firstX % 2 < gemArray.GetLength(1) && gemArray[firstX - 1, firstY + 0 + firstX % 2] != farAwayObject)
        { 
            neighbourColor[5] = closeGems[5].GetComponent<GemProps>().gemColor;
        }
        else
        {
            
            neighbourColor[5] = -1;
        }
        */
        #endregion
        return neighbourColor;


    }
    public void getCloserTwo(GameObject centerGem,Vector3 hitPoint)//find closest neighbours to touch point
    {
        GameObject[] closeCells = getCloseCells(centerGem);
        GameObject[] closestCell1 = closeCells.OrderBy(x => Vector3.Distance(x.transform.position, hitPoint)).ToArray();
   
        int secondGem = System.Array.IndexOf(closeCells, closestCell1[0]);
        GameObject[] closestCell2 = new GameObject[2];
        closestCell2[0] = closeCells[(secondGem + 1)%6];
        closestCell2[1] = closeCells[(secondGem + 5)%6];
        closestCell2 = closestCell2.OrderBy(x => Vector3.Distance(x.transform.position, hitPoint)).ToArray();
        selectedGems[0] = centerGem;
        selectedGems[1] = closestCell1[0];
        selectedGems[2] = closestCell2[0];
       
      
        if (((System.Array.IndexOf(closeGems, selectedGems[1]) + 5) % 6) == System.Array.IndexOf(closeGems, selectedGems[2]))// define selected gems rotation order
        {
            gemRotation = true;
        }
        else { gemRotation = false; }
            for (int i = 0; i < 3; i++)
        {
            if (selectedGems[i] != farAwayObject)
            {
                selectedGems[i].GetComponent<GemProps>().toFront = true;
                selectedGems[i].GetComponent<GemProps>().goToSpot();
            }
        }

    }

    public void tryAmove(bool isTurnLeft)//starts turn to gems
    {
        scoreDisp.text = "Score" + "\n" + score.ToString();
        moveCountDisp.text = "Moves" + "\n" + moveCount.ToString(); ;
        StartCoroutine(turnOrder(isTurnLeft));
        playerCanMove = false;
    }
    private IEnumerator turnOrder(bool isTurnLeft)//turns gems with some delay and send info to check any matches and ticks bomb timer, if there is no matched checks for exploeded bombs
    {
        for (int i = 0; i < 3; i++)
        {

           
            turnGems(isTurnLeft);
            getMatched();
            yield return new WaitForSeconds(0.4f);
            if (isThereMatchedGems)
            {
                
                foreach (GameObject gem in selectedGems)
                {
                    gem.GetComponent<GemProps>().toFront = false;
                    gem.GetComponent<GemProps>().goToSpot();
                }
                // DestroyMatchedGems();
                StartCoroutine(gemDestroyWait());
                moveCount++;
                i++;
                i++;
                returnBack();
                bombTick.Invoke();

            }
            else
            {
               
                if (i==2)
                {
                   
                    returnBack();
                    
                    playerCanMove = true;
                    moveCount++;
                    bombTick.Invoke();
                    checkExploded();

                }
               
            }
           
        }
      
 
      
        

    }

    public void turnGems(bool isTurnLeft) // rotate selected gems
    {
        int[,] gemIds = new int[3, 2];
    
        if(gemRotation)
        {
           // Debug.Log("gem rotating right");
            if (isTurnLeft)
            {
                
              //  Debug.Log(selectedCloseGems[0] + "-" + selectedCloseGems[1] + "-" + selectedCloseGems[2]);
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    gemIds[i, 0] = selectedGems[i].GetComponent<GemProps>().gemIdx;
                    gemIds[i, 1] = selectedGems[i].GetComponent<GemProps>().gemIdy;
                }
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    selectedGems[i].GetComponent<GemProps>().gemIdx = gemIds[(i + 1) % 3, 0];
                    selectedGems[i].GetComponent<GemProps>().gemIdy = gemIds[(i + 1) % 3, 1];
                    gemArray[selectedGems[i].GetComponent<GemProps>().gemIdx, selectedGems[i].GetComponent<GemProps>().gemIdy] = selectedGems[i];
                    selectedGems[i].GetComponent<GemProps>().goToSpot();
                }
            }
            else
            {
                
              //  Debug.Log(selectedCloseGems[0] + "-" + selectedCloseGems[1] + "-" + selectedCloseGems[2]);
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    gemIds[i, 0] = selectedGems[i].GetComponent<GemProps>().gemIdx;
                    gemIds[i, 1] = selectedGems[i].GetComponent<GemProps>().gemIdy;
                }
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    selectedGems[i].GetComponent<GemProps>().gemIdx = gemIds[(i + 2) % 3, 0];
                    selectedGems[i].GetComponent<GemProps>().gemIdy = gemIds[(i + 2) % 3, 1];
                    gemArray[selectedGems[i].GetComponent<GemProps>().gemIdx, selectedGems[i].GetComponent<GemProps>().gemIdy] = selectedGems[i];
                    selectedGems[i].GetComponent<GemProps>().goToSpot();
                }
            }
        }
        else
        {
           
            // Debug.Log("gem rotating left");
            if (isTurnLeft)
            {
                
               // Debug.Log(selectedCloseGems[0] + "-" + selectedCloseGems[1] + "-" + selectedCloseGems[2]);
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    gemIds[i, 0] = selectedGems[i].GetComponent<GemProps>().gemIdx;
                    gemIds[i, 1] = selectedGems[i].GetComponent<GemProps>().gemIdy;
                }
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    selectedGems[i].GetComponent<GemProps>().gemIdx = gemIds[(i + 2) % 3, 0];
                    selectedGems[i].GetComponent<GemProps>().gemIdy = gemIds[(i + 2) % 3, 1];
                    gemArray[selectedGems[i].GetComponent<GemProps>().gemIdx, selectedGems[i].GetComponent<GemProps>().gemIdy] = selectedGems[i];
                    selectedGems[i].GetComponent<GemProps>().goToSpot();
                }
            }
            else
            {
              
               // Debug.Log(selectedCloseGems[0] + "-" + selectedCloseGems[1] + "-" + selectedCloseGems[2]);
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    gemIds[i, 0] = selectedGems[i].GetComponent<GemProps>().gemIdx;
                    gemIds[i, 1] = selectedGems[i].GetComponent<GemProps>().gemIdy;
                }
                for (int i = 0; i < selectedGems.Length; i++)
                {
                    selectedGems[i].GetComponent<GemProps>().gemIdx = gemIds[(i + 1) % 3, 0];
                    selectedGems[i].GetComponent<GemProps>().gemIdy = gemIds[(i + 1) % 3, 1];
                    gemArray[selectedGems[i].GetComponent<GemProps>().gemIdx, selectedGems[i].GetComponent<GemProps>().gemIdy] = selectedGems[i];
                    selectedGems[i].GetComponent<GemProps>().goToSpot();
                }
            }
        }
    }
    public bool isThereAnyMove()//returns true  if there is any possible move 
    {
        int avaibleMoveCounts = 0;
        for (int x = 0; x < gemArray.GetLength(0); x++)
        {
            for (int y = 0; y < gemArray.GetLength(1); y++)
            {
                if (gemArray[x, y] != farAwayObject)
                {
                    avaibleMoveCounts = avaibleMoveCounts+checkFutureColors(gemArray[x, y], getCloseCells(gemArray[x, y]), getCloseColor(gemArray[x, y]));
                 //   Debug.Log("1--" + avaibleMoveCounts);
                }
            }
        }
        if (avaibleMoveCounts > 0)
        {

            return true;
        }
        else { return false; }
    }
    public int checkFutureColors(GameObject centerGem, GameObject[] gemNeighbours, int[] neighbourColor)//returns possible move count
    {
        int avaibleMoveCount = 0;
        // isThereMatchedGems = false;
        int homeColor = centerGem.GetComponent<GemProps>().gemColor;
        for (int i = 0; i < gemNeighbours.Length; i++)
        {
           
            if (neighbourColor[i] == homeColor && gemNeighbours[i] != farAwayObject)
            {
               // Debug.Log(gemNeighbours[i].name + "----" + centerGem.name + "---" + gemNeighbours[((i + 1) % 6)].name + "-----" + gemNeighbours[((i + 5) % 6)].name);
                int colorRing1Matches = 0;
                int colorRing2Matches = 0;
                if (gemNeighbours[((i + 1) % 6)] != farAwayObject)
                {
                    int[] colorRing1 = getCloseColor(gemNeighbours[((i + 1) % 6)]);
                    
                    for (int a = 0; a < colorRing1.Length; a++)
                    {
                       // Debug.Log("1--"+colorRing1[a]);
                        if (colorRing1[a] == homeColor)
                        {
                            colorRing1Matches++;
                        }
                    }
                }

                if (gemNeighbours[((i + 5) % 6)] != farAwayObject)
                {
                    int[] colorRing2 = getCloseColor(gemNeighbours[((i + 5) % 6)]);
                   
                    for (int b = 0; b < colorRing2.Length; b++)
                    {
                       // Debug.Log("2---"+colorRing2[b]);
                        if (colorRing2[b] == homeColor)
                        {
                            colorRing2Matches++;
                        }
                    }
                }

              //  Debug.Log("match1-" + colorRing1Matches + "match2-" + colorRing2Matches);
                if(colorRing1Matches > 2 || colorRing2Matches > 2)
                {
                    avaibleMoveCount++;
                }

        
            }


        }
       // Debug.Log(avaibleMoveCount);
        return avaibleMoveCount;


    }

    public int loadingBar()//when gems preparing shows a loading bar, till there is a playable gem board avaible.
    {
        GameObject[] Gems = GameObject.FindGameObjectsWithTag("gem");
        List<GameObject> matchedGems = new List<GameObject>();
        foreach (GameObject gem in Gems)
        {
            if(gem.GetComponent<GemProps>().isMatched)
            {
                matchedGems.Add(gem);
            }
        }
        if(loadStart < matchedGems.Count+ emptySpots)
        {
           // loadStart = matchedGems.Count + emptySpots;
            loadingSlider.maxValue = loadStart;
            loadingSlider.value = 0;
        }
        else
        {
            loadStart =  30;
            loadingSlider.maxValue = loadStart;
            loadingSlider.value = loadStart - (matchedGems.Count + emptySpots);
        }
        return matchedGems.Count;

    }
    void Update()
    {
      
    }
}
