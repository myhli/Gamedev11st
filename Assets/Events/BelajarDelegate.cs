using UnityEngine;
using System; 

public class BelajarDelegate : MonoBehaviour
{
    // Deklarasi Custom Delegate
    delegate void ContohDelegate();

    void Start()
    {
        Debug.Log("--- Uji Delegate 1 (Single) ---");
        UjiDelegate();

        Debug.Log("--- Uji Delegate 2 (Multicast) ---");
        UjiDelegate2();

        Debug.Log("--- Uji Delegate 3 (Action) ---");
        UjiDelegate3();
    }

    // 1. Single Cast Delegate (Manggil 1 fungsi)
    void UjiDelegate()
    {
        ContohDelegate Halo = PanggilHello;
        Halo();
    }

    // 2. Multicast Delegate (Manggil 2 fungsi berturut-turut)
    void UjiDelegate2()
    {
        ContohDelegate Halo = PanggilHello;
        Halo += PanggilWorld;
        Halo();
    }

    // 3. System Action (Bentuk simpel dari Delegate tanpa butuh keyword 'delegate void')
    void UjiDelegate3()
    {
        Action Halo = PanggilHello;
        Halo += PanggilWorld;
        Halo();
    }

    void PanggilHello()
    {
        Debug.Log("Hello");
    }

    void PanggilWorld()
    {
        Debug.Log("World");
    }
}