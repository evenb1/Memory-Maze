using UnityEngine;

public class VirusAI : MonoBehaviour
{
    public float moveSpeed = 3f;
    public float detectionRange = 15f;
    private Transform player;
    private GameManager gameManager;
    
    void Start()
    {
        GameObject playerObj = GameObject.Find("Player");
        if(playerObj == null) playerObj = GameObject.Find("Bit27");
        
        if(playerObj != null)
        {
            player = playerObj.transform;
        }
        
        gameManager = FindObjectOfType<GameManager>();
        
        // Make virus look menacing
        MakeVirusGlow();
    }
    
    void Update()
    {
        if(player != null)
        {
            HuntPlayer();
        }
    }
    
    void HuntPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if(distanceToPlayer <= detectionRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position += direction * moveSpeed * Time.deltaTime;
            transform.LookAt(player);
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.name.Contains("Player") || other.gameObject.name.Contains("Bit27"))
        {
            Debug.Log("GAME OVER! Virus caught Bit-27!");
            
            if(gameManager != null)
            {
                gameManager.GameOver();
            }
        }
    }
    
    void MakeVirusGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        Material virusMat = new Material(renderer.material);
        virusMat.EnableKeyword("_EMISSION");
        virusMat.SetColor("_EmissionColor", Color.red * 3f);
        renderer.material = virusMat;
    }
}