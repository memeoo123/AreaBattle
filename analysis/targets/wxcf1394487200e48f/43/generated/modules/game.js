define("game.js", function(require, module, exports) {
  "use strict";
  require("./plugins/screen-adapter"), require("./weapp-adapter"), require("./events"), require("./texture-config");
  var e = o(require("./unity-namespace"));
  require("./wasm-split"), require("./webgl.wasm.framework.unityweb"), require("./unity-sdk/index");
  var a = o(require("./check-version")),
    t = require("./plugin-config"),
    n = require("./unity-sdk/font/index");

  function o(e) {
    return e && e.__esModule ? e : {
      default: e
    }
  }
  var r = {
    DATA_FILE_MD5: "5ff35f7064c1f713",
    CODE_FILE_MD5: "8cf71c9e452b70bc",
    GAME_NAME: "webgl",
    APPID: "wxcf1394487200e48f",
    DATA_FILE_SIZE: "15103186",
    OPT_DATA_FILE_SIZE: "$OPT_DATA_FILE_SIZE",
    DATA_CDN: "https://gameoss-hz.entermore.cn/app-3/Release/Proj_hdzd/XYX/weixin/hcrzd/1.36",
    loadDataPackageFromSubpackage: !1,
    compressDataPackage: !0,
    preloadDataList: [, ],
    contextConfig: {
      contextType: 1,
      contextExt: {
        enableGLX: !1,
        enableMetal: !1
      }
    },
    PROFILER_UPLOAD_URL: ""
  };
  GameGlobal.managerConfig = r, (0, a.default)().then((function(a) {
    if (a) {
      var o;
      try {
        o = requirePlugin("UnityPlugin", {
          enableRequireHostModule: !0,
          customEnv: {
            wx: wx,
            unityNamespace: e.default,
            document: document,
            canvas: canvas,
            events: GameGlobal.events,
            WXWASMSDK: GameGlobal.WXWASMSDK
          }
        }).default
      } catch (e) {
        GameGlobal.realtimeLogManager.error(e), GameGlobal.logmanager.warn(e.stack), console.error("requirePlugin:", e), -1 !== e.message.indexOf("not defined") && console.error("！！！插件需要申请才可使用\n请勿使用测试AppID，并登录 https://mp.weixin.qq.com/ 并前往：能力地图-开发提效包-快适配 开通\n阅读文档获取详情:https://github.com/wechat-miniprogram/minigame-unity-webgl-transform/blob/main/Design/Transform.md")
      }
      Error.stackTraceLimit = 1 / 0, Object.assign(r, {
        hideAfterCallmain: !0,
        loadingPageConfig: {
          totalLaunchTime: 7e3,
          animationDuration: 100,
          designWidth: 0,
          designHeight: 0,
          scaleMode: t.scaleMode.default,
          textConfig: {
            firstStartText: "首次加载请耐心等待",
            downloadingText: ["正在加载资源"],
            compilingText: "编译中",
            initText: "初始化中",
            completeText: "开始游戏",
            textDuration: 1500,
            style: {
              bottom: 64,
              height: 24,
              width: 240,
              lineHeight: 24,
              color: "#ffffff",
              fontSize: 12
            }
          },
          barConfig: {
            style: {
              width: 240,
              height: 24,
              padding: 2,
              bottom: 64,
              backgroundColor: "#07C160"
            }
          },
          iconConfig: {
            visible: !0,
            style: {
              width: 64,
              height: 23,
              bottom: 20
            }
          },
          materialConfig: {
            backgroundImage: "images/loading.png",
            backgroundVideo: ""
          }
        }
      }), GameGlobal.managerConfig = r;
      var l = new o(r);
      l.onLaunchProgress((function(e) {
        e.type, t.launchEventType.launchPlugin, e.type, t.launchEventType.loadWasm, e.type, t.launchEventType.compileWasm, e.type, t.launchEventType.loadAssets, e.type, t.launchEventType.readAssets, e.type, t.launchEventType.prepareGame
      })), l.onModulePrepared((function() {
        for (var a in e.default) GameGlobal.hasOwnProperty(a) && "DATA_CDN" !== a || (GameGlobal[a] = e.default[a]);
        r.DATA_CDN = GameGlobal.DATA_CDN, l.assetPath = "".concat((r.DATA_CDN || "").replace(/\/$/, ""), "/Assets"), (0, n.preloadWxCommonFont)()
      })), l.onLogError = function(e) {
        GameGlobal.realtimeLogManager.error(e);
        var a = e && e.stack;
        GameGlobal.logmanager.warn(a ? e.stack : e)
      }, GameGlobal.canUseiOSAutoGC && 0 !== e.default.iOSAutoGCInterval && setInterval((function() {
        wx.triggerGC()
      }), e.default.iOSAutoGCInterval), l.startGame(), GameGlobal.manager = l, GameGlobal.events.on("launchOperaPushMsgToWasm", (function(e, a) {
        return GameGlobal.WXWASMSDK.WXLaunchOperaBridgeToC(e, a)
      })), GameGlobal.events.on("createWorker", (function(e) {}))
    }
  }));
  var l = GameGlobal.WXWASMSDK.GetJsonValue("ams_actionid"),
    i = GameGlobal.WXWASMSDK.GetJsonValue("ams_secretkey"),
    s = GameGlobal.WXWASMSDK.GetJsonValue("ams_appid");
  if (l && i && s) {
    l = Number(l), GameGlobal.WXWASMSDK.InitAMS(l, i, s);
    var u = GameGlobal.WXWASMSDK.GetJsonValue("UsePayModule");
    u && "0" !== u || GameGlobal.WXWASMSDK.START_LOAD()
  }
});
require("game.js");