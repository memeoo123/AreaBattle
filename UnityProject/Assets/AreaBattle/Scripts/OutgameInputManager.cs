using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    public struct OutgameTouch
    {
        public Vector2 Position;public TouchPhase Phase;
        public OutgameTouch(Vector2 position,TouchPhase phase){Position=position;Phase=phase;}
    }
    public sealed class OutgameMultiTouchData{public int id;public float x,y;}
    // Each property preserves the original independent Unity read. Test providers replace only device reads.
    public interface IOutgameInputSource
    {
        RuntimePlatform Platform{get;}bool MouseDown{get;}bool MouseUp{get;}bool MouseHeld{get;}Vector3 MousePosition{get;}
        float Scroll{get;}int TouchCount{get;}OutgameTouch FirstTouch{get;}OutgameTouch GetTouch(int index);Event CurrentEvent{get;}
    }
    public sealed class OutgameUnityInputSource:IOutgameInputSource
    {
        public RuntimePlatform Platform=>Application.platform;public bool MouseDown=>Input.GetMouseButtonDown(0);public bool MouseUp=>Input.GetMouseButtonUp(0);public bool MouseHeld=>Input.GetMouseButton(0);
        public Vector3 MousePosition=>Input.mousePosition;public float Scroll=>Input.GetAxis("Mouse ScrollWheel");public int TouchCount=>Input.touchCount;
        static OutgameTouch Convert(Touch t)=>new OutgameTouch(t.position,t.phase);
        public OutgameTouch FirstTouch=>Convert(Input.touches[0]);public OutgameTouch GetTouch(int i)=>Convert(Input.GetTouch(i));public Event CurrentEvent=>Event.current;
    }
    // Source GFRunning InputManager type3603. Update27771 / OnGUI27698 / singleton27760.
    public sealed class OutgameInputManager:MonoBehaviour
    {
        static OutgameInputManager instance;public static int MoveStep=50;
        public static OutgameInputManager Instance
        {
            get{
                if(instance==null){var go=GameObject.Find("InputManager");if(go==null)go=new GameObject("InputManager");instance=go.GetComponent<OutgameInputManager>();if(instance==null)instance=go.AddComponent<OutgameInputManager>();DontDestroyOnLoad(go);}return instance;
            }
        }
        public IOutgameInputSource Source=new OutgameUnityInputSource();
        public float MouseX{get;private set;}public float MouseY{get;private set;}public int MulTouchLen{get;private set;}
        public KeyCode HeldKey;public List<KeyCode> HeldKeys;public Vector3 PressPoint;
        public List<OutgameMultiTouchData> MultiTouches;public float PreviousX,PreviousY,LastScroll;
        // Source27697 logs before Delegate.Combine. CLog routing is supplied by the host.
        public Action<object[]> RegistrationLog=args=>Debug.Log(args[0]);
        Action<float,float> touchBegin;
        public event Action<float,float> TouchBegin{add{RegistrationLog(new object[]{"add touch begin handle:"});touchBegin+=value;}remove{touchBegin-=value;}}
        public event Action<float,float> TouchHold,TouchMove,TouchEnd,Click;
        public event Action<string> TouchDebug;
        public event Action<int,List<OutgameMultiTouchData>> MulTouchBegin,MulTouchHold,MulTouchMove;
        public event Action MulTouchEnd;public event Action<float> Scale;
        public event Action<KeyCode> KeyUp,KeyDown;public event Action<List<KeyCode>> KeyHold;
        void Update()=>CheckTouch();void OnGUI()=>CheckKeys();
        public void CheckTouch()
        {
            if(Source.Platform==RuntimePlatform.Android||Source.Platform==RuntimePlatform.IPhonePlayer||Source.Platform==RuntimePlatform.WebGLPlayer)GetTouchInput();else GetMouseInput();
        }
        // Source27761: Down > Up > Held, exact XY movement; Click precedes End only for mouse.
        public void GetMouseInput()
        {
            if(Source.MouseDown){MouseX=Source.MousePosition.x;MouseY=Source.MousePosition.y;PreviousY=MouseY;PreviousX=MouseX;PressPoint=Source.MousePosition;touchBegin?.Invoke(MouseX,MouseY);}
            else if(Source.MouseUp){MouseX=Source.MousePosition.x;MouseY=Source.MousePosition.y;var press=PressPoint;var current=Source.MousePosition;if(Distance(press.x,press.y,current.x,current.y)<MoveStep)Click?.Invoke(MouseX,MouseY);PreviousX=MouseX;PreviousY=MouseY;TouchEnd?.Invoke(MouseX,MouseY);}
            else if(Source.MouseHeld){PreviousX=MouseX;PreviousY=MouseY;MouseX=Source.MousePosition.x;MouseY=Source.MousePosition.y;if(PreviousX==MouseX&&PreviousY==MouseY)TouchHold?.Invoke(MouseX,MouseY);else TouchMove?.Invoke(MouseX,MouseY);}
            float scroll=Source.Scroll;if(scroll!=0){LastScroll=scroll;if(LastScroll!=0)Scale?.Invoke(LastScroll);}
        }
        static float Distance(float ax,float ay,float bx,float by){float x=ax-bx,y=ay-by;return Mathf.Sqrt(x*x+y*y);}
        // Source27752: single-touch End includes Canceled; no synthetic Begin for multiple touches.
        public void GetTouchInput()
        {
            int count=Source.TouchCount;
            if(count==1){MulTouchLen=1;var t=Source.FirstTouch;MouseX=t.Position.x;MouseY=t.Position.y;
                if(t.Phase==TouchPhase.Began){PressPoint=new Vector3(t.Position.x,t.Position.y,0);touchBegin?.Invoke(MouseX,MouseY);}
                else if(t.Phase==TouchPhase.Ended||t.Phase==TouchPhase.Canceled){TouchEnd?.Invoke(MouseX,MouseY);if(Distance(PressPoint.x,PressPoint.y,t.Position.x,t.Position.y)<MoveStep)Click?.Invoke(MouseX,MouseY);}
                else if(t.Phase==TouchPhase.Moved)TouchMove?.Invoke(MouseX,MouseY);
                else if(t.Phase==TouchPhase.Stationary)TouchHold?.Invoke(MouseX,MouseY);
                return;
            }
            if(count>=2){MulTouchLen=0;var previous=MultiTouches;MultiTouches=new List<OutgameMultiTouchData>();bool moved=false;
                for(int i=0;i<count;i++){var t=Source.GetTouch(i);if(t.Phase==TouchPhase.Ended||t.Phase==TouchPhase.Canceled)continue;MulTouchLen++;MultiTouches.Add(new OutgameMultiTouchData{id=i,x=t.Position.x,y=t.Position.y});if(t.Phase==TouchPhase.Moved)moved=true;}
                if(MulTouchLen<2)return;
                if(!moved){if(MultiTouches!=null)MulTouchHold?.Invoke(MulTouchLen,MultiTouches);return;}
                if(MultiTouches!=null)MulTouchMove?.Invoke(MulTouchLen,MultiTouches);
                if(Scale==null||previous==null||previous.Count<2)return;
                float distance=Distance(MultiTouches[0].x,MultiTouches[0].y,MultiTouches[1].x,MultiTouches[1].y);
                LastScroll=(distance-Distance(previous[0].x,previous[0].y,previous[1].x,previous[1].y))/50f;
                if(LastScroll!=0)Scale?.Invoke(LastScroll);return;
            }
            if(MulTouchLen<1)return;MulTouchLen=0;MulTouchEnd?.Invoke();
        }
        // Source27750/27742: dispatch live list; duplicate KeyDown still reaches KeyHold.
        public void CheckKeys()
        {
            if(HeldKeys==null)HeldKeys=new List<KeyCode>();var e=Source.CurrentEvent;if(e==null||!e.isKey)return;
            var key=e.keyCode;if(key!=KeyCode.None){HeldKey=key;
                if(e.type==EventType.KeyDown){if(HeldKeys.IndexOf(HeldKey)<0){HeldKeys.Add(HeldKey);KeyDown?.Invoke(HeldKey);}}
                else if(e.type==EventType.KeyUp){KeyUp?.Invoke(HeldKey);int index=HeldKeys.IndexOf(HeldKey);if(index>=0)HeldKeys.RemoveAt(index);}
            }
            if(HeldKeys.Count>=1&&KeyHold!=null&&HeldKeys!=null&&HeldKeys.Count>=1)KeyHold(HeldKeys);
        }
        // Source27712 clears delegates only: singleton, touch history and held keys survive.
        public void Shutdown(){touchBegin=null;TouchHold=null;TouchMove=null;TouchEnd=null;Click=null;TouchDebug=null;MulTouchBegin=null;MulTouchHold=null;MulTouchMove=null;MulTouchEnd=null;Scale=null;KeyUp=null;KeyHold=null;KeyDown=null;}
        public static void AddTouchBeginListener(Action<float,float> callback)=>Instance.TouchBegin+=callback;
        public static void RemoveTouchBeginListener(Action<float,float> callback)=>Instance.TouchBegin-=callback;
        public static void AddTouchHoldListener(Action<float,float> callback)=>Instance.TouchHold+=callback;
        public static void RemoveTouchHoldListener(Action<float,float> callback)=>Instance.TouchHold-=callback;
        public static void AddTouchMoveListener(Action<float,float> callback)=>Instance.TouchMove+=callback;
        public static void RemoveTouchMoveListener(Action<float,float> callback)=>Instance.TouchMove-=callback;
        public static void AddTouchEndListener(Action<float,float> callback)=>Instance.TouchEnd+=callback;
        public static void RemoveTouchEndListener(Action<float,float> callback)=>Instance.TouchEnd-=callback;
        public static void AddClickListener(Action<float,float> callback)=>Instance.Click+=callback;
        public static void RemoveClickListener(Action<float,float> callback)=>Instance.Click-=callback;
        public static void AddTouchDebugListener(Action<string> callback)=>Instance.TouchDebug+=callback;
        public static void RemoveTouchDebugListener(Action<string> callback)=>Instance.TouchDebug-=callback;
        public static void AddMulTouchBeginListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchBegin+=callback;
        public static void RemoveMulTouchBeginListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchBegin-=callback;
        public static void AddMulTouchHoldListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchHold+=callback;
        public static void RemoveMulTouchHoldListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchHold-=callback;
        public static void AddMulTouchMoveListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchMove+=callback;
        public static void RemoveMulTouchMoveListener(Action<int,List<OutgameMultiTouchData>> callback)=>Instance.MulTouchMove-=callback;
        public static void AddMulTouchEndListener(Action callback)=>Instance.MulTouchEnd+=callback;
        public static void RemoveMulTouchEndListener(Action callback)=>Instance.MulTouchEnd-=callback;
        public static void AddScaleListener(Action<float> callback)=>Instance.Scale+=callback;
        public static void RemoveScaleListener(Action<float> callback)=>Instance.Scale-=callback;
        public static void AddKeyUpListener(Action<KeyCode> callback)=>Instance.KeyUp+=callback;
        public static void RemoveKeyUpListener(Action<KeyCode> callback)=>Instance.KeyUp-=callback;
        public static void AddKeyHoldListener(Action<List<KeyCode>> callback)=>Instance.KeyHold+=callback;
        public static void RemoveKeyHoldListener(Action<List<KeyCode>> callback)=>Instance.KeyHold-=callback;
        public static void AddKeyDownListener(Action<KeyCode> callback)=>Instance.KeyDown+=callback;
        public static void RemoveKeyDownListener(Action<KeyCode> callback)=>Instance.KeyDown-=callback;
    }
}
