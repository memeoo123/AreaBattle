using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
namespace AreaBattle
{
    public interface IOutgameSdkFunctionActions
    {
        void AdsVideoFunction();void ShowShareFunction();void ShowLoginFunction();
        void FeedbackFunction();void GDPRUserFunction();void ShowPolicyFunction();
        void ShowUserProtocolFunction();void ShowGameBanHaoFunction();void StartRestore();
        void ShowOppoGameCenter();void DrawVideo();void OpenPrivacyRecall();
    }
    // DBTSDKManager.InitSDKOpenFunction23906 / iterator23947. Dictionary is shared with button binding.
    public static class OutgameSdkFunctionInitialization
    {
        public static IEnumerator InitSDKOpenFunction(IDictionary<int,UnityAction> actions,IOutgameSdkFunctionActions owner)
        {
            yield return new WaitForEndOfFrame();
            actions.Add(5,owner.AdsVideoFunction);
            actions.Add(7,owner.ShowShareFunction);
            actions.Add(8,owner.ShowLoginFunction);
            actions.Add(9,owner.FeedbackFunction);
            actions.Add(10,owner.GDPRUserFunction);
            actions.Add(11,owner.ShowPolicyFunction);
            actions.Add(12,owner.ShowUserProtocolFunction);
            actions.Add(17,owner.ShowGameBanHaoFunction);
            actions.Add(19,owner.StartRestore);
            actions.Add(20,owner.ShowOppoGameCenter);
            actions.Add(21,owner.DrawVideo);
            actions.Add(22,owner.OpenPrivacyRecall);
            yield return new WaitForUpdate();
        }
        // Original WaitForUpdate Type2886: custom yield predicate is always false.
        sealed class WaitForUpdate:CustomYieldInstruction {public override bool keepWaiting=>false;}
    }
}
