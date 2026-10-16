using UnityEngine;
namespace Aurelia
{
    public class HarbourResidentMotion:MonoBehaviour
    {
        public bool walking;public float phase;public Vector3[] route;
        Transform left,right,armL,armR;Quaternion l0,r0,a0,b0;int next;Vector3 origin;
        Transform Find(string name){foreach(var t in GetComponentsInChildren<Transform>())if(t.name==name)return t;return null;}
        void Start(){left=Find("LeftLeg");right=Find("RightLeg");armL=Find("LeftArm");armR=Find("RightArm");if(left)l0=left.localRotation;if(right)r0=right.localRotation;if(armL)a0=armL.localRotation;if(armR)b0=armR.localRotation;origin=transform.position;}
        void Update()
        {
            float t=Time.time+phase,amp=walking?22:1.3f;
            if(walking&&route!=null&&route.Length>1){var target=route[next];var delta=target-transform.position;if(delta.magnitude<.2f)next=(next+1)%route.Length;else{transform.position=Vector3.MoveTowards(transform.position,target,Time.deltaTime*.65f);delta.y=0;if(delta.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(delta),Time.deltaTime*4);}}
            if(left)left.localRotation=l0*Quaternion.Euler(Mathf.Sin(t*4)*amp,0,0);if(right)right.localRotation=r0*Quaternion.Euler(-Mathf.Sin(t*4)*amp,0,0);
            if(armL)armL.localRotation=a0*Quaternion.Euler(-Mathf.Sin(t*4)*amp*.5f,0,0);if(armR)armR.localRotation=b0*Quaternion.Euler(Mathf.Sin(t*4)*amp*.5f,0,0);
        }
    }
}
