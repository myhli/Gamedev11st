using UnityEngine;

public class PenerimaEvent : MonoBehaviour
{
   private void OnEnable()
   {
    PemancarEvent.SaatTombolDitekan += Respon;
   }
   private void OnDisable()
   {
    PemancarEvent.SaatTombolDitekan -= Respon;
   }
   void Respon()
   {
    Debug.Log("Tombol ditekan");
   }
}
