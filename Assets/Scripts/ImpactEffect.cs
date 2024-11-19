using UnityEngine;

public class ImpactEffect : MonoBehaviour
{
    public ImpactEffect Instance { get; private set; }
    
    [SerializeField] private float impactEffectDeleteTime;

    private void Awake()
    {
        Instance = this;
    }
    
    private void DestroyEffect()
    {
        Destroy(gameObject);
    }

    // Start is called before the first frame update
    void Start()
    {
        Invoke(nameof(DestroyEffect), impactEffectDeleteTime);
    }
}
