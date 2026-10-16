using UnityEngine;
namespace Aurelia
{
    public class WoodlandLife:MonoBehaviour
    {
        public enum Kind{Petals,Swing,Butterfly}
        public Kind kind;
        public float seed;
        Vector3 start;Quaternion rotation;
        void Start(){start=transform.position;rotation=transform.rotation;}
        void Update(){float t=Time.time;if(kind==Kind.Swing)transform.rotation=rotation*Quaternion.Euler(Mathf.Sin(t*.7f+seed)*2,0,0);else if(kind==Kind.Butterfly){transform.position=start+new Vector3(Mathf.Sin(t*.6f+seed)*2,Mathf.Sin(t*1.1f+seed)*.4f,Mathf.Cos(t*.45f+seed)*1.5f);transform.rotation=Quaternion.Euler(Mathf.Sin(t*13)*30,t*20,Mathf.Sin(t*16)*35);}}
    }
}
