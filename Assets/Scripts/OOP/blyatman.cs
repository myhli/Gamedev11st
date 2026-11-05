using UnityEngine;

public class blyatman : Enemy
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public bool flag =true;

    public override void Serang()
    {
        Debug.Log("blyatman throwing vodka");
    }
}
