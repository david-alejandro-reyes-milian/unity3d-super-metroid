using UnityEngine;
using System.Collections;

public class BackScreenController : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    public bool turnScreenBlack = false;
    public float screenBlackoutSpeed = .02f;
    Color transparentBlackColor = new Color(0, 0, 0, 0);
    Color normalBlackColor = new Color(0, 0, 0, 1);
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>(); spriteRenderer.color = normalBlackColor;
    }
    void Update()
    {
        if (turnScreenBlack)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, normalBlackColor, screenBlackoutSpeed);
            //if (spriteRenderer.color.a >= .8f) spriteRenderer.color = normalBlackColor;
        }
        else
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, transparentBlackColor, screenBlackoutSpeed);
            //if (spriteRenderer.color.a <= .2f) spriteRenderer.color = transparentBlackColor;
        }
    }
}
