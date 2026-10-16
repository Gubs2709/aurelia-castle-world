using UnityEngine;

namespace Aurelia
{
    // Each serializable MonoBehaviour has its own matching source asset.
    public class ValleyMotion : MonoBehaviour
    {
        public enum Kind { Flag, Water, Dragon, Cloud, Fireflies }
        public Kind kind;
        public float seed;
        Vector3 initial;
        Material flowingMaterial;
        void Start()
        {
            initial = transform.position;
            if(kind==Kind.Water && TryGetComponent<Renderer>(out var r)) flowingMaterial=r.material;
        }
        void Update()
        {
            float t = Time.time;
            if (kind == Kind.Flag) transform.localRotation = Quaternion.Euler(0, Mathf.Sin(t * 1.4f + seed) * 8, Mathf.Sin(t * 2 + seed) * 2);
            if (kind == Kind.Water && flowingMaterial) flowingMaterial.mainTextureOffset = new Vector2(t * .02f, -t * .19f);
            if (kind == Kind.Dragon) { transform.position = initial + new Vector3(Mathf.Sin(t * .065f) * 60, Mathf.Sin(t * .3f) * 3, Mathf.Cos(t * .065f) * 45); transform.rotation = Quaternion.Euler(0, t * 3.72f + 90, Mathf.Sin(t * .13f) * 7); }
            if (kind == Kind.Cloud) transform.position = initial + Vector3.right * Mathf.Sin(t * .01f + seed) * 12;
            if (kind == Kind.Fireflies) transform.localRotation = Quaternion.Euler(0, t * 1.5f, 0);
        }
        void OnDestroy(){if(flowingMaterial)Destroy(flowingMaterial);}
    }
}
