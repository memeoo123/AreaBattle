define("webgl.wasm.framework.unityweb.js", function(require, module, exports) {
  var n = window.AudioContext || window.webkitAudioContext;
  window.AudioContext = function() {
    return this instanceof window.AudioContext ? wx.createWebAudioContext() : new n
  }, GameGlobal.unityNamespace.UnityModule = function(n) {
    function e(n, e) {
      return T("The JavaScript function 'Pointer_stringify(ptrToSomeCString)' is obsoleted and will be removed in a future Unity version. Please call 'UTF8ToString(ptrToSomeCString)' instead."), rn(n, e)
    }(n = void 0 !== n ? n : {}).Pointer_stringify = e;
    var i = "(^|\\n)(\\s+at\\s+|)jsStackTrace(\\s+\\(|@)([^\\n]+):\\d+:\\d+(\\)|)(\\n|$)",
      t = Hn().match(new RegExp(i));
    t && (n.stackTraceRegExp = new RegExp(i.replace("([^\\n]+)", t[4].replace(/[\\^${}[\]().*+?|]/g, "\\$&")).replace("jsStackTrace", "[^\\n]+")));
    var r = function(e) {
      if (!I) {
        I = !0, O = 1, "undefined" != typeof ENVIRONMENT_IS_PTHREAD && ENVIRONMENT_IS_PTHREAD && x("Pthread aborting at " + (new Error).stack), void 0 !== e ? (M(e), x(e), e = e instanceof Error ? e.toString() : JSON.stringify(e)) : e = "", n.IsWxGame && window.WXWASMSDK.WXUncaughtException(!0);
        var i = "abort(" + e + ") at " + Jn();
        if (!n.abortHandler || !n.abortHandler(i)) throw i
      }
    };
    n.SetFullscreen = function(e) {
      if (void 0 !== wn && wn)
        if (void 0 === dd) console.log("Player not loaded yet.");
        else {
          var i = dd.canPerformEventHandlerRequests;
          dd.canPerformEventHandlerRequests = function() {
            return 1
          }, n.ccall("SetFullscreen", null, ["number"], [e]), dd.canPerformEventHandlerRequests = i
        }
      else console.log("Runtime not initialized yet.")
    }, "undefined" != typeof ENVIRONMENT_IS_PTHREAD && ENVIRONMENT_IS_PTHREAD || n.preRun.push((function() {
      xc.queuePersist = function(n) {
        function e() {
          "again" === n.idbPersistState ? i() : n.idbPersistState = 0
        }

        function i() {
          n.idbPersistState = "idb", xc.syncfs(n, !1, e)
        }
        n.idbPersistState ? "idb" === n.idbPersistState && (n.idbPersistState = "again") : n.idbPersistState = setTimeout(i, 0)
      }, xc.mount = function(n) {
        var e = Mc.mount(n);
        if (void 0 !== n && n.opts && n.opts.autoPersist) {
          e.idbPersistState = 0;
          var i = e.node_ops;
          e.node_ops = Object.assign({}, e.node_ops), e.node_ops.mknod = function(n, t, r, o) {
            var a = i.mknod(n, t, r, o);
            return a.node_ops = e.node_ops, a.idbfs_mount = e.mount, a.memfs_stream_ops = a.stream_ops, a.stream_ops = Object.assign({}, a.stream_ops), a.stream_ops.write = function(n, e, i, t, r, o) {
              return n.node.isModified = !0, a.memfs_stream_ops.write(n, e, i, t, r, o)
            }, a.stream_ops.close = function(n) {
              var e = n.node;
              if (e.isModified && (xc.queuePersist(e.idbfs_mount), e.isModified = !1), e.memfs_stream_ops.close) return e.memfs_stream_ops.close(n)
            }, a
          }, e.node_ops.rmdir = function(n) {
            return xc.queuePersist(e.mount), i.rmdir(n)
          }, e.node_ops.unlink = function(n) {
            return xc.queuePersist(e.mount), i.unlink(n)
          }, e.node_ops.mkdir = function(n, t) {
            return xc.queuePersist(e.mount), i.mkdir(n, t)
          }, e.node_ops.symlink = function(n, t, r) {
            return xc.queuePersist(e.mount), i.symlink(n, t, r)
          }, e.node_ops.rename = function(n, t, r) {
            return xc.queuePersist(e.mount), i.rename(n, t, r)
          }
        }
        return e
      }, (n.unityFileSystemInit || function() {
        Xc.mkdir("/idbfs"), n.__unityIdbfsMount = Xc.mount(xc, {
          autoPersist: !!n.autoSyncPersistentDataPath
        }, "/idbfs"), n.addRunDependency("JS_FileSystem_Mount"), Xc.syncfs(!0, (function(e) {
          e && console.log("IndexedDB is not available. Data will not persist in cache and PlayerPrefs will not be saved."), n.removeRunDependency("JS_FileSystem_Mount")
        }))
      })()
    }));
    var o, a = [],
      l = null;

    function u(n) {
      for (var e = Object.keys(a), i = 0; i < e.length; ++i) {
        if ((t = a[e[i]]).deviceId && t.deviceId == n.deviceId) return t
      }
      for (i = 0; i < e.length; ++i) {
        if ((t = a[e[i]]) == n) return t
      }
      for (i = 0; i < e.length; ++i) {
        if ((t = a[e[i]]).label && t.label == n.label) return t
      }
      for (i = 0; i < e.length; ++i) {
        var t;
        if ((t = a[e[i]]).groupId && t.kind && t.groupId == n.groupId && t.kind == n.kind) return t
      }
    }

    function f() {
      for (var n = 0;; ++n)
        if (!a[n]) return n
    }

    function c(n) {
      o(), a = [];
      var e = {},
        i = [];
      n.forEach((function(n) {
        if ("videoinput" === n.kind) {
          var t = u(n);
          t ? e[t.id] = t : i.push(n)
        }
      })), a = e, i.forEach((function(n) {
        n.id || (n.id = f(), n.name = n.label || "Video input #" + (n.id + 1), n.isFrontFacing = n.name.toLowerCase().includes("front") || !n.name.toLowerCase().includes("front") && !n.name.toLowerCase().includes("back"), a[n.id] = n)
      }))
    }

    function s() {
      a && (navigator.mediaDevices.enumerateDevices().then((function(n) {
        c(n), !0
      })).catch((function(n) {
        console.warn("Unable to enumerate media devices: " + n + "\nWebcams will not be available."), d()
      })), /Firefox/.test(navigator.userAgent) && (setTimeout(s, 6e4), T("Applying workaround to Firefox bug https://bugzilla.mozilla.org/show_bug.cgi?id=1397977")))
    }

    function d() {
      navigator.mediaDevices && navigator.mediaDevices.removeEventListener && navigator.mediaDevices.removeEventListener("devicechange", s), a = null
    }

    function m(n, e, i) {
      var t = Ve(e),
        r = Ve(n),
        o = 0;
      try {
        if (void 0 === i) wg(r, t);
        else if ("string" == typeof i) o = Ve(i), hg(r, t, o);
        else {
          if ("number" != typeof i) throw i + " is does not have a type which is supported by SendMessage.";
          gg(r, t, i)
        }
      } finally {
        Fg(o), Fg(r), Fg(t)
      }
    }
    n.disableAccessToMediaDevices = d, navigator.mediaDevices ? "undefined" != typeof ENVIRONMENT_IS_PTHREAD && ENVIRONMENT_IS_PTHREAD || setTimeout((function() {
      try {
        jn("enumerateMediaDevices"), o = function() {
          null !== l && clearTimeout(l), Tn("enumerateMediaDevices"), navigator.mediaDevices && console.log("navigator.mediaDevices support available"), o = function() {}
        }, s(), l = setTimeout(o, 1e3), navigator.mediaDevices.addEventListener("devicechange", s)
      } catch (n) {
        console.warn("Unable to enumerate media devices: " + n), d()
      }
    }), 0) : (console.warn("navigator.mediaDevices not supported by this browser. Webcam access will not be available." + ("https:" == location.protocol ? "" : " Try hosting the page over HTTPS, because some browsers disable webcam access when insecure HTTP is being used.")), d()), n.SendMessage = m;
    var p, y = {};
    for (p in n) n.hasOwnProperty(p) && (y[p] = n[p]);
    var v = [],
      _ = "./this.program",
      g = function(n, e) {
        throw e
      },
      h = !1,
      w = !1,
      S = !1,
      C = !1;
    h = "object" == typeof window, w = "function" == typeof importScripts, S = "object" == typeof process && "object" == typeof process.versions && "string" == typeof process.versions.node, C = !h && !S && !w;
    var E, b, W, D, A = "";

    function k(e) {
      return n.locateFile ? n.locateFile(e, A) : A + e
    }
    S ? (A = w ? require("path").dirname(A) + "/" : __dirname + "/", E = function(n, e) {
      return W || (W = require("fs")), D || (D = require("path")), n = D.normalize(n), W.readFileSync(n, e ? null : "utf8")
    }, function(n) {
      var e = E(n, !0);
      return e.buffer || (e = new Uint8Array(e)), K(e.buffer), e
    }, process.argv.length > 1 && (_ = process.argv[1].replace(/\\/g, "/")), v = process.argv.slice(2), "undefined" != typeof module && (module.exports = n), process.on("uncaughtException", (function(n) {
      if (!(n instanceof uE)) throw n
    })), process.on("unhandledRejection", r), g = function(n) {
      process.exit(n)
    }, n.inspect = function() {
      return "[Emscripten Module object]"
    }) : C ? ("undefined" != typeof read && (E = function(n) {
      return read(n)
    }), function(n) {
      var e;
      return "function" == typeof readbuffer ? new Uint8Array(readbuffer(n)) : (K("object" == typeof(e = read(n, "binary"))), e)
    }, "undefined" != typeof scriptArgs ? v = scriptArgs : void 0 !== arguments && (v = arguments), "function" == typeof quit && (g = function(n) {
      quit(n)
    }), "undefined" != typeof print && ("undefined" == typeof console && (console = {}), console.log = print, console.warn = console.error = "undefined" != typeof printErr ? printErr : print)) : (h || w) && (w ? A = this.location.href : "undefined" != typeof document && document.currentScript && (A = document.currentScript.src), A = 0 !== A.indexOf("blob:") ? A.substr(0, A.lastIndexOf("/") + 1) : "", E = function(n) {
      var e = new XMLHttpRequest;
      return e.open("GET", n, !1), e.send(null), e.responseText
    }, b = function(n, e, i) {
      var t = new XMLHttpRequest;
      t.open("GET", n, !0), t.responseType = "arraybuffer", t.onload = function() {
        200 == t.status || 0 == t.status && t.response ? e(t.response) : i()
      }, t.onerror = i, t.send(null)
    });
    var M = n.print || console.log.bind(console),
      x = n.printErr || console.warn.bind(console);
    for (p in y) y.hasOwnProperty(p) && (n[p] = y[p]);
    y = null, n.arguments && (v = n.arguments), n.thisProgram && (_ = n.thisProgram), n.quit && (g = n.quit);
    var X = 16;

    function j(n, e) {
      return e || (e = X), Math.ceil(n / e) * e
    }

    function T(n) {
      T.shown || (T.shown = {}), T.shown[n] || (T.shown[n] = 1, x(n))
    }
    var L, F = 0,
      P = function(n) {
        F = n
      },
      R = function() {
        return F
      };
    n.wasmBinary && (L = n.wasmBinary);
    var B, G = n.noExitRuntime || !0;
    "object" != typeof WebAssembly && r("no native wasm support detected");
    var O, I = !1;

    function K(n, e) {
      n || r("Assertion failed: " + e)
    }

    function N(e) {
      var i = n["_" + e];
      return K(i, "Cannot call unknown function " + e + ", make sure it is exported"), i
    }

    function U(n, e, i, t, r) {
      var o = {
        string: function(n) {
          var e = 0;
          if (null != n && 0 !== n) {
            var i = 1 + (n.length << 2);
            an(n, e = xg(i), i)
          }
          return e
        },
        array: function(n) {
          var e = xg(n.length);
          return cn(n, e), e
        }
      };
      var a = N(n),
        l = [],
        u = 0;
      if (t)
        for (var f = 0; f < t.length; f++) {
          var c = o[i[f]];
          c ? (0 === u && (u = kg()), l[f] = c(t[f])) : l[f] = t[f]
        }
      var s = a.apply(null, l);
      return s = function(n) {
        return "string" === e ? rn(n) : "boolean" === e ? Boolean(n) : n
      }(s), 0 !== u && Mg(u), s
    }

    function z(n, e, i, t) {
      var r = (i = i || []).every((function(n) {
        return "number" === n
      }));
      return "string" !== e && r && !t ? N(n) : function() {
        return U(n, e, i, arguments)
      }
    }
    var q, H, V, Y, J, Z, Q, $, nn, en = "undefined" != typeof TextDecoder ? new TextDecoder("utf8") : void 0;

    function tn(n, e, i) {
      for (var t = e + i, r = e; n[r] && !(r >= t);) ++r;
      if (r - e > 16 && n.subarray && en) return en.decode(n.subarray(e, r));
      for (var o = ""; e < r;) {
        var a = n[e++];
        if (128 & a) {
          var l = 63 & n[e++];
          if (192 != (224 & a)) {
            var u = 63 & n[e++];
            if ((a = 224 == (240 & a) ? (15 & a) << 12 | l << 6 | u : (7 & a) << 18 | l << 12 | u << 6 | 63 & n[e++]) < 65536) o += String.fromCharCode(a);
            else {
              var f = a - 65536;
              o += String.fromCharCode(55296 | f >> 10, 56320 | 1023 & f)
            }
          } else o += String.fromCharCode((31 & a) << 6 | l)
        } else o += String.fromCharCode(a)
      }
      return o
    }

    function e(n) {
      return rn(n)
    }

    function rn(n, e) {
      return n ? tn(V, n, e) : ""
    }

    function on(n, e, i, t) {
      if (!(t > 0)) return 0;
      for (var r = i, o = i + t - 1, a = 0; a < n.length; ++a) {
        var l = n.charCodeAt(a);
        if (l >= 55296 && l <= 57343) l = 65536 + ((1023 & l) << 10) | 1023 & n.charCodeAt(++a);
        if (l <= 127) {
          if (i >= o) break;
          e[i++] = l
        } else if (l <= 2047) {
          if (i + 1 >= o) break;
          e[i++] = 192 | l >> 6, e[i++] = 128 | 63 & l
        } else if (l <= 65535) {
          if (i + 2 >= o) break;
          e[i++] = 224 | l >> 12, e[i++] = 128 | l >> 6 & 63, e[i++] = 128 | 63 & l
        } else {
          if (i + 3 >= o) break;
          e[i++] = 240 | l >> 18, e[i++] = 128 | l >> 12 & 63, e[i++] = 128 | l >> 6 & 63, e[i++] = 128 | 63 & l
        }
      }
      return e[i] = 0, i - r
    }

    function an(n, e, i) {
      return on(n, V, e, i)
    }

    function ln(n) {
      for (var e = 0, i = 0; i < n.length; ++i) {
        var t = n.charCodeAt(i);
        t >= 55296 && t <= 57343 && (t = 65536 + ((1023 & t) << 10) | 1023 & n.charCodeAt(++i)), t <= 127 ? ++e : e += t <= 2047 ? 2 : t <= 65535 ? 3 : 4
      }
      return e
    }

    function un(n) {
      var e = ln(n) + 1,
        i = Lg(e);
      return i && on(n, H, i, e), i
    }

    function fn(n) {
      var e = ln(n) + 1,
        i = xg(e);
      return on(n, H, i, e), i
    }

    function cn(n, e) {
      H.set(n, e)
    }

    function sn(n, e, i) {
      for (var t = 0; t < n.length; ++t) H[e++ >> 0] = n.charCodeAt(t);
      i || (H[e >> 0] = 0)
    }

    function dn(n, e) {
      return n % e > 0 && (n += e - n % e), n
    }

    function mn(e) {
      q = e, n.HEAP8 = window.EMSCRIPTEN_HEAP8 = H = new Int8Array(e), n.HEAP16 = window.EMSCRIPTEN_HEAP16 = Y = new Int16Array(e), n.HEAP32 = window.EMSCRIPTEN_HEAP32 = Z = new Int32Array(e), n.HEAPU8 = window.EMSCRIPTEN_HEAPU8 = V = new Uint8Array(e), n.HEAPU16 = window.EMSCRIPTEN_HEAPU16 = J = new Uint16Array(e), n.HEAPU32 = window.EMSCRIPTEN_HEAPU32 = Q = new Uint32Array(e), n.HEAPF32 = window.EMSCRIPTEN_HEAPF32 = $ = new Float32Array(e), n.HEAPF64 = window.EMSCRIPTEN_HEAPF64 = nn = new Float64Array(e)
    }
    var pn = 5242880,
      yn = (n.INITIAL_MEMORY, []),
      vn = [],
      _n = [],
      gn = [],
      hn = [],
      wn = !1;

    function Sn() {
      if (n.preRun)
        for ("function" == typeof n.preRun && (n.preRun = [n.preRun]); n.preRun.length;) Dn(n.preRun.shift());
      Nn(yn)
    }

    function Cn() {
      wn = !0, n.noFSInit || Xc.init.initialized || Xc.init(), void 0 !== Dr && Dr(unityNamespace.ttlAssetBundle ? unityNamespace.ttlAssetBundle : 5), Ac.init(), Fc.root = Xc.mount(Fc, {}, null), Cs.root = Xc.mount(Cs, {}, null), Nn(vn)
    }

    function En() {
      Xc.ignorePermissions = !1, Nn(_n)
    }

    function bn() {
      !0
    }

    function Wn() {
      if (n.postRun)
        for ("function" == typeof n.postRun && (n.postRun = [n.postRun]); n.postRun.length;) kn(n.postRun.shift());
      Nn(hn)
    }

    function Dn(n) {
      yn.unshift(n)
    }

    function An(n) {
      vn.unshift(n)
    }

    function kn(n) {
      hn.unshift(n)
    }
    var Mn = GameGlobal.unityNamespace.runDependencies = 0,
      xn = null,
      Xn = null;

    function jn(e) {
      GameGlobal.manager.Logger.eventLog("addRunDependency: ", e), Mn++, n.monitorRunDependencies && n.monitorRunDependencies(Mn, e)
    }

    function Tn(e) {
      if (GameGlobal.manager.Logger.eventLog("removeRunDependency: ", e), Mn--, n.monitorRunDependencies && n.monitorRunDependencies(Mn, e), 0 == Mn && (null !== xn && (clearInterval(xn), xn = null), Xn)) {
        var i = Xn;
        Xn = null, i()
      }
    }

    function r(e) {
      throw n.onAbort && n.onAbort(e), x(e += ""), I = !0, O = 1, n.IsWxGame && window.WXWASMSDK.WXUncaughtException(!0), e = "abort(" + e + "). Build with -s ASSERTIONS=1 for more info.", new WebAssembly.RuntimeError(e)
    }
    n.preloadedImages = {}, n.preloadedAudios = {};
    var Ln = "data:application/octet-stream;base64,";

    function Fn(n) {
      return n.startsWith(Ln)
    }

    function Pn(n) {
      return n.startsWith("file://")
    }
    var Rn, Bn, Gn = "build.wasm";

    function On() {
      if (!L && (h || w)) {
        if ("function" == typeof fetch && !Pn(Gn)) return fetch(Gn, {
          credentials: "same-origin"
        }).then((function(n) {
          if (!n.ok) throw "failed to load wasm binary file at '" + Gn + "'";
          return n.arrayBuffer()
        })).catch((function() {}));
        if (b && !n.IsWxGame) return new Promise((function(n, e) {
          b(Gn, (function(e) {
            n(new Uint8Array(e))
          }), e)
        }))
      }
      return Promise.resolve().then((function() {}))
    }

    function In() {
      var e = {
        a: vg,
        wx: {
          ignore_opt_glue_apis: ["glGenTextures", "glBindTexture", "glDeleteTextures", "glFramebufferTexture2D", "glIsTexture", "glCompressedTexImage2D", "glGetString"],
          wx_disable_wasm_opt: (wx.getDeviceInfo ? "ios" == wx.getDeviceInfo().platform : "ios" == wx.getSystemInfoSync().platform) ? 2 == GameGlobal.managerConfig.contextConfig.contextType ? 1 : 0 : 1
        }
      };

      function i(e, i) {
        var t = e.exports;
        n.asm = t, mn((B = n.asm.Mp).buffer), n.asm.nq, An(n.asm.Np), n.wasmInstantiated && (n.wasmInstantiated(), Tn("wasm-instantiate"))
      }

      function t(n) {
        i(n.instance)
      }

      function o(i) {
        return On().then((function(i) {
          return n.wasmBin ? WebAssembly.instantiate(n.wasmBin, e) : WebAssembly.instantiate(n.wasmPath, e)
        })).then(i, (function(n) {
          x("failed to asynchronously prepare wasm: " + n), r(n)
        }))
      }
      if (jn("wasm-instantiate"), jn("wasm-preloadAssets"), GameGlobal.manager.TimeLogger.timeStart("wasm编译耗时"), n.instantiateWasm) try {
        return n.instantiateWasm(e, i)
      } catch (n) {
        return x("Module.instantiateWasm callback failed with error: " + n), !1
      }
      return L || "function" != typeof WebAssembly.instantiateStreaming || Fn(Gn) || Pn(Gn) || "function" != typeof fetch ? o(t) : fetch(Gn, {
        credentials: "same-origin"
      }).then((function(n) {
        return WebAssembly.instantiateStreaming(n, e).then(t, (function(n) {
          return x("wasm streaming compile failed: " + n), x("falling back to ArrayBuffer instantiation"), o(t)
        }))
      })), {}
    }
    Fn(Gn) || (Gn = k(Gn));
    var Kn = {
      4476480: function() {
        return n.webglContextAttributes.premultipliedAlpha
      },
      4476541: function() {
        return n.webglContextAttributes.preserveDrawingBuffer
      },
      4476605: function() {
        return n.webglContextAttributes.powerPreference
      }
    };

    function Nn(e) {
      for (; e.length > 0;) {
        var i = e.shift();
        if ("function" != typeof i) {
          var t = i.func;
          "number" == typeof t ? void 0 === i.arg ? Zg.call(null, t) : (r = i.arg, zg.apply(null, [t, r])) : t(void 0 === i.arg ? null : i.arg)
        } else i(n)
      }
      var r
    }

    function Un(n) {
      return n.replace(/\b_Z[\w\d_]+/g, (function(n) {
        return n === n ? n : n + " [" + n + "]"
      }))
    }

    function zn(e, i, t) {
      var r = n["dynCall_" + e];
      return t && t.length ? r.apply(null, [i].concat(t)) : r.call(null, i)
    }

    function qn(n, e, i) {
      return zn(n, e, i)
    }

    function Hn() {
      var n = new Error;
      if (!n.stack) {
        try {
          throw new Error
        } catch (e) {
          n = e
        }
        if (!n.stack) return "(no stack trace available)"
      }
      return n.stack.toString()
    }
    var Vn = 0;

    function Yn() {
      return G || Vn > 0
    }

    function Jn() {
      var e = Hn();
      return n.extraStackTrace && (e += "\n" + n.extraStackTrace()), Un(e)
    }

    function Zn(n) {
      var e = GameGlobal.dnSDK.track("AD_CLICK", {
        ad_placement_name: n
      });
      null != e && 0 !== e.code ? console.warn("WXAMS AD_CLICK failed, code:", e.code, "message:", e.message) : null != e && 0 === e.code && console.log("WXAMS AD_CLICK success")
    }

    function Qn(n) {
      var e = GameGlobal.dnSDK.track("AD_PLACEMENT_SHOW", {
        ad_placement_name: n
      });
      null != e && 0 !== e.code ? console.warn("WXAMS AD_PLACEMENT_SHOW failed, code:", e.code, "message:", e.message) : null != e && 0 === e.code && console.log("WXAMS AD_PLACEMENT_SHOW success")
    }

    function $n(n) {
      var e = GameGlobal.dnSDK.track("AD_VIDEO_FINISH", {
        ad_placement_name: n
      });
      null != e && 0 !== e.code ? console.warn("WXAMS AD_VIDEO_FINISH failed, code:", e.code, "message:", e.message) : null != e && 0 === e.code && console.log("WXAMS AD_VIDEO_FINISH success")
    }

    function ne() {
      var n = GameGlobal.dnSDK.track("AD_IMPRESSION", {
        ad_type: 4
      });
      null != n && 0 !== n.code ? console.warn("WXAMS BANNER_SHOW failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS BANNER_SHOW success")
    }

    function ee(n) {
      window.WXWASMSDK.CheckIsAddedToMyMiniProgram(n)
    }
    var ie = {};

    function te() {
      return void 0 !== ie.fs
    }

    function re(n, e, i, t, r) {
      window.WXWASMSDK.CreateFeedBackButton(n, e, i, t, r)
    }

    function oe(n) {
      var e = Fo(n);
      window.WXWASMSDK.DestroyPageManager(e)
    }

    function ae(n) {
      "undefined" != typeof GameGlobal && GameGlobal.monkeyCallback(Fo(n))
    }

    function le() {
      var n = GameGlobal.dnSDK.track("AD_IMPRESSION", {
        ad_type: 3
      });
      null != n && 0 !== n.code ? console.warn("WXAMS GRIDAD_SHOW failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS GRIDAD_SHOW success")
    }

    function ue(n) {
      var e = Fo(n),
        i = window.WXWASMSDK.GetABValue(e),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function fe(n) {
      window.WXWASMSDK.GetAuthorizeSetting(n)
    }

    function ce(n) {
      var e = Fo(n),
        i = window.WXWASMSDK.GetJsonValue(e),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function se() {
      var n = ln(ie.cache.obsolete) + 1,
        e = Lg(n);
      return an(ie.cache.obsolete, e, n), ie.cache.obsolete = "", e
    }

    function de() {
      window.WXWASMSDK.GetPrivacySetting()
    }

    function me() {
      window.WXWASMSDK.GetSubGameUpdateStatus()
    }

    function pe() {
      var n = window.WXWASMSDK.GetWXDATA_CDN(),
        e = ln(n) + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function ye() {
      window.WXWASMSDK.HideFeedBackButton()
    }

    function ve() {
      var n = GameGlobal.dnSDK.track("AD_IMPRESSION", {
        ad_type: 2
      });
      null != n && 0 !== n.code ? console.warn("WXAMS INTERSTITIAL_SHOW failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS INTERSTITIAL_SHOW success")
    }

    function _e() {
      return window.WXWASMSDK.IsInitWXAMS
    }

    function ge() {
      return !!window.wx.createPageManager
    }

    function he(n, e, i, t, r, o, a, l, u, f) {
      console.log("call JSReportUnityProfileData \n");
      let c = {
        timestamp: (new Date).getTime(),
        fps: {
          targetFrameRate: n,
          avgEXFrameTime: Nr()
        },
        profiler: {
          monoHeapReserved: e,
          monoHeapUsed: i,
          nativeReserved: t,
          nativeUnused: r,
          nativeAllocated: o
        },
        render: {
          setPassCalls: a,
          drawCalls: l,
          vertices: u,
          trianglesCount: f
        },
        webassembly: {
          totalHeapMemory: Jr(),
          dynamicMemory: Kr(),
          usedHeapMemory: $r(),
          unAllocatedMemory: Qr()
        },
        assetbundle: {
          numberInMemory: Pr(),
          numberOnDisk: Rr(),
          sizeInMemory: Br(),
          sizeOnDisk: Gr()
        }
      };
      GameGlobal.manager.getGameDataMonitor().reportUnityProfileData(c)
    }

    function we() {
      console.log("call JSStartGameDataMonitor \n"), "function" == typeof GameGlobal.manager.getGameDataMonitor ? GameGlobal.manager.getGameDataMonitor().start() : console.log("GameGlobal.manager.getGameDataMonitor is not a function \n")
    }
    var Se = null,
      Ce = 0;

    function Ee() {
      return Se && Se.activated || 0 != Ce
    }
    var be = 1,
      We = {
        x: 0,
        y: 0,
        z: 0
      };

    function De() {
      We = {
        x: Se.x * be,
        y: Se.y * be,
        z: Se.z * be
      }, 0 != Ce && Zw(Ce, We.x, We.y, We.z)
    }
    var Ae = 0,
      ke = 0,
      Me = 0,
      xe = 0,
      Xe = 0;

    function je(n, e) {
      var i = {
          x: n.x - e.x,
          y: n.y - e.y,
          z: n.z - e.z
        },
        t = i.x * i.x + i.y * i.y + i.z * i.z,
        r = {
          x: n.x + e.x,
          y: n.y + e.y,
          z: n.z + e.z
        };
      return t <= r.x * r.x + r.y * r.y + r.z * r.z ? i : r
    }

    function Te(n) {
      var e = {
        x: n.accelerationIncludingGravity.x * be,
        y: n.accelerationIncludingGravity.y * be,
        z: n.accelerationIncludingGravity.z * be
      };
      0 != Ce && Zw(Ce, e.x, e.y, e.z);
      var i = {
        x: n.acceleration.x * be,
        y: n.acceleration.y * be,
        z: n.acceleration.z * be
      };
      if (0 != Me && Zw(Me, i.x, i.y, i.z), 0 != xe) {
        var t = je(e, i);
        Zw(xe, t.x, t.y, t.z)
      }
      if (0 != Xe) {
        var r = Math.PI / 180;
        Zw(Xe, n.rotationRate.alpha * r, n.rotationRate.beta * r, n.rotationRate.gamma * r)
      }
    }
    var Le = 0;

    function Fe(n) {
      1 & n && "function" == typeof DeviceOrientationEvent.requestPermission && DeviceOrientationEvent.requestPermission().then((function(n) {
        "granted" === n ? Le &= -2 : T("DeviceOrientationEvent permission not granted")
      })).catch((function(n) {
        T(n), Le |= 1
      })), 2 & n && "function" == typeof DeviceMotionEvent.requestPermission && DeviceMotionEvent.requestPermission().then((function(n) {
        "granted" === n ? Le &= -3 : T("DeviceMotionEvent permission not granted")
      })).catch((function(n) {
        T(n), Le |= 2
      }))
    }

    function Pe() {
      0 == Ce && 0 == Me && 0 == xe && 0 == Xe && (Fe(2), window.addEventListener("devicemotion", Te))
    }

    function Re() {
      var n = 9.80665;
      be = /(iPhone|iPad|Macintosh)/i.test(navigator.userAgent) ? 1 / n : -1 / n
    }

    function Be(n, e) {
      if (Re(), "undefined" == typeof Accelerometer) return Pe(), void(0 != n && (Ce = n));

      function i(n) {
        (Se = new Accelerometer({
          frequency: n,
          referenceFrame: "device"
        })).addEventListener("reading", De), Se.addEventListener("error", (function(n) {
          T(n.error ? n.error : n)
        })), Se.start(), ke = n
      }
      0 != n && (Ce = n), Se ? ke != e && (Se.stop(), Se.removeEventListener("reading", De), i(e)) : 0 != Ae ? Ae = e : (Ae = e, navigator.permissions.query({
        name: "accelerometer"
      }).then((function(n) {
        "granted" === n.state ? i(Ae) : T("No permission to use Accelerometer."), Ae = 0
      })))
    }

    function Ge() {
      0 == Ce && 0 == Me && 0 == xe && 0 == Xe && window.removeEventListener("devicemotion", ki)
    }

    function Oe() {
      Se ? ("undefined" == typeof GravitySensor && 0 != xe || (Se.stop(), Se.removeEventListener("reading", De), Se = null), Ce = 0, ke = 0) : 0 != Ce && (Ce = 0, Ge())
    }
    var Ie = 0;

    function Ke(e) {
      if (!Ie) try {
        Zg.call(null, e)
      } catch (e) {
        throw Ie = 1, x("Uncaught exception from main loop:"), x(e), x("Halting program."), n.errorHandler && n.errorHandler(e), e
      }
    }

    function Ne(e, i) {
      for (var t = "", r = 0; r < i; r++) t += String.fromCharCode(V[e + r]);
      n.canvas.style.cursor = "url(data:image/cur;base64," + btoa(t) + "),default"
    }

    function Ue(e) {
      n.canvas.style.cursor = e ? "default" : "none"
    }

    function ze(n) {
      return void 0 !== window.CSS && void 0 !== window.CSS.escape ? window.CSS.escape(n) : n.replace(/(#|\.|\+|\[|\]|\(|\)|\{|\})/g, "\\$1")
    }

    function qe() {
      return "#" + ze(n.canvas ? n.canvas.id : "unity-canvas")
    }

    function He(n, e, i, t) {
      var r = document.querySelector(qe()),
        o = r && r.getBoundingClientRect();
      Q[i >> 2] = n - (o ? o.left : 0), Q[t >> 2] = e - (o ? o.top : 0)
    }

    function Ve(n) {
      var e = ln(n) + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function Ye() {
      var n = qe();
      return Ye.selector != n && (Fg(Ye.ptr), Ye.ptr = Ve(n), Ye.selector = n), Ye.ptr
    }

    function Je(n) {
      var e = rn(n);
      window.open(e, "_blank", "")
    }
    var Ze = {
      numPendingSync: 0,
      syncInternal: 1e3,
      syncInProgress: !1,
      sync: function(n) {
        if (n) {
          if (0 == Ze.numPendingSync) return
        } else if (Ze.syncInProgress) return void Ze.numPendingSync++;
        Ze.syncInProgress = !0, Xc.syncfs(!1, (function(n) {
          Ze.syncInProgress = !1
        })), Ze.numPendingSync = 0
      }
    };

    function Qe() {
      n.indexedDB && n.indexedDB && n.setInterval((function() {
        Ze.sync(!0)
      }), Ze.syncInternal)
    }

    function $e() {
      n.indexedDB && (xc.queuePersist(n.__unityIdbfsMount.mount), window.warnedAboutManualFilesystemSyncGettingDeprecated || (window.warnedAboutManualFilesystemSyncGettingDeprecated = !0, n.autoSyncPersistentDataPath || console.warn("Manual synchronization of Unity Application.persistentDataPath via JS_FileSystem_Sync() is deprecated and will be later removed in a future Unity version. The persistent data directory will be automatically synchronized instead on file modification. Pass config.autoSyncPersistentDataPath = true; to configuration in createUnityInstance() to opt in to the new behavior.")))
    }
    var ni = null;

    function ei() {
      return "undefined" != typeof GravitySensor ? ni && ni.activated : 0 != xe
    }

    function ii() {
      0 != xe && Zw(xe, ni.x * be, ni.y * be, ni.z * be)
    }
    var ti = 0,
      ri = null;

    function oi() {
      var n = {
        x: ri.x * be,
        y: ri.y * be,
        z: ri.z * be
      };
      if (0 != Me && Zw(Me, n.x, n.y, n.z), 0 != xe && "undefined" == typeof GravitySensor) {
        var e = je(We, n);
        Zw(xe, e.x, e.y, e.z)
      }
    }
    var ai = 0,
      li = 0;

    function ui(n, e) {
      if (Re(), "undefined" == typeof LinearAccelerationSensor) return Pe(), void(0 != n && (Me = n));

      function i(n) {
        (ri = new LinearAccelerationSensor({
          frequency: n,
          referenceFrame: "device"
        })).addEventListener("reading", oi), ri.addEventListener("error", (function(n) {
          T(n.error ? n.error : n)
        })), ri.start(), li = n
      }
      0 != n && (Me = n), ri ? li != e && (ri.stop(), ri.removeEventListener("reading", oi), i(e)) : 0 != ai ? ai = e : (ai = e, navigator.permissions.query({
        name: "accelerometer"
      }).then((function(n) {
        "granted" === n.state ? i(ai) : T("No permission to use LinearAccelerationSensor."), ai = 0
      })))
    }

    function fi(n, e) {
      if ("undefined" == typeof GravitySensor) return Be(0, Math.max(e, ke)), ui(0, Math.max(e, li)), void(xe = n);

      function i(n) {
        (ni = new GravitySensor({
          frequency: n,
          referenceFrame: "device"
        })).addEventListener("reading", ii), ni.addEventListener("error", (function(n) {
          T(n.error ? n.error : n)
        })), ni.start()
      }
      Re(), xe = n, ni ? (ni.stop(), ni.removeEventListener("reading", ii), i(e)) : 0 != ti ? ti = e : (ti = e, navigator.permissions.query({
        name: "accelerometer"
      }).then((function(n) {
        "granted" === n.state ? i(ti) : T("No permission to use GravitySensor."), ti = 0
      })))
    }

    function ci() {
      ri ? ("undefined" == typeof GravitySensor && 0 != xe || (ri.stop(), ri.removeEventListener("reading", oi), ri = null), Me = 0, li = 0) : 0 != Me && (Me = 0, Ge())
    }

    function si() {
      if (xe = 0, "undefined" == typeof GravitySensor) return 0 == Ce && Oe(), void(0 == Me && ci());
      ni && (ni.stop(), ni.removeEventListener("reading", ii), ni = null)
    }

    function di(n) {
      try {
        Zg.call(null, n)
      } catch (n) {
        console.warn(n)
      }
    }
    var mi = null;

    function pi() {
      return mi && mi.activated || 0 != Xe
    }

    function yi() {
      0 != Xe && Zw(Xe, mi.x, mi.y, mi.z)
    }
    var vi = 0;

    function _i(n, e) {
      if ("undefined" == typeof Gyroscope) return Pe(), void(Xe = n);

      function i(n) {
        (mi = new Gyroscope({
          frequency: n,
          referenceFrame: "device"
        })).addEventListener("reading", yi), mi.addEventListener("error", (function(n) {
          T(n.error ? n.error : n)
        })), mi.start()
      }
      Xe = n, mi ? (mi.stop(), mi.removeEventListener("reading", yi), i(e)) : 0 != vi ? vi = e : (vi = e, navigator.permissions.query({
        name: "gyroscope"
      }).then((function(n) {
        "granted" === n.state ? i(vi) : T("No permission to use Gyroscope."), vi = 0
      })))
    }

    function gi() {
      mi ? (mi.stop(), mi.removeEventListener("reading", yi), mi = null, Xe = 0) : 0 != Xe && (Xe = 0, Ge())
    }

    function hi() {
      if (n.IsWxGame) return;
      const e = function(n) {
        "canvas" !== n.target.localName && _g()
      };
      document.addEventListener("contextmenu", e), n.deinitializers.push((function() {
        document.removeEventListener("contextmenu", e)
      }))
    }

    function wi() {
      return ri && ri.activated || 0 != Me
    }

    function Si(n, e) {
      var i = rn(n);
      switch ("function" == typeof dump && dump(i), e) {
        case 0:
        case 1:
        case 4:
          if (i.startsWith("An abnormal situation")) {
            if (null != GameGlobal.logAbNormalOnce) return;
            GameGlobal.logAbNormalOnce = 1
          }
          if (i.indexOf("is corrupted! Remove it and launch unity again!") > -1) return;
          return void x(i);
        case 2:
          return void console.warn(i);
        case 3:
        case 5:
          return void console.log(i);
        default:
          x("Unknown console message type!"), x(i)
      }
    }

    function Ci(n, e) {
      var i = Jn();
      return n && an(i, n, e), ln(i)
    }
    var Ei = null,
      bi = 0;

    function Wi() {
      return Ei && Ei.activated || 0 != bi
    }

    function Di() {
      0 != bi && Jw(bi, Ei.quaternion[0], Ei.quaternion[1], Ei.quaternion[2], Ei.quaternion[3])
    }
    var Ai = 0;

    function ki(n) {
      if (bi) {
        var e = Math.PI / 180,
          i = n.beta * e,
          t = n.gamma * e,
          r = n.alpha * e,
          o = Math.cos(i / 2),
          a = Math.sin(i / 2),
          l = Math.cos(t / 2),
          u = Math.sin(t / 2),
          f = Math.cos(r / 2),
          c = Math.sin(r / 2);
        Jw(bi, a * l * f - o * u * c, o * u * f + a * l * c, o * l * c + a * u * f, o * l * f - a * u * c)
      }
    }

    function Mi(n, e) {
      function i(n) {
        (Ei = new RelativeOrientationSensor({
          frequency: n,
          referenceFrame: "device"
        })).addEventListener("reading", Di), Ei.addEventListener("error", (function(n) {
          T(n.error ? n.error : n)
        })), Ei.start()
      }
      "undefined" != typeof RelativeOrientationSensor ? (bi = n, Ei ? (Ei.stop(), Ei.removeEventListener("reading", Di), i(e)) : 0 != Ai ? Ai = e : (Ai = e, Promise.all([navigator.permissions.query({
        name: "accelerometer"
      }), navigator.permissions.query({
        name: "gyroscope"
      })]).then((function(n) {
        n.every((function(n) {
          return "granted" === n.state
        })) ? i(Ai) : T("No permissions to use RelativeOrientationSensor."), Ai = 0
      })))) : 0 == bi && (bi = n, Fe(1), window.addEventListener("deviceorientation", ki))
    }

    function xi() {
      Ei ? (Ei.stop(), Ei.removeEventListener("reading", Di), Ei = null) : 0 != bi && window.removeEventListener("deviceorientation", ki), bi = 0
    }

    function Xi() {
      0 != Le && Fe(Le)
    }

    function ji() {
      n.QuitCleanup()
    }
    var Ti = 0;

    function Li() {
      Ti && Hg(Ti, window.innerWidth, window.innerHeight, screen.orientation ? screen.orientation.angle : window.orientation)
    }

    function Fi() {
      Ti = 0, window.removeEventListener("resize", Li), screen.orientation && screen.orientation.removeEventListener("change", Li)
    }

    function Pi(n) {
      Ti || (screen.orientation && screen.orientation.addEventListener("change", Li), window.addEventListener("resize", Li), Ti = n, setTimeout(Li, 0))
    }
    var Ri = -1,
      Bi = -1,
      Gi = -1;

    function Oi(n) {
      screen.orientation && screen.orientation.lock && (Ri = n, -1 == Gi && n != Bi && (Gi = setTimeout((function n() {
        var e = ["any", 0, "landscape", "portrait", "portrait-primary", "portrait-secondary", "landscape-primary", "landscape-secondary"][Bi = Ri];
        screen.orientation.lock(e).then((function() {
          Gi = Ri != Bi ? setTimeout(n, 0) : -1
        })).catch((function(n) {
          T(n), Gi = -1
        }))
      }), 0)))
    }

    function Ii(n, e) {
      var i = UnityLoader.SystemInfo.browser;
      return n && an(i, n, e), ln(i)
    }

    function Ki(n, e) {
      var i = UnityLoader.SystemInfo.browserVersion;
      return n && an(i, n, e), ln(i)
    }

    function Ni(e, i, t) {
      var r = rn(e),
        o = "#canvas" == r ? n.canvas : document.querySelector(r),
        a = 0,
        l = 0;
      if (o) {
        var u = o.getBoundingClientRect();
        a = u.width, l = u.height
      }
      nn[i >> 3] = a, nn[t >> 3] = l
    }

    function Ui(n, e) {
      return n && an(GameGlobal.unityNamespace.DATA_CDN || "https://game.weixin.qq.com", n, e), ln(GameGlobal.unityNamespace.DATA_CDN || "https://game.weixin.qq.com")
    }

    function zi(n, e) {
      var i = UnityLoader.SystemInfo.gpu;
      return n && an(i, n, e), ln(i)
    }

    function qi(n, e) {
      var i = UnityLoader.SystemInfo.language;
      return n && an(i, n, e), ln(i)
    }

    function Hi() {
      return n.matchWebGLToCanvasSize || void 0 === n.matchWebGLToCanvasSize
    }

    function Vi() {
      return V.length / 1048576
    }

    function Yi(n, e) {
      var i = UnityLoader.SystemInfo.os + " " + UnityLoader.SystemInfo.osVersion;
      return n && an(i, n, e), ln(i)
    }

    function Ji() {
      return 0 == n.matchWebGLToCanvasSize ? 1 : n.devicePixelRatio || window.devicePixelRatio || 1
    }

    function Zi(n, e) {
      nn[n >> 3] = UnityLoader.SystemInfo.width, nn[e >> 3] = UnityLoader.SystemInfo.height
    }

    function Qi(e, i) {
      return n.IsWxGame && (n.streamingAssetsUrl = n.resolveBuildUrl("StreamingAssets")), e && an(n.streamingAssetsUrl, e, i), ln(n.streamingAssetsUrl)
    }

    function $i() {
      var e = lg.getExtension("WEBGL_compressed_texture_astc");
      return !(!e || !e.getSupportedProfiles) && (!n.IsWxGame && e.getSupportedProfiles().includes("hdr"))
    }

    function nt() {
      return UnityLoader.SystemInfo.hasCursorLock
    }

    function et() {
      return UnityLoader.SystemInfo.hasFullscreen
    }

    function it() {
      return UnityLoader.SystemInfo.hasWebGL
    }

    function tt() {
      return UnityLoader.SystemInfo.mobile
    }

    function rt() {
      return !!n.shouldQuit
    }
    var ot = {
      requests: {},
      responses: {},
      abortControllers: {},
      timer: {},
      nextRequestId: 1
    };

    function at(n) {
      var e = ot.abortControllers[n];
      e && !e.signal.aborted && e.abort()
    }

    function lt(n, e) {
      var i = rn(n),
        t = rn(e),
        r = new GameGlobal.unityNamespace.UnityLoader.UnityCache.XMLHttpRequest;
      GameGlobal.TEXTURE_PARALLEL_BUNDLE && GameGlobal.ParalleLDownloadTexture(i);
      var o = {
        url: i,
        init: {
          method: t,
          signal: r.signal,
          headers: {},
          enableStreamingDownload: !1
        },
        tempBuffer: null,
        tempBufferSize: 0
      };
      return ot.abortControllers[ot.nextRequestId] = r, ot.requests[ot.nextRequestId] = o, ot.nextRequestId++
    }

    function ut(n) {
      var e = ot.responses[n];
      if (!e) return "";
      if (e.headerString) return e.headerString;
      for (var i = "", t = e.headers.entries(), r = t.next(); !r.done; r = t.next()) i += r.value[0] + ": " + r.value[1] + "\r\n";
      return e.headerString = i, i
    }

    function ft(n, e, i, t, r) {
      var o = ot.responses[n];
      if (!o) return an("", e, i), void an("", t, r);
      e && an(ut(n), e, i);
      t && an(o.url, t, r)
    }

    function ct(n, e) {
      var i = ot.responses[n];
      if (!i) return Q[e >> 2] = 0, void(Q[1 + (e >> 2)] = 0);
      var t = ut(n);
      Q[e >> 2] = ln(t), Q[1 + (e >> 2)] = ln(i.url)
    }

    function st(n) {
      ot.timer[n] && clearTimeout(ot.timer[n]), delete ot.requests[n], delete ot.responses[n], delete ot.abortControllers[n], delete ot.timer[n]
    }

    function dt(n, e, i, t) {
      var r = ot.abortControllers[n];
      r.retryCount = r.retryCount || 0, r.retryCount++;
      var o = new GameGlobal.unityNamespace.UnityLoader.UnityCache.XMLHttpRequest;
      o.open("GET", r.paramsCache.url, !0), o.responseType = r.responseType, o.onload = function() {
        if (r.status >= 400 && t) return setTimeout((function() {
          dt(n, e, i)
        }), 1e3), !1;
        if (i) {
          var a = new Uint8Array(o.response);
          if (0 != a.length) {
            var l = Lg(a.length);
            V.set(a, l), qn("viiiiii", i, [e, o.status, l, a.length, 0, 0])
          } else qn("viiiiii", i, [e, o.status, 0, 0, 0, 0])
        }
      }, o.send(r.postData), o.onerror = r.onerror, o.ontimeout = r.ontimeout, o.onabort = r.onabort, console.error("load url error:" + r.paramsCache.url), GameGlobal.logmanager.warn("load url error:" + r.paramsCache.url), GameGlobal.realtimeLogManager.error("load url error:" + r.paramsCache.url)
    }

    function mt(e, i, t, r, o, a) {
      var l = ot.requests[e],
        u = ot.abortControllers[e];

      function f() {
        ot.timer[e] && (clearTimeout(ot.timer[e]), delete ot.timer[e])
      }

      function c(n, i) {
        if (u.retryCount = u.retryCount || 0, void 0 !== u && "GET" === u.paramsCache.method && /\b(settings|catalog)\.json\b/.test(u.paramsCache.url) && u.retryCount < 2) return setTimeout((function() {
          dt(e, r, o)
        }), 1e3);
        if (f(), o) {
          var t = ln(n) + 1,
            a = Lg(t);
          an(n, a, t), qn("viiiiii", o, [r, 500, 0, 0, a, i]), Fg(a), l.tempBuffer && Fg(l.tempBuffer)
        }
      }

      function s(n) {
        if (a && n.lengthComputable) {
          var i = n.response;
          if (ot.responses[e] = i, n.chunk) {
            var t = function(n) {
              if (!l.tempBuffer) {
                const e = Math.max(n, 1024);
                l.tempBuffer = Lg(e), l.tempBufferSize = e
              }
              return l.tempBufferSize < n && (Fg(l.tempBuffer), l.tempBuffer = Lg(n), l.tempBufferSize = n), l.tempBuffer
            }(n.chunk.length);
            V.set(n.chunk, t), qn("viiiiii", a, [r, i.status, n.loaded, n.total, t, n.chunk.length])
          } else qn("viiiiii", a, [r, i.status, n.loaded, n.total, 0, 0])
        }
      }
      try {
        if (t > 0) {
          var d = V.subarray(i, i + t);
          l.init.body = d
        }
        l.timeout && (ot.timer[e] = setTimeout((function() {
          l.isTimedOut = !0, u.abort()
        }), l.timeout));
        n.fetchWithProgress;
        l.init.onProgress = s, n.companyName && n.productName && n.cachedFetch && (n.cachedFetch, l.init.companyName = n.companyName, l.init.productName = n.productName, l.control = n.cacheControl(l.url)), (0, u.openAndSend)(l.url, l.init).then((function(n) {
          ot.responses[e] = n,
            function(n, e) {
              if (f(), o) {
                s({
                  response: n,
                  loaded: !0,
                  lengthComputable: !0,
                  total: 100,
                  type: "progress"
                });
                if (l.init.enableStreamingDownload) qn("viiiiii", o, [r, n.status, 0, e.length, 0, 0]);
                else if (void 0 !== ie && ie.isWXAssetBundle(n.url)) {
                  if (!n.url.startsWith(GameGlobal.unityNamespace.DATA_CDN)) {
                    var i = Lg(e.length);
                    return V.set(e, i), void qn("viiiiii", o, [r, n.status, i, e.length, 0, 0])
                  }
                  var t = n.originXHR;
                  if (0 == e.length || t.status >= 400) return GameGlobal.manager.reporter.wxAssetBundle.reportEmptyContent({
                    stage: ie.WXABErrorSteps.kWebRequestResponse,
                    fromCache: t.isReadFromCache,
                    httpStatus: t.status,
                    path: t.paramsCache.url,
                    size: 0
                  }), GameGlobal.manager.Logger.pluginLog("[WXAssetBundle]WebRequest status: " + t.status + ", url: " + t.paramsCache.url + " return size " + e.length + " from " + (t.isReadFromCache ? "Cache" : "CDN")), void qn("viiiiii", o, [r, n.status, Lg(1), 1, 0, 0]);
                  t.onsave = function(n) {
                    ie.cache.cleanable(ie.path2fd.get(n))
                  };
                  var a = t.response;
                  let l = ie.url2path(n.url),
                    u = ie.path2fd.get(l);
                  null == u && (u = ie.newfd(), ie.path2fd.set(l, u));
                  let f = ie.fd2wxStream.get(u);
                  f = {
                    node: {
                      mode: 32768,
                      usedBytes: e.length
                    },
                    fd: u,
                    path: l,
                    seekable: !0,
                    position: 0,
                    stream_ops: Mc.stream_ops,
                    ungotten: [],
                    error: !1
                  }, f.stream_ops.read = ie.read, ie.fd2wxStream.set(u, f), ie.cache.put(u, a, t.isReadFromCache), qn("viiiiii", o, [r, t.status, 0, 0, 0, 0]), ie.disk.set(unityNamespace.PathInFileOS(l), e.length)
                } else if (u.ignoreCallback || 0 == e.length) qn("viiiiii", o, [r, n.status, 0, 0, 0, 0]);
                else {
                  i = Lg(e.length);
                  V.set(e, i), qn("viiiiii", o, [r, n.status, i, e.length, 0, 0])
                }
                l.tempBuffer && Fg(l.tempBuffer)
              }
            }(n, n.parsedBody)
        })).catch((function(n) {
          l.isTimedOut ? c("Connection timed out.", 14) : u.signal.aborted ? c("Aborted.", 17) : c(n.message, 2)
        }))
      } catch (n) {
        c(n.message, 2)
      }
    }

    function pt(n, e) {
      var i = ot.requests[n];
      i && (i.init.redirect = 0 === e ? "error" : "follow")
    }

    function yt(n, e, i) {
      var t = ot.requests[n];
      if (t) {
        var r = rn(e),
          o = rn(i);
        t.init.headers[r] = o
      }
    }

    function vt(n, e) {
      var i = ot.requests[n];
      i && (i.init.timeout = e, i.timeout = e)
    }

    function _t(n) {
      var e = rn(n),
        i = JSON.parse(e);
      const t = GameGlobal.dnSDK.track("LEVEL_ENTER", {
        enter_level_id: i.enter_level_id,
        enter_level_name: i.enter_level_name,
        game_mode: i.game_mode,
        level_id: i.level_id,
        chapter_id: i.chapter_id,
        coin_amount: i.coin_amount,
        stamina_value: i.stamina_value,
        level_value: i.level_value
      });
      null != t && 0 !== t.code ? console.warn("WXAMS LEVEL_ENTER failed, code:", t.code, "message:", t.message) : null != t && 0 === t.code && console.log("WXAMS LEVEL_ENTER success")
    }

    function gt(n) {
      var e = rn(n),
        i = JSON.parse(e);
      const t = GameGlobal.dnSDK.track("LEVEL_EXIT", {
        ad_cnt: i.ad_cnt,
        duration: i.duration,
        enter_level_id: i.enter_level_id,
        game_mode: i.game_mode,
        items: i.items,
        level_id: i.level_id,
        chapter_id: i.chapter_id,
        coin_amount: i.coin_amount,
        stamina_value: i.stamina_value,
        level_value: i.level_value
      });
      null != t && 0 !== t.code ? console.warn("WXAMS LEVEL_EXIT failed, code:", t.code, "message:", t.message) : null != t && 0 === t.code && console.log("WXAMS LEVEL_EXIT success")
    }

    function ht(n) {
      var e = rn(n),
        i = JSON.parse(e);
      const t = GameGlobal.dnSDK.track("LEVEL_LOSE", {
        ad_cnt: i.ad_cnt,
        duration: i.duration,
        enter_level_id: i.enter_level_id,
        game_mode: i.game_mode,
        items: i.items,
        level_id: i.level_id,
        chapter_id: i.chapter_id,
        coin_amount: i.coin_amount,
        stamina_value: i.stamina_value,
        level_value: i.level_value
      });
      null != t && 0 !== t.code ? console.warn("WXAMS LEVEL_LOSE failed, code:", t.code, "message:", t.message) : null != t && 0 === t.code && console.log("WXAMS LEVEL_LOSE success")
    }

    function wt(n) {
      var e = rn(n),
        i = JSON.parse(e);
      const t = GameGlobal.dnSDK.track("LEVEL_PASS", {
        ad_cnt: i.ad_cnt,
        duration: i.duration,
        enter_level_id: i.enter_level_id,
        game_mode: i.game_mode,
        items: i.items,
        level_id: i.level_id,
        chapter_id: i.chapter_id,
        coin_amount: i.coin_amount,
        stamina_value: i.stamina_value,
        level_value: i.level_value
      });
      null != t && 0 !== t.code ? console.warn("WXAMS LEVEL_PASS failed, code:", t.code, "message:", t.message) : null != t && 0 === t.code && console.log("WXAMS LEVEL_PASS success")
    }

    function St() {
      var n = GameGlobal.dnSDK.track("LOAD_FINISH", {});
      null != n && 0 !== n.code ? console.warn("WXAMS LOAD_FINISH failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS LOAD_FINISH success")
    }

    function Ct() {
      window.WXWASMSDK.OpenPrivacyContract()
    }

    function Et() {
      var n = window.WXWASMSDK.GetOrCreateRecommendPageManager();
      null != n && window.WXWASMSDK.PreLoadPageManager(n, {
        openlink: "TWFRCqV5WeM2AkMXhKwJ03MhfPOieJfAsvXKUbWvQFQtLyyA5etMPabBehga950uzfZcH3Vi3QeEh41xRGEVFw"
      })
    }

    function bt() {
      var n = GameGlobal.dnSDK.track("RETENTION_5S", {});
      null != n && 0 !== n.code ? console.warn("WXAMS RETENTION_5S failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS RETENTION_5S success")
    }

    function Wt() {
      var n = GameGlobal.dnSDK.track("AD_IMPRESSION", {
        ad_type: 1
      });
      null != n && 0 !== n.code ? console.warn("WXAMS REWARD_AD_SHOW failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS REWARD_AD_SHOW success")
    }

    function Dt(n) {
      var e = Fo(n);
      window.WXWASMSDK.RequestSubscribe(e)
    }

    function At(n) {
      var e = Fo(n).split(";");
      window.WXWASMSDK.RequestSubscribeMulti(e)
    }

    function kt() {
      window.WXWASMSDK.RequireOpenPrivacyAuthorize()
    }

    function Mt() {
      var n = GameGlobal.dnSDK.track("SUBSCRIBE", {});
      null != n && 0 !== n.code ? console.warn("WXAMS SUBSCRIBE failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS SUBSCRIBE success")
    }

    function xt(n) {
      var e = rn(n);
      GameGlobal.dnSDK.setOpenId(e)
    }

    function Xt(n) {
      "undefined" != typeof GameGlobal && (GameGlobal.UnityUIType = Fo(n))
    }

    function jt(n, e, i, t, r) {
      var o = Fo(n),
        a = Fo(e);
      window.WXWASMSDK.ShowCPSPageManager(o, a, i, t, r)
    }

    function Tt() {
      var n = window.WXWASMSDK.GetOrCreateRecommendPageManager();
      null != n && window.WXWASMSDK.ShowPageManager(n, {
        openlink: "TWFRCqV5WeM2AkMXhKwJ03MhfPOieJfAsvXKUbWvQFQtLyyA5etMPabBehga950uzfZcH3Vi3QeEh41xRGEVFw"
      })
    }

    function Lt() {
      window.WXWASMSDK.SubGameUpdate()
    }

    function Ft(n, e) {
      var i = rn(n),
        t = ie.url2path(i);
      return !(e && !GameGlobal.manager.fs.accessSync(t)) && (ie.disk.has(t) || ie.disk.set(t, 0), !0)
    }

    function Pt(n) {
      var e = ie.url2path(rn(n)),
        i = ie.path2fd.get(e);
      ie.cache.has(i) && ie.cache.delete(i), ie.disk.has(e) && ie.disk.delete(e)
    }

    function Rt(n) {
      window.WXWASMSDK.WXADDestroy(Fo(n))
    }

    function Bt(n, e) {
      return window.WXWASMSDK.WXADGetStyleValue(Fo(n), Fo(e))
    }

    function Gt(n, e, i) {
      window.WXWASMSDK.WXADLoad(Fo(n), Fo(e), Fo(i))
    }

    function Ot(n, e, i) {
      window.WXWASMSDK.WXADStyleChange(Fo(n), Fo(e), i)
    }

    function It(n, e, i, t) {
      return window.WXWASMSDK.WXAccessFile(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function Kt(n) {
      var e = window.WXWASMSDK.WXAccessFileSync(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Nt(n, e, i, t, r, o, a) {
      window.WXWASMSDK.WXAppendFile(Fo(n), V.slice(e, i + e), Fo(t), Fo(r), Fo(o), Fo(a))
    }

    function Ut(n, e, i, t, r, o) {
      window.WXWASMSDK.WXAppendStringFile(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r), Fo(o))
    }

    function zt(n) {
      window.WXWASMSDK.WXCameraCloseFrameChange(Fo(n))
    }

    function qt(n, e) {
      window.WXWASMSDK.WXCameraCreateCamera(Fo(n), Fo(e))
    }

    function Ht(n) {
      window.WXWASMSDK.WXCameraDestroy(Fo(n))
    }

    function Vt(n) {
      window.WXWASMSDK.WXCameraListenFrameChange(Fo(n))
    }

    function Yt(n) {
      window.WXWASMSDK.WXCameraOnAuthCancel(Fo(n))
    }

    function Jt(n) {
      window.WXWASMSDK.WXCameraOnCameraFrame(Fo(n))
    }

    function Zt(n) {
      window.WXWASMSDK.WXCameraOnStop(Fo(n))
    }

    function Qt(n) {
      if (!n || !Fo(n)) return !1;
      const e = Fo(n);
      return void 0 !== wx[e[0].toLowerCase() + e.slice(1)]
    }

    function $t(n, e) {
      var i = Fo(n);
      window.WXWASMSDK.WXChallengeMiddleUpdate(i, e)
    }

    function nr() {
      window.WXWASMSDK.WXChatClose()
    }

    function er(n) {
      var e = window.WXWASMSDK.WXChatCreate(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function ir() {
      window.WXWASMSDK.WXChatHide()
    }

    function tr(n) {
      window.WXWASMSDK.WXChatOff(Fo(n))
    }

    function rr(n) {
      window.WXWASMSDK.WXChatOn(Fo(n))
    }

    function or(n) {
      window.WXWASMSDK.WXChatOpen(Fo(n))
    }

    function ar(n) {
      window.WXWASMSDK.WXChatSetSignature(Fo(n))
    }

    function lr(n) {
      window.WXWASMSDK.WXChatSetTabs(Fo(n))
    }

    function ur(n) {
      window.WXWASMSDK.WXChatShow(Fo(n))
    }

    function fr() {
      return window.WXWASMSDK.WXCheckIsSupportMidasPayment()
    }

    function cr() {
      var n = window.WXWASMSDK.WXCleanAllFileCache(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function sr(n) {
      var e = window.WXWASMSDK.WXCleanFileCache(n),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function dr(n, e, i, t, r) {
      return window.WXWASMSDK.WXCopyFile(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r))
    }

    function mr(n, e) {
      var i = window.WXWASMSDK.WXCopyFileSync(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function pr(n) {
      var e = window.WXWASMSDK.WXCreateBannerAd(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function yr(n) {
      var e = Fo(n);
      window.WXWASMSDK.WXCreateChallenge(e)
    }

    function vr(n) {
      var e = window.WXWASMSDK.WXCreateCustomAd(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function _r(n, e, i) {
      var t = window.WXWASMSDK.WXCreateFixedBottomMiddleBannerAd(Fo(n), e, i),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t, o, r), o
    }

    function gr(n) {
      var e = window.WXWASMSDK.WXCreateGameClubButton(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function hr(n, e, i, t, r, o, a) {
      var l = window.WXWASMSDK.WXCreateInnerAudioContext(Fo(n), e, i, t, r, o, a),
        u = ln(l || "") + 1,
        f = Lg(u);
      return an(l, f, u), f
    }

    function wr(n) {
      var e = window.WXWASMSDK.WXCreateInterstitialAd(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Sr(n) {
      var e = window.WXWASMSDK.WXCreateRewardedVideoAd(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Cr(n, e, i, t, r, o) {
      var a = window.WXWASMSDK.WXCreateUserInfoButton(n, e, i, t, Fo(r), o),
        l = ln(a || "") + 1,
        u = Lg(l);
      return an(a, u, l), u
    }

    function Er(n) {
      var e = window.WXWASMSDK.WXCreateVideo(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function br(n) {
      window.WXWASMSDK.WXDataContextPostMessage(Fo(n))
    }

    function Wr(n, e) {
      var i = Fo(n);
      window.WXWASMSDK.WXEndChallenge(i, e)
    }

    function Dr(n, e) {
      function i(n) {
        return (i = "function" == typeof Symbol && "symbol" == typeof Symbol.iterator ? function(n) {
          return typeof n
        } : function(n) {
          return n && "function" == typeof Symbol && n.constructor === Symbol && n !== Symbol.prototype ? "symbol" : typeof n
        })(n)
      }

      function t(n, e) {
        if (i = n, !(null != (t = e) && "undefined" != typeof Symbol && t[Symbol.hasInstance] ? t[Symbol.hasInstance](i) : i instanceof t)) throw new TypeError("Cannot call a class as a function");
        var i, t
      }

      function r(n, e) {
        for (var i = 0; i < e.length; i++) {
          var t = e[i];
          t.enumerable = t.enumerable || !1, t.configurable = !0, "value" in t && (t.writable = !0), Object.defineProperty(n, a(t.key), t)
        }
      }

      function o(n, e, i) {
        return e && r(n.prototype, e), i && r(n, i), Object.defineProperty(n, "prototype", {
          writable: !1
        }), n
      }

      function a(n) {
        var e = function(n, e) {
          if ("object" !== i(n) || null === n) return n;
          var t = n[Symbol.toPrimitive];
          if (void 0 !== t) {
            var r = t.call(n, e || "default");
            if ("object" !== i(r)) return r;
            throw new TypeError("@@toPrimitive must return a primitive value.")
          }
          return ("string" === e ? String : Number)(n)
        }(n, "string");
        return "symbol" === i(e) ? e : String(e)
      }
      var l = function() {
        function n(e, i) {
          t(this, n), this.hash = e, this.rename = i, this.size = 0
        }
        return o(n, [{
          key: "get",
          value: function(n) {
            return this.hash.get(this.rename(n))
          }
        }, {
          key: "set",
          value: function(n, e) {
            return this.delete(n), this.size += e, this.hash.set(this.rename(n), e)
          }
        }, {
          key: "has",
          value: function(n) {
            return this.hash.has(this.rename(n))
          }
        }, {
          key: "delete",
          value: function(n) {
            return this.size -= 0 | this.hash.get(this.rename(n)), this.hash.delete(this.rename(n))
          }
        }]), n
      }();
      ie.WXABErrorSteps = {
        kWebRequestResponse: 0,
        kLoadBundleFromFile: 1,
        kCacheGet: 2
      }, ie.disk = new l(unityNamespace.WXAssetBundles, unityNamespace.PathInFileOS), ie.msg = "", ie.fd2wxStream = new Map, ie.path2fd = new Map, ie.fs = wx.getFileSystemManager(), ie.nowfd = Xc.MAX_OPEN_FDS + 1, ie.isWXAssetBundle = function(n) {
        return ie._url2path.has(n) || n.startsWith(GameGlobal.unityNamespace.DATA_CDN) || n.startsWith("/vfs_streamingassets") ? unityNamespace.isWXAssetBundle(ie.url2path(n)) : unityNamespace.isWXAssetBundle(n)
      }, ie.newfd = function() {
        return ie.nowfd++
      }, ie.doWXAccess = function(n, e) {
        if (-8 & e) return -28;
        try {
          ie.fs.accessSync(n)
        } catch (n) {
          return -44
        }
        return 0
      };
      var u = function() {
        function n(e, i) {
          t(this, n), this.ttl = e, i > 0 && (this.capacity = i), this.hash = new Map, this.size = 0, this.maxSize = 0, this.obsolete = ""
        }
        return o(n, [{
          key: "record",
          value: function(n) {
            this.obsolete.includes(n) || ("" != this.obsolete && (this.obsolete += ";"), this.obsolete += n)
          }
        }, {
          key: "get",
          value: function(n) {
            var e = this.hash.get(n);
            return void 0 !== e ? (this.hash.delete(n), e.time = Date.now(), this.hash.set(n, e), e.ab) : -1
          }
        }, {
          key: "put",
          value: function(n, e, i) {
            if (e) {
              i = null == i || i;
              var t = {
                  ab: e,
                  time: Date.now(),
                  cleanable: i
                },
                r = this.hash.get(n);
              if (void 0 !== r) this.size -= r.ab.byteLength, this.hash.delete(n), this.hash.set(n, t);
              else if (void 0 !== this.capacity && this.size >= this.capacity) {
                var o = this.hash.keys().next().value;
                this.size -= o.ab.byteLength, this.hash.delete(o), this.hash.set(n, t)
              } else this.hash.set(n, t);
              this.size += t.ab.byteLength, this.maxSize = Math.max(this.size, this.maxSize)
            }
          }
        }, {
          key: "cleanable",
          value: function(n, e) {
            e = null == e || e;
            var i = this.hash.get(n);
            return void 0 !== i ? (i.cleanable = e, this.hash.set(n, i), 0) : -1
          }
        }, {
          key: "cleanbytime",
          value: function(n) {
            for (var e, i, t = this.hash.keys(); null != (e = t.next().value) && (i = this.hash.get(e)).time < n;) i.cleanable && (this.size -= i.ab.byteLength, this.hash.delete(e))
          }
        }, {
          key: "RegularCleaning",
          value: function(n) {
            var e = this;
            setInterval((function() {
              e.cleanbytime(Date.now() - 1e3 * e.ttl)
            }), 1e3 * n)
          }
        }, {
          key: "delete",
          value: function(n) {
            return this.size -= this.hash.get(n).ab.byteLength, this.hash.delete(n)
          }
        }, {
          key: "has",
          value: function(n) {
            return this.hash.has(n)
          }
        }]), n
      }();
      ie.cache = new u(n, e), unityNamespace.isIOS && unityNamespace.isH5Renderer && ie.cache.RegularCleaning(1), ie.wxstat = function(n) {
        try {
          var e, i = ie.path2fd.get(n);
          return void 0 !== i ? ((e = {
            mode: 33206,
            size: ie.cache.get(i).byteLength,
            dev: 1,
            ino: 1,
            nlink: 1,
            uid: 0,
            gid: 0,
            rdev: 0,
            atime: new Date,
            mtime: new Date(0),
            ctime: new Date,
            blksize: 4096
          }).blocks = Math.ceil(e.size / e.blksize), e) : ((e = ie.fs.statSync(n)).dev = 1, e.ino = 1, e.nlink = 1, e.uid = 0, e.gid = 0, e.rdev = 0, e.atime = new Date(1e3 * e.lastAccessedTime), e.mtime = new Date(0), e.ctime = new Date(1e3 * e.lastModifiedTime), delete e.lastAccessedTime, delete e.lastModifiedTime, e.blksize = 4096, e.blocks = Math.ceil(e.size / e.blksize), e)
        } catch (n) {
          throw x(n), n
        }
      }, ie._url2path = new Map, ie.url2path = function(n) {
        if (ie._url2path.has(n)) return ie._url2path.get(n);
        if ((n = n.replaceAll(" ", "%20")).startsWith("/vfs_streamingassets/")) var e = n.replace("/vfs_streamingassets/", wx.env.USER_DATA_PATH + "/__GAME_FILE_CACHE/StreamingAssets/");
        else e = n.replace(GameGlobal.unityNamespace.DATA_CDN, wx.env.USER_DATA_PATH + "/__GAME_FILE_CACHE/");
        return e.indexOf("?") > -1 && (e = e.substring(0, e.indexOf("?"))), ie._url2path.set(n, e), e
      }, ie.LoadBundleFromFile = function(n) {
        try {
          var e = ie.fs.readFileSync(n)
        } catch (n) {
          var i = n ? n.toString() : "unknown"
        }
        var t = ie.disk.get(n);
        if (0 === t && (ie.disk.set(n, e.byteLength), t = e.byteLength), !e || e.byteLength != t) {
          var r = {
            stage: ie.WXABErrorSteps.kLoadBundleFromFile,
            path: n,
            size: e ? e.byteLength : 0,
            expected_size: t,
            error: i
          };
          return GameGlobal.manager.reporter.wxAssetBundle.reportEmptyContent(r), GameGlobal.manager.Logger.pluginLog("[WXAssetBundle]readFileSync at path " + n + " return size " + (e ? e.byteLength : 0) + ", different from expected size " + t + " error: " + i), wx.setStorageSync("wxfs_unserviceable", !0), GameGlobal.onCrash(), ""
        }
        return e
      }, ie.read = function(n, e, i, t, r) {
        var o = ie.cache.get(n.fd);
        if (-1 === o) {
          var a = ie.LoadBundleFromFile(n.path);
          ie.cache.put(n.fd, a), o = a
        }
        if (r >= n.node.usedBytes) return 0;
        var l = Math.min(n.node.usedBytes - r, t);
        return K(l >= 0), e.set(new Uint8Array(o.slice(r, r + l)), i), l
      }
    }

    function Ar(n, e) {
      window.WXWASMSDK.WXGameClubButtonAddListener(Fo(n), Fo(e))
    }

    function kr(n) {
      window.WXWASMSDK.WXGameClubButtonDestroy(Fo(n))
    }

    function Mr(n) {
      window.WXWASMSDK.WXGameClubButtonHide(Fo(n))
    }

    function xr(n, e) {
      window.WXWASMSDK.WXGameClubButtonRemoveListener(Fo(n), Fo(e))
    }

    function Xr(n, e, i) {
      window.WXWASMSDK.WXGameClubButtonSetProperty(Fo(n), Fo(e), Fo(i))
    }

    function jr(n) {
      window.WXWASMSDK.WXGameClubButtonShow(Fo(n))
    }

    function Tr(n, e, i) {
      window.WXWASMSDK.WXGameClubStyleChangeInt(Fo(n), Fo(e), i)
    }

    function Lr(n, e, i) {
      window.WXWASMSDK.WXGameClubStyleChangeStr(Fo(n), Fo(e), Fo(i))
    }

    function Fr(n, e, i, t) {
      t = t || !0;
      var r = rn(n),
        o = rn(e),
        a = ln(o) + 1,
        l = Lg(a);
      an(o, l, a);
      var u = new GameGlobal.unityNamespace.UnityLoader.UnityCache.XMLHttpRequest;

      function f(e, r) {
        if (t) return setTimeout((function() {
          Fr(n, !1)
        }), 1e3);
        if (i) {
          var o = ln(e) + 1,
            a = Lg(o);
          an(e, a, o), qn("viii", i, [l, r, a]), Fg(a), Fg(l)
        }
      }
      u.open("GET", r, !0), u.responseType = "arraybuffer", u.onload = function(e) {
        if (u.status >= 400 && t) return setTimeout((function() {
          Fr(n, !1)
        }), 1e3), u = null, !1;
        if (i) {
          var o = new Uint8Array(u.response);
          if (0 != o.length) {
            var a = u.response,
              f = ie.url2path(r),
              c = ie.path2fd.get(f);
            null == c && (c = ie.newfd(), ie.path2fd.set(f, c));
            var s = ie.fd2wxStream.get(c);
            null == s && ((s = {
              fd: c,
              path: f,
              seekable: !0,
              position: 0,
              stream_ops: Mc.stream_ops,
              ungotten: [],
              node: {
                mode: 32768,
                usedBytes: o.length
              },
              error: !1
            }).stream_ops.read = ie.read, ie.fd2wxStream.set(c, s)), ie.cache.put(c, a, u.isReadFromCache), ie.disk.set(f, o.length), qn("viii", i, [l, 0, 0]), u.isReadFromCache && Fg(l)
          } else qn("viii", i, [l, 1111, 0]), Fg(l)
        }
      }, u.onsave = function(n) {
        ie.cache.cleanable(ie.path2fd.get(n)), Fg(l)
      }, u.onerror = function(n) {
        f("Unknown error.", 2)
      }, u.ontimeout = function(n) {
        f("Connection timed out.", 14)
      }, u.onabort = function(n) {
        f("Aborted.", 17)
      }, u.send()
    }

    function Pr() {
      return ie && ie.cache && ie.cache.hash && ie.cache.hash.size
    }

    function Rr() {
      return ie && ie.disk && ie.disk.hash && ie.disk.hash.size
    }

    function Br() {
      return ie && ie.cache && ie.cache.size
    }

    function Gr() {
      return ie && ie.disk && ie.disk.size
    }

    function Or(n) {
      var e = window.WXWASMSDK.WXGetCachePath(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Ir() {
      window.WXWASMSDK.WXGetDirectAdStatus()
    }

    function Kr() {
      if ("undefined" != typeof DYNAMIC_BASE) return Z[DYNAMICTOP_PTR >> 2] - DYNAMIC_BASE;
      var e = 7936880;
      return void 0 !== n.___heap_base && (e = n.___heap_base), Rg() - e
    }

    function Nr() {
      var n, e;
      return void 0 === GameGlobal.calcFrameTimeFunc && (GameGlobal.calcFrameTimeFunc = (n = 0, e = 0, function(i, t) {
        n++, e += t - i, n >= 60 ? (GameGlobal.avgExFrameTime = e / 60, n = 0, e = 0) : void 0 === GameGlobal.avgExFrameTime && (GameGlobal.avgExFrameTime = e / n)
      }), GameGlobal.avgExFrameTime = 0), GameGlobal.avgExFrameTime
    }

    function Ur(n, e) {
      window.WXWASMSDK.WXGetFontRawData(Fo(n), Fo(e))
    }

    function zr() {
      window.WXWASMSDK.WXGetGameExptInfo()
    }

    function qr(n) {
      window.WXWASMSDK.WXGetOpenDataContext(Fo(n))
    }

    function Hr() {
      var n = window.WXWASMSDK.WXGetPluginCachePath(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function Vr(n, e) {
      var i = Fo(n),
        t = Fo(e),
        r = window.WXWASMSDK.WXGetSign(i, t),
        o = ln(r) + 1,
        a = Lg(o);
      return an(r, a, o), a
    }

    function Yr() {
      return STATICTOP - STATIC_BASE
    }

    function Jr() {
      return "undefined" != typeof TOTAL_MEMORY ? TOTAL_MEMORY : B && B.buffer ? B.buffer.byteLength : (x("Fail to find wasmMemory.buffer, TotalMemorySize is not correct."), 0)
    }

    function Zr() {
      return pn
    }

    function Qr() {
      var n = Rg();
      return H.length - n
    }

    function $r() {
      if ("undefined" != typeof emscriptenMemoryProfiler) return emscriptenMemoryProfiler.totalMemoryAllocated
    }

    function no() {
      var n = window.WXWASMSDK.WXGetUserDataPath(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function eo(n, e, i) {
      window.WXWASMSDK.WXHideAd(Fo(n), Fo(e), Fo(i))
    }

    function io() {
      window.WXWASMSDK && window.WXWASMSDK.WXHideLoadingPage()
    }

    function to() {
      window.WXWASMSDK.WXHideOpenData()
    }

    function ro(n) {
      window.WXWASMSDK.WXInitializeSDK(Fo(n)), "undefined" != typeof emscriptenMemoryProfiler && (GameGlobal.memprofiler = emscriptenMemoryProfiler, GameGlobal.memprofiler.onDump = function() {
        var n = wx.getFileSystemManager(),
          e = GameGlobal.memprofiler.allocationsAtLoc;
        void 0 === e && (e = GameGlobal.memprofiler.allocationSiteStatistics);
        var i = [];
        for (var t in e) i.push(t);
        i.sort((function(n, i) {
          return e[i][1] - e[n][1]
        })), console.log("WXDumpUnityHeap begin", Object.keys(e).length, i.length), wx.getFileSystemManager().open({
          filePath: wx.env.USER_DATA_PATH + "/alloc_used.csv",
          flag: "w",
          success: function(t) {
            var r = t.fd;
            n.write({
              fd: r,
              data: "callback;count;size;malloc;free\r\n",
              fail: function(n) {
                x(n)
              }
            });
            for (var o = 0, a = 0; a < 1e5 && a < i.length; ++a) {
              var l = i[a],
                u = e[l];
              if (void 0 !== u) {
                var f = l.indexOf("emscripten_trace_record_") + "emscripten_trace_record_".length; - 1 != f && (l = l.substr(f));
                var c = l.lastIndexOf("InitWebGLPlayeriPPc "); - 1 != c && (l = l.substr(0, c)), -1 != (c = l.lastIndexOf("InitPlayerLoopCallbacks")) && (l = l.substr(0, c)), l = (l = (l = (l = (l = l.replace(/\(.*?\)/g, "")).replace(/[A-Z0-9]{40}/g, "")).replace(/\n/g, "<-")).replace(/_malloc <-.*?MemLabelId15AllocateOptions/g, "")).replace(/<-    at dynCall.*?at invoke_/g, ""), n.write({
                  fd: r,
                  data: l + ";" + u[0] + ";" + u[1] + ";" + u[2] + ";" + u[3] + "\r\n",
                  fail: function(n) {
                    x(n)
                  }
                })
              } else ++o
            }
            console.log("WXDumpUnityHeap end", o)
          }
        })
      })
    }

    function oo(n, e) {
      window.WXWASMSDK.WXInnerAudioContextAddListener(Fo(n), Fo(e))
    }

    function ao(n) {
      window.WXWASMSDK.WXInnerAudioContextDestroy(Fo(n))
    }

    function lo(n, e) {
      return window.WXWASMSDK.WXInnerAudioContextGetBool(Fo(n), Fo(e))
    }

    function uo(n, e) {
      return window.WXWASMSDK.WXInnerAudioContextGetFloat(Fo(n), Fo(e))
    }

    function fo(n) {
      window.WXWASMSDK.WXInnerAudioContextPause(Fo(n))
    }

    function co(n) {
      window.WXWASMSDK.WXInnerAudioContextPlay(Fo(n))
    }

    function so(n, e) {
      window.WXWASMSDK.WXInnerAudioContextRemoveListener(Fo(n), Fo(e))
    }

    function mo(n, e) {
      window.WXWASMSDK.WXInnerAudioContextSeek(Fo(n), e)
    }

    function po(n, e, i) {
      window.WXWASMSDK.WXInnerAudioContextSetBool(Fo(n), Fo(e), i)
    }

    function yo(n, e, i) {
      window.WXWASMSDK.WXInnerAudioContextSetFloat(Fo(n), Fo(e), i)
    }

    function vo(n, e, i) {
      window.WXWASMSDK.WXInnerAudioContextSetString(Fo(n), Fo(e), Fo(i))
    }

    function _o(n) {
      window.WXWASMSDK.WXInnerAudioContextStop(Fo(n))
    }

    function go() {
      return window.WXWASMSDK.WXIsCloudTest()
    }

    function ho(n) {
      var e = window.WXWASMSDK.WXLaunchOperaBridge(Fo(n));
      if (e) {
        var i = ln(e) + 1,
          t = Lg(i);
        return an(e, t, i), t
      }
    }

    function wo(n) {
      window.WXWASMSDK.WXLogManagerDebug(Fo(n))
    }

    function So(n) {
      window.WXWASMSDK.WXLogManagerInfo(Fo(n))
    }

    function Co(n) {
      window.WXWASMSDK.WXLogManagerLog(Fo(n))
    }

    function Eo(n) {
      window.WXWASMSDK.WXLogManagerWarn(Fo(n))
    }

    function bo(n, e, i, t, r, o) {
      var a = Fo(n),
        l = Fo(e),
        u = Fo(r);
      window.WXWASMSDK.WXMiniGameCommonGetUserLabel(a, l, i, t, u, o)
    }

    function Wo(n, e, i, t, r, o) {
      var a = Fo(n),
        l = Fo(e),
        u = Fo(r);
      window.WXWASMSDK.WXMiniGameCommonGetUserLabelV2(a, l, i, t, u, o)
    }

    function Do(n, e, i, t, r) {
      window.WXWASMSDK.WXMkdir(Fo(n), e, Fo(i), Fo(t), Fo(r))
    }

    function Ao(n, e) {
      var i = window.WXWASMSDK.WXMkdirSync(Fo(n), e),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function ko() {
      window.WXWASMSDK.WXOnChallengeStart()
    }

    function Mo() {
      window.WXWASMSDK.WXOnDirectAdStatusChange()
    }

    function xo() {
      var n = window.WXWASMSDK.WXOnLaunchProgress(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function Xo(n, e) {
      return window.WXWASMSDK.WXOnShareAppMessage(Fo(n), e)
    }

    function jo(n) {
      return window.WXWASMSDK.WXOnShareAppMessageResolve(Fo(n))
    }

    function To(n, e, i, t) {
      window.WXWASMSDK.WXOpenDataToTempFilePath(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function Lo(n) {
      var e = window.WXWASMSDK.WXOpenDataToTempFilePathSync(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Fo(n) {
      return void 0 !== rn ? rn(n) : e(n)
    }

    function Po(n, e) {
      window.WXWASMSDK.WXPreDownloadAudios(Fo(n), e)
    }

    function Ro(n) {
      window.WXWASMSDK.WXPreloadConcurrent(n)
    }

    function Bo() {
      if ("undefined" != typeof emscriptenMemoryProfiler) return GameGlobal.memprofiler.onDump(), void wx.showModal({
        title: "ProfilingMemory",
        content: "OnDump Complete!"
      });
      x("Please call WX.InitSDK & Select ProfilingMemory Option")
    }

    function Go() {
      window.WXWASMSDK.WXQuitChallenge()
    }

    function Oo(n, e) {
      window.WXWASMSDK.WXReadFile(Fo(n), Fo(e))
    }

    function Io(n) {
      var e = window.WXWASMSDK.WXReadFileSync(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Ko(n) {
      var e = window.WXWASMSDK.WXRemoveFile(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function No(n, e, i, t) {
      window.WXWASMSDK.WXReportGameSceneError(n, e, Fo(i), Fo(t))
    }

    function Uo() {
      window.WXWASMSDK.WXReportGameStart()
    }

    function zo(n, e, i) {
      window.WXWASMSDK.WXReportUserBehaviorBranchAnalytics(Fo(n), Fo(e), i)
    }

    function qo(n, e) {
      var i = window.WXWASMSDK.WXReportShareBehavior(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function Ho(n, e, i, t, r) {
      window.WXWASMSDK.WXRmdir(Fo(n), e, Fo(i), Fo(t), Fo(r))
    }

    function Vo(n, e) {
      var i = window.WXWASMSDK.WXRmdirSync(Fo(n), e),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function Yo(n, e) {
      window.WXWASMSDK.WXSetArrayBuffer(V, n, Fo(e))
    }

    function Jo(n) {
      window.WXWASMSDK.WXSetDataCDN(Fo(n))
    }

    function Zo(n) {
      window.WXWASMSDK.WXSetPreloadList(Fo(n))
    }

    function Qo(n) {
      window.WXWASMSDK.WXSetSyncReadCacheEnabled(n)
    }

    function $o(n, e) {
      window.WXWASMSDK.WXShareFontBuffer(V, n, Fo(e))
    }

    function na(n, e, i) {
      window.WXWASMSDK.WXShowAd(Fo(n), Fo(e), Fo(i))
    }

    function ea(n, e, i, t, r) {
      window.WXWASMSDK.WXShowAd2(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r))
    }

    function ia(n, e, i, t, r) {
      window.WXWASMSDK.WXShowOpenData(n, e, i, t, r)
    }

    function ta(n, e) {
      window.WXWASMSDK.WXStat(Fo(n), Fo(e))
    }

    function ra() {
      window.WXWASMSDK.WXStorageDeleteAllSync()
    }

    function oa(n) {
      window.WXWASMSDK.WXStorageDeleteKeySync(Fo(n))
    }

    function aa(n, e) {
      return window.WXWASMSDK.WXStorageGetFloatSync(Fo(n), e)
    }

    function la(n, e) {
      return window.WXWASMSDK.WXStorageGetIntSync(Fo(n), e)
    }

    function ua(n, e) {
      var i = window.WXWASMSDK.WXStorageGetStringSync(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function fa(n) {
      return window.WXWASMSDK.WXStorageHasKeySync(Fo(n))
    }

    function ca(n, e) {
      window.WXWASMSDK.WXStorageSetFloatSync(Fo(n), e)
    }

    function sa(n, e) {
      window.WXWASMSDK.WXStorageSetIntSync(Fo(n), e)
    }

    function da(n, e) {
      window.WXWASMSDK.WXStorageSetStringSync(Fo(n), Fo(e))
    }

    function ma(n, e, i, t) {
      window.WXWASMSDK.WXToTempFilePath(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function pa(n) {
      var e = window.WXWASMSDK.WXToTempFilePathSync(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function ya() {
      window.WXWASMSDK.WXUncaughtException(!1)
    }

    function va(n, e, i, t) {
      return window.WXWASMSDK.WXUnlink(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function _a(n) {
      var e = window.WXWASMSDK.WXUnlinkSync(Fo(n)),
        i = ln(e || "") + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function ga(n, e) {
      var i = Fo(n);
      window.WXWASMSDK.WXUpdateScore(i, e)
    }

    function ha(n) {
      window.WXWASMSDK.WXUploadTaskAbort(Fo(n))
    }

    function wa(n) {
      window.WXWASMSDK.WXUploadTaskOffHeadersReceived(Fo(n))
    }

    function Sa(n) {
      window.WXWASMSDK.WXUploadTaskOffProgressUpdate(Fo(n))
    }

    function Ca(n) {
      window.WXWASMSDK.WXUploadTaskOnHeadersReceived(Fo(n))
    }

    function Ea(n) {
      window.WXWASMSDK.WXUploadTaskOnProgressUpdate(Fo(n))
    }

    function ba(n) {
      window.WXWASMSDK.WXUserInfoButtonDestroy(Fo(n))
    }

    function Wa(n) {
      window.WXWASMSDK.WXUserInfoButtonHide(Fo(n))
    }

    function Da(n) {
      window.WXWASMSDK.WXUserInfoButtonOffTap(Fo(n))
    }

    function Aa(n) {
      window.WXWASMSDK.WXUserInfoButtonOnTap(Fo(n))
    }

    function ka(n) {
      window.WXWASMSDK.WXUserInfoButtonShow(Fo(n))
    }

    function Ma(n, e) {
      window.WXWASMSDK.WXVideoAddListener(Fo(n), Fo(e))
    }

    function xa(n, e) {
      window.WXWASMSDK.WXVideoDestroy(Fo(n), e)
    }

    function Xa(n) {
      window.WXWASMSDK.WXVideoExitFullScreen(Fo(n))
    }

    function ja(n) {
      window.WXWASMSDK.WXVideoPause(Fo(n))
    }

    function Ta(n) {
      window.WXWASMSDK.WXVideoPlay(Fo(n))
    }

    function La(n, e) {
      window.WXWASMSDK.WXVideoRemoveListener(Fo(n), Fo(e))
    }

    function Fa(n, e) {
      window.WXWASMSDK.WXVideoRequestFullScreen(Fo(n), e)
    }

    function Pa(n, e) {
      window.WXWASMSDK.WXVideoSeek(Fo(n), e)
    }

    function Ra(n, e, i) {
      window.WXWASMSDK.WXVideoSetProperty(Fo(n), Fo(e), Fo(i))
    }

    function Ba(n) {
      window.WXWASMSDK.WXVideoStop(Fo(n))
    }

    function Ga(n, e, i, t) {
      var r = window.WXWASMSDK.WXWriteBinFileSync(Fo(n), V.slice(e, i + e), Fo(t)),
        o = ln(r || "") + 1,
        a = Lg(o);
      return an(r, a, o), a
    }

    function Oa(n, e, i, t, r, o, a) {
      window.WXWASMSDK.WXWriteFile(Fo(n), V.slice(e, i + e), Fo(t), Fo(r), Fo(o), Fo(a))
    }

    function Ia(n, e, i) {
      var t = window.WXWASMSDK.WXWriteFileSync(Fo(n), Fo(e), Fo(i)),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t, o, r), o
    }

    function Ka(n) {
      window.WXWASMSDK.WXWriteLog(Fo(n))
    }

    function Na(n, e, i, t, r, o) {
      window.WXWASMSDK.WXWriteStringFile(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r), Fo(o))
    }

    function Ua(n) {
      window.WXWASMSDK.WXWriteWarn(Fo(n))
    }

    function za(n, e, i) {
      var t = Fo(n),
        r = Fo(e),
        o = JSON.parse(Fo(i));
      GameGlobal[t][r].apply(GameGlobal[t], o)
    }

    function qa(n, e, i) {
      var t = Fo(n),
        r = Fo(e),
        o = JSON.parse(Fo(i)),
        a = GameGlobal[t][r].apply(GameGlobal[t], o),
        l = JSON.stringify(a),
        u = ln(l || "") + 1,
        f = Lg(u);
      return an(l || "", f, u), f
    }

    function Ha(n, e, i, t, r, o) {
      var a = window.WXWASMSDK.WX_ClassConstructor(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r), Fo(o)),
        l = ln(a || "") + 1,
        u = Lg(l);
      return an(a || "", u, l), u
    }

    function Va(n, e, i) {
      var t = window.WXWASMSDK.WX_ClassFunction(Fo(n), Fo(e), Fo(i)),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t || "", o, r), o
    }

    function Ya(n, e, i, t) {
      window.WXWASMSDK.WX_ClassOffEventFunction(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function Ja(n, e, i, t, r) {
      window.WXWASMSDK.WX_ClassOnEventFunction(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r))
    }

    function Za(n, e, i, t, r, o, a, l, u) {
      window.WXWASMSDK.WX_ClassOneWayFunction(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r), Fo(o), Fo(a), Fo(l), u)
    }

    function Qa(n, e, i, t) {
      var r = window.WXWASMSDK.WX_ClassOneWayNoFunction_t(Fo(n), Fo(e), Fo(i), Fo(t)),
        o = ln(r || "") + 1,
        a = Lg(o);
      return an(r || "", a, o), a
    }

    function $a(n, e, i) {
      window.WXWASMSDK.WX_ClassOneWayNoFunction_v(Fo(n), Fo(e), Fo(i))
    }

    function nl(n, e, i, t) {
      window.WXWASMSDK.WX_ClassOneWayNoFunction_vs(Fo(n), Fo(e), Fo(i), t)
    }

    function el(n, e, i, t) {
      window.WXWASMSDK.WX_ClassOneWayNoFunction_vs(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function il(n, e, i, t) {
      window.WXWASMSDK.WX_ClassOneWayNoFunction_vt(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function tl(n, e, i, t) {
      window.WXWASMSDK.WX_ClassSetProperty(Fo(n), Fo(e), Fo(i), Fo(t))
    }

    function rl(n, e, i) {
      var t = window.WXWASMSDK.WX_CloudCDN(Fo(n), V.buffer.slice(e, e + i)),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t, o, r), o
    }

    function ol(n, e, i) {
      e = i ? JSON.parse(Fo(e)) : Fo(e);
      var t = window.WXWASMSDK.WX_CloudCDN(Fo(n), e),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t, o, r), o
    }

    function al(n, e, i) {
      window.WXWASMSDK.WX_CloudCallContainer(Fo(n), Fo(e), Fo(i))
    }

    function ll(n, e, i) {
      window.WXWASMSDK.WX_CloudCallFunction(Fo(n), Fo(e), Fo(i))
    }

    function ul(n) {
      window.WXWASMSDK.WX_CloudCloud(Fo(n))
    }

    function fl(n, e) {
      var i = window.WXWASMSDK.WX_CloudCloudID(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function cl(n, e, i) {
      window.WXWASMSDK.WX_CloudDeleteFile(Fo(n), Fo(e), Fo(i))
    }

    function sl(n, e, i) {
      window.WXWASMSDK.WX_CloudDownloadFile(Fo(n), Fo(e), Fo(i))
    }

    function dl(n, e, i) {
      window.WXWASMSDK.WX_CloudGetTempFileURL(Fo(n), Fo(e), Fo(i))
    }

    function ml(n) {
      window.WXWASMSDK.WX_CloudInit(Fo(n))
    }

    function pl(n, e, i) {
      window.WXWASMSDK.WX_CloudUploadFile(Fo(n), Fo(e), Fo(i))
    }

    function yl() {
      var n = window.WXWASMSDK.WX_CreateTCPSocket(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function vl() {
      var n = window.WXWASMSDK.WX_CreateUDPSocket(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function _l(n, e, i) {
      window.WXWASMSDK.WX_FileSystemManagerAppendFileStringSync(Fo(n), Fo(e), Fo(i))
    }

    function gl(n, e, i, t) {
      window.WXWASMSDK.WX_FileSystemManagerAppendFileSync(Fo(n), V.slice(e, i + e), Fo(t))
    }

    function hl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerClose(Fo(n), Fo(e))
    }

    function wl(n) {
      window.WXWASMSDK.WX_FileSystemManagerCloseSync(Fo(n))
    }

    function Sl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerFstat(Fo(n), Fo(e))
    }

    function Cl(n) {
      var e = window.WXWASMSDK.WX_FileSystemManagerFstatSync(Fo(n)),
        i = ln(e) + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function El(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerFtruncate(Fo(n), Fo(e))
    }

    function bl(n) {
      window.WXWASMSDK.WX_FileSystemManagerFtruncateSync(Fo(n))
    }

    function Wl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerGetFileInfo(Fo(n), Fo(e))
    }

    function Dl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerGetSavedFileList(Fo(n), Fo(e))
    }

    function Al(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerOpen(Fo(n), Fo(e))
    }

    function kl(n) {
      var e = window.WXWASMSDK.WX_FileSystemManagerOpenSync(Fo(n)),
        i = ln(e) + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Ml(n, e, i, t) {
      window.WXWASMSDK.WX_FileSystemManagerRead(Fo(n), V.slice(e, i + e), Fo(t))
    }

    function xl(n, e) {
      var i = window.WXWASMSDK.WX_FileSystemManagerReadCompressedFileSync(Fo(n), Fo(e)),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function Xl(n, e) {
      var i = window.WXWASMSDK.WX_FileSystemManagerReadSync(Fo(n), Fo(e)),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function jl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerReadZipEntry(Fo(n), Fo(e))
    }

    function Tl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerReadZipEntryString(Fo(n), Fo(e))
    }

    function Ll(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerReaddir(Fo(n), Fo(e))
    }

    function Fl(n) {
      var e = window.WXWASMSDK.WX_FileSystemManagerReaddirSync(Fo(n)),
        i = ln(e) + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Pl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerRemoveSavedFile(Fo(n), Fo(e))
    }

    function Rl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerRename(Fo(n), Fo(e))
    }

    function Bl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerRenameSync(Fo(n), Fo(e))
    }

    function Gl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerSaveFile(Fo(n), Fo(e))
    }

    function Ol(n, e) {
      var i = window.WXWASMSDK.WX_FileSystemManagerSaveFileSync(Fo(n), Fo(e)),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function Il(n, e) {
      var i = window.WXWASMSDK.WX_FileSystemManagerStatSync(Fo(n), e),
        t = ln(i) + 1,
        r = Lg(t);
      return an(i, r, t), r
    }

    function Kl(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerTruncate(Fo(n), Fo(e))
    }

    function Nl(n) {
      window.WXWASMSDK.WX_FileSystemManagerTruncateSync(Fo(n))
    }

    function Ul(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerUnzip(Fo(n), Fo(e))
    }

    function zl(n, e, i, t) {
      window.WXWASMSDK.WX_FileSystemManagerWrite(Fo(n), V.slice(e, i + e), Fo(t))
    }

    function ql(n, e) {
      window.WXWASMSDK.WX_FileSystemManagerWriteString(Fo(n), Fo(e))
    }

    function Hl(n) {
      var e = window.WXWASMSDK.WX_FileSystemManagerWriteStringSync(Fo(n)),
        i = ln(e) + 1,
        t = Lg(i);
      return an(e, t, i), t
    }

    function Vl(n, e, i) {
      var t = window.WXWASMSDK.WX_FileSystemManagerWriteSync(Fo(n), V.slice(e, i + e)),
        r = ln(t) + 1,
        o = Lg(r);
      return an(t, o, r), o
    }

    function Yl(n) {
      window.WXWASMSDK.WX_GameRecorderAbort(Fo(n))
    }

    function Jl(n, e) {
      window.WXWASMSDK.WX_GameRecorderOff(Fo(n), Fo(e))
    }

    function Zl(n, e) {
      window.WXWASMSDK.WX_GameRecorderOn(Fo(n), Fo(e))
    }

    function Ql(n) {
      window.WXWASMSDK.WX_GameRecorderPause(Fo(n))
    }

    function $l(n) {
      window.WXWASMSDK.WX_GameRecorderResume(Fo(n))
    }

    function nu(n, e) {
      window.WXWASMSDK.WX_GameRecorderStart(Fo(n), Fo(e))
    }

    function eu(n) {
      window.WXWASMSDK.WX_GameRecorderStop(Fo(n))
    }

    function iu() {
      var n = window.WXWASMSDK.WX_GetGameRecorder(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function tu() {
      var n = window.WXWASMSDK.WX_GetRecorderManager(),
        e = ln(n || "") + 1,
        i = Lg(e);
      return an(n, i, e), i
    }

    function ru() {
      window.WXWASMSDK.WX_OffAddToFavorites()
    }

    function ou() {
      window.WXWASMSDK.WX_OffBLECharacteristicValueChange()
    }

    function au() {
      window.WXWASMSDK.WX_OffCopyUrl()
    }

    function lu(n) {
      window.WXWASMSDK.WX_OffEventRegister(Fo(n))
    }

    function uu() {
      window.WXWASMSDK.WX_OffGameLiveStateChange()
    }

    function fu() {
      window.WXWASMSDK.WX_OffGyroscopeChange()
    }

    function cu() {
      window.WXWASMSDK.WX_OffHandoff()
    }

    function su() {
      window.WXWASMSDK.WX_OffShareTimeline()
    }

    function du() {
      window.WXWASMSDK.WX_OffTouchCancel()
    }

    function mu() {
      window.WXWASMSDK.WX_OffTouchEnd()
    }

    function pu() {
      window.WXWASMSDK.WX_OffTouchMove()
    }

    function yu() {
      window.WXWASMSDK.WX_OffTouchStart()
    }

    function vu() {
      window.WXWASMSDK.WX_OnAddToFavorites()
    }

    function _u(n) {
      window.WXWASMSDK.WX_OnAddToFavorites_Resolve(Fo(n))
    }

    function gu() {
      window.WXWASMSDK.WX_OnBLECharacteristicValueChange()
    }

    function hu() {
      window.WXWASMSDK.WX_OnCopyUrl()
    }

    function wu(n) {
      window.WXWASMSDK.WX_OnCopyUrl_Resolve(Fo(n))
    }

    function Su(n, e) {
      window.WXWASMSDK.WX_OnEventRegister(Fo(n), Fo(e))
    }

    function Cu() {
      window.WXWASMSDK.WX_OnGameLiveStateChange()
    }

    function Eu(n) {
      window.WXWASMSDK.WX_OnGameLiveStateChange_Resolve(Fo(n))
    }

    function bu() {
      window.WXWASMSDK.WX_OnGyroscopeChange()
    }

    function Wu() {
      window.WXWASMSDK.WX_OnHandoff()
    }

    function Du(n) {
      window.WXWASMSDK.WX_OnHandoff_Resolve(Fo(n))
    }

    function Au() {
      window.WXWASMSDK.WX_OnNeedPrivacyAuthorization()
    }

    function ku(n) {
      window.WXWASMSDK.WX_OnRecorderError(Fo(n))
    }

    function Mu(n) {
      window.WXWASMSDK.WX_OnRecorderFrameRecorded(Fo(n))
    }

    function xu(n) {
      window.WXWASMSDK.WX_OnRecorderInterruptionBegin(Fo(n))
    }

    function Xu(n) {
      window.WXWASMSDK.WX_OnRecorderInterruptionEnd(Fo(n))
    }

    function ju(n) {
      window.WXWASMSDK.WX_OnRecorderPause(Fo(n))
    }

    function Tu(n) {
      window.WXWASMSDK.WX_OnRecorderResume(Fo(n))
    }

    function Lu(n) {
      window.WXWASMSDK.WX_OnRecorderStart(Fo(n))
    }

    function Fu(n) {
      window.WXWASMSDK.WX_OnRecorderStop(Fo(n))
    }

    function Pu() {
      window.WXWASMSDK.WX_OnShareTimeline()
    }

    function Ru(n) {
      window.WXWASMSDK.WX_OnShareTimeline_Resolve(Fo(n))
    }

    function Bu() {
      window.WXWASMSDK.WX_OnTouchCancel()
    }

    function Gu() {
      window.WXWASMSDK.WX_OnTouchEnd()
    }

    function Ou() {
      window.WXWASMSDK.WX_OnTouchMove()
    }

    function Iu() {
      window.WXWASMSDK.WX_OnTouchStart()
    }

    function Ku(n, e, i, t, r, o) {
      window.WXWASMSDK.WX_OneWayFunction(Fo(n), Fo(e), Fo(i), Fo(t), Fo(r), Fo(o))
    }

    function Nu(n) {
      window.WXWASMSDK.WX_OneWayNoFunction_v(Fo(n))
    }

    function Uu(n, e, i, t) {
      window.WXWASMSDK.WX_OneWayNoFunction_vnns(Fo(n), e, i, Fo(t))
    }

    function zu(n, e) {
      window.WXWASMSDK.WX_OneWayNoFunction_vs(Fo(n), Fo(e))
    }

    function qu(n, e, i) {
      window.WXWASMSDK.WX_OneWayNoFunction_vsn(Fo(n), Fo(e), i)
    }

    function Hu(n, e, i) {
      window.WXWASMSDK.WX_OneWayNoFunction_vst(Fo(n), Fo(e), Fo(i))
    }

    function Vu(n, e) {
      window.WXWASMSDK.WX_OneWayNoFunction_vt(Fo(n), Fo(e))
    }

    function Yu(n) {
      window.WXWASMSDK.WX_PrivacyAuthorizeResolve(Fo(n))
    }

    function Ju(n) {
      window.WXWASMSDK.WX_RecorderPause(Fo(n))
    }

    function Zu(n) {
      window.WXWASMSDK.WX_RecorderResume(Fo(n))
    }

    function Qu(n, e) {
      window.WXWASMSDK.WX_RecorderStart(Fo(n), Fo(e))
    }

    function $u(n) {
      window.WXWASMSDK.WX_RecorderStop(Fo(n))
    }

    function nf(n) {
      window.WXWASMSDK.WX_RegisterOnBLECharacteristicValueChangeCallback(n)
    }

    function ef(n) {
      window.WXWASMSDK.WX_RegisterOnGyroscopeChangeCallback(n)
    }

    function tf(n) {
      window.WXWASMSDK.WX_RegisterOnTouchCancelCallback(n)
    }

    function rf(n) {
      window.WXWASMSDK.WX_RegisterOnTouchEndCallback(n)
    }

    function of(n) {
      window.WXWASMSDK.WX_RegisterOnTouchMoveCallback(n)
    }

    function af(n) {
      window.WXWASMSDK.WX_RegisterOnTouchStartCallback(n)
    }

    function lf(n) {
      window.WXWASMSDK.WX_RegisterStartGyroscopeCallback(n)
    }

    function uf(n) {
      window.WXWASMSDK.WX_RegisterStopGyroscopeCallback(n)
    }

    function ff(n) {
      window.WXWASMSDK.WX_RegisterTCPSocketOnMessageCallback(n)
    }

    function cf(n) {
      window.WXWASMSDK.WX_RegisterUDPSocketOnMessageCallback(n)
    }

    function sf(n) {
      window.devicePixelRatio = n
    }

    function df(n) {
      window.WXWASMSDK.WX_SetPreferredFramesPerSecond(n)
    }

    function mf(n, e) {
      window.WXWASMSDK.WX_StartGyroscope(Fo(n), Fo(e))
    }

    function pf(n) {
      window.WXWASMSDK.WX_StopGyroscope(Fo(n))
    }

    function yf(n) {
      return window.WXWASMSDK.WX_SyncFunction_b(Fo(n))
    }

    function vf(n, e) {
      return window.WXWASMSDK.WX_SyncFunction_bs(Fo(n), Fo(e))
    }

    function _f(n, e, i, t) {
      return window.WXWASMSDK.WX_SyncFunction_bsnn(Fo(n), Fo(e), i, t)
    }

    function gf(n, e) {
      return window.WXWASMSDK.WX_SyncFunction_bt(Fo(n), Fo(e))
    }

    function hf(n, e) {
      return window.WXWASMSDK.WX_SyncFunction_nt(Fo(n), Fo(e))
    }

    function wf(n, e) {
      var i = window.WXWASMSDK.WX_SyncFunction_ss(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i || "", r, t), r
    }

    function Sf(n, e) {
      var i = window.WXWASMSDK.WX_SyncFunction_t(Fo(n), Fo(e)),
        t = ln(i || "") + 1,
        r = Lg(t);
      return an(i || "", r, t), r
    }

    function Cf(n, e, i, t) {
      var r = window.WXWASMSDK.WX_SyncFunction_tnn(Fo(n), Fo(e), i, t),
        o = ln(r || "") + 1,
        a = Lg(o);
      return an(r || "", a, o), a
    }

    function Ef(n, e, i) {
      var t = window.WXWASMSDK.WX_SyncFunction_tt(Fo(n), Fo(e), Fo(i)),
        r = ln(t || "") + 1,
        o = Lg(r);
      return an(t || "", o, r), o
    }

    function bf(n, e) {
      window.WXWASMSDK.WX_TCPSocketBindWifi(Fo(n), Fo(e))
    }

    function Wf(n) {
      window.WXWASMSDK.WX_TCPSocketClose(Fo(n))
    }

    function Df(n, e) {
      window.WXWASMSDK.WX_TCPSocketConnect(Fo(n), Fo(e))
    }

    function Af(n) {
      window.WXWASMSDK.WX_TCPSocketOffBindWifi(Fo(n))
    }

    function kf(n) {
      window.WXWASMSDK.WX_TCPSocketOffClose(Fo(n))
    }

    function Mf(n) {
      window.WXWASMSDK.WX_TCPSocketOffConnect(Fo(n))
    }

    function xf(n) {
      window.WXWASMSDK.WX_TCPSocketOffError(Fo(n))
    }

    function Xf(n) {
      window.WXWASMSDK.WX_TCPSocketOffMessage(Fo(n))
    }

    function jf(n) {
      window.WXWASMSDK.WX_TCPSocketOnBindWifi(Fo(n))
    }

    function Tf(n) {
      window.WXWASMSDK.WX_TCPSocketOnClose(Fo(n))
    }

    function Lf(n) {
      window.WXWASMSDK.WX_TCPSocketOnConnect(Fo(n))
    }

    function Ff(n) {
      window.WXWASMSDK.WX_TCPSocketOnError(Fo(n))
    }

    function Pf(n, e) {
      window.WXWASMSDK.WX_TCPSocketOnMessage(Fo(n), e)
    }

    function Rf(n, e, i) {
      window.WXWASMSDK.WX_TCPSocketWriteBuffer(Fo(n), e, i)
    }

    function Bf(n, e) {
      window.WXWASMSDK.WX_TCPSocketWriteString(Fo(n), Fo(e))
    }

    function Gf(n, e) {
      return window.WXWASMSDK.WX_UDPSocketBind(Fo(n), e)
    }

    function Of(n) {
      window.WXWASMSDK.WX_UDPSocketClose(Fo(n))
    }

    function If(n, e) {
      window.WXWASMSDK.WX_UDPSocketConnect(Fo(n), Fo(e))
    }

    function Kf(n) {
      window.WXWASMSDK.WX_UDPSocketOffClose(Fo(n))
    }

    function Nf(n) {
      window.WXWASMSDK.WX_UDPSocketOffError(Fo(n))
    }

    function Uf(n) {
      window.WXWASMSDK.WX_UDPSocketOffListening(Fo(n))
    }

    function zf(n) {
      window.WXWASMSDK.WX_UDPSocketOffMessage(Fo(n))
    }

    function qf(n) {
      window.WXWASMSDK.WX_UDPSocketOnClose(Fo(n))
    }

    function Hf(n) {
      window.WXWASMSDK.WX_UDPSocketOnError(Fo(n))
    }

    function Vf(n) {
      window.WXWASMSDK.WX_UDPSocketOnListening(Fo(n))
    }

    function Yf(n, e) {
      window.WXWASMSDK.WX_UDPSocketOnMessage(Fo(n), e)
    }

    function Jf(n, e, i, t) {
      window.WXWASMSDK.WX_UDPSocketSendBuffer(Fo(n), e, i, Fo(t))
    }

    function Zf(n, e, i) {
      window.WXWASMSDK.WX_UDPSocketSendString(Fo(n), Fo(e), Fo(i))
    }

    function Qf(n, e) {
      window.WXWASMSDK.WX_UDPSocketSetTTL(Fo(n), e)
    }

    function $f(n, e, i, t) {
      window.WXWASMSDK.WX_UDPSocketWriteBuffer(Fo(n), e, i, Fo(t))
    }

    function nc(n, e, i) {
      window.WXWASMSDK.WX_UDPSocketWriteString(Fo(n), Fo(e), Fo(i))
    }

    function ec(n, e) {
      window.WXWASMSDK.WX_UploadFile(Fo(n), Fo(e))
    }
    var ic = {
      DESTRUCTOR_OFFSET: 0,
      REFCOUNT_OFFSET: 4,
      TYPE_OFFSET: 8,
      CAUGHT_OFFSET: 12,
      RETHROWN_OFFSET: 13,
      SIZE: 16
    };

    function tc(n) {
      return Lg(n + ic.SIZE) + ic.SIZE
    }

    function rc(n) {
      this.excPtr = n, this.ptr = n - ic.SIZE, this.set_type = function(n) {
        Z[this.ptr + ic.TYPE_OFFSET >> 2] = n
      }, this.get_type = function() {
        return Z[this.ptr + ic.TYPE_OFFSET >> 2]
      }, this.set_destructor = function(n) {
        Z[this.ptr + ic.DESTRUCTOR_OFFSET >> 2] = n
      }, this.get_destructor = function() {
        return Z[this.ptr + ic.DESTRUCTOR_OFFSET >> 2]
      }, this.set_refcount = function(n) {
        Z[this.ptr + ic.REFCOUNT_OFFSET >> 2] = n
      }, this.set_caught = function(n) {
        n = n ? 1 : 0, H[this.ptr + ic.CAUGHT_OFFSET >> 0] = n
      }, this.get_caught = function() {
        return 0 != H[this.ptr + ic.CAUGHT_OFFSET >> 0]
      }, this.set_rethrown = function(n) {
        n = n ? 1 : 0, H[this.ptr + ic.RETHROWN_OFFSET >> 0] = n
      }, this.get_rethrown = function() {
        return 0 != H[this.ptr + ic.RETHROWN_OFFSET >> 0]
      }, this.init = function(n, e) {
        this.set_type(n), this.set_destructor(e), this.set_refcount(0), this.set_caught(!1), this.set_rethrown(!1)
      }, this.add_ref = function() {
        var n = Z[this.ptr + ic.REFCOUNT_OFFSET >> 2];
        Z[this.ptr + ic.REFCOUNT_OFFSET >> 2] = n + 1
      }, this.release_ref = function() {
        var n = Z[this.ptr + ic.REFCOUNT_OFFSET >> 2];
        return Z[this.ptr + ic.REFCOUNT_OFFSET >> 2] = n - 1, 1 === n
      }
    }

    function oc(n) {
      this.free = function() {
        Fg(this.ptr), this.ptr = 0
      }, this.set_base_ptr = function(n) {
        Z[this.ptr >> 2] = n
      }, this.get_base_ptr = function() {
        return Z[this.ptr >> 2]
      }, this.set_adjusted_ptr = function(n) {
        Z[this.ptr + 4 >> 2] = n
      }, this.get_adjusted_ptr = function() {
        return Z[this.ptr + 4 >> 2]
      }, this.get_exception_ptr = function() {
        if (Tg(this.get_exception_info().get_type())) return Z[this.get_base_ptr() >> 2];
        var n = this.get_adjusted_ptr();
        return 0 !== n ? n : this.get_base_ptr()
      }, this.get_exception_info = function() {
        return new rc(this.get_base_ptr())
      }, void 0 === n ? (this.ptr = Lg(8), this.set_adjusted_ptr(0)) : this.ptr = n
    }
    var ac = [];

    function lc(n) {
      n.add_ref()
    }

    function uc(n) {
      var e = new oc(n),
        i = e.get_exception_info();
      return i.get_caught() || (i.set_caught(!0)), i.set_rethrown(!1), ac.push(e), lc(i), e.get_exception_ptr()
    }
    var fc = 0;

    function cc(n) {
      return Fg(new rc(n).ptr)
    }

    function sc(n) {
      if (n.release_ref() && !n.get_rethrown()) {
        var e = n.get_destructor();
        e && (i = n.excPtr, Kg.apply(null, [e, i])), cc(n.excPtr)
      }
      var i
    }

    function dc() {
      Xg(0);
      var n = ac.pop();
      sc(n.get_exception_info()), n.free(), fc = 0
    }

    function mc(n) {
      var e = new oc(n),
        i = e.get_base_ptr();
      throw fc || (fc = i), e.free(), i
    }

    function pc() {
      var n = fc;
      if (!n) return P(0), 0;
      var e = new rc(n),
        i = e.get_type(),
        t = new oc;
      if (t.set_base_ptr(n), !i) return P(0), 0 | t.ptr;
      var r = Array.prototype.slice.call(arguments),
        o = kg(),
        a = xg(4);
      Z[a >> 2] = n;
      for (var l = 0; l < r.length; l++) {
        var u = r[l];
        if (0 === u || u === i) break;
        if (jg(u, i, a)) {
          var f = Z[a >> 2];
          return n !== f && t.set_adjusted_ptr(f), P(u), 0 | t.ptr
        }
      }
      return Mg(o), P(i), 0 | t.ptr
    }

    function yc() {
      var n = fc;
      if (!n) return P(0), 0;
      var e = new rc(n),
        i = e.get_type(),
        t = new oc;
      if (t.set_base_ptr(n), !i) return P(0), 0 | t.ptr;
      var r = Array.prototype.slice.call(arguments),
        o = kg(),
        a = xg(4);
      Z[a >> 2] = n;
      for (var l = 0; l < r.length; l++) {
        var u = r[l];
        if (0 === u || u === i) break;
        if (jg(u, i, a)) {
          var f = Z[a >> 2];
          return n !== f && t.set_adjusted_ptr(f), P(u), 0 | t.ptr
        }
      }
      return Mg(o), P(i), 0 | t.ptr
    }

    function vc() {
      var n = fc;
      if (!n) return P(0), 0;
      var e = new rc(n),
        i = e.get_type(),
        t = new oc;
      if (t.set_base_ptr(n), !i) return P(0), 0 | t.ptr;
      var r = Array.prototype.slice.call(arguments),
        o = kg(),
        a = xg(4);
      Z[a >> 2] = n;
      for (var l = 0; l < r.length; l++) {
        var u = r[l];
        if (0 === u || u === i) break;
        if (jg(u, i, a)) {
          var f = Z[a >> 2];
          return n !== f && t.set_adjusted_ptr(f), P(u), 0 | t.ptr
        }
      }
      return Mg(o), P(i), 0 | t.ptr
    }

    function _c() {
      var n = ac.pop();
      n || r("no exception to throw");
      var e = n.get_exception_info(),
        i = n.get_base_ptr();
      throw e.get_rethrown() ? n.free() : (ac.push(n), e.set_rethrown(!0), e.set_caught(!1)), fc = i, i
    }

    function gc(n, e, i) {
      throw new rc(n).init(e, i), fc = n, n
    }

    function hc(n, e) {
      var i = new Date(1e3 * Z[n >> 2]);
      Z[e >> 2] = i.getUTCSeconds(), Z[e + 4 >> 2] = i.getUTCMinutes(), Z[e + 8 >> 2] = i.getUTCHours(), Z[e + 12 >> 2] = i.getUTCDate(), Z[e + 16 >> 2] = i.getUTCMonth(), Z[e + 20 >> 2] = i.getUTCFullYear() - 1900, Z[e + 24 >> 2] = i.getUTCDay(), Z[e + 36 >> 2] = 0, Z[e + 32 >> 2] = 0;
      var t = Date.UTC(i.getUTCFullYear(), 0, 1, 0, 0, 0, 0),
        r = (i.getTime() - t) / 864e5 | 0;
      return Z[e + 28 >> 2] = r, hc.GMTString || (hc.GMTString = un("GMT")), Z[e + 40 >> 2] = hc.GMTString, e
    }

    function wc(n, e) {
      return hc(n, e)
    }

    function Sc() {
      if (!Sc.called) {
        Sc.called = !0;
        var n = (new Date).getFullYear(),
          e = new Date(n, 0, 1),
          i = new Date(n, 6, 1),
          t = e.getTimezoneOffset(),
          r = i.getTimezoneOffset(),
          o = Math.max(t, r);
        Z[Ag() >> 2] = 60 * o, Z[Dg() >> 2] = Number(t != r);
        var a = c(e),
          l = c(i),
          u = un(a),
          f = un(l);
        r < t ? (Z[Wg() >> 2] = u, Z[Wg() + 4 >> 2] = f) : (Z[Wg() >> 2] = f, Z[Wg() + 4 >> 2] = u)
      }

      function c(n) {
        var e = n.toTimeString().match(/\(([A-Za-z ]+)\)$/);
        return e ? e[1] : "GMT"
      }
    }

    function Cc(n, e) {
      Sc();
      var i = new Date(1e3 * Z[n >> 2]);
      Z[e >> 2] = i.getSeconds(), Z[e + 4 >> 2] = i.getMinutes(), Z[e + 8 >> 2] = i.getHours(), Z[e + 12 >> 2] = i.getDate(), Z[e + 16 >> 2] = i.getMonth(), Z[e + 20 >> 2] = i.getFullYear() - 1900, Z[e + 24 >> 2] = i.getDay();
      var t = new Date(i.getFullYear(), 0, 1),
        r = (i.getTime() - t.getTime()) / 864e5 | 0;
      Z[e + 28 >> 2] = r, Z[e + 36 >> 2] = -60 * i.getTimezoneOffset();
      var o = new Date(i.getFullYear(), 6, 1).getTimezoneOffset(),
        a = t.getTimezoneOffset(),
        l = 0 | (o != a && i.getTimezoneOffset() == Math.min(a, o));
      Z[e + 32 >> 2] = l;
      var u = Z[Wg() + (l ? 4 : 0) >> 2];
      return Z[e + 40 >> 2] = u, e
    }

    function Ec(n, e) {
      return Cc(n, e)
    }
    var bc = {
      splitPath: function(n) {
        return /^(\/?|)([\s\S]*?)((?:\.{1,2}|[^\/]+?|)(\.[^.\/]*|))(?:[\/]*)$/.exec(n).slice(1)
      },
      normalizeArray: function(n, e) {
        for (var i = 0, t = n.length - 1; t >= 0; t--) {
          var r = n[t];
          "." === r ? n.splice(t, 1) : ".." === r ? (n.splice(t, 1), i++) : i && (n.splice(t, 1), i--)
        }
        if (e)
          for (; i; i--) n.unshift("..");
        return n
      },
      normalize: function(n) {
        var e = "/" === n.charAt(0),
          i = "/" === n.substr(-1);
        return (n = bc.normalizeArray(n.split("/").filter((function(n) {
          return !!n
        })), !e).join("/")) || e || (n = "."), n && i && (n += "/"), (e ? "/" : "") + n
      },
      dirname: function(n) {
        var e = bc.splitPath(n),
          i = e[0],
          t = e[1];
        return i || t ? (t && (t = t.substr(0, t.length - 1)), i + t) : "."
      },
      basename: function(n) {
        if ("/" === n) return "/";
        var e = (n = (n = bc.normalize(n)).replace(/\/$/, "")).lastIndexOf("/");
        return -1 === e ? n : n.substr(e + 1)
      },
      extname: function(n) {
        return bc.splitPath(n)[3]
      },
      join: function() {
        var n = Array.prototype.slice.call(arguments, 0);
        return bc.normalize(n.join("/"))
      },
      join2: function(n, e) {
        return bc.normalize(n + "/" + e)
      }
    };

    function Wc() {
      if (n.IsWxGame) return function() {
        return 256 * Math.random() | 0
      };
      if ("object" == typeof crypto && "function" == typeof crypto.getRandomValues) {
        var e = new Uint8Array(1);
        return function() {
          return crypto.getRandomValues(e), e[0]
        }
      }
      if (S) try {
        var i = require("crypto");
        return function() {
          return i.randomBytes(1)[0]
        }
      } catch (n) {}
      return function() {
        if (n.IsWxGame) return 256 * Math.random() | 0;
        r("randomDevice")
      }
    }
    var Dc = {
        resolve: function() {
          for (var n = "", e = !1, i = arguments.length - 1; i >= -1 && !e; i--) {
            var t = i >= 0 ? arguments[i] : Xc.cwd();
            if ("string" != typeof t) throw new TypeError("Arguments to path.resolve must be strings");
            if (!t) return "";
            n = t + "/" + n, e = "/" === t.charAt(0)
          }
          return (e ? "/" : "") + (n = bc.normalizeArray(n.split("/").filter((function(n) {
            return !!n
          })), !e).join("/")) || "."
        },
        relative: function(n, e) {
          function i(n) {
            for (var e = 0; e < n.length && "" === n[e]; e++);
            for (var i = n.length - 1; i >= 0 && "" === n[i]; i--);
            return e > i ? [] : n.slice(e, i - e + 1)
          }
          n = Dc.resolve(n).substr(1), e = Dc.resolve(e).substr(1);
          for (var t = i(n.split("/")), r = i(e.split("/")), o = Math.min(t.length, r.length), a = o, l = 0; l < o; l++)
            if (t[l] !== r[l]) {
              a = l;
              break
            } var u = [];
          for (l = a; l < t.length; l++) u.push("..");
          return (u = u.concat(r.slice(a))).join("/")
        }
      },
      Ac = {
        ttys: [],
        init: function() {},
        shutdown: function() {},
        register: function(n, e) {
          Ac.ttys[n] = {
            input: [],
            output: [],
            ops: e
          }, Xc.registerDevice(n, Ac.stream_ops)
        },
        stream_ops: {
          open: function(n) {
            var e = Ac.ttys[n.node.rdev];
            if (!e) throw new Xc.ErrnoError(43);
            n.tty = e, n.seekable = !1
          },
          close: function(n) {
            n.tty.ops.flush(n.tty)
          },
          flush: function(n) {
            n.tty.ops.flush(n.tty)
          },
          read: function(n, e, i, t, r) {
            if (!n.tty || !n.tty.ops.get_char) throw new Xc.ErrnoError(60);
            for (var o = 0, a = 0; a < t; a++) {
              var l;
              try {
                l = n.tty.ops.get_char(n.tty)
              } catch (n) {
                throw new Xc.ErrnoError(29)
              }
              if (void 0 === l && 0 === o) throw new Xc.ErrnoError(6);
              if (null == l) break;
              o++, e[i + a] = l
            }
            return o && (n.node.timestamp = Date.now()), o
          },
          write: function(n, e, i, t, r) {
            if (!n.tty || !n.tty.ops.put_char) throw new Xc.ErrnoError(60);
            try {
              for (var o = 0; o < t; o++) n.tty.ops.put_char(n.tty, e[i + o])
            } catch (n) {
              throw new Xc.ErrnoError(29)
            }
            return t && (n.node.timestamp = Date.now()), o
          }
        },
        default_tty_ops: {
          get_char: function(n) {
            if (!n.input.length) {
              var e = null;
              if (S) {
                var i = Buffer.alloc ? Buffer.alloc(256) : new Buffer(256),
                  t = 0;
                try {
                  t = W.readSync(process.stdin.fd, i, 0, 256, null)
                } catch (n) {
                  if (!n.toString().includes("EOF")) throw n;
                  t = 0
                }
                e = t > 0 ? i.slice(0, t).toString("utf-8") : null
              } else "undefined" != typeof window && "function" == typeof window.prompt ? null !== (e = window.prompt("Input: ")) && (e += "\n") : "function" == typeof readline && null !== (e = readline()) && (e += "\n");
              if (!e) return null;
              n.input = pg(e, !0)
            }
            return n.input.shift()
          },
          put_char: function(n, e) {
            null === e || 10 === e ? (M(tn(n.output, 0)), n.output = []) : 0 != e && n.output.push(e)
          },
          flush: function(n) {
            n.output && n.output.length > 0 && (M(tn(n.output, 0)), n.output = [])
          }
        },
        default_tty1_ops: {
          put_char: function(n, e) {
            null === e || 10 === e ? (x(tn(n.output, 0)), n.output = []) : 0 != e && n.output.push(e)
          },
          flush: function(n) {
            n.output && n.output.length > 0 && (x(tn(n.output, 0)), n.output = [])
          }
        }
      };

    function kc(n) {
      for (var e = j(n, 65536), i = Lg(e); n < e;) H[i + n++] = 0;
      return i
    }
    var Mc = {
        ops_table: null,
        mount: function(n) {
          return Mc.createNode(null, "/", 16895, 0)
        },
        createNode: function(n, e, i, t) {
          if (Xc.isBlkdev(i) || Xc.isFIFO(i)) throw new Xc.ErrnoError(63);
          Mc.ops_table || (Mc.ops_table = {
            dir: {
              node: {
                getattr: Mc.node_ops.getattr,
                setattr: Mc.node_ops.setattr,
                lookup: Mc.node_ops.lookup,
                mknod: Mc.node_ops.mknod,
                rename: Mc.node_ops.rename,
                unlink: Mc.node_ops.unlink,
                rmdir: Mc.node_ops.rmdir,
                readdir: Mc.node_ops.readdir,
                symlink: Mc.node_ops.symlink
              },
              stream: {
                llseek: Mc.stream_ops.llseek
              }
            },
            file: {
              node: {
                getattr: Mc.node_ops.getattr,
                setattr: Mc.node_ops.setattr
              },
              stream: {
                llseek: Mc.stream_ops.llseek,
                read: Mc.stream_ops.read,
                write: Mc.stream_ops.write,
                allocate: Mc.stream_ops.allocate,
                mmap: Mc.stream_ops.mmap,
                msync: Mc.stream_ops.msync
              }
            },
            link: {
              node: {
                getattr: Mc.node_ops.getattr,
                setattr: Mc.node_ops.setattr,
                readlink: Mc.node_ops.readlink
              },
              stream: {}
            },
            chrdev: {
              node: {
                getattr: Mc.node_ops.getattr,
                setattr: Mc.node_ops.setattr
              },
              stream: Xc.chrdev_stream_ops
            }
          });
          var r = Xc.createNode(n, e, i, t);
          return Xc.isDir(r.mode) ? (r.node_ops = Mc.ops_table.dir.node, r.stream_ops = Mc.ops_table.dir.stream, r.contents = {}) : Xc.isFile(r.mode) ? (r.node_ops = Mc.ops_table.file.node, r.stream_ops = Mc.ops_table.file.stream, r.usedBytes = 0, r.contents = null) : Xc.isLink(r.mode) ? (r.node_ops = Mc.ops_table.link.node, r.stream_ops = Mc.ops_table.link.stream) : Xc.isChrdev(r.mode) && (r.node_ops = Mc.ops_table.chrdev.node, r.stream_ops = Mc.ops_table.chrdev.stream), r.timestamp = Date.now(), n && (n.contents[e] = r, n.timestamp = r.timestamp), r
        },
        getFileDataAsTypedArray: function(n) {
          return n.contents ? n.contents.subarray ? n.contents.subarray(0, n.usedBytes) : new Uint8Array(n.contents) : new Uint8Array(0)
        },
        expandFileStorage: function(n, e) {
          var i = n.contents ? n.contents.length : 0;
          if (!(i >= e)) {
            e = Math.max(e, i * (i < 1048576 ? 2 : 1.125) >>> 0), 0 != i && (e = Math.max(e, 256));
            var t = n.contents;
            n.contents = new Uint8Array(e), n.usedBytes > 0 && n.contents.set(t.subarray(0, n.usedBytes), 0)
          }
        },
        resizeFileStorage: function(n, e) {
          if (n.usedBytes != e)
            if (0 == e) n.contents = null, n.usedBytes = 0;
            else {
              var i = n.contents;
              n.contents = new Uint8Array(e), i && n.contents.set(i.subarray(0, Math.min(e, n.usedBytes))), n.usedBytes = e
            }
        },
        node_ops: {
          getattr: function(n) {
            var e = {};
            return e.dev = Xc.isChrdev(n.mode) ? n.id : 1, e.ino = n.id, e.mode = n.mode, e.nlink = 1, e.uid = 0, e.gid = 0, e.rdev = n.rdev, Xc.isDir(n.mode) ? e.size = 4096 : Xc.isFile(n.mode) ? e.size = n.usedBytes : Xc.isLink(n.mode) ? e.size = n.link.length : e.size = 0, e.atime = new Date(n.timestamp), e.mtime = new Date(n.timestamp), e.ctime = new Date(n.timestamp), e.blksize = 4096, e.blocks = Math.ceil(e.size / e.blksize), e
          },
          setattr: function(n, e) {
            void 0 !== e.mode && (n.mode = e.mode), void 0 !== e.timestamp && (n.timestamp = e.timestamp), void 0 !== e.size && Mc.resizeFileStorage(n, e.size)
          },
          lookup: function(n, e) {
            throw Xc.genericErrors[44]
          },
          mknod: function(n, e, i, t) {
            return Mc.createNode(n, e, i, t)
          },
          rename: function(n, e, i) {
            if (Xc.isDir(n.mode)) {
              var t;
              try {
                t = Xc.lookupNode(e, i)
              } catch (n) {}
              if (t)
                for (var r in t.contents) throw new Xc.ErrnoError(55)
            }
            delete n.parent.contents[n.name], n.parent.timestamp = Date.now(), n.name = i, e.contents[i] = n, e.timestamp = n.parent.timestamp, n.parent = e
          },
          unlink: function(n, e) {
            delete n.contents[e], n.timestamp = Date.now()
          },
          rmdir: function(n, e) {
            var i = Xc.lookupNode(n, e);
            for (var t in i.contents) throw new Xc.ErrnoError(55);
            delete n.contents[e], n.timestamp = Date.now()
          },
          readdir: function(n) {
            var e = [".", ".."];
            for (var i in n.contents) n.contents.hasOwnProperty(i) && e.push(i);
            return e
          },
          symlink: function(n, e, i) {
            var t = Mc.createNode(n, e, 41471, 0);
            return t.link = i, t
          },
          readlink: function(n) {
            if (!Xc.isLink(n.mode)) throw new Xc.ErrnoError(28);
            return n.link
          }
        },
        stream_ops: {
          read: function(n, e, i, t, r) {
            var o = n.node.contents;
            if (r >= n.node.usedBytes) return 0;
            var a = Math.min(n.node.usedBytes - r, t);
            if (a > 8 && o.subarray) e.set(o.subarray(r, r + a), i);
            else
              for (var l = 0; l < a; l++) e[i + l] = o[r + l];
            return a
          },
          write: function(e, i, t, r, o, a) {
            if (n.IsWxGame || i.buffer !== H.buffer || (a = !1), !r) return 0;
            var l = e.node;
            if (l.timestamp = Date.now(), i.subarray && (!l.contents || l.contents.subarray)) {
              if (a) return l.contents = i.subarray(t, t + r), l.usedBytes = r, r;
              if (0 === l.usedBytes && 0 === o) return l.contents = i.slice(t, t + r), l.usedBytes = r, r;
              if (o + r <= l.usedBytes) return l.contents.set(i.subarray(t, t + r), o), r
            }
            if (Mc.expandFileStorage(l, o + r), l.contents.subarray && i.subarray) l.contents.set(i.subarray(t, t + r), o);
            else
              for (var u = 0; u < r; u++) l.contents[o + u] = i[t + u];
            return l.usedBytes = Math.max(l.usedBytes, o + r), r
          },
          llseek: function(n, e, i) {
            var t = e;
            if (1 === i ? t += n.position : 2 === i && Xc.isFile(n.node.mode) && (t += n.node.usedBytes), t < 0) throw new Xc.ErrnoError(28);
            return t
          },
          allocate: function(n, e, i) {
            Mc.expandFileStorage(n.node, e + i), n.node.usedBytes = Math.max(n.node.usedBytes, e + i)
          },
          mmap: function(n, e, i, t, r, o) {
            if (0 !== e) throw new Xc.ErrnoError(28);
            if (!Xc.isFile(n.node.mode)) throw new Xc.ErrnoError(43);
            var a, l, u = n.node.contents;
            if (2 & o || u.buffer !== q) {
              if ((t > 0 || t + i < u.length) && (u = u.subarray ? u.subarray(t, t + i) : Array.prototype.slice.call(u, t, t + i)), l = !0, !(a = kc(i))) throw new Xc.ErrnoError(48);
              H.set(u, a)
            } else l = !1, a = u.byteOffset;
            return {
              ptr: a,
              allocated: l
            }
          },
          msync: function(n, e, i, t, r) {
            if (!Xc.isFile(n.node.mode)) throw new Xc.ErrnoError(43);
            if (2 & r) return 0;
            Mc.stream_ops.write(n, e, 0, t, i, !1);
            return 0
          }
        }
      },
      xc = GameGlobal.unityNamespace.IDBFS = {
        dbs: {},
        indexedDB: function() {
          if ("undefined" != typeof indexedDB) return indexedDB;
          var n = null;
          return "object" == typeof window && (n = window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB), K(n, "IDBFS used, but indexedDB not supported"), n
        },
        DB_VERSION: 21,
        DB_STORE_NAME: "FILE_DATA",
        mount: function(n) {
          return Mc.mount.apply(null, arguments)
        },
        syncfs: function(n, e, i) {
          xc.getLocalSet(n, (function(t, r) {
            if (t) return i(t);
            xc.getRemoteSet(n, (function(n, t) {
              if (n) return i(n);
              var o = e ? t : r,
                a = e ? r : t;
              xc.reconcile(o, a, i)
            }))
          }))
        },
        getDB: function(n, e) {
          var i, t = xc.dbs[n];
          if (t) return e(null, t);
          try {
            i = xc.indexedDB().open(n, xc.DB_VERSION)
          } catch (n) {
            return e(n)
          }
          if (!i) return e("Unable to connect to IndexedDB");
          i.onupgradeneeded = function(n) {
            var e, i = n.target.result,
              t = n.target.transaction;
            (e = i.objectStoreNames.contains(xc.DB_STORE_NAME) ? t.objectStore(xc.DB_STORE_NAME) : i.createObjectStore(xc.DB_STORE_NAME)).indexNames.contains("timestamp") || e.createIndex("timestamp", "timestamp", {
              unique: !1
            })
          }, i.onsuccess = function() {
            t = i.result, xc.dbs[n] = t, e(null, t)
          }, i.onerror = function(n) {
            e(this.error), n.preventDefault()
          }
        },
        getLocalSet: function(n, e) {
          var i = {};

          function t(n) {
            return "." !== n && ".." !== n
          }

          function r(n) {
            return function(e) {
              return bc.join2(n, e)
            }
          }
          for (var o = Xc.readdir(n.mountpoint).filter(t).map(r(n.mountpoint)); o.length;) {
            var a, l = o.pop();
            try {
              a = Xc.stat(l)
            } catch (n) {
              return e(n)
            }
            Xc.isDir(a.mode) && o.push.apply(o, Xc.readdir(l).filter(t).map(r(l))), i[l] = {
              timestamp: a.mtime
            }
          }
          return e(null, {
            type: "local",
            entries: i
          })
        },
        getRemoteSet: function(n, e) {
          var i = {};
          xc.getDB(n.mountpoint, (function(n, t) {
            if (n) return e(n);
            try {
              var r = t.transaction([xc.DB_STORE_NAME], "readonly");
              r.onerror = function(n) {
                e(this.error), n.preventDefault()
              }, r.objectStore(xc.DB_STORE_NAME).index("timestamp").openKeyCursor().onsuccess = function(n) {
                var r = n.target.result;
                if (!r) return e(null, {
                  type: "remote",
                  db: t,
                  entries: i
                });
                i[r.primaryKey] = {
                  timestamp: r.key
                }, r.continue()
              }
            } catch (n) {
              return e(n)
            }
          }))
        },
        loadLocalEntry: function(n, e) {
          var i, t;
          try {
            t = Xc.lookupPath(n).node, i = Xc.stat(n)
          } catch (n) {
            return e(n)
          }
          return Xc.isDir(i.mode) ? e(null, {
            timestamp: i.mtime,
            mode: i.mode
          }) : Xc.isFile(i.mode) ? (t.contents = Mc.getFileDataAsTypedArray(t), e(null, {
            timestamp: i.mtime,
            mode: i.mode,
            contents: t.contents
          })) : e(new Error("node type not supported"))
        },
        storeLocalEntry: function(n, e, i) {
          try {
            if (Xc.isDir(e.mode)) Xc.mkdirTree(n, e.mode);
            else {
              if (!Xc.isFile(e.mode)) return i(new Error("node type not supported"));
              Xc.writeFile(n, e.contents, {
                canOwn: !0
              })
            }
            Xc.chmod(n, e.mode), Xc.utime(n, e.timestamp, e.timestamp)
          } catch (n) {
            return i(n)
          }
          i(null)
        },
        removeLocalEntry: function(n, e) {
          try {
            Xc.lookupPath(n);
            var i = Xc.stat(n);
            Xc.isDir(i.mode) ? Xc.rmdir(n) : Xc.isFile(i.mode) && Xc.unlink(n)
          } catch (n) {
            return e(n)
          }
          e(null)
        },
        loadRemoteEntry: function(n, e, i) {
          var t = n.get(e);
          t.onsuccess = function(n) {
            i(null, n.target.result)
          }, t.onerror = function(n) {
            i(this.error), n.preventDefault()
          }
        },
        storeRemoteEntry: function(n, e, i, t) {
          var r = n.put(i, e);
          r.onsuccess = function() {
            t(null)
          }, r.onerror = function(n) {
            t(this.error), n.preventDefault()
          }
        },
        removeRemoteEntry: function(n, e, i) {
          var t = n.delete(e);
          t.onsuccess = function() {
            i(null)
          }, t.onerror = function(n) {
            i(this.error), n.preventDefault()
          }
        },
        reconcile: function(n, e, i) {
          var t = 0,
            r = [];
          Object.keys(n.entries).forEach((function(i) {
            var o = n.entries[i],
              a = e.entries[i];
            a && o.timestamp.getTime() == a.timestamp.getTime() || (r.push(i), t++)
          }));
          var o = [];
          if (Object.keys(e.entries).forEach((function(e) {
              n.entries[e] || (o.push(e), t++)
            })), !t) return i(null);
          var a = !1,
            l = ("remote" === n.type ? n.db : e.db).transaction([xc.DB_STORE_NAME], "readwrite"),
            u = l.objectStore(xc.DB_STORE_NAME);

          function f(n) {
            if (n && !a) return a = !0, i(n)
          }
          l.onerror = function(n) {
            f(this.error), n.preventDefault()
          }, l.oncomplete = function(n) {
            a || i(null)
          }, r.sort().forEach((function(n) {
            "local" === e.type ? xc.loadRemoteEntry(u, n, (function(e, i) {
              if (e) return f(e);
              xc.storeLocalEntry(n, i, f)
            })) : xc.loadLocalEntry(n, (function(e, i) {
              if (e) return f(e);
              xc.storeRemoteEntry(u, n, i, f)
            }))
          })), o.sort().reverse().forEach((function(n) {
            "local" === e.type ? xc.removeLocalEntry(n, f) : xc.removeRemoteEntry(u, n, f)
          }))
        }
      },
      Xc = GameGlobal.unityNamespace.FS = {
        root: null,
        mounts: [],
        devices: {},
        streams: [],
        nextInode: 1,
        nameTable: null,
        currentPath: "/",
        initialized: !1,
        ignorePermissions: !0,
        trackingDelegate: {},
        tracking: {
          openFlags: {
            READ: 1,
            WRITE: 2
          }
        },
        ErrnoError: null,
        genericErrors: {},
        filesystems: null,
        syncFSRequests: 0,
        lookupPath: function(n, e) {
          if (e = e || {}, !(n = Dc.resolve(Xc.cwd(), n))) return {
            path: "",
            node: null
          };
          var i = {
            follow_mount: !0,
            recurse_count: 0
          };
          for (var t in i) void 0 === e[t] && (e[t] = i[t]);
          if (e.recurse_count > 8) throw new Xc.ErrnoError(32);
          for (var r = bc.normalizeArray(n.split("/").filter((function(n) {
              return !!n
            })), !1), o = Xc.root, a = "/", l = 0; l < r.length; l++) {
            var u = l === r.length - 1;
            if (u && e.parent) break;
            if (o = Xc.lookupNode(o, r[l]), a = bc.join2(a, r[l]), Xc.isMountpoint(o) && (!u || u && e.follow_mount) && (o = o.mounted.root), !u || e.follow)
              for (var f = 0; Xc.isLink(o.mode);) {
                var c = Xc.readlink(a);
                if (a = Dc.resolve(bc.dirname(a), c), o = Xc.lookupPath(a, {
                    recurse_count: e.recurse_count
                  }).node, f++ > 40) throw new Xc.ErrnoError(32)
              }
          }
          return {
            path: a,
            node: o
          }
        },
        getPath: function(n) {
          for (var e;;) {
            if (Xc.isRoot(n)) {
              var i = n.mount.mountpoint;
              return e ? "/" !== i[i.length - 1] ? i + "/" + e : i + e : i
            }
            e = e ? n.name + "/" + e : n.name, n = n.parent
          }
        },
        hashName: function(n, e) {
          for (var i = 0, t = 0; t < e.length; t++) i = (i << 5) - i + e.charCodeAt(t) | 0;
          return (n + i >>> 0) % Xc.nameTable.length
        },
        hashAddNode: function(n) {
          var e = Xc.hashName(n.parent.id, n.name);
          n.name_next = Xc.nameTable[e], Xc.nameTable[e] = n
        },
        hashRemoveNode: function(n) {
          var e = Xc.hashName(n.parent.id, n.name);
          if (Xc.nameTable[e] === n) Xc.nameTable[e] = n.name_next;
          else
            for (var i = Xc.nameTable[e]; i;) {
              if (i.name_next === n) {
                i.name_next = n.name_next;
                break
              }
              i = i.name_next
            }
        },
        lookupNode: function(n, e) {
          var i = Xc.mayLookup(n);
          if (i) throw new Xc.ErrnoError(i, n);
          for (var t = Xc.hashName(n.id, e), r = Xc.nameTable[t]; r; r = r.name_next) {
            var o = r.name;
            if (r.parent.id === n.id && o === e) return r
          }
          return Xc.lookup(n, e)
        },
        createNode: function(n, e, i, t) {
          var r = new Xc.FSNode(n, e, i, t);
          return Xc.hashAddNode(r), r
        },
        destroyNode: function(n) {
          Xc.hashRemoveNode(n)
        },
        isRoot: function(n) {
          return n === n.parent
        },
        isMountpoint: function(n) {
          return !!n.mounted
        },
        isFile: function(n) {
          return 32768 == (61440 & n)
        },
        isDir: function(n) {
          return 16384 == (61440 & n)
        },
        isLink: function(n) {
          return 40960 == (61440 & n)
        },
        isChrdev: function(n) {
          return 8192 == (61440 & n)
        },
        isBlkdev: function(n) {
          return 24576 == (61440 & n)
        },
        isFIFO: function(n) {
          return 4096 == (61440 & n)
        },
        isSocket: function(n) {
          return 49152 == (49152 & n)
        },
        flagModes: {
          r: 0,
          "r+": 2,
          w: 577,
          "w+": 578,
          a: 1089,
          "a+": 1090
        },
        modeStringToFlags: function(n) {
          var e = Xc.flagModes[n];
          if (void 0 === e) throw new Error("Unknown file open mode: " + n);
          return e
        },
        flagsToPermissionString: function(n) {
          var e = ["r", "w", "rw"][3 & n];
          return 512 & n && (e += "w"), e
        },
        nodePermissions: function(n, e) {
          return Xc.ignorePermissions || (!e.includes("r") || 292 & n.mode) && (!e.includes("w") || 146 & n.mode) && (!e.includes("x") || 73 & n.mode) ? 0 : 2
        },
        mayLookup: function(n) {
          var e = Xc.nodePermissions(n, "x");
          return e || (n.node_ops.lookup ? 0 : 2)
        },
        mayCreate: function(n, e) {
          try {
            Xc.lookupNode(n, e);
            return 20
          } catch (n) {}
          return Xc.nodePermissions(n, "wx")
        },
        mayDelete: function(n, e, i) {
          var t;
          try {
            t = Xc.lookupNode(n, e)
          } catch (n) {
            return n.errno
          }
          var r = Xc.nodePermissions(n, "wx");
          if (r) return r;
          if (i) {
            if (!Xc.isDir(t.mode)) return 54;
            if (Xc.isRoot(t) || Xc.getPath(t) === Xc.cwd()) return 10
          } else if (Xc.isDir(t.mode)) return 31;
          return 0
        },
        mayOpen: function(n, e) {
          return n ? Xc.isLink(n.mode) ? 32 : Xc.isDir(n.mode) && ("r" !== Xc.flagsToPermissionString(e) || 512 & e) ? 31 : Xc.nodePermissions(n, Xc.flagsToPermissionString(e)) : 44
        },
        MAX_OPEN_FDS: 4096,
        nextfd: function(n, e) {
          n = n || 0, e = e || Xc.MAX_OPEN_FDS;
          for (var i = n; i <= e; i++)
            if (!Xc.streams[i]) return i;
          throw new Xc.ErrnoError(33)
        },
        getStream: function(n) {
          return Xc.streams[n]
        },
        createStream: function(n, e, i) {
          Xc.FSStream || (Xc.FSStream = function() {}, Xc.FSStream.prototype = {
            object: {
              get: function() {
                return this.node
              },
              set: function(n) {
                this.node = n
              }
            },
            isRead: {
              get: function() {
                return 1 != (2097155 & this.flags)
              }
            },
            isWrite: {
              get: function() {
                return 0 != (2097155 & this.flags)
              }
            },
            isAppend: {
              get: function() {
                return 1024 & this.flags
              }
            }
          });
          var t = new Xc.FSStream;
          for (var r in n) t[r] = n[r];
          n = t;
          var o = Xc.nextfd(e, i);
          return n.fd = o, Xc.streams[o] = n, n
        },
        closeStream: function(n) {
          Xc.streams[n] = null
        },
        chrdev_stream_ops: {
          open: function(n) {
            var e = Xc.getDevice(n.node.rdev);
            n.stream_ops = e.stream_ops, n.stream_ops.open && n.stream_ops.open(n)
          },
          llseek: function() {
            throw new Xc.ErrnoError(70)
          }
        },
        major: function(n) {
          return n >> 8
        },
        minor: function(n) {
          return 255 & n
        },
        makedev: function(n, e) {
          return n << 8 | e
        },
        registerDevice: function(n, e) {
          Xc.devices[n] = {
            stream_ops: e
          }
        },
        getDevice: function(n) {
          return Xc.devices[n]
        },
        getMounts: function(n) {
          for (var e = [], i = [n]; i.length;) {
            var t = i.pop();
            e.push(t), i.push.apply(i, t.mounts)
          }
          return e
        },
        syncfs: function(n, e) {
          "function" == typeof n && (e = n, n = !1), Xc.syncFSRequests++, Xc.syncFSRequests > 1 && x("warning: " + Xc.syncFSRequests + " FS.syncfs operations in flight at once, probably just doing extra work");
          var i = Xc.getMounts(Xc.root.mount),
            t = 0;

          function r(n) {
            return Xc.syncFSRequests--, e(n)
          }

          function o(n) {
            if (n) return o.errored ? void 0 : (o.errored = !0, r(n));
            ++t >= i.length && r(null)
          }
          i.forEach((function(e) {
            if (!e.type.syncfs) return o(null);
            e.type.syncfs(e, n, o)
          }))
        },
        mount: function(n, e, i) {
          var t, r = "/" === i,
            o = !i;
          if (r && Xc.root) throw new Xc.ErrnoError(10);
          if (!r && !o) {
            var a = Xc.lookupPath(i, {
              follow_mount: !1
            });
            if (i = a.path, t = a.node, Xc.isMountpoint(t)) throw new Xc.ErrnoError(10);
            if (!Xc.isDir(t.mode)) throw new Xc.ErrnoError(54)
          }
          var l = {
              type: n,
              opts: e,
              mountpoint: i,
              mounts: []
            },
            u = n.mount(l);
          return u.mount = l, l.root = u, r ? Xc.root = u : t && (t.mounted = l, t.mount && t.mount.mounts.push(l)), u
        },
        unmount: function(n) {
          var e = Xc.lookupPath(n, {
            follow_mount: !1
          });
          if (!Xc.isMountpoint(e.node)) throw new Xc.ErrnoError(28);
          var i = e.node,
            t = i.mounted,
            r = Xc.getMounts(t);
          Object.keys(Xc.nameTable).forEach((function(n) {
            for (var e = Xc.nameTable[n]; e;) {
              var i = e.name_next;
              r.includes(e.mount) && Xc.destroyNode(e), e = i
            }
          })), i.mounted = null;
          var o = i.mount.mounts.indexOf(t);
          i.mount.mounts.splice(o, 1)
        },
        lookup: function(n, e) {
          return n.node_ops.lookup(n, e)
        },
        mknod: function(n, e, i) {
          var t = Xc.lookupPath(n, {
              parent: !0
            }).node,
            r = bc.basename(n);
          if (!r || "." === r || ".." === r) throw new Xc.ErrnoError(28);
          var o = Xc.mayCreate(t, r);
          if (o) throw new Xc.ErrnoError(o);
          if (!t.node_ops.mknod) throw new Xc.ErrnoError(63);
          return t.node_ops.mknod(t, r, e, i)
        },
        create: function(n, e) {
          return e = void 0 !== e ? e : 438, e &= 4095, e |= 32768, Xc.mknod(n, e, 0)
        },
        mkdir: function(n, e) {
          return e = void 0 !== e ? e : 511, e &= 1023, e |= 16384, Xc.mknod(n, e, 0)
        },
        mkdirTree: function(n, e) {
          for (var i = n.split("/"), t = "", r = 0; r < i.length; ++r)
            if (i[r]) {
              t += "/" + i[r];
              try {
                Xc.mkdir(t, e)
              } catch (n) {
                if (20 != n.errno) throw n
              }
            }
        },
        mkdev: function(n, e, i) {
          return void 0 === i && (i = e, e = 438), e |= 8192, Xc.mknod(n, e, i)
        },
        symlink: function(n, e) {
          if (!Dc.resolve(n)) throw new Xc.ErrnoError(44);
          var i = Xc.lookupPath(e, {
            parent: !0
          }).node;
          if (!i) throw new Xc.ErrnoError(44);
          var t = bc.basename(e),
            r = Xc.mayCreate(i, t);
          if (r) throw new Xc.ErrnoError(r);
          if (!i.node_ops.symlink) throw new Xc.ErrnoError(63);
          return i.node_ops.symlink(i, t, n)
        },
        rename: function(n, e) {
          var i, t, r = bc.dirname(n),
            o = bc.dirname(e),
            a = bc.basename(n),
            l = bc.basename(e);
          if (i = Xc.lookupPath(n, {
              parent: !0
            }).node, t = Xc.lookupPath(e, {
              parent: !0
            }).node, !i || !t) throw new Xc.ErrnoError(44);
          if (i.mount !== t.mount) throw new Xc.ErrnoError(75);
          var u, f = Xc.lookupNode(i, a),
            c = Dc.relative(n, o);
          if ("." !== c.charAt(0)) throw new Xc.ErrnoError(28);
          if ("." !== (c = Dc.relative(e, r)).charAt(0)) throw new Xc.ErrnoError(55);
          try {
            u = Xc.lookupNode(t, l)
          } catch (n) {}
          if (f !== u) {
            var s = Xc.isDir(f.mode),
              d = Xc.mayDelete(i, a, s);
            if (d) throw new Xc.ErrnoError(d);
            if (d = u ? Xc.mayDelete(t, l, s) : Xc.mayCreate(t, l)) throw new Xc.ErrnoError(d);
            if (!i.node_ops.rename) throw new Xc.ErrnoError(63);
            if (Xc.isMountpoint(f) || u && Xc.isMountpoint(u)) throw new Xc.ErrnoError(10);
            if (t !== i && (d = Xc.nodePermissions(i, "w"))) throw new Xc.ErrnoError(d);
            try {
              Xc.trackingDelegate.willMovePath && Xc.trackingDelegate.willMovePath(n, e)
            } catch (i) {
              x("FS.trackingDelegate['willMovePath']('" + n + "', '" + e + "') threw an exception: " + i.message)
            }
            Xc.hashRemoveNode(f);
            try {
              i.node_ops.rename(f, t, l)
            } catch (n) {
              throw n
            } finally {
              Xc.hashAddNode(f)
            }
            try {
              Xc.trackingDelegate.onMovePath && Xc.trackingDelegate.onMovePath(n, e)
            } catch (i) {
              x("FS.trackingDelegate['onMovePath']('" + n + "', '" + e + "') threw an exception: " + i.message)
            }
          }
        },
        rmdir: function(n) {
          var e = Xc.lookupPath(n, {
              parent: !0
            }).node,
            i = bc.basename(n),
            t = Xc.lookupNode(e, i),
            r = Xc.mayDelete(e, i, !0);
          if (r) throw new Xc.ErrnoError(r);
          if (!e.node_ops.rmdir) throw new Xc.ErrnoError(63);
          if (Xc.isMountpoint(t)) throw new Xc.ErrnoError(10);
          try {
            Xc.trackingDelegate.willDeletePath && Xc.trackingDelegate.willDeletePath(n)
          } catch (e) {
            x("FS.trackingDelegate['willDeletePath']('" + n + "') threw an exception: " + e.message)
          }
          e.node_ops.rmdir(e, i), Xc.destroyNode(t);
          try {
            Xc.trackingDelegate.onDeletePath && Xc.trackingDelegate.onDeletePath(n)
          } catch (e) {
            x("FS.trackingDelegate['onDeletePath']('" + n + "') threw an exception: " + e.message)
          }
        },
        readdir: function(n) {
          var e = Xc.lookupPath(n, {
            follow: !0
          }).node;
          if (!e.node_ops.readdir) throw new Xc.ErrnoError(54);
          return e.node_ops.readdir(e)
        },
        unlink: function(n) {
          var e = Xc.lookupPath(n, {
              parent: !0
            }).node,
            i = bc.basename(n),
            t = Xc.lookupNode(e, i),
            r = Xc.mayDelete(e, i, !1);
          if (r) throw new Xc.ErrnoError(r);
          if (!e.node_ops.unlink) throw new Xc.ErrnoError(63);
          if (Xc.isMountpoint(t)) throw new Xc.ErrnoError(10);
          try {
            Xc.trackingDelegate.willDeletePath && Xc.trackingDelegate.willDeletePath(n)
          } catch (e) {
            x("FS.trackingDelegate['willDeletePath']('" + n + "') threw an exception: " + e.message)
          }
          e.node_ops.unlink(e, i), Xc.destroyNode(t);
          try {
            Xc.trackingDelegate.onDeletePath && Xc.trackingDelegate.onDeletePath(n)
          } catch (e) {
            x("FS.trackingDelegate['onDeletePath']('" + n + "') threw an exception: " + e.message)
          }
        },
        readlink: function(n) {
          var e = Xc.lookupPath(n).node;
          if (!e) throw new Xc.ErrnoError(44);
          if (!e.node_ops.readlink) throw new Xc.ErrnoError(28);
          return Dc.resolve(Xc.getPath(e.parent), e.node_ops.readlink(e))
        },
        stat: function(n, e) {
          var i = Xc.lookupPath(n, {
            follow: !e
          }).node;
          if (!i) throw new Xc.ErrnoError(44);
          if (!i.node_ops.getattr) throw new Xc.ErrnoError(63);
          return i.node_ops.getattr(i)
        },
        lstat: function(n) {
          return Xc.stat(n, !0)
        },
        chmod: function(n, e, i) {
          var t;
          "string" == typeof n ? t = Xc.lookupPath(n, {
            follow: !i
          }).node : t = n;
          if (!t.node_ops.setattr) throw new Xc.ErrnoError(63);
          t.node_ops.setattr(t, {
            mode: 4095 & e | -4096 & t.mode,
            timestamp: Date.now()
          })
        },
        lchmod: function(n, e) {
          Xc.chmod(n, e, !0)
        },
        fchmod: function(n, e) {
          var i = Xc.getStream(n);
          if (!i) throw new Xc.ErrnoError(8);
          Xc.chmod(i.node, e)
        },
        chown: function(n, e, i, t) {
          var r;
          "string" == typeof n ? r = Xc.lookupPath(n, {
            follow: !t
          }).node : r = n;
          if (!r.node_ops.setattr) throw new Xc.ErrnoError(63);
          r.node_ops.setattr(r, {
            timestamp: Date.now()
          })
        },
        lchown: function(n, e, i) {
          Xc.chown(n, e, i, !0)
        },
        fchown: function(n, e, i) {
          var t = Xc.getStream(n);
          if (!t) throw new Xc.ErrnoError(8);
          Xc.chown(t.node, e, i)
        },
        truncate: function(n, e) {
          if (e < 0) throw new Xc.ErrnoError(28);
          var i;
          "string" == typeof n ? i = Xc.lookupPath(n, {
            follow: !0
          }).node : i = n;
          if (!i.node_ops.setattr) throw new Xc.ErrnoError(63);
          if (Xc.isDir(i.mode)) throw new Xc.ErrnoError(31);
          if (!Xc.isFile(i.mode)) throw new Xc.ErrnoError(28);
          var t = Xc.nodePermissions(i, "w");
          if (t) throw new Xc.ErrnoError(t);
          i.node_ops.setattr(i, {
            size: e,
            timestamp: Date.now()
          })
        },
        ftruncate: function(n, e) {
          var i = Xc.getStream(n);
          if (!i) throw new Xc.ErrnoError(8);
          if (0 == (2097155 & i.flags)) throw new Xc.ErrnoError(28);
          Xc.truncate(i.node, e)
        },
        utime: function(n, e, i) {
          var t = Xc.lookupPath(n, {
            follow: !0
          }).node;
          t.node_ops.setattr(t, {
            timestamp: Math.max(e, i)
          })
        },
        open: function(e, i, t, r, o) {
          if ("" === e) throw new Xc.ErrnoError(44);
          var a;
          if (t = void 0 === t ? 438 : t, t = 64 & (i = "string" == typeof i ? Xc.modeStringToFlags(i) : i) ? 4095 & t | 32768 : 0, "object" == typeof e) a = e;
          else {
            e = bc.normalize(e);
            try {
              a = Xc.lookupPath(e, {
                follow: !(131072 & i)
              }).node
            } catch (n) {}
          }
          var l = !1;
          if (64 & i)
            if (a) {
              if (128 & i) throw new Xc.ErrnoError(20)
            } else a = Xc.mknod(e, t, 0), l = !0;
          if (!a) throw new Xc.ErrnoError(44);
          if (Xc.isChrdev(a.mode) && (i &= -513), 65536 & i && !Xc.isDir(a.mode)) throw new Xc.ErrnoError(54);
          if (!l) {
            var u = Xc.mayOpen(a, i);
            if (u) throw new Xc.ErrnoError(u)
          }
          512 & i && Xc.truncate(a, 0), i &= -131713;
          var f = Xc.createStream({
            node: a,
            path: Xc.getPath(a),
            flags: i,
            seekable: !0,
            position: 0,
            stream_ops: a.stream_ops,
            ungotten: [],
            error: !1
          }, r, o);
          f.stream_ops.open && f.stream_ops.open(f), !n.logReadFiles || 1 & i || (Xc.readFiles || (Xc.readFiles = {}), e in Xc.readFiles || (Xc.readFiles[e] = 1, x("FS.trackingDelegate error on read file: " + e)));
          try {
            if (Xc.trackingDelegate.onOpenFile) {
              var c = 0;
              1 != (2097155 & i) && (c |= Xc.tracking.openFlags.READ), 0 != (2097155 & i) && (c |= Xc.tracking.openFlags.WRITE), Xc.trackingDelegate.onOpenFile(e, c)
            }
          } catch (n) {
            x("FS.trackingDelegate['onOpenFile']('" + e + "', flags) threw an exception: " + n.message)
          }
          return f
        },
        close: function(n) {
          if (Xc.isClosed(n)) throw new Xc.ErrnoError(8);
          n.getdents && (n.getdents = null);
          try {
            n.stream_ops.close && n.stream_ops.close(n)
          } catch (n) {
            throw n
          } finally {
            Xc.closeStream(n.fd)
          }
          n.fd = null
        },
        isClosed: function(n) {
          return null === n.fd
        },
        llseek: function(n, e, i) {
          if (Xc.isClosed(n)) throw new Xc.ErrnoError(8);
          if (!n.seekable || !n.stream_ops.llseek) throw new Xc.ErrnoError(70);
          if (0 != i && 1 != i && 2 != i) throw new Xc.ErrnoError(28);
          return n.position = n.stream_ops.llseek(n, e, i), n.ungotten = [], n.position
        },
        read: function(n, e, i, t, r) {
          if (t < 0 || r < 0) throw new Xc.ErrnoError(28);
          if (Xc.isClosed(n)) throw new Xc.ErrnoError(8);
          if (1 == (2097155 & n.flags)) throw new Xc.ErrnoError(8);
          if (Xc.isDir(n.node.mode)) throw new Xc.ErrnoError(31);
          if (!n.stream_ops.read) throw new Xc.ErrnoError(28);
          var o = void 0 !== r;
          if (o) {
            if (!n.seekable) throw new Xc.ErrnoError(70)
          } else r = n.position;
          var a = n.stream_ops.read(n, e, i, t, r);
          return o || (n.position += a), a
        },
        write: function(n, e, i, t, r, o) {
          if (t < 0 || r < 0) throw new Xc.ErrnoError(28);
          if (Xc.isClosed(n)) throw new Xc.ErrnoError(8);
          if (0 == (2097155 & n.flags)) throw new Xc.ErrnoError(8);
          if (Xc.isDir(n.node.mode)) throw new Xc.ErrnoError(31);
          if (!n.stream_ops.write) throw new Xc.ErrnoError(28);
          n.seekable && 1024 & n.flags && Xc.llseek(n, 0, 2);
          var a = void 0 !== r;
          if (a) {
            if (!n.seekable) throw new Xc.ErrnoError(70)
          } else r = n.position;
          var l = n.stream_ops.write(n, e, i, t, r, o);
          a || (n.position += l);
          try {
            n.path && Xc.trackingDelegate.onWriteToFile && Xc.trackingDelegate.onWriteToFile(n.path)
          } catch (e) {
            x("FS.trackingDelegate['onWriteToFile']('" + n.path + "') threw an exception: " + e.message)
          }
          return l
        },
        allocate: function(n, e, i) {
          if (Xc.isClosed(n)) throw new Xc.ErrnoError(8);
          if (e < 0 || i <= 0) throw new Xc.ErrnoError(28);
          if (0 == (2097155 & n.flags)) throw new Xc.ErrnoError(8);
          if (!Xc.isFile(n.node.mode) && !Xc.isDir(n.node.mode)) throw new Xc.ErrnoError(43);
          if (!n.stream_ops.allocate) throw new Xc.ErrnoError(138);
          n.stream_ops.allocate(n, e, i)
        },
        mmap: function(n, e, i, t, r, o) {
          if (0 != (2 & r) && 0 == (2 & o) && 2 != (2097155 & n.flags)) throw new Xc.ErrnoError(2);
          if (1 == (2097155 & n.flags)) throw new Xc.ErrnoError(2);
          if (!n.stream_ops.mmap) throw new Xc.ErrnoError(43);
          return n.stream_ops.mmap(n, e, i, t, r, o)
        },
        msync: function(n, e, i, t, r) {
          return n && n.stream_ops.msync ? n.stream_ops.msync(n, e, i, t, r) : 0
        },
        munmap: function(n) {
          return 0
        },
        ioctl: function(n, e, i) {
          if (!n.stream_ops.ioctl) throw new Xc.ErrnoError(59);
          return n.stream_ops.ioctl(n, e, i)
        },
        readFile: function(n, e) {
          if ((e = e || {}).flags = e.flags || 0, e.encoding = e.encoding || "binary", "utf8" !== e.encoding && "binary" !== e.encoding) throw new Error('Invalid encoding type "' + e.encoding + '"');
          var i, t = Xc.open(n, e.flags),
            r = Xc.stat(n).size,
            o = new Uint8Array(r);
          return Xc.read(t, o, 0, r, 0), "utf8" === e.encoding ? i = tn(o, 0) : "binary" === e.encoding && (i = o), Xc.close(t), i
        },
        writeFile: function(n, e, i) {
          (i = i || {}).flags = i.flags || 577;
          var t = Xc.open(n, i.flags, i.mode);
          if ("string" == typeof e) {
            var r = new Uint8Array(ln(e) + 1),
              o = on(e, r, 0, r.length);
            Xc.write(t, r, 0, o, void 0, i.canOwn)
          } else {
            if (!ArrayBuffer.isView(e)) throw new Error("Unsupported data type");
            Xc.write(t, e, 0, e.byteLength, void 0, i.canOwn)
          }
          Xc.close(t)
        },
        cwd: function() {
          return Xc.currentPath
        },
        chdir: function(n) {
          var e = Xc.lookupPath(n, {
            follow: !0
          });
          if (null === e.node) throw new Xc.ErrnoError(44);
          if (!Xc.isDir(e.node.mode)) throw new Xc.ErrnoError(54);
          var i = Xc.nodePermissions(e.node, "x");
          if (i) throw new Xc.ErrnoError(i);
          Xc.currentPath = e.path
        },
        createDefaultDirectories: function() {
          Xc.mkdir("/tmp"), Xc.mkdir("/home"), Xc.mkdir("/home/web_user")
        },
        createDefaultDevices: function() {
          Xc.mkdir("/dev"), Xc.registerDevice(Xc.makedev(1, 3), {
            read: function() {
              return 0
            },
            write: function(n, e, i, t, r) {
              return t
            }
          }), Xc.mkdev("/dev/null", Xc.makedev(1, 3)), Ac.register(Xc.makedev(5, 0), Ac.default_tty_ops), Ac.register(Xc.makedev(6, 0), Ac.default_tty1_ops), Xc.mkdev("/dev/tty", Xc.makedev(5, 0)), Xc.mkdev("/dev/tty1", Xc.makedev(6, 0));
          var n = Wc();
          Xc.createDevice("/dev", "random", n), Xc.createDevice("/dev", "urandom", n), Xc.mkdir("/dev/shm"), Xc.mkdir("/dev/shm/tmp")
        },
        createSpecialDirectories: function() {
          Xc.mkdir("/proc");
          var n = Xc.mkdir("/proc/self");
          Xc.mkdir("/proc/self/fd"), Xc.mount({
            mount: function() {
              var e = Xc.createNode(n, "fd", 16895, 73);
              return e.node_ops = {
                lookup: function(n, e) {
                  var i = +e,
                    t = Xc.getStream(i);
                  if (!t) throw new Xc.ErrnoError(8);
                  var r = {
                    parent: null,
                    mount: {
                      mountpoint: "fake"
                    },
                    node_ops: {
                      readlink: function() {
                        return t.path
                      }
                    }
                  };
                  return r.parent = r, r
                }
              }, e
            }
          }, {}, "/proc/self/fd")
        },
        createStandardStreams: function() {
          n.stdin ? Xc.createDevice("/dev", "stdin", n.stdin) : Xc.symlink("/dev/tty", "/dev/stdin"), n.stdout ? Xc.createDevice("/dev", "stdout", null, n.stdout) : Xc.symlink("/dev/tty", "/dev/stdout"), n.stderr ? Xc.createDevice("/dev", "stderr", null, n.stderr) : Xc.symlink("/dev/tty1", "/dev/stderr");
          Xc.open("/dev/stdin", 0), Xc.open("/dev/stdout", 1), Xc.open("/dev/stderr", 1)
        },
        ensureErrnoError: function() {
          Xc.ErrnoError || (Xc.ErrnoError = function(n, e) {
            this.node = e, this.setErrno = function(n) {
              this.errno = n
            }, this.setErrno(n), this.message = "FS error"
          }, Xc.ErrnoError.prototype = new Error, Xc.ErrnoError.prototype.constructor = Xc.ErrnoError, [44].forEach((function(n) {
            Xc.genericErrors[n] = new Xc.ErrnoError(n), Xc.genericErrors[n].stack = "<generic error, no stack>"
          })))
        },
        staticInit: function() {
          Xc.ensureErrnoError(), Xc.nameTable = new Array(4096), Xc.mount(Mc, {}, "/"), Xc.createDefaultDirectories(), Xc.createDefaultDevices(), Xc.createSpecialDirectories(), Xc.filesystems = {
            MEMFS: Mc,
            IDBFS: xc
          }
        },
        init: function(e, i, t) {
          Xc.init.initialized = !0, Xc.ensureErrnoError(), n.stdin = e || n.stdin, n.stdout = i || n.stdout, n.stderr = t || n.stderr, Xc.createStandardStreams()
        },
        quit: function() {
          Xc.init.initialized = !1;
          var e = n._fflush;
          e && e(0);
          for (var i = 0; i < Xc.streams.length; i++) {
            var t = Xc.streams[i];
            t && Xc.close(t)
          }
        },
        getMode: function(n, e) {
          var i = 0;
          return n && (i |= 365), e && (i |= 146), i
        },
        findObject: function(n, e) {
          var i = Xc.analyzePath(n, e);
          return i.exists ? i.object : null
        },
        analyzePath: function(n, e) {
          try {
            n = (t = Xc.lookupPath(n, {
              follow: !e
            })).path
          } catch (n) {}
          var i = {
            isRoot: !1,
            exists: !1,
            error: 0,
            name: null,
            path: null,
            object: null,
            parentExists: !1,
            parentPath: null,
            parentObject: null
          };
          try {
            var t = Xc.lookupPath(n, {
              parent: !0
            });
            i.parentExists = !0, i.parentPath = t.path, i.parentObject = t.node, i.name = bc.basename(n), t = Xc.lookupPath(n, {
              follow: !e
            }), i.exists = !0, i.path = t.path, i.object = t.node, i.name = t.node.name, i.isRoot = "/" === t.path
          } catch (n) {
            i.error = n.errno
          }
          return i
        },
        createPath: function(n, e, i, t) {
          n = "string" == typeof n ? n : Xc.getPath(n);
          for (var r = e.split("/").reverse(); r.length;) {
            var o = r.pop();
            if (o) {
              var a = bc.join2(n, o);
              try {
                Xc.mkdir(a)
              } catch (n) {}
              n = a
            }
          }
          return a
        },
        createFile: function(n, e, i, t, r) {
          var o = bc.join2("string" == typeof n ? n : Xc.getPath(n), e),
            a = Xc.getMode(t, r);
          return Xc.create(o, a)
        },
        createDataFile: function(n, e, i, t, r, o) {
          var a = e ? bc.join2("string" == typeof n ? n : Xc.getPath(n), e) : n,
            l = Xc.getMode(t, r),
            u = Xc.create(a, l);
          if (i) {
            if ("string" == typeof i) {
              for (var f = new Array(i.length), c = 0, s = i.length; c < s; ++c) f[c] = i.charCodeAt(c);
              i = f
            }
            Xc.chmod(u, 146 | l);
            var d = Xc.open(u, 577);
            Xc.write(d, i, 0, i.length, 0, o), Xc.close(d), Xc.chmod(u, l)
          }
          return u
        },
        createDevice: function(n, e, i, t) {
          var r = bc.join2("string" == typeof n ? n : Xc.getPath(n), e),
            o = Xc.getMode(!!i, !!t);
          Xc.createDevice.major || (Xc.createDevice.major = 64);
          var a = Xc.makedev(Xc.createDevice.major++, 0);
          return Xc.registerDevice(a, {
            open: function(n) {
              n.seekable = !1
            },
            close: function(n) {
              t && t.buffer && t.buffer.length && t(10)
            },
            read: function(n, e, t, r, o) {
              for (var a = 0, l = 0; l < r; l++) {
                var u;
                try {
                  u = i()
                } catch (n) {
                  throw new Xc.ErrnoError(29)
                }
                if (void 0 === u && 0 === a) throw new Xc.ErrnoError(6);
                if (null == u) break;
                a++, e[t + l] = u
              }
              return a && (n.node.timestamp = Date.now()), a
            },
            write: function(n, e, i, r, o) {
              for (var a = 0; a < r; a++) try {
                t(e[i + a])
              } catch (n) {
                throw new Xc.ErrnoError(29)
              }
              return r && (n.node.timestamp = Date.now()), a
            }
          }), Xc.mkdev(r, o, a)
        },
        forceLoadFile: function(n) {
          if (n.isDevice || n.isFolder || n.link || n.contents) return !0;
          if ("undefined" != typeof XMLHttpRequest) throw new Error("Lazy loading should have been performed (contents set) in createLazyFile, but it was not. Lazy loading only works in web workers. Use --embed-file or --preload-file in emcc on the main thread.");
          if (!E) throw new Error("Cannot load without read() or XMLHttpRequest.");
          try {
            n.contents = pg(E(n.url), !0), n.usedBytes = n.contents.length
          } catch (n) {
            throw new Xc.ErrnoError(29)
          }
        },
        createLazyFile: function(n, e, i, t, r) {
          function o() {
            this.lengthKnown = !1, this.chunks = []
          }
          if (o.prototype.get = function(n) {
              if (!(n > this.length - 1 || n < 0)) {
                var e = n % this.chunkSize,
                  i = n / this.chunkSize | 0;
                return this.getter(i)[e]
              }
            }, o.prototype.setDataGetter = function(n) {
              this.getter = n
            }, o.prototype.cacheLength = function() {
              var n = new XMLHttpRequest;
              if (n.open("HEAD", i, !1), n.send(null), !(n.status >= 200 && n.status < 300 || 304 === n.status)) throw new Error("Couldn't load " + i + ". Status: " + n.status);
              var e, t = Number(n.getResponseHeader("Content-length")),
                r = (e = n.getResponseHeader("Accept-Ranges")) && "bytes" === e,
                o = (e = n.getResponseHeader("Content-Encoding")) && "gzip" === e,
                a = 1048576;
              r || (a = t);
              var l = this;
              l.setDataGetter((function(n) {
                var e = n * a,
                  r = (n + 1) * a - 1;
                if (r = Math.min(r, t - 1), void 0 === l.chunks[n] && (l.chunks[n] = function(n, e) {
                    if (n > e) throw new Error("invalid range (" + n + ", " + e + ") or no bytes requested!");
                    if (e > t - 1) throw new Error("only " + t + " bytes available! programmer error!");
                    var r = new XMLHttpRequest;
                    if (r.open("GET", i, !1), t !== a && r.setRequestHeader("Range", "bytes=" + n + "-" + e), "undefined" != typeof Uint8Array && (r.responseType = "arraybuffer"), r.overrideMimeType && r.overrideMimeType("text/plain; charset=x-user-defined"), r.send(null), !(r.status >= 200 && r.status < 300 || 304 === r.status)) throw new Error("Couldn't load " + i + ". Status: " + r.status);
                    return void 0 !== r.response ? new Uint8Array(r.response || []) : pg(r.responseText || "", !0)
                  }(e, r)), void 0 === l.chunks[n]) throw new Error("doXHR failed!");
                return l.chunks[n]
              })), !o && t || (a = t = 1, t = this.getter(0).length, a = t, M("LazyFiles on gzip forces download of the whole file when length is accessed")), this._length = t, this._chunkSize = a, this.lengthKnown = !0
            }, "undefined" != typeof XMLHttpRequest) {
            if (!w) throw "Cannot do synchronous binary XHRs outside webworkers in modern browsers. Use --embed-file or --preload-file in emcc";
            var a = new o;
            Object.defineProperties(a, {
              length: {
                get: function() {
                  return this.lengthKnown || this.cacheLength(), this._length
                }
              },
              chunkSize: {
                get: function() {
                  return this.lengthKnown || this.cacheLength(), this._chunkSize
                }
              }
            });
            var l = {
              isDevice: !1,
              contents: a
            }
          } else l = {
            isDevice: !1,
            url: i
          };
          var u = Xc.createFile(n, e, l, t, r);
          l.contents ? u.contents = l.contents : l.url && (u.contents = null, u.url = l.url), Object.defineProperties(u, {
            usedBytes: {
              get: function() {
                return this.contents.length
              }
            }
          });
          var f = {};
          return Object.keys(u.stream_ops).forEach((function(n) {
            var e = u.stream_ops[n];
            f[n] = function() {
              return Xc.forceLoadFile(u), e.apply(null, arguments)
            }
          })), f.read = function(n, e, i, t, r) {
            Xc.forceLoadFile(u);
            var o = n.node.contents;
            if (r >= o.length) return 0;
            var a = Math.min(o.length - r, t);
            if (o.slice)
              for (var l = 0; l < a; l++) e[i + l] = o[r + l];
            else
              for (l = 0; l < a; l++) e[i + l] = o.get(r + l);
            return a
          }, u.stream_ops = f, u
        },
        createPreloadedFile: function(e, i, t, r, o, a, l, u, f, c) {
          fd.init();
          var s = i ? Dc.resolve(bc.join2(e, i)) : e,
            d = "cp " + s;

          function m(t) {
            function m(n) {
              c && c(), u || Xc.createDataFile(e, i, n, r, o, f), a && a(), Tn(d)
            }
            var p = !1;
            n.preloadPlugins.forEach((function(n) {
              p || n.canHandle(s) && (n.handle(t, s, m, (function() {
                l && l(), Tn(d)
              })), p = !0)
            })), p || m(t)
          }
          jn(d), "string" == typeof t ? fd.asyncLoad(t, (function(n) {
            m(n)
          }), l) : m(t)
        },
        indexedDB: function() {
          return window.indexedDB || window.mozIndexedDB || window.webkitIndexedDB || window.msIndexedDB
        },
        DB_NAME: function() {
          return "EM_FS_" + window.location.pathname
        },
        DB_VERSION: 20,
        DB_STORE_NAME: "FILE_DATA",
        saveFilesToDB: function(n, e, i) {
          e = e || function() {}, i = i || function() {};
          var t = Xc.indexedDB();
          try {
            var r = t.open(Xc.DB_NAME(), Xc.DB_VERSION)
          } catch (n) {
            return i(n)
          }
          r.onupgradeneeded = function() {
            M("creating db"), r.result.createObjectStore(Xc.DB_STORE_NAME)
          }, r.onsuccess = function() {
            var t = r.result.transaction([Xc.DB_STORE_NAME], "readwrite"),
              o = t.objectStore(Xc.DB_STORE_NAME),
              a = 0,
              l = 0,
              u = n.length;

            function f() {
              0 == l ? e() : i()
            }
            n.forEach((function(n) {
              var e = o.put(Xc.analyzePath(n).object.contents, n);
              e.onsuccess = function() {
                ++a + l == u && f()
              }, e.onerror = function() {
                l++, a + l == u && f()
              }
            })), t.onerror = i
          }, r.onerror = i
        },
        loadFilesFromDB: function(n, e, i) {
          e = e || function() {}, i = i || function() {};
          var t = Xc.indexedDB();
          try {
            var r = t.open(Xc.DB_NAME(), Xc.DB_VERSION)
          } catch (n) {
            return i(n)
          }
          r.onupgradeneeded = i, r.onsuccess = function() {
            var t = r.result;
            try {
              var o = t.transaction([Xc.DB_STORE_NAME], "readonly")
            } catch (n) {
              return void i(n)
            }
            var a = o.objectStore(Xc.DB_STORE_NAME),
              l = 0,
              u = 0,
              f = n.length;

            function c() {
              0 == u ? e() : i()
            }
            n.forEach((function(n) {
              var e = a.get(n);
              e.onsuccess = function() {
                Xc.analyzePath(n).exists && Xc.unlink(n), Xc.createDataFile(bc.dirname(n), bc.basename(n), e.result, !0, !0, !0), ++l + u == f && c()
              }, e.onerror = function() {
                u++, l + u == f && c()
              }
            })), o.onerror = i
          }, r.onerror = i
        }
      },
      jc = {
        mappings: {},
        DEFAULT_POLLMASK: 5,
        umask: 511,
        calculateAt: function(n, e, i) {
          if ("/" === e[0]) return e;
          var t;
          if (-100 === n) t = Xc.cwd();
          else {
            var r = Xc.getStream(n);
            if (!r) throw new Xc.ErrnoError(8);
            t = r.path
          }
          if (0 == e.length) {
            if (!i) throw new Xc.ErrnoError(44);
            return t
          }
          return bc.join2(t, e)
        },
        doStat: function(n, e, i) {
          try {
            var t = n(e)
          } catch (n) {
            if (n && n.node && bc.normalize(e) !== bc.normalize(Xc.getPath(n.node))) return -54;
            throw n
          }
          return Z[i >> 2] = t.dev, Z[i + 4 >> 2] = 0, Z[i + 8 >> 2] = t.ino, Z[i + 12 >> 2] = t.mode, Z[i + 16 >> 2] = t.nlink, Z[i + 20 >> 2] = t.uid, Z[i + 24 >> 2] = t.gid, Z[i + 28 >> 2] = t.rdev, Z[i + 32 >> 2] = 0, Bn = [t.size >>> 0, (Rn = t.size, +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[i + 40 >> 2] = Bn[0], Z[i + 44 >> 2] = Bn[1], Z[i + 48 >> 2] = 4096, Z[i + 52 >> 2] = t.blocks, Z[i + 56 >> 2] = t.atime.getTime() / 1e3 | 0, Z[i + 60 >> 2] = 0, Z[i + 64 >> 2] = t.mtime.getTime() / 1e3 | 0, Z[i + 68 >> 2] = 0, Z[i + 72 >> 2] = t.ctime.getTime() / 1e3 | 0, Z[i + 76 >> 2] = 0, Bn = [t.ino >>> 0, (Rn = t.ino, +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[i + 80 >> 2] = Bn[0], Z[i + 84 >> 2] = Bn[1], 0
        },
        doMsync: function(n, e, i, t, r) {
          var o = V.slice(n, n + i);
          Xc.msync(e, o, r, i, t)
        },
        doMkdir: function(n, e) {
          return "/" === (n = bc.normalize(n))[n.length - 1] && (n = n.substr(0, n.length - 1)), Xc.mkdir(n, e, 0), 0
        },
        doMknod: function(n, e, i) {
          switch (61440 & e) {
            case 32768:
            case 8192:
            case 24576:
            case 4096:
            case 49152:
              break;
            default:
              return -28
          }
          return Xc.mknod(n, e, i), 0
        },
        doReadlink: function(n, e, i) {
          if (i <= 0) return -28;
          var t = Xc.readlink(n),
            r = Math.min(i, ln(t)),
            o = H[e + r];
          return an(t, e, i + 1), H[e + r] = o, r
        },
        doAccess: function(n, e) {
          if (-8 & e) return -28;
          var i;
          if (!(i = Xc.lookupPath(n, {
              follow: !0
            }).node)) return -44;
          var t = "";
          return 4 & e && (t += "r"), 2 & e && (t += "w"), 1 & e && (t += "x"), t && Xc.nodePermissions(i, t) ? -2 : 0
        },
        doDup: function(n, e, i) {
          var t = Xc.getStream(i);
          return t && Xc.close(t), Xc.open(n, e, 0, i, i).fd
        },
        doReadv: function(n, e, i, t) {
          for (var r = 0, o = 0; o < i; o++) {
            var a = Z[e + 8 * o >> 2],
              l = Z[e + (8 * o + 4) >> 2],
              u = Xc.read(n, H, a, l, t);
            if (u < 0) return -1;
            if (r += u, u < l) break
          }
          return r
        },
        doWritev: function(n, e, i, t) {
          for (var r = 0, o = 0; o < i; o++) {
            var a = Z[e + 8 * o >> 2],
              l = Z[e + (8 * o + 4) >> 2],
              u = Xc.write(n, H, a, l, t);
            if (u < 0) return -1;
            r += u
          }
          return r
        },
        varargs: void 0,
        get: function() {
          return jc.varargs += 4, Z[jc.varargs - 4 >> 2]
        },
        getStr: function(n) {
          var e = rn(n);
          return void 0 !== ie && ie.isWXAssetBundle(e) ? ie.url2path(e) : e
        },
        getStreamFromFD: function(n) {
          if (n > Xc.MAX_OPEN_FDS) {
            if (null == (e = ie.fd2wxStream.get(n))) throw new Xc.ErrnoError(8);
            return e
          }
          var e;
          if (!(e = Xc.getStream(n))) throw new Xc.ErrnoError(8);
          return e
        },
        get64: function(n, e) {
          return n
        }
      };

    function Tc(n, e, i, t, o) {
      try {
        for (var a = 0, l = e ? Z[e >> 2] : 0, u = e ? Z[e + 4 >> 2] : 0, f = i ? Z[i >> 2] : 0, c = i ? Z[i + 4 >> 2] : 0, s = t ? Z[t >> 2] : 0, d = t ? Z[t + 4 >> 2] : 0, m = 0, p = 0, y = 0, v = 0, _ = 0, g = 0, h = (e ? Z[e >> 2] : 0) | (i ? Z[i >> 2] : 0) | (t ? Z[t >> 2] : 0), w = (e ? Z[e + 4 >> 2] : 0) | (i ? Z[i + 4 >> 2] : 0) | (t ? Z[t + 4 >> 2] : 0), S = function(n, e, i, t) {
            return n < 32 ? e & t : i & t
          }, C = 0; C < n; C++) {
          var E = 1 << C % 32;
          if (S(C, h, w, E)) {
            var b = Xc.getStream(C);
            if (!b) throw new Xc.ErrnoError(8);
            var W = jc.DEFAULT_POLLMASK;
            b.stream_ops.poll && (W = b.stream_ops.poll(b)), 1 & W && S(C, l, u, E) && (C < 32 ? m |= E : p |= E, a++), 4 & W && S(C, f, c, E) && (C < 32 ? y |= E : v |= E, a++), 2 & W && S(C, s, d, E) && (C < 32 ? _ |= E : g |= E, a++)
          }
        }
        return e && (Z[e >> 2] = m, Z[e + 4 >> 2] = p), i && (Z[i >> 2] = y, Z[i + 4 >> 2] = v), t && (Z[t >> 2] = _, Z[t + 4 >> 2] = g), a
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }
    var Lc = {
        EPERM: 63,
        ENOENT: 44,
        ESRCH: 71,
        EINTR: 27,
        EIO: 29,
        ENXIO: 60,
        E2BIG: 1,
        ENOEXEC: 45,
        EBADF: 8,
        ECHILD: 12,
        EAGAIN: 6,
        EWOULDBLOCK: 6,
        ENOMEM: 48,
        EACCES: 2,
        EFAULT: 21,
        ENOTBLK: 105,
        EBUSY: 10,
        EEXIST: 20,
        EXDEV: 75,
        ENODEV: 43,
        ENOTDIR: 54,
        EISDIR: 31,
        EINVAL: 28,
        ENFILE: 41,
        EMFILE: 33,
        ENOTTY: 59,
        ETXTBSY: 74,
        EFBIG: 22,
        ENOSPC: 51,
        ESPIPE: 70,
        EROFS: 69,
        EMLINK: 34,
        EPIPE: 64,
        EDOM: 18,
        ERANGE: 68,
        ENOMSG: 49,
        EIDRM: 24,
        ECHRNG: 106,
        EL2NSYNC: 156,
        EL3HLT: 107,
        EL3RST: 108,
        ELNRNG: 109,
        EUNATCH: 110,
        ENOCSI: 111,
        EL2HLT: 112,
        EDEADLK: 16,
        ENOLCK: 46,
        EBADE: 113,
        EBADR: 114,
        EXFULL: 115,
        ENOANO: 104,
        EBADRQC: 103,
        EBADSLT: 102,
        EDEADLOCK: 16,
        EBFONT: 101,
        ENOSTR: 100,
        ENODATA: 116,
        ETIME: 117,
        ENOSR: 118,
        ENONET: 119,
        ENOPKG: 120,
        EREMOTE: 121,
        ENOLINK: 47,
        EADV: 122,
        ESRMNT: 123,
        ECOMM: 124,
        EPROTO: 65,
        EMULTIHOP: 36,
        EDOTDOT: 125,
        EBADMSG: 9,
        ENOTUNIQ: 126,
        EBADFD: 127,
        EREMCHG: 128,
        ELIBACC: 129,
        ELIBBAD: 130,
        ELIBSCN: 131,
        ELIBMAX: 132,
        ELIBEXEC: 133,
        ENOSYS: 52,
        ENOTEMPTY: 55,
        ENAMETOOLONG: 37,
        ELOOP: 32,
        EOPNOTSUPP: 138,
        EPFNOSUPPORT: 139,
        ECONNRESET: 15,
        ENOBUFS: 42,
        EAFNOSUPPORT: 5,
        EPROTOTYPE: 67,
        ENOTSOCK: 57,
        ENOPROTOOPT: 50,
        ESHUTDOWN: 140,
        ECONNREFUSED: 14,
        EADDRINUSE: 3,
        ECONNABORTED: 13,
        ENETUNREACH: 40,
        ENETDOWN: 38,
        ETIMEDOUT: 73,
        EHOSTDOWN: 142,
        EHOSTUNREACH: 23,
        EINPROGRESS: 26,
        EALREADY: 7,
        EDESTADDRREQ: 17,
        EMSGSIZE: 35,
        EPROTONOSUPPORT: 66,
        ESOCKTNOSUPPORT: 137,
        EADDRNOTAVAIL: 4,
        ENETRESET: 39,
        EISCONN: 30,
        ENOTCONN: 53,
        ETOOMANYREFS: 141,
        EUSERS: 136,
        EDQUOT: 19,
        ESTALE: 72,
        ENOTSUP: 138,
        ENOMEDIUM: 148,
        EILSEQ: 25,
        EOVERFLOW: 61,
        ECANCELED: 11,
        ENOTRECOVERABLE: 56,
        EOWNERDEAD: 62,
        ESTRPIPE: 135
      },
      Fc = {
        mount: function(e) {
          return n.websocket = n.websocket && "object" == typeof n.websocket ? n.websocket : {}, n.websocket._callbacks = {}, n.websocket.on = function(n, e) {
            return "function" == typeof e && (this._callbacks[n] = e), this
          }, n.websocket.emit = function(n, e) {
            "function" == typeof this._callbacks[n] && this._callbacks[n].call(this, e)
          }, Xc.createNode(null, "/", 16895, 0)
        },
        createSocket: function(n, e, i) {
          e &= -526337, i && K(1 == e == (6 == i));
          var t = {
              family: n,
              type: e,
              protocol: i,
              server: null,
              error: null,
              peers: {},
              pending: [],
              recv_queue: [],
              sock_ops: Fc.websocket_sock_ops
            },
            r = Fc.nextname(),
            o = Xc.createNode(Fc.root, r, 49152, 0);
          o.sock = t;
          var a = Xc.createStream({
            path: r,
            node: o,
            flags: 2,
            seekable: !1,
            stream_ops: Fc.stream_ops
          });
          return t.stream = a, t
        },
        getSocket: function(n) {
          var e = Xc.getStream(n);
          return e && Xc.isSocket(e.node.mode) ? e.node.sock : null
        },
        stream_ops: {
          poll: function(n) {
            var e = n.node.sock;
            return e.sock_ops.poll(e)
          },
          ioctl: function(n, e, i) {
            var t = n.node.sock;
            return t.sock_ops.ioctl(t, e, i)
          },
          read: function(n, e, i, t, r) {
            var o = n.node.sock,
              a = o.sock_ops.recvmsg(o, t);
            return a ? (e.set(a.buffer, i), a.buffer.length) : 0
          },
          write: function(n, e, i, t, r) {
            var o = n.node.sock;
            return o.sock_ops.sendmsg(o, e, i, t)
          },
          close: function(n) {
            var e = n.node.sock;
            e.sock_ops.close(e)
          }
        },
        nextname: function() {
          return Fc.nextname.current || (Fc.nextname.current = 0), "socket[" + Fc.nextname.current++ + "]"
        },
        websocket_sock_ops: {
          createPeer: function(e, i, t) {
            var r;
            if ("object" == typeof i && (r = i, i = null, t = null), r)
              if (r._socket) i = r._socket.remoteAddress, t = r._socket.remotePort;
              else {
                var o = /ws[s]?:\/\/([^:]+):(\d+)/.exec(r.url);
                if (!o) throw new Error("WebSocket URL must be in the format ws(s)://address:port");
                i = o[1], t = parseInt(o[2], 10)
              }
            else try {
              var a = n.websocket && "object" == typeof n.websocket,
                l = "ws:#".replace("#", "//");
              if (a && "string" == typeof n.websocket.url && (l = n.websocket.url), "ws://" === l || "wss://" === l) {
                var u = i.split("/");
                l = l + u[0] + ":" + t + "/" + u.slice(1).join("/")
              }
              var f = "binary";
              a && "string" == typeof n.websocket.subprotocol && (f = n.websocket.subprotocol);
              var c = void 0;
              "null" !== f && (f = f.replace(/^ +| +$/g, "").split(/ *, */), c = S ? {
                protocol: f.toString()
              } : f), a && null === n.websocket.subprotocol && (f = "null", c = void 0), (r = new(S ? require("ws") : WebSocket)(l, c)).binaryType = "arraybuffer"
            } catch (n) {
              throw new Xc.ErrnoError(Lc.EHOSTUNREACH)
            }
            var s = {
              addr: i,
              port: t,
              socket: r,
              dgram_send_queue: []
            };
            return Fc.websocket_sock_ops.addPeer(e, s), Fc.websocket_sock_ops.handlePeerEvents(e, s), 2 === e.type && void 0 !== e.sport && s.dgram_send_queue.push(new Uint8Array([255, 255, 255, 255, "p".charCodeAt(0), "o".charCodeAt(0), "r".charCodeAt(0), "t".charCodeAt(0), (65280 & e.sport) >> 8, 255 & e.sport])), s
          },
          getPeer: function(n, e, i) {
            return n.peers[e + ":" + i]
          },
          addPeer: function(n, e) {
            n.peers[e.addr + ":" + e.port] = e
          },
          removePeer: function(n, e) {
            delete n.peers[e.addr + ":" + e.port]
          },
          handlePeerEvents: function(e, i) {
            var t = !0,
              r = function() {
                n.websocket.emit("open", e.stream.fd);
                try {
                  for (var t = i.dgram_send_queue.shift(); t;) i.socket.send(t), t = i.dgram_send_queue.shift()
                } catch (n) {
                  i.socket.close()
                }
              };

            function o(r) {
              if ("string" == typeof r) {
                r = (new TextEncoder).encode(r)
              } else {
                if (K(void 0 !== r.byteLength), 0 == r.byteLength) return;
                r = new Uint8Array(r)
              }
              var o = t;
              if (t = !1, o && 10 === r.length && 255 === r[0] && 255 === r[1] && 255 === r[2] && 255 === r[3] && r[4] === "p".charCodeAt(0) && r[5] === "o".charCodeAt(0) && r[6] === "r".charCodeAt(0) && r[7] === "t".charCodeAt(0)) {
                var a = r[8] << 8 | r[9];
                return Fc.websocket_sock_ops.removePeer(e, i), i.port = a, void Fc.websocket_sock_ops.addPeer(e, i)
              }
              e.recv_queue.push({
                addr: i.addr,
                port: i.port,
                data: r
              }), n.websocket.emit("message", e.stream.fd)
            }
            S ? (i.socket.on("open", r), i.socket.on("message", (function(n, e) {
              e.binary && o(new Uint8Array(n).buffer)
            })), i.socket.on("close", (function() {
              n.websocket.emit("close", e.stream.fd)
            })), i.socket.on("error", (function(i) {
              e.error = Lc.ECONNREFUSED, n.websocket.emit("error", [e.stream.fd, e.error, "ECONNREFUSED: Connection refused"])
            }))) : (i.socket.onopen = r, i.socket.onclose = function() {
              n.websocket.emit("close", e.stream.fd)
            }, i.socket.onmessage = function(n) {
              o(n.data)
            }, i.socket.onerror = function(i) {
              e.error = Lc.ECONNREFUSED, n.websocket.emit("error", [e.stream.fd, e.error, "ECONNREFUSED: Connection refused"])
            })
          },
          poll: function(n) {
            if (1 === n.type && n.server) return n.pending.length ? 65 : 0;
            var e = 0,
              i = 1 === n.type ? Fc.websocket_sock_ops.getPeer(n, n.daddr, n.dport) : null;
            return (n.recv_queue.length || !i || i && i.socket.readyState === i.socket.CLOSING || i && i.socket.readyState === i.socket.CLOSED) && (e |= 65), (!i || i && i.socket.readyState === i.socket.OPEN) && (e |= 4), (i && i.socket.readyState === i.socket.CLOSING || i && i.socket.readyState === i.socket.CLOSED) && (e |= 16), e
          },
          ioctl: function(n, e, i) {
            switch (e) {
              case 21531:
                var t = 0;
                return n.recv_queue.length && (t = n.recv_queue[0].data.length), Z[i >> 2] = t, 0;
              default:
                return Lc.EINVAL
            }
          },
          close: function(n) {
            if (n.server) {
              try {
                n.server.close()
              } catch (n) {}
              n.server = null
            }
            for (var e = Object.keys(n.peers), i = 0; i < e.length; i++) {
              var t = n.peers[e[i]];
              try {
                t.socket.close()
              } catch (n) {}
              Fc.websocket_sock_ops.removePeer(n, t)
            }
            return 0
          },
          bind: function(n, e, i) {
            if (void 0 !== n.saddr || void 0 !== n.sport) throw new Xc.ErrnoError(Lc.EINVAL);
            if (n.saddr = e, n.sport = i, 2 === n.type) {
              n.server && (n.server.close(), n.server = null);
              try {
                n.sock_ops.listen(n, 0)
              } catch (n) {
                if (!(n instanceof Xc.ErrnoError)) throw n;
                if (n.errno !== Lc.EOPNOTSUPP) throw n
              }
            }
          },
          connect: function(n, e, i) {
            if (n.server) throw new Xc.ErrnoError(Lc.EOPNOTSUPP);
            if (void 0 !== n.daddr && void 0 !== n.dport) {
              var t = Fc.websocket_sock_ops.getPeer(n, n.daddr, n.dport);
              if (t) throw t.socket.readyState === t.socket.CONNECTING ? new Xc.ErrnoError(Lc.EALREADY) : new Xc.ErrnoError(Lc.EISCONN)
            }
            var r = Fc.websocket_sock_ops.createPeer(n, e, i);
            throw n.daddr = r.addr, n.dport = r.port, new Xc.ErrnoError(Lc.EINPROGRESS)
          },
          listen: function(e, i) {
            if (!S) throw new Xc.ErrnoError(Lc.EOPNOTSUPP);
            if (e.server) throw new Xc.ErrnoError(Lc.EINVAL);
            var t = require("ws").Server,
              r = e.saddr;
            e.server = new t({
              host: r,
              port: e.sport
            }), n.websocket.emit("listen", e.stream.fd), e.server.on("connection", (function(i) {
              if (1 === e.type) {
                var t = Fc.createSocket(e.family, e.type, e.protocol),
                  r = Fc.websocket_sock_ops.createPeer(t, i);
                t.daddr = r.addr, t.dport = r.port, e.pending.push(t), n.websocket.emit("connection", t.stream.fd)
              } else Fc.websocket_sock_ops.createPeer(e, i), n.websocket.emit("connection", e.stream.fd)
            })), e.server.on("closed", (function() {
              n.websocket.emit("close", e.stream.fd), e.server = null
            })), e.server.on("error", (function(i) {
              e.error = Lc.EHOSTUNREACH, n.websocket.emit("error", [e.stream.fd, e.error, "EHOSTUNREACH: Host is unreachable"])
            }))
          },
          accept: function(n) {
            if (!n.server) throw new Xc.ErrnoError(Lc.EINVAL);
            var e = n.pending.shift();
            return e.stream.flags = n.stream.flags, e
          },
          getname: function(n, e) {
            var i, t;
            if (e) {
              if (void 0 === n.daddr || void 0 === n.dport) throw new Xc.ErrnoError(Lc.ENOTCONN);
              i = n.daddr, t = n.dport
            } else i = n.saddr || 0, t = n.sport || 0;
            return {
              addr: i,
              port: t
            }
          },
          sendmsg: function(n, e, i, t, r, o) {
            if (2 === n.type) {
              if (void 0 !== r && void 0 !== o || (r = n.daddr, o = n.dport), void 0 === r || void 0 === o) throw new Xc.ErrnoError(Lc.EDESTADDRREQ)
            } else r = n.daddr, o = n.dport;
            var a, l = Fc.websocket_sock_ops.getPeer(n, r, o);
            if (1 === n.type) {
              if (!l || l.socket.readyState === l.socket.CLOSING || l.socket.readyState === l.socket.CLOSED) throw new Xc.ErrnoError(Lc.ENOTCONN);
              if (l.socket.readyState === l.socket.CONNECTING) throw new Xc.ErrnoError(Lc.EAGAIN)
            }
            if (ArrayBuffer.isView(e) && (i += e.byteOffset, e = e.buffer), a = e.slice(i, i + t), 2 === n.type && (!l || l.socket.readyState !== l.socket.OPEN)) return l && l.socket.readyState !== l.socket.CLOSING && l.socket.readyState !== l.socket.CLOSED || (l = Fc.websocket_sock_ops.createPeer(n, r, o)), l.dgram_send_queue.push(a), t;
            try {
              return l.socket.send(a), t
            } catch (n) {
              throw new Xc.ErrnoError(Lc.EINVAL)
            }
          },
          recvmsg: function(n, e) {
            if (1 === n.type && n.server) throw new Xc.ErrnoError(Lc.ENOTCONN);
            var i = n.recv_queue.shift();
            if (!i) {
              if (1 === n.type) {
                var t = Fc.websocket_sock_ops.getPeer(n, n.daddr, n.dport);
                if (t) {
                  if (t.socket.readyState === t.socket.CLOSING || t.socket.readyState === t.socket.CLOSED) return null;
                  throw new Xc.ErrnoError(Lc.EAGAIN)
                }
                throw new Xc.ErrnoError(Lc.ENOTCONN)
              }
              throw new Xc.ErrnoError(Lc.EAGAIN)
            }
            var r = i.data.byteLength || i.data.length,
              o = i.data.byteOffset || 0,
              a = i.data.buffer || i.data,
              l = Math.min(e, r),
              u = {
                buffer: new Uint8Array(a, o, l),
                addr: i.addr,
                port: i.port
              };
            if (1 === n.type && l < r) {
              var f = r - l;
              i.data = new Uint8Array(a, o + l, f), n.recv_queue.unshift(i)
            }
            return u
          }
        }
      };

    function Pc(n) {
      var e = Fc.getSocket(n);
      if (!e) throw new Xc.ErrnoError(8);
      return e
    }

    function Rc(n) {
      return Z[Sg() >> 2] = n, n
    }

    function Bc(n) {
      for (var e = n.split("."), i = 0; i < 4; i++) {
        var t = Number(e[i]);
        if (isNaN(t)) return null;
        e[i] = t
      }
      return (e[0] | e[1] << 8 | e[2] << 16 | e[3] << 24) >>> 0
    }

    function Gc(n) {
      return parseInt(n)
    }

    function Oc(n) {
      var e, i, t, r, o = [];
      if (!/^((?=.*::)(?!.*::.+::)(::)?([\dA-F]{1,4}:(:|\b)|){5}|([\dA-F]{1,4}:){6})((([\dA-F]{1,4}((?!\3)::|:\b|$))|(?!\2\3)){2}|(((2[0-4]|1\d|[1-9])?\d|25[0-5])\.?\b){4})$/i.test(n)) return null;
      if ("::" === n) return [0, 0, 0, 0, 0, 0, 0, 0];
      for ((n = n.startsWith("::") ? n.replace("::", "Z:") : n.replace("::", ":Z:")).indexOf(".") > 0 ? ((e = (n = n.replace(new RegExp("[.]", "g"), ":")).split(":"))[e.length - 4] = Gc(e[e.length - 4]) + 256 * Gc(e[e.length - 3]), e[e.length - 3] = Gc(e[e.length - 2]) + 256 * Gc(e[e.length - 1]), e = e.slice(0, e.length - 2)) : e = n.split(":"), t = 0, r = 0, i = 0; i < e.length; i++)
        if ("string" == typeof e[i])
          if ("Z" === e[i]) {
            for (r = 0; r < 8 - e.length + 1; r++) o[i + r] = 0;
            t = r - 1
          } else o[i + t] = Eg(parseInt(e[i], 16));
      else o[i + t] = e[i];
      return [o[1] << 16 | o[0], o[3] << 16 | o[2], o[5] << 16 | o[4], o[7] << 16 | o[6]]
    }

    function Ic(n, e, i, t, r) {
      switch (e) {
        case 2:
          i = Bc(i), r && (Z[r >> 2] = 16), Y[n >> 1] = e, Z[n + 4 >> 2] = i, Y[n + 2 >> 1] = Eg(t), Bn = [0, (Rn = 0, +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[n + 8 >> 2] = Bn[0], Z[n + 12 >> 2] = Bn[1];
          break;
        case 10:
          i = Oc(i), r && (Z[r >> 2] = 28), Z[n >> 2] = e, Z[n + 8 >> 2] = i[0], Z[n + 12 >> 2] = i[1], Z[n + 16 >> 2] = i[2], Z[n + 20 >> 2] = i[3], Y[n + 2 >> 1] = Eg(t), Z[n + 4 >> 2] = 0, Z[n + 24 >> 2] = 0;
          break;
        default:
          return 5
      }
      return 0
    }
    var Kc = {
      address_map: {
        id: 1,
        addrs: {},
        names: {}
      },
      lookup_name: function(n) {
        var e, i = Bc(n);
        if (null !== i) return n;
        if (null !== (i = Oc(n))) return n;
        if (Kc.address_map.addrs[n]) e = Kc.address_map.addrs[n];
        else {
          var t = Kc.address_map.id++;
          K(t < 65535, "exceeded max address mappings of 65535"), e = "172.29." + (255 & t) + "." + (65280 & t), Kc.address_map.names[e] = n, Kc.address_map.addrs[n] = e
        }
        return e
      },
      lookup_addr: function(n) {
        return Kc.address_map.names[n] ? Kc.address_map.names[n] : null
      }
    };

    function Nc(n, e, i, t) {
      try {
        var o = Pc(n),
          a = o.sock_ops.accept(o);
        if (e) Ic(e, a.family, Kc.lookup_name(a.daddr), a.dport, i);
        return a.stream.fd
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Uc(n, e) {
      try {
        return n = jc.getStr(n), void 0 !== ie && ie.isWXAssetBundle(n) ? ie.path2fd.has(n) ? 0 : ie.doWXAccess(n, e) : jc.doAccess(n, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function zc(n) {
      return (255 & n) + "." + (n >> 8 & 255) + "." + (n >> 16 & 255) + "." + (n >> 24 & 255)
    }

    function qc(n) {
      var e = "",
        i = 0,
        t = 0,
        r = 0,
        o = 0,
        a = 0,
        l = 0,
        u = [65535 & n[0], n[0] >> 16, 65535 & n[1], n[1] >> 16, 65535 & n[2], n[2] >> 16, 65535 & n[3], n[3] >> 16],
        f = !0,
        c = "";
      for (l = 0; l < 5; l++)
        if (0 !== u[l]) {
          f = !1;
          break
        } if (f) {
        if (c = zc(u[6] | u[7] << 16), -1 === u[5]) return e = "::ffff:", e += c;
        if (0 === u[5]) return e = "::", "0.0.0.0" === c && (c = ""), "0.0.0.1" === c && (c = "1"), e += c
      }
      for (i = 0; i < 8; i++) 0 === u[i] && (i - r > 1 && (a = 0), r = i, a++), a > t && (o = i - (t = a) + 1);
      for (i = 0; i < 8; i++) t > 1 && 0 === u[i] && i >= o && i < o + t ? i === o && (e += ":", 0 === o && (e += ":")) : (e += Number(bg(65535 & u[i])).toString(16), e += i < 7 ? ":" : "");
      return e
    }

    function Hc(n, e) {
      var i, t = Y[n >> 1],
        r = bg(J[n + 2 >> 1]);
      switch (t) {
        case 2:
          if (16 !== e) return {
            errno: 28
          };
          i = zc(i = Z[n + 4 >> 2]);
          break;
        case 10:
          if (28 !== e) return {
            errno: 28
          };
          i = qc(i = [Z[n + 8 >> 2], Z[n + 12 >> 2], Z[n + 16 >> 2], Z[n + 20 >> 2]]);
          break;
        default:
          return {
            errno: 5
          }
      }
      return {
        family: t,
        addr: i,
        port: r
      }
    }

    function Vc(n, e, i) {
      if (i && 0 === n) return null;
      var t = Hc(n, e);
      if (t.errno) throw new Xc.ErrnoError(t.errno);
      return t.addr = Kc.lookup_addr(t.addr) || t.addr, t
    }

    function Yc(n, e, i) {
      try {
        var t = Pc(n),
          o = Vc(e, i);
        return t.sock_ops.bind(t, o.addr, o.port), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Jc(n, e) {
      try {
        return n = jc.getStr(n), Xc.chmod(n, e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Zc(n, e, i) {
      try {
        var t = Pc(n),
          o = Vc(e, i);
        return t.sock_ops.connect(t, o.addr, o.port), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Qc(n, e) {
      try {
        var i = jc.getStreamFromFD(n);
        return i.fd === e ? e : jc.doDup(i.path, i.flags, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function $c(n, e) {
      try {
        return Xc.fchmod(n, e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ns(n, e, i) {
      jc.varargs = i;
      try {
        var t = jc.getStreamFromFD(n);
        switch (e) {
          case 0:
            return (o = jc.get()) < 0 ? -28 : Xc.open(t.path, t.flags, 0, o).fd;
          case 1:
          case 2:
            return 0;
          case 3:
            return t.flags;
          case 4:
            var o = jc.get();
            return t.flags |= o, 0;
          case 12:
            o = jc.get();
            return Y[o + 0 >> 1] = 2, 0;
          case 13:
          case 14:
            return 0;
          case 16:
          case 8:
            return -28;
          case 9:
            return Rc(28), -1;
          default:
            return -28
        }
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function es(n, e) {
      try {
        var i = jc.getStreamFromFD(n);
        return n > Xc.MAX_OPEN_FDS ? jc.doStat(ie.wxstat, i.path, e) : jc.doStat(Xc.stat, i.path, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function is(n, e, i, t) {
      try {
        var o = jc.get64(i, t);
        return Xc.ftruncate(n, o), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ts(n, e) {
      try {
        if (0 === e) return -28;
        var i = Xc.cwd();
        return e < ln(i) + 1 ? -68 : (an(i, n, e), n)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function rs(n, e, i) {
      try {
        var t = jc.getStreamFromFD(n);
        t.getdents || (t.getdents = Xc.readdir(t.path));
        for (var o = 0, a = Xc.llseek(t, 0, 1), l = Math.floor(a / 280); l < t.getdents.length && o + 280 <= i;) {
          var u, f, c = t.getdents[l];
          if ("." === c[0]) u = 1, f = 4;
          else {
            var s = Xc.lookupNode(t.node, c);
            u = s.id, f = Xc.isChrdev(s.mode) ? 2 : Xc.isDir(s.mode) ? 4 : Xc.isLink(s.mode) ? 10 : 8
          }
          Bn = [u >>> 0, (Rn = u, +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[e + o >> 2] = Bn[0], Z[e + o + 4 >> 2] = Bn[1], Bn = [280 * (l + 1) >>> 0, (Rn = 280 * (l + 1), +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[e + o + 8 >> 2] = Bn[0], Z[e + o + 12 >> 2] = Bn[1], Y[e + o + 16 >> 1] = 280, H[e + o + 18 >> 0] = f, an(c, e + o + 19, 256), o += 280, l += 1
        }
        return Xc.llseek(t, 280 * l, 0), o
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function os() {
      return 0
    }

    function as() {
      return 0
    }

    function ls(n, e, i) {
      try {
        var t = Pc(n);
        if (!t.daddr) return -53;
        Ic(e, t.family, Kc.lookup_name(t.daddr), t.dport, i);
        return 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function us(n, e) {
      try {
        return Bg(e, 0, 136), Z[e >> 2] = 1, Z[e + 4 >> 2] = 2, Z[e + 8 >> 2] = 3, Z[e + 12 >> 2] = 4, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function fs(n, e, i) {
      try {
        x("__sys_getsockname " + n);
        var t = Pc(n);
        Ic(e, t.family, Kc.lookup_name(t.saddr || "0.0.0.0"), t.sport, i);
        return 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function cs(n, e, i, t, o) {
      try {
        var a = Pc(n);
        return 1 === e && 4 === i ? (Z[t >> 2] = a.error, Z[o >> 2] = 4, a.error = null, 0) : -50
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ss() {
      return 0
    }

    function ds(n, e, i) {
      jc.varargs = i;
      try {
        var t = jc.getStreamFromFD(n);
        switch (e) {
          case 21509:
          case 21505:
            return t.tty ? 0 : -59;
          case 21510:
          case 21511:
          case 21512:
          case 21506:
          case 21507:
          case 21508:
            return t.tty ? 0 : -59;
          case 21519:
            if (!t.tty) return -59;
            var o = jc.get();
            return Z[o >> 2] = 0, 0;
          case 21520:
            return t.tty ? -28 : -59;
          case 21531:
            o = jc.get();
            return Xc.ioctl(t, e, o);
          case 21523:
          case 21524:
            return t.tty ? 0 : -59;
          default:
            r("bad ioctl syscall " + e)
        }
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ms(n, e) {
      return -34
    }

    function ps(n, e) {
      try {
        var i = Pc(n);
        return i.sock_ops.listen(i, e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ys(n, e) {
      try {
        return n = jc.getStr(n), jc.doStat(Xc.lstat, n, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function vs(n, e) {
      try {
        return n = jc.getStr(n), jc.doMkdir(n, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function _s(n, e, i, t, r, o) {
      var a;
      o <<= 12;
      var l = !1;
      if (0 != (16 & t) && n % 65536 != 0) return -28;
      if (0 != (32 & t)) {
        if (!(a = Pg(65536, e))) return -48;
        Bg(a, 0, e), l = !0
      } else {
        var u = Xc.getStream(r);
        if (!u) return -8;
        var f = Xc.mmap(u, n, e, o, i, t);
        a = f.ptr, l = f.allocated
      }
      return jc.mappings[a] = {
        malloc: a,
        len: e,
        allocated: l,
        fd: r,
        prot: i,
        flags: t,
        offset: o
      }, a
    }

    function gs(n, e, i, t, o, a) {
      try {
        return _s(n, e, i, t, o, a)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function hs(n, e) {
      if (-1 == (0 | n) || 0 === e) return -28;
      var i = jc.mappings[n];
      if (!i) return 0;
      if (e === i.len) {
        var t = Xc.getStream(i.fd);
        t && (2 & i.prot && jc.doMsync(n, t, e, i.flags, i.offset), Xc.munmap(t)), jc.mappings[n] = null, i.allocated && Fg(i.malloc)
      }
      return 0
    }

    function ws(n, e) {
      try {
        return hs(n, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ss(n, e, i) {
      jc.varargs = i;
      try {
        var t = jc.getStr(n);
        if (void 0 !== ie && ie.isWXAssetBundle(t)) {
          var o = ie.path2fd.get(t);
          if (void 0 !== o) return o;
          const n = ie.LoadBundleFromFile(t);
          let i = {
            fd: o = ie.newfd(),
            path: t,
            flags: e,
            seekable: !0,
            position: 0,
            stream_ops: Mc.stream_ops,
            ungotten: [],
            node: {
              mode: 32768,
              usedBytes: new Uint8Array(n).length
            },
            error: !1
          };
          return i.stream_ops.read = ie.read, ie.path2fd.set(t, o), ie.fd2wxStream.set(o, i), ie.cache.put(o, n), o
        }
        var a = i ? jc.get() : 0;
        return Xc.open(t, e, a).fd
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }
    var Cs = {
      BUCKET_BUFFER_SIZE: 8192,
      mount: function(n) {
        return Xc.createNode(null, "/", 16895, 0)
      },
      createPipe: function() {
        var n = {
          buckets: []
        };
        n.buckets.push({
          buffer: new Uint8Array(Cs.BUCKET_BUFFER_SIZE),
          offset: 0,
          roffset: 0
        });
        var e = Cs.nextname(),
          i = Cs.nextname(),
          t = Xc.createNode(Cs.root, e, 4096, 0),
          r = Xc.createNode(Cs.root, i, 4096, 0);
        t.pipe = n, r.pipe = n;
        var o = Xc.createStream({
          path: e,
          node: t,
          flags: 0,
          seekable: !1,
          stream_ops: Cs.stream_ops
        });
        t.stream = o;
        var a = Xc.createStream({
          path: i,
          node: r,
          flags: 1,
          seekable: !1,
          stream_ops: Cs.stream_ops
        });
        return r.stream = a, {
          readable_fd: o.fd,
          writable_fd: a.fd
        }
      },
      stream_ops: {
        poll: function(n) {
          var e = n.node.pipe;
          if (1 == (2097155 & n.flags)) return 260;
          if (e.buckets.length > 0)
            for (var i = 0; i < e.buckets.length; i++) {
              var t = e.buckets[i];
              if (t.offset - t.roffset > 0) return 65
            }
          return 0
        },
        ioctl: function(n, e, i) {
          return Lc.EINVAL
        },
        fsync: function(n) {
          return Lc.EINVAL
        },
        read: function(n, e, i, t, r) {
          for (var o = n.node.pipe, a = 0, l = 0; l < o.buckets.length; l++) {
            var u = o.buckets[l];
            a += u.offset - u.roffset
          }
          K(e instanceof ArrayBuffer || ArrayBuffer.isView(e));
          var f = e.subarray(i, i + t);
          if (t <= 0) return 0;
          if (0 == a) throw new Xc.ErrnoError(Lc.EAGAIN);
          var c = Math.min(a, t),
            s = c,
            d = 0;
          for (l = 0; l < o.buckets.length; l++) {
            var m = o.buckets[l],
              p = m.offset - m.roffset;
            if (c <= p) {
              var y = m.buffer.subarray(m.roffset, m.offset);
              c < p ? (y = y.subarray(0, c), m.roffset += c) : d++, f.set(y);
              break
            }
            y = m.buffer.subarray(m.roffset, m.offset);
            f.set(y), f = f.subarray(y.byteLength), c -= y.byteLength, d++
          }
          return d && d == o.buckets.length && (d--, o.buckets[d].offset = 0, o.buckets[d].roffset = 0), o.buckets.splice(0, d), s
        },
        write: function(n, e, i, t, r) {
          var o = n.node.pipe;
          K(e instanceof ArrayBuffer || ArrayBuffer.isView(e));
          var a = e.subarray(i, i + t),
            l = a.byteLength;
          if (l <= 0) return 0;
          var u = null;
          0 == o.buckets.length ? (u = {
            buffer: new Uint8Array(Cs.BUCKET_BUFFER_SIZE),
            offset: 0,
            roffset: 0
          }, o.buckets.push(u)) : u = o.buckets[o.buckets.length - 1], K(u.offset <= Cs.BUCKET_BUFFER_SIZE);
          var f = Cs.BUCKET_BUFFER_SIZE - u.offset;
          if (f >= l) return u.buffer.set(a, u.offset), u.offset += l, l;
          f > 0 && (u.buffer.set(a.subarray(0, f), u.offset), u.offset += f, a = a.subarray(f, a.byteLength));
          for (var c = a.byteLength / Cs.BUCKET_BUFFER_SIZE | 0, s = a.byteLength % Cs.BUCKET_BUFFER_SIZE, d = 0; d < c; d++) {
            var m = {
              buffer: new Uint8Array(Cs.BUCKET_BUFFER_SIZE),
              offset: Cs.BUCKET_BUFFER_SIZE,
              roffset: 0
            };
            o.buckets.push(m), m.buffer.set(a.subarray(0, Cs.BUCKET_BUFFER_SIZE)), a = a.subarray(Cs.BUCKET_BUFFER_SIZE, a.byteLength)
          }
          if (s > 0) {
            m = {
              buffer: new Uint8Array(Cs.BUCKET_BUFFER_SIZE),
              offset: a.byteLength,
              roffset: 0
            };
            o.buckets.push(m), m.buffer.set(a)
          }
          return l
        },
        close: function(n) {
          n.node.pipe.buckets = null
        }
      },
      nextname: function() {
        return Cs.nextname.current || (Cs.nextname.current = 0), "pipe[" + Cs.nextname.current++ + "]"
      }
    };

    function Es(n) {
      try {
        if (0 == n) throw new Xc.ErrnoError(21);
        var e = Cs.createPipe();
        return Z[n >> 2] = e.readable_fd, Z[n + 4 >> 2] = e.writable_fd, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function bs(n, e, i) {
      try {
        for (var t = 0, o = 0; o < e; o++) {
          var a = n + 8 * o,
            l = Z[a >> 2],
            u = Y[a + 4 >> 1],
            f = 32,
            c = Xc.getStream(l);
          c && (f = jc.DEFAULT_POLLMASK, c.stream_ops.poll && (f = c.stream_ops.poll(c))), (f &= 24 | u) && t++, Y[a + 6 >> 1] = f
        }
        return t
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ws(n, e, i) {
      try {
        return n = jc.getStr(n), jc.doReadlink(n, e, i)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ds(n, e, i, t, o, a) {
      try {
        var l = Pc(n),
          u = l.sock_ops.recvmsg(l, i);
        if (!u) return 0;
        if (o) Ic(o, l.family, Kc.lookup_name(u.addr), u.port, a);
        return V.set(u.buffer, e), u.buffer.byteLength
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function As(n, e, i) {
      try {
        for (var t = Pc(n), o = Z[e + 8 >> 2], a = Z[e + 12 >> 2], l = 0, u = 0; u < a; u++) l += Z[o + (8 * u + 4) >> 2];
        var f = t.sock_ops.recvmsg(t, l);
        if (!f) return 0;
        var c = Z[e >> 2];
        if (c) Ic(c, t.family, Kc.lookup_name(f.addr), f.port);
        var s = 0,
          d = f.buffer.byteLength;
        for (u = 0; d > 0 && u < a; u++) {
          var m = Z[o + (8 * u + 0) >> 2],
            p = Z[o + (8 * u + 4) >> 2];
          if (p) {
            var y = Math.min(p, d),
              v = f.buffer.subarray(s, s + y);
            V.set(v, m + s), s += y, d -= y
          }
        }
        return s
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function ks(n, e) {
      try {
        return n = jc.getStr(n), e = jc.getStr(e), Xc.rename(n, e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ms(n) {
      try {
        return n = jc.getStr(n), Xc.rmdir(n), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function xs(n, e, i) {
      try {
        var t, o, a = Pc(n),
          l = Z[e + 8 >> 2],
          u = Z[e + 12 >> 2],
          f = Z[e >> 2],
          c = Z[e + 4 >> 2];
        if (f) {
          var s = Hc(f, c);
          if (s.errno) return -s.errno;
          o = s.port, t = Kc.lookup_addr(s.addr) || s.addr
        }
        for (var d = 0, m = 0; m < u; m++) d += Z[l + (8 * m + 4) >> 2];
        var p = new Uint8Array(d),
          y = 0;
        for (m = 0; m < u; m++)
          for (var v = Z[l + (8 * m + 0) >> 2], _ = Z[l + (8 * m + 4) >> 2], g = 0; g < _; g++) p[y++] = H[v + g >> 0];
        return a.sock_ops.sendmsg(a, p, 0, d, t, o)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Xs(n, e, i, t, o, a) {
      try {
        var l = Pc(n),
          u = Vc(o, a, !0);
        return u ? l.sock_ops.sendmsg(l, H, e, i, u.addr, u.port) : Xc.write(l.stream, H, e, i)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function js(n) {
      try {
        return -50
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ts(n, e) {
      try {
        return Pc(n), -52
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ls(n, e, i) {
      try {
        return Fc.createSocket(n, e, i).stream.fd
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Fs(n, e) {
      try {
        return n = jc.getStr(n), void 0 !== ie && ie.isWXAssetBundle(n) ? jc.doStat(ie.wxstat, n, e) : jc.doStat(Xc.stat, n, e)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ps(n, e, i) {
      try {
        return n = jc.getStr(n), Z[i + 4 >> 2] = 4096, Z[i + 40 >> 2] = 4096, Z[i + 8 >> 2] = 1e6, Z[i + 12 >> 2] = 5e5, Z[i + 16 >> 2] = 5e5, Z[i + 20 >> 2] = Xc.nextInode, Z[i + 24 >> 2] = 1e6, Z[i + 28 >> 2] = 42, Z[i + 44 >> 2] = 2, Z[i + 36 >> 2] = 255, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Rs(n, e) {
      try {
        return n = jc.getStr(n), e = jc.getStr(e), Xc.symlink(n, e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Bs(n, e, i, t) {
      try {
        n = jc.getStr(n);
        var o = jc.get64(i, t);
        return Xc.truncate(n, o), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Gs(n) {
      try {
        if (!n) return -21;
        var e = {
            __size__: 390,
            domainname: 325,
            machine: 260,
            nodename: 65,
            release: 130,
            sysname: 0,
            version: 195
          },
          i = function(i, t) {
            sn(t, n + e[i])
          };
        return i("sysname", "Emscripten"), i("nodename", "emscripten"), i("release", "1.0"), i("version", "#1"), i("machine", "wasm32"), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Os(n) {
      try {
        return n = jc.getStr(n), Xc.unlink(n), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Is(n, e, i, t) {
      try {
        e = jc.getStr(e), e = jc.calculateAt(n, e, !0);
        var o = Z[i >> 2],
          a = Z[i + 4 >> 2],
          l = 1e3 * o + a / 1e6,
          u = 1e3 * (o = Z[(i += 8) >> 2]) + (a = Z[i + 4 >> 2]) / 1e6;
        return Xc.utime(e, l, u), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), -n.errno
      }
    }

    function Ks() {
      r()
    }

    function Ns() {
      return void 0 === Ns.start && (Ns.start = Date.now()), 1e3 * (Date.now() - Ns.start) | 0
    }

    function Us() {
      return S ? 1 : 1e3
    }
    var zs, qs = !0;

    function Hs(n, e) {
      var i;
      if (0 === n) i = 1e6;
      else {
        if (1 !== n || !qs) return Rc(28), -1;
        i = Us()
      }
      return Z[e >> 2] = i / 1e9 | 0, Z[e + 4 >> 2] = i, 0
    }

    function Vs(n, e) {
      var i;
      if (0 === n) i = Date.now();
      else {
        if (1 !== n && 4 !== n || !qs) return Rc(28), -1;
        i = zs()
      }
      return Z[e >> 2] = i / 1e3 | 0, Z[e + 4 >> 2] = i % 1e3 * 1e3 * 1e3 | 0, 0
    }

    function Ys(n, e) {
      return n - e
    }

    function Js(n) {}

    function Zs() {
      return 0
    }

    function Qs(n, e) {}

    function $s(n, e) {
      return 0
    }
    zs = S ? function() {
      var n = process.hrtime();
      return 1e3 * n[0] + n[1] / 1e6
    } : "undefined" != typeof dateNow ? dateNow : function() {
      return performance.now()
    };
    var nd = [];

    function ed(n, e) {
      var i;
      for (nd.length = 0, e >>= 2; i = V[n++];) {
        var t = i < 105;
        t && 1 & e && e++, nd.push(t ? nn[e++ >> 1] : Z[e]), ++e
      }
      return nd
    }

    function id(n, e, i, t) {
      var r = ed(e, i);
      return Kn[n].apply(null, r)
    }

    function td(n, e, i) {
      return id(n, e, i)
    }

    function rd(e, i) {
      if (fd.mainLoop.timingMode = e, fd.mainLoop.timingValue = i, 0 == e ? (16 == i && (e = 1, i = 1), 33 == i && (e = 1, i = 2), 66 == i && (e = 1, i = 3)) : 2 == e && (e = 1, i = 1), !fd.mainLoop.func) return 1;
      if (fd.mainLoop.running || (fd.mainLoop.running = !0), 0 == e) fd.mainLoop.scheduler = function() {
        var n = 0 | Math.max(0, fd.mainLoop.tickStartTime + i - zs());
        setTimeout(fd.mainLoop.runner, n)
      }, fd.mainLoop.method = "timeout";
      else if (1 == e) fd.mainLoop.timingValue = 1, wx.setPreferredFramesPerSecond(60 / i), fd.mainLoop.scheduler = function() {
        fd.requestAnimationFrame(fd.mainLoop.runner)
      }, fd.mainLoop.method = "rAF";
      else if (2 == e) {
        if ("undefined" == typeof setImmediate) {
          var t = [];
          addEventListener("message", (function(n) {
            "setimmediate" !== n.data && "setimmediate" !== n.data.target || (n.stopPropagation(), t.shift()())
          }), !0), setImmediate = function(e) {
            t.push(e), w ? (void 0 === n.setImmediates && (n.setImmediates = []), n.setImmediates.push(e), postMessage({
              target: "setimmediate"
            })) : postMessage("setimmediate", "*")
          }
        }
        fd.mainLoop.scheduler = function() {
          setImmediate(fd.mainLoop.runner)
        }, fd.mainLoop.method = "immediate"
      }
      return 0
    }

    function od(n) {
      sE(n)
    }

    function ad() {
      if (!Yn()) try {
        od(O)
      } catch (n) {
        if (n instanceof uE) return;
        throw n
      }
    }

    function ld(n, e, i, t, r) {
      K(!fd.mainLoop.func, "emscripten_set_main_loop: there can only be one main loop function at once: call emscripten_cancel_main_loop to cancel the previous one before setting a new one with different parameters."), fd.mainLoop.func = n, fd.mainLoop.arg = t;
      var o = fd.mainLoop.currentlyRunningMainloop;

      function a() {
        return !(o < fd.mainLoop.currentlyRunningMainloop) || (ad(), !1)
      }
      if (fd.mainLoop.running = !1, fd.mainLoop.runner = function() {
          if (!I)
            if (fd.mainLoop.queue.length > 0) {
              var e = Date.now(),
                i = fd.mainLoop.queue.shift();
              if (i.func(i.arg), fd.mainLoop.remainingBlockers) {
                var t = fd.mainLoop.remainingBlockers,
                  r = t % 1 == 0 ? t - 1 : Math.floor(t);
                i.counted ? fd.mainLoop.remainingBlockers = r : (r += .5, fd.mainLoop.remainingBlockers = (8 * t + r) / 9)
              }
              if (console.log('main loop blocker "' + i.name + '" took ' + (Date.now() - e) + " ms"), fd.mainLoop.updateStatus(), !a()) return;
              setTimeout(fd.mainLoop.runner, 0)
            } else if (a())
            if (fd.mainLoop.currentFrameNumber = fd.mainLoop.currentFrameNumber + 1 | 0, 1 == fd.mainLoop.timingMode && fd.mainLoop.timingValue > 1 && fd.mainLoop.currentFrameNumber % fd.mainLoop.timingValue != 0) fd.mainLoop.scheduler();
            else {
              if (0 == fd.mainLoop.timingMode && (fd.mainLoop.tickStartTime = zs()), Bm.newRenderingFrameStarted(), GameGlobal.manager.isVisible) {
                let e;
                if (("function" == typeof GameGlobal.manager.getGameDataMonitor && GameGlobal.manager.getGameDataMonitor().isRunning() || void 0 !== GameGlobal.calcFrameTimeFunc) && (e = performance.now()), fd.mainLoop.runIter(n), void 0 !== e) {
                  const n = performance.now();
                  void 0 !== GameGlobal.calcFrameTimeFunc && GameGlobal.calcFrameTimeFunc(e, n), "function" == typeof GameGlobal.manager.getGameDataMonitor && GameGlobal.manager.getGameDataMonitor().isRunning() && m("WXSDKManagerHandler", "OnFrameInterval")
                }
              }
              a() && ("object" == typeof SDL && SDL.audio && SDL.audio.queueNewAudioData && SDL.audio.queueNewAudioData(), fd.mainLoop.scheduler())
            }
        }, !r) {
        if (e && e > 0 ? rd(0, 1e3 / e) : rd(1, 1), !GameGlobal.unityNamespace.isLoopRunnerEnable) return;
        fd.mainLoop.scheduler()
      }
      if (i) throw "unwind"
    }

    function ud(n, e) {
      if (!I)
        if (e) n();
        else try {
          n()
        } catch (n) {
          if (n instanceof uE) return;
          if ("unwind" !== n) throw n && "object" == typeof n && n.stack && x("exception thrown: " + [n, n.stack]), n
        }
    }
    var fd = GameGlobal.unityNamespace.Browser = {
      mainLoop: {
        running: !1,
        scheduler: null,
        method: "",
        currentlyRunningMainloop: 0,
        func: null,
        arg: 0,
        timingMode: 0,
        timingValue: 0,
        currentFrameNumber: 0,
        queue: [],
        pause: function() {
          fd.mainLoop.scheduler = null, fd.mainLoop.currentlyRunningMainloop++
        },
        resume: function() {
          fd.mainLoop.currentlyRunningMainloop++;
          var n = fd.mainLoop.timingMode,
            e = fd.mainLoop.timingValue,
            i = fd.mainLoop.func;
          fd.mainLoop.func = null, ld(i, 0, !1, fd.mainLoop.arg, !0), rd(n, e), fd.mainLoop.scheduler()
        },
        updateStatus: function() {
          if (n.setStatus) {
            var e = n.statusMessage || "Please wait...",
              i = fd.mainLoop.remainingBlockers,
              t = fd.mainLoop.expectedBlockers;
            i ? i < t ? n.setStatus(e + " (" + (t - i) + "/" + t + ")") : n.setStatus(e) : n.setStatus("")
          }
        },
        runIter: function(e) {
          if (!I) {
            if (n.preMainLoop)
              if (!1 === n.preMainLoop()) return;
            ud(e), n.postMainLoop && n.postMainLoop()
          }
        }
      },
      isFullscreen: !1,
      pointerLock: !1,
      moduleContextCreatedCallbacks: [],
      workers: [],
      init: function() {
        if (n.preloadPlugins || (n.preloadPlugins = []), !fd.initted) {
          fd.initted = !0;
          try {
            new Blob, fd.hasBlobConstructor = !0
          } catch (n) {
            fd.hasBlobConstructor = !1, console.log("warning: no blob constructor, cannot create blobs with mimetypes")
          }
          fd.BlobBuilder = "undefined" != typeof MozBlobBuilder ? MozBlobBuilder : "undefined" != typeof WebKitBlobBuilder ? WebKitBlobBuilder : fd.hasBlobConstructor ? null : console.log("warning: no BlobBuilder"), fd.URLObject = "undefined" != typeof window ? window.URL ? window.URL : window.webkitURL : void 0, n.noImageDecoding || void 0 !== fd.URLObject || (console.log("warning: Browser does not support creating object URLs. Built-in browser image decoding will not be available."), n.noImageDecoding = !0);
          var e = {
            canHandle: function(e) {
              return !n.noImageDecoding && /\.(jpg|jpeg|png|bmp)$/i.test(e)
            },
            handle: function(e, i, t, r) {
              var o = null;
              if (fd.hasBlobConstructor) try {
                (o = new Blob([e], {
                  type: fd.getMimetype(i)
                })).size !== e.length && (o = new Blob([new Uint8Array(e).buffer], {
                  type: fd.getMimetype(i)
                }))
              } catch (n) {
                T("Blob constructor present but fails: " + n + "; falling back to blob builder")
              }
              if (!o) {
                var a = new fd.BlobBuilder;
                a.append(new Uint8Array(e).buffer), o = a.getBlob()
              }
              var l = fd.URLObject.createObjectURL(o),
                u = new Image;
              u.onload = function() {
                K(u.complete, "Image " + i + " could not be decoded");
                var r = document.createElement("canvas");
                r.width = u.width, r.height = u.height, r.getContext("2d").drawImage(u, 0, 0), n.preloadedImages[i] = r, fd.URLObject.revokeObjectURL(l), t && t(e)
              }, u.onerror = function(n) {
                console.log("Image " + l + " could not be decoded"), r && r()
              }, u.src = l
            }
          };
          n.preloadPlugins.push(e);
          var i = {
            canHandle: function(e) {
              return !n.noAudioDecoding && e.substr(-4) in {
                ".ogg": 1,
                ".wav": 1,
                ".mp3": 1
              }
            },
            handle: function(e, i, t, r) {
              var o = !1;

              function a(r) {
                o || (o = !0, n.preloadedAudios[i] = r, t && t(e))
              }

              function l() {
                o || (o = !0, n.preloadedAudios[i] = new Audio, r && r())
              }
              if (!fd.hasBlobConstructor) return l();
              try {
                var u = new Blob([e], {
                  type: fd.getMimetype(i)
                })
              } catch (n) {
                return l()
              }
              var f = fd.URLObject.createObjectURL(u),
                c = new Audio;
              c.addEventListener("canplaythrough", (function() {
                a(c)
              }), !1), c.onerror = function(n) {
                o || (console.log("warning: browser could not fully decode audio " + i + ", trying slower base64 approach"), c.src = "data:audio/x-" + i.substr(-3) + ";base64," + function(n) {
                  for (var e = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/", i = "", t = 0, r = 0, o = 0; o < n.length; o++)
                    for (t = t << 8 | n[o], r += 8; r >= 6;) {
                      var a = t >> r - 6 & 63;
                      r -= 6, i += e[a]
                    }
                  return 2 == r ? (i += e[(3 & t) << 4], i += "==") : 4 == r && (i += e[(15 & t) << 2], i += "="), i
                }(e), a(c))
              }, c.src = f, fd.safeSetTimeout((function() {
                a(c)
              }), 1e4)
            }
          };
          n.preloadPlugins.push(i);
          var t = n.canvas;
          t && (t.requestPointerLock = t.requestPointerLock || t.mozRequestPointerLock || t.webkitRequestPointerLock || t.msRequestPointerLock || function() {}, t.exitPointerLock = document.exitPointerLock || document.mozExitPointerLock || document.webkitExitPointerLock || document.msExitPointerLock || function() {}, t.exitPointerLock = t.exitPointerLock.bind(document), document.addEventListener("pointerlockchange", r, !1), document.addEventListener("mozpointerlockchange", r, !1), document.addEventListener("webkitpointerlockchange", r, !1), document.addEventListener("mspointerlockchange", r, !1), n.elementPointerLock && t.addEventListener("click", (function(e) {
            !fd.pointerLock && n.canvas.requestPointerLock && (n.canvas.requestPointerLock(), e.preventDefault())
          }), !1))
        }

        function r() {
          fd.pointerLock = document.pointerLockElement === n.canvas || document.mozPointerLockElement === n.canvas || document.webkitPointerLockElement === n.canvas || document.msPointerLockElement === n.canvas
        }
      },
      createContext: function(e, i, t, r) {
        if (i && n.ctx && e == n.canvas) return n.ctx;
        var o, a;
        if (i) {
          var l = {
            antialias: !1,
            alpha: !1,
            majorVersion: "undefined" != typeof WebGL2RenderingContext ? 2 : 1
          };
          if (r)
            for (var u in r) l[u] = r[u];
          void 0 !== Bm && (a = Bm.createContext(e, l), WXWASMSDK.canvasContext && WXWASMSDK.canvasContext._triggerCallback(), a && (o = Bm.getContext(a).GLctx))
        } else o = e.getContext("2d");
        return o ? (t && (i || K(void 0 === lg, "cannot set in module if GLctx is used, but we are a non-GL context that would replace it"), n.ctx = o, i && Bm.makeContextCurrent(a), n.useWebGL = i, fd.moduleContextCreatedCallbacks.forEach((function(n) {
          n()
        })), fd.init()), o) : null
      },
      destroyContext: function(n, e, i) {},
      fullscreenHandlersInstalled: !1,
      lockPointer: void 0,
      resizeCanvas: void 0,
      requestFullscreen: function(e, i) {
        fd.lockPointer = e, fd.resizeCanvas = i, void 0 === fd.lockPointer && (fd.lockPointer = !0), void 0 === fd.resizeCanvas && (fd.resizeCanvas = !1);
        var t = n.canvas;

        function r() {
          fd.isFullscreen = !1;
          var e = t.parentNode;
          (document.fullscreenElement || document.mozFullScreenElement || document.msFullscreenElement || document.webkitFullscreenElement || document.webkitCurrentFullScreenElement) === e ? (t.exitFullscreen = fd.exitFullscreen, fd.lockPointer && t.requestPointerLock(), fd.isFullscreen = !0, fd.resizeCanvas ? fd.setFullscreenCanvasSize() : fd.updateCanvasDimensions(t)) : (e.parentNode.insertBefore(t, e), e.parentNode.removeChild(e), fd.resizeCanvas ? fd.setWindowedCanvasSize() : fd.updateCanvasDimensions(t)), n.onFullScreen && n.onFullScreen(fd.isFullscreen), n.onFullscreen && n.onFullscreen(fd.isFullscreen)
        }
        fd.fullscreenHandlersInstalled || (fd.fullscreenHandlersInstalled = !0, document.addEventListener("fullscreenchange", r, !1), document.addEventListener("mozfullscreenchange", r, !1), document.addEventListener("webkitfullscreenchange", r, !1), document.addEventListener("MSFullscreenChange", r, !1));
        var o = document.createElement("div");
        t.parentNode.insertBefore(o, t), o.appendChild(t), o.requestFullscreen = o.requestFullscreen || o.mozRequestFullScreen || o.msRequestFullscreen || (o.webkitRequestFullscreen ? function() {
          o.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT)
        } : null) || (o.webkitRequestFullScreen ? function() {
          o.webkitRequestFullScreen(Element.ALLOW_KEYBOARD_INPUT)
        } : null), o.requestFullscreen()
      },
      exitFullscreen: function() {
        return !!fd.isFullscreen && ((document.exitFullscreen || document.cancelFullScreen || document.mozCancelFullScreen || document.msExitFullscreen || document.webkitCancelFullScreen || function() {}).apply(document, []), !0)
      },
      nextRAF: 0,
      fakeRequestAnimationFrame: function(n) {
        var e = Date.now();
        if (0 === fd.nextRAF) fd.nextRAF = e + 1e3 / 60;
        else
          for (; e + 2 >= fd.nextRAF;) fd.nextRAF += 1e3 / 60;
        var i = Math.max(fd.nextRAF - e, 0);
        setTimeout(n, i)
      },
      requestAnimationFrame: function(n) {
        "function" != typeof requestAnimationFrame ? (0, fd.fakeRequestAnimationFrame)(n) : requestAnimationFrame(n)
      },
      safeRequestAnimationFrame: function(n) {
        return fd.requestAnimationFrame((function() {
          ud(n)
        }))
      },
      safeSetTimeout: function(n, e) {
        return setTimeout((function() {
          ud(n)
        }), e)
      },
      getMimetype: function(n) {
        return {
          jpg: "image/jpeg",
          jpeg: "image/jpeg",
          png: "image/png",
          bmp: "image/bmp",
          ogg: "audio/ogg",
          wav: "audio/wav",
          mp3: "audio/mpeg"
        } [n.substr(n.lastIndexOf(".") + 1)]
      },
      getUserMedia: function(n) {
        window.getUserMedia || (window.getUserMedia = navigator.getUserMedia || navigator.mozGetUserMedia), window.getUserMedia(n)
      },
      getMovementX: function(n) {
        return n.movementX || n.mozMovementX || n.webkitMovementX || 0
      },
      getMovementY: function(n) {
        return n.movementY || n.mozMovementY || n.webkitMovementY || 0
      },
      getMouseWheelDelta: function(n) {
        var e = 0;
        switch (n.type) {
          case "DOMMouseScroll":
            e = n.detail / 3;
            break;
          case "mousewheel":
            e = n.wheelDelta / 120;
            break;
          case "wheel":
            switch (e = n.deltaY, n.deltaMode) {
              case 0:
                e /= 100;
                break;
              case 1:
                e /= 3;
                break;
              case 2:
                e *= 80;
                break;
              default:
                throw "unrecognized mouse wheel delta mode: " + n.deltaMode
            }
            break;
          default:
            throw "unrecognized mouse wheel event: " + n.type
        }
        return e
      },
      mouseX: 0,
      mouseY: 0,
      mouseMovementX: 0,
      mouseMovementY: 0,
      touches: {},
      lastTouches: {},
      calculateMouseEvent: function(e) {
        if (fd.pointerLock) "mousemove" != e.type && "mozMovementX" in e ? fd.mouseMovementX = fd.mouseMovementY = 0 : (fd.mouseMovementX = fd.getMovementX(e), fd.mouseMovementY = fd.getMovementY(e)), "undefined" != typeof SDL ? (fd.mouseX = SDL.mouseX + fd.mouseMovementX, fd.mouseY = SDL.mouseY + fd.mouseMovementY) : (fd.mouseX += fd.mouseMovementX, fd.mouseY += fd.mouseMovementY);
        else {
          var i = n.canvas.getBoundingClientRect(),
            t = n.canvas.width,
            r = n.canvas.height,
            o = void 0 !== window.scrollX ? window.scrollX : window.pageXOffset,
            a = void 0 !== window.scrollY ? window.scrollY : window.pageYOffset;
          if ("touchstart" === e.type || "touchend" === e.type || "touchmove" === e.type) {
            var l = e.touch;
            if (void 0 === l) return;
            var u = l.pageX - (o + i.left),
              f = l.pageY - (a + i.top),
              c = {
                x: u *= t / i.width,
                y: f *= r / i.height
              };
            if ("touchstart" === e.type) fd.lastTouches[l.identifier] = c, fd.touches[l.identifier] = c;
            else if ("touchend" === e.type || "touchmove" === e.type) {
              var s = fd.touches[l.identifier];
              s || (s = c), fd.lastTouches[l.identifier] = s, fd.touches[l.identifier] = c
            }
            return
          }
          var d = e.pageX - (o + i.left),
            m = e.pageY - (a + i.top);
          d *= t / i.width, m *= r / i.height, fd.mouseMovementX = d - fd.mouseX, fd.mouseMovementY = m - fd.mouseY, fd.mouseX = d, fd.mouseY = m
        }
      },
      asyncLoad: function(n, e, i, t) {
        var r = t ? "" : "al " + n;
        b(n, (function(i) {
          K(i, 'Loading data file "' + n + '" failed (no arrayBuffer).'), e(new Uint8Array(i)), r && Tn(r)
        }), (function(e) {
          if (!i) throw 'Loading data file "' + n + '" failed.';
          i()
        })), r && jn(r)
      },
      resizeListeners: [],
      updateResizeListeners: function() {
        var e = n.canvas;
        fd.resizeListeners.forEach((function(n) {
          n(e.width, e.height)
        }))
      },
      setCanvasSize: function(e, i, t) {
        var r = n.canvas;
        fd.updateCanvasDimensions(r, e, i), t || fd.updateResizeListeners()
      },
      windowedWidth: 0,
      windowedHeight: 0,
      setFullscreenCanvasSize: function() {
        if ("undefined" != typeof SDL) {
          var e = Q[SDL.screen >> 2];
          e |= 8388608, Z[SDL.screen >> 2] = e
        }
        fd.updateCanvasDimensions(n.canvas), fd.updateResizeListeners()
      },
      setWindowedCanvasSize: function() {
        if ("undefined" != typeof SDL) {
          var e = Q[SDL.screen >> 2];
          e &= -8388609, Z[SDL.screen >> 2] = e
        }
        fd.updateCanvasDimensions(n.canvas), fd.updateResizeListeners()
      },
      updateCanvasDimensions: function(e, i, t) {
        i && t ? (e.widthNative = i, e.heightNative = t) : (i = e.widthNative, t = e.heightNative);
        var r = i,
          o = t;
        if (n.forcedAspectRatio && n.forcedAspectRatio > 0 && (r / o < n.forcedAspectRatio ? r = Math.round(o * n.forcedAspectRatio) : o = Math.round(r / n.forcedAspectRatio)), (document.fullscreenElement || document.mozFullScreenElement || document.msFullscreenElement || document.webkitFullscreenElement || document.webkitCurrentFullScreenElement) === e.parentNode && "undefined" != typeof screen) {
          var a = Math.min(screen.width / r, screen.height / o);
          r = Math.round(r * a), o = Math.round(o * a)
        }
        fd.resizeCanvas ? (e.width != r && (e.width = r), e.height != o && (e.height = o), void 0 !== e.style && (e.style.removeProperty && e.style.removeProperty("width"), e.style.removeProperty && e.style.removeProperty("height"))) : (e.width != i && (e.width = i), e.height != t && (e.height = t), void 0 !== e.style && (r != i || o != t ? (e.style.setProperty && e.style.setProperty("width", r + "px", "important"), e.style.setProperty && e.style.setProperty("height", o + "px", "important")) : (e.style.removeProperty && e.style.removeProperty("width"), e.style.removeProperty && e.style.removeProperty("height"))))
      },
      wgetRequests: {},
      nextWgetRequestHandle: 0,
      getNextWgetRequestHandle: function() {
        var n = fd.nextWgetRequestHandle;
        return fd.nextWgetRequestHandle++, n
      }
    };

    function cd() {
      fd.mainLoop.pause(), fd.mainLoop.func = null
    }

    function sd(n) {
      clearInterval(n)
    }
    var dd = {
        inEventHandler: 0,
        removeAllEventListeners: function() {
          for (var n = dd.eventHandlers.length - 1; n >= 0; --n) dd._removeHandler(n);
          dd.eventHandlers = [], dd.deferredCalls = []
        },
        registerRemoveEventListeners: function() {
          dd.removeEventListenersRegistered || (gn.push(dd.removeAllEventListeners), dd.removeEventListenersRegistered = !0)
        },
        deferredCalls: [],
        deferCall: function(n, e, i) {
          function t(n, e) {
            if (n.length != e.length) return !1;
            for (var i in n)
              if (n[i] != e[i]) return !1;
            return !0
          }
          for (var r in dd.deferredCalls) {
            var o = dd.deferredCalls[r];
            if (o.targetFunction == n && t(o.argsList, i)) return
          }
          dd.deferredCalls.push({
            targetFunction: n,
            precedence: e,
            argsList: i
          }), dd.deferredCalls.sort((function(n, e) {
            return n.precedence < e.precedence
          }))
        },
        removeDeferredCalls: function(n) {
          for (var e = 0; e < dd.deferredCalls.length; ++e) dd.deferredCalls[e].targetFunction == n && (dd.deferredCalls.splice(e, 1), --e)
        },
        canPerformEventHandlerRequests: function() {
          return dd.inEventHandler && dd.currentEventHandler.allowsDeferredCalls
        },
        runDeferredCalls: function() {
          if (dd.canPerformEventHandlerRequests())
            for (var n = 0; n < dd.deferredCalls.length; ++n) {
              var e = dd.deferredCalls[n];
              dd.deferredCalls.splice(n, 1), --n, e.targetFunction.apply(null, e.argsList)
            }
        },
        eventHandlers: [],
        removeAllHandlersOnTarget: function(n, e) {
          for (var i = 0; i < dd.eventHandlers.length; ++i) dd.eventHandlers[i].target != n || e && e != dd.eventHandlers[i].eventTypeString || dd._removeHandler(i--)
        },
        _removeHandler: function(n) {
          var e = dd.eventHandlers[n];
          e.target.removeEventListener(e.eventTypeString, e.eventListenerFunc, e.useCapture), dd.eventHandlers.splice(n, 1)
        },
        registerOrRemoveHandler: function(n) {
          var e = function(e) {
            ++dd.inEventHandler, dd.currentEventHandler = n, dd.runDeferredCalls(), n.handlerFunc(e), dd.runDeferredCalls(), --dd.inEventHandler
          };
          if (n.callbackfunc) n.eventListenerFunc = e, "blur" === n.eventTypeString ? wx.onHide(e) : "focus" === n.eventTypeString ? wx.onShow(e) : n.target.addEventListener(n.eventTypeString, e, n.useCapture), dd.eventHandlers.push(n), dd.registerRemoveEventListeners();
          else
            for (var i = 0; i < dd.eventHandlers.length; ++i) dd.eventHandlers[i].target == n.target && dd.eventHandlers[i].eventTypeString == n.eventTypeString && dd._removeHandler(i--)
        },
        getNodeNameForTarget: function(n) {
          return n ? n == window ? "#window" : n == screen ? "#screen" : n && n.nodeName ? n.nodeName : "" : ""
        },
        fullscreenEnabled: function() {
          return document.fullscreenEnabled || document.webkitFullscreenEnabled
        }
      },
      md = {},
      pd = [0, "undefined" != typeof document ? document : 0, "undefined" != typeof window ? window : 0];

    function yd(e) {
      return n.canvas
    }

    function vd(e) {
      return n.canvas
    }

    function _d(n, e, i) {
      var t = vd();
      if (!t) return -4;
      Z[e >> 2] = t.width, Z[i >> 2] = t.height
    }

    function gd(n) {
      var e = kg(),
        i = xg(8),
        t = i + 4,
        r = xg(n.id.length + 1);
      an(n.id, r, n.id.length + 1);
      _d(0, i, t);
      var o = [Z[i >> 2], Z[t >> 2]];
      return Mg(e), o
    }

    function hd(n, e, i) {
      var t = vd();
      return t ? (t.width = e, t.height = i, 0) : -4
    }

    function wd(n, e, i) {
      if (n.controlTransferredOffscreen) {
        var t = kg(),
          r = xg(n.id.length + 1);
        an(n.id, r, n.id.length + 1), hd(0, e, i), Mg(t)
      } else n.width = e, n.height = i
    }

    function Sd(n) {
      var e = gd(n),
        i = e[0],
        t = e[1],
        r = n.style.width,
        o = n.style.height,
        a = n.style.backgroundColor,
        l = document.body.style.backgroundColor,
        u = n.style.paddingLeft,
        f = n.style.paddingRight,
        c = n.style.paddingTop,
        s = n.style.paddingBottom,
        d = n.style.marginLeft,
        m = n.style.marginRight,
        p = n.style.marginTop,
        y = n.style.marginBottom,
        v = document.body.style.margin,
        _ = document.documentElement.style.overflow,
        g = document.body.scroll,
        h = n.style.imageRendering;

      function w() {
        var e;
        document.fullscreenElement || document.webkitFullscreenElement || document.msFullscreenElement || (document.removeEventListener("fullscreenchange", w), document.removeEventListener("webkitfullscreenchange", w), wd(n, i, t), n.style.width = r, n.style.height = o, n.style.backgroundColor = a, l || (document.body.style.backgroundColor = "white"), document.body.style.backgroundColor = l, n.style.paddingLeft = u, n.style.paddingRight = f, n.style.paddingTop = c, n.style.paddingBottom = s, n.style.marginLeft = d, n.style.marginRight = m, n.style.marginTop = p, n.style.marginBottom = y, document.body.style.margin = v, document.documentElement.style.overflow = _, document.body.scroll = g, n.style.imageRendering = h, n.GLctxObject && n.GLctxObject.GLctx.viewport(0, 0, i, t), md.canvasResizedCallback && (e = md.canvasResizedCallbackUserData, Ng.apply(null, [md.canvasResizedCallback, 37, 0, e])))
      }
      return document.addEventListener("fullscreenchange", w), document.addEventListener("webkitfullscreenchange", w), w
    }

    function Cd(n, e, i) {
      n.style.paddingLeft = n.style.paddingRight = i + "px", n.style.paddingTop = n.style.paddingBottom = e + "px"
    }

    function Ed(n) {
      return pd.indexOf(n) < 0 ? n.getBoundingClientRect() : {
        left: 0,
        top: 0
      }
    }

    function bd(n, e) {
      var i = Sd(n),
        t = e.softFullscreen ? innerWidth : screen.width,
        r = e.softFullscreen ? innerHeight : screen.height,
        o = Ed(n),
        a = o.width,
        l = o.height,
        u = gd(n),
        f = u[0],
        c = u[1];
      if (3 == e.scaleMode) Cd(n, (r - l) / 2, (t - a) / 2), t = a, r = l;
      else if (2 == e.scaleMode)
        if (t * c < f * r) {
          var s = c * t / f;
          Cd(n, (r - s) / 2, 0), r = s
        } else {
          var d = f * r / c;
          Cd(n, 0, (t - d) / 2), t = d
        } n.style.backgroundColor || (n.style.backgroundColor = "black"), document.body.style.backgroundColor || (document.body.style.backgroundColor = "black"), n.style.width = t + "px", n.style.height = r + "px", 1 == e.filteringMode && (n.style.imageRendering = "optimizeSpeed", n.style.imageRendering = "-moz-crisp-edges", n.style.imageRendering = "-o-crisp-edges", n.style.imageRendering = "-webkit-optimize-contrast", n.style.imageRendering = "optimize-contrast", n.style.imageRendering = "crisp-edges", n.style.imageRendering = "pixelated");
      var m = 2 == e.canvasResolutionScaleMode ? devicePixelRatio : 1;
      if (0 != e.canvasResolutionScaleMode) {
        var p = t * m | 0,
          y = r * m | 0;
        wd(n, p, y), n.GLctxObject && n.GLctxObject.GLctx.viewport(0, 0, p, y)
      }
      return i
    }

    function Wd(n, e) {
      if (0 == e.scaleMode && 0 == e.canvasResolutionScaleMode || bd(n, e), n.requestFullscreen) n.requestFullscreen();
      else {
        if (!n.webkitRequestFullscreen) return dd.fullscreenEnabled() ? -3 : -1;
        n.webkitRequestFullscreen(Element.ALLOW_KEYBOARD_INPUT)
      }
      var i;
      return md = e, e.canvasResizedCallback && (i = e.canvasResizedCallbackUserData, Ng.apply(null, [e.canvasResizedCallback, 37, 0, i])), 0
    }

    function Dd() {
      if (!dd.fullscreenEnabled()) return -1;
      dd.removeDeferredCalls(Wd);
      var n = pd[1];
      if (n.exitFullscreen) n.fullscreenElement && n.exitFullscreen();
      else {
        if (!n.webkitExitFullscreen) return -1;
        n.webkitFullscreenElement && n.webkitExitFullscreen()
      }
      return 0
    }

    function Ad(n) {
      if (n.requestPointerLock) n.requestPointerLock();
      else {
        if (!n.msRequestPointerLock) return document.body.requestPointerLock || document.body.msRequestPointerLock ? -3 : -1;
        n.msRequestPointerLock()
      }
      return 0
    }

    function kd() {
      if (dd.removeDeferredCalls(Ad), document.exitPointerLock) document.exitPointerLock();
      else {
        if (!document.msExitPointerLock) return -1;
        document.msExitPointerLock()
      }
      return 0
    }

    function Md(n) {
      var e = document.fullscreenElement || document.mozFullScreenElement || document.webkitFullscreenElement || document.msFullscreenElement,
        i = !!e;
      Z[n >> 2] = i, Z[n + 4 >> 2] = dd.fullscreenEnabled();
      var t = i ? e : dd.previousFullscreenElement,
        r = dd.getNodeNameForTarget(t),
        o = t && t.id ? t.id : "";
      an(r, n + 8, 128), an(o, n + 136, 128), Z[n + 264 >> 2] = t ? t.clientWidth : 0, Z[n + 268 >> 2] = t ? t.clientHeight : 0, Z[n + 272 >> 2] = screen.width, Z[n + 276 >> 2] = screen.height, i && (dd.previousFullscreenElement = e)
    }

    function xd(n) {
      return dd.fullscreenEnabled() ? (Md(n), 0) : -1
    }

    function Xd(n, e) {
      nn[n >> 3] = e.timestamp;
      for (var i = 0; i < e.axes.length; ++i) nn[n + 8 * i + 16 >> 3] = e.axes[i];
      for (i = 0; i < e.buttons.length; ++i) "object" == typeof e.buttons[i] ? nn[n + 8 * i + 528 >> 3] = e.buttons[i].value : nn[n + 8 * i + 528 >> 3] = e.buttons[i];
      for (i = 0; i < e.buttons.length; ++i) "object" == typeof e.buttons[i] ? Z[n + 4 * i + 1040 >> 2] = e.buttons[i].pressed : Z[n + 4 * i + 1040 >> 2] = 1 == e.buttons[i];
      Z[n + 1296 >> 2] = e.connected, Z[n + 1300 >> 2] = e.index, Z[n + 8 >> 2] = e.axes.length, Z[n + 12 >> 2] = e.buttons.length, an(e.id, n + 1304, 64), an(e.mapping, n + 1368, 64)
    }

    function jd(n, e) {
      return n < 0 || n >= dd.lastGamepadState.length ? -5 : dd.lastGamepadState[n] ? (Xd(e, dd.lastGamepadState[n]), 0) : -7
    }

    function Td() {
      return 2147483648
    }

    function Ld() {
      return n.IsWxGame || n.IsWxGame ? 0 : dd.lastGamepadState.length
    }

    function Fd() {
      dd.removeAllEventListeners()
    }

    function Pd(n) {
      return !Bm.contexts[n] || Bm.contexts[n].GLctx.isContextLost()
    }

    function Rd(n) {
      return n < 0 || 0 === n && 1 / n == -1 / 0
    }

    function Bd(n, e) {
      return (n >>> 0) + 4294967296 * e
    }

    function Gd(n, e) {
      return (n >>> 0) + 4294967296 * (e >>> 0)
    }

    function Od(n, e) {
      if (n <= 0) return n;
      var i = e <= 32 ? Math.abs(1 << e - 1) : Math.pow(2, e - 1);
      return n >= i && (e <= 32 || n > i) && (n = -2 * i + n), n
    }

    function Id(n, e) {
      return n >= 0 ? n : e <= 32 ? 2 * Math.abs(1 << e - 1) + n : Math.pow(2, e) + n
    }

    function Kd(n, e) {
      var i = n,
        t = e;

      function r(n) {
        var e;
        return t = function(n, e) {
          return "double" !== e && "i64" !== e || 7 & n && (n += 4), n
        }(t, n), "double" === n ? (e = nn[t >> 3], t += 8) : "i64" == n ? (e = [Z[t >> 2], Z[t + 4 >> 2]], t += 8) : (n = "i32", e = Z[t >> 2], t += 4), e
      }
      for (var o, a, l, u = [];;) {
        var f = i;
        if (0 === (o = H[i >> 0])) break;
        if (a = H[i + 1 >> 0], 37 == o) {
          var c = !1,
            s = !1,
            d = !1,
            m = !1,
            p = !1;
          n: for (;;) {
            switch (a) {
              case 43:
                c = !0;
                break;
              case 45:
                s = !0;
                break;
              case 35:
                d = !0;
                break;
              case 48:
                if (m) break n;
                m = !0;
                break;
              case 32:
                p = !0;
                break;
              default:
                break n
            }
            i++, a = H[i + 1 >> 0]
          }
          var y = 0;
          if (42 == a) y = r("i32"), i++, a = H[i + 1 >> 0];
          else
            for (; a >= 48 && a <= 57;) y = 10 * y + (a - 48), i++, a = H[i + 1 >> 0];
          var v, _ = !1,
            g = -1;
          if (46 == a) {
            if (g = 0, _ = !0, i++, 42 == (a = H[i + 1 >> 0])) g = r("i32"), i++;
            else
              for (;;) {
                var h = H[i + 1 >> 0];
                if (h < 48 || h > 57) break;
                g = 10 * g + (h - 48), i++
              }
            a = H[i + 1 >> 0]
          }
          switch (g < 0 && (g = 6, _ = !1), String.fromCharCode(a)) {
            case "h":
              104 == H[i + 2 >> 0] ? (i++, v = 1) : v = 2;
              break;
            case "l":
              108 == H[i + 2 >> 0] ? (i++, v = 8) : v = 4;
              break;
            case "L":
            case "q":
            case "j":
              v = 8;
              break;
            case "z":
            case "t":
            case "I":
              v = 4;
              break;
            default:
              v = null
          }
          switch (v && i++, a = H[i + 1 >> 0], String.fromCharCode(a)) {
            case "d":
            case "i":
            case "u":
            case "o":
            case "x":
            case "X":
            case "p":
              var w = 100 == a || 105 == a;
              if (l = r("i" + 8 * (v = v || 4)), 8 == v && (l = 117 == a ? Gd(l[0], l[1]) : Bd(l[0], l[1])), v <= 4) l = (w ? Od : Id)(l & Math.pow(256, v) - 1, 8 * v);
              var S = Math.abs(l),
                C = "";
              if (100 == a || 105 == a) W = Od(l, 8 * v).toString(10);
              else if (117 == a) W = Id(l, 8 * v).toString(10), l = Math.abs(l);
              else if (111 == a) W = (d ? "0" : "") + S.toString(8);
              else if (120 == a || 88 == a) {
                if (C = d && 0 != l ? "0x" : "", l < 0) {
                  l = -l, W = (S - 1).toString(16);
                  for (var E = [], b = 0; b < W.length; b++) E.push((15 - parseInt(W[b], 16)).toString(16));
                  for (W = E.join(""); W.length < 2 * v;) W = "f" + W
                } else W = S.toString(16);
                88 == a && (C = C.toUpperCase(), W = W.toUpperCase())
              } else 112 == a && (0 === S ? W = "(nil)" : (C = "0x", W = S.toString(16)));
              if (_)
                for (; W.length < g;) W = "0" + W;
              for (l >= 0 && (c ? C = "+" + C : p && (C = " " + C)), "-" == W.charAt(0) && (C = "-" + C, W = W.substr(1)); C.length + W.length < y;) s ? W += " " : m ? W = "0" + W : C = " " + C;
              (W = C + W).split("").forEach((function(n) {
                u.push(n.charCodeAt(0))
              }));
              break;
            case "f":
            case "F":
            case "e":
            case "E":
            case "g":
            case "G":
              var W;
              if (l = r("double"), isNaN(l)) W = "nan", m = !1;
              else if (isFinite(l)) {
                var D = !1,
                  A = Math.min(g, 20);
                if (103 == a || 71 == a) {
                  D = !0, g = g || 1;
                  var k = parseInt(l.toExponential(A).split("e")[1], 10);
                  g > k && k >= -4 ? (a = (103 == a ? "f" : "F").charCodeAt(0), g -= k + 1) : (a = (103 == a ? "e" : "E").charCodeAt(0), g--), A = Math.min(g, 20)
                }
                101 == a || 69 == a ? (W = l.toExponential(A), /[eE][-+]\d$/.test(W) && (W = W.slice(0, -1) + "0" + W.slice(-1))) : 102 != a && 70 != a || (W = l.toFixed(A), 0 === l && Rd(l) && (W = "-" + W));
                var M = W.split("e");
                if (D && !d)
                  for (; M[0].length > 1 && M[0].includes(".") && ("0" == M[0].slice(-1) || "." == M[0].slice(-1));) M[0] = M[0].slice(0, -1);
                else
                  for (d && -1 == W.indexOf(".") && (M[0] += "."); g > A++;) M[0] += "0";
                W = M[0] + (M.length > 1 ? "e" + M[1] : ""), 69 == a && (W = W.toUpperCase()), l >= 0 && (c ? W = "+" + W : p && (W = " " + W))
              } else W = (l < 0 ? "-" : "") + "inf", m = !1;
              for (; W.length < y;) s ? W += " " : W = !m || "-" != W[0] && "+" != W[0] ? (m ? "0" : " ") + W : W[0] + "0" + W.slice(1);
              a < 97 && (W = W.toUpperCase()), W.split("").forEach((function(n) {
                u.push(n.charCodeAt(0))
              }));
              break;
            case "s":
              var x = r("i8*"),
                X = x ? Gg(x) : "(null)".length;
              if (_ && (X = Math.min(X, g)), !s)
                for (; X < y--;) u.push(32);
              if (x)
                for (b = 0; b < X; b++) u.push(V[x++ >> 0]);
              else u = u.concat(pg("(null)".substr(0, X), !0));
              if (s)
                for (; X < y--;) u.push(32);
              break;
            case "c":
              for (s && u.push(r("i8")); --y > 0;) u.push(32);
              s || u.push(r("i8"));
              break;
            case "n":
              var j = r("i32*");
              Z[j >> 2] = u.length;
              break;
            case "%":
              u.push(o);
              break;
            default:
              for (b = f; b < i + 2; b++) u.push(H[b >> 0])
          }
          i += 2
        } else u.push(o), i += 1
      }
      return u
    }

    function Nd(n) {
      if (!n || !n.callee || !n.callee.name) return [null, "", ""];
      n.callee.toString();
      var e = n.callee.name,
        i = "(",
        t = !0;
      for (var r in n) {
        var o = n[r];
        t || (i += ", "), t = !1, i += "number" == typeof o || "string" == typeof o ? o : "(" + typeof o + ")"
      }
      i += ")";
      var a = n.callee.caller;
      return t && (i = ""), [n = a ? a.arguments : [], e, i]
    }

    function Ud(n) {
      var e = Hn(),
        i = e.lastIndexOf("_emscripten_log"),
        t = e.lastIndexOf("_emscripten_get_callstack"),
        r = e.indexOf("\n", Math.max(i, t)) + 1;
      e = e.slice(r), 32 & n && T("EM_LOG_DEMANGLE is deprecated; ignoring"), 8 & n && "undefined" == typeof emscripten_source_map && (T('Source map information is not available, emscripten_log with EM_LOG_C_STACK will be ignored. Build with "--pre-js $EMSCRIPTEN/src/emscripten-source-map.min.js" linker flag to add source map loading to code.'), n ^= 8, n |= 16);
      var o = null;
      if (128 & n)
        for (o = Nd(arguments); o[1].includes("_emscripten_");) o = Nd(o[0]);
      var a = e.split("\n");
      e = "";
      var l = new RegExp("\\s*(.*?)@(.*?):([0-9]+):([0-9]+)"),
        u = new RegExp("\\s*(.*?)@(.*):(.*)(:(.*))?"),
        f = new RegExp("\\s*at (.*?) \\((.*):(.*):(.*)\\)");
      for (var c in a) {
        var s = a[c],
          d = "",
          m = "",
          p = 0,
          y = 0,
          v = f.exec(s);
        if (v && 5 == v.length) d = v[1], m = v[2], p = v[3], y = v[4];
        else {
          if ((v = l.exec(s)) || (v = u.exec(s)), !(v && v.length >= 4)) {
            e += s + "\n";
            continue
          }
          d = v[1], m = v[2], p = v[3], y = 0 | v[4]
        }
        var _ = !1;
        if (8 & n) {
          var g = emscripten_source_map.originalPositionFor({
            line: p,
            column: y
          });
          (_ = g && g.source) && (64 & n && (g.source = g.source.substring(g.source.replace(/\\/g, "/").lastIndexOf("/") + 1)), e += "    at " + d + " (" + g.source + ":" + g.line + ":" + g.column + ")\n")
        }(16 & n || !_) && (64 & n && (m = m.substring(m.replace(/\\/g, "/").lastIndexOf("/") + 1)), e += (_ ? "     = " + d : "    at " + d) + " (" + m + ":" + p + ":" + y + ")\n"), 128 & n && o[0] && (o[1] == d && o[2].length > 0 && (e = e.replace(/\s+$/, ""), e += " with values: " + o[1] + o[2] + "\n"), o = Nd(o[0]))
      }
      return e = e.replace(/\s+$/, "")
    }

    function zd(n, e) {
      24 & n && (e = e.replace(/\s+$/, ""), e += (e.length > 0 ? "\n" : "") + Ud(n)), 1 & n ? 4 & n ? x(e) : 2 & n ? console.warn(e) : 512 & n ? console.info(e) : 256 & n ? console.debug(e) : console.log(e) : 6 & n ? x(e) : M(e)
    }

    function qd(n, e, i) {
      zd(n, tn(Kd(e, i), 0))
    }

    function Hd(n, e) {
      throw Xg(n, e || 1), "longjmp"
    }

    function Vd(n, e) {
      return Hd(n, e)
    }

    function Yd(n, e, i) {
      V.copyWithin(n, e, e + i)
    }

    function Jd(n, e) {
      return dd.fullscreenEnabled() ? (n = yd()) ? n.requestFullscreen || n.webkitRequestFullscreen ? dd.canPerformEventHandlerRequests() ? Wd(n, e) : e.deferUntilInEventHandler ? (dd.deferCall(Wd, 1, [n, e]), 1) : -2 : -3 : -4 : -1
    }

    function Zd(n, e) {
      return Jd(n, {
        scaleMode: 0,
        canvasResolutionScaleMode: 0,
        filteringMode: 0,
        deferUntilInEventHandler: e,
        canvasResizedCallbackTargetThread: 2
      })
    }

    function Qd(n, e) {
      return (n = yd()) ? n.requestPointerLock || n.msRequestPointerLock ? dd.canPerformEventHandlerRequests() ? Ad(n) : e ? (dd.deferCall(Ad, 2, [n]), 1) : -2 : -1 : -4
    }

    function $d(n) {
      try {
        return B.grow(n - q.byteLength + 65535 >>> 16), mn(B.buffer), 1
      } catch (i) {
        var e = {
          stage: "WasmMemoryGrowException",
          error: i ? i.toString() : "empty",
          oldSize: q.byteLength,
          newSize: n
        };
        GameGlobal.manager.reporter.alarm.malloc(e)
      }
    }

    function nm(n) {
      var e = V.length;
      if ((n >>>= 0) > 2147483648) return !1;
      for (var i = 1; i <= 4; i *= 2) {
        var t = e * (1 + .2 / i);
        t = Math.min(t, n + 100663296);
        var r = Math.min(2147483648, dn(Math.max(n, t), 65536)),
          o = {
            stage: "TryToRealloc",
            oldSize: e,
            requestedSize: n,
            newSize: r
          };
        if (GameGlobal.manager.reporter.alarm.malloc(o), $d(r)) return !0
      }
      return !1
    }

    function em() {
      try {
        if (navigator.getGamepads) return (dd.lastGamepadState = navigator.getGamepads()) ? 0 : -1
      } catch (n) {
        navigator.getGamepads = null
      }
      return -1
    }

    function im(n, e, i, t, r, o, a) {
      dd.focusEvent || (dd.focusEvent = Lg(256));
      var l = {
        target: yd(),
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(n) {
          var i = dd.focusEvent;
          Ng(t, r, i, e)
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function tm(n, e, i, t, r) {
      return im(0, e, i, t, 12, "blur"), 0
    }

    function rm(n, e, i, t, r) {
      return im(0, e, i, t, 13, "focus"), 0
    }

    function om(n, e, i, t, r, o, a) {
      dd.fullscreenChangeEvent || (dd.fullscreenChangeEvent = Lg(280));
      var l = {
        target: n,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(n) {
          var i, o, a, l = n || event,
            u = dd.fullscreenChangeEvent;
          Md(u), i = r, o = u, a = e, Ng.apply(null, [t, i, o, a]) && l.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function am(n, e, i, t, r) {
      return dd.fullscreenEnabled() ? (n = yd()) ? (om(n, e, i, t, 19, "fullscreenchange"), om(n, e, i, t, 19, "webkitfullscreenchange"), 0) : -4 : -1
    }

    function lm(n, e, i, t, r, o, a) {
      dd.gamepadEvent || (dd.gamepadEvent = Lg(1432));
      var l = {
        target: yd(),
        allowsDeferredCalls: !0,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(n) {
          var i, o, a, l = n || event,
            u = dd.gamepadEvent;
          Xd(u, l.gamepad), i = r, o = u, a = e, Ng.apply(null, [t, i, o, a]) && l.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function um(n, e, i, t) {
      return em() ? -1 : lm(0, n, e, i, 26, "gamepadconnected")
    }

    function fm(n, e, i, t) {
      return em() ? -1 : lm(0, n, e, i, 27, "gamepaddisconnected")
    }

    function cm(n, e, i) {
      return setInterval((function() {
        var e;
        e = i, zg.apply(null, [n, e])
      }), e)
    }

    function sm(n, e, i, t, r, o, a) {
      dd.keyEvent || (dd.keyEvent = Lg(164));
      var l = {
        target: yd(),
        allowsDeferredCalls: !0,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(n) {
          var i, o, a, l = dd.keyEvent,
            u = l >> 2;
          Z[u + 0] = n.location, Z[u + 1] = n.ctrlKey, Z[u + 2] = n.shiftKey, Z[u + 3] = n.altKey, Z[u + 4] = n.metaKey, Z[u + 5] = n.repeat, Z[u + 6] = n.charCode, Z[u + 7] = n.keyCode, Z[u + 8] = n.which, an(n.key || "", l + 36, 32), an(n.code || "", l + 68, 32), an(n.char || "", l + 100, 32), an(n.locale || "", l + 132, 32), i = r, o = l, a = e, Ng.apply(null, [t, i, o, a]) && n.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function dm(n, e, i, t, r) {
      return sm(0, e, i, t, 2, "keydown"), 0
    }

    function mm(n, e, i, t, r) {
      return sm(0, e, i, t, 1, "keypress"), 0
    }

    function pm(n, e, i, t, r) {
      return sm(0, e, i, t, 3, "keyup"), 0
    }

    function ym(n, e, i) {
      ld((function() {
        Zg.call(null, n)
      }), e, i)
    }

    function vm(n, e, i) {
      var t = n >> 2;
      Z[t + 0] = e.screenX, Z[t + 1] = e.screenY, Z[t + 2] = e.clientX, Z[t + 3] = e.clientY, Z[t + 4] = e.ctrlKey, Z[t + 5] = e.shiftKey, Z[t + 6] = e.altKey, Z[t + 7] = e.metaKey, Y[2 * t + 16] = e.button, Y[2 * t + 17] = e.buttons, Z[t + 9] = e.movementX, Z[t + 10] = e.movementY;
      var r = Ed(i);
      Z[t + 11] = e.clientX - r.left, Z[t + 12] = e.clientY - r.top
    }

    function _m(n, e, i, t, r, o, a) {
      dd.mouseEvent || (dd.mouseEvent = Lg(64));
      var l = {
        target: n = yd(),
        allowsDeferredCalls: "mousemove" != o && "mouseenter" != o && "mouseleave" != o,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(i) {
          var o, a, l, u = i || event;
          vm(dd.mouseEvent, u, n), o = r, a = dd.mouseEvent, l = e, Ng.apply(null, [t, o, a, l]) && u.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function gm(n, e, i, t, r) {
      return _m(n, e, i, t, 5, "mousedown"), 0
    }

    function hm(n, e, i, t, r) {
      return _m(n, e, i, t, 8, "mousemove"), 0
    }

    function wm(n, e, i, t, r) {
      return _m(n, e, i, t, 6, "mouseup"), 0
    }

    function Sm(n) {
      var e = document.pointerLockElement || document.mozPointerLockElement || document.webkitPointerLockElement || document.msPointerLockElement,
        i = !!e;
      Z[n >> 2] = i;
      var t = dd.getNodeNameForTarget(e),
        r = e && e.id ? e.id : "";
      an(t, n + 4, 128), an(r, n + 132, 128)
    }

    function Cm(n, e, i, t, r, o, a) {
      dd.pointerlockChangeEvent || (dd.pointerlockChangeEvent = Lg(260));
      var l = {
        target: n,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(n) {
          var i, o, a, l = n || event,
            u = dd.pointerlockChangeEvent;
          Sm(u), i = r, o = u, a = e, Ng.apply(null, [t, i, o, a]) && l.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function Em(n, e, i, t, r) {
      return document && document.body && (document.body.requestPointerLock || document.body.mozRequestPointerLock || document.body.webkitRequestPointerLock || document.body.msRequestPointerLock) ? (n = yd()) ? (Cm(n, e, i, t, 20, "pointerlockchange"), Cm(n, e, i, t, 20, "mozpointerlockchange"), Cm(n, e, i, t, 20, "webkitpointerlockchange"), Cm(n, e, i, t, 20, "mspointerlockchange"), 0) : -4 : -1
    }

    function bm(n, e, i, t, r, o, a) {
      dd.touchEvent || (dd.touchEvent = Lg(1684));
      var l = {
        target: n = yd(),
        allowsDeferredCalls: "touchstart" == o || "touchend" == o,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(i) {
          for (var o = {}, a = i.touches, l = 0; l < a.length; ++l)(y = a[l]).isChanged = y.onTarget = 0, o[y.identifier] = y;
          for (l = 0; l < i.changedTouches.length; ++l)(y = i.changedTouches[l]).isChanged = 1, o[y.identifier] = y;
          for (l = 0; l < i.targetTouches.length; ++l) o[i.targetTouches[l].identifier].onTarget = 1;
          var u = dd.touchEvent,
            f = u >> 2;
          Z[f + 1] = i.ctrlKey, Z[f + 2] = i.shiftKey, Z[f + 3] = i.altKey, Z[f + 4] = i.metaKey, f += 5;
          var c, s, d, m = Ed(n),
            p = 0;
          for (var l in o) {
            var y = o[l];
            if (Z[f + 0] = y.identifier, Z[f + 1] = y.screenX, Z[f + 2] = y.screenY, Z[f + 3] = y.clientX, Z[f + 4] = y.clientY, Z[f + 5] = y.pageX, Z[f + 6] = y.pageY, Z[f + 7] = y.isChanged, Z[f + 8] = y.onTarget, Z[f + 9] = y.clientX - m.left, Z[f + 10] = y.clientY - m.top, f += 13, ++p > 31) break
          }
          Z[u >> 2] = p, c = r, s = u, d = e, Ng.apply(null, [t, c, s, d]) && i.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function Wm(n, e, i, t, r) {
      return bm(n, e, i, t, 25, "touchcancel"), 0
    }

    function Dm(n, e, i, t, r) {
      return bm(n, e, i, t, 23, "touchend"), 0
    }

    function Am(n, e, i, t, r) {
      return bm(n, e, i, t, 24, "touchmove"), 0
    }

    function km(n, e, i, t, r) {
      return bm(n, e, i, t, 22, "touchstart"), 0
    }

    function Mm(n, e, i, t, r, o, a) {
      dd.wheelEvent || (dd.wheelEvent = Lg(96));
      var l = {
        target: n,
        allowsDeferredCalls: !0,
        eventTypeString: o,
        callbackfunc: t,
        handlerFunc: function(i) {
          var o, a, l, u = i || event,
            f = dd.wheelEvent;
          vm(f, u, n), nn[f + 64 >> 3] = u.deltaX, nn[f + 72 >> 3] = u.deltaY, nn[f + 80 >> 3] = u.deltaZ, Z[f + 88 >> 2] = u.deltaMode, o = r, a = f, l = e, Ng.apply(null, [t, o, a, l]) && u.preventDefault()
        },
        useCapture: i
      };
      dd.registerOrRemoveHandler(l)
    }

    function xm(n, e, i, t, r) {
      return void 0 !== (n = yd()).onwheel ? (Mm(n, e, i, t, 9, "wheel"), 0) : -1
    }

    function Xm(n) {
      for (var e = zs(); zs() - e < n;);
    }

    function jm(n) {
      var e = n.getExtension("ANGLE_instanced_arrays");
      if (e) return n.vertexAttribDivisor = function(n, i) {
        e.vertexAttribDivisorANGLE(n, i)
      }, n.drawArraysInstanced = function(n, i, t, r) {
        e.drawArraysInstancedANGLE(n, i, t, r)
      }, n.drawElementsInstanced = function(n, i, t, r, o) {
        e.drawElementsInstancedANGLE(n, i, t, r, o)
      }, 1
    }

    function Tm(n) {
      var e = n.getExtension("OES_vertex_array_object");
      if (e) return n.createVertexArray = function() {
        return e.createVertexArrayOES()
      }, n.deleteVertexArray = function(n) {
        e.deleteVertexArrayOES(n)
      }, n.bindVertexArray = function(n) {
        e.bindVertexArrayOES(n)
      }, n.isVertexArray = function(n) {
        return e.isVertexArrayOES(n)
      }, 1
    }

    function Lm(n) {
      var e = n.getExtension("WEBGL_draw_buffers");
      if (e) return n.drawBuffers = function(n, i) {
        e.drawBuffersWEBGL(n, i)
      }, 1
    }

    function Fm(n) {
      return !!(n.dibvbi = n.getExtension("WEBGL_draw_instanced_base_vertex_base_instance"))
    }

    function Pm(n) {
      return !!(n.mdibvbi = n.getExtension("WEBGL_multi_draw_instanced_base_vertex_base_instance"))
    }

    function Rm(n) {
      return !!(n.multiDrawWebgl = n.getExtension("WEBGL_multi_draw"))
    }
    var Bm = {
        counter: 1,
        buffers: [],
        mappedBuffers: {},
        programs: [],
        framebuffers: [],
        renderbuffers: [],
        textures: [],
        shaders: [],
        vaos: [],
        contexts: [],
        offscreenCanvases: {},
        queries: [],
        samplers: [],
        transformFeedbacks: [],
        syncs: [],
        byteSizeByTypeRoot: 5120,
        byteSizeByType: [1, 1, 2, 2, 4, 4, 4, 2, 3, 4, 8],
        stringCache: {},
        stringiCache: {},
        unpackAlignment: 4,
        recordError: function(n) {
          Bm.lastError || (Bm.lastError = n)
        },
        getNewId: function(n) {
          for (var e = Bm.counter++, i = n.length; i < e; i++) n[i] = null;
          return e
        },
        MAX_TEMP_BUFFER_SIZE: 2097152,
        numTempVertexBuffersPerSize: 64,
        log2ceilLookup: function(n) {
          return 32 - Math.clz32(0 === n ? 0 : n - 1)
        },
        generateTempBuffers: function(n, e) {
          var i = Bm.log2ceilLookup(Bm.MAX_TEMP_BUFFER_SIZE);
          e.tempVertexBufferCounters1 = [], e.tempVertexBufferCounters2 = [], e.tempVertexBufferCounters1.length = e.tempVertexBufferCounters2.length = i + 1, e.tempVertexBuffers1 = [], e.tempVertexBuffers2 = [], e.tempVertexBuffers1.length = e.tempVertexBuffers2.length = i + 1, e.tempIndexBuffers = [], e.tempIndexBuffers.length = i + 1;
          for (var t = 0; t <= i; ++t) {
            e.tempIndexBuffers[t] = null, e.tempVertexBufferCounters1[t] = e.tempVertexBufferCounters2[t] = 0;
            var r = Bm.numTempVertexBuffersPerSize;
            e.tempVertexBuffers1[t] = [], e.tempVertexBuffers2[t] = [];
            var o = e.tempVertexBuffers1[t],
              a = e.tempVertexBuffers2[t];
            o.length = a.length = r;
            for (var l = 0; l < r; ++l) o[l] = a[l] = null
          }
          if (n) {
            e.tempQuadIndexBuffer = lg.createBuffer(), e.GLctx.bindBuffer(34963, e.tempQuadIndexBuffer);
            for (var u = Bm.MAX_TEMP_BUFFER_SIZE >> 1, f = new Uint16Array(u), c = (t = 0, 0); !(f[t++] = c, t >= u || (f[t++] = c + 1, t >= u) || (f[t++] = c + 2, t >= u) || (f[t++] = c, t >= u) || (f[t++] = c + 2, t >= u) || (f[t++] = c + 3, t >= u));) c += 4;
            e.GLctx.bufferData(34963, f, 35044), e.GLctx.bindBuffer(34963, null)
          }
        },
        getTempVertexBuffer: function(n) {
          var e = Bm.log2ceilLookup(n),
            i = Bm.currentContext.tempVertexBuffers1[e],
            t = Bm.currentContext.tempVertexBufferCounters1[e];
          Bm.currentContext.tempVertexBufferCounters1[e] = Bm.currentContext.tempVertexBufferCounters1[e] + 1 & Bm.numTempVertexBuffersPerSize - 1;
          var r = i[t];
          if (r) return r;
          var o = lg.getParameter(34964);
          return i[t] = lg.createBuffer(), lg.bindBuffer(34962, i[t]), lg.bufferData(34962, 1 << e, 35048), lg.bindBuffer(34962, o), i[t]
        },
        getTempIndexBuffer: function(n) {
          var e = Bm.log2ceilLookup(n),
            i = Bm.currentContext.tempIndexBuffers[e];
          if (i) return i;
          var t = lg.getParameter(34965);
          return Bm.currentContext.tempIndexBuffers[e] = lg.createBuffer(), lg.bindBuffer(34963, Bm.currentContext.tempIndexBuffers[e]), lg.bufferData(34963, 1 << e, 35048), lg.bindBuffer(34963, t), Bm.currentContext.tempIndexBuffers[e]
        },
        newRenderingFrameStarted: function() {
          if (Bm.currentContext) {
            var n = Bm.currentContext.tempVertexBuffers1;
            Bm.currentContext.tempVertexBuffers1 = Bm.currentContext.tempVertexBuffers2, Bm.currentContext.tempVertexBuffers2 = n, n = Bm.currentContext.tempVertexBufferCounters1, Bm.currentContext.tempVertexBufferCounters1 = Bm.currentContext.tempVertexBufferCounters2, Bm.currentContext.tempVertexBufferCounters2 = n;
            for (var e = Bm.log2ceilLookup(Bm.MAX_TEMP_BUFFER_SIZE), i = 0; i <= e; ++i) Bm.currentContext.tempVertexBufferCounters1[i] = 0
          }
        },
        getSource: function(n, e, i, t) {
          for (var r = "", o = 0; o < e; ++o) {
            var a = t ? Z[t + 4 * o >> 2] : -1;
            r += rn(Z[i + 4 * o >> 2], a < 0 ? void 0 : a)
          }
          return r
        },
        calcBufLength: function(n, e, i, t) {
          return i > 0 ? t * i : n * Bm.byteSizeByType[e - Bm.byteSizeByTypeRoot] * t
        },
        usedTempBuffers: [],
        preDrawHandleClientVertexAttribBindings: function(n) {
          Bm.resetBufferBinding = !1;
          for (var e = 0; e < Bm.currentContext.maxVertexAttribs; ++e) {
            var i = Bm.currentContext.clientBuffers[e];
            if (i.clientside && i.enabled) {
              Bm.resetBufferBinding = !0;
              var t = Bm.calcBufLength(i.size, i.type, i.stride, n),
                r = Bm.getTempVertexBuffer(t);
              lg.bindBuffer(34962, r), lg.bufferSubData(34962, 0, V.subarray(i.ptr, i.ptr + t)), i.vertexAttribPointerAdaptor.call(lg, e, i.size, i.type, i.normalized, i.stride, 0)
            }
          }
        },
        postDrawHandleClientVertexAttribBindings: function() {
          Bm.resetBufferBinding && lg.bindBuffer(34962, Bm.buffers[lg.currentArrayBufferBinding])
        },
        createContext: function(e, i) {
          e.getContextSafariWebGL2Fixed || (e.getContextSafariWebGL2Fixed = e.getContext, e.getContext = function(i, t) {
            var r = e.getContextSafariWebGL2Fixed(i, t);
            return n.IsWxGame || "webgl" == i == r instanceof WebGLRenderingContext ? r : null
          });
          var t = i.majorVersion > 1 ? e.getContext("webgl2", i) : e.getContext("webgl", i);
          return t ? Bm.registerContext(t, i) : 0
        },
        registerContext: function(n, e) {
          var i = Bm.getNewId(Bm.contexts),
            t = {
              handle: i,
              attributes: e,
              version: e.majorVersion,
              GLctx: n
            };
          n.canvas && (n.canvas.GLctxObject = t), Bm.contexts[i] = t, (void 0 === e.enableExtensionsByDefault || e.enableExtensionsByDefault) && Bm.initExtensions(t), t.maxVertexAttribs = t.GLctx.getParameter(34921), t.clientBuffers = [];
          for (var r = 0; r < t.maxVertexAttribs; r++) t.clientBuffers[r] = {
            enabled: !1,
            clientside: !1,
            size: 0,
            type: 0,
            normalized: 0,
            stride: 0,
            ptr: 0,
            vertexAttribPointerAdaptor: null
          };
          return Bm.generateTempBuffers(!1, t), i
        },
        makeContextCurrent: function(e) {
          return Bm.currentContext = Bm.contexts[e], n.ctx = lg = Bm.currentContext && Bm.currentContext.GLctx, !(e && !lg)
        },
        getContext: function(n) {
          return Bm.contexts[n]
        },
        deleteContext: function(n) {
          Bm.currentContext === Bm.contexts[n] && (Bm.currentContext = null), "object" == typeof dd && dd.removeAllHandlersOnTarget(Bm.contexts[n].GLctx.canvas), Bm.contexts[n] && Bm.contexts[n].GLctx.canvas && (Bm.contexts[n].GLctx.canvas.GLctxObject = void 0), Bm.contexts[n] = null
        },
        initExtensions: function(n) {
          if (n || (n = Bm.currentContext), !n.initExtensionsDone) {
            n.initExtensionsDone = !0;
            var e = n.GLctx;
            jm(e), Tm(e), Lm(e), Fm(e), Pm(e), n.version >= 2 && (e.disjointTimerQueryExt = e.getExtension("EXT_disjoint_timer_query_webgl2")), (n.version < 2 || !e.disjointTimerQueryExt) && (e.disjointTimerQueryExt = e.getExtension("EXT_disjoint_timer_query")), Rm(e);
            var i = e.getSupportedExtensions() || [];
            GameGlobal.USED_TEXTURE_COMPRESSION && (i.push("WEBGL_compressed_texture_etc1"), i.push("WEBGL_compressed_texture_etc")), i.forEach((function(n) {
              n.includes("lose_context") || n.includes("debug") || e.getExtension(n)
            }))
          }
        }
      },
      Gm = ["default", "low-power", "high-performance"];

    function Om(n, e) {
      var i = e >> 2,
        t = Z[i + 6],
        r = {
          alpha: !!Z[i + 0],
          depth: !!Z[i + 1],
          stencil: !!Z[i + 2],
          antialias: !!Z[i + 3],
          premultipliedAlpha: !!Z[i + 4],
          preserveDrawingBuffer: !!Z[i + 5],
          powerPreference: Gm[t],
          failIfMajorPerformanceCaveat: !!Z[i + 7],
          majorVersion: Z[i + 8],
          minorVersion: Z[i + 9],
          enableExtensionsByDefault: Z[i + 10],
          explicitSwapControl: Z[i + 11],
          proxyContextToMainThread: Z[i + 12],
          renderViaOffscreenBackBuffer: Z[i + 13]
        },
        o = vd();
      if (!o) return 0;
      if (r.explicitSwapControl) return 0;
      var a = Bm.createContext(o, r);
      return WXWASMSDK.canvasContext && WXWASMSDK.canvasContext._triggerCallback(), a
    }

    function Im(n, e) {
      return Om(0, e)
    }

    function Km() {
      return Bm.currentContext ? Bm.currentContext.handle : 0
    }

    function Nm() {
      return Km()
    }

    function Um(n) {
      return Bm.makeContextCurrent(n) ? 0 : -5
    }

    function zm(n) {
      Bm.currentContext == n && (Bm.currentContext = 0), Bm.deleteContext(n)
    }

    function qm(n, e) {
      var i = Bm.getContext(n),
        t = rn(e);
      return t.startsWith("GL_") && (t = t.substr(3)), "ANGLE_instanced_arrays" == t && jm(lg), "OES_vertex_array_object" == t && Tm(lg), "WEBGL_draw_buffers" == t && Lm(lg), "WEBGL_draw_instanced_base_vertex_base_instance" == t && Fm(lg), "WEBGL_multi_draw_instanced_base_vertex_base_instance" == t && Pm(lg), "WEBGL_multi_draw" == t && Rm(lg), !!i.GLctx.getExtension(t)
    }

    function Hm(n) {
      for (var e = n >> 2, i = 0; i < 14; ++i) Z[e + i] = 0;
      Z[e + 0] = Z[e + 1] = Z[e + 3] = Z[e + 4] = Z[e + 8] = Z[e + 10] = 1
    }
    n._emscripten_webgl_get_current_context = Nm, n._emscripten_webgl_make_context_current = Um;
    var Vm = {};

    function Ym() {
      return _ || "./this.program"
    }

    function Jm() {
      if (!Jm.strings) {
        var n = {
          USER: "web_user",
          LOGNAME: "web_user",
          PATH: "/",
          PWD: "/",
          HOME: "/home/web_user",
          LANG: ("object" == typeof navigator && navigator.languages && navigator.languages[0] || "C").replace("-", "_") + ".UTF-8",
          _: Ym()
        };
        for (var e in Vm) n[e] = Vm[e];
        var i = [];
        for (var e in n) i.push(e + "=" + n[e]);
        Jm.strings = i
      }
      return Jm.strings
    }

    function Zm(n, e) {
      try {
        var i = 0;
        return Jm().forEach((function(t, r) {
          var o = e + i;
          Z[n + 4 * r >> 2] = o, sn(t, o), i += t.length + 1
        })), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function Qm(n, e) {
      try {
        var i = Jm();
        Z[n >> 2] = i.length;
        var t = 0;
        return i.forEach((function(n) {
          t += n.length + 1
        })), Z[e >> 2] = t, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function $m(n) {
      if (n > Xc.MAX_OPEN_FDS) return 0;
      try {
        var e = jc.getStreamFromFD(n);
        return Xc.close(e), 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function np(n, e) {
      try {
        var i = jc.getStreamFromFD(n),
          t = i.tty ? 2 : Xc.isDir(i.mode) ? 3 : Xc.isLink(i.mode) ? 7 : 4;
        return H[e >> 0] = t, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function ep(n, e, i, t) {
      try {
        var o = jc.getStreamFromFD(n),
          a = jc.doReadv(o, e, i);
        return Z[t >> 2] = a, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function ip(n, e, i, t, o) {
      try {
        var a = jc.getStreamFromFD(n),
          l = 4294967296 * i + (e >>> 0);
        return l <= -9007199254740992 || l >= 9007199254740992 ? -61 : (Xc.llseek(a, l, t), Bn = [a.position >>> 0, (Rn = a.position, +Math.abs(Rn) >= 1 ? Rn > 0 ? (0 | Math.min(+Math.floor(Rn / 4294967296), 4294967295)) >>> 0 : ~~+Math.ceil((Rn - +(~~Rn >>> 0)) / 4294967296) >>> 0 : 0)], Z[o >> 2] = Bn[0], Z[o + 4 >> 2] = Bn[1], a.getdents && 0 === l && 0 === t && (a.getdents = null), 0)
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function tp(n, e, i, t) {
      try {
        var o = jc.getStreamFromFD(n),
          a = jc.doWritev(o, e, i);
        return Z[t >> 2] = a, 0
      } catch (n) {
        return void 0 !== Xc && n instanceof Xc.ErrnoError || r(n), n.errno
      }
    }

    function rp(n, e) {
      return 0
    }

    function op() {
      return R()
    }

    function ap(n, e, i, t) {
      var r, o = 0,
        a = 0,
        l = 0,
        u = 0,
        f = 0,
        c = 0;

      function s(n, e, i, t, r, o) {
        var a, l, u;
        return l = 10 === n ? 28 : 16, r = 10 === n ? qc(r) : zc(r), K(!Ic(a = Lg(l), n, r, o)), u = Lg(32), Z[u + 4 >> 2] = n, Z[u + 8 >> 2] = e, Z[u + 12 >> 2] = i, Z[u + 24 >> 2] = t, Z[u + 20 >> 2] = a, Z[u + 16 >> 2] = 10 === n ? 28 : 16, Z[u + 28 >> 2] = 0, u
      }
      if (i && (l = Z[i >> 2], u = Z[i + 4 >> 2], f = Z[i + 8 >> 2], c = Z[i + 12 >> 2]), f && !c && (c = 2 === f ? 17 : 6), !f && c && (f = 17 === c ? 2 : 1), 0 === c && (c = 6), 0 === f && (f = 1), !n && !e) return -2;
      if (-1088 & l) return -1;
      if (0 !== i && 2 & Z[i >> 2] && !n) return -1;
      if (32 & l) return -2;
      if (0 !== f && 1 !== f && 2 !== f) return -7;
      if (0 !== u && 2 !== u && 10 !== u) return -6;
      if (e && (e = rn(e), a = parseInt(e, 10), isNaN(a))) return 1024 & l ? -2 : -8;
      if (!n) return 0 === u && (u = 2), 0 == (1 & l) && (o = 2 === u ? Cg(2130706433) : [0, 0, 0, 1]), r = s(u, f, c, null, o, a), Z[t >> 2] = r, 0;
      if (null !== (o = Bc(n = rn(n))))
        if (0 === u || 2 === u) u = 2;
        else {
          if (!(10 === u && 8 & l)) return -2;
          o = [0, 0, Cg(65535), o], u = 10
        }
      else if (null !== (o = Oc(n))) {
        if (0 !== u && 10 !== u) return -2;
        u = 10
      }
      return null != o ? (r = s(u, f, c, n, o, a), Z[t >> 2] = r, 0) : 4 & l ? -2 : (o = Bc(n = Kc.lookup_name(n)), 0 === u ? u = 2 : 10 === u && (o = [0, 0, Cg(65535), o]), r = s(u, f, c, null, o, a), Z[t >> 2] = r, 0)
    }

    function lp(n) {
      var e = Lg(20),
        i = Lg(n.length + 1);
      an(n, i, n.length + 1), Z[e >> 2] = i;
      var t = Lg(4);
      Z[t >> 2] = 0, Z[e + 4 >> 2] = t;
      Z[e + 8 >> 2] = 2, Z[e + 12 >> 2] = 4;
      var r = Lg(12);
      return Z[r >> 2] = r + 8, Z[r + 4 >> 2] = 0, Z[r + 8 >> 2] = Bc(Kc.lookup_name(n)), Z[e + 16 >> 2] = r, e
    }

    function up(n, e, i) {
      if (2 !== i) return Rc(5), null;
      var t = zc(n = Z[n >> 2]),
        r = Kc.lookup_addr(t);
      return r && (t = r), lp(t)
    }

    function fp(n) {
      return lp(rn(n))
    }

    function cp(n, e, i, t, r, o, a) {
      var l = Hc(n, e);
      if (l.errno) return -6;
      var u = l.port,
        f = l.addr,
        c = !1;
      if (i && t) {
        var s;
        if (1 & a || !(s = Kc.lookup_addr(f))) {
          if (8 & a) return -2
        } else f = s;
        an(f, i, t) + 1 >= t && (c = !0)
      }
      r && o && (an(u = "" + u, r, o) + 1 >= o && (c = !0));
      return c ? -12 : 0
    }

    function sp() {
      throw "getpwuid: TODO"
    }

    function dp(n) {
      var e = Date.now();
      return Z[n >> 2] = e / 1e3 | 0, Z[n + 4 >> 2] = e % 1e3 * 1e3 | 0, 0
    }

    function mp(n) {
      lg.activeTexture(n)
    }

    function pp(n, e) {
      (n = Bm.programs[n])[(e = Bm.shaders[e]).shaderType] = e, lg.attachShader(n, e)
    }

    function yp(n, e) {
      lg.beginQuery(n, Bm.queries[e])
    }

    function vp(n) {
      lg.beginTransformFeedback(n)
    }

    function _p(n, e, i) {
      lg.bindAttribLocation(Bm.programs[n], e, rn(i))
    }

    function gp(n, e) {
      34962 == n ? lg.currentArrayBufferBinding = e : 34963 == n && (lg.currentElementArrayBufferBinding = e), 35051 == n ? lg.currentPixelPackBufferBinding = e : 35052 == n && (lg.currentPixelUnpackBufferBinding = e), lg.bindBuffer(n, Bm.buffers[e])
    }

    function hp(n, e, i) {
      lg.bindBufferBase(n, e, Bm.buffers[i])
    }

    function wp(n, e, i, t, r) {
      lg.bindBufferRange(n, e, Bm.buffers[i], t, r)
    }

    function Sp(n, e) {
      lg.bindFramebuffer(n, Bm.framebuffers[e])
    }

    function Cp(n, e) {
      lg.bindRenderbuffer(n, Bm.renderbuffers[e])
    }

    function Ep(n, e) {
      lg.bindSampler(n, Bm.samplers[e])
    }

    function bp(n, e) {
      window._lastBoundTexture = e, lg.bindTexture(n, e ? Bm.textures[e] : null)
    }

    function Wp(n, e) {
      lg.bindTransformFeedback(n, Bm.transformFeedbacks[e])
    }

    function Dp(n) {
      lg.bindVertexArray(Bm.vaos[n]);
      var e = lg.getParameter(34965);
      lg.currentElementArrayBufferBinding = e ? 0 | e.name : 0
    }

    function Ap(n) {
      lg.blendEquation(n)
    }

    function kp(n, e) {
      lg.blendEquationSeparate(n, e)
    }

    function Mp(n, e, i, t) {
      lg.blendFuncSeparate(n, e, i, t)
    }

    function xp(n, e, i, t, r, o, a, l, u, f) {
      lg.blitFramebuffer(n, e, i, t, r, o, a, l, u, f)
    }

    function Xp(n, e, i, t) {
      Bm.currentContext.version >= 2 ? i ? lg.bufferData(n, V, t, i, e) : lg.bufferData(n, e, t) : lg.bufferData(n, i ? V.subarray(i, i + e) : e, t)
    }

    function jp(n, e, i, t) {
      Bm.currentContext.version >= 2 ? lg.bufferSubData(n, e, V, t, i) : lg.bufferSubData(n, e, V.subarray(t, t + i))
    }

    function Tp(n) {
      return lg.checkFramebufferStatus(n)
    }

    function Lp(n) {
      lg.clear(n)
    }

    function Fp(n, e, i, t) {
      lg.clearBufferfi(n, e, i, t)
    }

    function Pp(n, e, i) {
      lg.clearBufferfv(n, e, $, i >> 2)
    }

    function Rp(n, e, i) {
      lg.clearBufferuiv(n, e, Q, i >> 2)
    }

    function Bp(n, e, i, t) {
      lg.clearColor(n, e, i, t)
    }

    function Gp(n) {
      lg.clearDepth(n)
    }

    function Op(n) {
      lg.clearStencil(n)
    }

    function Ip(n, e, i, t) {
      return 37146
    }

    function Kp(n, e, i, t) {
      lg.colorMask(!!n, !!e, !!i, !!t)
    }

    function Np(n) {
      lg.compileShader(Bm.shaders[n])
    }

    function Up(n, e, i, t, r, o, a, l) {
      var u = window._lastTextureId,
        f = "undefined" != typeof wx;
      var c = function() {
          var n = 36196 == i;
          if (f && GameGlobal.USED_TEXTURE_COMPRESSION && n) {
            var e = V.subarray(l, l + 1)[0],
              t = V.subarray(l + 1, l + 1 + e),
              r = [];
            t.forEach((function(n) {
              r.push(String.fromCharCode(n))
            }));
            var o = r.join(""),
              a = r.length - 8,
              u = r.length - 5;
            if ("_" == r[a]) {
              a++;
              for (var c = ["a", "s", "t", "c"], s = 0; s < c.length; s++)
                if (r[a + s] != c[s]) return [o, "8x8", !1];
              a--;
              var d = o.substring(a + 5);
              return [o.substr(0, a), d, !1]
            }
            if ("_" == r[u]) {
              u++;
              var m = r[u++];
              if ("4" != m && "5" != m && "6" != m && "8" != m) return [o, "8x8", !1];
              d = m + "x" + m;
              var p = !1;
              return "#" != r[u] && (p = !0), u -= 2, [o.substr(0, u), d, p]
            }
            return [o, "8x8", !1]
          }
          return [-1, "8x8", !1]
        }(),
        s = c[0],
        d = c[1],
        m = c[2];

      function p(n) {
        lg.texImage2D(lg.TEXTURE_2D, 0, lg.RGBA, lg.RGBA, lg.UNSIGNED_BYTE, n)
      }

      function y(i) {
        if (Bm.textures[u]) {
          var a = GameGlobal.DownloadedTextures[i].data,
            l = u;
          Bm.textures[l] && (lg.bindTexture(lg.TEXTURE_2D, Bm.textures[l]), m && !GameGlobal.NoneLimitSupportedTexture ? p(a) : GameGlobal.TextureCompressedFormat && ("pvr" != GameGlobal.TextureCompressedFormat || t === r && -1 != [1, 2, 4, 8, 16, 32, 64, 128, 256, 512, 1024, 2048, 4096].indexOf(r)) && ("dds" != GameGlobal.TextureCompressedFormat || r % 4 == 0 && t % 4 == 0) ? function(i) {
            var a = 0,
              l = 16;
            switch (m ? GameGlobal.NoneLimitSupportedTexture : GameGlobal.TextureCompressedFormat) {
              case "astc":
                var u = lg.getExtension("WEBGL_compressed_texture_astc");
                if ("4x4" == d) {
                  a = u.COMPRESSED_RGBA_ASTC_4x4_KHR;
                  break
                }
                if ("5x5" == d) {
                  a = u.COMPRESSED_RGBA_ASTC_5x5_KHR;
                  break
                }
                if ("6x6" == d) {
                  a = 37812;
                  break
                }
                a = u.COMPRESSED_RGBA_ASTC_8x8_KHR;
                break;
              case "etc2":
                a = lg.getExtension("WEBGL_compressed_texture_etc").COMPRESSED_RGBA8_ETC2_EAC;
                break;
              case "dds":
                a = lg.getExtension("WEBGL_compressed_texture_s3tc").COMPRESSED_RGBA_S3TC_DXT5_EXT, l = 128;
                break;
              case "pvr":
                a = lg.getExtension("WEBGL_compressed_texture_pvrtc").COMPRESSED_RGBA_PVRTC_4BPPV1_IMG;
                l = new Int32Array(i, 0, 13)[12] + 52;
                break;
              case "etc1":
                a = lg.getExtension("WEBGL_compressed_texture_etc1").COMPRESSED_RGB_ETC1_WEBGL
            }
            lg.compressedTexImage2D(n, e, a, t, r, o, new Uint8Array(i, l))
          }(a) : p(a), lg.bindTexture(lg.TEXTURE_2D, window._lastBoundTexture ? Bm.textures[window._lastBoundTexture] : null))
        }
      } - 1 == s ? Bm.currentContext.supportsWebGL2EntryPoints ? lg.compressedTexImage2D(n, e, i, t, r, o, V, l, a) : lg.compressedTexImage2D(n, e, i, t, r, o, l ? V.subarray(l, l + a) : null) : GameGlobal.DownloadedTextures[s] && GameGlobal.DownloadedTextures[s].data ? y(s) : (lg.texImage2D(lg.TEXTURE_2D, 0, lg.RGBA, 1, 1, 0, lg.RGBA, lg.UNSIGNED_SHORT_4_4_4_4, new Uint16Array([0, 0])), window.WXWASMSDK.WXDownloadTexture(s, t, r, (function() {
        y(s)
      }), m))
    }

    function zp(n, e, i, t, r, o, a, l, u) {
      lg.currentPixelUnpackBufferBinding ? lg.compressedTexImage3D(n, e, i, t, r, o, a, l, u) : lg.compressedTexImage3D(n, e, i, t, r, o, a, V, u, l)
    }

    function qp(n, e, i, t, r, o, a, l, u) {
      var f = window._lastTextureId,
        c = "undefined" != typeof wx;
      var s = function() {
          var n = 36196 == a;
          if (c && GameGlobal.USED_TEXTURE_COMPRESSION && n) {
            var e = V.subarray(u, u + 1)[0],
              i = V.subarray(u + 1, u + 1 + e),
              t = [];
            i.forEach((function(n) {
              t.push(String.fromCharCode(n))
            }));
            var r = t.join(""),
              o = t.length - 8,
              l = t.length - 5;
            if ("_" == t[o]) {
              o++;
              for (var f = ["a", "s", "t", "c"], s = 0; s < f.length; s++)
                if (t[o + s] != f[s]) return [r, "8x8", !1];
              o--;
              var d = r.substring(o + 5);
              return [r.substr(0, o), d, !1]
            }
            if ("_" == t[l]) {
              l++;
              var m = t[l++];
              if ("4" != m && "5" != m && "6" != m && "8" != m) return [r, "8x8", !1];
              d = m + "x" + m;
              var p = !1;
              return "#" != t[l] && (p = !0), l -= 2, [r.substr(0, l), d, p]
            }
            return [r, "8x8", !1]
          }
          return [-1, "8x8", !1]
        }(),
        d = s[0],
        m = s[1],
        p = s[2];

      function y(a) {
        lg.texSubImage2D(n, e, i, t, r, o, lg.RGBA, lg.UNSIGNED_BYTE, a)
      }

      function v(a) {
        if (Bm.textures[f]) {
          var l = GameGlobal.DownloadedTextures[a].data,
            u = f;
          Bm.textures[u] && (lg.bindTexture(lg.TEXTURE_2D, Bm.textures[u]), p && !GameGlobal.NoneLimitSupportedTexture ? y(l) : GameGlobal.TextureCompressedFormat && ("pvr" != GameGlobal.TextureCompressedFormat || r === o && -1 != PotList.indexOf(o)) && ("dds" != GameGlobal.TextureCompressedFormat || o % 4 == 0 && r % 4 == 0) ? function(a) {
            var l = 0,
              u = 16;
            switch (p ? GameGlobal.NoneLimitSupportedTexture : GameGlobal.TextureCompressedFormat) {
              case "astc":
                var f = lg.getExtension("WEBGL_compressed_texture_astc");
                if ("4x4" == m) {
                  l = f.COMPRESSED_RGBA_ASTC_4x4_KHR;
                  break
                }
                if ("5x5" == m) {
                  l = f.COMPRESSED_RGBA_ASTC_5x5_KHR;
                  break
                }
                if ("6x6" == m) {
                  l = 37812;
                  break
                }
                l = f.COMPRESSED_RGBA_ASTC_8x8_KHR;
                break;
              case "etc2":
                l = lg.getExtension("WEBGL_compressed_texture_etc").COMPRESSED_RGBA8_ETC2_EAC;
                break;
              case "dds":
                l = lg.getExtension("WEBGL_compressed_texture_s3tc").COMPRESSED_RGBA_S3TC_DXT5_EXT, u = 128;
                break;
              case "pvr":
                l = lg.getExtension("WEBGL_compressed_texture_pvrtc").COMPRESSED_RGBA_PVRTC_4BPPV1_IMG;
                u = new Int32Array(a, 0, 13)[12] + 52;
                break;
              case "etc1":
                l = lg.getExtension("WEBGL_compressed_texture_etc1").COMPRESSED_RGB_ETC1_WEBGL
            }
            lg.compressedTexSubImage2D(n, e, i, t, r, o, l, new Uint8Array(a, u))
          }(l) : y(l), lg.bindTexture(lg.TEXTURE_2D, window._lastBoundTexture ? Bm.textures[window._lastBoundTexture] : null))
        }
      }
      var _ = window._lastTexStorage2DParams;
      if (-1 != d) {
        var g = lg.RGBA8;
        switch (GameGlobal.TextureCompressedFormat) {
          case "astc":
            var h = lg.getExtension("WEBGL_compressed_texture_astc");
            if ("4x4" == m) {
              g = h.COMPRESSED_RGBA_ASTC_4x4_KHR;
              break
            }
            if ("5x5" == m) {
              g = h.COMPRESSED_RGBA_ASTC_5x5_KHR;
              break
            }
            if ("6x6" == m) {
              g = 37812;
              break
            }
            g = h.COMPRESSED_RGBA_ASTC_8x8_KHR;
            break;
          case "etc2":
            g = lg.getExtension("WEBGL_compressed_texture_etc").COMPRESSED_RGBA8_ETC2_EAC;
            break;
          case "dds":
            g = lg.getExtension("WEBGL_compressed_texture_s3tc").COMPRESSED_RGBA_S3TC_DXT5_EXT;
            break;
          case "pvr":
            g = lg.getExtension("WEBGL_compressed_texture_pvrtc").COMPRESSED_RGBA_PVRTC_4BPPV1_IMG
        }
        return lg.texStorage2D(_[0], _[1], g, r, o), void(GameGlobal.DownloadedTextures[d] && GameGlobal.DownloadedTextures[d].data ? v(d) : window.WXWASMSDK.WXDownloadTexture(d, r, o, (function() {
          v(d)
        }), p))
      }
      Bm.currentContext.supportsWebGL2EntryPoints ? lg.compressedTexSubImage2D(n, e, i, t, r, o, a, V, u, l) : lg.compressedTexSubImage2D(n, e, i, t, r, o, a, u ? V.subarray(u, u + l) : null)
    }

    function Hp(n, e, i, t, r, o, a, l, u, f, c) {
      lg.currentPixelUnpackBufferBinding ? lg.compressedTexSubImage3D(n, e, i, t, r, o, a, l, u, f, c) : lg.compressedTexSubImage3D(n, e, i, t, r, o, a, l, u, V, c, f)
    }

    function Vp(n, e, i, t, r) {
      lg.copyBufferSubData(n, e, i, t, r)
    }

    function Yp(n, e, i, t, r, o, a, l) {
      lg.copyTexImage2D(n, e, i, t, r, o, a, l)
    }

    function Jp(n, e, i, t, r, o, a, l) {
      lg.copyTexSubImage2D(n, e, i, t, r, o, a, l)
    }

    function Zp() {
      var n = Bm.getNewId(Bm.programs),
        e = lg.createProgram();
      return e.name = n, e.maxUniformLength = e.maxAttributeLength = e.maxUniformBlockNameLength = 0, e.uniformIdCounter = 1, Bm.programs[n] = e, n
    }

    function Qp(n) {
      var e = Bm.getNewId(Bm.shaders);
      return Bm.shaders[e] = lg.createShader(n), Bm.shaders[e].shaderType = 1 & n ? "vs" : "fs", e
    }

    function $p(n) {
      lg.cullFace(n)
    }

    function ny(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.buffers[t];
        r && (lg.deleteBuffer(r), r.name = 0, Bm.buffers[t] = null, t == lg.currentArrayBufferBinding && (lg.currentArrayBufferBinding = 0), t == lg.currentElementArrayBufferBinding && (lg.currentElementArrayBufferBinding = 0), t == lg.currentPixelPackBufferBinding && (lg.currentPixelPackBufferBinding = 0), t == lg.currentPixelUnpackBufferBinding && (lg.currentPixelUnpackBufferBinding = 0))
      }
    }

    function ey(n, e) {
      for (var i = 0; i < n; ++i) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.framebuffers[t];
        r && (lg.deleteFramebuffer(r), r.name = 0, Bm.framebuffers[t] = null)
      }
    }

    function iy(n) {
      if (n) {
        var e = Bm.programs[n];
        e ? (lg.deleteProgram(e), e.name = 0, Bm.programs[n] = null) : Bm.recordError(1281)
      }
    }

    function ty(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.queries[t];
        r && (lg.deleteQuery(r), Bm.queries[t] = null)
      }
    }

    function ry(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.renderbuffers[t];
        r && (lg.deleteRenderbuffer(r), r.name = 0, Bm.renderbuffers[t] = null)
      }
    }

    function oy(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.samplers[t];
        r && (lg.deleteSampler(r), r.name = 0, Bm.samplers[t] = null)
      }
    }

    function ay(n) {
      if (n) {
        var e = Bm.shaders[n];
        e ? (lg.deleteShader(e), Bm.shaders[n] = null) : Bm.recordError(1281)
      }
    }

    function ly(n) {
      if (n) {
        var e = Bm.syncs[n];
        e ? (lg.deleteSync(e), e.name = 0, Bm.syncs[n] = null) : Bm.recordError(1281)
      }
    }

    function uy(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.textures[t];
        r && (lg.deleteTexture(r), r.name = 0, Bm.textures[t] = null)
      }
    }

    function fy(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2],
          r = Bm.transformFeedbacks[t];
        r && (lg.deleteTransformFeedback(r), r.name = 0, Bm.transformFeedbacks[t] = null)
      }
    }

    function cy(n, e) {
      for (var i = 0; i < n; i++) {
        var t = Z[e + 4 * i >> 2];
        lg.deleteVertexArray(Bm.vaos[t]), Bm.vaos[t] = null
      }
    }

    function sy(n) {
      lg.depthFunc(n)
    }

    function dy(n) {
      lg.depthMask(!!n)
    }

    function my(n, e) {
      lg.detachShader(Bm.programs[n], Bm.shaders[e])
    }

    function py(n) {
      lg.disable(n)
    }

    function yy(n) {
      Bm.currentContext.clientBuffers[n].enabled = !1, lg.disableVertexAttribArray(n)
    }

    function vy(n, e, i) {
      Bm.preDrawHandleClientVertexAttribBindings(e + i), lg.drawArrays(n, e, i), Bm.postDrawHandleClientVertexAttribBindings()
    }

    function _y(n, e, i, t) {
      lg.drawArraysInstanced(n, e, i, t)
    }
    var gy = [];

    function hy(n, e) {
      for (var i = gy[n], t = 0; t < n; t++) i[t] = Z[e + 4 * t >> 2];
      lg.drawBuffers(i)
    }

    function wy(n, e, i, t) {
      var r;
      if (!lg.currentElementArrayBufferBinding) {
        var o = Bm.calcBufLength(1, i, 0, e);
        r = Bm.getTempIndexBuffer(o), lg.bindBuffer(34963, r), lg.bufferSubData(34963, 0, V.subarray(t, t + o)), t = 0
      }
      Bm.preDrawHandleClientVertexAttribBindings(e), lg.drawElements(n, e, i, t), Bm.postDrawHandleClientVertexAttribBindings(e), lg.currentElementArrayBufferBinding || lg.bindBuffer(34963, null)
    }

    function Sy(n, e, i, t, r) {
      lg.drawElementsInstanced(n, e, i, t, r)
    }

    function Cy(n) {
      lg.enable(n)
    }

    function Ey(n) {
      Bm.currentContext.clientBuffers[n].enabled = !0, lg.enableVertexAttribArray(n)
    }

    function by(n) {
      lg.endQuery(n)
    }

    function Wy() {
      lg.endTransformFeedback()
    }

    function Dy(n, e) {
      var i = lg.fenceSync(n, e);
      if (i) {
        var t = Bm.getNewId(Bm.syncs);
        return i.name = t, Bm.syncs[t] = i, t
      }
      return 0
    }

    function Ay() {
      lg.finish()
    }

    function ky() {
      lg.flush()
    }

    function My(n) {
      switch (n) {
        case 34962:
          n = 34964;
          break;
        case 34963:
          n = 34965;
          break;
        case 35051:
          n = 35053;
          break;
        case 35052:
          n = 35055;
          break;
        case 35982:
          n = 35983;
          break;
        case 36662:
          n = 36662;
          break;
        case 36663:
          n = 36663;
          break;
        case 35345:
          n = 35368
      }
      var e = lg.getParameter(n);
      return e ? 0 | e.name : 0
    }

    function xy(n) {
      switch (n) {
        case 34962:
        case 34963:
        case 36662:
        case 36663:
        case 35051:
        case 35052:
        case 35882:
        case 35982:
        case 35345:
          return !0;
        default:
          return !1
      }
    }

    function Xy(n, e, i) {
      if (!xy(n)) return Bm.recordError(1280), void x("GL_INVALID_ENUM in glFlushMappedBufferRange");
      var t = Bm.mappedBuffers[My(n)];
      return t ? 16 & t.access ? e < 0 || i < 0 || e + i > t.length ? (Bm.recordError(1281), void x("invalid range in glFlushMappedBufferRange")) : void lg.bufferSubData(n, t.offset, V.subarray(t.mem + e, t.mem + e + i)) : (Bm.recordError(1282), void x("buffer was not mapped with GL_MAP_FLUSH_EXPLICIT_BIT in glFlushMappedBufferRange")) : (Bm.recordError(1282), void x("buffer was never mapped in glFlushMappedBufferRange"))
    }

    function jy(n, e, i, t) {
      lg.framebufferRenderbuffer(n, e, i, Bm.renderbuffers[t])
    }

    function Ty(n, e, i, t, r) {
      lg.framebufferTexture2D(n, e, i, Bm.textures[t], r)
    }

    function Ly(n, e, i, t, r) {
      lg.framebufferTextureLayer(n, e, Bm.textures[i], t, r)
    }

    function Fy(n) {
      lg.frontFace(n)
    }

    function Py(n, e, i, t) {
      for (var r = 0; r < n; r++) {
        var o = lg[i](),
          a = o && Bm.getNewId(t);
        o ? (o.name = a, t[a] = o) : Bm.recordError(1282), Z[e + 4 * r >> 2] = a
      }
    }

    function Ry(n, e) {
      Py(n, e, "createBuffer", Bm.buffers)
    }

    function By(n, e) {
      Py(n, e, "createFramebuffer", Bm.framebuffers)
    }

    function Gy(n, e) {
      Py(n, e, "createQuery", Bm.queries)
    }

    function Oy(n, e) {
      Py(n, e, "createRenderbuffer", Bm.renderbuffers)
    }

    function Iy(n, e) {
      Py(n, e, "createSampler", Bm.samplers)
    }

    function Ky(n, e) {
      for (var i = 0; i < n; i++) {
        var t = lg.createTexture();
        if (!t) {
          for (Bm.recordError(1282); i < n;) Z[e + 4 * i++ >> 2] = 0;
          return
        }
        var r = Bm.getNewId(Bm.textures);
        t.name = r, Bm.textures[r] = t, window._lastTextureId = r, Z[e + 4 * i >> 2] = r
      }
    }

    function Ny(n, e) {
      Py(n, e, "createTransformFeedback", Bm.transformFeedbacks)
    }

    function Uy(n, e) {
      Py(n, e, "createVertexArray", Bm.vaos)
    }

    function zy(n) {
      lg.generateMipmap(n)
    }

    function qy(n, e, i, t, r, o, a, l) {
      e = Bm.programs[e];
      var u = lg[n](e, i);
      if (u) {
        var f = l && an(u.name, l, t);
        r && (Z[r >> 2] = f), o && (Z[o >> 2] = u.size), a && (Z[a >> 2] = u.type)
      }
    }

    function Hy(n, e, i, t, r, o, a) {
      qy("getActiveAttrib", n, e, i, t, r, o, a)
    }

    function Vy(n, e, i, t, r, o, a) {
      qy("getActiveUniform", n, e, i, t, r, o, a)
    }

    function Yy(n, e, i, t, r) {
      n = Bm.programs[n];
      var o = lg.getActiveUniformBlockName(n, e);
      if (o)
        if (r && i > 0) {
          var a = an(o, r, i);
          t && (Z[t >> 2] = a)
        } else t && (Z[t >> 2] = 0)
    }

    function Jy(n, e, i, t) {
      if (t)
        if (n = Bm.programs[n], 35393 != i) {
          var r = lg.getActiveUniformBlockParameter(n, e, i);
          if (null !== r)
            if (35395 == i)
              for (var o = 0; o < r.length; o++) Z[t + 4 * o >> 2] = r[o];
            else Z[t >> 2] = r
        } else {
          var a = lg.getActiveUniformBlockName(n, e);
          Z[t >> 2] = a.length + 1
        }
      else Bm.recordError(1281)
    }

    function Zy(n, e, i, t, r) {
      if (r)
        if (e > 0 && 0 == i) Bm.recordError(1281);
        else {
          n = Bm.programs[n];
          for (var o = [], a = 0; a < e; a++) o.push(Z[i + 4 * a >> 2]);
          var l = lg.getActiveUniforms(n, o, t);
          if (l) {
            var u = l.length;
            for (a = 0; a < u; a++) Z[r + 4 * a >> 2] = l[a]
          }
        }
      else Bm.recordError(1281)
    }

    function Qy(n, e) {
      return lg.getAttribLocation(Bm.programs[n], rn(e))
    }

    function $y(n, e, i, t) {
      t ? lg.getBufferSubData(n, e, V, t, i) : Bm.recordError(1281)
    }

    function nv() {
      var n = lg.getError() || Bm.lastError;
      return Bm.lastError = 0, n
    }

    function ev(n, e, i, t) {
      var r = lg.getFramebufferAttachmentParameter(n, e, i);
      (r instanceof WebGLRenderbuffer || r instanceof WebGLTexture) && (r = 0 | r.name), Z[t >> 2] = r
    }

    function iv(n, e) {
      Q[n >> 2] = e, Q[n + 4 >> 2] = (e - Q[n >> 2]) / 4294967296
    }

    function tv(n, e, i, t) {
      if (i) {
        var r, o = lg.getIndexedParameter(n, e);
        switch (typeof o) {
          case "boolean":
            r = o ? 1 : 0;
            break;
          case "number":
            r = o;
            break;
          case "object":
            if (null === o) switch (n) {
              case 35983:
              case 35368:
                r = 0;
                break;
              default:
                return void Bm.recordError(1280)
            } else {
              if (!(o instanceof WebGLBuffer)) return void Bm.recordError(1280);
              r = 0 | o.name
            }
            break;
          default:
            return void Bm.recordError(1280)
        }
        switch (t) {
          case 1:
            iv(i, r);
            break;
          case 0:
            Z[i >> 2] = r;
            break;
          case 2:
            $[i >> 2] = r;
            break;
          case 4:
            H[i >> 0] = r ? 1 : 0;
            break;
          default:
            throw "internal emscriptenWebGLGetIndexed() error, bad type: " + t
        }
      } else Bm.recordError(1281)
    }

    function rv(n, e, i) {
      tv(n, e, i, 0)
    }

    function ov(n, e, i) {
      if (e) {
        var t = void 0;
        switch (n) {
          case 36346:
            t = 1;
            break;
          case 36344:
            return void(0 != i && 1 != i && Bm.recordError(1280));
          case 34814:
          case 36345:
            t = 0;
            break;
          case 34466:
            var r = lg.getParameter(34467);
            t = r ? r.length : 0;
            break;
          case 33390:
            t = 1048576;
            break;
          case 33309:
            if (Bm.currentContext.version < 2) return void Bm.recordError(1282);
            var o = lg.getSupportedExtensions() || [];
            GameGlobal.USED_TEXTURE_COMPRESSION && (o.push("WEBGL_compressed_texture_etc1"), o.push("WEBGL_compressed_texture_etc")), t = 2 * o.length;
            break;
          case 33307:
          case 33308:
            if (Bm.currentContext.version < 2) return void Bm.recordError(1280);
            t = 33307 == n ? 3 : 0
        }
        if (void 0 === t) {
          var a = lg.getParameter(n);
          switch (typeof a) {
            case "number":
              t = a;
              break;
            case "boolean":
              t = a ? 1 : 0;
              break;
            case "string":
              return void Bm.recordError(1280);
            case "object":
              if (null === a) switch (n) {
                case 34964:
                case 35725:
                case 34965:
                case 36006:
                case 36007:
                case 32873:
                case 34229:
                case 36662:
                case 36663:
                case 35053:
                case 35055:
                case 36010:
                case 35097:
                case 35869:
                case 32874:
                case 36389:
                case 35983:
                case 35368:
                case 34068:
                  t = 0;
                  break;
                default:
                  return void Bm.recordError(1280)
              } else {
                if (a instanceof Float32Array || a instanceof Uint32Array || a instanceof Int32Array || a instanceof Array) {
                  for (var l = 0; l < a.length; ++l) switch (i) {
                    case 0:
                      Z[e + 4 * l >> 2] = a[l];
                      break;
                    case 2:
                      $[e + 4 * l >> 2] = a[l];
                      break;
                    case 4:
                      H[e + l >> 0] = a[l] ? 1 : 0
                  }
                  return
                }
                try {
                  t = 0 | a.name
                } catch (e) {
                  return Bm.recordError(1280), void x("GL_INVALID_ENUM in glGet" + i + "v: Unknown object returned from WebGL getParameter(" + n + ")! (error: " + e + ")")
                }
              }
              break;
            default:
              return Bm.recordError(1280), void x("GL_INVALID_ENUM in glGet" + i + "v: Native code calling glGet" + i + "v(" + n + ") and it returns " + a + " of type " + typeof a + "!")
          }
        }
        switch (i) {
          case 1:
            iv(e, t);
            break;
          case 0:
            Z[e >> 2] = t;
            break;
          case 2:
            $[e >> 2] = t;
            break;
          case 4:
            H[e >> 0] = t ? 1 : 0
        }
      } else Bm.recordError(1281)
    }

    function av(n, e) {
      ov(n, e, 0)
    }

    function lv(n, e, i, t, r) {
      if (t < 0) Bm.recordError(1281);
      else if (r) {
        var o = lg.getInternalformatParameter(n, e, i);
        if (null !== o)
          for (var a = 0; a < o.length && a < t; ++a) Z[r + 4 * a >> 2] = o[a]
      } else Bm.recordError(1281)
    }

    function uv(n, e, i, t, r) {
      Bm.recordError(1282)
    }

    function fv(n, e, i, t) {
      var r = lg.getProgramInfoLog(Bm.programs[n]);
      null === r && (r = "(unknown error)");
      var o = e > 0 && t ? an(r, t, e) : 0;
      i && (Z[i >> 2] = o)
    }

    function cv(n, e, i) {
      if (i)
        if (n >= Bm.counter) Bm.recordError(1281);
        else if (n = Bm.programs[n], 35716 == e) {
        var t = lg.getProgramInfoLog(n);
        null === t && (t = "(unknown error)"), Z[i >> 2] = t.length + 1
      } else if (35719 == e) {
        if (!n.maxUniformLength)
          for (var r = 0; r < lg.getProgramParameter(n, 35718); ++r) n.maxUniformLength = Math.max(n.maxUniformLength, lg.getActiveUniform(n, r).name.length + 1);
        Z[i >> 2] = n.maxUniformLength
      } else if (35722 == e) {
        if (!n.maxAttributeLength)
          for (r = 0; r < lg.getProgramParameter(n, 35721); ++r) n.maxAttributeLength = Math.max(n.maxAttributeLength, lg.getActiveAttrib(n, r).name.length + 1);
        Z[i >> 2] = n.maxAttributeLength
      } else if (35381 == e) {
        if (!n.maxUniformBlockNameLength)
          for (r = 0; r < lg.getProgramParameter(n, 35382); ++r) n.maxUniformBlockNameLength = Math.max(n.maxUniformBlockNameLength, lg.getActiveUniformBlockName(n, r).length + 1);
        Z[i >> 2] = n.maxUniformBlockNameLength
      } else Z[i >> 2] = lg.getProgramParameter(n, e);
      else Bm.recordError(1281)
    }

    function sv(n, e, i) {
      if (i) {
        var t, r = Bm.queries[n],
          o = lg.getQueryParameter(r, e);
        t = "boolean" == typeof o ? o ? 1 : 0 : o, Z[i >> 2] = t
      } else Bm.recordError(1281)
    }

    function dv(n, e, i) {
      i ? Z[i >> 2] = lg.getQuery(n, e) : Bm.recordError(1281)
    }

    function mv(n, e, i) {
      i ? Z[i >> 2] = lg.getRenderbufferParameter(n, e) : Bm.recordError(1281)
    }

    function pv(n, e, i, t) {
      var r = lg.getShaderInfoLog(Bm.shaders[n]);
      null === r && (r = "(unknown error)");
      var o = e > 0 && t ? an(r, t, e) : 0;
      i && (Z[i >> 2] = o)
    }

    function yv(n, e, i, t) {
      var r = lg.getShaderPrecisionFormat(n, e);
      Z[i >> 2] = r.rangeMin, Z[i + 4 >> 2] = r.rangeMax, Z[t >> 2] = r.precision
    }

    function vv(n, e, i, t) {
      var r = lg.getShaderSource(Bm.shaders[n]);
      if (r) {
        var o = e > 0 && t ? an(r, t, e) : 0;
        i && (Z[i >> 2] = o)
      }
    }

    function _v(n, e, i) {
      if (i)
        if (35716 == e) {
          var t = lg.getShaderInfoLog(Bm.shaders[n]);
          null === t && (t = "(unknown error)");
          var r = t ? t.length + 1 : 0;
          Z[i >> 2] = r
        } else if (35720 == e) {
        var o = lg.getShaderSource(Bm.shaders[n]),
          a = o ? o.length + 1 : 0;
        Z[i >> 2] = a
      } else Z[i >> 2] = lg.getShaderParameter(Bm.shaders[n], e);
      else Bm.recordError(1281)
    }

    function gv(n) {
      var e = Bm.stringCache[n];
      if (!e) {
        switch (n) {
          case 7939:
            var i = lg.getSupportedExtensions() || [];
            GameGlobal.USED_TEXTURE_COMPRESSION && (i.push("WEBGL_compressed_texture_etc1"), i.push("WEBGL_compressed_texture_etc")), e = Ve((i = i.concat(i.map((function(n) {
              return "GL_" + n
            })))).join(" "));
            break;
          case 7936:
          case 7937:
          case 37445:
          case 37446:
            var t = lg.getParameter(n);
            t || Bm.recordError(1280), e = t && Ve(t);
            break;
          case 7938:
            var r = lg.getParameter(7938);
            e = Ve(r = Bm.currentContext.version >= 2 ? "OpenGL ES 3.0 (" + r + ")" : "OpenGL ES 2.0 (" + r + ")");
            break;
          case 35724:
            var o = lg.getParameter(35724),
              a = o.match(/^WebGL GLSL ES ([0-9]\.[0-9][0-9]?)(?:$| .*)/);
            null !== a && (3 == a[1].length && (a[1] = a[1] + "0"), o = "OpenGL ES GLSL ES " + a[1] + " (" + o + ")"), e = Ve(o);
            break;
          default:
            Bm.recordError(1280)
        }
        Bm.stringCache[n] = e
      }
      return e
    }

    function hv(n, e) {
      if (Bm.currentContext.version < 2) return Bm.recordError(1282), 0;
      var i = Bm.stringiCache[n];
      if (i) return e < 0 || e >= i.length ? (Bm.recordError(1281), 0) : i[e];
      switch (n) {
        case 7939:
          var t = lg.getSupportedExtensions() || [];
          return GameGlobal.USED_TEXTURE_COMPRESSION && (t.push("WEBGL_compressed_texture_etc1"), t.push("WEBGL_compressed_texture_etc")), t = (t = t.concat(t.map((function(n) {
            return "GL_" + n
          })))).map((function(n) {
            return Ve(n)
          })), i = Bm.stringiCache[n] = t, e < 0 || e >= i.length ? (Bm.recordError(1281), 0) : i[e];
        default:
          return Bm.recordError(1280), 0
      }
    }

    function wv(n, e, i) {
      i ? Z[i >> 2] = lg.getTexParameter(n, e) : Bm.recordError(1281)
    }

    function Sv(n, e) {
      return lg.getUniformBlockIndex(Bm.programs[n], rn(e))
    }

    function Cv(n, e, i, t) {
      if (t)
        if (e > 0 && (0 == i || 0 == t)) Bm.recordError(1281);
        else {
          n = Bm.programs[n];
          for (var r = [], o = 0; o < e; o++) r.push(rn(Z[i + 4 * o >> 2]));
          var a = lg.getUniformIndices(n, r);
          if (a) {
            var l = a.length;
            for (o = 0; o < l; o++) Z[t + 4 * o >> 2] = a[o]
          }
        }
      else Bm.recordError(1281)
    }

    function Ev(n, e) {
      function i(n) {
        return "]" == n.slice(-1) && n.lastIndexOf("[")
      }
      if (e = rn(e), n = Bm.programs[n]) {
        var t, r, o = n.uniformLocsById,
          a = n.uniformSizeAndIdsByName,
          l = 0,
          u = e,
          f = i(e);
        if (!o)
          for (n.uniformLocsById = o = {}, n.uniformArrayNamesById = {}, t = 0; t < lg.getProgramParameter(n, 35718); ++t) {
            var c = lg.getActiveUniform(n, t),
              s = c.name,
              d = c.size,
              m = i(s),
              p = m > 0 ? s.slice(0, m) : s,
              y = a[p] ? a[p][1] : n.uniformIdCounter;
            for (n.uniformIdCounter = Math.max(y + d, n.uniformIdCounter), a[p] = [d, y], r = 0; r < d; ++r) o[y] = r, n.uniformArrayNamesById[y++] = p
          }
        f > 0 && (l = Gc(e.slice(f + 1)) >>> 0, u = e.slice(0, f));
        var v = a[u];
        if (v && l < v[0] && (o[l += v[1]] = o[l] || lg.getUniformLocation(n, e))) return l
      } else Bm.recordError(1281);
      return -1
    }

    function bv(n) {
      var e = lg.currentProgram;
      if (e) {
        var i = e.uniformLocsById[n];
        return "number" == typeof i && (e.uniformLocsById[n] = i = lg.getUniformLocation(e, e.uniformArrayNamesById[n] + (i > 0 ? "[" + i + "]" : ""))), i
      }
      Bm.recordError(1282)
    }

    function Wv(n, e, i, t) {
      if (i) {
        n = Bm.programs[n];
        var r = lg.getUniform(n, bv(e));
        if ("number" == typeof r || "boolean" == typeof r) switch (t) {
          case 0:
            Z[i >> 2] = r;
            break;
          case 2:
            $[i >> 2] = r
        } else
          for (var o = 0; o < r.length; o++) switch (t) {
            case 0:
              Z[i + 4 * o >> 2] = r[o];
              break;
            case 2:
              $[i + 4 * o >> 2] = r[o]
          }
      } else Bm.recordError(1281)
    }

    function Dv(n, e, i) {
      Wv(n, e, i, 0)
    }

    function Av(n, e, i, t) {
      if (i) {
        Bm.currentContext.clientBuffers[n].enabled && x("glGetVertexAttrib*v on client-side array: not supported, bad data returned");
        var r = lg.getVertexAttrib(n, e);
        if (34975 == e) Z[i >> 2] = r && r.name;
        else if ("number" == typeof r || "boolean" == typeof r) switch (t) {
          case 0:
            Z[i >> 2] = r;
            break;
          case 2:
            $[i >> 2] = r;
            break;
          case 5:
            Z[i >> 2] = Math.fround(r)
        } else
          for (var o = 0; o < r.length; o++) switch (t) {
            case 0:
              Z[i + 4 * o >> 2] = r[o];
              break;
            case 2:
              $[i + 4 * o >> 2] = r[o];
              break;
            case 5:
              Z[i + 4 * o >> 2] = Math.fround(r[o])
          }
      } else Bm.recordError(1281)
    }

    function kv(n, e, i) {
      Av(n, e, i, 5)
    }

    function Mv(n, e, i) {
      for (var t = gy[e], r = 0; r < e; r++) t[r] = Z[i + 4 * r >> 2];
      lg.invalidateFramebuffer(n, t)
    }

    function xv(n) {
      return lg.isEnabled(n)
    }

    function Xv(n) {
      var e = Bm.vaos[n];
      return e ? lg.isVertexArray(e) : 0
    }

    function jv(n) {
      function e(n, e) {
        Object.keys(e).forEach((function(i) {
          n[i] = e[i]
        }))
      }
      n = Bm.programs[n], lg.linkProgram(n), n.uniformLocsById = 0, n.uniformSizeAndIdsByName = {}, [n.vs, n.fs].forEach((function(e) {
        Object.keys(e.explicitUniformLocations).forEach((function(i) {
          var t = e.explicitUniformLocations[i];
          n.uniformSizeAndIdsByName[i] = [1, t], n.uniformIdCounter = Math.max(n.uniformIdCounter, t + 1)
        }))
      })), n.explicitUniformBindings = {}, n.explicitSamplerBindings = {}, [n.vs, n.fs].forEach((function(i) {
        e(n.explicitUniformBindings, i.explicitUniformBindings), e(n.explicitSamplerBindings, i.explicitSamplerBindings)
      })), n.explicitProgramBindingsApplied = 0
    }

    function Tv(n, e, i, t) {
      if (26 != t && 10 != t) return x("glMapBufferRange is only supported when access is MAP_WRITE|INVALIDATE_BUFFER"), 0;
      if (!xy(n)) return Bm.recordError(1280), x("GL_INVALID_ENUM in glMapBufferRange"), 0;
      var r = Lg(i);
      return r ? (Bm.mappedBuffers[My(n)] = {
        offset: e,
        length: i,
        mem: r,
        access: t
      }, r) : 0
    }

    function Lv(n, e) {
      3317 == n && (Bm.unpackAlignment = e), lg.pixelStorei(n, e)
    }

    function Fv(n, e) {
      lg.polygonOffset(n, e)
    }

    function Pv(n, e, i, t) {
      Bm.recordError(1280)
    }

    function Rv(n, e, i) {
      Bm.recordError(1280)
    }

    function Bv(n) {
      lg.readBuffer(n)
    }

    function Gv(n, e, i, t) {
      var r;
      return e * (n * i + (r = t) - 1 & -r)
    }

    function Ov(n) {
      return {
        5: 3,
        6: 4,
        8: 2,
        29502: 3,
        29504: 4,
        26917: 2,
        26918: 2,
        29846: 3,
        29847: 4
      } [n - 6402] || 1
    }

    function Iv(n) {
      return 0 == (n -= 5120) ? H : 1 == n ? V : 2 == n ? Y : 4 == n ? Z : 6 == n ? $ : 5 == n || 28922 == n || 28520 == n || 30779 == n || 30782 == n ? Q : J
    }

    function Kv(n) {
      return 31 - Math.clz32(n.BYTES_PER_ELEMENT)
    }

    function Nv(n, e, i, t, r, o) {
      var a = Iv(n),
        l = Kv(a),
        u = 1 << l,
        f = Gv(i, t, Ov(e) * u, Bm.unpackAlignment);
      return a.subarray(r >> l, r + f >> l)
    }

    function Uv(n, e, i, t, r, o, a) {
      if (Bm.currentContext.version >= 2)
        if (lg.currentPixelPackBufferBinding) lg.readPixels(n, e, i, t, r, o, a);
        else {
          Iv(o);
          lg.readPixels(n, e, i, t, r, o, Nv(o, r, i, t, a))
        }
      else {
        var l = Nv(o, r, i, t, a);
        l ? lg.readPixels(n, e, i, t, r, o, l) : Bm.recordError(1280)
      }
    }

    function zv(n, e, i, t) {
      lg.renderbufferStorage(n, e, i, t)
    }

    function qv(n, e, i, t, r) {
      lg.renderbufferStorageMultisample(n, e, i, t, r)
    }

    function Hv(n, e, i) {
      lg.samplerParameteri(Bm.samplers[n], e, i)
    }

    function Vv(n, e, i, t) {
      lg.scissor(n, e, i, t)
    }

    function Yv(n, e, i = "(", t = ")") {
      for (var r = 0; e < n.length; ++e)
        if (n[e] == i && ++r, n[e] == t && 0 == --r) return e
    }

    function Jv(n) {
      var e = 0,
        i = n.length,
        t = "",
        r = [1],
        o = {
          defined: function(n) {
            return o[n[0]] ? 1 : 0
          },
          GL_FRAGMENT_PRECISION_HIGH: function() {
            return 1
          }
        };

      function a(n, e) {
        return !(n.charCodeAt(e) > 32)
      }

      function l(n, e) {
        for (; !a(n, e);) ++e;
        return e
      }

      function u(n, e) {
        var i = n.charCodeAt(e);
        return i > 32 ? i < 48 ? 1 : i < 58 ? 2 : i < 65 ? 1 : i < 91 || 95 == i ? 3 : i < 97 ? 1 : i < 123 ? 3 : 1 : i < 33 ? 0 : 4
      }

      function f(n, e) {
        for (var i = [], t = n.length, r = 0; r <= t; ++r) {
          var o = u(n, r);
          if (2 == o || 3 == o)
            for (var a = r + 1; a <= t; ++a) {
              var l = u(n, a);
              if (l != o && (2 != l || 3 != o)) {
                i.push(n.substring(r, a)), r = a - 1;
                break
              }
            } else if (1 == o) {
              var f = n.substr(r, 2);
              ["<=", ">=", "==", "!=", "&&", "||"].includes(f) ? (i.push(f), ++r) : i.push(n[r])
            }
        }
        return i
      }

      function c(n, e, i) {
        void 0 === i && (i = n.length);
        n.length;
        for (var t = "", r = e; r < i; ++r) {
          if (3 == u(n, r))
            for (var a = r + 1; a <= i; ++a) {
              var l = u(n, a);
              if (2 != l && 3 != l) {
                var f = n.substring(r, a),
                  s = o[f];
                if (s) {
                  var d = n.substring(e, r);
                  if (s.length && "(" == n[a]) {
                    var m = Yv(n, a);
                    d += s(n.substring(a + 1, m).split(",")) + n.substring(m + 1, i)
                  } else d += s() + n.substring(a, i);
                  return c(d, 0)
                }
                t += f, r = a - 1;
                break
              }
            } else t += n[r]
        }
        return t
      }

      function s(n) {
        for (; n.length > 1 || "function" != typeof n[0];) n = function(n) {
          var e, i, t, r = -2;
          for (t = 0; t < n.length; ++t)(i = ["*", "/", "+", "-", "!", "<", "<=", ">", ">=", "==", "!=", "&&", "||", "("].indexOf(n[t])) > r && (e = t, r = i);
          if (13 == r && (t = Yv(n, e))) return n.splice(e, t + 1 - e, s(n.slice(e + 1, t))), n;
          if (4 == r) {
            e = n.lastIndexOf("!");
            var o = s(n.slice(e + 1, e + 2));
            return n.splice(e, 2, (function() {
              return !o()
            })), n
          }
          if (r >= 0) {
            var a = s(n.slice(0, e)),
              l = s(n.slice(e + 1));
            switch (n[e]) {
              case "&&":
                return [function() {
                  return a() && l()
                }];
              case "||":
                return [function() {
                  return a() || l()
                }];
              case "==":
                return [function() {
                  return a() == l()
                }];
              case "!=":
                return [function() {
                  return a() != l()
                }];
              case "<":
                return [function() {
                  return a() < l()
                }];
              case "<=":
                return [function() {
                  return a() <= l()
                }];
              case ">":
                return [function() {
                  return a() > l()
                }];
              case ">=":
                return [function() {
                  return a() >= l()
                }];
              case "+":
                return [function() {
                  return a() + l()
                }];
              case "-":
                return [function() {
                  return a() - l()
                }];
              case "*":
                return [function() {
                  return a() * l()
                }];
              case "/":
                return [function() {
                  return Math.floor(a() / l())
                }]
            }
          }
          var u = Gc(n[e]);
          return [function() {
            return u
          }]
        }(n);
        return n[0]
      }
      for (; e < i; ++e) {
        var d = e;
        (e = n.indexOf("\n", e)) < 0 && (e = i);
        for (var m = d; m < e && a(n, m); ++m);
        var p = r[r.length - 1];
        if ("#" == n[m]) {
          var y = l(n, m),
            v = n.substring(m + 1, y),
            _ = n.substring(y, e).trim();
          switch (v) {
            case "if":
              var g = s(f(c(_, 0)))();
              r.push(!!g * r[r.length - 1]);
              break;
            case "ifdef":
              r.push(!!o[_] * r[r.length - 1]);
              break;
            case "ifndef":
              r.push(!o[_] * r[r.length - 1]);
              break;
            case "else":
              r[r.length - 1] = 1 - r[r.length - 1];
              break;
            case "endif":
              r.pop();
              break;
            case "define":
              if (p) {
                var h = _.indexOf("("),
                  w = l(_, 0);
                if (w < h && (h = 0), h > 0) {
                  var S = _.indexOf(")", h);
                  let n = _.substring(h + 1, S).split(",").map(n => n.trim()),
                    e = f(_.substring(S + 1).trim());
                  o[_.substring(0, h)] = function(i) {
                    var t = "";
                    return e.forEach(e => {
                      var r = n.indexOf(e);
                      t += r >= 0 ? i[r] : e
                    }), t
                  }
                } else {
                  let n = c(_.substring(w + 1).trim(), 0);
                  o[_.substring(0, w)] = function() {
                    return n
                  }
                }
              }
              break;
            case "undef":
              p && delete o[_];
              break;
            default:
              t += c(n, d, e) + "\n"
          }
        } else p && (t += c(n, d, e) + "\n")
      }
      return t
    }

    function Zv(n) {
      for (var e, i, t = 0, r = "", o = n.length; t < o; ++t)
        if ("/" == (e = n[t]))
          if ("/" == (i = n[t + 1]))
            for (; t < o && "\n" != n[t + 1];) ++t;
          else if ("*" == i)
        for (; t < o && ("*" != n[t - 1] || "/" != n[t]);) ++t;
      else r += e;
      else r += e;
      return r
    }

    function Qv(n, e, i, t) {
      var r = Bm.getSource(n, e, i, t);
      r = Jv(Zv(r));
      for (var o, a = /layout\s*\(\s*location\s*=\s*(-?\d+)\s*\)\s*(uniform\s+((lowp|mediump|highp)\s+)?\w+\s+(\w+))/g, l = {}; o = a.exec(r);)
        if (l[o[5]] = Gc(o[1]), !(l[o[5]] >= 0 && l[o[5]] < 1048576)) return x('Specified an out of range layout(location=x) directive "' + l[o[5]] + '"! (' + o[0] + ")"), void Bm.recordError(1281);
      r = r.replace(a, "$2"), Bm.shaders[n].explicitUniformLocations = l;
      for (var u, f = /layout\s*\(.*?binding\s*=\s*(-?\d+).*?\)\s*uniform\s+(\w+)\s+(\w+)?/g, c = {}, s = {}; u = f.exec(r);) {
        for (var d = 1, m = u.index; m < r.length && ";" != r[m]; ++m) {
          if ("[" == r[m]) {
            d = Gc(r.slice(m + 1));
            break
          }
          "{" == r[m] && (m = Yv(r, m, "{", "}") - 1)
        }
        var p = Gc(u[1]),
          y = 34930;
        u[3] && -1 != u[2].indexOf("sampler") ? c[u[3]] = [p, d] : (y = 35374, s[u[2]] = [p, d]);
        var v = lg.getParameter(y);
        if (!(p >= 0 && p + d <= v)) return x('Specified an out of range layout(binding=x) directive "' + p + '"! (' + u[0] + "). Valid range is [0, " + v + "-1]"), void Bm.recordError(1281)
      }
      r = (r = (r = r.replace(/layout\s*\(.*?binding\s*=\s*([-\d]+).*?\)/g, "")).replace(/(layout\s*\((.*?)),\s*binding\s*=\s*([-\d]+)\)/g, "$1)")).replace(/layout\s*\(\s*binding\s*=\s*([-\d]+)\s*,(.*?)\)/g, "layout($2)"), Bm.shaders[n].explicitSamplerBindings = c, Bm.shaders[n].explicitUniformBindings = s, lg.shaderSource(Bm.shaders[n], r)
    }

    function $v(n, e, i, t) {
      lg.stencilFuncSeparate(n, e, i, t)
    }

    function n_(n) {
      lg.stencilMask(n)
    }

    function e_(n, e, i, t) {
      lg.stencilOpSeparate(n, e, i, t)
    }

    function i_(n, e, i, t, r, o, a, l, u) {
      if (Bm.currentContext.version >= 2)
        if (lg.currentPixelUnpackBufferBinding) lg.texImage2D(n, e, i, t, r, o, a, l, u);
        else if (u) {
        Iv(l);
        lg.texImage2D(n, e, i, t, r, o, a, l, Nv(l, a, t, r, u))
      } else lg.texImage2D(n, e, i, t, r, o, a, l, null);
      else lg.texImage2D(n, e, i, t, r, o, a, l, u ? Nv(l, a, t, r, u) : null)
    }

    function t_(n, e, i, t, r, o, a, l, u, f) {
      if (lg.currentPixelUnpackBufferBinding) lg.texImage3D(n, e, i, t, r, o, a, l, u, f);
      else if (f) {
        Iv(u);
        lg.texImage3D(n, e, i, t, r, o, a, l, u, Nv(u, l, t, r, f))
      } else lg.texImage3D(n, e, i, t, r, o, a, l, u, null)
    }

    function r_(n, e, i) {
      lg.texParameterf(n, e, i)
    }

    function o_(n, e, i) {
      lg.texParameteri(n, e, i)
    }

    function a_(n, e, i) {
      var t = Z[i >> 2];
      lg.texParameteri(n, e, t)
    }

    function l_(n, e, i, t, r) {
      window._lastTexStorage2DParams = [n, e, i, t, r], 36196 != i && 37492 != i && 37493 != i && lg.texStorage2D(n, e, i, t, r)
    }

    function u_(n, e, i, t, r, o) {
      lg.texStorage3D(n, e, i, t, r, o)
    }

    function f_(n, e, i, t, r, o, a, l, u) {
      if (Bm.currentContext.version >= 2)
        if (lg.currentPixelUnpackBufferBinding) lg.texSubImage2D(n, e, i, t, r, o, a, l, u);
        else if (u) {
        Iv(l);
        lg.texSubImage2D(n, e, i, t, r, o, a, l, Nv(l, a, r, o, u))
      } else lg.texSubImage2D(n, e, i, t, r, o, a, l, null);
      else {
        var f = null;
        u && (f = Nv(l, a, r, o, u)), lg.texSubImage2D(n, e, i, t, r, o, a, l, f)
      }
    }

    function c_(n, e, i, t, r, o, a, l, u, f, c) {
      if (lg.currentPixelUnpackBufferBinding) lg.texSubImage3D(n, e, i, t, r, o, a, l, u, f, c);
      else if (c) {
        Iv(f);
        lg.texSubImage3D(n, e, i, t, r, o, a, l, u, f, Nv(f, u, o, a, c))
      } else lg.texSubImage3D(n, e, i, t, r, o, a, l, u, f, null)
    }

    function s_(n, e, i, t) {
      n = Bm.programs[n];
      for (var r = [], o = 0; o < e; o++) r.push(rn(Z[i + 4 * o >> 2]));
      lg.transformFeedbackVaryings(n, r, t)
    }
    var d_ = [];

    function m_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform1fv(bv(n), $, i >> 2, e);
      else {
        if (e <= 288)
          for (var t = d_[e - 1], r = 0; r < e; ++r) t[r] = $[i + 4 * r >> 2];
        else t = $.subarray(i >> 2, i + 4 * e >> 2);
        lg.uniform1fv(bv(n), t)
      }
    }

    function p_(n, e) {
      lg.uniform1i(bv(n), e)
    }
    var y_ = [];

    function v_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform1iv(bv(n), Z, i >> 2, e);
      else {
        if (e <= 288)
          for (var t = y_[e - 1], r = 0; r < e; ++r) t[r] = Z[i + 4 * r >> 2];
        else t = Z.subarray(i >> 2, i + 4 * e >> 2);
        lg.uniform1iv(bv(n), t)
      }
    }

    function __(n, e, i) {
      lg.uniform1uiv(bv(n), Q, i >> 2, e)
    }

    function g_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform2fv(bv(n), $, i >> 2, 2 * e);
      else {
        if (e <= 144)
          for (var t = d_[2 * e - 1], r = 0; r < 2 * e; r += 2) t[r] = $[i + 4 * r >> 2], t[r + 1] = $[i + (4 * r + 4) >> 2];
        else t = $.subarray(i >> 2, i + 8 * e >> 2);
        lg.uniform2fv(bv(n), t)
      }
    }

    function h_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform2iv(bv(n), Z, i >> 2, 2 * e);
      else {
        if (e <= 144)
          for (var t = y_[2 * e - 1], r = 0; r < 2 * e; r += 2) t[r] = Z[i + 4 * r >> 2], t[r + 1] = Z[i + (4 * r + 4) >> 2];
        else t = Z.subarray(i >> 2, i + 8 * e >> 2);
        lg.uniform2iv(bv(n), t)
      }
    }

    function w_(n, e, i) {
      lg.uniform2uiv(bv(n), Q, i >> 2, 2 * e)
    }

    function S_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform3fv(bv(n), $, i >> 2, 3 * e);
      else {
        if (e <= 96)
          for (var t = d_[3 * e - 1], r = 0; r < 3 * e; r += 3) t[r] = $[i + 4 * r >> 2], t[r + 1] = $[i + (4 * r + 4) >> 2], t[r + 2] = $[i + (4 * r + 8) >> 2];
        else t = $.subarray(i >> 2, i + 12 * e >> 2);
        lg.uniform3fv(bv(n), t)
      }
    }

    function C_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform3iv(bv(n), Z, i >> 2, 3 * e);
      else {
        if (e <= 96)
          for (var t = y_[3 * e - 1], r = 0; r < 3 * e; r += 3) t[r] = Z[i + 4 * r >> 2], t[r + 1] = Z[i + (4 * r + 4) >> 2], t[r + 2] = Z[i + (4 * r + 8) >> 2];
        else t = Z.subarray(i >> 2, i + 12 * e >> 2);
        lg.uniform3iv(bv(n), t)
      }
    }

    function E_(n, e, i) {
      lg.uniform3uiv(bv(n), Q, i >> 2, 3 * e)
    }

    function b_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform4fv(bv(n), $, i >> 2, 4 * e);
      else {
        if (e <= 72) {
          var t = d_[4 * e - 1],
            r = $;
          i >>= 2;
          for (var o = 0; o < 4 * e; o += 4) {
            var a = i + o;
            t[o] = r[a], t[o + 1] = r[a + 1], t[o + 2] = r[a + 2], t[o + 3] = r[a + 3]
          }
        } else t = $.subarray(i >> 2, i + 16 * e >> 2);
        lg.uniform4fv(bv(n), t)
      }
    }

    function W_(n, e, i) {
      if (Bm.currentContext.version >= 2) lg.uniform4iv(bv(n), Z, i >> 2, 4 * e);
      else {
        if (e <= 72)
          for (var t = y_[4 * e - 1], r = 0; r < 4 * e; r += 4) t[r] = Z[i + 4 * r >> 2], t[r + 1] = Z[i + (4 * r + 4) >> 2], t[r + 2] = Z[i + (4 * r + 8) >> 2], t[r + 3] = Z[i + (4 * r + 12) >> 2];
        else t = Z.subarray(i >> 2, i + 16 * e >> 2);
        lg.uniform4iv(bv(n), t)
      }
    }

    function D_(n, e, i) {
      lg.uniform4uiv(bv(n), Q, i >> 2, 4 * e)
    }

    function A_(n, e, i) {
      n = Bm.programs[n], lg.uniformBlockBinding(n, e, i)
    }

    function k_(n, e, i, t) {
      if (Bm.currentContext.version >= 2) lg.uniformMatrix3fv(bv(n), !!i, $, t >> 2, 9 * e);
      else {
        if (e <= 32)
          for (var r = d_[9 * e - 1], o = 0; o < 9 * e; o += 9) r[o] = $[t + 4 * o >> 2], r[o + 1] = $[t + (4 * o + 4) >> 2], r[o + 2] = $[t + (4 * o + 8) >> 2], r[o + 3] = $[t + (4 * o + 12) >> 2], r[o + 4] = $[t + (4 * o + 16) >> 2], r[o + 5] = $[t + (4 * o + 20) >> 2], r[o + 6] = $[t + (4 * o + 24) >> 2], r[o + 7] = $[t + (4 * o + 28) >> 2], r[o + 8] = $[t + (4 * o + 32) >> 2];
        else r = $.subarray(t >> 2, t + 36 * e >> 2);
        lg.uniformMatrix3fv(bv(n), !!i, r)
      }
    }

    function M_(n, e, i, t) {
      if (Bm.currentContext.version >= 2) lg.uniformMatrix4fv(bv(n), !!i, $, t >> 2, 16 * e);
      else {
        if (e <= 18) {
          var r = d_[16 * e - 1],
            o = $;
          t >>= 2;
          for (var a = 0; a < 16 * e; a += 16) {
            var l = t + a;
            r[a] = o[l], r[a + 1] = o[l + 1], r[a + 2] = o[l + 2], r[a + 3] = o[l + 3], r[a + 4] = o[l + 4], r[a + 5] = o[l + 5], r[a + 6] = o[l + 6], r[a + 7] = o[l + 7], r[a + 8] = o[l + 8], r[a + 9] = o[l + 9], r[a + 10] = o[l + 10], r[a + 11] = o[l + 11], r[a + 12] = o[l + 12], r[a + 13] = o[l + 13], r[a + 14] = o[l + 14], r[a + 15] = o[l + 15]
          }
        } else r = $.subarray(t >> 2, t + 64 * e >> 2);
        lg.uniformMatrix4fv(bv(n), !!i, r)
      }
    }

    function x_(n) {
      if (!xy(n)) return Bm.recordError(1280), x("GL_INVALID_ENUM in glUnmapBuffer"), 0;
      var e = My(n),
        i = Bm.mappedBuffers[e];
      return i ? (Bm.mappedBuffers[e] = null, 16 & i.access || (Bm.currentContext.version >= 2 ? lg.bufferSubData(n, i.offset, V, i.mem, i.length) : lg.bufferSubData(n, i.offset, V.subarray(i.mem, i.mem + i.length))), Fg(i.mem), 1) : (Bm.recordError(1282), x("buffer was never mapped in glUnmapBuffer"), 0)
    }

    function X_() {
      var n = lg.currentProgram;
      n.explicitProgramBindingsApplied || (Bm.currentContext.version >= 2 && Object.keys(n.explicitUniformBindings).forEach((function(e) {
        for (var i = n.explicitUniformBindings[e], t = 0; t < i[1]; ++t) {
          var r = lg.getUniformBlockIndex(n, e + (i[1] > 1 ? "[" + t + "]" : ""));
          lg.uniformBlockBinding(n, r, i[0] + t)
        }
      })), Object.keys(n.explicitSamplerBindings).forEach((function(e) {
        for (var i = n.explicitSamplerBindings[e], t = 0; t < i[1]; ++t) lg.uniform1i(lg.getUniformLocation(n, e + (t ? "[" + t + "]" : "")), i[0] + t)
      })), n.explicitProgramBindingsApplied = 1)
    }

    function j_(n) {
      n = Bm.programs[n], lg.useProgram(n), (lg.currentProgram = n) && X_()
    }

    function T_(n) {
      lg.validateProgram(Bm.programs[n])
    }

    function L_(n, e, i, t, r) {
      lg.vertexAttrib4f(n, e, i, t, r)
    }

    function F_(n, e) {
      lg.vertexAttrib4f(n, $[e >> 2], $[e + 4 >> 2], $[e + 8 >> 2], $[e + 12 >> 2])
    }

    function P_(n, e, i, t, r) {
      var o = Bm.currentContext.clientBuffers[n];
      if (!lg.currentArrayBufferBinding) return o.size = e, o.type = i, o.normalized = !1, o.stride = t, o.ptr = r, o.clientside = !0, void(o.vertexAttribPointerAdaptor = function(n, e, i, t, r, o) {
        this.vertexAttribIPointer(n, e, i, r, o)
      });
      o.clientside = !1, lg.vertexAttribIPointer(n, e, i, t, r)
    }

    function R_(n, e, i, t, r, o) {
      var a = Bm.currentContext.clientBuffers[n];
      if (!lg.currentArrayBufferBinding) return a.size = e, a.type = i, a.normalized = t, a.stride = r, a.ptr = o, a.clientside = !0, void(a.vertexAttribPointerAdaptor = function(n, e, i, t, r, o) {
        this.vertexAttribPointer(n, e, i, t, r, o)
      });
      a.clientside = !1, lg.vertexAttribPointer(n, e, i, !!t, r, o)
    }

    function B_(n, e, i, t) {
      lg.viewport(n, e, i, t)
    }

    function G_(n) {
      return n
    }

    function O_(n) {
      Sc();
      var e = new Date(Z[n + 20 >> 2] + 1900, Z[n + 16 >> 2], Z[n + 12 >> 2], Z[n + 8 >> 2], Z[n + 4 >> 2], Z[n >> 2], 0),
        i = Z[n + 32 >> 2],
        t = e.getTimezoneOffset(),
        r = new Date(e.getFullYear(), 0, 1),
        o = new Date(e.getFullYear(), 6, 1).getTimezoneOffset(),
        a = r.getTimezoneOffset(),
        l = Math.min(a, o);
      if (i < 0) Z[n + 32 >> 2] = Number(o != a && l == t);
      else if (i > 0 != (l == t)) {
        var u = Math.max(a, o),
          f = i > 0 ? l : u;
        e.setTime(e.getTime() + 6e4 * (f - t))
      }
      Z[n + 24 >> 2] = e.getDay();
      var c = (e.getTime() - r.getTime()) / 864e5 | 0;
      return Z[n + 28 >> 2] = c, Z[n >> 2] = e.getSeconds(), Z[n + 4 >> 2] = e.getMinutes(), Z[n + 8 >> 2] = e.getHours(), Z[n + 12 >> 2] = e.getDate(), Z[n + 16 >> 2] = e.getMonth(), e.getTime() / 1e3 | 0
    }

    function I_(n) {
      var e = rn(n);
      GameGlobal.dnSDK.track("ADD_TO_WISHLIST", {
        type: e
      })
    }

    function K_(n) {
      var e = rn(n);
      GameGlobal.dnSDK.onCreateRole(e)
    }

    function N_(n, e) {
      var i = rn(e);
      GameGlobal.dnSDK.track("PURCHASE", {
        value: n,
        outer_action_id: i
      })
    }

    function U_(n) {
      GameGlobal.dnSDK.track("RE_ACTIVE", {
        backFlowDay: n
      })
    }

    function z_() {
      GameGlobal.dnSDK.onRegister()
    }

    function q_(n) {
      var e = rn(n);
      GameGlobal.dnSDK.track("SHARE", {
        target: e
      })
    }

    function H_() {
      var n = GameGlobal.dnSDK.onTutorialFinish();
      null != n && 0 !== n.code ? console.warn("WXAMS TUTORIAL_FINISH failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS TUTORIAL_FINISH success")
    }

    function V_() {
      var n = GameGlobal.dnSDK.track("TUTORIAL_START", {});
      null != n && 0 !== n.code ? console.warn("WXAMS TUTORIAL_START failed, code:", n.code, "message:", n.message) : null != n && 0 === n.code && console.log("WXAMS TUTORIAL_START success")
    }

    function Y_(n, e) {
      GameGlobal.dnSDK.track("UPDATE_LEVEL", {
        level: n,
        power: e
      })
    }

    function J_(n) {
      var e = rn(n);
      GameGlobal.dnSDK.track("VIEW_CONTENT", {
        item: e
      })
    }

    function Z_(n) {
      P(n)
    }

    function Q_(n) {
      return n % 4 == 0 && (n % 100 != 0 || n % 400 == 0)
    }

    function $_(n, e) {
      for (var i = 0, t = 0; t <= e; i += n[t++]);
      return i
    }
    var ng = [31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31],
      eg = [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31];

    function ig(n, e) {
      for (var i = new Date(n.getTime()); e > 0;) {
        var t = Q_(i.getFullYear()),
          r = i.getMonth(),
          o = (t ? ng : eg)[r];
        if (!(e > o - i.getDate())) return i.setDate(i.getDate() + e), i;
        e -= o - i.getDate() + 1, i.setDate(1), r < 11 ? i.setMonth(r + 1) : (i.setMonth(0), i.setFullYear(i.getFullYear() + 1))
      }
      return i
    }

    function tg(n, e, i, t) {
      var r = Z[t + 40 >> 2],
        o = {
          tm_sec: Z[t >> 2],
          tm_min: Z[t + 4 >> 2],
          tm_hour: Z[t + 8 >> 2],
          tm_mday: Z[t + 12 >> 2],
          tm_mon: Z[t + 16 >> 2],
          tm_year: Z[t + 20 >> 2],
          tm_wday: Z[t + 24 >> 2],
          tm_yday: Z[t + 28 >> 2],
          tm_isdst: Z[t + 32 >> 2],
          tm_gmtoff: Z[t + 36 >> 2],
          tm_zone: r ? rn(r) : ""
        },
        a = rn(i),
        l = {
          "%c": "%a %b %d %H:%M:%S %Y",
          "%D": "%m/%d/%y",
          "%F": "%Y-%m-%d",
          "%h": "%b",
          "%r": "%I:%M:%S %p",
          "%R": "%H:%M",
          "%T": "%H:%M:%S",
          "%x": "%m/%d/%y",
          "%X": "%H:%M:%S",
          "%Ec": "%c",
          "%EC": "%C",
          "%Ex": "%m/%d/%y",
          "%EX": "%H:%M:%S",
          "%Ey": "%y",
          "%EY": "%Y",
          "%Od": "%d",
          "%Oe": "%e",
          "%OH": "%H",
          "%OI": "%I",
          "%Om": "%m",
          "%OM": "%M",
          "%OS": "%S",
          "%Ou": "%u",
          "%OU": "%U",
          "%OV": "%V",
          "%Ow": "%w",
          "%OW": "%W",
          "%Oy": "%y"
        };
      for (var u in l) a = a.replace(new RegExp(u, "g"), l[u]);
      var f = ["Sunday", "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday"],
        c = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];

      function s(n, e, i) {
        for (var t = "number" == typeof n ? n.toString() : n || ""; t.length < e;) t = i[0] + t;
        return t
      }

      function d(n, e) {
        return s(n, e, "0")
      }

      function m(n, e) {
        function i(n) {
          return n < 0 ? -1 : n > 0 ? 1 : 0
        }
        var t;
        return 0 === (t = i(n.getFullYear() - e.getFullYear())) && 0 === (t = i(n.getMonth() - e.getMonth())) && (t = i(n.getDate() - e.getDate())), t
      }

      function p(n) {
        switch (n.getDay()) {
          case 0:
            return new Date(n.getFullYear() - 1, 11, 29);
          case 1:
            return n;
          case 2:
            return new Date(n.getFullYear(), 0, 3);
          case 3:
            return new Date(n.getFullYear(), 0, 2);
          case 4:
            return new Date(n.getFullYear(), 0, 1);
          case 5:
            return new Date(n.getFullYear() - 1, 11, 31);
          case 6:
            return new Date(n.getFullYear() - 1, 11, 30)
        }
      }

      function y(n) {
        var e = ig(new Date(n.tm_year + 1900, 0, 1), n.tm_yday),
          i = new Date(e.getFullYear(), 0, 4),
          t = new Date(e.getFullYear() + 1, 0, 4),
          r = p(i),
          o = p(t);
        return m(r, e) <= 0 ? m(o, e) <= 0 ? e.getFullYear() + 1 : e.getFullYear() : e.getFullYear() - 1
      }
      var v = {
        "%a": function(n) {
          return f[n.tm_wday].substring(0, 3)
        },
        "%A": function(n) {
          return f[n.tm_wday]
        },
        "%b": function(n) {
          return c[n.tm_mon].substring(0, 3)
        },
        "%B": function(n) {
          return c[n.tm_mon]
        },
        "%C": function(n) {
          return d((n.tm_year + 1900) / 100 | 0, 2)
        },
        "%d": function(n) {
          return d(n.tm_mday, 2)
        },
        "%e": function(n) {
          return s(n.tm_mday, 2, " ")
        },
        "%g": function(n) {
          return y(n).toString().substring(2)
        },
        "%G": function(n) {
          return y(n)
        },
        "%H": function(n) {
          return d(n.tm_hour, 2)
        },
        "%I": function(n) {
          var e = n.tm_hour;
          return 0 == e ? e = 12 : e > 12 && (e -= 12), d(e, 2)
        },
        "%j": function(n) {
          return d(n.tm_mday + $_(Q_(n.tm_year + 1900) ? ng : eg, n.tm_mon - 1), 3)
        },
        "%m": function(n) {
          return d(n.tm_mon + 1, 2)
        },
        "%M": function(n) {
          return d(n.tm_min, 2)
        },
        "%n": function() {
          return "\n"
        },
        "%p": function(n) {
          return n.tm_hour >= 0 && n.tm_hour < 12 ? "AM" : "PM"
        },
        "%S": function(n) {
          return d(n.tm_sec, 2)
        },
        "%t": function() {
          return "\t"
        },
        "%u": function(n) {
          return n.tm_wday || 7
        },
        "%U": function(n) {
          var e = new Date(n.tm_year + 1900, 0, 1),
            i = 0 === e.getDay() ? e : ig(e, 7 - e.getDay()),
            t = new Date(n.tm_year + 1900, n.tm_mon, n.tm_mday);
          if (m(i, t) < 0) {
            var r = $_(Q_(t.getFullYear()) ? ng : eg, t.getMonth() - 1) - 31,
              o = 31 - i.getDate() + r + t.getDate();
            return d(Math.ceil(o / 7), 2)
          }
          return 0 === m(i, e) ? "01" : "00"
        },
        "%V": function(n) {
          var e, i = new Date(n.tm_year + 1900, 0, 4),
            t = new Date(n.tm_year + 1901, 0, 4),
            r = p(i),
            o = p(t),
            a = ig(new Date(n.tm_year + 1900, 0, 1), n.tm_yday);
          return m(a, r) < 0 ? "53" : m(o, a) <= 0 ? "01" : (e = r.getFullYear() < n.tm_year + 1900 ? n.tm_yday + 32 - r.getDate() : n.tm_yday + 1 - r.getDate(), d(Math.ceil(e / 7), 2))
        },
        "%w": function(n) {
          return n.tm_wday
        },
        "%W": function(n) {
          var e = new Date(n.tm_year, 0, 1),
            i = 1 === e.getDay() ? e : ig(e, 0 === e.getDay() ? 1 : 7 - e.getDay() + 1),
            t = new Date(n.tm_year + 1900, n.tm_mon, n.tm_mday);
          if (m(i, t) < 0) {
            var r = $_(Q_(t.getFullYear()) ? ng : eg, t.getMonth() - 1) - 31,
              o = 31 - i.getDate() + r + t.getDate();
            return d(Math.ceil(o / 7), 2)
          }
          return 0 === m(i, e) ? "01" : "00"
        },
        "%y": function(n) {
          return (n.tm_year + 1900).toString().substring(2)
        },
        "%Y": function(n) {
          return n.tm_year + 1900
        },
        "%z": function(n) {
          var e = n.tm_gmtoff,
            i = e >= 0;
          return e = (e = Math.abs(e) / 60) / 60 * 100 + e % 60, (i ? "+" : "-") + String("0000" + e).slice(-4)
        },
        "%Z": function(n) {
          return n.tm_zone
        },
        "%%": function() {
          return "%"
        }
      };
      for (var u in v) a.includes(u) && (a = a.replace(new RegExp(u, "g"), v[u](o)));
      var _ = pg(a, !1);
      return _.length > e ? 0 : (cn(_, n), _.length - 1)
    }

    function rg(n) {
      var e = Date.now() / 1e3 | 0;
      return n && (Z[n >> 2] = e), e
    }

    function og(n, e) {
      n = rn(n);
      try {
        return Xc.utime(n, e, e), 0
      } catch (n) {
        if (!(n instanceof Xc.ErrnoError)) throw n + " : " + Jn();
        return Rc(n.errno), -1
      }
    }

    function ag(n, e) {
      return og(n, e ? 1e3 * Z[e + 4 >> 2] : Date.now())
    }
    var lg, ug = function(n, e, i, t) {
        n || (n = this), this.parent = n, this.mount = n.mount, this.mounted = null, this.id = Xc.nextInode++, this.name = e, this.mode = i, this.node_ops = {}, this.stream_ops = {}, this.rdev = t
      },
      fg = 365,
      cg = 146;
    Object.defineProperties(ug.prototype, {
      read: {
        get: function() {
          return (this.mode & fg) === fg
        },
        set: function(n) {
          n ? this.mode |= fg : this.mode &= ~fg
        }
      },
      write: {
        get: function() {
          return (this.mode & cg) === cg
        },
        set: function(n) {
          n ? this.mode |= cg : this.mode &= ~cg
        }
      },
      isFolder: {
        get: function() {
          return Xc.isDir(this.mode)
        }
      },
      isDevice: {
        get: function() {
          return Xc.isChrdev(this.mode)
        }
      }
    }), Xc.FSNode = ug, Xc.staticInit(), n.FS_createPath = Xc.createPath, n.FS_createDataFile = Xc.createDataFile, n.requestFullscreen = function(n, e) {
      fd.requestFullscreen(n, e)
    }, n.requestAnimationFrame = function(n) {
      fd.requestAnimationFrame(n)
    }, n.setCanvasSize = function(n, e, i) {
      fd.setCanvasSize(n, e, i)
    }, n.pauseMainLoop = function() {
      fd.mainLoop.pause()
    }, n.resumeMainLoop = function() {
      fd.mainLoop.resume()
    }, n.getUserMedia = function() {
      fd.getUserMedia()
    }, n.createContext = function(n, e, i, t) {
      return fd.createContext(n, e, i, t)
    };
    for (var sg = 0; sg < 32; ++sg) gy.push(new Array(sg));
    var dg = new Float32Array(288);
    for (sg = 0; sg < 288; ++sg) d_[sg] = dg.subarray(0, sg + 1);
    var mg = new Int32Array(288);
    for (sg = 0; sg < 288; ++sg) y_[sg] = mg.subarray(0, sg + 1);

    function pg(n, e, i) {
      var t = i > 0 ? i : ln(n) + 1,
        r = new Array(t),
        o = on(n, r, 0, r.length);
      return e && (r.length = o), r
    }
    var yg, vg = {
        me: Zn,
        le: Qn,
        ke: $n,
        je: ne,
        ie: ee,
        Lp: te,
        Kp: re,
        Jp: oe,
        Ip: ae,
        he: le,
        Hp: ue,
        ge: fe,
        Gp: ce,
        Fp: se,
        fe: de,
        ee: me,
        Ep: pe,
        de: ye,
        ce: ve,
        Dp: _e,
        Cp: ge,
        Bp: he,
        be: we,
        Ap: Ee,
        ub: Be,
        tb: Oe,
        ae: Ke,
        zp: Ne,
        sb: Ue,
        rb: He,
        yp: Ye,
        xp: Je,
        wp: Qe,
        la: $e,
        vp: ei,
        qb: fi,
        pb: si,
        up: di,
        $d: pi,
        ob: _i,
        nb: gi,
        tp: hi,
        sp: wi,
        mb: ui,
        lb: ci,
        rp: Si,
        qp: Ci,
        pp: Wi,
        kb: Mi,
        jb: xi,
        _d: Xi,
        op: ji,
        np: Fi,
        mp: Pi,
        pa: Oi,
        lp: window.WXWASMSDK._JS_Sound_Create_Channel,
        Z: window.WXWASMSDK._JS_Sound_GetAudioBufferSampleRate,
        V: window.WXWASMSDK._JS_Sound_GetAudioContextSampleRate,
        kp: window.WXWASMSDK._JS_Sound_GetData,
        ea: window.WXWASMSDK._JS_Sound_GetLength,
        jp: window.WXWASMSDK._JS_Sound_GetLoadState,
        ip: (n, e) => window.WXWASMSDK._JS_Sound_GetMetaData(Q, n, e),
        hp: window.WXWASMSDK._JS_Sound_Init,
        gp: window.WXWASMSDK._JS_Sound_IsStopped,
        Zd: window.WXWASMSDK._JS_Sound_Load,
        Yd: window.WXWASMSDK._JS_Sound_Load_PCM,
        ib: window.WXWASMSDK._JS_Sound_Play,
        Na: window.WXWASMSDK._JS_Sound_ReleaseInstance,
        Xd: window.WXWASMSDK._JS_Sound_ResumeIfNeeded,
        fp: window.WXWASMSDK._JS_Sound_Set3D,
        ep: window.WXWASMSDK._JS_Sound_SetListenerOrientation,
        dp: window.WXWASMSDK._JS_Sound_SetListenerPosition,
        Wd: window.WXWASMSDK._JS_Sound_SetLoop,
        Vd: window.WXWASMSDK._JS_Sound_SetLoopPoints,
        Ma: window.WXWASMSDK._JS_Sound_SetPaused,
        Ud: window.WXWASMSDK._JS_Sound_SetPitch,
        cp: window.WXWASMSDK._JS_Sound_SetPosition,
        bp: window.WXWASMSDK._JS_Sound_SetVolume,
        La: window.WXWASMSDK._JS_Sound_Stop,
        Td: Ii,
        Sd: Ki,
        Ka: Ni,
        Rd: Ui,
        Qd: zi,
        Pd: qi,
        Od: Hi,
        hb: Vi,
        Nd: Yi,
        Md: Ji,
        ap: Zi,
        Ld: Qi,
        $o: $i,
        Kd: nt,
        _o: et,
        Ja: it,
        Zo: tt,
        Yo: rt,
        Xo: at,
        Wo: lt,
        Vo: ft,
        Uo: ct,
        gb: st,
        To: mt,
        So: pt,
        Ro: yt,
        Qo: vt,
        Po: _t,
        Oo: gt,
        No: ht,
        Mo: wt,
        Jd: St,
        Id: Ct,
        Hd: Et,
        Gd: bt,
        Fd: Wt,
        Lo: Dt,
        Ko: At,
        Ed: kt,
        Dd: Mt,
        Jo: xt,
        Io: Xt,
        Ho: jt,
        Cd: Tt,
        Bd: Lt,
        Go: Ft,
        Fo: Pt,
        Eo: Rt,
        Do: Bt,
        Co: Gt,
        Bo: Ot,
        Ao: It,
        zo: Kt,
        yo: Nt,
        xo: Ut,
        wo: zt,
        vo: qt,
        uo: Ht,
        to: Vt,
        so: Yt,
        ro: Jt,
        qo: Zt,
        po: Qt,
        uf: $t,
        Ad: nr,
        oo: er,
        zd: ir,
        no: tr,
        mo: rr,
        lo: or,
        ko: ar,
        jo: lr,
        io: ur,
        yd: fr,
        ho: cr,
        go: sr,
        fo: dr,
        eo: mr,
        co: pr,
        bo: yr,
        ao: vr,
        $n: _r,
        _n: gr,
        Zn: hr,
        Yn: wr,
        Xn: Sr,
        Wn: Cr,
        Vn: Er,
        Un: br,
        tf: Wr,
        Tn: Dr,
        Sn: Ar,
        Rn: kr,
        Qn: Mr,
        Pn: xr,
        On: Xr,
        Nn: jr,
        Mn: Tr,
        Ln: Lr,
        Kn: Fr,
        xd: Pr,
        wd: Rr,
        vd: Br,
        ud: Gr,
        Jn: Or,
        td: Ir,
        fb: Kr,
        sd: Nr,
        In: Ur,
        rd: zr,
        Hn: qr,
        Gn: Hr,
        Fn: Vr,
        eb: Yr,
        qd: Jr,
        En: Zr,
        pd: Qr,
        od: $r,
        Dn: no,
        Cn: eo,
        nd: io,
        md: to,
        Bn: ro,
        An: oo,
        zn: ao,
        yn: lo,
        xn: uo,
        wn: fo,
        vn: co,
        un: so,
        tn: mo,
        sn: po,
        rn: yo,
        qn: vo,
        pn: _o,
        on: go,
        nn: ho,
        mn: wo,
        ln: So,
        kn: Co,
        jn: Eo,
        hn: bo,
        gn: Wo,
        fn: Do,
        en: Ao,
        ld: ko,
        kd: Mo,
        dn: xo,
        cn: Xo,
        bn: jo,
        an: To,
        $m: Lo,
        _m: Fo,
        Zm: Po,
        jd: Ro,
        id: Bo,
        hd: Go,
        Ym: Oo,
        Xm: Io,
        Wm: Ko,
        Vm: No,
        gd: Uo,
        Um: zo,
        Tm: qo,
        Sm: Ho,
        Rm: Vo,
        Qm: Yo,
        Pm: Jo,
        Om: Zo,
        fd: Qo,
        Nm: $o,
        Mm: na,
        Lm: ea,
        Km: ia,
        Jm: ta,
        ed: ra,
        Im: oa,
        Hm: aa,
        Gm: la,
        Fm: ua,
        Em: fa,
        Dm: ca,
        Cm: sa,
        Bm: da,
        Am: ma,
        zm: pa,
        dd: ya,
        ym: va,
        xm: _a,
        sf: ga,
        wm: ha,
        vm: wa,
        um: Sa,
        tm: Ca,
        sm: Ea,
        rm: ba,
        qm: Wa,
        pm: Da,
        om: Aa,
        nm: ka,
        mm: Ma,
        lm: xa,
        km: Xa,
        jm: ja,
        im: Ta,
        hm: La,
        gm: Fa,
        fm: Pa,
        em: Ra,
        dm: Ba,
        cm: Ga,
        bm: Oa,
        am: Ia,
        $l: Ka,
        _l: Na,
        Zl: Ua,
        Yl: za,
        Xl: qa,
        Wl: Ha,
        Vl: Va,
        Ul: Ya,
        Tl: Ja,
        Sl: Za,
        Rl: Qa,
        Ql: $a,
        Pl: nl,
        Ol: el,
        Nl: il,
        Ml: tl,
        Ll: rl,
        Kl: ol,
        Jl: al,
        Il: ll,
        Hl: ul,
        Gl: fl,
        Fl: cl,
        El: sl,
        Dl: dl,
        Cl: ml,
        Bl: pl,
        Al: yl,
        zl: vl,
        yl: _l,
        xl: gl,
        wl: hl,
        vl: wl,
        ul: Sl,
        tl: Cl,
        sl: El,
        rl: bl,
        ql: Wl,
        pl: Dl,
        ol: Al,
        nl: kl,
        ml: Ml,
        ll: xl,
        kl: Xl,
        jl: jl,
        il: Tl,
        hl: Ll,
        gl: Fl,
        fl: Pl,
        el: Rl,
        dl: Bl,
        cl: Gl,
        bl: Ol,
        al: Il,
        $k: Kl,
        _k: Nl,
        Zk: Ul,
        Yk: zl,
        Xk: ql,
        Wk: Hl,
        Vk: Vl,
        Uk: Yl,
        Tk: Jl,
        Sk: Zl,
        Rk: Ql,
        Qk: $l,
        Pk: nu,
        Ok: eu,
        Nk: iu,
        Mk: tu,
        cd: ru,
        bd: ou,
        ad: au,
        Lk: lu,
        $c: uu,
        _c: fu,
        Zc: cu,
        Yc: su,
        Xc: du,
        Wc: mu,
        Vc: pu,
        Uc: yu,
        Tc: vu,
        Kk: _u,
        Sc: gu,
        Rc: hu,
        Jk: wu,
        Ik: Su,
        Qc: Cu,
        Hk: Eu,
        Pc: bu,
        Oc: Wu,
        Gk: Du,
        Nc: Au,
        Fk: ku,
        Ek: Mu,
        Dk: xu,
        Ck: Xu,
        Bk: ju,
        Ak: Tu,
        zk: Lu,
        yk: Fu,
        Mc: Pu,
        xk: Ru,
        Lc: Bu,
        Kc: Gu,
        Jc: Ou,
        Ic: Iu,
        wk: Ku,
        vk: Nu,
        uk: Uu,
        tk: zu,
        sk: qu,
        rk: Hu,
        qk: Vu,
        pk: Yu,
        ok: Ju,
        nk: Zu,
        mk: Qu,
        lk: $u,
        kk: nf,
        jk: ef,
        ik: tf,
        hk: rf,
        gk: of,
        fk: af,
        ek: lf,
        dk: uf,
        ck: ff,
        bk: cf,
        Hc: sf,
        Gc: df,
        ak: mf,
        $j: pf,
        _j: yf,
        Zj: vf,
        Yj: _f,
        Xj: gf,
        Wj: hf,
        Vj: wf,
        Uj: Sf,
        Tj: Cf,
        Sj: Ef,
        Rj: bf,
        Qj: Wf,
        Pj: Df,
        Oj: Af,
        Nj: kf,
        Mj: Mf,
        Lj: xf,
        Kj: Xf,
        Jj: jf,
        Ij: Tf,
        Hj: Lf,
        Gj: Ff,
        Fj: Pf,
        Ej: Rf,
        Dj: Bf,
        Cj: Gf,
        Bj: Of,
        Aj: If,
        zj: Kf,
        yj: Nf,
        xj: Uf,
        wj: zf,
        vj: qf,
        uj: Hf,
        tj: Vf,
        sj: Yf,
        rj: Jf,
        qj: Zf,
        pj: Qf,
        oj: $f,
        nj: nc,
        mj: ec,
        o: tc,
        i: uc,
        p: dc,
        g: pc,
        a: yc,
        lj: vc,
        db: cc,
        kj: _c,
        ka: gc,
        jj: wc,
        ij: Ec,
        k: mc,
        fj: Tc,
        Ri: Nc,
        Vi: Uc,
        Ui: Yc,
        Fc: Jc,
        Ti: Zc,
        Di: Qc,
        xi: $c,
        U: ns,
        _i: es,
        aj: is,
        dj: ts,
        Yi: rs,
        Cc: os,
        Dc: as,
        Ni: ls,
        Ci: us,
        Oi: fs,
        Qi: cs,
        Zi: ss,
        Ac: ds,
        zi: ms,
        Si: ps,
        $i: ys,
        Gi: vs,
        cj: gs,
        yi: ws,
        cb: Ss,
        Ei: Es,
        ej: bs,
        Ai: Ws,
        Ki: Ds,
        Ji: As,
        Hi: ks,
        Fi: Ms,
        Li: xs,
        Mi: Xs,
        Pi: js,
        Ii: Ts,
        Bc: Ls,
        Ec: Fs,
        Xi: Ps,
        Bi: Rs,
        bj: Bs,
        gj: Gs,
        hj: Os,
        Wi: Is,
        x: Ks,
        P: Ns,
        ti: Hs,
        yc: Vs,
        si: Ys,
        ri: Js,
        Ia: Zs,
        xc: Qs,
        qi: $s,
        ab: td,
        pi: cd,
        oi: sd,
        ni: Dd,
        mi: kd,
        li: _d,
        ki: xd,
        wc: jd,
        ji: Td,
        N: zs,
        vc: Ld,
        ii: Fd,
        hi: Pd,
        G: qd,
        D: Vd,
        gi: Yd,
        fi: Zd,
        ei: Qd,
        di: nm,
        uc: em,
        tc: tm,
        $a: hd,
        ci: rm,
        bi: am,
        sc: um,
        rc: fm,
        ai: cm,
        Ha: dm,
        Ga: mm,
        Fa: pm,
        $h: ym,
        _h: rd,
        qc: gm,
        pc: hm,
        oc: wm,
        Zh: Em,
        nc: Wm,
        mc: Dm,
        lc: Am,
        kc: km,
        jc: xm,
        Yh: Xm,
        Xh: Im,
        Wh: zm,
        Ea: qm,
        Vh: Nm,
        Uh: Hm,
        Da: Um,
        wi: Zm,
        vi: Qm,
        B: od,
        da: $m,
        ui: np,
        zc: ep,
        rf: ip,
        bb: tp,
        Ca: rp,
        b: op,
        Th: ap,
        Sh: up,
        Rh: fp,
        ic: cp,
        Qh: sp,
        Ba: dp,
        Ph: mp,
        Oh: pp,
        hc: yp,
        Nh: vp,
        Aa: _p,
        Mh: gp,
        Lh: hp,
        Kh: wp,
        Jh: Sp,
        Ih: Cp,
        Hh: Ep,
        Gh: bp,
        Fh: Wp,
        Eh: Dp,
        gc: Ap,
        fc: kp,
        ec: Mp,
        Dh: xp,
        Ch: Xp,
        Bh: jp,
        Ah: Tp,
        zh: Lp,
        yh: Fp,
        xh: Pp,
        wh: Rp,
        vh: Bp,
        uh: Gp,
        th: Op,
        qf: Ip,
        _a: Kp,
        sh: Np,
        rh: Up,
        qh: zp,
        ph: qp,
        oh: Hp,
        nh: Vp,
        mh: Yp,
        dc: Jp,
        lh: Zp,
        kh: Qp,
        jh: $p,
        ih: ny,
        hh: ey,
        gh: iy,
        Za: ty,
        fh: ry,
        eh: oy,
        dh: ay,
        cc: ly,
        ch: uy,
        bh: fy,
        ah: cy,
        za: sy,
        ya: dy,
        $g: my,
        _g: py,
        Zg: yy,
        Yg: vy,
        Xg: _y,
        Wg: hy,
        Vg: wy,
        Ug: Sy,
        Tg: Cy,
        Sg: Ey,
        bc: by,
        Rg: Wy,
        ac: Dy,
        Qg: Ay,
        $b: ky,
        Pg: Xy,
        Y: jy,
        Q: Ty,
        xa: Ly,
        wa: Fy,
        Og: Ry,
        Ng: By,
        _b: Gy,
        Mg: Oy,
        Lg: Iy,
        Kg: Ky,
        Jg: Ny,
        Ig: Uy,
        Hg: zy,
        Gg: Hy,
        Ya: Vy,
        Xa: Yy,
        ja: Jy,
        ia: Zy,
        Fg: Qy,
        Eg: $y,
        Dg: nv,
        Cg: ev,
        Bg: rv,
        va: av,
        Ag: lv,
        Zb: uv,
        zg: fv,
        $: cv,
        yg: sv,
        xg: dv,
        wg: mv,
        vg: pv,
        Yb: yv,
        ug: vv,
        tg: _v,
        sg: gv,
        rg: hv,
        qg: wv,
        pg: Sv,
        Wa: Cv,
        oa: Ev,
        Xb: Dv,
        og: kv,
        Va: Mv,
        ng: xv,
        mg: Xv,
        lg: jv,
        kg: Tv,
        jg: Lv,
        Wb: Fv,
        Vb: Pv,
        ig: Rv,
        hg: Bv,
        ha: Uv,
        gg: zv,
        fg: qv,
        eg: Hv,
        Ua: Vv,
        dg: Qv,
        cg: $v,
        bg: n_,
        ag: e_,
        $f: i_,
        _f: t_,
        Zf: r_,
        Ta: o_,
        Yf: a_,
        Xf: l_,
        Wf: u_,
        Vf: f_,
        Uf: c_,
        Tf: s_,
        Ub: m_,
        ua: p_,
        Tb: v_,
        Sb: __,
        Rb: g_,
        Qb: h_,
        Pb: w_,
        Sa: S_,
        Ob: C_,
        Nb: E_,
        ga: b_,
        Mb: W_,
        Lb: D_,
        Ra: A_,
        Kb: k_,
        ta: M_,
        Sf: x_,
        Rf: j_,
        Qf: T_,
        Pf: L_,
        Of: F_,
        Nf: P_,
        Mf: R_,
        Qa: B_,
        sa: qS,
        Lf: lC,
        L: xS,
        X: rS,
        Jb: zS,
        pf: qC,
        Ib: KS,
        T: gS,
        M: DS,
        z: VS,
        I: tS,
        F: RS,
        v: sS,
        na: GS,
        Hb: BS,
        d: fS,
        Kf: NS,
        Jf: kS,
        Gb: tC,
        If: US,
        c: Qw,
        _: ZS,
        fa: hS,
        e: nS,
        Hf: aC,
        Fb: AS,
        Gf: iC,
        q: iS,
        ra: QS,
        s: $w,
        J: JS,
        w: pS,
        A: dS,
        H: wS,
        O: HS,
        Eb: fC,
        Ff: PS,
        of: KC,
        nf: CC,
        mf: MC,
        lf: FC,
        kf: pC,
        jf: xC,
        hf: vC,
        gf: yC,
        ff: LC,
        ef: DC,
        df: BC,
        cf: OC,
        bf: GC,
        af: HC,
        $e: SC,
        _e: tE,
        Ze: mC,
        Ye: EC,
        Xe: _C,
        We: QC,
        Ve: gC,
        Ue: TC,
        Te: dC,
        Se: lE,
        Re: nE,
        Qe: WC,
        Pe: JC,
        Oe: aE,
        Ne: IC,
        Me: UC,
        Le: zC,
        Ke: bC,
        h: cS,
        Ef: rC,
        l: lS,
        Df: sC,
        y: IS,
        ma: TS,
        E: XS,
        Cf: $S,
        m: aS,
        Bf: uC,
        Af: oS,
        S: _S,
        zf: bS,
        Db: oC,
        W: MS,
        Pa: YS,
        f: uS,
        yf: jS,
        ca: WS,
        n: eS,
        xf: LS,
        wf: nC,
        ba: eC,
        r: mS,
        u: vS,
        C: yS,
        vf: cC,
        K: SS,
        R: ES,
        Cb: FS,
        qa: CS,
        Bb: OS,
        Je: wC,
        Ie: hC,
        He: kC,
        Ge: $C,
        Fe: PC,
        Ee: YC,
        De: eE,
        Ce: jC,
        Be: VC,
        Ae: AC,
        ze: RC,
        ye: ZC,
        xe: rE,
        we: XC,
        ve: NC,
        ue: oE,
        te: iE,
        j: G_,
        Ab: O_,
        se: I_,
        re: K_,
        qe: N_,
        zb: U_,
        yb: z_,
        pe: q_,
        xb: H_,
        wb: V_,
        oe: Y_,
        ne: J_,
        t: Z_,
        Oa: tg,
        aa: rg,
        vb: ag
      },
      _g = (In(), n.___wasm_call_ctors = function() {
        return (n.___wasm_call_ctors = n.asm.Np).apply(null, arguments)
      }, n._ReleaseKeys = function() {
        return (_g = n._ReleaseKeys = n.asm.Op).apply(null, arguments)
      }),
      gg = n._SendMessageFloat = function() {
        return (gg = n._SendMessageFloat = n.asm.Pp).apply(null, arguments)
      },
      hg = n._SendMessageString = function() {
        return (hg = n._SendMessageString = n.asm.Qp).apply(null, arguments)
      },
      wg = n._SendMessage = function() {
        return (wg = n._SendMessage = n.asm.Rp).apply(null, arguments)
      },
      Sg = (n._SetFullscreen = function() {
        return (n._SetFullscreen = n.asm.Sp).apply(null, arguments)
      }, n._main = function() {
        return (n._main = n.asm.Tp).apply(null, arguments)
      }, n.___errno_location = function() {
        return (Sg = n.___errno_location = n.asm.Up).apply(null, arguments)
      }),
      Cg = n._htonl = function() {
        return (Cg = n._htonl = n.asm.Vp).apply(null, arguments)
      },
      Eg = n._htons = function() {
        return (Eg = n._htons = n.asm.Wp).apply(null, arguments)
      },
      bg = n._ntohs = function() {
        return (bg = n._ntohs = n.asm.Xp).apply(null, arguments)
      },
      Wg = n.__get_tzname = function() {
        return (Wg = n.__get_tzname = n.asm.Yp).apply(null, arguments)
      },
      Dg = n.__get_daylight = function() {
        return (Dg = n.__get_daylight = n.asm.Zp).apply(null, arguments)
      },
      Ag = n.__get_timezone = function() {
        return (Ag = n.__get_timezone = n.asm._p).apply(null, arguments)
      },
      kg = n.stackSave = function() {
        return (kg = n.stackSave = n.asm.$p).apply(null, arguments)
      },
      Mg = n.stackRestore = function() {
        return (Mg = n.stackRestore = n.asm.aq).apply(null, arguments)
      },
      xg = n.stackAlloc = function() {
        return (xg = n.stackAlloc = n.asm.bq).apply(null, arguments)
      },
      Xg = (n._emscripten_stack_get_base = function() {
        return (n._emscripten_stack_get_base = n.asm.cq).apply(null, arguments)
      }, n._emscripten_stack_get_end = function() {
        return (n._emscripten_stack_get_end = n.asm.dq).apply(null, arguments)
      }, n._setThrew = function() {
        return (Xg = n._setThrew = n.asm.eq).apply(null, arguments)
      }),
      jg = n.___cxa_can_catch = function() {
        return (jg = n.___cxa_can_catch = n.asm.fq).apply(null, arguments)
      },
      Tg = n.___cxa_is_pointer_type = function() {
        return (Tg = n.___cxa_is_pointer_type = n.asm.gq).apply(null, arguments)
      },
      Lg = n._malloc = function() {
        return (Lg = n._malloc = n.asm.hq).apply(null, arguments)
      },
      Fg = n._free = function() {
        return (Fg = n._free = n.asm.iq).apply(null, arguments)
      },
      Pg = n._memalign = function() {
        return (Pg = n._memalign = n.asm.jq).apply(null, arguments)
      },
      Rg = n._sbrk = function() {
        return (Rg = n._sbrk = n.asm.kq).apply(null, arguments)
      },
      Bg = n._memset = function() {
        return (Bg = n._memset = n.asm.lq).apply(null, arguments)
      },
      Gg = n._strlen = function() {
        return (Gg = n._strlen = n.asm.mq).apply(null, arguments)
      },
      Og = (n.dynCall_iidiiii = function() {
        return (n.dynCall_iidiiii = n.asm.oq).apply(null, arguments)
      }, n.dynCall_vii = function() {
        return (Og = n.dynCall_vii = n.asm.pq).apply(null, arguments)
      }),
      Ig = n.dynCall_iii = function() {
        return (Ig = n.dynCall_iii = n.asm.qq).apply(null, arguments)
      },
      Kg = n.dynCall_ii = function() {
        return (Kg = n.dynCall_ii = n.asm.rq).apply(null, arguments)
      },
      Ng = n.dynCall_iiii = function() {
        return (Ng = n.dynCall_iiii = n.asm.sq).apply(null, arguments)
      },
      Ug = n.dynCall_jiji = function() {
        return (Ug = n.dynCall_jiji = n.asm.tq).apply(null, arguments)
      },
      zg = n.dynCall_vi = function() {
        return (zg = n.dynCall_vi = n.asm.uq).apply(null, arguments)
      },
      qg = n.dynCall_iiiii = function() {
        return (qg = n.dynCall_iiiii = n.asm.vq).apply(null, arguments)
      },
      Hg = n.dynCall_viii = function() {
        return (Hg = n.dynCall_viii = n.asm.wq).apply(null, arguments)
      },
      Vg = n.dynCall_iiiiii = function() {
        return (Vg = n.dynCall_iiiiii = n.asm.xq).apply(null, arguments)
      },
      Yg = n.dynCall_viiii = function() {
        return (Yg = n.dynCall_viiii = n.asm.yq).apply(null, arguments)
      },
      Jg = n.dynCall_iiij = function() {
        return (Jg = n.dynCall_iiij = n.asm.zq).apply(null, arguments)
      },
      Zg = n.dynCall_v = function() {
        return (Zg = n.dynCall_v = n.asm.Aq).apply(null, arguments)
      },
      Qg = n.dynCall_i = function() {
        return (Qg = n.dynCall_i = n.asm.Bq).apply(null, arguments)
      },
      $g = n.dynCall_iiiiiiii = function() {
        return ($g = n.dynCall_iiiiiiii = n.asm.Cq).apply(null, arguments)
      },
      nh = n.dynCall_iiijiii = function() {
        return (nh = n.dynCall_iiijiii = n.asm.Dq).apply(null, arguments)
      },
      eh = n.dynCall_iij = function() {
        return (eh = n.dynCall_iij = n.asm.Eq).apply(null, arguments)
      },
      ih = n.dynCall_viiiii = function() {
        return (ih = n.dynCall_viiiii = n.asm.Fq).apply(null, arguments)
      },
      th = n.dynCall_iiiiiii = function() {
        return (th = n.dynCall_iiiiiii = n.asm.Gq).apply(null, arguments)
      },
      rh = n.dynCall_jii = function() {
        return (rh = n.dynCall_jii = n.asm.Hq).apply(null, arguments)
      },
      oh = n.dynCall_viiiiiii = function() {
        return (oh = n.dynCall_viiiiiii = n.asm.Iq).apply(null, arguments)
      },
      ah = (n.dynCall_fiiffi = function() {
        return (n.dynCall_fiiffi = n.asm.Jq).apply(null, arguments)
      }, n.dynCall_viiiiii = function() {
        return (ah = n.dynCall_viiiiii = n.asm.Kq).apply(null, arguments)
      }),
      lh = (n.dynCall_viiififii = function() {
        return (n.dynCall_viiififii = n.asm.Lq).apply(null, arguments)
      }, n.dynCall_viiiiij = function() {
        return (lh = n.dynCall_viiiiij = n.asm.Mq).apply(null, arguments)
      }),
      uh = n.dynCall_viiff = function() {
        return (uh = n.dynCall_viiff = n.asm.Nq).apply(null, arguments)
      },
      fh = n.dynCall_fi = function() {
        return (fh = n.dynCall_fi = n.asm.Oq).apply(null, arguments)
      },
      ch = n.dynCall_iiifi = function() {
        return (ch = n.dynCall_iiifi = n.asm.Pq).apply(null, arguments)
      },
      sh = n.dynCall_ji = function() {
        return (sh = n.dynCall_ji = n.asm.Qq).apply(null, arguments)
      },
      dh = n.dynCall_iiiiiiiii = function() {
        return (dh = n.dynCall_iiiiiiiii = n.asm.Rq).apply(null, arguments)
      },
      mh = n.dynCall_viiiiiiii = function() {
        return (mh = n.dynCall_viiiiiiii = n.asm.Sq).apply(null, arguments)
      },
      ph = n.dynCall_viiiiiiiiii = function() {
        return (ph = n.dynCall_viiiiiiiiii = n.asm.Tq).apply(null, arguments)
      },
      yh = n.dynCall_iiiiij = function() {
        return (yh = n.dynCall_iiiiij = n.asm.Uq).apply(null, arguments)
      },
      vh = n.dynCall_viiffi = function() {
        return (vh = n.dynCall_viiffi = n.asm.Vq).apply(null, arguments)
      },
      _h = n.dynCall_iiiifii = function() {
        return (_h = n.dynCall_iiiifii = n.asm.Wq).apply(null, arguments)
      },
      gh = (n.dynCall_iiifii = function() {
        return (n.dynCall_iiifii = n.asm.Xq).apply(null, arguments)
      }, n.dynCall_viiiifii = function() {
        return (gh = n.dynCall_viiiifii = n.asm.Yq).apply(null, arguments)
      }),
      hh = (n.dynCall_fiffffi = function() {
        return (n.dynCall_fiffffi = n.asm.Zq).apply(null, arguments)
      }, n.dynCall_viiiiiiiii = function() {
        return (hh = n.dynCall_viiiiiiiii = n.asm._q).apply(null, arguments)
      }),
      wh = n.dynCall_jiiji = function() {
        return (wh = n.dynCall_jiiji = n.asm.$q).apply(null, arguments)
      },
      Sh = n.dynCall_iijii = function() {
        return (Sh = n.dynCall_iijii = n.asm.ar).apply(null, arguments)
      },
      Ch = n.dynCall_viji = function() {
        return (Ch = n.dynCall_viji = n.asm.br).apply(null, arguments)
      },
      Eh = (n.dynCall_viiijji = function() {
        return (n.dynCall_viiijji = n.asm.cr).apply(null, arguments)
      }, n.dynCall_viiji = function() {
        return (Eh = n.dynCall_viiji = n.asm.dr).apply(null, arguments)
      }),
      bh = (n.dynCall_viiffifiiifiifi = function() {
        return (n.dynCall_viiffifiiifiifi = n.asm.er).apply(null, arguments)
      }, n.dynCall_vifi = function() {
        return (bh = n.dynCall_vifi = n.asm.fr).apply(null, arguments)
      }),
      Wh = n.dynCall_viij = function() {
        return (Wh = n.dynCall_viij = n.asm.gr).apply(null, arguments)
      },
      Dh = n.dynCall_fii = function() {
        return (Dh = n.dynCall_fii = n.asm.hr).apply(null, arguments)
      },
      Ah = n.dynCall_iiiifi = function() {
        return (Ah = n.dynCall_iiiifi = n.asm.ir).apply(null, arguments)
      },
      kh = n.dynCall_iiiijfii = function() {
        return (kh = n.dynCall_iiiijfii = n.asm.jr).apply(null, arguments)
      },
      Mh = n.dynCall_iiffi = function() {
        return (Mh = n.dynCall_iiffi = n.asm.kr).apply(null, arguments)
      },
      xh = n.dynCall_viifi = function() {
        return (xh = n.dynCall_viifi = n.asm.lr).apply(null, arguments)
      },
      Xh = n.dynCall_viiifi = function() {
        return (Xh = n.dynCall_viiifi = n.asm.mr).apply(null, arguments)
      },
      jh = n.dynCall_dii = function() {
        return (jh = n.dynCall_dii = n.asm.nr).apply(null, arguments)
      },
      Th = n.dynCall_viiiff = function() {
        return (Th = n.dynCall_viiiff = n.asm.or).apply(null, arguments)
      },
      Lh = n.dynCall_jjji = function() {
        return (Lh = n.dynCall_jjji = n.asm.pr).apply(null, arguments)
      },
      Fh = (n.dynCall_viffffi = function() {
        return (n.dynCall_viffffi = n.asm.qr).apply(null, arguments)
      }, n.dynCall_viffi = function() {
        return (Fh = n.dynCall_viffi = n.asm.rr).apply(null, arguments)
      }),
      Ph = n.dynCall_iiiji = function() {
        return (Ph = n.dynCall_iiiji = n.asm.sr).apply(null, arguments)
      },
      Rh = n.dynCall_jdi = function() {
        return (Rh = n.dynCall_jdi = n.asm.tr).apply(null, arguments)
      },
      Bh = n.dynCall_vijjji = function() {
        return (Bh = n.dynCall_vijjji = n.asm.ur).apply(null, arguments)
      },
      Gh = n.dynCall_viijiiijiiii = function() {
        return (Gh = n.dynCall_viijiiijiiii = n.asm.vr).apply(null, arguments)
      },
      Oh = n.dynCall_jiii = function() {
        return (Oh = n.dynCall_jiii = n.asm.wr).apply(null, arguments)
      },
      Ih = n.dynCall_ijji = function() {
        return (Ih = n.dynCall_ijji = n.asm.xr).apply(null, arguments)
      },
      Kh = n.dynCall_iiji = function() {
        return (Kh = n.dynCall_iiji = n.asm.yr).apply(null, arguments)
      },
      Nh = (n.dynCall_viiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiii = n.asm.zr).apply(null, arguments)
      }, n.dynCall_iiijii = function() {
        return (n.dynCall_iiijii = n.asm.Ar).apply(null, arguments)
      }, n.dynCall_jiiii = function() {
        return (Nh = n.dynCall_jiiii = n.asm.Br).apply(null, arguments)
      }),
      Uh = (n.dynCall_iijiiii = function() {
        return (n.dynCall_iijiiii = n.asm.Cr).apply(null, arguments)
      }, n.dynCall_jijiii = function() {
        return (Uh = n.dynCall_jijiii = n.asm.Dr).apply(null, arguments)
      }),
      zh = n.dynCall_viijii = function() {
        return (zh = n.dynCall_viijii = n.asm.Er).apply(null, arguments)
      },
      qh = n.dynCall_iijiiiiii = function() {
        return (qh = n.dynCall_iijiiiiii = n.asm.Fr).apply(null, arguments)
      },
      Hh = n.dynCall_iijjiiiiii = function() {
        return (Hh = n.dynCall_iijjiiiiii = n.asm.Gr).apply(null, arguments)
      },
      Vh = n.dynCall_iiiijjii = function() {
        return (Vh = n.dynCall_iiiijjii = n.asm.Hr).apply(null, arguments)
      },
      Yh = n.dynCall_vijii = function() {
        return (Yh = n.dynCall_vijii = n.asm.Ir).apply(null, arguments)
      },
      Jh = n.dynCall_iijiii = function() {
        return (Jh = n.dynCall_iijiii = n.asm.Jr).apply(null, arguments)
      },
      Zh = n.dynCall_j = function() {
        return (Zh = n.dynCall_j = n.asm.Kr).apply(null, arguments)
      },
      Qh = n.dynCall_jijj = function() {
        return (Qh = n.dynCall_jijj = n.asm.Lr).apply(null, arguments)
      },
      $h = n.dynCall_iiiiiiiiiji = function() {
        return ($h = n.dynCall_iiiiiiiiiji = n.asm.Mr).apply(null, arguments)
      },
      nw = n.dynCall_vji = function() {
        return (nw = n.dynCall_vji = n.asm.Nr).apply(null, arguments)
      },
      ew = n.dynCall_viiij = function() {
        return (ew = n.dynCall_viiij = n.asm.Or).apply(null, arguments)
      },
      iw = n.dynCall_viiiifi = function() {
        return (iw = n.dynCall_viiiifi = n.asm.Pr).apply(null, arguments)
      },
      tw = n.dynCall_fiii = function() {
        return (tw = n.dynCall_fiii = n.asm.Qr).apply(null, arguments)
      },
      rw = n.dynCall_viiiiiiiiifi = function() {
        return (rw = n.dynCall_viiiiiiiiifi = n.asm.Rr).apply(null, arguments)
      },
      ow = n.dynCall_iiiiiiiiiiiii = function() {
        return (ow = n.dynCall_iiiiiiiiiiiii = n.asm.Sr).apply(null, arguments)
      },
      aw = n.dynCall_fiiii = function() {
        return (aw = n.dynCall_fiiii = n.asm.Tr).apply(null, arguments)
      },
      lw = n.dynCall_ifi = function() {
        return (lw = n.dynCall_ifi = n.asm.Ur).apply(null, arguments)
      },
      uw = n.dynCall_idi = function() {
        return (uw = n.dynCall_idi = n.asm.Vr).apply(null, arguments)
      },
      fw = n.dynCall_viiiiiiiiiiii = function() {
        return (fw = n.dynCall_viiiiiiiiiiii = n.asm.Wr).apply(null, arguments)
      },
      cw = (n.dynCall_iiiiji = function() {
        return (n.dynCall_iiiiji = n.asm.Xr).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiii = n.asm.Yr).apply(null, arguments)
      }, n.dynCall_diii = function() {
        return (cw = n.dynCall_diii = n.asm.Zr).apply(null, arguments)
      }),
      sw = n.dynCall_vidi = function() {
        return (sw = n.dynCall_vidi = n.asm._r).apply(null, arguments)
      },
      dw = n.dynCall_fffi = function() {
        return (dw = n.dynCall_fffi = n.asm.$r).apply(null, arguments)
      },
      mw = n.dynCall_jji = function() {
        return (mw = n.dynCall_jji = n.asm.as).apply(null, arguments)
      },
      pw = n.dynCall_diiii = function() {
        return (pw = n.dynCall_diiii = n.asm.bs).apply(null, arguments)
      },
      yw = n.dynCall_iidi = function() {
        return (yw = n.dynCall_iidi = n.asm.cs).apply(null, arguments)
      },
      vw = n.dynCall_iifi = function() {
        return (vw = n.dynCall_iifi = n.asm.ds).apply(null, arguments)
      },
      _w = n.dynCall_dddi = function() {
        return (_w = n.dynCall_dddi = n.asm.es).apply(null, arguments)
      },
      gw = n.dynCall_iiiiiiiiii = function() {
        return (gw = n.dynCall_iiiiiiiiii = n.asm.fs).apply(null, arguments)
      },
      hw = n.dynCall_jjii = function() {
        return (hw = n.dynCall_jjii = n.asm.gs).apply(null, arguments)
      },
      ww = n.dynCall_dji = function() {
        return (ww = n.dynCall_dji = n.asm.hs).apply(null, arguments)
      },
      Sw = n.dynCall_iji = function() {
        return (Sw = n.dynCall_iji = n.asm.is).apply(null, arguments)
      },
      Cw = n.dynCall_viijjii = function() {
        return (Cw = n.dynCall_viijjii = n.asm.js).apply(null, arguments)
      },
      Ew = n.dynCall_viijiii = function() {
        return (Ew = n.dynCall_viijiii = n.asm.ks).apply(null, arguments)
      },
      bw = n.dynCall_fiifi = function() {
        return (bw = n.dynCall_fiifi = n.asm.ls).apply(null, arguments)
      },
      Ww = n.dynCall_viifii = function() {
        return (Ww = n.dynCall_viifii = n.asm.ms).apply(null, arguments)
      },
      Dw = (n.dynCall_viifffffi = function() {
        return (n.dynCall_viifffffi = n.asm.ns).apply(null, arguments)
      }, n.dynCall_iiiiiifffffi = function() {
        return (Dw = n.dynCall_iiiiiifffffi = n.asm.os).apply(null, arguments)
      }),
      Aw = (n.dynCall_viiffffi = function() {
        return (n.dynCall_viiffffi = n.asm.ps).apply(null, arguments)
      }, n.dynCall_iiiffi = function() {
        return (Aw = n.dynCall_iiiffi = n.asm.qs).apply(null, arguments)
      }),
      kw = (n.dynCall_viiffffffffi = function() {
        return (n.dynCall_viiffffffffi = n.asm.rs).apply(null, arguments)
      }, n.dynCall_viifffffffi = function() {
        return (n.dynCall_viifffffffi = n.asm.ss).apply(null, arguments)
      }, n.dynCall_iiiiiffi = function() {
        return (kw = n.dynCall_iiiiiffi = n.asm.ts).apply(null, arguments)
      }),
      Mw = (n.dynCall_viifffiiii = function() {
        return (n.dynCall_viifffiiii = n.asm.us).apply(null, arguments)
      }, n.dynCall_viifiifi = function() {
        return (n.dynCall_viifiifi = n.asm.vs).apply(null, arguments)
      }, n.dynCall_vifii = function() {
        return (Mw = n.dynCall_vifii = n.asm.ws).apply(null, arguments)
      }),
      xw = (n.dynCall_viiifffii = function() {
        return (n.dynCall_viiifffii = n.asm.xs).apply(null, arguments)
      }, n.dynCall_viiiifiii = function() {
        return (xw = n.dynCall_viiiifiii = n.asm.ys).apply(null, arguments)
      }),
      Xw = n.dynCall_vijiii = function() {
        return (Xw = n.dynCall_vijiii = n.asm.zs).apply(null, arguments)
      },
      jw = n.dynCall_iiffii = function() {
        return (jw = n.dynCall_iiffii = n.asm.As).apply(null, arguments)
      },
      Tw = n.dynCall_jidi = function() {
        return (Tw = n.dynCall_jidi = n.asm.Bs).apply(null, arguments)
      },
      Lw = n.dynCall_vffi = function() {
        return (Lw = n.dynCall_vffi = n.asm.Cs).apply(null, arguments)
      },
      Fw = n.dynCall_viif = function() {
        return (Fw = n.dynCall_viif = n.asm.Ds).apply(null, arguments)
      },
      Pw = n.dynCall_viiffii = function() {
        return (Pw = n.dynCall_viiffii = n.asm.Es).apply(null, arguments)
      },
      Rw = n.dynCall_iiiifffi = function() {
        return (Rw = n.dynCall_iiiifffi = n.asm.Fs).apply(null, arguments)
      },
      Bw = (n.dynCall_viiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiii = n.asm.Gs).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiiii = n.asm.Hs).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiiiii = n.asm.Is).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiiiiii = n.asm.Js).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiiiiiii = n.asm.Ks).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_viiiiiiiiiiiiiiiiiii = n.asm.Ls).apply(null, arguments)
      }, n.dynCall_ddiii = function() {
        return (Bw = n.dynCall_ddiii = n.asm.Ms).apply(null, arguments)
      }),
      Gw = n.dynCall_viidi = function() {
        return (Gw = n.dynCall_viidi = n.asm.Ns).apply(null, arguments)
      },
      Ow = n.dynCall_iiiiiiiiiii = function() {
        return (Ow = n.dynCall_iiiiiiiiiii = n.asm.Os).apply(null, arguments)
      },
      Iw = n.dynCall_viiiiiiifiifiii = function() {
        return (Iw = n.dynCall_viiiiiiifiifiii = n.asm.Ps).apply(null, arguments)
      },
      Kw = n.dynCall_jiiiiiiiiii = function() {
        return (Kw = n.dynCall_jiiiiiiiiii = n.asm.Qs).apply(null, arguments)
      },
      Nw = n.dynCall_viijiiiiii = function() {
        return (Nw = n.dynCall_viijiiiiii = n.asm.Rs).apply(null, arguments)
      },
      Uw = n.dynCall_vjjjiiii = function() {
        return (Uw = n.dynCall_vjjjiiii = n.asm.Ss).apply(null, arguments)
      },
      zw = n.dynCall_ijjjiijii = function() {
        return (zw = n.dynCall_ijjjiijii = n.asm.Ts).apply(null, arguments)
      },
      qw = n.dynCall_vijiiii = function() {
        return (qw = n.dynCall_vijiiii = n.asm.Us).apply(null, arguments)
      },
      Hw = n.dynCall_vjiiiii = function() {
        return (Hw = n.dynCall_vjiiiii = n.asm.Vs).apply(null, arguments)
      },
      Vw = n.dynCall_jiiiii = function() {
        return (Vw = n.dynCall_jiiiii = n.asm.Ws).apply(null, arguments)
      },
      Yw = (n.dynCall_ffffi = function() {
        return (n.dynCall_ffffi = n.asm.Xs).apply(null, arguments)
      }, n.dynCall_viifiiiiiiiii = function() {
        return (n.dynCall_viifiiiiiiiii = n.asm.Ys).apply(null, arguments)
      }, n.dynCall_vfi = function() {
        return (n.dynCall_vfi = n.asm.Zs).apply(null, arguments)
      }, n.dynCall_viiifii = function() {
        return (n.dynCall_viiifii = n.asm._s).apply(null, arguments)
      }, n.dynCall_viiddiii = function() {
        return (n.dynCall_viiddiii = n.asm.$s).apply(null, arguments)
      }, n.dynCall_viiiffiiiii = function() {
        return (n.dynCall_viiiffiiiii = n.asm.at).apply(null, arguments)
      }, n.dynCall_viiifiii = function() {
        return (n.dynCall_viiifiii = n.asm.bt).apply(null, arguments)
      }, n.dynCall_iiffiii = function() {
        return (n.dynCall_iiffiii = n.asm.ct).apply(null, arguments)
      }, n.dynCall_iiffiiii = function() {
        return (n.dynCall_iiffiiii = n.asm.dt).apply(null, arguments)
      }, n.dynCall_iiifiifi = function() {
        return (n.dynCall_iiifiifi = n.asm.et).apply(null, arguments)
      }, n.dynCall_ijii = function() {
        return (n.dynCall_ijii = n.asm.ft).apply(null, arguments)
      }, n.dynCall_ifii = function() {
        return (n.dynCall_ifii = n.asm.gt).apply(null, arguments)
      }, n.dynCall_viffii = function() {
        return (n.dynCall_viffii = n.asm.ht).apply(null, arguments)
      }, n.dynCall_fifi = function() {
        return (n.dynCall_fifi = n.asm.it).apply(null, arguments)
      }, n.dynCall_iiiifiii = function() {
        return (n.dynCall_iiiifiii = n.asm.jt).apply(null, arguments)
      }, n.dynCall_vijji = function() {
        return (n.dynCall_vijji = n.asm.kt).apply(null, arguments)
      }, n.dynCall_viffffii = function() {
        return (n.dynCall_viffffii = n.asm.lt).apply(null, arguments)
      }, n.dynCall_viijiiiii = function() {
        return (n.dynCall_viijiiiii = n.asm.mt).apply(null, arguments)
      }, n.dynCall_viijiiii = function() {
        return (n.dynCall_viijiiii = n.asm.nt).apply(null, arguments)
      }, n.dynCall_viiiji = function() {
        return (n.dynCall_viiiji = n.asm.ot).apply(null, arguments)
      }, n.dynCall_vijjiii = function() {
        return (n.dynCall_vijjiii = n.asm.pt).apply(null, arguments)
      }, n.dynCall_viiiiifi = function() {
        return (n.dynCall_viiiiifi = n.asm.qt).apply(null, arguments)
      }, n.dynCall_ffffffffi = function() {
        return (n.dynCall_ffffffffi = n.asm.rt).apply(null, arguments)
      }, n.dynCall_viiiifiiiifi = function() {
        return (n.dynCall_viiiifiiiifi = n.asm.st).apply(null, arguments)
      }, n.dynCall_vifiii = function() {
        return (n.dynCall_vifiii = n.asm.tt).apply(null, arguments)
      }, n.dynCall_iifiiiiii = function() {
        return (n.dynCall_iifiiiiii = n.asm.ut).apply(null, arguments)
      }, n.dynCall_iifiiiii = function() {
        return (n.dynCall_iifiiiii = n.asm.vt).apply(null, arguments)
      }, n.dynCall_iiffiiiii = function() {
        return (n.dynCall_iiffiiiii = n.asm.wt).apply(null, arguments)
      }, n.dynCall_iifiii = function() {
        return (n.dynCall_iifiii = n.asm.xt).apply(null, arguments)
      }, n.dynCall_fiifii = function() {
        return (n.dynCall_fiifii = n.asm.yt).apply(null, arguments)
      }, n.dynCall_viiiiiifiifiiii = function() {
        return (n.dynCall_viiiiiifiifiiii = n.asm.zt).apply(null, arguments)
      }, n.dynCall_diidi = function() {
        return (n.dynCall_diidi = n.asm.At).apply(null, arguments)
      }, n.dynCall_fiifdi = function() {
        return (n.dynCall_fiifdi = n.asm.Bt).apply(null, arguments)
      }, n.dynCall_viiiiiifddfiiii = function() {
        return (n.dynCall_viiiiiifddfiiii = n.asm.Ct).apply(null, arguments)
      }, n.dynCall_fiifji = function() {
        return (n.dynCall_fiifji = n.asm.Dt).apply(null, arguments)
      }, n.dynCall_viiiiiifjjfiiii = function() {
        return (n.dynCall_viiiiiifjjfiiii = n.asm.Et).apply(null, arguments)
      }, n.dynCall_viiiiiiffffiiii = function() {
        return (n.dynCall_viiiiiiffffiiii = n.asm.Ft).apply(null, arguments)
      }, n.dynCall_viifiiii = function() {
        return (n.dynCall_viifiiii = n.asm.Gt).apply(null, arguments)
      }, n.dynCall_iifii = function() {
        return (n.dynCall_iifii = n.asm.Ht).apply(null, arguments)
      }, n.dynCall_iiiiifiii = function() {
        return (n.dynCall_iiiiifiii = n.asm.It).apply(null, arguments)
      }, n.dynCall_fffffi = function() {
        return (n.dynCall_fffffi = n.asm.Jt).apply(null, arguments)
      }, n.dynCall_fiiffffi = function() {
        return (n.dynCall_fiiffffi = n.asm.Kt).apply(null, arguments)
      }, n.dynCall_fffifffi = function() {
        return (n.dynCall_fffifffi = n.asm.Lt).apply(null, arguments)
      }, n.dynCall_viifiiiii = function() {
        return (n.dynCall_viifiiiii = n.asm.Mt).apply(null, arguments)
      }, n.dynCall_iiiiifiiii = function() {
        return (n.dynCall_iiiiifiiii = n.asm.Nt).apply(null, arguments)
      }, n.dynCall_jijii = function() {
        return (n.dynCall_jijii = n.asm.Ot).apply(null, arguments)
      }, n.dynCall_viiijiiii = function() {
        return (n.dynCall_viiijiiii = n.asm.Pt).apply(null, arguments)
      }, n.dynCall_viiiiji = function() {
        return (n.dynCall_viiiiji = n.asm.Qt).apply(null, arguments)
      }, n.dynCall_iiiiifii = function() {
        return (n.dynCall_iiiiifii = n.asm.Rt).apply(null, arguments)
      }, n.dynCall_viifiii = function() {
        return (n.dynCall_viifiii = n.asm.St).apply(null, arguments)
      }, n.dynCall_viiiiifiii = function() {
        return (n.dynCall_viiiiifiii = n.asm.Tt).apply(null, arguments)
      }, n.dynCall_iiiiiifiiii = function() {
        return (n.dynCall_iiiiiifiiii = n.asm.Ut).apply(null, arguments)
      }, n.dynCall_vfii = function() {
        return (n.dynCall_vfii = n.asm.Vt).apply(null, arguments)
      }, n.dynCall_diji = function() {
        return (n.dynCall_diji = n.asm.Wt).apply(null, arguments)
      }, n.dynCall_vijfi = function() {
        return (n.dynCall_vijfi = n.asm.Xt).apply(null, arguments)
      }, n.dynCall_ffi = function() {
        return (n.dynCall_ffi = n.asm.Yt).apply(null, arguments)
      }, n.dynCall_jjiji = function() {
        return (n.dynCall_jjiji = n.asm.Zt).apply(null, arguments)
      }, n.dynCall_iiiiiji = function() {
        return (n.dynCall_iiiiiji = n.asm._t).apply(null, arguments)
      }, n.dynCall_iiiifiiii = function() {
        return (n.dynCall_iiiifiiii = n.asm.$t).apply(null, arguments)
      }, n.dynCall_viffifiiifiifi = function() {
        return (n.dynCall_viffifiiifiifi = n.asm.au).apply(null, arguments)
      }, n.dynCall_fifffffi = function() {
        return (n.dynCall_fifffffi = n.asm.bu).apply(null, arguments)
      }, n.dynCall_vjiii = function() {
        return (n.dynCall_vjiii = n.asm.cu).apply(null, arguments)
      }, n.dynCall_ijiii = function() {
        return (n.dynCall_ijiii = n.asm.du).apply(null, arguments)
      }, n.dynCall_iiddi = function() {
        return (n.dynCall_iiddi = n.asm.eu).apply(null, arguments)
      }, n.dynCall_djii = function() {
        return (n.dynCall_djii = n.asm.fu).apply(null, arguments)
      }, n.dynCall_viiijii = function() {
        return (n.dynCall_viiijii = n.asm.gu).apply(null, arguments)
      }, n.dynCall_viiijiii = function() {
        return (n.dynCall_viiijiii = n.asm.hu).apply(null, arguments)
      }, n.dynCall_iiiiifi = function() {
        return (n.dynCall_iiiiifi = n.asm.iu).apply(null, arguments)
      }, n.dynCall_ifiiii = function() {
        return (n.dynCall_ifiiii = n.asm.ju).apply(null, arguments)
      }, n.dynCall_idiiiii = function() {
        return (n.dynCall_idiiiii = n.asm.ku).apply(null, arguments)
      }, n.dynCall_idiiii = function() {
        return (n.dynCall_idiiii = n.asm.lu).apply(null, arguments)
      }, n.dynCall_idii = function() {
        return (n.dynCall_idii = n.asm.mu).apply(null, arguments)
      }, n.dynCall_iiijiiii = function() {
        return (n.dynCall_iiijiiii = n.asm.nu).apply(null, arguments)
      }, n.dynCall_vjiiii = function() {
        return (n.dynCall_vjiiii = n.asm.ou).apply(null, arguments)
      }, n.dynCall_iddi = function() {
        return (n.dynCall_iddi = n.asm.pu).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiii = n.asm.qu).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiii = n.asm.ru).apply(null, arguments)
      }, n.dynCall_viiiiffi = function() {
        return (n.dynCall_viiiiffi = n.asm.su).apply(null, arguments)
      }, n.dynCall_viiiffi = function() {
        return (n.dynCall_viiiffi = n.asm.tu).apply(null, arguments)
      }, n.dynCall_vifiifi = function() {
        return (n.dynCall_vifiifi = n.asm.uu).apply(null, arguments)
      }, n.dynCall_viddfffi = function() {
        return (n.dynCall_viddfffi = n.asm.vu).apply(null, arguments)
      }, n.dynCall_viidfffi = function() {
        return (n.dynCall_viidfffi = n.asm.wu).apply(null, arguments)
      }, n.dynCall_vidifffi = function() {
        return (n.dynCall_vidifffi = n.asm.xu).apply(null, arguments)
      }, n.dynCall_viiifffi = function() {
        return (n.dynCall_viiifffi = n.asm.yu).apply(null, arguments)
      }, n.dynCall_viddi = function() {
        return (n.dynCall_viddi = n.asm.zu).apply(null, arguments)
      }, n.dynCall_vidii = function() {
        return (n.dynCall_vidii = n.asm.Au).apply(null, arguments)
      }, n.dynCall_viiiiiiifi = function() {
        return (n.dynCall_viiiiiiifi = n.asm.Bu).apply(null, arguments)
      }, n.dynCall_viiiiffffffffii = function() {
        return (n.dynCall_viiiiffffffffii = n.asm.Cu).apply(null, arguments)
      }, n.dynCall_viidii = function() {
        return (n.dynCall_viidii = n.asm.Du).apply(null, arguments)
      }, n.dynCall_iffi = function() {
        return (n.dynCall_iffi = n.asm.Eu).apply(null, arguments)
      }, n.dynCall_ffffii = function() {
        return (n.dynCall_ffffii = n.asm.Fu).apply(null, arguments)
      }, n.dynCall_ffii = function() {
        return (n.dynCall_ffii = n.asm.Gu).apply(null, arguments)
      }, n.dynCall_fiiiii = function() {
        return (n.dynCall_fiiiii = n.asm.Hu).apply(null, arguments)
      }, n.dynCall_ddddi = function() {
        return (n.dynCall_ddddi = n.asm.Iu).apply(null, arguments)
      }, n.dynCall_ddi = function() {
        return (n.dynCall_ddi = n.asm.Ju).apply(null, arguments)
      }, n.dynCall_vijjiiiii = function() {
        return (n.dynCall_vijjiiiii = n.asm.Ku).apply(null, arguments)
      }, n.dynCall_vijjjii = function() {
        return (n.dynCall_vijjjii = n.asm.Lu).apply(null, arguments)
      }, n.dynCall_viijji = function() {
        return (n.dynCall_viijji = n.asm.Mu).apply(null, arguments)
      }, n.dynCall_viffffffi = function() {
        return (n.dynCall_viffffffi = n.asm.Nu).apply(null, arguments)
      }, n.dynCall_viffffffffffffffffi = function() {
        return (n.dynCall_viffffffffffffffffi = n.asm.Ou).apply(null, arguments)
      }, n.dynCall_ijjiiii = function() {
        return (n.dynCall_ijjiiii = n.asm.Pu).apply(null, arguments)
      }, n.dynCall_vdiiiii = function() {
        return (n.dynCall_vdiiiii = n.asm.Qu).apply(null, arguments)
      }, n.dynCall_diiji = function() {
        return (n.dynCall_diiji = n.asm.Ru).apply(null, arguments)
      }, n.dynCall_vjiiiiiiii = function() {
        return (n.dynCall_vjiiiiiiii = n.asm.Su).apply(null, arguments)
      }, n.dynCall_vjiiiiiii = function() {
        return (n.dynCall_vjiiiiiii = n.asm.Tu).apply(null, arguments)
      }, n.dynCall_ijiiii = function() {
        return (n.dynCall_ijiiii = n.asm.Uu).apply(null, arguments)
      }, n.dynCall_iidii = function() {
        return (n.dynCall_iidii = n.asm.Vu).apply(null, arguments)
      }, n.dynCall_iidiii = function() {
        return (n.dynCall_iidiii = n.asm.Wu).apply(null, arguments)
      }, n.dynCall_fidi = function() {
        return (n.dynCall_fidi = n.asm.Xu).apply(null, arguments)
      }, n.dynCall_iiifiii = function() {
        return (n.dynCall_iiifiii = n.asm.Yu).apply(null, arguments)
      }, n.dynCall_vjii = function() {
        return (n.dynCall_vjii = n.asm.Zu).apply(null, arguments)
      }, n.dynCall_iffffi = function() {
        return (n.dynCall_iffffi = n.asm._u).apply(null, arguments)
      }, n.dynCall_vfffi = function() {
        return (n.dynCall_vfffi = n.asm.$u).apply(null, arguments)
      }, n.dynCall_vffffi = function() {
        return (n.dynCall_vffffi = n.asm.av).apply(null, arguments)
      }, n.dynCall_viiiffii = function() {
        return (n.dynCall_viiiffii = n.asm.bv).apply(null, arguments)
      }, n.dynCall_vifffi = function() {
        return (n.dynCall_vifffi = n.asm.cv).apply(null, arguments)
      }, n.dynCall_vfiii = function() {
        return (n.dynCall_vfiii = n.asm.dv).apply(null, arguments)
      }, n.dynCall_di = function() {
        return (n.dynCall_di = n.asm.ev).apply(null, arguments)
      }, n.dynCall_vifffii = function() {
        return (n.dynCall_vifffii = n.asm.fv).apply(null, arguments)
      }, n.dynCall_iiiifiiiii = function() {
        return (n.dynCall_iiiifiiiii = n.asm.gv).apply(null, arguments)
      }, n.dynCall_vijjii = function() {
        return (n.dynCall_vijjii = n.asm.hv).apply(null, arguments)
      }, n.dynCall_viiiififfi = function() {
        return (n.dynCall_viiiififfi = n.asm.iv).apply(null, arguments)
      }, n.dynCall_viiiifiifi = function() {
        return (n.dynCall_viiiifiifi = n.asm.jv).apply(null, arguments)
      }, n.dynCall_viiiifiiii = function() {
        return (n.dynCall_viiiifiiii = n.asm.kv).apply(null, arguments)
      }, n.dynCall_viiiifiiiii = function() {
        return (n.dynCall_viiiifiiiii = n.asm.lv).apply(null, arguments)
      }, n.dynCall_viiiifiiiiiiii = function() {
        return (n.dynCall_viiiifiiiiiiii = n.asm.mv).apply(null, arguments)
      }, n.dynCall_viiiiiffii = function() {
        return (n.dynCall_viiiiiffii = n.asm.nv).apply(null, arguments)
      }, n.dynCall_viffiii = function() {
        return (n.dynCall_viffiii = n.asm.ov).apply(null, arguments)
      }, n.dynCall_viffffiii = function() {
        return (n.dynCall_viffffiii = n.asm.pv).apply(null, arguments)
      }, n.dynCall_vifiiii = function() {
        return (n.dynCall_vifiiii = n.asm.qv).apply(null, arguments)
      }, n.dynCall_viiififi = function() {
        return (n.dynCall_viiififi = n.asm.rv).apply(null, arguments)
      }, n.dynCall_viiififfi = function() {
        return (n.dynCall_viiififfi = n.asm.sv).apply(null, arguments)
      }, n.dynCall_iiifiiii = function() {
        return (n.dynCall_iiifiiii = n.asm.tv).apply(null, arguments)
      }, n.dynCall_iiiififi = function() {
        return (n.dynCall_iiiififi = n.asm.uv).apply(null, arguments)
      }, n.dynCall_iiiififfi = function() {
        return (n.dynCall_iiiififfi = n.asm.vv).apply(null, arguments)
      }, n.dynCall_vififfii = function() {
        return (n.dynCall_vififfii = n.asm.wv).apply(null, arguments)
      }, n.dynCall_vififfi = function() {
        return (n.dynCall_vififfi = n.asm.xv).apply(null, arguments)
      }, n.dynCall_vififi = function() {
        return (n.dynCall_vififi = n.asm.yv).apply(null, arguments)
      }, n.dynCall_vifffffi = function() {
        return (n.dynCall_vifffffi = n.asm.zv).apply(null, arguments)
      }, n.dynCall_viffiiii = function() {
        return (n.dynCall_viffiiii = n.asm.Av).apply(null, arguments)
      }, n.dynCall_viiiiffffiiii = function() {
        return (n.dynCall_viiiiffffiiii = n.asm.Bv).apply(null, arguments)
      }, n.dynCall_iiiiiiffiiiiiiiiiffffiiii = function() {
        return (n.dynCall_iiiiiiffiiiiiiiiiffffiiii = n.asm.Cv).apply(null, arguments)
      }, n.dynCall_iiiiiiffiiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiffiiiiiiiiiiiiiii = n.asm.Dv).apply(null, arguments)
      }, n.dynCall_viififii = function() {
        return (n.dynCall_viififii = n.asm.Ev).apply(null, arguments)
      }, n.dynCall_iiiffiiii = function() {
        return (n.dynCall_iiiffiiii = n.asm.Fv).apply(null, arguments)
      }, n.dynCall_iiiiffiiii = function() {
        return (n.dynCall_iiiiffiiii = n.asm.Gv).apply(null, arguments)
      }, n.dynCall_fifffi = function() {
        return (n.dynCall_fifffi = n.asm.Hv).apply(null, arguments)
      }, n.dynCall_fffffffi = function() {
        return (n.dynCall_fffffffi = n.asm.Iv).apply(null, arguments)
      }, n.dynCall_viffifi = function() {
        return (n.dynCall_viffifi = n.asm.Jv).apply(null, arguments)
      }, n.dynCall_viiffifi = function() {
        return (n.dynCall_viiffifi = n.asm.Kv).apply(null, arguments)
      }, n.dynCall_ifffi = function() {
        return (n.dynCall_ifffi = n.asm.Lv).apply(null, arguments)
      }, n.dynCall_fiiiffi = function() {
        return (n.dynCall_fiiiffi = n.asm.Mv).apply(null, arguments)
      }, n.dynCall_viiififiii = function() {
        return (n.dynCall_viiififiii = n.asm.Nv).apply(null, arguments)
      }, n.dynCall_viiffiiiiiiiii = function() {
        return (n.dynCall_viiffiiiiiiiii = n.asm.Ov).apply(null, arguments)
      }, n.dynCall_viiiiiffiii = function() {
        return (n.dynCall_viiiiiffiii = n.asm.Pv).apply(null, arguments)
      }, n.dynCall_viiffiii = function() {
        return (n.dynCall_viiffiii = n.asm.Qv).apply(null, arguments)
      }, n.dynCall_viiffiiiiiii = function() {
        return (n.dynCall_viiffiiiiiii = n.asm.Rv).apply(null, arguments)
      }, n.dynCall_fffffffffi = function() {
        return (n.dynCall_fffffffffi = n.asm.Sv).apply(null, arguments)
      }, n.dynCall_vifiiiiii = function() {
        return (n.dynCall_vifiiiiii = n.asm.Tv).apply(null, arguments)
      }, n.dynCall_vifiiiii = function() {
        return (n.dynCall_vifiiiii = n.asm.Uv).apply(null, arguments)
      }, n.dynCall_viifiiiiiii = function() {
        return (n.dynCall_viifiiiiiii = n.asm.Vv).apply(null, arguments)
      }, n.dynCall_viiififfiiiiiii = function() {
        return (n.dynCall_viiififfiiiiiii = n.asm.Wv).apply(null, arguments)
      }, n.dynCall_viiffiifiiiiiii = function() {
        return (n.dynCall_viiffiifiiiiiii = n.asm.Xv).apply(null, arguments)
      }, n.dynCall_viifiiiiii = function() {
        return (n.dynCall_viifiiiiii = n.asm.Yv).apply(null, arguments)
      }, n.dynCall_viiifiiiiii = function() {
        return (n.dynCall_viiifiiiiii = n.asm.Zv).apply(null, arguments)
      }, n.dynCall_viiiifiiiiii = function() {
        return (n.dynCall_viiiifiiiiii = n.asm._v).apply(null, arguments)
      }, n.dynCall_viififiiiiii = function() {
        return (n.dynCall_viififiiiiii = n.asm.$v).apply(null, arguments)
      }, n.dynCall_viiiffiifiiiiiii = function() {
        return (n.dynCall_viiiffiifiiiiiii = n.asm.aw).apply(null, arguments)
      }, n.dynCall_viiiiiifiiiiii = function() {
        return (n.dynCall_viiiiiifiiiiii = n.asm.bw).apply(null, arguments)
      }, n.dynCall_vififiii = function() {
        return (n.dynCall_vififiii = n.asm.cw).apply(null, arguments)
      }, n.dynCall_fiffi = function() {
        return (n.dynCall_fiffi = n.asm.dw).apply(null, arguments)
      }, n.dynCall_viiiiiiiijiiii = function() {
        return (n.dynCall_viiiiiiiijiiii = n.asm.ew).apply(null, arguments)
      }, n.dynCall_fifii = function() {
        return (n.dynCall_fifii = n.asm.fw).apply(null, arguments)
      }, n.dynCall_viiiiiffi = function() {
        return (n.dynCall_viiiiiffi = n.asm.gw).apply(null, arguments)
      }, n.dynCall_iifffi = function() {
        return (n.dynCall_iifffi = n.asm.hw).apply(null, arguments)
      }, n.dynCall_jiiiiffffii = function() {
        return (n.dynCall_jiiiiffffii = n.asm.iw).apply(null, arguments)
      }, n.dynCall_iiiffffi = function() {
        return (n.dynCall_iiiffffi = n.asm.jw).apply(null, arguments)
      }, n.dynCall_vffffii = function() {
        return (n.dynCall_vffffii = n.asm.kw).apply(null, arguments)
      }, n.dynCall_vddii = function() {
        return (n.dynCall_vddii = n.asm.lw).apply(null, arguments)
      }, n.dynCall_vdi = function() {
        return (n.dynCall_vdi = n.asm.mw).apply(null, arguments)
      }, n.dynCall_viiijjii = function() {
        return (n.dynCall_viiijjii = n.asm.nw).apply(null, arguments)
      }, n.dynCall_vjji = function() {
        return (n.dynCall_vjji = n.asm.ow).apply(null, arguments)
      }, n.dynCall_viiiijji = function() {
        return (n.dynCall_viiiijji = n.asm.pw).apply(null, arguments)
      }, n.dynCall_viiiijjii = function() {
        return (n.dynCall_viiiijjii = n.asm.qw).apply(null, arguments)
      }, n.dynCall_jjiiii = function() {
        return (n.dynCall_jjiiii = n.asm.rw).apply(null, arguments)
      }, n.dynCall_vijiiiiiii = function() {
        return (n.dynCall_vijiiiiiii = n.asm.sw).apply(null, arguments)
      }, n.dynCall_vijiiiiiiii = function() {
        return (n.dynCall_vijiiiiiiii = n.asm.tw).apply(null, arguments)
      }, n.dynCall_iijji = function() {
        return (n.dynCall_iijji = n.asm.uw).apply(null, arguments)
      }, n.dynCall_jjiiiii = function() {
        return (n.dynCall_jjiiiii = n.asm.vw).apply(null, arguments)
      }, n.dynCall_jijjji = function() {
        return (n.dynCall_jijjji = n.asm.ww).apply(null, arguments)
      }, n.dynCall_jijjjii = function() {
        return (n.dynCall_jijjjii = n.asm.xw).apply(null, arguments)
      }, n.dynCall_jjiii = function() {
        return (n.dynCall_jjiii = n.asm.yw).apply(null, arguments)
      }, n.dynCall_ijijiiiii = function() {
        return (n.dynCall_ijijiiiii = n.asm.zw).apply(null, arguments)
      }, n.dynCall_ijjjiii = function() {
        return (n.dynCall_ijjjiii = n.asm.Aw).apply(null, arguments)
      }, n.dynCall_vijjjiijii = function() {
        return (n.dynCall_vijjjiijii = n.asm.Bw).apply(null, arguments)
      }, n.dynCall_vijiiiiii = function() {
        return (n.dynCall_vijiiiiii = n.asm.Cw).apply(null, arguments)
      }, n.dynCall_jfi = function() {
        return (n.dynCall_jfi = n.asm.Dw).apply(null, arguments)
      }, n.dynCall_fji = function() {
        return (n.dynCall_fji = n.asm.Ew).apply(null, arguments)
      }, n.dynCall_fdi = function() {
        return (n.dynCall_fdi = n.asm.Fw).apply(null, arguments)
      }, n.dynCall_dfi = function() {
        return (n.dynCall_dfi = n.asm.Gw).apply(null, arguments)
      }, n.dynCall_jidii = function() {
        return (n.dynCall_jidii = n.asm.Hw).apply(null, arguments)
      }, n.dynCall_viiiiiiiji = function() {
        return (n.dynCall_viiiiiiiji = n.asm.Iw).apply(null, arguments)
      }, n.dynCall_viiiiiiiiji = function() {
        return (n.dynCall_viiiiiiiiji = n.asm.Jw).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiji = function() {
        return (n.dynCall_viiiiiiiiiji = n.asm.Kw).apply(null, arguments)
      }, n.dynCall_ijiijii = function() {
        return (n.dynCall_ijiijii = n.asm.Lw).apply(null, arguments)
      }, n.dynCall_vjjiiiii = function() {
        return (n.dynCall_vjjiiiii = n.asm.Mw).apply(null, arguments)
      }, n.dynCall_vjjii = function() {
        return (n.dynCall_vjjii = n.asm.Nw).apply(null, arguments)
      }, n.dynCall_ijiiji = function() {
        return (n.dynCall_ijiiji = n.asm.Ow).apply(null, arguments)
      }, n.dynCall_ijiiiii = function() {
        return (n.dynCall_ijiiiii = n.asm.Pw).apply(null, arguments)
      }, n.dynCall_ijiiiiji = function() {
        return (n.dynCall_ijiiiiji = n.asm.Qw).apply(null, arguments)
      }, n.dynCall_ijjiii = function() {
        return (n.dynCall_ijjiii = n.asm.Rw).apply(null, arguments)
      }, n.dynCall_jiiiiii = function() {
        return (n.dynCall_jiiiiii = n.asm.Sw).apply(null, arguments)
      }, n.dynCall_ddii = function() {
        return (n.dynCall_ddii = n.asm.Tw).apply(null, arguments)
      }, n.dynCall_idiii = function() {
        return (n.dynCall_idiii = n.asm.Uw).apply(null, arguments)
      }, n.dynCall_ifiii = function() {
        return (n.dynCall_ifiii = n.asm.Vw).apply(null, arguments)
      }, n.dynCall_ifiiiii = function() {
        return (n.dynCall_ifiiiii = n.asm.Ww).apply(null, arguments)
      }, n.dynCall_jjjii = function() {
        return (n.dynCall_jjjii = n.asm.Xw).apply(null, arguments)
      }, n.dynCall_vdiii = function() {
        return (n.dynCall_vdiii = n.asm.Yw).apply(null, arguments)
      }, n.dynCall_jdii = function() {
        return (n.dynCall_jdii = n.asm.Zw).apply(null, arguments)
      }, n.dynCall_vdii = function() {
        return (n.dynCall_vdii = n.asm._w).apply(null, arguments)
      }, n.dynCall_jijji = function() {
        return (n.dynCall_jijji = n.asm.$w).apply(null, arguments)
      }, n.dynCall_diddi = function() {
        return (n.dynCall_diddi = n.asm.ax).apply(null, arguments)
      }, n.dynCall_didi = function() {
        return (n.dynCall_didi = n.asm.bx).apply(null, arguments)
      }, n.dynCall_iijjii = function() {
        return (n.dynCall_iijjii = n.asm.cx).apply(null, arguments)
      }, n.dynCall_jjjji = function() {
        return (n.dynCall_jjjji = n.asm.dx).apply(null, arguments)
      }, n.dynCall_viijijii = function() {
        return (n.dynCall_viijijii = n.asm.ex).apply(null, arguments)
      }, n.dynCall_viijijiii = function() {
        return (n.dynCall_viijijiii = n.asm.fx).apply(null, arguments)
      }, n.dynCall_vijiji = function() {
        return (n.dynCall_vijiji = n.asm.gx).apply(null, arguments)
      }, n.dynCall_viijiijiii = function() {
        return (n.dynCall_viijiijiii = n.asm.hx).apply(null, arguments)
      }, n.dynCall_viiiijiiii = function() {
        return (n.dynCall_viiiijiiii = n.asm.ix).apply(null, arguments)
      }, n.dynCall_jiiiiiiiii = function() {
        return (n.dynCall_jiiiiiiiii = n.asm.jx).apply(null, arguments)
      }, n.dynCall_viiffiifiii = function() {
        return (n.dynCall_viiffiifiii = n.asm.kx).apply(null, arguments)
      }, n.dynCall_viiiiffffffffi = function() {
        return (n.dynCall_viiiiffffffffi = n.asm.lx).apply(null, arguments)
      }, n.dynCall_fifiiii = function() {
        return (n.dynCall_fifiiii = n.asm.mx).apply(null, arguments)
      }, n.dynCall_viifffi = function() {
        return (n.dynCall_viifffi = n.asm.nx).apply(null, arguments)
      }, n.dynCall_viiffifiii = function() {
        return (n.dynCall_viiffifiii = n.asm.ox).apply(null, arguments)
      }, n.dynCall_viiffiiiii = function() {
        return (n.dynCall_viiffiiiii = n.asm.px).apply(null, arguments)
      }, n.dynCall_vifffffffi = function() {
        return (n.dynCall_vifffffffi = n.asm.qx).apply(null, arguments)
      }, n.dynCall_viffiiifi = function() {
        return (n.dynCall_viffiiifi = n.asm.rx).apply(null, arguments)
      }, n.dynCall_viiffiiiffi = function() {
        return (n.dynCall_viiffiiiffi = n.asm.sx).apply(null, arguments)
      }, n.dynCall_vfiiiii = function() {
        return (n.dynCall_vfiiiii = n.asm.tx).apply(null, arguments)
      }, n.dynCall_vfffffffffiiii = function() {
        return (n.dynCall_vfffffffffiiii = n.asm.ux).apply(null, arguments)
      }, n.dynCall_viiiiiifffffi = function() {
        return (n.dynCall_viiiiiifffffi = n.asm.vx).apply(null, arguments)
      }, n.dynCall_iiffffffiii = function() {
        return (n.dynCall_iiffffffiii = n.asm.wx).apply(null, arguments)
      }, n.dynCall_iffffffi = function() {
        return (n.dynCall_iffffffi = n.asm.xx).apply(null, arguments)
      }, n.dynCall_viiiiiffffiii = function() {
        return (n.dynCall_viiiiiffffiii = n.asm.yx).apply(null, arguments)
      }, n.dynCall_fiffii = function() {
        return (n.dynCall_fiffii = n.asm.zx).apply(null, arguments)
      }, n.dynCall_viiffiiii = function() {
        return (n.dynCall_viiffiiii = n.asm.Ax).apply(null, arguments)
      }, n.dynCall_iiiiiifiii = function() {
        return (n.dynCall_iiiiiifiii = n.asm.Bx).apply(null, arguments)
      }, n.dynCall_iiiiiiififii = function() {
        return (n.dynCall_iiiiiiififii = n.asm.Cx).apply(null, arguments)
      }, n.dynCall_viiiififi = function() {
        return (n.dynCall_viiiififi = n.asm.Dx).apply(null, arguments)
      }, n.dynCall_fffiii = function() {
        return (n.dynCall_fffiii = n.asm.Ex).apply(null, arguments)
      }, n.dynCall_fffii = function() {
        return (n.dynCall_fffii = n.asm.Fx).apply(null, arguments)
      }, n.dynCall_viddii = function() {
        return (n.dynCall_viddii = n.asm.Gx).apply(null, arguments)
      }, n.dynCall_iiiddi = function() {
        return (n.dynCall_iiiddi = n.asm.Hx).apply(null, arguments)
      }, n.dynCall_viiidi = function() {
        return (n.dynCall_viiidi = n.asm.Ix).apply(null, arguments)
      }, n.dynCall_iiififfii = function() {
        return (n.dynCall_iiififfii = n.asm.Jx).apply(null, arguments)
      }, n.dynCall_viiiiiiji = function() {
        return (n.dynCall_viiiiiiji = n.asm.Kx).apply(null, arguments)
      }, n.dynCall_viiiijii = function() {
        return (n.dynCall_viiiijii = n.asm.Lx).apply(null, arguments)
      }, n.dynCall_jiiiiiii = function() {
        return (n.dynCall_jiiiiiii = n.asm.Mx).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiiii = n.asm.Nx).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiiiii = n.asm.Ox).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiiiiii = n.asm.Px).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiiiiiii = n.asm.Qx).apply(null, arguments)
      }, n.dynCall_iiiiiiiiiiiiiiiiiii = function() {
        return (n.dynCall_iiiiiiiiiiiiiiiiiii = n.asm.Rx).apply(null, arguments)
      }, n.dynCall_iiijjii = function() {
        return (n.dynCall_iiijjii = n.asm.Sx).apply(null, arguments)
      }, n.dynCall_viifffii = function() {
        return (n.dynCall_viifffii = n.asm.Tx).apply(null, arguments)
      }, n.dynCall_vij = function() {
        return (n.dynCall_vij = n.asm.Ux).apply(null, arguments)
      }, n.dynCall_fff = function() {
        return (n.dynCall_fff = n.asm.Vx).apply(null, arguments)
      }, n.dynCall_vif = function() {
        return (n.dynCall_vif = n.asm.Wx).apply(null, arguments)
      }, n.dynCall_ijj = function() {
        return (n.dynCall_ijj = n.asm.Xx).apply(null, arguments)
      }, n.dynCall_ij = function() {
        return (n.dynCall_ij = n.asm.Yx).apply(null, arguments)
      }, n.dynCall_viffff = function() {
        return (n.dynCall_viffff = n.asm.Zx).apply(null, arguments)
      }, n.dynCall_vid = function() {
        return (n.dynCall_vid = n.asm._x).apply(null, arguments)
      }, n.dynCall_viiiiif = function() {
        return (n.dynCall_viiiiif = n.asm.$x).apply(null, arguments)
      }, n.dynCall_viiiif = function() {
        return (n.dynCall_viiiif = n.asm.ay).apply(null, arguments)
      }, n.dynCall_viiiiiif = function() {
        return (n.dynCall_viiiiiif = n.asm.by).apply(null, arguments)
      }, n.dynCall_iiiijiii = function() {
        return (n.dynCall_iiiijiii = n.asm.cy).apply(null, arguments)
      }, n.dynCall_iiiijii = function() {
        return (n.dynCall_iiiijii = n.asm.dy).apply(null, arguments)
      }, n.dynCall_iiiij = function() {
        return (n.dynCall_iiiij = n.asm.ey).apply(null, arguments)
      }, n.dynCall_iiif = function() {
        return (n.dynCall_iiif = n.asm.fy).apply(null, arguments)
      }, n.dynCall_fif = function() {
        return (n.dynCall_fif = n.asm.gy).apply(null, arguments)
      }, n.dynCall_iiiiiifff = function() {
        return (n.dynCall_iiiiiifff = n.asm.hy).apply(null, arguments)
      }, n.dynCall_iiiiiifiif = function() {
        return (n.dynCall_iiiiiifiif = n.asm.iy).apply(null, arguments)
      }, n.dynCall_iiiiiiifiif = function() {
        return (n.dynCall_iiiiiiifiif = n.asm.jy).apply(null, arguments)
      }, n.dynCall_fiff = function() {
        return (n.dynCall_fiff = n.asm.ky).apply(null, arguments)
      }, n.dynCall_fiiiiiifiifif = function() {
        return (n.dynCall_fiiiiiifiifif = n.asm.ly).apply(null, arguments)
      }, n.dynCall_fiiiiiifiiiif = function() {
        return (n.dynCall_fiiiiiifiiiif = n.asm.my).apply(null, arguments)
      }, n.dynCall_iifiiiijii = function() {
        return (n.dynCall_iifiiiijii = n.asm.ny).apply(null, arguments)
      }, n.dynCall_vifijii = function() {
        return (n.dynCall_vifijii = n.asm.oy).apply(null, arguments)
      }, n.dynCall_iiiifffiii = function() {
        return (n.dynCall_iiiifffiii = n.asm.py).apply(null, arguments)
      }, n.dynCall_iiiifffffi = function() {
        return (n.dynCall_iiiifffffi = n.asm.qy).apply(null, arguments)
      }, n.dynCall_viffiiiif = function() {
        return (n.dynCall_viffiiiif = n.asm.ry).apply(null, arguments)
      }, n.dynCall_viffiifffffiii = function() {
        return (n.dynCall_viffiifffffiii = n.asm.sy).apply(null, arguments)
      }, n.dynCall_viffffiifffiiiiif = function() {
        return (n.dynCall_viffffiifffiiiiif = n.asm.ty).apply(null, arguments)
      }, n.dynCall_iiiifffffii = function() {
        return (n.dynCall_iiiifffffii = n.asm.uy).apply(null, arguments)
      }, n.dynCall_viiiiiiiiiiifii = function() {
        return (n.dynCall_viiiiiiiiiiifii = n.asm.vy).apply(null, arguments)
      }, n.dynCall_viff = function() {
        return (n.dynCall_viff = n.asm.wy).apply(null, arguments)
      }, n.dynCall_iiiiifiiiiif = function() {
        return (n.dynCall_iiiiifiiiiif = n.asm.xy).apply(null, arguments)
      }, n.dynCall_viiifiiiii = function() {
        return (n.dynCall_viiifiiiii = n.asm.yy).apply(null, arguments)
      }, n.dynCall_viiiifiiiiif = function() {
        return (n.dynCall_viiiifiiiiif = n.asm.zy).apply(null, arguments)
      }, n.dynCall_iifff = function() {
        return (n.dynCall_iifff = n.asm.Ay).apply(null, arguments)
      }, n.dynCall_iif = function() {
        return (n.dynCall_iif = n.asm.By).apply(null, arguments)
      }, n.dynCall_viijijj = function() {
        return (n.dynCall_viijijj = n.asm.Cy).apply(null, arguments)
      }, n.dynCall_viijj = function() {
        return (n.dynCall_viijj = n.asm.Dy).apply(null, arguments)
      }, n.dynCall_viiiij = function() {
        return (n.dynCall_viiiij = n.asm.Ey).apply(null, arguments)
      }, n.dynCall_iiijji = function() {
        return (n.dynCall_iiijji = n.asm.Fy).apply(null, arguments)
      }, n.dynCall_ijjiiiii = function() {
        return (n.dynCall_ijjiiiii = n.asm.Gy).apply(null, arguments)
      }, n.dynCall_vidd = function() {
        return (Yw = n.dynCall_vidd = n.asm.Hy).apply(null, arguments)
      }),
      Jw = (n.dynCall_iiiiiifffiiifiii = function() {
        return (n.dynCall_iiiiiifffiiifiii = n.asm.Iy).apply(null, arguments)
      }, n.dynCall_viiif = function() {
        return (n.dynCall_viiif = n.asm.Jy).apply(null, arguments)
      }, n.dynCall_fiiiif = function() {
        return (n.dynCall_fiiiif = n.asm.Ky).apply(null, arguments)
      }, n.dynCall_vifff = function() {
        return (n.dynCall_vifff = n.asm.Ly).apply(null, arguments)
      }, n.dynCall_viifff = function() {
        return (n.dynCall_viifff = n.asm.My).apply(null, arguments)
      }, n.dynCall_vf = function() {
        return (n.dynCall_vf = n.asm.Ny).apply(null, arguments)
      }, n.dynCall_vffff = function() {
        return (Jw = n.dynCall_vffff = n.asm.Oy).apply(null, arguments)
      }),
      Zw = (n.dynCall_vff = function() {
        return (n.dynCall_vff = n.asm.Py).apply(null, arguments)
      }, n.dynCall_f = function() {
        return (n.dynCall_f = n.asm.Qy).apply(null, arguments)
      }, n.dynCall_vfff = function() {
        return (Zw = n.dynCall_vfff = n.asm.Ry).apply(null, arguments)
      });

    function Qw(n, e, i) {
      var t = kg();
      try {
        return Ig(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function $w(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Vg(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function nS(n, e, i, t) {
      var r = kg();
      try {
        return Ng(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function eS(n, e, i, t, r) {
      var o = kg();
      try {
        Yg(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function iS(n, e, i, t, r) {
      var o = kg();
      try {
        return qg(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function tS(n, e, i, t) {
      var r = kg();
      try {
        return tw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function rS(n, e, i, t) {
      var r = kg();
      try {
        return cw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function oS(n, e, i, t) {
      var r = kg();
      try {
        Fw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function aS(n, e, i) {
      var t = kg();
      try {
        Og(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function lS(n, e) {
      var i = kg();
      try {
        zg(n, e)
      } catch (n) {
        if (Mg(i), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function uS(n, e, i, t) {
      var r = kg();
      try {
        Hg(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function fS(n, e) {
      var i = kg();
      try {
        return Kg(n, e)
      } catch (n) {
        if (Mg(i), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function cS(n) {
      var e = kg();
      try {
        Zg(n)
      } catch (n) {
        if (Mg(e), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function sS(n) {
      var e = kg();
      try {
        return Qg(n)
      } catch (n) {
        if (Mg(e), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function dS(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        return $g(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function mS(n, e, i, t, r, o) {
      var a = kg();
      try {
        ih(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function pS(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        return th(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function yS(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        oh(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function vS(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        ah(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function _S(n, e, i, t, r) {
      var o = kg();
      try {
        uh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function gS(n, e) {
      var i = kg();
      try {
        return fh(n, e)
      } catch (n) {
        if (Mg(i), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function hS(n, e, i, t, r) {
      var o = kg();
      try {
        return ch(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function wS(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        return dh(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function SS(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        mh(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function CS(n, e, i, t, r, o, a, l, u, f, c) {
      var s = kg();
      try {
        ph(n, e, i, t, r, o, a, l, u, f, c)
      } catch (n) {
        if (Mg(s), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function ES(n, e, i, t, r, o, a, l, u, f) {
      var c = kg();
      try {
        hh(n, e, i, t, r, o, a, l, u, f)
      } catch (n) {
        if (Mg(c), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function bS(n, e, i, t, r, o) {
      var a = kg();
      try {
        vh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function WS(n, e, i, t, r, o) {
      var a = kg();
      try {
        Xh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function DS(n, e, i) {
      var t = kg();
      try {
        return Dh(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function AS(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Ah(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function kS(n, e, i, t, r) {
      var o = kg();
      try {
        return Mh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function MS(n, e, i, t, r) {
      var o = kg();
      try {
        xh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function xS(n, e, i) {
      var t = kg();
      try {
        return jh(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function XS(n, e, i, t) {
      var r = kg();
      try {
        bh(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function jS(n, e, i, t, r, o) {
      var a = kg();
      try {
        Th(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function TS(n, e, i, t, r) {
      var o = kg();
      try {
        Fh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function LS(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        iw(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function FS(n, e, i, t, r, o, a, l, u, f, c, s) {
      var d = kg();
      try {
        rw(n, e, i, t, r, o, a, l, u, f, c, s)
      } catch (n) {
        if (Mg(d), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function PS(n, e, i, t, r, o, a, l, u, f, c, s, d) {
      var m = kg();
      try {
        return ow(n, e, i, t, r, o, a, l, u, f, c, s, d)
      } catch (n) {
        if (Mg(m), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function RS(n, e, i, t, r) {
      var o = kg();
      try {
        return aw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function BS(n, e, i) {
      var t = kg();
      try {
        return lw(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function GS(n, e, i) {
      var t = kg();
      try {
        return uw(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function OS(n, e, i, t, r, o, a, l, u, f, c, s, d) {
      var m = kg();
      try {
        fw(n, e, i, t, r, o, a, l, u, f, c, s, d)
      } catch (n) {
        if (Mg(m), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function IS(n, e, i, t) {
      var r = kg();
      try {
        sw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function KS(n, e, i, t) {
      var r = kg();
      try {
        return dw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function NS(n, e, i, t) {
      var r = kg();
      try {
        return yw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function US(n, e, i, t) {
      var r = kg();
      try {
        return vw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function zS(n, e, i, t, r) {
      var o = kg();
      try {
        return pw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function qS(n, e, i, t) {
      var r = kg();
      try {
        return _w(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function HS(n, e, i, t, r, o, a, l, u, f) {
      var c = kg();
      try {
        return gw(n, e, i, t, r, o, a, l, u, f)
      } catch (n) {
        if (Mg(c), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function VS(n, e, i, t, r) {
      var o = kg();
      try {
        return bw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function YS(n, e, i, t, r, o) {
      var a = kg();
      try {
        Ww(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function JS(n, e, i, t, r, o, a, l, u, f, c, s) {
      var d = kg();
      try {
        return Dw(n, e, i, t, r, o, a, l, u, f, c, s)
      } catch (n) {
        if (Mg(d), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function ZS(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Aw(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function QS(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        return kw(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function $S(n, e, i, t, r) {
      var o = kg();
      try {
        Mw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function nC(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        gh(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function eC(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        xw(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function iC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        return _h(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function tC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return jw(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function rC(n, e, i, t) {
      var r = kg();
      try {
        Lw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function oC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        Pw(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function aC(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        return Rw(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function lC(n, e, i, t, r) {
      var o = kg();
      try {
        return Bw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function uC(n, e, i, t, r) {
      var o = kg();
      try {
        Gw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function fC(n, e, i, t, r, o, a, l, u, f, c) {
      var s = kg();
      try {
        return Ow(n, e, i, t, r, o, a, l, u, f, c)
      } catch (n) {
        if (Mg(s), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function cC(n, e, i, t, r, o, a, l, u, f, c, s, d, m, p) {
      var y = kg();
      try {
        Iw(n, e, i, t, r, o, a, l, u, f, c, s, d, m, p)
      } catch (n) {
        if (Mg(y), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function sC(n, e, i, t) {
      var r = kg();
      try {
        Yw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function dC(n, e, i, t, r) {
      var o = kg();
      try {
        return Nh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function mC(n) {
      var e = kg();
      try {
        return Zh(n)
      } catch (n) {
        if (Mg(e), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function pC(n, e, i, t, r) {
      var o = kg();
      try {
        return Jg(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function yC(n, e, i, t) {
      var r = kg();
      try {
        return eh(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function vC(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        return nh(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function _C(n, e) {
      var i = kg();
      try {
        return sh(n, e)
      } catch (n) {
        if (Mg(i), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function gC(n, e, i) {
      var t = kg();
      try {
        return rh(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function hC(n, e, i, t, r, o) {
      var a = kg();
      try {
        ew(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function wC(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        lh(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function SC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Ih(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function CC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        return yh(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function EC(n, e, i) {
      var t = kg();
      try {
        return Rh(n, e, i)
      } catch (n) {
        if (Mg(t), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function bC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Lh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function WC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return wh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function DC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Sh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function AC(n, e, i, t, r) {
      var o = kg();
      try {
        Ch(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function kC(n, e, i, t, r) {
      var o = kg();
      try {
        Wh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function MC(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        return kh(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function xC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Ph(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function XC(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        Bh(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function jC(n, e, i, t, r, o, a, l, u, f, c, s, d, m) {
      var p = kg();
      try {
        Gh(n, e, i, t, r, o, a, l, u, f, c, s, d, m)
      } catch (n) {
        if (Mg(p), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function TC(n, e, i, t) {
      var r = kg();
      try {
        return Oh(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function LC(n, e, i, t, r) {
      var o = kg();
      try {
        return Kh(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function FC(n, e, i, t, r, o, a, l, u, f) {
      var c = kg();
      try {
        return Vh(n, e, i, t, r, o, a, l, u, f)
      } catch (n) {
        if (Mg(c), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function PC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        zh(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function RC(n, e, i, t, r, o) {
      var a = kg();
      try {
        Yh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function BC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        return Jh(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function GC(n, e, i, t, r, o, a, l, u, f, c, s) {
      var d = kg();
      try {
        return Hh(n, e, i, t, r, o, a, l, u, f, c, s)
      } catch (n) {
        if (Mg(d), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function OC(n, e, i, t, r, o, a, l, u, f) {
      var c = kg();
      try {
        return qh(n, e, i, t, r, o, a, l, u, f)
      } catch (n) {
        if (Mg(c), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function IC(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Qh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function KC(n, e, i, t, r, o, a, l, u, f, c, s) {
      var d = kg();
      try {
        return $h(n, e, i, t, r, o, a, l, u, f, c, s)
      } catch (n) {
        if (Mg(d), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function NC(n, e, i, t) {
      var r = kg();
      try {
        nw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function UC(n, e, i, t) {
      var r = kg();
      try {
        return mw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function zC(n, e, i, t, r) {
      var o = kg();
      try {
        return hw(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function qC(n, e, i, t) {
      var r = kg();
      try {
        return ww(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function HC(n, e, i, t) {
      var r = kg();
      try {
        return Sw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function VC(n, e, i, t, r, o, a, l, u) {
      var f = kg();
      try {
        Cw(n, e, i, t, r, o, a, l, u)
      } catch (n) {
        if (Mg(f), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function YC(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        Ew(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function JC(n, e, i, t, r) {
      var o = kg();
      try {
        return Ug(n, e, i, t, r)
      } catch (n) {
        if (Mg(o), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function ZC(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        Xw(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function QC(n, e, i, t) {
      var r = kg();
      try {
        return Tw(n, e, i, t)
      } catch (n) {
        if (Mg(r), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function $C(n, e, i, t, r, o) {
      var a = kg();
      try {
        Eh(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function nE(n, e, i, t, r, o, a, l, u, f, c) {
      var s = kg();
      try {
        return Kw(n, e, i, t, r, o, a, l, u, f, c)
      } catch (n) {
        if (Mg(s), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function eE(n, e, i, t, r, o, a, l, u, f, c) {
      var s = kg();
      try {
        Nw(n, e, i, t, r, o, a, l, u, f, c)
      } catch (n) {
        if (Mg(s), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function iE(n, e, i, t, r, o, a, l, u, f, c) {
      var s = kg();
      try {
        Uw(n, e, i, t, r, o, a, l, u, f, c)
      } catch (n) {
        if (Mg(s), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function tE(n, e, i, t, r, o, a, l, u, f, c, s, d) {
      var m = kg();
      try {
        return zw(n, e, i, t, r, o, a, l, u, f, c, s, d)
      } catch (n) {
        if (Mg(m), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function rE(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        qw(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function oE(n, e, i, t, r, o, a, l) {
      var u = kg();
      try {
        Hw(n, e, i, t, r, o, a, l)
      } catch (n) {
        if (Mg(u), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function aE(n, e, i, t, r, o, a) {
      var l = kg();
      try {
        return Uh(n, e, i, t, r, o, a)
      } catch (n) {
        if (Mg(l), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function lE(n, e, i, t, r, o) {
      var a = kg();
      try {
        return Vw(n, e, i, t, r, o)
      } catch (n) {
        if (Mg(a), n !== n + 0 && "longjmp" !== n) throw n;
        Xg(1, 0)
      }
    }

    function uE(n) {
      this.name = "ExitStatus", this.message = "Program terminated with exit(" + n + ")", this.status = n
    }

    function fE(e) {
      var i = n._main,
        t = (e = e || []).length + 1,
        r = xg(4 * (t + 1));
      Z[r >> 2] = fn(_);
      for (var o = 1; o < t; o++) Z[(r >> 2) + o] = fn(e[o - 1]);
      Z[(r >> 2) + t] = 0;
      try {
        sE(i(t, r), !0)
      } catch (n) {
        if (n instanceof uE) return;
        if ("unwind" == n) return;
        var a = n;
        n && "object" == typeof n && n.stack && (a = [n, n.stack]), x("exception thrown: " + a), g(1, n)
      } finally {
        if (!0 === I) return;
        GameGlobal.unityNamespace.pluginCalledMainCb && GameGlobal.unityNamespace.pluginCalledMainCb(), !0, n.calledMainCb && n.calledMainCb(), (GameGlobal.unityNamespace.enableProfileStats || "function" == typeof GameGlobal.manager.getWXAppCheatMonitor && GameGlobal.manager.getWXAppCheatMonitor().shouldForceShowPerfMonitor()) && setTimeout(() => {
          m("WXSDKManagerHandler", "OpenProfileStats")
        }, 1e4)
      }
    }

    function cE(e) {
      function i() {
        GameGlobal.manager.TimeLogger.timeStart("callMain耗时"), yg || (yg = !0, n.calledRun = !0, I || (Cn(), En(), n.onRuntimeInitialized && n.onRuntimeInitialized(), dE && fE(e), Wn()))
      }
      e = e || v, Mn > 0 || (Sn(), Mn > 0 || (n.setStatus ? (n.setStatus("Running..."), setTimeout((function() {
        setTimeout((function() {
          n.setStatus("")
        }), 1), i()
      }), 1)) : i()))
    }

    function sE(e, i) {
      O = e, i && Yn() && 0 === e || (Yn() || (bn(), n.onExit && n.onExit(e), I = !0), g(e, new uE(e)))
    }
    if (n.dynCall_ff = function() {
        return (n.dynCall_ff = n.asm.Sy).apply(null, arguments)
      }, n.dynCall_d = function() {
        return (n.dynCall_d = n.asm.Ty).apply(null, arguments)
      }, n.dynCall_fiif = function() {
        return (n.dynCall_fiif = n.asm.Uy).apply(null, arguments)
      }, n.dynCall_iiiiiiffiiiiiiiiiffffiii = function() {
        return (n.dynCall_iiiiiiffiiiiiiiiiffffiii = n.asm.Vy).apply(null, arguments)
      }, n.dynCall_viififi = function() {
        return (n.dynCall_viififi = n.asm.Wy).apply(null, arguments)
      }, n.dynCall_viiiiiiiijiii = function() {
        return (n.dynCall_viiiiiiiijiii = n.asm.Xy).apply(null, arguments)
      }, n.GL = Bm, n.ccall = U, n.cwrap = z, n.stringToUTF8 = an, n.lengthBytesUTF8 = ln, n.stackTrace = Jn, n.addRunDependency = jn, n.removeRunDependency = Tn, n.FS_createPath = Xc.createPath, n.FS_createDataFile = Xc.createDataFile, n.stackTrace = Jn, Xn = function n() {
        yg || cE(), yg || (Xn = n)
      }, n.run = cE, n.preInit)
      for ("function" == typeof n.preInit && (n.preInit = [n.preInit]); n.preInit.length > 0;) n.preInit.pop()();
    var dE = !0;
    n.noInitialRun && (dE = !1), cE()
  }, GameGlobal.unityNamespace.useWasmCodeSplit = !0, GameGlobal.unityNamespace.WASM_SPLIT_SUB_VERSION = 7, GameGlobal.unityNamespace.WASM_SPLIT_API_VERSION = 7, GameGlobal.unityNamespace.WASM_SPLIT_PLUGIN_VERSION = "1.1.35";
});
var global = (function() {
  return this
})();
if (!global && typeof GameGlobal !== 'undefined') global = GameGlobal;
var pluginInfoMap = {};;
global.requirePlugin = global.requirePlugin || function(path) {
  var position = path.indexOf('/');
  var alias = '';
  var pagePath = '';
  if (position !== -1) {
    alias = path.substr(0, position);
    pagePath = path.substr(position + 1, path.length);
  } else {
    alias = path;
  }
  if (pluginInfoMap.hasOwnProperty(alias)) {
    var realPath = '';
    if (pagePath.length === 0) {
      realPath = '__plugin__/' + pluginInfoMap[alias].appid;
      return require(realPath);
    } else {
      realPath = '__plugin__/' + pluginInfoMap[alias].appid + '/' + pagePath;
      return require(realPath);
    }
  } else {
    console.error('not found alias: ', alias);
    throw new Error('Plugin ' + alias + ' is not defined.')
  }
};