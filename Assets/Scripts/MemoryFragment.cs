using UnityEngine;

public class MemoryFragment : MonoBehaviour
{
    public float rotateSpeed = 50f;
    public float collectionRadius = 2f; // Increased radius
    private GameManager gameManager;
    private bool collected = false;
    private Transform player;
    
    void Start()
    {
gameManager = FindFirstObjectByType<GameManager>();
        
        // Find player by tag - most reliable
        GameObject playerObj = GameObject.FindWithTag("Player");
        if(playerObj != null) 
        {
            player = playerObj.transform;
            Debug.Log($"Fragment found player: {playerObj.name}");
        }
        else
        {
            Debug.LogError("Fragment cannot find player! Make sure player is tagged 'Player'");
        }
        
        MakeFragmentGlow();
        MakeCollectionEasier();
    }
    
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        
        // Add floating animation
        float newY = Mathf.Sin(Time.time * 3f) * 0.3f + 1f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Check distance to player every frame
        if(!collected && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if(distance <= collectionRadius)
            {
                Debug.Log("Fragment collected by distance check!");
                CollectFragment();
            }
        }
    }
    
    // Also add trigger detection as backup
    void OnTriggerEnter(Collider other)
    {
        if (!collected && other.CompareTag("Player"))
        {
            Debug.Log("Fragment collected by trigger!");
            CollectFragment();
        }
    }
    
    void CollectFragment()
    {
        if (collected) return; // Prevent double collection
        
        collected = true;
        CreateCollectionEffect();
        
        if (gameManager != null)
        {
            gameManager.CollectFragment();
        }
        else
        {
            Debug.LogError("GameManager not found!");
        }
        
        Destroy(gameObject);
    }
    
    void MakeCollectionEasier()
    {
        // Ensure fragment has a trigger collider
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            SphereCollider sphereCol = gameObject.AddComponent<SphereCollider>();
            sphereCol.isTrigger = true;
            sphereCol.radius = collectionRadius;
            Debug.Log("Added trigger collider to fragment");
        }
        else
        {
            col.isTrigger = true;
            if (col is SphereCollider sphereCol)
            {
                sphereCol.radius = collectionRadius;
            }
        }
    }
    
    void CreateCollectionEffect()
    {
        GameObject effect = new GameObject("CollectionEffect");
        effect.transform.position = transform.position;
        
        ParticleSystem particles = effect.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = Color.yellow;
        main.startSize = 0.1f;
        main.startSpeed = 5f;
        main.maxParticles = 20;
        
        var emission = particles.emission;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 20)
        });
        
        Destroy(effect, 2f);
    }
    
    void MakeFragmentGlow()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Material glowMat = new Material(renderer.sharedMaterial);
            glowMat.EnableKeyword("_EMISSION");
            glowMat.SetColor("_EmissionColor", Color.yellow * 2f);
            renderer.material = glowMat;
        }
    }
}