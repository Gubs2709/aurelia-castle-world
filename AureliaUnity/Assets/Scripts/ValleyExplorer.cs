using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Aurelia
{
    // Local, offline world inspection. No progression or service dependencies.
    public class ValleyExplorer : MonoBehaviour
    {
        public Transform avatar;
        public Camera view;
        public Light sun;
        public Material sky;
        public Vector3 spawn = new Vector3(0, 3, -175);
        public bool freeFlight;
        public float yaw, pitch = 4, distance = 7;
        CharacterController body;
        Animation clips;
        Animator animator;
        float vertical, clock = .32f;
        bool paused, map, photo, ready;
        Vector3 cameraVelocity;
        GUIStyle title, label, small, button;
        Texture2D panel;
        float smoothDt;
        public float FramesPerSecond => 1f / Mathf.Max(.001f, smoothDt);
        public bool IsPaused => paused;
        void Start()
        {
            body = GetComponent<CharacterController>();
            clips = GetComponentInChildren<Animation>();
            animator = GetComponentInChildren<Animator>();
            if (clips) foreach (AnimationState s in clips) s.wrapMode = WrapMode.Loop;
            ready = true;
            Cursor.lockState = CursorLockMode.Locked; Cursor.visible = false;
            QualitySettings.vSyncCount = 1;
            Application.runInBackground = true;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        bool Press(Key k) => Keyboard.current != null && Keyboard.current[k].wasPressedThisFrame;
        bool Held(Key k) => Keyboard.current != null && Keyboard.current[k].isPressed;
        public void Teleport(Vector3 p)
        {
            if (body) body.enabled = false;
            transform.position = p;
            if (body) body.enabled = !freeFlight;
            vertical = 0;
        }
        void Update()
        {
            smoothDt = Mathf.Lerp(smoothDt, Time.unscaledDeltaTime, .025f);
            if (Press(Key.Escape)) SetPaused(!paused);
            if (paused) return;
            if (Press(Key.Tab)) map = !map;
            if (Press(Key.H)) photo = !photo;
            if (Press(Key.F)) { freeFlight = !freeFlight; body.enabled = !freeFlight; vertical = 0; }
            if (Press(Key.R)) { freeFlight = false; body.enabled = true; Teleport(spawn); }
            if (Press(Key.T)) clock = Mathf.Repeat(clock + .22f, 1);
            if (Press(Key.Digit1)) Visit(0);
            if (Press(Key.Digit2)) Visit(1);
            if (Press(Key.Digit3)) Visit(2);
            if (Press(Key.Digit4)) Visit(3);
            if (Mouse.current != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 delta = Mouse.current.delta.ReadValue();
                yaw += delta.x * .1f; pitch = Mathf.Clamp(pitch - delta.y * .085f, -35, 75);
                distance = Mathf.Clamp(distance - Mouse.current.scroll.ReadValue().y * .006f, 3, 18);
            }
            var forward = Quaternion.Euler(0, yaw, 0) * Vector3.forward;
            var right = Quaternion.Euler(0, yaw, 0) * Vector3.right;
            Vector3 direction = forward * ((Held(Key.W) || Held(Key.UpArrow) ? 1 : 0) - (Held(Key.S) || Held(Key.DownArrow) ? 1 : 0))
                + right * ((Held(Key.D) || Held(Key.RightArrow) ? 1 : 0) - (Held(Key.A) || Held(Key.LeftArrow) ? 1 : 0));
            direction = Vector3.ClampMagnitude(direction, 1);
            float dt = Mathf.Min(Time.deltaTime, .05f);
            bool sprint = Held(Key.LeftShift) || Held(Key.RightShift);
            if (freeFlight)
            {
                direction += Vector3.up * ((Held(Key.E) ? 1 : 0) - (Held(Key.Q) ? 1 : 0));
                transform.position += direction * (sprint ? 65 : 20) * dt;
            }
            else
            {
                if (body.isGrounded && vertical < 0) vertical = -2;
                if (body.isGrounded && Press(Key.Space)) vertical = 8;
                vertical -= 22 * dt;
                body.Move((direction * (sprint ? 10 : 5.3f) + Vector3.up * vertical) * dt);
                if (transform.position.y < -15 || Mathf.Abs(transform.position.x)>390 || Mathf.Abs(transform.position.z)>390) Teleport(spawn);
            }
            if (avatar)
            {
                avatar.gameObject.SetActive(!freeFlight);
                if (direction.sqrMagnitude > .01f) avatar.rotation = Quaternion.Slerp(avatar.rotation, Quaternion.LookRotation(direction), dt * 10);
                if (clips)
                {
                    var clipName = direction.sqrMagnitude > .01f ? "Run" : "Idle";
                    if (clips[clipName]) { clips[clipName].speed = sprint ? 1.1f : .65f; clips.CrossFade(clipName, .2f); }
                }
                if(animator){animator.SetFloat("Speed",direction.magnitude);animator.speed=sprint?1.15f:direction.magnitude>.01f?.7f:1;}
            }
            UpdateAtmosphere();
        }
        void LateUpdate()
        {
            if (!ready || paused) return;
            Quaternion rotation = Quaternion.Euler(pitch, yaw, 0);
            Vector3 target = transform.position + Vector3.up * 1.65f;
            Vector3 goal = target - rotation * Vector3.forward * (freeFlight ? 0 : distance);
            if (!freeFlight && Physics.SphereCast(target, .22f, (goal - target).normalized, out RaycastHit hit, distance, 1 << 0, QueryTriggerInteraction.Ignore))
                goal = target + (goal - target).normalized * Mathf.Max(.5f, hit.distance - .2f);
            view.transform.SetPositionAndRotation(Vector3.SmoothDamp(view.transform.position, goal, ref cameraVelocity, .055f), rotation);
        }
        public void SetPaused(bool value)
        {
            paused = value; Cursor.lockState = value ? CursorLockMode.None : CursorLockMode.Locked; Cursor.visible = value;
        }
        public void Visit(int index)
        {
            freeFlight = false; body.enabled = true;
            Vector3[] points = { spawn, new Vector3(0, 18, 38), new Vector3(111, 12, -48), new Vector3(0, 36, 158) };
            Teleport(points[Mathf.Clamp(index, 0, 3)]); yaw = index == 2 ? 35 : 0; pitch = 12;
        }
        public void UpdateAtmosphere()
        {
            float dusk = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(.45f, .85f, clock));
            sun.transform.rotation = Quaternion.Euler(Mathf.Lerp(43, 5, dusk), -32, 0);
            sun.color = Color.Lerp(new Color(1, .9f, .73f), new Color(1, .38f, .19f), dusk);
            sun.intensity = Mathf.Lerp(2.3f, .7f, dusk);
            RenderSettings.ambientSkyColor = Color.Lerp(new Color(.47f, .65f, .78f), new Color(.21f, .24f, .43f), dusk);
            RenderSettings.fogColor = Color.Lerp(new Color(.55f, .73f, .78f), new Color(.43f, .34f, .47f), dusk);
            if (sky) { sky.SetColor("_SkyTint", Color.Lerp(new Color(.46f, .65f, .77f), new Color(.57f, .27f, .32f), dusk)); sky.SetFloat("_Exposure", Mathf.Lerp(1.05f, .6f, dusk)); }
        }
        void Styles()
        {
            if (title != null) return;
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            title = new GUIStyle { font = font, fontSize = 28, normal = { textColor = new Color(.97f, .91f, .75f) }, fontStyle = FontStyle.Bold };
            label = new GUIStyle(title) { fontSize = 15, fontStyle = FontStyle.Normal, wordWrap = true };
            small = new GUIStyle(label) { fontSize = 11, normal = { textColor = new Color(.78f, .84f, .8f) } };
            button = new GUIStyle(GUI.skin.button) { font = font, fontSize = 16, fixedHeight = 40 };
            panel = new Texture2D(1, 1); panel.SetPixel(0, 0, new Color(.025f, .07f, .075f, .82f)); panel.Apply();
        }
        void OnGUI()
        {
            Styles();
            var scale = Mathf.Max(.65f, Screen.height / 900f);
            GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one * scale);
            float w = Screen.width / scale, h = Screen.height / scale;
            if (!photo || paused)
            {
                GUI.Label(new Rect(34, 28, 600, 45), "A U R E L I A", title);
                GUI.Label(new Rect(36, 66, 600, 30), "THE KINGDOM BEYOND THE FALLS", small);
                GUI.Label(new Rect(36, h - 79, 1000, 25), "WASD  move     SHIFT  sprint     SPACE  jump     MOUSE  look     WHEEL  zoom", label);
                GUI.Label(new Rect(36, h - 49, 1200, 25), "F  free flight [Q/E down/up]     T  atmosphere     TAB  places     H  hide interface     ESC  pause", small);
                GUI.Label(new Rect(w - 210, 34, 190, 25), $"{(freeFlight ? "FREE CAMERA" : "EXPLORING")}  /  {FramesPerSecond:0} FPS", small);
                GUI.Label(new Rect(w - 330, h - 58, 290, 30), Location(), label);
            }
            if (map && !paused)
            {
                GUI.DrawTexture(new Rect(w - 320, 100, 285, 225), panel);
                GUI.Label(new Rect(w - 298, 120, 260, 30), "PLACES TO WANDER", label);
                GUI.Label(new Rect(w - 298, 160, 260, 150), "1   Lantern village\n\n2   The waterfall viaduct\n\n3   Whispering Grove\n\n4   Castle gardens", label);
            }
            if (paused)
            {
                GUI.DrawTexture(new Rect(0, 0, w, h), panel);
                float x = w / 2 - 200, y = h / 2 - 210;
                GUI.Label(new Rect(x, y, 420, 65), "A moment of quiet.", title);
                GUI.Label(new Rect(x, y + 55, 410, 65), "Wander the kingdom at your own pace.\nThe world is the destination.", label);
                if (GUI.Button(new Rect(x, y + 135, 400, 40), "Return to the world", button)) SetPaused(false);
                if (GUI.Button(new Rect(x, y + 188, 400, 40), "Return to the village", button)) { Visit(0); SetPaused(false); }
                if (GUI.Button(new Rect(x, y + 241, 400, 40), "Change light: day / golden hour / dusk", button)) { clock = Mathf.Repeat(clock + .22f, 1); UpdateAtmosphere(); }
                if (GUI.Button(new Rect(x, y + 294, 400, 40), "Quit", button)) { Application.Quit(); }
            }
        }
        string Location()
        {
            var p = transform.position;
            if (p.z > 130) return "Aurelia  /  The crown gardens";
            if (p.x > 75 && p.z < 0) return "Whispering Grove";
            if (p.z > -5 && p.z < 130) return "The waterfall viaduct";
            if (p.z > -160 && p.z < -55) return "Briarwood  /  Lantern quarter";
            return "The southern wildflowers";
        }
    }
}
