define("wasm-split.js", function(require, module, exports) {
  "use strict";
  var e = require("@babel/runtime/helpers/typeof");
  require("@babel/runtime/helpers/Objectentries");
  var a = require("@babel/runtime/helpers/toConsumableArray"),
    t = require("@babel/runtime/helpers/slicedToArray"),
    n = require("@babel/runtime/helpers/createForOfIteratorHelper"),
    s = require("@babel/runtime/helpers/classCallCheck"),
    i = require("@babel/runtime/helpers/createClass"),
    o = require("./import-func-index"),
    r = "undefined" != typeof wx && wx.getSystemInfoSync ? wx.getSystemInfoSync() : {},
    l = r.platform || "",
    m = "ios" === l,
    u = "android" === l,
    c = r.version || "0.0.0",
    f = r.SDKVersion || "0.0.0";

  function G(e, a) {
    var t = /^\d+(\.\d+)*$/;
    if (!t.test(e) || !t.test(a)) return -1;
    for (var n = e.split("."), s = a.split("."), i = Math.max(n.length, s.length), o = 0; o < i; o++) {
      var r = o < n.length ? parseInt(n[o], 10) : 0,
        l = o < s.length ? parseInt(s[o], 10) : 0;
      if (r > l) return 1;
      if (r < l) return -1
    }
    return 0
  }
  var p, b = "wasmcode",
    g = "wasmcode1",
    d = "wasmcode2",
    h = "wasmcode/",
    w = "wasmcode1/",
    v = "wasmcode2/",
    y = !1,
    M = !1,
    S = 3e4,
    k = 0,
    N = u && G(c, "8.0.30") && G(f, "2.28.1"),
    _ = m && !GameGlobal.isIOSHighPerformanceMode && G(c, "8.0.31") && G(f, "2.28.1"),
    F = u && "8.0.25" === c;
  (N || _) && (M = !0);
  var C = null;

  function I(e) {
    var a;
    !C && e && (wx && "function" == typeof wx.createSignal && (a = wx.createSignal()), C = null == a ? a : {
      waitingCnt: 0,
      signal: a,
      wait: function() {
        this.signal && (this.waitingCnt++, console.log("[PLUGIN SUBWASM LOG]before signal wait, waitingCount: ", this.waitingCnt), this.signal.wait(), this.waitingCnt--, console.log("[PLUGIN SUBWASM LOG]after signal wait"))
      },
      notify: function() {
        GameGlobal.unityNamespace.eventLog("signal notify, waitingCount: ", this.waitingCnt), this.waitingCnt <= 0 || (this.signal.notify(), setTimeout(this.notify.bind(this), 1))
      }
    });
    return C
  }
  var D, A = (wx.getAccountInfoSync() || {}).miniProgram,
    L = A && "release" != A.envVersion,
    T = [],
    W = !1;

  function E(e, a) {
    if (W) e(a);
    else {
      var t = Date.now();
      if (GameGlobal.unityNamespace.eventLog("start loadSubPackage wasmcode1"), T.push({
          callback: e,
          needBlock: a
        }), !D) {
        D = wx.loadSubpackage({
          name: g,
          waiterMode: M,
          success: function() {
            var e = Date.now() - t;
            GameGlobal.unityNamespace.eventLog("下载代码分包1完毕: ", (e / 1e3).toFixed(2), "s"), GameGlobal.manager.subWasmDownloaded(e), W = !0, T.forEach((function(e) {
              return e.callback(e.needBlock)
            })), T = []
          },
          fail: function(e) {
            GameGlobal.manager.Logger.pluginError("load wasmcode1 fail:", e), GameGlobal.manager.subWasmDownloadError(e)
          }
        });
        var n = 0;
        D.onProgressUpdate((function(e) {
          var a = Math.floor(e.progress / 10);
          if (a != n) {
            var t = (e.totalBytesWritten / 1048576).toFixed(2),
              s = (e.totalBytesExpectedToWrite / 1048576).toFixed(2);
            console.log("代码分包1下载进度", e.progress, t, "MB;", s, "MB")
          }
          n = a
        }))
      }
      if (M && a) {
        D.await();
        var s = Date.now() - t;
        GameGlobal.unityNamespace.eventLog("await loadSubWasmPackage and callback: ", (s / 1e3).toFixed(2), "s")
      }
    }
  }
  var P = !1,
    J = !1;

  function B(e) {
    GameGlobal.unityNamespace.eventLog("start compileSubWasm: needBlock ", e), GameGlobal.manager.beforeLoadSubWasm();
    var a = Date.now(),
      t = w + GameGlobal.unityNamespace.CODE_FILE_MD5 + "." + GameGlobal.unityNamespace.GAME_NAME + ".wasm.code.unityweb.wasm.br",
      n = function(e) {
        var t = Date.now() - a;
        GameGlobal.manager.subWasmCompiled(t), GameGlobal.unityNamespace.eventLog("[PLUGIN SUBWASM LOG]subwasm loaded: ", t), ne(wasm_split_info.primary.memory), J = !0, M || null === I(!1) || I(!1).notify()
      };
    M && e ? (new WXWebAssembly.Instance(t, wasm_split_info), n()) : WXWebAssembly.instantiate(t, wasm_split_info).then(n).catch((function(e) {
      console.error("wasmcode1 catch ", e)
    }))
  }

  function x(e) {
    var a = function() {
      J ? e() : setTimeout(a, 1)
    };
    setTimeout(a, 1)
  }

  function O(e) {
    if (!J)
      if (!P || M) {
        if (P = !0, GameGlobal.unityNamespace.eventLog("start instantiateSubWasm: needBlock ", e), E(B, e), !M && e && null != I(!0)) {
          GameGlobal.unityNamespace.eventLog("start wait for loadSubWasmPackage");
          var a = Date.now();
          I(!0).wait(), GameGlobal.manager.reportWaitTime(Date.now() - a)
        }
      } else if (e && null != I(!0)) {
      GameGlobal.unityNamespace.eventLog("start wait for instantiating task");
      var t = Date.now();
      I(!0).wait(), GameGlobal.manager.reportWaitTime(Date.now() - t)
    }
  }

  function R(e) {
    GameGlobal.unityNamespace.eventLog("Now init wasmsplit"), j = !0;
    var a = Date.now();
    GameGlobal.manager.wasmsplit.innerInitFuncSplitWasm({
      archiveArray: e,
      success: function() {
        var t = Date.now() - a;
        GameGlobal.manager.reportInitFuncSplitWasm(e.byteLength, t, !GameGlobal.firstInvoke)
      }
    })
  }
  var j = !1;

  function U() {
    if (GameGlobal.unityNamespace.eventLog("Async initializing function split..."), !j) {
      var e = wx.getFileSystemManager(),
        a = Date.now(),
        t = v + p;
      e.readFile({
        filePath: t,
        position: 0,
        success: function(e) {
          var t = Date.now() - a;
          GameGlobal.unityNamespace.eventLog("wasmcode2 file async read time (ms): ", t), R(e.data)
        },
        fail: function(e) {
          GameGlobal.manager.Logger.pluginError("Async wasmcode2 file read failed", e), j = !0
        }
      })
    }
  }
  var H = !1;

  function z() {
    var e = Date.now();
    if (!H) {
      GameGlobal.unityNamespace.eventLog("start loadSubPackage", d), H = !0;
      var a = wx.loadSubpackage({
          name: d,
          waiterMode: !1,
          success: function() {
            var a = Date.now() - e;
            console.log("下载wasmcode2数据完毕: ", (a / 1e3).toFixed(2), "s"), k > 0 && setTimeout(U, k), 0 == k && U()
          },
          fail: function(e) {
            GameGlobal.manager.Logger.pluginError("下载wasmcode2数据失败:", e)
          }
        }),
        t = 0;
      a && a.onProgressUpdate((function(e) {
        var a = Math.floor(e.progress / 10);
        if (a != t) {
          var n = (e.totalBytesWritten / 1048576).toFixed(2),
            s = (e.totalBytesExpectedToWrite / 1048576).toFixed(2);
          console.log("下载wasmcode2数据进度", e.progress, n, "MB;", s, "MB")
        }
        t = a
      }))
    }
  }

  function q() {
    V.size > 0 && (GameGlobal.manager.reportCalledFuncs && GameGlobal.manager.reportCalledFuncs({
      called_func_list: Array.from(V.keys()),
      func_cnt_list: Array.from(V.values()),
      sub_version: GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION
    }), V.clear())
  }
  GameGlobal.firstInvoke = !0, window.Math_abs = Math.abs, window.Math_cos = Math.cos, window.Math_sin = Math.sin, window.Math_sqrt = Math.sqrt, window.Math_ceil = Math.ceil, window.Math_floor = Math.floor, window.Math_pow = Math.pow, window.Math_min = Math.min, window.Math_max = Math.max, window.Math_fround = Math.fround, window.Math_imul = Math.imul, window.Math_clz32 = Math.clz32, window.Math_trunc = Math.trunc, window.FUNCTION_TABLE = [];
  var V = new Map,
    X = new Set;

  function $() {
    var e = GameGlobal.unityNamespace.logCallBasePtr;
    if (e)
      for (var a = new Int32Array(window.wasm_split_info.primary.memory.buffer), t = 0; t < GameGlobal.unityNamespace.logCallMemorySize; t++) a[e + t] && (V.set(t, a[e + t]), a[e + t] = 0);
    V.size > 0 && (GameGlobal.manager.reportCalledFuncs({
      called_func_list: Array.from(V.keys()),
      func_cnt_list: Array.from(V.values()),
      sub_version: GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION
    }), V.clear()), X.size > 0 && (GameGlobal.manager.reportCalledFuncs({
      called_func_list: Array.from(X),
      sub_version: GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION,
      func_type: 1
    }), X.clear()), setTimeout($, 5e3)
  }
  GameGlobal.unityNamespace.reportCalledFunc = $, GameGlobal.unityNamespace.pluginCalledMainCbs || (GameGlobal.unityNamespace.pluginCalledMainCbs = []), GameGlobal.unityNamespace.pluginCalledMainCb || (GameGlobal.unityNamespace.pluginCalledMainCb = function() {
    this.pluginCalledMainCbs.forEach((function(e) {
      return e()
    }))
  }), L && GameGlobal.unityNamespace.pluginCalledMainCbs.push($);
  var K = 1,
    Q = 6e4,
    Y = 5 * Q,
    Z = new(function() {
      return i((function e() {
        s(this, e), this.gameStartTime = Date.now(), this.totalFrames = 0, this.currentSceneID = null, this.totalMissingFuncs = 0, this.accumulativeInvokeTime = 0, this.lastReportTime = 0, this.lastReportMissingFuncs = 0, this.lastReportAccumulativeInvokeTime = 0, this.funcMissTimes = [], this.totalFrameMissCount = 0, this.frameEventDurations = [], this.funcMissInFrameCounts = [], this.consecutiveMissStreaks = [], this.currentConsecutiveMissStreak = 0, this.lastFrameHadMiss = !1, this.lastProcessedFrameNumber = -1, this.currentFrameNumber = -1, this.frameMissEvents = new Map, this.processedFrames = new Set, this.jankStats = {
          MicroJanks: 0,
          SmallJanks: 0,
          Janks: 0,
          BigJanks: 0,
          HugeJanks: 0,
          Stucks: 0,
          MicroJankDuration: 0,
          SmallJankDuration: 0,
          JankDuration: 0,
          BigJankDuration: 0,
          HugeJankDuration: 0,
          StuckDuration: 0
        }
      }), [{
        key: "calculatePercentiles",
        value: function(e) {
          if (0 === e.length) return [0, 0, 0, 0];
          var a = e.slice().sort((function(e, a) {
              return e - a
            })),
            t = a.length,
            n = function(e) {
              var n = Math.ceil(t * e / 100) - 1;
              return a[Math.max(0, Math.min(n, t - 1))]
            };
          return [n(10), n(50), n(90), a[t - 1]]
        }
      }, {
        key: "getCurrentFrameNumber",
        value: function() {
          try {
            var e, a;
            return (null === (e = GameGlobal.unityNamespace.Browser) || void 0 === e || null === (a = e.mainLoop) || void 0 === a ? void 0 : a.currentFrameNumber) || 0
          } catch (e) {
            return 0
          }
        }
      }, {
        key: "processCompletedFrames",
        value: function(e) {
          var a, s = n(this.frameMissEvents.entries());
          try {
            for (s.s(); !(a = s.n()).done;) {
              var i = t(a.value, 2),
                o = i[0],
                r = i[1];
              o < e && !this.processedFrames.has(o) && (this.finalizeFrameEvent(o, r), this.processedFrames.add(o))
            }
          } catch (e) {
            s.e(e)
          } finally {
            s.f()
          }
        }
      }, {
        key: "finalizeFrameEvent",
        value: function(e, a) {
          var t = a.lastMissEndTime - a.firstMissStartTime;
          this.frameEventDurations.push(t), this.funcMissInFrameCounts.push(a.missCount), e === this.lastProcessedFrameNumber + 1 ? this.currentConsecutiveMissStreak++ : (this.currentConsecutiveMissStreak > 0 && this.consecutiveMissStreaks.push(this.currentConsecutiveMissStreak), this.currentConsecutiveMissStreak = 1), this.lastProcessedFrameNumber = e, this.classifyAndRecordJank(t)
        }
      }, {
        key: "classifyAndRecordJank",
        value: function(e) {
          e <= 8 ? (this.jankStats.MicroJanks++, this.jankStats.MicroJankDuration += e) : e > 8 && e <= 16 ? (this.jankStats.SmallJanks++, this.jankStats.SmallJankDuration += e) : e > 16 && e <= 33 ? (this.jankStats.Janks++, this.jankStats.JankDuration += e) : e > 33 && e <= 200 ? (this.jankStats.BigJanks++, this.jankStats.BigJankDuration += e) : e > 200 && e <= 1e3 ? (this.jankStats.HugeJanks++, this.jankStats.HugeJankDuration += e) : e > 1e3 && (this.jankStats.Stucks++, this.jankStats.StuckDuration += e)
        }
      }, {
        key: "recordFuncMiss",
        value: function(e) {
          if (ee) {
            var a = Date.now(),
              t = Date.now() - e;
            this.funcMissTimes.push(t), this.totalMissingFuncs++, this.accumulativeInvokeTime += t;
            var n = this.getCurrentFrameNumber();
            this.processCompletedFrames(n), this.frameMissEvents.has(n) || (this.frameMissEvents.set(n, {
              firstMissStartTime: e,
              lastMissEndTime: a,
              missCount: 0,
              missTimes: []
            }), this.totalFrameMissCount++);
            var s = this.frameMissEvents.get(n);
            s.lastMissEndTime = a, s.missCount++, s.missTimes.push(t)
          }
        }
      }, {
        key: "setSceneID",
        value: function(e) {
          this.currentSceneID = e
        }
      }, {
        key: "finalizeAllPendingFrames",
        value: function() {
          var e, a = this.getCurrentFrameNumber(),
            s = n(this.frameMissEvents.entries());
          try {
            for (s.s(); !(e = s.n()).done;) {
              var i = t(e.value, 2),
                o = i[0],
                r = i[1];
              this.processedFrames.has(o) || (o !== a || r.lastMissEndTime || (r.lastMissEndTime = Date.now()), this.finalizeFrameEvent(o, r), this.processedFrames.add(o))
            }
          } catch (e) {
            s.e(e)
          } finally {
            s.f()
          }
          this.currentConsecutiveMissStreak > 0 && (this.consecutiveMissStreaks.push(this.currentConsecutiveMissStreak), this.currentConsecutiveMissStreak = 0)
        }
      }, {
        key: "generateReportData",
        value: function() {
          return this.finalizeAllPendingFrames(), {
            TotalFrames: this.getCurrentFrameNumber(),
            SceneId: this.currentSceneID,
            TotalMissFuncs: this.totalMissingFuncs,
            TotalMissTime: this.accumulativeInvokeTime,
            FuncMissDist: JSON.stringify(this.calculatePercentiles(this.funcMissTimes)),
            TotalMissFrames: this.totalFrameMissCount,
            FrameMissDist: JSON.stringify(this.calculatePercentiles(this.frameEventDurations)),
            FuncMissInFrameDist: JSON.stringify(this.calculatePercentiles(this.funcMissInFrameCounts)),
            ConsecutiveMissFrames: this.consecutiveMissStreaks.length > 0 ? Math.max.apply(Math, a(this.consecutiveMissStreaks)) : 0,
            MicroJanks: this.jankStats.MicroJanks,
            SmallJanks: this.jankStats.SmallJanks,
            Janks: this.jankStats.Janks,
            BigJanks: this.jankStats.BigJanks,
            HugeJanks: this.jankStats.HugeJanks,
            Stucks: this.jankStats.Stucks,
            MicroJankDuration: this.jankStats.MicroJankDuration,
            SmallJankDuration: this.jankStats.SmallJankDuration,
            JankDuration: this.jankStats.JankDuration,
            BigJankDuration: this.jankStats.BigJankDuration,
            HugeJankDuration: this.jankStats.HugeJankDuration,
            StuckDuration: this.jankStats.StuckDuration
          }
        }
      }, {
        key: "report",
        value: function() {
          this.lastReportTime = Date.now(), this.lastReportMissingFuncs = this.totalMissingFuncs, this.lastReportAccumulativeInvokeTime = this.accumulativeInvokeTime;
          var e = ae();
          if ("new" === e) {
            var a = this.generateReportData();
            GameGlobal.manager.reportWasmSplitPerformance(a)
          } else if ("old" === e) GameGlobal.manager.reportInvokeWasmFuncInfo(this.totalMissingFuncs, this.accumulativeInvokeTime);
          else {
            console.warn("[PLUGIN WARN] Wasm split report not supported");
            var t = this.generateReportData();
            console.log("[PLUGIN] Wasm split func missing report:", t)
          }
          q()
        }
      }])
    }()),
    ee = !1;

  function ae() {
    return GameGlobal.manager.reportWasmSplitPerformance ? "new" : GameGlobal.manager.reportInvokeWasmFuncInfo ? "old" : null
  }

  function te() {
    var e = !0;
    4 & K && Z.totalMissingFuncs < Z.lastReportMissingFuncs + 10 && Date.now() - Z.lastReportTime < Y && (e = !1), 0 === Z.totalMissingFuncs && (e = !1), e && Z.report(), setTimeout(te, Q)
  }

  function ne(e) {
    if (!GameGlobal.unityNamespace.initedRedirMem) {
      GameGlobal.unityNamespace.initedRedirMem = !0;
      var a = wx.getFileSystemManager();
      try {
        var t = a.readFileSync(h + GameGlobal.unityNamespace.CODE_FILE_MD5 + "." + GameGlobal.unityNamespace.GAME_NAME + ".redirmem.bin")
      } catch (e) {
        return void GameGlobal.unityNamespace.eventLog("[PLUGIN LOG] release does not have redirmem.bin")
      }
      var n = new Int32Array(t);
      GameGlobal.unityNamespace.logCallMemorySize = n.length;
      var s = e.buffer.byteLength - t.byteLength;
      GameGlobal.unityNamespace.logCallBaseAddr.value = s, s >>= 2, GameGlobal.unityNamespace.logCallBasePtr = s;
      for (var i = new Int32Array(e.buffer), o = 0; o < n.length; o++) i[s + o] = n[o];
      var r = GameGlobal.manager.gameInstance.Module._malloc(t.byteLength);
      for (GameGlobal.unityNamespace.logCallBaseAddr.value = r, r >>= 2, GameGlobal.unityNamespace.logCallBasePtr = r, o = 0; o < n.length; o++) i[r + o] = i[s + o]
    }
  }

  function se(e, a) {
    console.log("in wasm-split preInstantiateWasm"), p = GameGlobal.unityNamespace.CODE_FILE_MD5 + "." + GameGlobal.unityNamespace.GAME_NAME + ".wasm.code.unityweb.wasm.br",
      function() {
        if (!y) {
          y = !0;
          var e = "undefined" != typeof GameGlobal && GameGlobal.managerConfig || null;
          if (e) {
            var a = e.WASM_MODULE_NAME || "wasmcode",
              t = 0 === a.indexOf("wasmcode") ? a.slice(8) : "";
            b = "wasmcode" + t, g = "wasmcode1" + t, d = "wasmcode2" + t;
            var n = e.GAME_DIR || "",
              s = n ? n.replace(/\/?$/, "/") : "";
            h = s + "wasmcode/", w = s + "wasmcode1/", v = s + "wasmcode2/", console.log("[WASM-SPLIT] resolveManagerConfig:", "WASMCODE_NAME=", b, "WASMCODE1_NAME=", g, "WASMCODE2_NAME=", d, "WASMCODE_PATH=", h, "WASMCODE1_PATH=", w, "WASMCODE2_PATH=", v)
          }
        }
      }(),
      function() {
        if (GameGlobal.manager.wasmsplit.getFeatures) {
          var e = GameGlobal.manager.wasmsplit.getFeatures(),
            a = function(a, t, n, s) {
              if (void 0 !== (null == e ? void 0 : e[a])) {
                var i = parseInt(e[a], 10);
                !isNaN(i) && i >= 0 ? (s(i), GameGlobal.unityNamespace.eventLog(t + " set to:", i)) : GameGlobal.unityNamespace.eventLog("Invalid " + a + " value, using default:", e[a])
              } else GameGlobal.unityNamespace.eventLog("No " + a + " found, using default:", n)
            };
          a("delayInitTime", "kDelayLoadWasmcode2Time", S, (function(e) {
            S = e
          })), a("reportStrategy", "reportStrategy", K, (function(e) {
            K = e
          })), a("reportInterval", "kMinWasmSplitReportInterval", Q, (function(e) {
            Y = 5 * (Q = e)
          })), a("delayFuncInitTime", "kDelayFuncInitTime", k, (function(e) {
            k = e
          }))
        } else GameGlobal.unityNamespace.eventLog("No features found")
      }(), m && GameGlobal.canUseH5Renderer && GameGlobal.unityNamespace.pluginCalledMainCbs.push((function() {
        GameGlobal.unityNamespace.eventLog("setTimeout for download wasmcode2: ", S), setTimeout(z, S),
          function() {
            if (!ae()) return console.warn("[PLUGIN WARN] Wasm split report not supported, fallback to onHide for missing funcs only"), void wx.onHide((function() {
              q()
            }));
            ee = !0, 1 & K && wx.onHide((function() {
              0 !== Z.totalMissingFuncs && Z.report()
            })), 2 & K && te()
          }()
      }));
    var t = a.asm2wasm = a.asm2wasm || {};
    t["f64-to-int"] = t["f64-to-int"] || function(e) {
      return 0 | e
    }, t["f64-rem"] = t["f64-rem"] || function(e, a) {
      return e % a
    }, a.env = a.env || {}, a.primary = {
      table: a.env.table || new WebAssembly.Table({
        initial: o.tableSize,
        maximum: o.tableSize,
        element: "anyfunc"
      }),
      memory: a.env.memory
    }, (0, o.setImportGlobal)(a), a.wasm_split = a.wasm_split || {}, a.wasm_split.wait = function() {
      var e = GameGlobal.unityNamespace.waitTableId.value;
      if (GameGlobal.unityNamespace.eventLog("wait for func: ", e), m && canUseH5Renderer) return function e(a, t) {
        GameGlobal.firstInvoke && (GameGlobal.firstInvoke = !1, GameGlobal.manager.startFetchJsCode()), j || function() {
          GameGlobal.unityNamespace.eventLog("Sync wasm split initialization");
          var e = wx.getFileSystemManager(),
            a = v + p;
          try {
            var t = Date.now(),
              n = e.readFileSync(a),
              s = Date.now() - t;
            GameGlobal.unityNamespace.eventLog("wasmcode2 file sync read time (ms): ", s), R(n)
          } catch (e) {
            GameGlobal.unityNamespace.eventLog("Subpackage wasmcode2 not loaded, trying to load..."), z()
          }
        }();
        var n = Date.now();
        GameGlobal.manager.wasmsplit.innerInvokeWasmBuffer({
          invokeWasmBuffer: invokeWasmBuffer,
          funcName: a,
          importObj: window.wasm_split_info,
          success: function(e) {
            Z.recordFuncMiss(n), "number" == typeof e && V.set(e, (V.get(e) || 0) + 1)
          },
          fail: function(n) {
            if (t > 0) return GameGlobal.unityNamespace.eventLog("fetch table_id: ", a, "; remainRetryTimes: ", t), e(a, t - 1);
            if ("system error" !== n && "content is null" !== n && "system error" !== n.errmsg && "content is null" !== n.errmsg) throw wx.showModal({
              title: "网络状态异常",
              content: "请检查网络后重启 小游戏",
              showCancel: !1,
              confirmText: wx.restartMiniProgram ? "立即重启" : "确定",
              success: function() {
                wx.restartMiniProgram && wx.restartMiniProgram({})
              }
            }), new Error("网络状态异常:" + n);
            j ? GameGlobal.onCrash() : GameGlobal.unityNamespace.eventLog("Try to syncly wait for func split initialization"), GameGlobal.unityNamespace.eventLog("fetch table_id:", a, "; md5: ", GameGlobal.unityNamespace.CODE_FILE_MD5, "; subversion: ", GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION, "; err: ", n)
          }
        })
      }(e.toString(), 3);
      O(!0), GameGlobal.manager.wasmsplit && GameGlobal.manager.wasmsplit.checkNeedReportMissPatchFunc && GameGlobal.manager.wasmsplit.checkNeedReportMissPatchFunc(e), GameGlobal.manager.reportCalledFuncs({
        called_func_list: [e],
        func_type: 2,
        sub_version: GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION
      })
    }, a.wasm_split.logCall = function(e, a) {
      GameGlobal.manager.mainCalled || X.add(e), V.set(e, (V.get(e) || 0) + 1)
    }, a.wasm_split.__wasm_split_waitTableId = GameGlobal.unityNamespace.waitTableId = new WebAssembly.Global({
      value: "i32",
      mutable: !0
    }), a.wasm_split.__wasm_split_logCallBaseAddr = GameGlobal.unityNamespace.logCallBaseAddr = new WebAssembly.Global({
      value: "i32",
      mutable: !0
    }), window.wasm_split_info = a
  }

  function ie(a, t, n) {
    for (var s = a.asm2wasm = a.asm2wasm || {}, i = GameGlobal.manager.gameInstance.Module, o = a.primary.table, r = 0; r < o.length; ++r) window.FUNCTION_TABLE[r] = o.get(r);
    if (Object.entries(t.instance.exports).forEach((function(t) {
        "object" == e(t[1]) && t[1] instanceof WebAssembly.Memory && (a.primary.memory = t[1], s.__wasm_memory_size = window.__wasm_memory_size = function() {
          return t[1].buffer.byteLength / 65536 | 0
        }), t[0].startsWith("wasm_split.") && (a.primary[t[0]] = t[1])
      })), a.primary["wasm_split.initGlobal"] = t.instance.exports.initGlobal, t.instance.exports.initGlobal(), i.asm = t.instance.exports, GameGlobal.canUseH5Renderer || M || null != I(!0)) {
      ne(a.primary.memory);
      var l = "android&wasm3";
      GameGlobal.canUseH5Renderer && (l = "iOS_hp"),
        function(e) {
          var a = e.runtimeType;
          return GameGlobal.manager.instantiatePatch ? GameGlobal.manager.instantiatePatch(GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION, wasm_split_info, {}).then((function(e) {
            return e && GameGlobal.unityNamespace.eventLog(a + " instantiatePatch success ", GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION), e
          })).catch((function(e) {
            console.warn("[PLUGIN SUBWASM WARN]", a, " instantiatePatch warn", e), GameGlobal.unityNamespace.eventLog(a, "instantiatePatch warn", e)
          })) : Promise.resolve()
        }({
          runtimeType: l,
          undefined: void 0
        }).finally((function() {
          console.log("instantiatePatch finally"), n(t.instance, t.module)
        }))
    } else GameGlobal.unityNamespace.eventLog("no signal api, start instantiate sub wasm"), O(!0), x((function() {
      return n(t.instance, t.module)
    }))
  }
  GameGlobal.unityNamespace.instantiateWasm = function(e, a) {
    console.log("in wasm-split instantiateWasm");
    var t = GameGlobal.manager.gameInstance.Module.wasmPath;
    return se(0, e), WebAssembly.instantiate(t, e).then((function(t) {
      ie(e, t, a)
    })), {}
  }, GameGlobal.unityNamespace.preInstantiateWasm = se, GameGlobal.unityNamespace.postInstantiateWasm = ie, GameGlobal.unityNamespace.compileSubWasm = function() {
    return new Promise((function(e) {
      if (GameGlobal.unityNamespace.eventLog("start asyncCompileSubWasm"), J) return e();
      F ? (O(!1), x(e)) : E((function() {
        GameGlobal.unityNamespace.eventLog("async load subpackage done")
      }), !1)
    }))
  };
});