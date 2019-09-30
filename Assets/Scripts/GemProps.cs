using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.Events;
using TMPro;
using UnityEngine;

public class GemProps : MonoBehaviour
{
    public int gemColor;
    public int gemIdx;
    public int gemIdy;
    public float normalXOffset;
    public float normalYOffset;
    public float oddLineOffset;
    public bool isMatched;
    public GameController gameControl;
    private Vector3 destination;
    public float gemSpeed;
    public bool toFront;
    public GameObject bombIcon;
    public bool isBomb;
    public int bombTimerStart = 9;
    public int bombTimer = 9;
    public int bombSpawnTurn;
    public TextMeshPro bombTimerDisp;
    public GameObject gemSelectedBorder;
   

    // Start is called before the first frame update
    void Start()
    {
        gameControl = GameObject.FindObjectOfType<GameController>();
       if(isBomb)
        {
            Debug.Log("bomb");
            bombIcon.SetActive(true);
            tickTheBomb();
        }

        else { bombIcon.SetActive(false); }
    }

    public void goToSpot()//Set position which depends on grid place
    {
        float z = 0;
        if(toFront)
        {
            z = -1;
            gemSelectedBorder.SetActive(true);
        }
        else
        {
            z = 0;
            gemSelectedBorder.SetActive(false);
        }
        
        if (gemIdx % 2 == 1)
        {
            destination = new Vector3((gemIdx * normalXOffset), (gemIdy * normalYOffset)  + oddLineOffset, z);
        }
        else
        {          
            destination = new Vector3((gemIdx * normalXOffset) , (gemIdy * normalYOffset), z);
        }
        
       
    }
    public void tickTheBomb() // ticks timer
    {
        if (isBomb)
        {
            bombTimer = bombTimerStart - (gameControl.moveCount - bombSpawnTurn);
            if(bombTimer<1)
            {
                bombTimerDisp.text = "0";
            }
            else
            {
                bombTimerDisp.text = bombTimer.ToString();
            }
           
        }
    }

    public void checkUnder()//check if grid under the gem is empty and if it is empty take empty place 
    {
        if(gemIdy-1 >-1 &&gameControl.GetComponent<GameController>().gemArray[gemIdx,gemIdy-1] == gameControl.GetComponent<GameController>().farAwayObject)
        {
           
            gameControl.GetComponent<GameController>().gemArray[gemIdx, gemIdy - 1] = this.gameObject;
            gameControl.GetComponent<GameController>().gemArray[gemIdx, gemIdy] = gameControl.GetComponent<GameController>().farAwayObject;
            gemIdy--;
        }
       
            
    }
  
    // Update is called once per frame
    void Update()
    {  
        checkUnder();
        transform.position = Vector3.Lerp(transform.position, destination, gemSpeed);
    }
  
  
}
