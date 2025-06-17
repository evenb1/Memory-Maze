using UnityEngine;

public class MemoryFragment : MonoBehaviour
{
    public float rotateSpeed = 50f;
    public float collectionRadius = 1.5f; // How close you need to be
    private GameManager gameManager;
    private bool collected = false;
    private Transform player;
    
    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        
        // Find player
        GameObject playerObj = GameObject.Find("Player");
        if(playerObj == null) playerObj = GameObject.Find("Bit27");
        if(playerObj != null) player = playerObj.transform;
        
        MakeFragmentGlow();
        MakeCollectionEasier();
    }
    
    void Update()
    {
        transform.Rotate(0, rotateSpeed * Time.deltaTime, 0);
        
        // Add floating animation
        float newY = Mathf.Sin(Time.time * 3f) * 0.3f + 1f;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        
        // Check distance to player every frame (more reliable than OnTriggerEnter)
        if(!collected && player != null)
        {
            float distance = Vector3.Distance(transform.position, player.position);
            if(distance <= collectionRadius)
            {
                CollectFragment();
            }
        }
    }
    
    void CollectFragment()
    {
        collected = true;
        CreateCollectionEffect();
        gameManager.CollectFragment();
        Destroy(gameObject);
    }
    
    void MakeCollectionEasier()
    {
        // Make the trigger area bigger
        SphereCollider collider = GetComponent<SphereCollider>();
        if(collider != null)
        {
            collider.radius = collectionRadius; // Much bigger trigger area
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
        Material glowMat = new Material(renderer.material);
        glowMat.EnableKeyword("_EMISSION");
        glowMat.SetColor("_EmissionColor", Color.yellow * 2f);
        renderer.material = glowMat;
    }
}