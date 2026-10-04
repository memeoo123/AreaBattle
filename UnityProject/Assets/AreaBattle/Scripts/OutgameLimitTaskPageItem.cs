using System;
using UnityEngine;
using UnityEngine.UI;
namespace AreaBattle
{
    public sealed class OutgameLimitTaskPageItemServices
    {
        public Func<OutgameActivityControl> Activities;
        public Func<OutgameCommonMessageDispatcher> Common=()=>OutgameCommonMessageDispatcher.Shared;
        public Func<OutgameMessageDispatcher> Messages=()=>OutgameMessageDispatcher.Shared;
        public Func<string,object[],string> LanguageFormat;
    }
    // CommonLimitTimeTaskPageItem4338: DynamicBaseItem disposal keeps the pooled native object.
    public sealed class OutgameLimitTaskPageView:IOutgameDynamicItem,IOutgameDynamicRenderSelect
    {
        const string RefreshList="CommonModule_RefreshNoviceTaskList",RefreshExt="CommonModule_NoviceExtRefresh";
        public const string SelectPage="GlobalEvent.CommonGameModule.LimitTimeTask.SelectPage";
        readonly OutgameLimitTaskPageItemServices services;
        OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem> provider;
        OutgameLimitTimeTaskActivity parent;
        public OutgameChildLimitTimeTaskActivity Child {get;private set;}
        public OutgameLimitTaskPageItem Data {get;private set;}
        public OutgameUiLifetime Lifetime {get;private set;}
        public Button Normal {get;private set;}
        public Image Selected {get;private set;}
        public Image Locked {get;private set;}
        public GameObject Red {get;private set;}
        public Text NormalName {get;private set;}
        public Text SelectedName {get;private set;}
        public Text LockedName {get;private set;}
        public OutgameLimitTaskPageView(OutgameLimitTaskPageItemServices services)=>this.services=services;
        public void OnCreate(IOutgameDynamicListProvider data)=>provider=(OutgameDynamicListProviderSelection<OutgameLimitTaskPageItem>)data;
        public void InstantiateNoNewItem(GameObject root)
        {
            Lifetime=new OutgameUiLifetime(root,()=>{});
            T At<T>(string path)where T:Component=>root.transform.Find(path).GetComponent<T>();
            Normal=At<Button>("btnNormal");NormalName=At<Text>("btnNormal/nameNormal");
            Selected=At<Image>("btnSelect");SelectedName=At<Text>("btnSelect/nameSelect");
            Red=root.transform.Find("redPoint").gameObject;
            Locked=At<Image>("btnLock");LockedName=At<Text>("btnLock/nameLock");
            Normal.onClick.RemoveAllListeners();Normal.onClick.AddListener(Click);
            services.Common().RemoveListener(RefreshList,OnListRefresh);services.Common().RemoveListener(RefreshExt,OnExtRefresh);
            services.Common().AddListener(RefreshList,OnListRefresh);services.Common().AddListener(RefreshExt,OnExtRefresh);
            parent=services.Activities().GetActivity<OutgameLimitTimeTaskActivity>(1301001);
        }
        public void InitializeSkin(){}
        public void OnRenderer(int index)
        {
            Data=provider.GetData(index);Child=parent.GetChildActivity<OutgameChildLimitTimeTaskActivity>(Data.activityId);
            NormalName.text=services.LanguageFormat("CommonGameModule.CommonNoviceTaskUITemp.pageName",new object[]{Data.day});
            int day=Data.day,current=Child.ModuleData.ext.dayId;
            Normal.gameObject.SetActive(day<=current);Locked.gameObject.SetActive(day>current);Selected.gameObject.SetActive(false);
            string name=string.Format("{0}天",Data.day);
            NormalName.text=name;LockedName.text=name;SelectedName.text=name;RefreshRed();
        }
        void RefreshRed()=>Red.SetActive(Child.ExitRewardWaitGet(Data.day));
        void Click()=>provider.SetSelect(provider.IndexOf(Data));
        public void OnSelect()
        {
            Selected.gameObject.SetActive(true);Normal.gameObject.SetActive(false);Locked.gameObject.SetActive(false);
            services.Messages().SendMessage(SelectPage,new object[]{Data.activityId,Data.day});
            if(provider.Data.Count==1)Lifetime.GameObject.SetActive(false);
        }
        public void OnDeSelect(){Selected.gameObject.SetActive(false);Normal.gameObject.SetActive(true);Locked.gameObject.SetActive(false);}
        void OnListRefresh(object[] args)
        {
            if(Data==null||args==null||args.Length<1)return;
            object id=args[0];int day=(int)args[1];
            if(Data.activityId==(int)id&&Data.day==day)RefreshRed();
        }
        void OnExtRefresh(object[] args)
        {if(Data==null||args==null||args.Length<1)return;if(Data.activityId==(int)args[0])RefreshRed();}
        public void Dispose()
        {
            Lifetime.Dispose();
            services.Common().RemoveListener(RefreshList,OnListRefresh);services.Common().RemoveListener(RefreshExt,OnExtRefresh);
            Normal.onClick.RemoveAllListeners();
        }
    }
}
