using System.Collections.Generic;
using UnityEngine;

// ObjectPool: This script implements an efficient object pooling system to reduce the performance impact
// of frequently instantiated and destroyed objects like bullets, particles, enemies, or collectibles.
// Instead of creating and destroying objects at runtime (which triggers garbage collection),
// this system pre-instantiates objects and reuses them when needed.

// Usage Examples:
// 1. Create a pool manager in your scene:
//    ObjectPool bulletPool = new ObjectPool(bulletPrefab, 30);

// 2. Get an object from the pool:
//    GameObject bullet = bulletPool.GetObject();
//    bullet.transform.position = gunBarrel.position;
//    bullet.transform.rotation = gunBarrel.rotation;

// 3. Return an object to the pool when done (e.g., when bullet hits something):
//    bulletPool.ReturnObject(bullet);
//    // Or call this from the bullet script itself:
//    ObjectPool.ReturnToPool(gameObject);

// 4. Create a pool with custom initialization and get callbacks:
//    ObjectPool enemyPool = new ObjectPool(
//        enemyPrefab, 
//        10, 
//        (enemy) => { enemy.GetComponent<Enemy>().ResetHealth(); },
//        (enemy) => { enemy.GetComponent<Enemy>().Activate(); }
//    );

public class ObjectPool : MonoBehaviour
{
    [SerializeField] private GameObject prefab; // The prefab to pool
    [SerializeField] private int initialPoolSize = 10; // Initial number of instances to create
    [SerializeField] private bool canExpand = true; // Whether the pool can grow beyond initialPoolSize
    [SerializeField] private Transform poolParent; // Optional parent transform for organization
    
    private Queue<GameObject> pooledObjects; // Queue to store inactive objects
    private List<GameObject> activeObjects; // List to track active objects
    private Dictionary<GameObject, ObjectPool> objectToPoolMapping; // Static mapping to find an object's pool
    
    private System.Action<GameObject> onReturnToPool; // Optional callback when object is returned to pool
    private System.Action<GameObject> onGetFromPool; // Optional callback when object is retrieved from pool
    
    // Static dictionary to track which pool an object belongs to
    private static Dictionary<GameObject, ObjectPool> globalObjectToPoolMapping = new Dictionary<GameObject, ObjectPool>();
    
    // Initialize the pool with default settings
    private void Awake()
    {
        // If no parent transform is specified, create one
        if (poolParent == null)
        {
            GameObject parent = new GameObject($"{prefab.name} Pool");
            poolParent = parent.transform;
        }
        
        // Initialize collections
        pooledObjects = new Queue<GameObject>();
        activeObjects = new List<GameObject>();
        objectToPoolMapping = new Dictionary<GameObject, ObjectPool>();
        
        // Pre-warm the pool with initial objects
        PreWarmPool();
    }
    
    // Create a pool programmatically (for when you need to create pools at runtime)
    public static ObjectPool CreatePool(GameObject prefab, int initialSize, Transform parent = null)
    {
        GameObject poolGO = new GameObject($"{prefab.name} Pool");
        ObjectPool pool = poolGO.AddComponent<ObjectPool>();
        pool.prefab = prefab;
        pool.initialPoolSize = initialSize;
        pool.poolParent = parent ?? poolGO.transform;
        pool.PreWarmPool();
        return pool;
    }
    
