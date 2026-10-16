using UnityEngine;
namespace Aurelia
{
    public class CloudDrift:MonoBehaviour
    {
        public float speed=.65f,phase,range=1600,billow=.8f;
        Vector3 origin;Vector3 scale;
        void OnEnable(){origin=transform.position;scale=transform.localScale;}
        void Update(){float t=Time.time;transform.position=origin+new Vector3(Mathf.Repeat(t*speed+range/2,range)-range/2,Mathf.Sin(t*.017f+phase)*billow,0);transform.localScale=scale*(1+Mathf.Sin(t*.025f+phase)*.008f);}
        public void FaceCamera(Camera camera){if(!camera)return;var d=camera.transform.position-transform.position;d.y=0;if(d.sqrMagnitude>.01f)transform.rotation=Quaternion.LookRotation(d);}
        void LateUpdate(){FaceCamera(Camera.main);}
    }
}
