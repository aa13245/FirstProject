using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DetectWall : MonoBehaviour
{
    public HashSet<GameObject> inRange = new HashSet<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            inRange.Add(other.gameObject);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            inRange.Remove(other.gameObject);
        }
    }
    float ColliderDistance(Collider wall)
    {
        Vector3 closestPoint = wall.ClosestPoint(transform.position);
        return Vector3.Distance(transform.position, closestPoint);
    }
    public GameObject[] GetInRangeEntities()
    {

        return inRange.OrderBy(wall => ColliderDistance(wall.GetComponent<Collider>())).ToArray();
    }
}
