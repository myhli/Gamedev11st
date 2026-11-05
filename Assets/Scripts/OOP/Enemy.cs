using UnityEngine;

public class Enemy : MonoBehaviour, IDamageable
{
    [SerializeField] private int hp = 100;
    public float ms = 2f;
    protected Transform player;

    [Header("Pengaturan State Machine")]
    [SerializeField] private float jarakDeteksi = 6f;
    [SerializeField] private float jarakSerang = 1.5f;
    

    // State sekarang
    private StateZombie state = StateZombie.IDLE;
    private float waktuSerangTerakhir;

    protected virtual void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void Update()
    {
        PeriksaTransisi();

        switch (state)
        {
            case StateZombie.IDLE: 
                PerilakuIdle(); 
                break;
            case StateZombie.PATROL: 
                PerilakuPatrol(); 
                break;
            case StateZombie.CHASE: 
                PerilakuChase(); 
                break;
            case StateZombie.ATTACK: 
                PerilakuAttack(); 
                break;
        }
    }

    public float JarakKePlayer()
    {
        if (player == null) return Mathf.Infinity;
        return Vector2.Distance(transform.position, player.position);
    }

    private void PeriksaTransisi()
    {
        float jarak = JarakKePlayer();

        if (jarak <= jarakSerang)
            state = StateZombie.ATTACK; // sangat dekat -> serang
        else if (jarak <= jarakDeteksi)
            state = StateZombie.CHASE;  // terlihat -> kejar
        else
            state = StateZombie.PATROL; // jauh -> keliling
    }

    // --- PERILAKU STATE ---
    private void PerilakuIdle()
    {
       // Debug.Log("blyatman sedang idle");
    }

    private void PerilakuPatrol()
    {
       // Debug.Log("blyatman sedang patrol");
    }

    private void PerilakuChase()
    {
       // Debug.Log("blyatman sedang chase");
        Kejar();
    }

    private void PerilakuAttack()
    {
        //Debug.Log("blyatman sedang attack");
        Serang();
    }

    // --- AKSI & DAMAGE ---
    public void Kejar()
    {
        if (player == null) return;

        transform.position = Vector2.MoveTowards(
            transform.position,
            player.position,
            ms * Time.deltaTime
        );
    }

    public virtual void Serang()
    {
       // Debug.Log("Enemy menyerang!");
    }

    public void KenaDamage(int jumlah)
    {
        hp -= jumlah;
        Debug.Log($"{gameObject.name} kena damage {jumlah}, HP sisa: {hp}");

        if (hp <= 0)
        {
            Mati();
        }
    }

    private void Mati()
    {
        Debug.Log($"{gameObject.name} mati!");
        Destroy(gameObject);
    }
}