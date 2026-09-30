using System;
using System.Collections.Generic;
using UnityEngine;
namespace AreaBattle
{
    // GameControl.ChangeSceneStyle f6059, helper f12185 and callback f15203.
    public sealed class OutgameSceneStyle
    {
        [Serializable] sealed class Rows {public Row[] Datas;}
        [Serializable] sealed class Row {public int id;public string idleIconName;}
        readonly Dictionary<int,string> textureNames=new Dictionary<int,string>();
        readonly Texture[] textures;readonly Renderer background;
        readonly Action<string,Action<Texture>> load;
        readonly Action<int> hideEffect,notifyShop;
        readonly Action<int,Transform> showEffect;
        public int CurrentId {get;private set;}=-1;
        public OutgameSceneStyle(string config,Renderer background,Action<string,Action<Texture>> load,Action<int> hideEffect,Action<int,Transform> showEffect,Action<int> notifyShop)
        {
            this.background=background!=null?background:throw new ArgumentNullException(nameof(background));
            this.load=load??throw new ArgumentNullException(nameof(load));this.hideEffect=hideEffect??throw new ArgumentNullException(nameof(hideEffect));
            this.showEffect=showEffect??throw new ArgumentNullException(nameof(showEffect));this.notifyShop=notifyShop??throw new ArgumentNullException(nameof(notifyShop));
            var rows=JsonUtility.FromJson<Rows>(config).Datas;textures=new Texture[rows.Length];foreach(var row in rows)textureNames.Add(row.id,row.idleIconName);
        }
        public void Change(int id,bool suppressShopRefresh)
        {
            if(CurrentId!=id)
            {
                hideEffect(CurrentId);CurrentId=id;int index=id-1;
                if(textures[index]==null)
                    load(textureNames[id],texture=>{textures[index]=texture;Apply(texture);});
                else Apply(textures[index]);
            }
            if(!suppressShopRefresh)notifyShop(id);
        }
        void Apply(Texture texture)
        {
            background.sharedMaterial.mainTexture=texture;
            // Source effect helper reads current selection at callback time, not captured texture ID.
            showEffect(CurrentId,background.transform);
        }
    }
}