    // Initialize the pool with the specified number of objects
    private void PreWarmPool()
    {
        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewPooledObject();
        }
    }
    
    // Create a new instance of the prefab and add it to the pool
    private GameObject CreateNewPooledObject()
    {
        // Instantiate a new object from the prefab
        GameObject newObject = Instantiate(prefab, poolParent);
        
        // Set it inactive
        newObject.SetActive(false);
        
        // Add it to the pool
        pooledObjects.Enqueue(newObject);
        
        // Register this object with its pool for later reference
        globalObjectToPoolMapping[newObject] = this;
        
        return newObject;
    }
    
    // Get an object from the pool
    public GameObject GetObject()
    {
        GameObject pooledObject;
        
        // If the pool is empty
        if (pooledObjects.Count == 0)
        {
            // If we can expand the pool, create a new object
            if (canExpand)
            {
                pooledObject = CreateNewPooledObject();
            }
            else
            {
                // If we can't expand, log a warning and return null
                Debug.LogWarning($"Object pool for {prefab.name} is empty and cannot expand!");
                return null;
            }
        }
        else
        {
            // Get an object from the pool
            pooledObject = pooledObjects.Dequeue();
        }
        
        // Activate the object
        pooledObject.SetActive(true);
        
        // Add to active objects list
        activeObjects.Add(pooledObject);
        
        // Call the get callback if it exists
        onGetFromPool?.Invoke(pooledObject);
        
        return pooledObject;
    }
    
    // Get an object from the pool and set its position and rotation
    public GameObject GetObject(Vector3 position, Quaternion rotation)
    {
        GameObject obj = GetObject();
        if (obj != null)
        {
            obj.transform.position = position;
            obj.transform.rotation = rotation;
        }
        return obj;
    }
    
    // Return an object to the pool
    public void ReturnObject(GameObject obj)
    {
        if (activeObjects.Contains(obj))
        {
            // Call the return callback if it exists
            onReturnToPool?.Invoke(obj);
            
            // Deactivate the object
            obj.SetActive(false);
            
            // Remove from active objects
            activeObjects.Remove(obj);
            
            // Add back to the pool
            pooledObjects.Enqueue(obj);
        }
        else
        {
            Debug.LogWarning($"Trying to return {obj.name} to pool, but it's not from this pool!");
        }
    }
    
    // Static method to return an object to its pool
    // This is useful for objects to return themselves without knowing their pool
    public static void ReturnToPool(GameObject obj)
    {
        if (globalObjectToPoolMapping.TryGetValue(obj, out ObjectPool pool))
        {
            pool.ReturnObject(obj);
        }
        else
        {
            Debug.LogWarning($"Object {obj.name} was not created from a pool. Destroying instead.");
            Destroy(obj);
        }
    }
    
    // Set callbacks for object initialization and retrieval
    public void SetCallbacks(System.Action<GameObject> onReturn, System.Action<GameObject> onGet)
    {
        onReturnToPool = onReturn;
        onGetFromPool = onGet;
    }
    
    // Return all active objects to the pool
    public void ReturnAllObjects()
    {
        // Create a copy of the active objects list to avoid modification during iteration
        GameObject[] objectsToReturn = activeObjects.ToArray();
        
        // Return each object to the pool
        foreach (GameObject obj in objectsToReturn)
        {
            ReturnObject(obj);
        }
    }
    
    // Get the count of available objects in the pool
    public int AvailableCount => pooledObjects.Count;
    
    // Get the count of active objects from this pool
    public int ActiveCount => activeObjects.Count;
    
    // Clean up when the pool is destroyed
    private void OnDestroy()
    {
        // Remove all references from the global mapping
        foreach (GameObject obj in activeObjects)
        {
            if (globalObjectToPoolMapping.ContainsKey(obj))
            {
                globalObjectToPoolMapping.Remove(obj);
            }
        }
        
        foreach (GameObject obj in pooledObjects)
        {
            if (globalObjectToPoolMapping.ContainsKey(obj))
            {
                globalObjectToPoolMapping.Remove(obj);
            }
        }
    }
}

// Optional helper component that can be added to pooled objects to automatically return them to the pool
// For example, add this to bullets with a timer to auto-return after a certain time
public class PooledObject : MonoBehaviour
{
    [SerializeField] private float autoReturnTime = 0f; // If > 0, automatically return after this many seconds
    
    private void OnEnable()
    {
        // If auto-return is enabled, schedule the return
        if (autoReturnTime > 0)
        {
            Invoke(nameof(ReturnToPool), autoReturnTime);
        }
    }
    
    private void OnDisable()
    {
        // Cancel any pending auto-return
        CancelInvoke(nameof(ReturnToPool));
    }
    
    // Return this object to its pool
    public void ReturnToPool()
    {
        ObjectPool.ReturnToPool(gameObject);
    }
}
