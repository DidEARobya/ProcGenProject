using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    public Image healthFill;

    private void Start()
    {
        healthFill.fillAmount = 1;
    }
    public void UpdateHealthBar(float difference)
    {
        healthFill.fillAmount += difference / 10;
    }
}
