using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // Shared PlayerControl source fields12,16,20,24,28,32..112. Type4462 methods34196/34198.
    public sealed class OutgameModelRotation
    {
        readonly IReadOnlyDictionary<int,Transform> roots;
        public Vector3 Left,Middle,Right,LeftScale,MiddleScale,RightScale;
        public int SelectedType=3;
        public bool IsFingerMove,PositiveDirection;
        public float BeginX,DragDistance,DragLimit=200f,Amount;
        public OutgameModelRotation(IReadOnlyDictionary<int,Transform> roots){this.roots=roots;}
        public OutgameModelRotation(OutgameModelRoots source):this(source.Categories()){Capture(source.Normal,source.Attack,source.Defense);}
        public void Capture(Transform left,Transform middle,Transform right)
        {Left=left.localPosition;Middle=middle.localPosition;Right=right.localPosition;LeftScale=left.localScale;MiddleScale=middle.localScale;RightScale=right.localScale;}
        public static int Wrap(int value)=>value==4?1:value==0?3:value;
        void Place(Transform current,Transform next,Transform previous,bool positive)
        {
            current.localPosition=positive?Right:Left;next.localPosition=positive?Middle:Right;previous.localPosition=positive?Left:Middle;
            current.localScale=positive?RightScale:LeftScale;next.localScale=positive?MiddleScale:RightScale;previous.localScale=positive?LeftScale:MiddleScale;
        }
        public void Select(int type)
        {
            if(type==SelectedType)return;
            int next=Wrap(unchecked(SelectedType+1)),previous=Wrap(unchecked(SelectedType-1));
            if(type==next)
            {
                roots[SelectedType].localPosition=Right;roots[next].localPosition=Middle;roots[previous].localPosition=Left;
                roots[SelectedType].localScale=RightScale;roots[next].localScale=MiddleScale;roots[previous].localScale=LeftScale;SelectedType=next;
            }
            else if(type==previous)
            {
                roots[SelectedType].localPosition=Left;roots[next].localPosition=Right;roots[previous].localPosition=Middle;
                roots[SelectedType].localScale=LeftScale;roots[next].localScale=RightScale;roots[previous].localScale=MiddleScale;SelectedType=previous;
            }
        }
        public void UpdateDrag(IOutgamePlayerInput input,Action<int> chosen)
        {
            if(!roots[SelectedType].gameObject.activeInHierarchy)return;
            if(input.MouseDown&&input.MousePosition.y>input.ScreenHeight/2){IsFingerMove=true;Amount=0;BeginX=input.MousePosition.x;}
            var current=roots[SelectedType];var next=roots[Wrap(unchecked(SelectedType+1))];var previous=roots[Wrap(unchecked(SelectedType-1))];
            if(input.MouseHeld&&IsFingerMove)
            {
                DragDistance=input.MousePosition.x-BeginX;
                bool positive=DragDistance>0;
                if(positive){if(DragLimit<DragDistance)DragDistance=DragLimit;Amount=DragDistance/DragLimit;}
                else{if(-DragDistance>DragLimit)DragDistance=-DragLimit;Amount=-DragDistance/DragLimit;}
                current.localPosition=Vector3.Lerp(Middle,positive?Right:Left,Amount);
                next.localPosition=Vector3.Lerp(Left,positive?Middle:Right,Amount);
                previous.localPosition=Vector3.Lerp(Right,positive?Left:Middle,Amount);
                current.localScale=Vector3.Lerp(MiddleScale,positive?RightScale:LeftScale,Amount);
                next.localScale=Vector3.Lerp(LeftScale,positive?MiddleScale:RightScale,Amount);
                previous.localScale=Vector3.Lerp(RightScale,positive?LeftScale:MiddleScale,Amount);
                PositiveDirection=positive;
            }
            if(!input.MouseUp||!IsFingerMove)return;
            if(Amount>.3f)
            {
                Place(current,next,previous,PositiveDirection);
                SelectedType=Wrap(unchecked(SelectedType+(PositiveDirection?1:-1)));
                chosen(SelectedType);
            }
            else
            {
                current.localPosition=Middle;next.localPosition=Left;previous.localPosition=Right;
                current.localScale=MiddleScale;next.localScale=LeftScale;previous.localScale=RightScale;
            }
            IsFingerMove=false;
        }
    }
}
