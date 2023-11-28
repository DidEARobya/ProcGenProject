using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utility : MonoBehaviour
{
    public static Utility instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public bool ValueCheck(float value, int check)
    {
        if (HasRemainder(check, value))
        {
            return true;
        }

        return false;
    }
    private bool HasRemainder(float dividend, float divisor)
    {
        return dividend % divisor == 0;
    }
}
