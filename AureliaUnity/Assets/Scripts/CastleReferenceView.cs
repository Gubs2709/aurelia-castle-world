using UnityEngine;
using UnityEngine.InputSystem;

namespace Aurelia
{
    public class CastleReferenceView : MonoBehaviour
    {
        public Camera view;
        public Vector3 picturePosition=new Vector3(-8,18,-330), pictureTarget=new Vector3(0,85,125);
        public bool flying;
        public Transform walker;
        public Transform rider;
        public Transform avatar;
        public Vector3 spawn,forestPoint,blossomPoint,cottagePoint,waterfallPoint;
        public Vector3 harbourPoint,autumnPoint;
        public Vector3 retreatPoint,headlandPoint;
        public bool startWithPlayer=true;
        public Vector3 lookTarget=new Vector3(0,90,100);
        public float cameraDistance=6.5f;
        CharacterController body;
        Animator animator;
        float yaw,pitch,vertical;
        bool paused,showHelp=true,photograph;
        public float CameraYaw=>yaw;
        void Lock(bool on){Cursor.lockState=on?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!on;}
        void Start()
        {
            var args=System.Environment.GetCommandLineArgs();
            int fullScreenArg=System.Array.IndexOf(args,"-screen-fullscreen");
            bool explicitlyWindowed=fullScreenArg>=0&&fullScreenArg+1<args.Length&&args[fullScreenArg+1]=="0";
            if(!Application.isEditor&&!explicitlyWindowed)
                Screen.SetResolution(Display.main.systemWidth,Display.main.systemHeight,FullScreenMode.FullScreenWindow);
            cameraDistance=Mathf.Min(cameraDistance,5f);Application.runInBackground=true;QualitySettings.vSyncCount=1;
            body=walker.GetComponent<CharacterController>();animator=avatar?avatar.GetComponentInChildren<Animator>():null;
            if(System.Array.IndexOf(args,"-start-bay")>=0)WalkAt(harbourPoint);else if(startWithPlayer)WalkAt(spawn);else Picture();
        }
        bool Hit(Key key)=>Keyboard.current!=null&&Keyboard.current[key].wasPressedThisFrame;
        bool Held(Key key)=>Keyboard.current!=null&&Keyboard.current[key].isPressed;
        public void Picture()
        {
            flying=false;photograph=true;paused=false;view.fieldOfView=48;view.transform.position=picturePosition;view.transform.LookAt(pictureTarget);var a=view.transform.eulerAngles;yaw=a.y;pitch=a.x>180?a.x-360:a.x;if(animator)animator.SetFloat("Speed",0);Lock(false);
        }
        public void WalkAt(Vector3 position)
        {
            if(!body)body=walker.GetComponent<CharacterController>();body.enabled=false;walker.position=position;walker.gameObject.SetActive(true);body.enabled=true;vertical=0;flying=false;photograph=false;paused=false;Lock(true);var toTarget=lookTarget-position;yaw=Mathf.Atan2(toTarget.x,toTarget.z)*Mathf.Rad2Deg;pitch=5;view.fieldOfView=58;if(avatar)avatar.rotation=Quaternion.Euler(0,yaw,0);FollowCamera(true);
        }
        public void Orbit(float horizontal,float verticalAngle){yaw+=horizontal;pitch=Mathf.Clamp(pitch+verticalAngle,-32,75);}
        public void RefreshCamera()=>FollowCamera(true);
        void Update()
        {
            if(Hit(Key.H))showHelp=!showHelp;
            if(Hit(Key.P))Picture();
            if(Hit(Key.Escape)){paused=!paused;Lock(!paused&&!photograph);}
            if(Hit(Key.F)){flying=!flying;photograph=false;paused=false;var a=view.transform.eulerAngles;yaw=a.y;pitch=a.x>180?a.x-360:a.x;}
            if(Hit(Key.Enter)) {flying=false;photograph=false;walker.gameObject.SetActive(true);paused=false;view.fieldOfView=58;Lock(true);}
            if(Hit(Key.R))WalkAt(spawn);
            if(Hit(Key.Digit1))WalkAt(forestPoint);
            if(Hit(Key.Digit2))WalkAt(blossomPoint);
            if(Hit(Key.Digit3))WalkAt(cottagePoint);
            if(Hit(Key.Digit4))WalkAt(waterfallPoint);
            if(Hit(Key.Digit5))WalkAt(harbourPoint);
            if(Hit(Key.Digit6))WalkAt(autumnPoint);
            if(Hit(Key.Digit7))WalkAt(retreatPoint);
            if(Hit(Key.Digit8)){WalkAt(headlandPoint);Orbit(45-yaw,0);RefreshCamera();}
            if(paused)return;
            if(Mouse.current!=null&&!photograph&&Cursor.lockState!=CursorLockMode.Locked&&Mouse.current.leftButton.wasPressedThisFrame)Lock(true);
            if(Mouse.current!=null&&(Cursor.lockState==CursorLockMode.Locked||Mouse.current.rightButton.isPressed))
            {
                var d=Mouse.current.delta.ReadValue();Orbit(d.x*.09f,-d.y*.07f);
            }
            if(Mouse.current!=null)cameraDistance=Mathf.Clamp(cameraDistance-Mouse.current.scroll.ReadValue().y*.006f,2.5f,15);
            var direction=new Vector3((Held(Key.D)?1:0)-(Held(Key.A)?1:0),0,(Held(Key.W)?1:0)-(Held(Key.S)?1:0));direction=Vector3.ClampMagnitude(direction,1);
            float dt=Mathf.Min(.05f,Time.deltaTime),speed=Held(Key.LeftShift)?55:16;
            if(flying){view.transform.rotation=Quaternion.Euler(pitch,yaw,0);direction.y=(Held(Key.E)?1:0)-(Held(Key.Q)?1:0);view.transform.position+=view.transform.rotation*direction*speed*dt;if(animator)animator.SetFloat("Speed",0);}
            else if(!photograph&&walker.gameObject.activeSelf)
            {
                if(body.isGrounded&&vertical<0)vertical=-2;if(body.isGrounded&&Hit(Key.Space))vertical=8;vertical-=22*dt;
                Vector3 move=Quaternion.Euler(0,yaw,0)*direction;body.Move((move*(Held(Key.LeftShift)?10:5)+Vector3.up*vertical)*dt);
                if(avatar&&move.sqrMagnitude>.01f)avatar.rotation=Quaternion.Slerp(avatar.rotation,Quaternion.LookRotation(move),dt*12);
                if(animator){animator.SetFloat("Speed",direction.magnitude*(Held(Key.LeftShift)?1:.5f));animator.speed=Held(Key.LeftShift)?1.1f:.8f;}
                if(walker.position.y<-15)WalkAt(spawn);
            }
        }
        void LateUpdate(){if(!paused&&!flying&&!photograph&&walker.gameObject.activeSelf)FollowCamera(false);}
        void FollowCamera(bool immediate)
        {
            var rotation=Quaternion.Euler(pitch,yaw,0);var target=walker.position+Vector3.up*1.75f;var offset=-(rotation*Vector3.forward)*cameraDistance;
            if(Physics.SphereCast(target,.22f,offset.normalized,out var hit,cameraDistance,1<<0,QueryTriggerInteraction.Ignore))offset=offset.normalized*Mathf.Max(1,hit.distance-.2f);
            view.transform.position=immediate?target+offset:Vector3.Lerp(view.transform.position,target+offset,1-Mathf.Exp(-Time.deltaTime*16));view.transform.rotation=rotation;
        }
        void OnGUI()
        {
            if(!showHelp)return;
            GUI.color=new Color(.04f,.11f,.14f,.86f);GUI.DrawTexture(new Rect(18,Screen.height-52,Mathf.Min(1080,Screen.width-36),34),Texture2D.whiteTexture);
            GUI.color=new Color(1,.97f,.87f);GUI.Label(new Rect(30,Screen.height-47,1080,28),"WASD / Shift run / Space jump   Mouse / Wheel zoom / Esc cursor   F fly   1 forest  2 blossoms  3 cottage  4 falls  5 bay  6 autumn  7 river  8 sea tree   P vista   H hide");
        }
    }
}
