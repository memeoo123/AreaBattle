using System;
using UnityEngine;
namespace AreaBattle
{
    public static class OutgameMenuRedDotBinding
    {
        // Original MenuTabUI has one4457 component. Its Tower_CheckReddot persistent call has a null target,
        // so the serialized event exists but Unity cannot invoke that call. Do not invent an activity predicate.
        public static OutgameRedDotItem Bind(Transform menu,Func<OutgameRedDotControl> control)
        {
            var root=menu.Find("objBottomTab/TabContent/objItemUnSelected");
            var item=root.gameObject.AddComponent<OutgameRedDotItem>();
            item.BindController(control);item.DotPrefab=root.Find("imgCommanderDot3").gameObject;
            item.Scale=Vector2.one;item.RectAnchorType=OutgameRedDotAnchorType.RightTop;item.PosOffset=Vector2.zero;
            item.CheckActionBool=new OutgameRedDotEvent();
            JsonUtility.FromJsonOverwrite("{\"m_PersistentCalls\":{\"m_Calls\":[{\"m_Target\":{\"instanceID\":0},\"m_TargetAssemblyTypeName\":\"\",\"m_MethodName\":\"Tower_CheckReddot\",\"m_Mode\":0,\"m_Arguments\":{\"m_ObjectArgument\":{\"instanceID\":0},\"m_ObjectArgumentAssemblyTypeName\":\"UnityEngine.Object, UnityEngine\",\"m_IntArgument\":0,\"m_FloatArgument\":0,\"m_StringArgument\":\"\",\"m_BoolArgument\":false},\"m_CallState\":2}]}}",item.CheckActionBool);
            return item;
        }
    }
}
