using UnityEngine;
using System.Text;

public class ColliderDataExtractor : MonoBehaviour
{
    void Start()
    {
        StringBuilder sb = new StringBuilder();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            BoxCollider collider = obj.GetComponent<BoxCollider>();
            if (collider != null)
            {
                Transform t = collider.transform;

                Vector3 size = collider.size * 0.5f;
                size.x *= t.lossyScale.x;
                size.y *= t.lossyScale.y;
                size.z *= t.lossyScale.z;

                Vector3 position = t.position;
                position += collider.center;
                position.x = -position.x; 

                Quaternion rotation = t.rotation;
                rotation.y = -rotation.y;
                rotation.z = -rotation.z; 

                // Çü½Ä: { sizex, sizey, sizez, positionx, positiony, positionz, rotationx, rotationy, rotationz, rotationw },
                sb.AppendLine($"{{ {size.x}, {size.y}, {size.z}, {position.x}, {position.y}, {position.z}, {rotation.x}, {rotation.y}, {rotation.z}, {rotation.w} }},");
            }
        }

        Debug.Log(sb.ToString());
    }
}
