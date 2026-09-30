using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
namespace AreaBattle
{
    public interface IOutgameLegacyDownload
    {
        bool IsDone {get;}bool NetworkError {get;}bool HttpError {get;}
        string Error {get;}AssetBundle GetContent();
    }
    public sealed class OutgameLegacyWebDownload:IOutgameLegacyDownload
    {
        public readonly UnityWebRequest Request;
        public OutgameLegacyWebDownload(UnityWebRequest request){Request=request;}
        public bool IsDone=>Request.isDone;
#pragma warning disable 0618
        public bool NetworkError=>Request.isNetworkError;
        public bool HttpError=>Request.isHttpError;
#pragma warning restore 0618
        public string Error=>Request.error;
        public AssetBundle GetContent()=>DownloadHandlerAssetBundle.GetContent(Request);
    }
    // AssetBundleManager.Update23843 download and operation phases. Final manager cleanup remains explicit.
    public sealed class OutgameLegacyDownloadUpdate
    {
        readonly IDictionary<string,IOutgameLegacyDownload> downloading;
        readonly IDictionary<string,OutgameLegacyBundleResult> loaded;readonly IDictionary<string,string> errors;
        readonly Func<float> clock;readonly Action<string> log;readonly Action updateOperations,cleanup;
        public OutgameLegacyDownloadUpdate(IDictionary<string,IOutgameLegacyDownload> downloading,
            IDictionary<string,OutgameLegacyBundleResult> loaded,IDictionary<string,string> errors,
            Func<float> clock,Action<string> log,Action updateOperations,Action cleanup)
        {this.downloading=downloading;this.loaded=loaded;this.errors=errors;this.clock=clock;this.log=log;this.updateOperations=updateOperations;this.cleanup=cleanup;}
        public void Update()
        {
            var finished=new List<string>();
            foreach(var pair in downloading)
            {
                var request=pair.Value;if(!request.IsDone)continue;
                if(request.NetworkError||request.HttpError)
                {
                    log("WebGl加载资源错误"+request.Error);
                    if(!errors.ContainsKey(pair.Key))errors.Add(pair.Key,string.Format("{0} is not a valid asset bundle.",pair.Key));
                    finished.Add(pair.Key);continue;
                }
                var bundle=request.GetContent();
                if(!loaded.ContainsKey(pair.Key))loaded.Add(pair.Key,new OutgameLegacyBundleResult{Bundle=bundle,ReferenceCount=1,BundleName=pair.Key,LoadedAt=clock()});
                finished.Add(pair.Key);
            }
            foreach(var name in finished)downloading.Remove(name);
            updateOperations();cleanup();
        }
    }
}
