using System;
using System.Collections;
using System.Collections.Generic;
namespace AreaBattle
{
    public interface IOutgameWebRequestAgent
    {
        OutgameWebRequestTask Task {get;}
        float FreeWaitTimes {get;set;}
        void Initialize();
        int Start(OutgameWebRequestTask task);
        void Update(float deltaTime,float unscaledDeltaTime);
        void Reset();
        void Shutdown();
    }
    // GameFrameworkLinkedList3804 uses a per-list FIFO cache of cleared BCL nodes.
    // Retaining node identity matters when a task callback changes the queue during Update.
    public sealed class OutgameRequestLinkedList<T>:IEnumerable<T>
    {
        readonly LinkedList<T> list=new LinkedList<T>();
        readonly Queue<LinkedListNode<T>> cache=new Queue<LinkedListNode<T>>();
        public int Count=>list.Count;
        public LinkedListNode<T> First=>list.First;
        public LinkedListNode<T> Last=>list.Last;
        LinkedListNode<T> Acquire(T value){if(cache.Count==0)return new LinkedListNode<T>(value);var node=cache.Dequeue();node.Value=value;return node;}
        void Release(LinkedListNode<T> node){node.Value=default(T);cache.Enqueue(node);}
        public LinkedListNode<T> AddFirst(T value){var node=Acquire(value);list.AddFirst(node);return node;}
        public LinkedListNode<T> AddLast(T value){var node=Acquire(value);list.AddLast(node);return node;}
        public LinkedListNode<T> AddAfter(LinkedListNode<T> after,T value){var node=Acquire(value);list.AddAfter(after,node);return node;}
        public void Remove(LinkedListNode<T> node){list.Remove(node);Release(node);}
        public bool Remove(T value){var node=list.Find(value);if(node==null)return false;list.Remove(node);Release(node);return true;}
        public void RemoveFirst(){var node=list.First;if(node==null)throw new OutgameFrameworkException("First is invalid.");list.RemoveFirst();Release(node);}
        public void Clear(){for(var node=list.First;node!=null;node=node.Next)Release(node);list.Clear();}
        public IEnumerator<T> GetEnumerator()=>list.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator()=>GetEnumerator();
    }
    public readonly struct OutgameWebRequestTaskInfo
    {
        public readonly int SerialId,Priority,Status;
        public readonly string Description;
        public OutgameWebRequestTaskInfo(OutgameWebRequestTask task,int status){SerialId=task.SerialId;Priority=task.Priority;Status=status;Description=task.Description;}
    }
    // Concrete HTTP specialization of source generic task pool3812 (29776..29797).
    public sealed class OutgameWebRequestTaskPool
    {
        readonly OutgameRequestLinkedList<IOutgameWebRequestAgent> free=new OutgameRequestLinkedList<IOutgameWebRequestAgent>();
        readonly OutgameRequestLinkedList<IOutgameWebRequestAgent> working=new OutgameRequestLinkedList<IOutgameWebRequestAgent>();
        readonly OutgameRequestLinkedList<OutgameWebRequestTask> waiting=new OutgameRequestLinkedList<OutgameWebRequestTask>();
        public int Capacity {get;set;}=20;
        public bool Paused {get;set;}
        public int TotalAgentCount=>free.Count+working.Count;
        public int FreeAgentCount=>free.Count;
        public int WorkingAgentCount=>working.Count;
        public int WaitingTaskCount=>waiting.Count;
        public void AddAgent(IOutgameWebRequestAgent agent){if(agent==null)throw new OutgameFrameworkException("Task agent is invalid.");agent.Initialize();free.AddLast(agent);}
        public void AddTask(OutgameWebRequestTask task)
        {
            var node=waiting.Last;
            while(node!=null){if(task.Priority<=node.Value.Priority){waiting.AddAfter(node,task);return;}node=node.Previous;}
            waiting.AddFirst(task);
        }
        public bool HaveDownTask(int id)
        {
            foreach(var agent in working)if(agent.Task.SerialId==id)return true;
            foreach(var task in waiting)if(task.SerialId==id)return true;
            return false; // Source name means membership, not Done status.
        }
        public OutgameWebRequestTaskInfo[] GetAllTaskInfos()
        {
            var result=new List<OutgameWebRequestTaskInfo>();
            foreach(var agent in working){var task=agent.Task;result.Add(new OutgameWebRequestTaskInfo(task,task.Done?2:1));}
            foreach(var task in waiting)result.Add(new OutgameWebRequestTaskInfo(task,0));
            return result.ToArray();
        }
        public void ChangeTaskPriority(int id,int priority)
        {
            OutgameWebRequestTask found=null;foreach(var task in waiting)if(task.SerialId==id)found=task;
            if(found==null||found.Priority==priority)return;waiting.Remove(found);found.Priority=priority;AddTask(found);
        }
        void ReturnAgent(IOutgameWebRequestAgent agent){agent.FreeWaitTimes=0;free.AddLast(agent);}
        public bool RemoveTask(int id)
        {
            foreach(var task in waiting)if(task.SerialId==id){waiting.Remove(task);task.Clear();return true;}
            foreach(var agent in working)if(agent.Task.SerialId==id){var task=agent.Task;agent.Reset();ReturnAgent(agent);working.Remove(agent);task.Clear();return true;}
            return false;
        }
        public void RemoveAllTasks()
        {
            foreach(var task in waiting)task.Clear();waiting.Clear();
            foreach(var agent in working){var task=agent.Task;agent.Reset();ReturnAgent(agent);task.Clear();}working.Clear();
        }
        public void Shutdown(){RemoveAllTasks();while(FreeAgentCount>0){free.First.Value.Shutdown();free.RemoveFirst();}free.Clear();}
        public void Update(float deltaTime,float unscaledDeltaTime)
        {
            if(Paused)return;
            UpdateWorking(deltaTime,unscaledDeltaTime);
            StartWaiting();
            ExpireFree(deltaTime);
        }
        void UpdateWorking(float deltaTime,float unscaledDeltaTime)
        {
            var node=working.First;
            while(node!=null){
                var task=node.Value.Task;
                if(!task.Done){node.Value.Update(deltaTime,unscaledDeltaTime);node=node.Next;continue;}
                var next=node.Next;node.Value.Reset();ReturnAgent(node.Value);working.Remove(node);task.Clear();node=next;
            }
        }
        void StartWaiting()
        {
            var node=waiting.First;
            while(node!=null&&FreeAgentCount>0){
                var current=node;var agent=free.First.Value;free.RemoveFirst();var active=working.AddLast(agent);
                var task=current.Value;node=current.Next;int result=agent.Start(task);
                if(result!=1){
                    if(result<0||result>3)continue;
                    agent.Reset();ReturnAgent(agent);working.Remove(active);
                    if(result==2)continue;
                }
                waiting.Remove(current);
                if(result==0||result==3)task.Clear();
            }
        }
        void ExpireFree(float deltaTime)
        {
            var node=free.First;
            while(node!=null){var current=node;node=node.Next;var agent=current.Value;agent.FreeWaitTimes+=deltaTime;if(agent.FreeWaitTimes>=300){agent.Shutdown();free.Remove(current);}}
        }
    }
}
