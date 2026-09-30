define("unity-namespace.js", function(require, module, exports) {
  "use strict";
  Object.defineProperty(exports, "__esModule", {
    value: !0
  }), exports.default = void 0, require("@babel/runtime/helpers/Arrayincludes");
  var e = {
    canvas: GameGlobal.canvas,
    canvas_width: GameGlobal.canvas.width,
    canvas_height: GameGlobal.canvas.height,
    navigator: GameGlobal.navigator,
    XMLHttpRequest: GameGlobal.XMLHttpRequest,
    hideTimeLogModal: !0,
    enableDebugLog: !1,
    bundleHashLength: 32,
    releaseMemorySize: 31457280,
    unityVersion: "2021.3.56f2",
    unityColorSpace: "Gamma",
    convertPluginVersion: "202509110649",
    streamingUrlPrefixPath: "",
    dataFileSubPrefix: "",
    maxStorage: 200,
    texturesHashLength: 8,
    texturesPath: "Assets/Textures",
    needCacheTextures: !0,
    ttlAssetBundle: 5,
    enableProfileStats: !1,
    preloadWXFont: !1,
    iOSAutoGCInterval: 1e4,
    usedTextureCompression: GameGlobal.USED_TEXTURE_COMPRESSION,
    usedAutoStreaming: !1,
    enableRenderAnalysisLog: !1,
    useDotnetRuntime: !1,
    useBrotliMT: !1,
    bootConfig: "player-connection-ip=192.168.170.46",
    isDevelopmentBuild: !1,
    isProfilingBuild: !1,
    unityHeapReservedMemory: 256,
    sendData2PerfStream: !1
  };
  e.monitorConfig = {
      showSuggestModal: !1,
      enableMonitor: !0,
      fps: 10,
      showResultAfterLaunch: !0,
      monitorDuration: 3e4
    }, e.isCacheableFile = function(e) {
      var a = [".json", ".xml", ".db", ".version"];
      return !!["StreamingAssets"].some((function(o) {
        return e.includes(o) && a.every((function(a) {
          return !e.includes(a)
        }))
      }))
    }, e.isReportableHttpError = function(e) {
      return !0
    }, e.isWXAssetBundle = function(a) {
      return e.WXAssetBundles.has(e.PathInFileOS(a))
    }, e.PathInFileOS = function(e) {
      return e.replace("".concat(wx.env.USER_DATA_PATH, "/__GAME_FILE_CACHE"), "")
    }, e.WXAssetBundles = new Map, e.isErasableFile = function(a) {
      if (e.WXAssetBundles.has(a.path)) return !1;
      return ![].some((function(e) {
        return a.path.includes(e)
      }))
    }, GameGlobal.WebAssembly = GameGlobal.WXWebAssembly, GameGlobal.unityNamespace = GameGlobal.unityNamespace || e, GameGlobal.realtimeLogManager = wx.getRealtimeLogManager(), GameGlobal.logmanager = wx.getLogManager({
      level: 0
    }), GameGlobal.disableMultiTouch = !1, wx.onError((function(e) {
      if (GameGlobal.manager) GameGlobal.manager.printErr(e.message);
      else {
        GameGlobal.realtimeLogManager.error(e);
        var a = e && e.stack;
        GameGlobal.logmanager.warn(a ? e.stack : e), console.error("onError:", e)
      }
    })), wx.onUnhandledRejection((function(e) {
      GameGlobal.realtimeLogManager.error(e);
      var a = e && e.reason && e.reason.stack;
      GameGlobal.logmanager.warn(a ? e.reason.stack : e.reason), console.error("unhandledRejection:", e.reason)
    })),
    function(a, o) {
      var n = a.version,
        r = a.SDKVersion,
        t = o.platform,
        s = o.system;
      e.version = n, e.SDKVersion = r, e.platform = t, e.system = s, e.isPc = "windows" === t || "mac" === t, e.isDevtools = "devtools" === t, e.isMobile = !e.isPc && !e.isDevtools, e.isH5Renderer = GameGlobal.isIOSHighPerformanceMode, e.isIOS = "ios" === t, e.isAndroid = "android" === t;
      var l = {
        renderer: GameGlobal.isIOSHighPerformanceMode ? "h5" : "",
        isH5Plus: GameGlobal.isIOSHighPerformanceModePlus || !1,
        abi: o.abi || "",
        brand: o.brand,
        model: o.model,
        platform: o.platform,
        system: o.system,
        version: a.version,
        SDKVersion: a.SDKVersion,
        benchmarkLevel: o.benchmarkLevel
      };
      GameGlobal.realtimeLogManager.info("game starting", l), GameGlobal.logmanager.info("game starting", l), console.info("game starting", l)
    }(wx.getAppBaseInfo ? wx.getAppBaseInfo() : wx.getSystemInfoSync(), wx.getDeviceInfo ? wx.getDeviceInfo() : wx.getSystemInfoSync()), GameGlobal.onCrash = GameGlobal.unityNamespace.onCrash = function() {
      GameGlobal.manager.showAbort();
      var e = wx.getWindowInfo ? wx.getWindowInfo() : wx.getSystemInfoSync();
      wx.createFeedbackButton({
        type: "text",
        text: "提交反馈",
        style: {
          left: (e.screenWidth - 184) / 2,
          top: e.screenHeight / 3 + 140,
          width: 184,
          height: 40,
          lineHeight: 40,
          backgroundColor: "#07C160",
          color: "#ffffff",
          textAlign: "center",
          fontSize: 16,
          borderRadius: 4
        }
      })
    };
  exports.default = GameGlobal.unityNamespace;
});