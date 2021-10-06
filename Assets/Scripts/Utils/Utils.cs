using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static void ResetArrayValue<T>(T[] array, T value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }
}
