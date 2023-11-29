using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerBase : MonoBehaviour
{
    [SerializeField]
    public HealthBar healthBar;

    public delegate void HealthUpdate(float diff);
    public HealthUpdate healthUpdate;

    private float health = 10;

    // Start is called before the first frame update
    void Start()
    {
        healthUpdate = UpdateHealth;
    }

    private void UpdateHealth(float diff)
    {
        healthBar.UpdateHealthBar(diff);
    }
}
