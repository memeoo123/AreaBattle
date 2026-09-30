using System;
using System.IO;
using UnityEngine;

namespace AreaBattle
{
    // Explicit command-line validation mode; absent the flag, this component is never created.
    [DefaultExecutionOrder(100)]
    public sealed class BattlePlayerSmoke : MonoBehaviour
    {
        [Serializable] sealed class Result { public bool passed,retryTopologyPreserved,simulationReused;public int frames,spawnsBeforeRetry,spawnsAfterRetry,lineCount;public string error; }
        BattleView view;
        readonly Result result=new Result();
        string output;
        int frame,initialLines;
        void Start()
        {
            view=GetComponent<BattleView>();
            var args=Environment.GetCommandLineArgs();
            int index=Array.IndexOf(args,"-battle-smoke-report");
            if(index<0 || index+1>=args.Length){Debug.LogError("Smoke report path required");Application.Quit(2);return;}
            output=args[index+1];
            Time.captureDeltaTime=1f/60f;
            if(!view.Initialized){Finish("Player scene did not initialize");return;}
            initialLines=view.Simulation.Lines.Count;result.lineCount=initialLines;
            AttachAndConnect();
        }
        void AttachAndConnect(bool attach=true)
        {
            if(attach)view.Simulation.Event+=e=>{if(e.Kind=="spawn"){if(frame<300)result.spawnsBeforeRetry++;else result.spawnsAfterRetry++;}};
            view.Simulation.Connect(3,1);view.Simulation.Connect(3,2);view.Simulation.Connect(3,5);
        }
        void Update()
        {
            if(output==null)return;
            frame++;
            if(frame==300)
            {
                var before=view.Simulation;
                view.RestartCurrentLevel();result.simulationReused=ReferenceEquals(before,view.Simulation);
                result.retryTopologyPreserved=view.Initialized && view.Simulation.Lines.Count==initialLines;
                AttachAndConnect(false);
            }
            if(frame>=600)Finish(null);
        }
        void Finish(string error)
        {
            result.frames=frame;result.error=error;
            result.passed=error==null&&result.retryTopologyPreserved&&result.simulationReused&&result.spawnsBeforeRetry>0&&result.spawnsAfterRetry>0;
            if(output!=null){Directory.CreateDirectory(Path.GetDirectoryName(output));File.WriteAllText(output,JsonUtility.ToJson(result,true));}
            Debug.Log(result.passed?"AREABATTLE_PLAYER_SMOKE_PASS":"AREABATTLE_PLAYER_SMOKE_FAIL");
            Application.Quit(result.passed?0:1);
            enabled=false;
        }
    }
}
