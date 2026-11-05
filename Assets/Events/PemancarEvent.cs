using UnityEngine;
using System;
using UnityEngine.InputSystem;

public class PemancarEvent : MonoBehaviour
{
    public static event Action SaatTombolDitekan;

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Debug.Log("tombol ditekan");
            SaatTombolDitekan?.Invoke(); // Atau bisa juga: SaatTombolDitekan?.();
        }
    }
}