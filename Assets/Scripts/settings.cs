using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class settings : MonoBehaviour
{

    public Slider xSizeSlider;
    public Slider ySizeSlider;
    public Slider bombThreshhold;
    public Slider bombTimerStart;
    public int maxValue = 50;
    public int bombThresholdMax = 2000;
    public int bombTimerStartMax = 20;
    public GameController gameControl;
    public TextMeshProUGUI xSizeDisp;
    public TextMeshProUGUI ySizeDisp;
    public TextMeshProUGUI bombThreshholdDisp;
    public TextMeshProUGUI bombTimerStartDisp;


    // Start is called before the first frame update
    void Start()
    {
        gameControl = transform.GetComponent<GameController>();
        xSizeSlider.maxValue = maxValue;
        ySizeSlider.maxValue = maxValue;
        bombThreshhold.maxValue = bombThresholdMax;
        bombTimerStart.maxValue = bombTimerStartMax;


        xSizeSlider.value = gameControl.xGridSize;
        ySizeSlider.value = gameControl.yGridSize;
        bombThreshhold.value = gameControl.bombThreshhold;
        bombTimerStart.value = gameControl.bombTimerStart;

        // UpdateSliderText();
    }

    public void OnSliderAdjustX()// gets slider values and apply to grid size
    {
        gameControl.xGridSize = getSliderValue(xSizeSlider);
        gameControl.yGridSize = getSliderValue(ySizeSlider);
        gameControl.bombThreshhold = getSliderValue(bombThreshhold);
        gameControl.bombTimerStart = getSliderValue(bombTimerStart);
        UpdateSliderText();
       

    }

    private void UpdateSliderText()//updates sliders display
    {
        xSizeDisp.text = gameControl.xGridSize.ToString();
        ySizeDisp.text = gameControl.yGridSize.ToString();
        bombThreshholdDisp.text = gameControl.bombThreshhold.ToString();
        bombTimerStartDisp.text = gameControl.bombTimerStart.ToString();


    }
  

    private int getSliderValue (Slider slider) // gets slider values as integers 
    {
        int sliderValue = Mathf.RoundToInt(slider.value);
        if(sliderValue > 2)
        {
            slider.value = sliderValue;
            return sliderValue;
           
        }
        else
        {
            slider.value = sliderValue;
            return 2;
            
        }
        
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
