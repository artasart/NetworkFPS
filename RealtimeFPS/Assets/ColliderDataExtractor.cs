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
                Vector3 size = collider.size * 0.5f; // 크기를 절반으로
                Vector3 worldSize = t.TransformVector(size);
                Vector3 position = t.position;
                position.x = -position.x; // x 위치 반전
                Quaternion rotation = t.rotation;
                rotation.y = -rotation.y; // x 회전 반전

                // 형식: { sizex, sizey, sizez, positionx, positiony, positionz, rotationx, rotationy, rotationz, rotationw },
                sb.AppendLine($"{{ {worldSize.x}, {worldSize.y}, {worldSize.z}, {position.x}, {position.y}, {position.z}, {rotation.x}, {rotation.y}, {rotation.z}, {rotation.w} }},");
            }
            else
            {
                sb.AppendLine($"'{obj.name}' has no BoxCollider,");
            }
        }

        Debug.Log(sb.ToString());
    }
}
