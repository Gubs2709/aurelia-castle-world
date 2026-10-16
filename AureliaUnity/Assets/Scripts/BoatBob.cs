using UnityEngine;

namespace Aurelia
{
    public class BoatBob:MonoBehaviour
    {
        public float amp=.08f,speed=1f,phase,roll=2.2f,drift=0f;
        Vector3 p0;Quaternion r0;
        void Start(){p0=transform.position;r0=transform.rotation;if(phase==0)phase=Random.value*10;}
        void Update()
        {
            float t=Time.time*speed+phase;
            transform.position=p0+new Vector3(0,Mathf.Sin(t*1.2f)*amp+Mathf.Sin(t*.53f)*amp*.5f,0)+transform.forward*Mathf.Sin(t*.3f)*drift;
            transform.rotation=r0*Quaternion.Euler(Mathf.Sin(t*.9f)*roll*.6f,0,Mathf.Sin(t*1.1f+1)*roll);
        }
    }
}
