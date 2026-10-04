using System;
namespace AreaBattle
{
    public sealed class OutgameSkinManager:IOutgameDataManager
    {
        readonly OutgameProfile profile;
        readonly string soldiers,scenes;
        readonly OutgameDataManagerStorage storage;
        readonly IOutgameDataStorageHost host;
        readonly Action<string> requestDownload;
        readonly Action<OutgameSkinManager> setInstance;
        public OutgameSkinCatalog Catalog {get;private set;}
        public string DataKey=>"SkinManager";
        public bool ParticipatesInSync {get=>storage.AutoSyn;set=>storage.AutoSyn=value;}
        public bool CompressData {get=>storage.CompressData;set=>storage.CompressData=value;}
        public OutgameSkinManager(OutgameProfile profile,string soldiers,string scenes,OutgameDataManagerStorage storage,IOutgameDataStorageHost host,Action<string> requestDownload,Action<OutgameSkinManager> setInstance)
        {this.profile=profile??throw new ArgumentNullException(nameof(profile));this.soldiers=soldiers;this.scenes=scenes;this.storage=storage??throw new ArgumentNullException(nameof(storage));this.host=host??throw new ArgumentNullException(nameof(host));this.requestDownload=requestDownload??throw new ArgumentNullException(nameof(requestDownload));this.setInstance=setInstance??throw new ArgumentNullException(nameof(setInstance));}
        public void OnInit(){setInstance(this);UpdateData(true);}
        public void UpdateData(bool allowServer)
        {
            if(allowServer&&host.IsUseServer&&ParticipatesInSync){requestDownload(DataKey);return;}
            UpdateDataCallBack(storage.ReadLocalData());
        }
        public void UpdateDataCallBack(string text)
        {Catalog=OutgameSkinCatalog.FromOriginal(text,soldiers,scenes);profile.skins=Catalog.State;}
        // Source OnRelease31631/31731 is empty.
        public void OnRelease(){}
        public void OnSave()=>storage.SaveLocalData(Catalog.ToOriginalJson());
        public void ApplyLegacy(string oldLocalData)=>Catalog.ApplyLegacy(oldLocalData);
    }
}
