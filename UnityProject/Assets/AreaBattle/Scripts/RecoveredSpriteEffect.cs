using UnityEngine;
namespace AreaBattle
{
    public sealed class RecoveredSpriteEffect : MonoBehaviour
    {
        public Sprite[] Frames;
        public float[] Times;
        public float ClipDuration,Lifetime;
        public bool Loop;
        float started;
        SpriteRenderer sprite;
        public void Begin(float clock){started=clock;sprite=GetComponent<SpriteRenderer>();}
        public bool Advance(float clock)
        {
            float elapsed=clock-started;
            if(elapsed>=Lifetime)return false;
            float time=Loop&&ClipDuration>0?elapsed%ClipDuration:elapsed;
            int index=0;for(int i=1;i<Times.Length&&Times[i]<=time;i++)index=i;
            sprite.sprite=Frames[index];return true;
        }
    }
}
