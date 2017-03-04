using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EnergyNumberSpriteRenderer : MonoBehaviour
{

    public Sprite[] numbers;
    Image number0, number1;
    void Awake()
    {
        number0 = GameObject.Find("/GUI/TopPanel/EnergyNumber/EnergyNumberImage0").GetComponent<Image>();
        number1 = GameObject.Find("/GUI/TopPanel/EnergyNumber/EnergyNumberImage1").GetComponent<Image>();
    }
    public void UpdateEnergyGui(int energy)
    {
        string energyString = energy + "";
        number0.sprite = numbers[0];
        if (energy > 9)
        {
            number0.sprite = numbers[energy / 10];
            number1.sprite = numbers[energy % 10];
        }
        else { number1.sprite = numbers[energy % 10]; }
    }

}
