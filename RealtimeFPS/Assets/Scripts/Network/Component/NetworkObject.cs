using Newtonsoft.Json.Bson;
using UnityEngine;
using UnityEngine.UIElements;

namespace Framework.Network
{
    public class NetworkObject : MonoBehaviour
    {
        public Client Client { get; set; }
        public int id = -1;
        public bool isMine = false;

        void Start()
        {
            EffectPool effectPool = FindObjectOfType<EffectPool>();
            effectPool?.Spawn(EffectType.Effect_Thunder, transform.position, Quaternion.identity);
        }

        void OnDestroy()
        {
            EffectPool effectPool = FindObjectOfType<EffectPool>();
            effectPool?.Spawn(EffectType.Effect_Thunder, transform.position, Quaternion.identity);
        }
    }
}