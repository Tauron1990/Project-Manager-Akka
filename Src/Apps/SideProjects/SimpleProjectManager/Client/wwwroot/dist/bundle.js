/******/
(() => { // webpackBootstrap
    /******/
    "use strict";
    /******/
    var __webpack_modules__ = ({

        /***/ "./src/js/Database/DatabaseContext.ts":
        /*!********************************************!*\
          !*** ./src/js/Database/DatabaseContext.ts ***!
          \********************************************/
        /***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

            __webpack_require__.r(__webpack_exports__);
            /* harmony export */
            __webpack_require__.d(__webpack_exports__, {
                /* harmony export */   "DatabaseContext": () => (/* binding */ DatabaseContext)
                /* harmony export */
            });
            /* harmony import */
            var tslib__WEBPACK_IMPORTED_MODULE_1__ = __webpack_require__(/*! tslib */ "./node_modules/tslib/tslib.es6.js");
            /* harmony import */
            var idb__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! idb */ "./node_modules/idb/build/index.js");
// noinspection JSUnusedGlobalSymbols


            var DatabaseContext;
            (function (DatabaseContext) {
                function initDatabase() {
                    console.log("Init Database Api");
                    // @ts-ignore
                    window.Database = {};
                    window.Database.getCacheEntry = getCacheEntry;
                    window.Database.updateTimeout = updateTimeout;
                    window.Database.getTimeout = getTimeout;
                    window.Database.deleteTimeoutElement = deleteTimeoutElement;
                    window.Database.deleteElement = deleteElement;
                    window.Database.getAllTimeoutElements = getAllTimeoutElements;
                    window.Database.saveData = saveData;
                }

                DatabaseContext.initDatabase = initDatabase;
                var DataContainer = /** @class */ (function () {
                    function DataContainer(id, data) {
                        this.id = id;
                        this.data = data;
                    }

                    return DataContainer;
                }());
                var StaticData = /** @class */ (function () {
                    function StaticData() {
                    }

                    StaticData.database = null;
                    return StaticData;
                }());
                var Result = /** @class */ (function () {
                    function Result(msg) {
                        if (msg === undefined) {
                            this.sucess = true;
                        } else {
                            this.sucess = false;
                            this.message = msg;
                        }
                    }

                    return Result;
                }());
                DatabaseContext.Result = Result;

                function openDataDatabase() {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var _a;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_b) {
                            switch (_b.label) {
                                case 0:
                                    if (!(StaticData.database == null)) return [3 /*break*/, 2];
                                    _a = StaticData;
                                    return [4 /*yield*/, (0, idb__WEBPACK_IMPORTED_MODULE_0__.openDB)("StateData", 1, {
                                        upgrade: function (db) {
                                            db.createObjectStore("timeout", {keyPath: "id"});
                                            db.createObjectStore("data", {keyPath: "id"});
                                        }
                                    })];
                                case 1:
                                    _a.database = _b.sent();
                                    _b.label = 2;
                                case 2:
                                    return [2 /*return*/, StaticData.database];
                            }
                        });
                    });
                }

                function saveData(id, data) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.put("data", new DataContainer(id, data))];
                                case 2:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    });
                }

                DatabaseContext.saveData = saveData;

                function getAllTimeoutElements() {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db, dataList;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.getAll("timeout")];
                                case 2:
                                    dataList = _a.sent();
                                    return [2 /*return*/, dataList.map(function (v) {
                                        return v.data;
                                    })];
                            }
                        });
                    });
                }

                DatabaseContext.getAllTimeoutElements = getAllTimeoutElements;

                function deleteElement(id, timeoutId) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db, transaction, timeoutStore, dataStore, timeoutData;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    transaction = db.transaction(["timeout", "data"], "readwrite");
                                    timeoutStore = transaction.objectStore(transaction.objectStoreNames[0]);
                                    dataStore = transaction.objectStore(transaction.objectStoreNames[1]);
                                    return [4 /*yield*/, timeoutStore.get(timeoutId)];
                                case 2:
                                    timeoutData = _a.sent();
                                    if (!(timeoutData !== undefined)) return [3 /*break*/, 4];
                                    return [4 /*yield*/, timeoutStore.delete(timeoutId)];
                                case 3:
                                    _a.sent();
                                    _a.label = 4;
                                case 4:
                                    return [4 /*yield*/, dataStore.delete(id)];
                                case 5:
                                    _a.sent();
                                    return [4 /*yield*/, transaction.done];
                                case 6:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    });
                }

                DatabaseContext.deleteElement = deleteElement;

                function deleteTimeoutElement(id) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db, e_1;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    _a.trys.push([0, 3, , 4]);
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.delete("timeout", id)];
                                case 2:
                                    _a.sent();
                                    return [2 /*return*/, new Result()];
                                case 3:
                                    e_1 = _a.sent();
                                    return [2 /*return*/, new Result(e_1.toString())];
                                case 4:
                                    return [2 /*return*/];
                            }
                        });
                    });
                }

                DatabaseContext.deleteTimeoutElement = deleteTimeoutElement;

                function getTimeout(id) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db, entry;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.get("timeout", id)];
                                case 2:
                                    entry = _a.sent();
                                    if (entry === undefined) {
                                        return [2 /*return*/, ""];
                                    }
                                    return [2 /*return*/, entry.data];
                            }
                        });
                    });
                }

                DatabaseContext.getTimeout = getTimeout;

                function updateTimeout(id, data) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.put("timeout", new DataContainer(id, data), id)];
                                case 2:
                                    _a.sent();
                                    return [2 /*return*/];
                            }
                        });
                    });
                }

                DatabaseContext.updateTimeout = updateTimeout;

                function getCacheEntry(id) {
                    return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__awaiter)(this, void 0, void 0, function () {
                        var db, result;
                        return (0, tslib__WEBPACK_IMPORTED_MODULE_1__.__generator)(this, function (_a) {
                            switch (_a.label) {
                                case 0:
                                    return [4 /*yield*/, openDataDatabase()];
                                case 1:
                                    db = _a.sent();
                                    return [4 /*yield*/, db.get("data", id)];
                                case 2:
                                    result = _a.sent();
                                    if (result === undefined)
                                        return [2 /*return*/, ""];
                                    return [2 /*return*/, result.data];
                            }
                        });
                    });
                }

                DatabaseContext.getCacheEntry = getCacheEntry;
            })(DatabaseContext || (DatabaseContext = {}));


            /***/
        }),

        /***/ "./node_modules/tslib/tslib.es6.js":
        /*!*****************************************!*\
          !*** ./node_modules/tslib/tslib.es6.js ***!
          \*****************************************/
        /***/ ((__unused_webpack_module, __webpack_exports__, __webpack_require__) => {

            __webpack_require__.r(__webpack_exports__);
            /* harmony export */
            __webpack_require__.d(__webpack_exports__, {
                /* harmony export */   "__assign": () => (/* binding */ __assign),
                /* harmony export */   "__asyncDelegator": () => (/* binding */ __asyncDelegator),
                /* harmony export */   "__asyncGenerator": () => (/* binding */ __asyncGenerator),
                /* harmony export */   "__asyncValues": () => (/* binding */ __asyncValues),
                /* harmony export */   "__await": () => (/* binding */ __await),
                /* harmony export */   "__awaiter": () => (/* binding */ __awaiter),
                /* harmony export */   "__classPrivateFieldGet": () => (/* binding */ __classPrivateFieldGet),
                /* harmony export */   "__classPrivateFieldIn": () => (/* binding */ __classPrivateFieldIn),
                /* harmony export */   "__classPrivateFieldSet": () => (/* binding */ __classPrivateFieldSet),
                /* harmony export */   "__createBinding": () => (/* binding */ __createBinding),
                /* harmony export */   "__decorate": () => (/* binding */ __decorate),
                /* harmony export */   "__exportStar": () => (/* binding */ __exportStar),
                /* harmony export */   "__extends": () => (/* binding */ __extends),
                /* harmony export */   "__generator": () => (/* binding */ __generator),
                /* harmony export */   "__importDefault": () => (/* binding */ __importDefault),
                /* harmony export */   "__importStar": () => (/* binding */ __importStar),
                /* harmony export */   "__makeTemplateObject": () => (/* binding */ __makeTemplateObject),
                /* harmony export */   "__metadata": () => (/* binding */ __metadata),
                /* harmony export */   "__param": () => (/* binding */ __param),
                /* harmony export */   "__read": () => (/* binding */ __read),
                /* harmony export */   "__rest": () => (/* binding */ __rest),
                /* harmony export */   "__spread": () => (/* binding */ __spread),
                /* harmony export */   "__spreadArray": () => (/* binding */ __spreadArray),
                /* harmony export */   "__spreadArrays": () => (/* binding */ __spreadArrays),
                /* harmony export */   "__values": () => (/* binding */ __values)
                /* harmony export */
            });
            /******************************************************************************
             Copyright (c) Microsoft Corporation.

             Permission to use, copy, modify, and/or distribute this software for any
             purpose with or without fee is hereby granted.

             THE SOFTWARE IS PROVIDED "AS IS" AND THE AUTHOR DISCLAIMS ALL WARRANTIES WITH
             REGARD TO THIS SOFTWARE INCLUDING ALL IMPLIED WARRANTIES OF MERCHANTABILITY
             AND FITNESS. IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY SPECIAL, DIRECT,
             INDIRECT, OR CONSEQUENTIAL DAMAGES OR ANY DAMAGES WHATSOEVER RESULTING FROM
             LOSS OF USE, DATA OR PROFITS, WHETHER IN AN ACTION OF CONTRACT, NEGLIGENCE OR
             OTHER TORTIOUS ACTION, ARISING OUT OF OR IN CONNECTION WITH THE USE OR
             PERFORMANCE OF THIS SOFTWARE.
             ***************************************************************************** */
            /* global Reflect, Promise */

            var extendStatics = function (d, b) {
                extendStatics = Object.setPrototypeOf ||
                    ({__proto__: []} instanceof Array && function (d, b) {
                        d.__proto__ = b;
                    }) ||
                    function (d, b) {
                        for (var p in b) if (Object.prototype.hasOwnProperty.call(b, p)) d[p] = b[p];
                    };
                return extendStatics(d, b);
            };

            function __extends(d, b) {
                if (typeof b !== "function" && b !== null)
                    throw new TypeError("Class extends value " + String(b) + " is not a constructor or null");
                extendStatics(d, b);

                function __() {
                    this.constructor = d;
                }

                d.prototype = b === null ? Object.create(b) : (__.prototype = b.prototype, new __());
            }

            var __assign = function () {
                __assign = Object.assign || function __assign(t) {
                    for (var s, i = 1, n = arguments.length; i < n; i++) {
                        s = arguments[i];
                        for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p)) t[p] = s[p];
                    }
                    return t;
                }
                return __assign.apply(this, arguments);
            }

            function __rest(s, e) {
                var t = {};
                for (var p in s) if (Object.prototype.hasOwnProperty.call(s, p) && e.indexOf(p) < 0)
                    t[p] = s[p];
                if (s != null && typeof Object.getOwnPropertySymbols === "function")
                    for (var i = 0, p = Object.getOwnPropertySymbols(s); i < p.length; i++) {
                        if (e.indexOf(p[i]) < 0 && Object.prototype.propertyIsEnumerable.call(s, p[i]))
                            t[p[i]] = s[p[i]];
                    }
                return t;
            }

            function __decorate(decorators, target, key, desc) {
                var c = arguments.length,
                    r = c < 3 ? target : desc === null ? desc = Object.getOwnPropertyDescriptor(target, key) : desc, d;
                if (typeof Reflect === "object" && typeof Reflect.decorate === "function") r = Reflect.decorate(decorators, target, key, desc);
                else for (var i = decorators.length - 1; i >= 0; i--) if (d = decorators[i]) r = (c < 3 ? d(r) : c > 3 ? d(target, key, r) : d(target, key)) || r;
                return c > 3 && r && Object.defineProperty(target, key, r), r;
            }

            function __param(paramIndex, decorator) {
                return function (target, key) {
                    decorator(target, key, paramIndex);
                }
            }

            function __metadata(metadataKey, metadataValue) {
                if (typeof Reflect === "object" && typeof Reflect.metadata === "function") return Reflect.metadata(metadataKey, metadataValue);
            }

            function __awaiter(thisArg, _arguments, P, generator) {
                function adopt(value) {
                    return value instanceof P ? value : new P(function (resolve) {
                        resolve(value);
                    });
                }

                return new (P || (P = Promise))(function (resolve, reject) {
                    function fulfilled(value) {
                        try {
                            step(generator.next(value));
                        } catch (e) {
                            reject(e);
                        }
                    }

                    function rejected(value) {
                        try {
                            step(generator["throw"](value));
                        } catch (e) {
                            reject(e);
                        }
                    }

                    function step(result) {
                        result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected);
                    }

                    step((generator = generator.apply(thisArg, _arguments || [])).next());
                });
            }

            function __generator(thisArg, body) {
                var _ = {
                    label: 0, sent: function () {
                        if (t[0] & 1) throw t[1];
                        return t[1];
                    }, trys: [], ops: []
                }, f, y, t, g;
                return g = {
                    next: verb(0),
                    "throw": verb(1),
                    "return": verb(2)
                }, typeof Symbol === "function" && (g[Symbol.iterator] = function () {
                    return this;
                }), g;

                function verb(n) {
                    return function (v) {
                        return step([n, v]);
                    };
                }

                function step(op) {
                    if (f) throw new TypeError("Generator is already executing.");
                    while (_) try {
                        if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
                        if (y = 0, t) op = [op[0] & 2, t.value];
                        switch (op[0]) {
                            case 0:
                            case 1:
                                t = op;
                                break;
                            case 4:
                                _.label++;
                                return {value: op[1], done: false};
                            case 5:
                                _.label++;
                                y = op[1];
                                op = [0];
                                continue;
                            case 7:
                                op = _.ops.pop();
                                _.trys.pop();
                                continue;
                            default:
                                if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) {
                                    _ = 0;
                                    continue;
                                }
                                if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) {
                                    _.label = op[1];
                                    break;
                                }
                                if (op[0] === 6 && _.label < t[1]) {
                                    _.label = t[1];
                                    t = op;
                                    break;
                                }
                                if (t && _.label < t[2]) {
                                    _.label = t[2];
                                    _.ops.push(op);
                                    break;
                                }
                                if (t[2]) _.ops.pop();
                                _.trys.pop();
                                continue;
                        }
                        op = body.call(thisArg, _);
                    } catch (e) {
                        op = [6, e];
                        y = 0;
                    } finally {
                        f = t = 0;
                    }
                    if (op[0] & 5) throw op[1];
                    return {value: op[0] ? op[1] : void 0, done: true};
                }
            }

            var __createBinding = Object.create ? (function (o, m, k, k2) {
                if (k2 === undefined) k2 = k;
                var desc = Object.getOwnPropertyDescriptor(m, k);
                if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
                    desc = {
                        enumerable: true, get: function () {
                            return m[k];
                        }
                    };
                }
                Object.defineProperty(o, k2, desc);
            }) : (function (o, m, k, k2) {
                if (k2 === undefined) k2 = k;
                o[k2] = m[k];
            });

            function __exportStar(m, o) {
                for (var p in m) if (p !== "default" && !Object.prototype.hasOwnProperty.call(o, p)) __createBinding(o, m, p);
            }

            function __values(o) {
                var s = typeof Symbol === "function" && Symbol.iterator, m = s && o[s], i = 0;
                if (m) return m.call(o);
                if (o && typeof o.length === "number") return {
                    next: function () {
                        if (o && i >= o.length) o = void 0;
                        return {value: o && o[i++], done: !o};
                    }
                };
                throw new TypeError(s ? "Object is not iterable." : "Symbol.iterator is not defined.");
            }

            function __read(o, n) {
                var m = typeof Symbol === "function" && o[Symbol.iterator];
                if (!m) return o;
                var i = m.call(o), r, ar = [], e;
                try {
                    while ((n === void 0 || n-- > 0) && !(r = i.next()).done) ar.push(r.value);
                } catch (error) {
                    e = {error: error};
                } finally {
                    try {
                        if (r && !r.done && (m = i["return"])) m.call(i);
                    } finally {
                        if (e) throw e.error;
                    }
                }
                return ar;
            }

            /** @deprecated */
            function __spread() {
                for (var ar = [], i = 0; i < arguments.length; i++)
                    ar = ar.concat(__read(arguments[i]));
                return ar;
            }

            /** @deprecated */
            function __spreadArrays() {
                for (var s = 0, i = 0, il = arguments.length; i < il; i++) s += arguments[i].length;
                for (var r = Array(s), k = 0, i = 0; i < il; i++)
                    for (var a = arguments[i], j = 0, jl = a.length; j < jl; j++, k++)
                        r[k] = a[j];
                return r;
            }

            function __spreadArray(to, from, pack) {
                if (pack || arguments.length === 2) for (var i = 0, l = from.length, ar; i < l; i++) {
                    if (ar || !(i in from)) {
                        if (!ar) ar = Array.prototype.slice.call(from, 0, i);
                        ar[i] = from[i];
                    }
                }
                return to.concat(ar || Array.prototype.slice.call(from));
            }

            function __await(v) {
                return this instanceof __await ? (this.v = v, this) : new __await(v);
            }

            function __asyncGenerator(thisArg, _arguments, generator) {
                if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
                var g = generator.apply(thisArg, _arguments || []), i, q = [];
                return i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () {
                    return this;
                }, i;

                function verb(n) {
                    if (g[n]) i[n] = function (v) {
                        return new Promise(function (a, b) {
                            q.push([n, v, a, b]) > 1 || resume(n, v);
                        });
                    };
                }

                function resume(n, v) {
                    try {
                        step(g[n](v));
                    } catch (e) {
                        settle(q[0][3], e);
                    }
                }

                function step(r) {
                    r.value instanceof __await ? Promise.resolve(r.value.v).then(fulfill, reject) : settle(q[0][2], r);
                }

                function fulfill(value) {
                    resume("next", value);
                }

                function reject(value) {
                    resume("throw", value);
                }

                function settle(f, v) {
                    if (f(v), q.shift(), q.length) resume(q[0][0], q[0][1]);
                }
            }

            function __asyncDelegator(o) {
                var i, p;
                return i = {}, verb("next"), verb("throw", function (e) {
                    throw e;
                }), verb("return"), i[Symbol.iterator] = function () {
                    return this;
                }, i;

                function verb(n, f) {
                    i[n] = o[n] ? function (v) {
                        return (p = !p) ? {value: __await(o[n](v)), done: n === "return"} : f ? f(v) : v;
                    } : f;
                }
            }

            function __asyncValues(o) {
                if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
                var m = o[Symbol.asyncIterator], i;
                return m ? m.call(o) : (o = typeof __values === "function" ? __values(o) : o[Symbol.iterator](), i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () {
                    return this;
                }, i);

                function verb(n) {
                    i[n] = o[n] && function (v) {
                        return new Promise(function (resolve, reject) {
                            v = o[n](v), settle(resolve, reject, v.done, v.value);
                        });
                    };
                }

                function settle(resolve, reject, d, v) {
                    Promise.resolve(v).then(function (v) {
                        resolve({value: v, done: d});
                    }, reject);
                }
            }

            function __makeTemplateObject(cooked, raw) {
                if (Object.defineProperty) {
                    Object.defineProperty(cooked, "raw", {value: raw});
                } else {
                    cooked.raw = raw;
                }
                return cooked;
            }

            var __setModuleDefault = Object.create ? (function (o, v) {
                Object.defineProperty(o, "default", {enumerable: true, value: v});
            }) : function (o, v) {
                o["default"] = v;
            };

            function __importStar(mod) {
                if (mod && mod.__esModule) return mod;
                var result = {};
                if (mod != null) for (var k in mod) if (k !== "default" && Object.prototype.hasOwnProperty.call(mod, k)) __createBinding(result, mod, k);
                __setModuleDefault(result, mod);
                return result;
            }

            function __importDefault(mod) {
                return (mod && mod.__esModule) ? mod : {default: mod};
            }

            function __classPrivateFieldGet(receiver, state, kind, f) {
                if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a getter");
                if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot read private member from an object whose class did not declare it");
                return kind === "m" ? f : kind === "a" ? f.call(receiver) : f ? f.value : state.get(receiver);
            }

            function __classPrivateFieldSet(receiver, state, value, kind, f) {
                if (kind === "m") throw new TypeError("Private method is not writable");
                if (kind === "a" && !f) throw new TypeError("Private accessor was defined without a setter");
                if (typeof state === "function" ? receiver !== state || !f : !state.has(receiver)) throw new TypeError("Cannot write private member to an object whose class did not declare it");
                return (kind === "a" ? f.call(receiver, value) : f ? f.value = value : state.set(receiver, value)), value;
            }

            function __classPrivateFieldIn(state, receiver) {
                if (receiver === null || (typeof receiver !== "object" && typeof receiver !== "function")) throw new TypeError("Cannot use 'in' operator on non-object");
                return typeof state === "function" ? receiver === state : state.has(receiver);
            }


            /***/
        }),

        /***/ "./node_modules/idb/build/index.js":
        /*!*****************************************!*\
          !*** ./node_modules/idb/build/index.js ***!
          \*****************************************/
        /***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

            __webpack_require__.r(__webpack_exports__);
            /* harmony export */
            __webpack_require__.d(__webpack_exports__, {
                /* harmony export */
                "deleteDB": () => (/* binding */ deleteDB),
                /* harmony export */
                "openDB": () => (/* binding */ openDB),
                /* harmony export */
                "unwrap": () => (/* reexport safe */ _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.u),
                /* harmony export */
                "wrap": () => (/* reexport safe */ _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.w)
                /* harmony export */
            });
            /* harmony import */
            var _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./wrap-idb-value.js */ "./node_modules/idb/build/wrap-idb-value.js");


            /**
             * Open a database.
             *
             * @param name Name of the database.
             * @param version Schema version.
             * @param callbacks Additional callbacks.
             */
            function openDB(name, version, {blocked, upgrade, blocking, terminated} = {}) {
                const request = indexedDB.open(name, version);
                const openPromise = (0, _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.w)(request);
                if (upgrade) {
                    request.addEventListener('upgradeneeded', (event) => {
                        upgrade((0, _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.w)(request.result), event.oldVersion, event.newVersion, (0, _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.w)(request.transaction), event);
                    });
                }
                if (blocked) {
                    request.addEventListener('blocked', (event) => blocked(
                        // Casting due to https://github.com/microsoft/TypeScript-DOM-lib-generator/pull/1405
                        event.oldVersion, event.newVersion, event));
                }
                openPromise
                    .then((db) => {
                        if (terminated)
                            db.addEventListener('close', () => terminated());
                        if (blocking) {
                            db.addEventListener('versionchange', (event) => blocking(event.oldVersion, event.newVersion, event));
                        }
                    })
                    .catch(() => {
                    });
                return openPromise;
            }

            /**
             * Delete a database.
             *
             * @param name Name of the database.
             */
            function deleteDB(name, {blocked} = {}) {
                const request = indexedDB.deleteDatabase(name);
                if (blocked) {
                    request.addEventListener('blocked', (event) => blocked(
                        // Casting due to https://github.com/microsoft/TypeScript-DOM-lib-generator/pull/1405
                        event.oldVersion, event));
                }
                return (0, _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.w)(request).then(() => undefined);
            }

            const readMethods = ['get', 'getKey', 'getAll', 'getAllKeys', 'count'];
            const writeMethods = ['put', 'add', 'delete', 'clear'];
            const cachedMethods = new Map();

            function getMethod(target, prop) {
                if (!(target instanceof IDBDatabase &&
                    !(prop in target) &&
                    typeof prop === 'string')) {
                    return;
                }
                if (cachedMethods.get(prop))
                    return cachedMethods.get(prop);
                const targetFuncName = prop.replace(/FromIndex$/, '');
                const useIndex = prop !== targetFuncName;
                const isWrite = writeMethods.includes(targetFuncName);
                if (
                    // Bail if the target doesn't exist on the target. Eg, getAll isn't in Edge.
                    !(targetFuncName in (useIndex ? IDBIndex : IDBObjectStore).prototype) ||
                    !(isWrite || readMethods.includes(targetFuncName))) {
                    return;
                }
                const method = async function (storeName, ...args) {
                    // isWrite ? 'readwrite' : undefined gzipps better, but fails in Edge :(
                    const tx = this.transaction(storeName, isWrite ? 'readwrite' : 'readonly');
                    let target = tx.store;
                    if (useIndex)
                        target = target.index(args.shift());
                    // Must reject if op rejects.
                    // If it's a write operation, must reject if tx.done rejects.
                    // Must reject with op rejection first.
                    // Must resolve with op value.
                    // Must handle both promises (no unhandled rejections)
                    return (await Promise.all([
                        target[targetFuncName](...args),
                        isWrite && tx.done,
                    ]))[0];
                };
                cachedMethods.set(prop, method);
                return method;
            }

            (0, _wrap_idb_value_js__WEBPACK_IMPORTED_MODULE_0__.r)((oldTraps) => ({
                ...oldTraps,
                get: (target, prop, receiver) => getMethod(target, prop) || oldTraps.get(target, prop, receiver),
                has: (target, prop) => !!getMethod(target, prop) || oldTraps.has(target, prop),
            }));


            /***/
        }),

        /***/ "./node_modules/idb/build/wrap-idb-value.js":
        /*!**************************************************!*\
          !*** ./node_modules/idb/build/wrap-idb-value.js ***!
          \**************************************************/
        /***/ ((__unused_webpack___webpack_module__, __webpack_exports__, __webpack_require__) => {

            __webpack_require__.r(__webpack_exports__);
            /* harmony export */
            __webpack_require__.d(__webpack_exports__, {
                /* harmony export */   "a": () => (/* binding */ reverseTransformCache),
                /* harmony export */   "i": () => (/* binding */ instanceOfAny),
                /* harmony export */   "r": () => (/* binding */ replaceTraps),
                /* harmony export */   "u": () => (/* binding */ unwrap),
                /* harmony export */   "w": () => (/* binding */ wrap)
                /* harmony export */
            });
            const instanceOfAny = (object, constructors) => constructors.some((c) => object instanceof c);

            let idbProxyableTypes;
            let cursorAdvanceMethods;

// This is a function to prevent it throwing up in node environments.
            function getIdbProxyableTypes() {
                return (idbProxyableTypes ||
                    (idbProxyableTypes = [
                        IDBDatabase,
                        IDBObjectStore,
                        IDBIndex,
                        IDBCursor,
                        IDBTransaction,
                    ]));
            }

// This is a function to prevent it throwing up in node environments.
            function getCursorAdvanceMethods() {
                return (cursorAdvanceMethods ||
                    (cursorAdvanceMethods = [
                        IDBCursor.prototype.advance,
                        IDBCursor.prototype.continue,
                        IDBCursor.prototype.continuePrimaryKey,
                    ]));
            }

            const cursorRequestMap = new WeakMap();
            const transactionDoneMap = new WeakMap();
            const transactionStoreNamesMap = new WeakMap();
            const transformCache = new WeakMap();
            const reverseTransformCache = new WeakMap();

            function promisifyRequest(request) {
                const promise = new Promise((resolve, reject) => {
                    const unlisten = () => {
                        request.removeEventListener('success', success);
                        request.removeEventListener('error', error);
                    };
                    const success = () => {
                        resolve(wrap(request.result));
                        unlisten();
                    };
                    const error = () => {
                        reject(request.error);
                        unlisten();
                    };
                    request.addEventListener('success', success);
                    request.addEventListener('error', error);
                });
                promise
                    .then((value) => {
                        // Since cursoring reuses the IDBRequest (*sigh*), we cache it for later retrieval
                        // (see wrapFunction).
                        if (value instanceof IDBCursor) {
                            cursorRequestMap.set(value, request);
                        }
                        // Catching to avoid "Uncaught Promise exceptions"
                    })
                    .catch(() => {
                    });
                // This mapping exists in reverseTransformCache but doesn't doesn't exist in transformCache. This
                // is because we create many promises from a single IDBRequest.
                reverseTransformCache.set(promise, request);
                return promise;
            }

            function cacheDonePromiseForTransaction(tx) {
                // Early bail if we've already created a done promise for this transaction.
                if (transactionDoneMap.has(tx))
                    return;
                const done = new Promise((resolve, reject) => {
                    const unlisten = () => {
                        tx.removeEventListener('complete', complete);
                        tx.removeEventListener('error', error);
                        tx.removeEventListener('abort', error);
                    };
                    const complete = () => {
                        resolve();
                        unlisten();
                    };
                    const error = () => {
                        reject(tx.error || new DOMException('AbortError', 'AbortError'));
                        unlisten();
                    };
                    tx.addEventListener('complete', complete);
                    tx.addEventListener('error', error);
                    tx.addEventListener('abort', error);
                });
                // Cache it for later retrieval.
                transactionDoneMap.set(tx, done);
            }

            let idbProxyTraps = {
                get(target, prop, receiver) {
                    if (target instanceof IDBTransaction) {
                        // Special handling for transaction.done.
                        if (prop === 'done')
                            return transactionDoneMap.get(target);
                        // Polyfill for objectStoreNames because of Edge.
                        if (prop === 'objectStoreNames') {
                            return target.objectStoreNames || transactionStoreNamesMap.get(target);
                        }
                        // Make tx.store return the only store in the transaction, or undefined if there are many.
                        if (prop === 'store') {
                            return receiver.objectStoreNames[1]
                                ? undefined
                                : receiver.objectStore(receiver.objectStoreNames[0]);
                        }
                    }
                    // Else transform whatever we get back.
                    return wrap(target[prop]);
                },
                set(target, prop, value) {
                    target[prop] = value;
                    return true;
                },
                has(target, prop) {
                    if (target instanceof IDBTransaction &&
                        (prop === 'done' || prop === 'store')) {
                        return true;
                    }
                    return prop in target;
                },
            };

            function replaceTraps(callback) {
                idbProxyTraps = callback(idbProxyTraps);
            }

            function wrapFunction(func) {
                // Due to expected object equality (which is enforced by the caching in `wrap`), we
                // only create one new func per func.
                // Edge doesn't support objectStoreNames (booo), so we polyfill it here.
                if (func === IDBDatabase.prototype.transaction &&
                    !('objectStoreNames' in IDBTransaction.prototype)) {
                    return function (storeNames, ...args) {
                        const tx = func.call(unwrap(this), storeNames, ...args);
                        transactionStoreNamesMap.set(tx, storeNames.sort ? storeNames.sort() : [storeNames]);
                        return wrap(tx);
                    };
                }
                // Cursor methods are special, as the behaviour is a little more different to standard IDB. In
                // IDB, you advance the cursor and wait for a new 'success' on the IDBRequest that gave you the
                // cursor. It's kinda like a promise that can resolve with many values. That doesn't make sense
                // with real promises, so each advance methods returns a new promise for the cursor object, or
                // undefined if the end of the cursor has been reached.
                if (getCursorAdvanceMethods().includes(func)) {
                    return function (...args) {
                        // Calling the original function with the proxy as 'this' causes ILLEGAL INVOCATION, so we use
                        // the original object.
                        func.apply(unwrap(this), args);
                        return wrap(cursorRequestMap.get(this));
                    };
                }
                return function (...args) {
                    // Calling the original function with the proxy as 'this' causes ILLEGAL INVOCATION, so we use
                    // the original object.
                    return wrap(func.apply(unwrap(this), args));
                };
            }

            function transformCachableValue(value) {
                if (typeof value === 'function')
                    return wrapFunction(value);
                // This doesn't return, it just creates a 'done' promise for the transaction,
                // which is later returned for transaction.done (see idbObjectHandler).
                if (value instanceof IDBTransaction)
                    cacheDonePromiseForTransaction(value);
                if (instanceOfAny(value, getIdbProxyableTypes()))
                    return new Proxy(value, idbProxyTraps);
                // Return the same value back if we're not going to transform it.
                return value;
            }

            function wrap(value) {
                // We sometimes generate multiple promises from a single IDBRequest (eg when cursoring), because
                // IDB is weird and a single IDBRequest can yield many responses, so these can't be cached.
                if (value instanceof IDBRequest)
                    return promisifyRequest(value);
                // If we've already transformed this value before, reuse the transformed value.
                // This is faster, but it also provides object equality.
                if (transformCache.has(value))
                    return transformCache.get(value);
                const newValue = transformCachableValue(value);
                // Not all types are transformed.
                // These may be primitive types, so they can't be WeakMap keys.
                if (newValue !== value) {
                    transformCache.set(value, newValue);
                    reverseTransformCache.set(newValue, value);
                }
                return newValue;
            }

            const unwrap = (value) => reverseTransformCache.get(value);


            /***/
        })

        /******/
    });
    /************************************************************************/
    /******/ 	// The module cache
    /******/
    var __webpack_module_cache__ = {};
    /******/
    /******/ 	// The require function
    /******/
    function __webpack_require__(moduleId) {
        /******/ 		// Check if module is in cache
        /******/
        var cachedModule = __webpack_module_cache__[moduleId];
        /******/
        if (cachedModule !== undefined) {
            /******/
            return cachedModule.exports;
            /******/
        }
        /******/ 		// Create a new module (and put it into the cache)
        /******/
        var module = __webpack_module_cache__[moduleId] = {
            /******/ 			// no module.id needed
            /******/ 			// no module.loaded needed
            /******/            exports: {}
            /******/
        };
        /******/
        /******/ 		// Execute the module function
        /******/
        __webpack_modules__[moduleId](module, module.exports, __webpack_require__);
        /******/
        /******/ 		// Return the exports of the module
        /******/
        return module.exports;
        /******/
    }

    /******/
    /************************************************************************/
    /******/ 	/* webpack/runtime/define property getters */
    /******/
    (() => {
        /******/ 		// define getter functions for harmony exports
        /******/
        __webpack_require__.d = (exports, definition) => {
            /******/
            for (var key in definition) {
                /******/
                if (__webpack_require__.o(definition, key) && !__webpack_require__.o(exports, key)) {
                    /******/
                    Object.defineProperty(exports, key, {enumerable: true, get: definition[key]});
                    /******/
                }
                /******/
            }
            /******/
        };
        /******/
    })();
    /******/
    /******/ 	/* webpack/runtime/hasOwnProperty shorthand */
    /******/
    (() => {
        /******/
        __webpack_require__.o = (obj, prop) => (Object.prototype.hasOwnProperty.call(obj, prop))
        /******/
    })();
    /******/
    /******/ 	/* webpack/runtime/make namespace object */
    /******/
    (() => {
        /******/ 		// define __esModule on exports
        /******/
        __webpack_require__.r = (exports) => {
            /******/
            if (typeof Symbol !== 'undefined' && Symbol.toStringTag) {
                /******/
                Object.defineProperty(exports, Symbol.toStringTag, {value: 'Module'});
                /******/
            }
            /******/
            Object.defineProperty(exports, '__esModule', {value: true});
            /******/
        };
        /******/
    })();
    /******/
    /************************************************************************/
    var __webpack_exports__ = {};
// This entry need to be wrapped in an IIFE because it need to be isolated against other modules in the chunk.
    (() => {
        /*!*************************!*\
          !*** ./src/js/index.ts ***!
          \*************************/
        __webpack_require__.r(__webpack_exports__);
        /* harmony export */
        __webpack_require__.d(__webpack_exports__, {
            /* harmony export */   "Index": () => (/* binding */ Index)
            /* harmony export */
        });
        /* harmony import */
        var _Database_DatabaseContext__WEBPACK_IMPORTED_MODULE_0__ = __webpack_require__(/*! ./Database/DatabaseContext */ "./src/js/Database/DatabaseContext.ts");

        var Index;
        (function (Index) {
            var initDatabase = _Database_DatabaseContext__WEBPACK_IMPORTED_MODULE_0__.DatabaseContext.initDatabase;
            init();

            function init() {
                console.log("Init Index Api");
                window.isOnline = isOnline;
                window.applyUrl = applyUrl;
                initDatabase();
            }

            function applyUrl(url) {
                window.history.pushState("null", "", url);
            }

            Index.applyUrl = applyUrl;

            function isOnline() {
                return window.navigator.onLine;
            }

            Index.isOnline = isOnline;
        })(Index || (Index = {}));

    })();

    /******/
})()
;
//# sourceMappingURL=data:application/json;charset=utf-8;base64,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QUNBO0FBQ0EsQ0FBQzs7QUFFMkI7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7QUM5RjVCOztBQUVBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0wsd0JBQXdCO0FBQ3hCO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQSxLQUFLO0FBQ0w7QUFDQTtBQUNBO0FBQ0EsS0FBSztBQUNMO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBLEtBQUs7QUFDTDtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7QUFDQTtBQUNBO0FBQ0E7O0FBRXFHOzs7Ozs7O1VDeExyRztVQUNBOztVQUVBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBO1VBQ0E7VUFDQTtVQUNBOztVQUVBO1VBQ0E7O1VBRUE7VUFDQTtVQUNBOzs7OztXQ3RCQTtXQUNBO1dBQ0E7V0FDQTtXQUNBLHlDQUF5Qyx3Q0FBd0M7V0FDakY7V0FDQTtXQUNBOzs7OztXQ1BBOzs7OztXQ0FBO1dBQ0E7V0FDQTtXQUNBLHVEQUF1RCxpQkFBaUI7V0FDeEU7V0FDQSxnREFBZ0QsYUFBYTtXQUM3RDs7Ozs7Ozs7Ozs7Ozs7O0FDTjJEO0FBV3BELElBQVUsS0FBSyxDQXFCckI7QUFyQkQsV0FBaUIsS0FBSztJQUVsQixJQUFPLFlBQVksR0FBRyxtRkFBNEIsQ0FBQztJQUNuRCxJQUFJLEVBQUU7SUFFTixTQUFTLElBQUk7UUFFVCxPQUFPLENBQUMsR0FBRyxDQUFDLGdCQUFnQixDQUFDO1FBQzdCLE1BQU0sQ0FBQyxRQUFRLEdBQUcsUUFBUSxDQUFDO1FBQzNCLE1BQU0sQ0FBQyxRQUFRLEdBQUcsUUFBUSxDQUFDO1FBRTNCLFlBQVksRUFBRTtJQUNsQixDQUFDO0lBRUQsU0FBZ0IsUUFBUSxDQUFDLEdBQVU7UUFDL0IsTUFBTSxDQUFDLE9BQU8sQ0FBQyxTQUFTLENBQUMsTUFBTSxFQUFFLEVBQUUsRUFBRSxHQUFHLENBQUMsQ0FBQztJQUM5QyxDQUFDO0lBRmUsY0FBUSxXQUV2QjtJQUVELFNBQWdCLFFBQVE7UUFDcEIsT0FBTyxNQUFNLENBQUMsU0FBUyxDQUFDLE1BQU0sQ0FBQztJQUNuQyxDQUFDO0lBRmUsY0FBUSxXQUV2QjtBQUNMLENBQUMsRUFyQmdCLEtBQUssS0FBTCxLQUFLLFFBcUJyQiIsInNvdXJjZXMiOlsid2VicGFjazovL3NpbXBsZXByb2plY3RtYW5hZ2VyLy4vc3JjL2pzL0RhdGFiYXNlL0RhdGFiYXNlQ29udGV4dC50cyIsIndlYnBhY2s6Ly9zaW1wbGVwcm9qZWN0bWFuYWdlci8uL25vZGVfbW9kdWxlcy90c2xpYi90c2xpYi5lczYuanMiLCJ3ZWJwYWNrOi8vc2ltcGxlcHJvamVjdG1hbmFnZXIvLi9ub2RlX21vZHVsZXMvaWRiL2J1aWxkL2luZGV4LmpzIiwid2VicGFjazovL3NpbXBsZXByb2plY3RtYW5hZ2VyLy4vbm9kZV9tb2R1bGVzL2lkYi9idWlsZC93cmFwLWlkYi12YWx1ZS5qcyIsIndlYnBhY2s6Ly9zaW1wbGVwcm9qZWN0bWFuYWdlci93ZWJwYWNrL2Jvb3RzdHJhcCIsIndlYnBhY2s6Ly9zaW1wbGVwcm9qZWN0bWFuYWdlci93ZWJwYWNrL3J1bnRpbWUvZGVmaW5lIHByb3BlcnR5IGdldHRlcnMiLCJ3ZWJwYWNrOi8vc2ltcGxlcHJvamVjdG1hbmFnZXIvd2VicGFjay9ydW50aW1lL2hhc093blByb3BlcnR5IHNob3J0aGFuZCIsIndlYnBhY2s6Ly9zaW1wbGVwcm9qZWN0bWFuYWdlci93ZWJwYWNrL3J1bnRpbWUvbWFrZSBuYW1lc3BhY2Ugb2JqZWN0Iiwid2VicGFjazovL3NpbXBsZXByb2plY3RtYW5hZ2VyLy4vc3JjL2pzL2luZGV4LnRzIl0sInNvdXJjZXNDb250ZW50IjpbIi8vIG5vaW5zcGVjdGlvbiBKU1VudXNlZEdsb2JhbFN5bWJvbHNcclxuXHJcbmltcG9ydCB7IG9wZW5EQiwgREJTY2hlbWEsIElEQlBEYXRhYmFzZSB9IGZyb20gJ2lkYic7XHJcbmltcG9ydCAqIGFzIENvbnNvbGUgZnJvbSBcImNvbnNvbGVcIjtcclxuXHJcbmV4cG9ydCB7fVxyXG5cclxuZGVjbGFyZSBnbG9iYWwge1xyXG4gICAgaW50ZXJmYWNlIERhdGFiYXNle1xyXG4gICAgICAgIGdldENhY2hlRW50cnkoaWQ6c3RyaW5nKVxyXG4gICAgICAgIHVwZGF0ZVRpbWVvdXQoaWQ6c3RyaW5nLCBkYXRhOkpTT04pXHJcbiAgICAgICAgZ2V0VGltZW91dChpZDpzdHJpbmcpXHJcbiAgICAgICAgZGVsZXRlVGltZW91dEVsZW1lbnQoaWQ6IHN0cmluZylcclxuICAgICAgICBkZWxldGVFbGVtZW50KGlkOnN0cmluZywgdGltZW91dElkOnN0cmluZylcclxuICAgICAgICBnZXRBbGxUaW1lb3V0RWxlbWVudHMoKVxyXG4gICAgICAgIHNhdmVEYXRhKGlkOiBzdHJpbmcsIGRhdGE6IEpTT04pXHJcbiAgICB9XHJcbiAgICBcclxuICAgIGludGVyZmFjZSBXaW5kb3d7XHJcbiAgICAgICAgRGF0YWJhc2U6IERhdGFiYXNlXHJcbiAgICB9XHJcbn1cclxuXHJcbmV4cG9ydCBuYW1lc3BhY2UgRGF0YWJhc2VDb250ZXh0IHtcclxuXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gaW5pdERhdGFiYXNlKCl7XHJcbiAgICAgICAgY29uc29sZS5sb2coXCJJbml0IERhdGFiYXNlIEFwaVwiKTtcclxuICAgICAgICBcclxuICAgICAgICAvLyBAdHMtaWdub3JlXHJcbiAgICAgICAgd2luZG93LkRhdGFiYXNlID0ge31cclxuICAgICAgICB3aW5kb3cuRGF0YWJhc2UuZ2V0Q2FjaGVFbnRyeSA9IGdldENhY2hlRW50cnk7XHJcbiAgICAgICAgd2luZG93LkRhdGFiYXNlLnVwZGF0ZVRpbWVvdXQgPSB1cGRhdGVUaW1lb3V0O1xyXG4gICAgICAgIHdpbmRvdy5EYXRhYmFzZS5nZXRUaW1lb3V0ID0gZ2V0VGltZW91dDtcclxuICAgICAgICB3aW5kb3cuRGF0YWJhc2UuZGVsZXRlVGltZW91dEVsZW1lbnQgPSBkZWxldGVUaW1lb3V0RWxlbWVudDtcclxuICAgICAgICB3aW5kb3cuRGF0YWJhc2UuZGVsZXRlRWxlbWVudCA9IGRlbGV0ZUVsZW1lbnQ7XHJcbiAgICAgICAgd2luZG93LkRhdGFiYXNlLmdldEFsbFRpbWVvdXRFbGVtZW50cyA9IGdldEFsbFRpbWVvdXRFbGVtZW50cztcclxuICAgICAgICB3aW5kb3cuRGF0YWJhc2Uuc2F2ZURhdGEgPSBzYXZlRGF0YTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgaW50ZXJmYWNlIElEZWZhdWx0RGF0YWJhc2UgZXh0ZW5kcyBEQlNjaGVtYSB7XHJcbiAgICAgICAgdGltZW91dDoge1xyXG4gICAgICAgICAgICB2YWx1ZTogRGF0YUNvbnRhaW5lcjtcclxuICAgICAgICAgICAga2V5OiBzdHJpbmc7XHJcbiAgICAgICAgfTtcclxuICAgICAgICBkYXRhOiB7XHJcbiAgICAgICAgICAgIHZhbHVlOiBEYXRhQ29udGFpbmVyO1xyXG4gICAgICAgICAgICBrZXk6IHN0cmluZztcclxuICAgICAgICB9O1xyXG4gICAgfVxyXG5cclxuICAgIGNsYXNzIERhdGFDb250YWluZXIge1xyXG4gICAgICAgIGlkOiBzdHJpbmc7XHJcbiAgICAgICAgZGF0YTogSlNPTjtcclxuXHJcbiAgICAgICAgY29uc3RydWN0b3IoaWQ6IHN0cmluZywgZGF0YTogSlNPTikge1xyXG4gICAgICAgICAgICB0aGlzLmlkID0gaWQ7XHJcbiAgICAgICAgICAgIHRoaXMuZGF0YSA9IGRhdGE7XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGNsYXNzIFN0YXRpY0RhdGEge1xyXG4gICAgICAgIHN0YXRpYyBkYXRhYmFzZTogSURCUERhdGFiYXNlPElEZWZhdWx0RGF0YWJhc2U+ID0gbnVsbDtcclxuICAgIH1cclxuXHJcbiAgICBleHBvcnQgY2xhc3MgUmVzdWx0IHtcclxuICAgICAgICBzdWNlc3M6IGJvb2xlYW47XHJcbiAgICAgICAgbWVzc2FnZTogc3RyaW5nO1xyXG5cclxuICAgICAgICBjb25zdHJ1Y3Rvcihtc2c/OiBzdHJpbmcpIHtcclxuICAgICAgICAgICAgaWYgKG1zZyA9PT0gdW5kZWZpbmVkKSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN1Y2VzcyA9IHRydWU7XHJcbiAgICAgICAgICAgIH0gZWxzZSB7XHJcbiAgICAgICAgICAgICAgICB0aGlzLnN1Y2VzcyA9IGZhbHNlO1xyXG4gICAgICAgICAgICAgICAgdGhpcy5tZXNzYWdlID0gbXNnO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG5cclxuICAgIGFzeW5jIGZ1bmN0aW9uIG9wZW5EYXRhRGF0YWJhc2UoKSB7XHJcblxyXG4gICAgICAgIGlmIChTdGF0aWNEYXRhLmRhdGFiYXNlID09IG51bGwpIHtcclxuICAgICAgICAgICAgU3RhdGljRGF0YS5kYXRhYmFzZSA9IGF3YWl0IG9wZW5EQjxJRGVmYXVsdERhdGFiYXNlPihcIlN0YXRlRGF0YVwiLFxyXG4gICAgICAgICAgICAgICAgMSxcclxuICAgICAgICAgICAgICAgIHtcclxuICAgICAgICAgICAgICAgICAgICB1cGdyYWRlKGRiKSB7XHJcbiAgICAgICAgICAgICAgICAgICAgICAgIGRiLmNyZWF0ZU9iamVjdFN0b3JlKFwidGltZW91dFwiLCB7IGtleVBhdGg6IFwiaWRcIiB9KTtcclxuICAgICAgICAgICAgICAgICAgICAgICAgZGIuY3JlYXRlT2JqZWN0U3RvcmUoXCJkYXRhXCIsIHsga2V5UGF0aDogXCJpZFwiIH0pO1xyXG4gICAgICAgICAgICAgICAgICAgIH1cclxuICAgICAgICAgICAgICAgIH0pO1xyXG4gICAgICAgICAgICBcclxuICAgICAgICB9XHJcblxyXG4gICAgICAgIHJldHVybiBTdGF0aWNEYXRhLmRhdGFiYXNlO1xyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBhc3luYyBmdW5jdGlvbiBzYXZlRGF0YShpZDogc3RyaW5nLCBkYXRhOiBKU09OKSB7XHJcbiAgICAgICAgICAgIGNvbnN0IGRiID0gYXdhaXQgb3BlbkRhdGFEYXRhYmFzZSgpO1xyXG4gICAgICAgICAgICBhd2FpdCBkYi5wdXQoXCJkYXRhXCIsIG5ldyBEYXRhQ29udGFpbmVyKGlkLCBkYXRhKSk7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGV4cG9ydCBhc3luYyBmdW5jdGlvbiBnZXRBbGxUaW1lb3V0RWxlbWVudHMoKSB7XHJcbiAgICAgICAgY29uc3QgZGIgPSBhd2FpdCBvcGVuRGF0YURhdGFiYXNlKCk7XHJcblxyXG4gICAgICAgIGNvbnN0IGRhdGFMaXN0ID0gYXdhaXQgZGIuZ2V0QWxsKFwidGltZW91dFwiKTtcclxuXHJcbiAgICAgICAgcmV0dXJuIGRhdGFMaXN0Lm1hcCgodjogRGF0YUNvbnRhaW5lcikgPT4gdi5kYXRhKTtcclxuICAgIH1cclxuXHJcbiAgICBleHBvcnQgYXN5bmMgZnVuY3Rpb24gZGVsZXRlRWxlbWVudChpZDpzdHJpbmcsIHRpbWVvdXRJZDpzdHJpbmcpIHtcclxuICAgICAgICBjb25zdCBkYiA9IGF3YWl0IG9wZW5EYXRhRGF0YWJhc2UoKTtcclxuICAgICAgICBjb25zdCB0cmFuc2FjdGlvbiA9IGRiLnRyYW5zYWN0aW9uKFtcInRpbWVvdXRcIiwgXCJkYXRhXCJdLCBcInJlYWR3cml0ZVwiKTtcclxuXHJcbiAgICAgICAgY29uc3QgdGltZW91dFN0b3JlID0gdHJhbnNhY3Rpb24ub2JqZWN0U3RvcmUodHJhbnNhY3Rpb24ub2JqZWN0U3RvcmVOYW1lc1swXSk7XHJcbiAgICAgICAgY29uc3QgZGF0YVN0b3JlID0gdHJhbnNhY3Rpb24ub2JqZWN0U3RvcmUodHJhbnNhY3Rpb24ub2JqZWN0U3RvcmVOYW1lc1sxXSk7XHJcblxyXG4gICAgICAgIGNvbnN0IHRpbWVvdXREYXRhID0gYXdhaXQgdGltZW91dFN0b3JlLmdldCh0aW1lb3V0SWQpO1xyXG5cclxuICAgICAgICBpZiAodGltZW91dERhdGEgIT09IHVuZGVmaW5lZCkge1xyXG4gICAgICAgICAgICBhd2FpdCB0aW1lb3V0U3RvcmUuZGVsZXRlKHRpbWVvdXRJZCk7XHJcbiAgICAgICAgfVxyXG4gICAgICAgIGF3YWl0IGRhdGFTdG9yZS5kZWxldGUoaWQpO1xyXG5cclxuICAgICAgICBhd2FpdCB0cmFuc2FjdGlvbi5kb25lO1xyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBhc3luYyBmdW5jdGlvbiBkZWxldGVUaW1lb3V0RWxlbWVudChpZDogc3RyaW5nKSB7XHJcbiAgICAgICAgdHJ5IHtcclxuICAgICAgICAgICAgY29uc3QgZGIgPSBhd2FpdCBvcGVuRGF0YURhdGFiYXNlKCk7XHJcbiAgICAgICAgICAgIGF3YWl0IGRiLmRlbGV0ZShcInRpbWVvdXRcIiwgaWQpO1xyXG5cclxuICAgICAgICAgICAgcmV0dXJuIG5ldyBSZXN1bHQoKTtcclxuICAgICAgICB9IGNhdGNoIChlKSB7XHJcbiAgICAgICAgICAgIHJldHVybiBuZXcgUmVzdWx0KGUudG9TdHJpbmcoKSk7XHJcbiAgICAgICAgfSBcclxuICAgIH1cclxuXHJcbiAgICBleHBvcnQgYXN5bmMgZnVuY3Rpb24gZ2V0VGltZW91dChpZDpzdHJpbmcpe1xyXG4gICAgICAgIGNvbnN0IGRiID0gYXdhaXQgb3BlbkRhdGFEYXRhYmFzZSgpO1xyXG4gICAgICAgIGNvbnN0IGVudHJ5ID0gYXdhaXQgZGIuZ2V0KFwidGltZW91dFwiLCBpZCk7XHJcbiAgICAgICAgaWYoZW50cnkgPT09IHVuZGVmaW5lZCl7XHJcbiAgICAgICAgICAgIHJldHVybiBcIlwiO1xyXG4gICAgICAgIH1cclxuICAgICAgICBcclxuICAgICAgICByZXR1cm4gZW50cnkuZGF0YTtcclxuICAgIH1cclxuICAgIFxyXG4gICAgZXhwb3J0IGFzeW5jIGZ1bmN0aW9uIHVwZGF0ZVRpbWVvdXQoaWQ6c3RyaW5nLCBkYXRhOkpTT04pe1xyXG4gICAgICAgIGNvbnN0IGRiID0gYXdhaXQgb3BlbkRhdGFEYXRhYmFzZSgpO1xyXG4gICAgICAgIGF3YWl0IGRiLnB1dChcInRpbWVvdXRcIiwgbmV3IERhdGFDb250YWluZXIoaWQsIGRhdGEpLCBpZCk7XHJcbiAgICB9XHJcbiAgICBcclxuICAgIGV4cG9ydCBhc3luYyBmdW5jdGlvbiBnZXRDYWNoZUVudHJ5KGlkOnN0cmluZykge1xyXG4gICAgICAgIGNvbnN0IGRiID0gYXdhaXQgb3BlbkRhdGFEYXRhYmFzZSgpO1xyXG4gICAgICAgIFxyXG4gICAgICAgIGNvbnN0IHJlc3VsdCA9IGF3YWl0IGRiLmdldChcImRhdGFcIiwgaWQpO1xyXG4gICAgXHJcbiAgICAgICAgaWYocmVzdWx0ID09PSB1bmRlZmluZWQpIHJldHVybiBcIlwiO1xyXG4gICAgICAgIFxyXG4gICAgICAgIHJldHVybiByZXN1bHQuZGF0YTtcclxuICAgIH1cclxufSIsIi8qKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKipcclxuQ29weXJpZ2h0IChjKSBNaWNyb3NvZnQgQ29ycG9yYXRpb24uXHJcblxyXG5QZXJtaXNzaW9uIHRvIHVzZSwgY29weSwgbW9kaWZ5LCBhbmQvb3IgZGlzdHJpYnV0ZSB0aGlzIHNvZnR3YXJlIGZvciBhbnlcclxucHVycG9zZSB3aXRoIG9yIHdpdGhvdXQgZmVlIGlzIGhlcmVieSBncmFudGVkLlxyXG5cclxuVEhFIFNPRlRXQVJFIElTIFBST1ZJREVEIFwiQVMgSVNcIiBBTkQgVEhFIEFVVEhPUiBESVNDTEFJTVMgQUxMIFdBUlJBTlRJRVMgV0lUSFxyXG5SRUdBUkQgVE8gVEhJUyBTT0ZUV0FSRSBJTkNMVURJTkcgQUxMIElNUExJRUQgV0FSUkFOVElFUyBPRiBNRVJDSEFOVEFCSUxJVFlcclxuQU5EIEZJVE5FU1MuIElOIE5PIEVWRU5UIFNIQUxMIFRIRSBBVVRIT1IgQkUgTElBQkxFIEZPUiBBTlkgU1BFQ0lBTCwgRElSRUNULFxyXG5JTkRJUkVDVCwgT1IgQ09OU0VRVUVOVElBTCBEQU1BR0VTIE9SIEFOWSBEQU1BR0VTIFdIQVRTT0VWRVIgUkVTVUxUSU5HIEZST01cclxuTE9TUyBPRiBVU0UsIERBVEEgT1IgUFJPRklUUywgV0hFVEhFUiBJTiBBTiBBQ1RJT04gT0YgQ09OVFJBQ1QsIE5FR0xJR0VOQ0UgT1JcclxuT1RIRVIgVE9SVElPVVMgQUNUSU9OLCBBUklTSU5HIE9VVCBPRiBPUiBJTiBDT05ORUNUSU9OIFdJVEggVEhFIFVTRSBPUlxyXG5QRVJGT1JNQU5DRSBPRiBUSElTIFNPRlRXQVJFLlxyXG4qKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKioqKiAqL1xyXG4vKiBnbG9iYWwgUmVmbGVjdCwgUHJvbWlzZSAqL1xyXG5cclxudmFyIGV4dGVuZFN0YXRpY3MgPSBmdW5jdGlvbihkLCBiKSB7XHJcbiAgICBleHRlbmRTdGF0aWNzID0gT2JqZWN0LnNldFByb3RvdHlwZU9mIHx8XHJcbiAgICAgICAgKHsgX19wcm90b19fOiBbXSB9IGluc3RhbmNlb2YgQXJyYXkgJiYgZnVuY3Rpb24gKGQsIGIpIHsgZC5fX3Byb3RvX18gPSBiOyB9KSB8fFxyXG4gICAgICAgIGZ1bmN0aW9uIChkLCBiKSB7IGZvciAodmFyIHAgaW4gYikgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChiLCBwKSkgZFtwXSA9IGJbcF07IH07XHJcbiAgICByZXR1cm4gZXh0ZW5kU3RhdGljcyhkLCBiKTtcclxufTtcclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2V4dGVuZHMoZCwgYikge1xyXG4gICAgaWYgKHR5cGVvZiBiICE9PSBcImZ1bmN0aW9uXCIgJiYgYiAhPT0gbnVsbClcclxuICAgICAgICB0aHJvdyBuZXcgVHlwZUVycm9yKFwiQ2xhc3MgZXh0ZW5kcyB2YWx1ZSBcIiArIFN0cmluZyhiKSArIFwiIGlzIG5vdCBhIGNvbnN0cnVjdG9yIG9yIG51bGxcIik7XHJcbiAgICBleHRlbmRTdGF0aWNzKGQsIGIpO1xyXG4gICAgZnVuY3Rpb24gX18oKSB7IHRoaXMuY29uc3RydWN0b3IgPSBkOyB9XHJcbiAgICBkLnByb3RvdHlwZSA9IGIgPT09IG51bGwgPyBPYmplY3QuY3JlYXRlKGIpIDogKF9fLnByb3RvdHlwZSA9IGIucHJvdG90eXBlLCBuZXcgX18oKSk7XHJcbn1cclxuXHJcbmV4cG9ydCB2YXIgX19hc3NpZ24gPSBmdW5jdGlvbigpIHtcclxuICAgIF9fYXNzaWduID0gT2JqZWN0LmFzc2lnbiB8fCBmdW5jdGlvbiBfX2Fzc2lnbih0KSB7XHJcbiAgICAgICAgZm9yICh2YXIgcywgaSA9IDEsIG4gPSBhcmd1bWVudHMubGVuZ3RoOyBpIDwgbjsgaSsrKSB7XHJcbiAgICAgICAgICAgIHMgPSBhcmd1bWVudHNbaV07XHJcbiAgICAgICAgICAgIGZvciAodmFyIHAgaW4gcykgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzLCBwKSkgdFtwXSA9IHNbcF07XHJcbiAgICAgICAgfVxyXG4gICAgICAgIHJldHVybiB0O1xyXG4gICAgfVxyXG4gICAgcmV0dXJuIF9fYXNzaWduLmFwcGx5KHRoaXMsIGFyZ3VtZW50cyk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3Jlc3QocywgZSkge1xyXG4gICAgdmFyIHQgPSB7fTtcclxuICAgIGZvciAodmFyIHAgaW4gcykgaWYgKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChzLCBwKSAmJiBlLmluZGV4T2YocCkgPCAwKVxyXG4gICAgICAgIHRbcF0gPSBzW3BdO1xyXG4gICAgaWYgKHMgIT0gbnVsbCAmJiB0eXBlb2YgT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9scyA9PT0gXCJmdW5jdGlvblwiKVxyXG4gICAgICAgIGZvciAodmFyIGkgPSAwLCBwID0gT2JqZWN0LmdldE93blByb3BlcnR5U3ltYm9scyhzKTsgaSA8IHAubGVuZ3RoOyBpKyspIHtcclxuICAgICAgICAgICAgaWYgKGUuaW5kZXhPZihwW2ldKSA8IDAgJiYgT2JqZWN0LnByb3RvdHlwZS5wcm9wZXJ0eUlzRW51bWVyYWJsZS5jYWxsKHMsIHBbaV0pKVxyXG4gICAgICAgICAgICAgICAgdFtwW2ldXSA9IHNbcFtpXV07XHJcbiAgICAgICAgfVxyXG4gICAgcmV0dXJuIHQ7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2RlY29yYXRlKGRlY29yYXRvcnMsIHRhcmdldCwga2V5LCBkZXNjKSB7XHJcbiAgICB2YXIgYyA9IGFyZ3VtZW50cy5sZW5ndGgsIHIgPSBjIDwgMyA/IHRhcmdldCA6IGRlc2MgPT09IG51bGwgPyBkZXNjID0gT2JqZWN0LmdldE93blByb3BlcnR5RGVzY3JpcHRvcih0YXJnZXQsIGtleSkgOiBkZXNjLCBkO1xyXG4gICAgaWYgKHR5cGVvZiBSZWZsZWN0ID09PSBcIm9iamVjdFwiICYmIHR5cGVvZiBSZWZsZWN0LmRlY29yYXRlID09PSBcImZ1bmN0aW9uXCIpIHIgPSBSZWZsZWN0LmRlY29yYXRlKGRlY29yYXRvcnMsIHRhcmdldCwga2V5LCBkZXNjKTtcclxuICAgIGVsc2UgZm9yICh2YXIgaSA9IGRlY29yYXRvcnMubGVuZ3RoIC0gMTsgaSA+PSAwOyBpLS0pIGlmIChkID0gZGVjb3JhdG9yc1tpXSkgciA9IChjIDwgMyA/IGQocikgOiBjID4gMyA/IGQodGFyZ2V0LCBrZXksIHIpIDogZCh0YXJnZXQsIGtleSkpIHx8IHI7XHJcbiAgICByZXR1cm4gYyA+IDMgJiYgciAmJiBPYmplY3QuZGVmaW5lUHJvcGVydHkodGFyZ2V0LCBrZXksIHIpLCByO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19wYXJhbShwYXJhbUluZGV4LCBkZWNvcmF0b3IpIHtcclxuICAgIHJldHVybiBmdW5jdGlvbiAodGFyZ2V0LCBrZXkpIHsgZGVjb3JhdG9yKHRhcmdldCwga2V5LCBwYXJhbUluZGV4KTsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19tZXRhZGF0YShtZXRhZGF0YUtleSwgbWV0YWRhdGFWYWx1ZSkge1xyXG4gICAgaWYgKHR5cGVvZiBSZWZsZWN0ID09PSBcIm9iamVjdFwiICYmIHR5cGVvZiBSZWZsZWN0Lm1ldGFkYXRhID09PSBcImZ1bmN0aW9uXCIpIHJldHVybiBSZWZsZWN0Lm1ldGFkYXRhKG1ldGFkYXRhS2V5LCBtZXRhZGF0YVZhbHVlKTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fYXdhaXRlcih0aGlzQXJnLCBfYXJndW1lbnRzLCBQLCBnZW5lcmF0b3IpIHtcclxuICAgIGZ1bmN0aW9uIGFkb3B0KHZhbHVlKSB7IHJldHVybiB2YWx1ZSBpbnN0YW5jZW9mIFAgPyB2YWx1ZSA6IG5ldyBQKGZ1bmN0aW9uIChyZXNvbHZlKSB7IHJlc29sdmUodmFsdWUpOyB9KTsgfVxyXG4gICAgcmV0dXJuIG5ldyAoUCB8fCAoUCA9IFByb21pc2UpKShmdW5jdGlvbiAocmVzb2x2ZSwgcmVqZWN0KSB7XHJcbiAgICAgICAgZnVuY3Rpb24gZnVsZmlsbGVkKHZhbHVlKSB7IHRyeSB7IHN0ZXAoZ2VuZXJhdG9yLm5leHQodmFsdWUpKTsgfSBjYXRjaCAoZSkgeyByZWplY3QoZSk7IH0gfVxyXG4gICAgICAgIGZ1bmN0aW9uIHJlamVjdGVkKHZhbHVlKSB7IHRyeSB7IHN0ZXAoZ2VuZXJhdG9yW1widGhyb3dcIl0odmFsdWUpKTsgfSBjYXRjaCAoZSkgeyByZWplY3QoZSk7IH0gfVxyXG4gICAgICAgIGZ1bmN0aW9uIHN0ZXAocmVzdWx0KSB7IHJlc3VsdC5kb25lID8gcmVzb2x2ZShyZXN1bHQudmFsdWUpIDogYWRvcHQocmVzdWx0LnZhbHVlKS50aGVuKGZ1bGZpbGxlZCwgcmVqZWN0ZWQpOyB9XHJcbiAgICAgICAgc3RlcCgoZ2VuZXJhdG9yID0gZ2VuZXJhdG9yLmFwcGx5KHRoaXNBcmcsIF9hcmd1bWVudHMgfHwgW10pKS5uZXh0KCkpO1xyXG4gICAgfSk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2dlbmVyYXRvcih0aGlzQXJnLCBib2R5KSB7XHJcbiAgICB2YXIgXyA9IHsgbGFiZWw6IDAsIHNlbnQ6IGZ1bmN0aW9uKCkgeyBpZiAodFswXSAmIDEpIHRocm93IHRbMV07IHJldHVybiB0WzFdOyB9LCB0cnlzOiBbXSwgb3BzOiBbXSB9LCBmLCB5LCB0LCBnO1xyXG4gICAgcmV0dXJuIGcgPSB7IG5leHQ6IHZlcmIoMCksIFwidGhyb3dcIjogdmVyYigxKSwgXCJyZXR1cm5cIjogdmVyYigyKSB9LCB0eXBlb2YgU3ltYm9sID09PSBcImZ1bmN0aW9uXCIgJiYgKGdbU3ltYm9sLml0ZXJhdG9yXSA9IGZ1bmN0aW9uKCkgeyByZXR1cm4gdGhpczsgfSksIGc7XHJcbiAgICBmdW5jdGlvbiB2ZXJiKG4pIHsgcmV0dXJuIGZ1bmN0aW9uICh2KSB7IHJldHVybiBzdGVwKFtuLCB2XSk7IH07IH1cclxuICAgIGZ1bmN0aW9uIHN0ZXAob3ApIHtcclxuICAgICAgICBpZiAoZikgdGhyb3cgbmV3IFR5cGVFcnJvcihcIkdlbmVyYXRvciBpcyBhbHJlYWR5IGV4ZWN1dGluZy5cIik7XHJcbiAgICAgICAgd2hpbGUgKF8pIHRyeSB7XHJcbiAgICAgICAgICAgIGlmIChmID0gMSwgeSAmJiAodCA9IG9wWzBdICYgMiA/IHlbXCJyZXR1cm5cIl0gOiBvcFswXSA/IHlbXCJ0aHJvd1wiXSB8fCAoKHQgPSB5W1wicmV0dXJuXCJdKSAmJiB0LmNhbGwoeSksIDApIDogeS5uZXh0KSAmJiAhKHQgPSB0LmNhbGwoeSwgb3BbMV0pKS5kb25lKSByZXR1cm4gdDtcclxuICAgICAgICAgICAgaWYgKHkgPSAwLCB0KSBvcCA9IFtvcFswXSAmIDIsIHQudmFsdWVdO1xyXG4gICAgICAgICAgICBzd2l0Y2ggKG9wWzBdKSB7XHJcbiAgICAgICAgICAgICAgICBjYXNlIDA6IGNhc2UgMTogdCA9IG9wOyBicmVhaztcclxuICAgICAgICAgICAgICAgIGNhc2UgNDogXy5sYWJlbCsrOyByZXR1cm4geyB2YWx1ZTogb3BbMV0sIGRvbmU6IGZhbHNlIH07XHJcbiAgICAgICAgICAgICAgICBjYXNlIDU6IF8ubGFiZWwrKzsgeSA9IG9wWzFdOyBvcCA9IFswXTsgY29udGludWU7XHJcbiAgICAgICAgICAgICAgICBjYXNlIDc6IG9wID0gXy5vcHMucG9wKCk7IF8udHJ5cy5wb3AoKTsgY29udGludWU7XHJcbiAgICAgICAgICAgICAgICBkZWZhdWx0OlxyXG4gICAgICAgICAgICAgICAgICAgIGlmICghKHQgPSBfLnRyeXMsIHQgPSB0Lmxlbmd0aCA+IDAgJiYgdFt0Lmxlbmd0aCAtIDFdKSAmJiAob3BbMF0gPT09IDYgfHwgb3BbMF0gPT09IDIpKSB7IF8gPSAwOyBjb250aW51ZTsgfVxyXG4gICAgICAgICAgICAgICAgICAgIGlmIChvcFswXSA9PT0gMyAmJiAoIXQgfHwgKG9wWzFdID4gdFswXSAmJiBvcFsxXSA8IHRbM10pKSkgeyBfLmxhYmVsID0gb3BbMV07IGJyZWFrOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKG9wWzBdID09PSA2ICYmIF8ubGFiZWwgPCB0WzFdKSB7IF8ubGFiZWwgPSB0WzFdOyB0ID0gb3A7IGJyZWFrOyB9XHJcbiAgICAgICAgICAgICAgICAgICAgaWYgKHQgJiYgXy5sYWJlbCA8IHRbMl0pIHsgXy5sYWJlbCA9IHRbMl07IF8ub3BzLnB1c2gob3ApOyBicmVhazsgfVxyXG4gICAgICAgICAgICAgICAgICAgIGlmICh0WzJdKSBfLm9wcy5wb3AoKTtcclxuICAgICAgICAgICAgICAgICAgICBfLnRyeXMucG9wKCk7IGNvbnRpbnVlO1xyXG4gICAgICAgICAgICB9XHJcbiAgICAgICAgICAgIG9wID0gYm9keS5jYWxsKHRoaXNBcmcsIF8pO1xyXG4gICAgICAgIH0gY2F0Y2ggKGUpIHsgb3AgPSBbNiwgZV07IHkgPSAwOyB9IGZpbmFsbHkgeyBmID0gdCA9IDA7IH1cclxuICAgICAgICBpZiAob3BbMF0gJiA1KSB0aHJvdyBvcFsxXTsgcmV0dXJuIHsgdmFsdWU6IG9wWzBdID8gb3BbMV0gOiB2b2lkIDAsIGRvbmU6IHRydWUgfTtcclxuICAgIH1cclxufVxyXG5cclxuZXhwb3J0IHZhciBfX2NyZWF0ZUJpbmRpbmcgPSBPYmplY3QuY3JlYXRlID8gKGZ1bmN0aW9uKG8sIG0sIGssIGsyKSB7XHJcbiAgICBpZiAoazIgPT09IHVuZGVmaW5lZCkgazIgPSBrO1xyXG4gICAgdmFyIGRlc2MgPSBPYmplY3QuZ2V0T3duUHJvcGVydHlEZXNjcmlwdG9yKG0sIGspO1xyXG4gICAgaWYgKCFkZXNjIHx8IChcImdldFwiIGluIGRlc2MgPyAhbS5fX2VzTW9kdWxlIDogZGVzYy53cml0YWJsZSB8fCBkZXNjLmNvbmZpZ3VyYWJsZSkpIHtcclxuICAgICAgICBkZXNjID0geyBlbnVtZXJhYmxlOiB0cnVlLCBnZXQ6IGZ1bmN0aW9uKCkgeyByZXR1cm4gbVtrXTsgfSB9O1xyXG4gICAgfVxyXG4gICAgT2JqZWN0LmRlZmluZVByb3BlcnR5KG8sIGsyLCBkZXNjKTtcclxufSkgOiAoZnVuY3Rpb24obywgbSwgaywgazIpIHtcclxuICAgIGlmIChrMiA9PT0gdW5kZWZpbmVkKSBrMiA9IGs7XHJcbiAgICBvW2syXSA9IG1ba107XHJcbn0pO1xyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fZXhwb3J0U3RhcihtLCBvKSB7XHJcbiAgICBmb3IgKHZhciBwIGluIG0pIGlmIChwICE9PSBcImRlZmF1bHRcIiAmJiAhT2JqZWN0LnByb3RvdHlwZS5oYXNPd25Qcm9wZXJ0eS5jYWxsKG8sIHApKSBfX2NyZWF0ZUJpbmRpbmcobywgbSwgcCk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX3ZhbHVlcyhvKSB7XHJcbiAgICB2YXIgcyA9IHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiBTeW1ib2wuaXRlcmF0b3IsIG0gPSBzICYmIG9bc10sIGkgPSAwO1xyXG4gICAgaWYgKG0pIHJldHVybiBtLmNhbGwobyk7XHJcbiAgICBpZiAobyAmJiB0eXBlb2Ygby5sZW5ndGggPT09IFwibnVtYmVyXCIpIHJldHVybiB7XHJcbiAgICAgICAgbmV4dDogZnVuY3Rpb24gKCkge1xyXG4gICAgICAgICAgICBpZiAobyAmJiBpID49IG8ubGVuZ3RoKSBvID0gdm9pZCAwO1xyXG4gICAgICAgICAgICByZXR1cm4geyB2YWx1ZTogbyAmJiBvW2krK10sIGRvbmU6ICFvIH07XHJcbiAgICAgICAgfVxyXG4gICAgfTtcclxuICAgIHRocm93IG5ldyBUeXBlRXJyb3IocyA/IFwiT2JqZWN0IGlzIG5vdCBpdGVyYWJsZS5cIiA6IFwiU3ltYm9sLml0ZXJhdG9yIGlzIG5vdCBkZWZpbmVkLlwiKTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fcmVhZChvLCBuKSB7XHJcbiAgICB2YXIgbSA9IHR5cGVvZiBTeW1ib2wgPT09IFwiZnVuY3Rpb25cIiAmJiBvW1N5bWJvbC5pdGVyYXRvcl07XHJcbiAgICBpZiAoIW0pIHJldHVybiBvO1xyXG4gICAgdmFyIGkgPSBtLmNhbGwobyksIHIsIGFyID0gW10sIGU7XHJcbiAgICB0cnkge1xyXG4gICAgICAgIHdoaWxlICgobiA9PT0gdm9pZCAwIHx8IG4tLSA+IDApICYmICEociA9IGkubmV4dCgpKS5kb25lKSBhci5wdXNoKHIudmFsdWUpO1xyXG4gICAgfVxyXG4gICAgY2F0Y2ggKGVycm9yKSB7IGUgPSB7IGVycm9yOiBlcnJvciB9OyB9XHJcbiAgICBmaW5hbGx5IHtcclxuICAgICAgICB0cnkge1xyXG4gICAgICAgICAgICBpZiAociAmJiAhci5kb25lICYmIChtID0gaVtcInJldHVyblwiXSkpIG0uY2FsbChpKTtcclxuICAgICAgICB9XHJcbiAgICAgICAgZmluYWxseSB7IGlmIChlKSB0aHJvdyBlLmVycm9yOyB9XHJcbiAgICB9XHJcbiAgICByZXR1cm4gYXI7XHJcbn1cclxuXHJcbi8qKiBAZGVwcmVjYXRlZCAqL1xyXG5leHBvcnQgZnVuY3Rpb24gX19zcHJlYWQoKSB7XHJcbiAgICBmb3IgKHZhciBhciA9IFtdLCBpID0gMDsgaSA8IGFyZ3VtZW50cy5sZW5ndGg7IGkrKylcclxuICAgICAgICBhciA9IGFyLmNvbmNhdChfX3JlYWQoYXJndW1lbnRzW2ldKSk7XHJcbiAgICByZXR1cm4gYXI7XHJcbn1cclxuXHJcbi8qKiBAZGVwcmVjYXRlZCAqL1xyXG5leHBvcnQgZnVuY3Rpb24gX19zcHJlYWRBcnJheXMoKSB7XHJcbiAgICBmb3IgKHZhciBzID0gMCwgaSA9IDAsIGlsID0gYXJndW1lbnRzLmxlbmd0aDsgaSA8IGlsOyBpKyspIHMgKz0gYXJndW1lbnRzW2ldLmxlbmd0aDtcclxuICAgIGZvciAodmFyIHIgPSBBcnJheShzKSwgayA9IDAsIGkgPSAwOyBpIDwgaWw7IGkrKylcclxuICAgICAgICBmb3IgKHZhciBhID0gYXJndW1lbnRzW2ldLCBqID0gMCwgamwgPSBhLmxlbmd0aDsgaiA8IGpsOyBqKyssIGsrKylcclxuICAgICAgICAgICAgcltrXSA9IGFbal07XHJcbiAgICByZXR1cm4gcjtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fc3ByZWFkQXJyYXkodG8sIGZyb20sIHBhY2spIHtcclxuICAgIGlmIChwYWNrIHx8IGFyZ3VtZW50cy5sZW5ndGggPT09IDIpIGZvciAodmFyIGkgPSAwLCBsID0gZnJvbS5sZW5ndGgsIGFyOyBpIDwgbDsgaSsrKSB7XHJcbiAgICAgICAgaWYgKGFyIHx8ICEoaSBpbiBmcm9tKSkge1xyXG4gICAgICAgICAgICBpZiAoIWFyKSBhciA9IEFycmF5LnByb3RvdHlwZS5zbGljZS5jYWxsKGZyb20sIDAsIGkpO1xyXG4gICAgICAgICAgICBhcltpXSA9IGZyb21baV07XHJcbiAgICAgICAgfVxyXG4gICAgfVxyXG4gICAgcmV0dXJuIHRvLmNvbmNhdChhciB8fCBBcnJheS5wcm90b3R5cGUuc2xpY2UuY2FsbChmcm9tKSk7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2F3YWl0KHYpIHtcclxuICAgIHJldHVybiB0aGlzIGluc3RhbmNlb2YgX19hd2FpdCA/ICh0aGlzLnYgPSB2LCB0aGlzKSA6IG5ldyBfX2F3YWl0KHYpO1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hc3luY0dlbmVyYXRvcih0aGlzQXJnLCBfYXJndW1lbnRzLCBnZW5lcmF0b3IpIHtcclxuICAgIGlmICghU3ltYm9sLmFzeW5jSXRlcmF0b3IpIHRocm93IG5ldyBUeXBlRXJyb3IoXCJTeW1ib2wuYXN5bmNJdGVyYXRvciBpcyBub3QgZGVmaW5lZC5cIik7XHJcbiAgICB2YXIgZyA9IGdlbmVyYXRvci5hcHBseSh0aGlzQXJnLCBfYXJndW1lbnRzIHx8IFtdKSwgaSwgcSA9IFtdO1xyXG4gICAgcmV0dXJuIGkgPSB7fSwgdmVyYihcIm5leHRcIiksIHZlcmIoXCJ0aHJvd1wiKSwgdmVyYihcInJldHVyblwiKSwgaVtTeW1ib2wuYXN5bmNJdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpO1xyXG4gICAgZnVuY3Rpb24gdmVyYihuKSB7IGlmIChnW25dKSBpW25dID0gZnVuY3Rpb24gKHYpIHsgcmV0dXJuIG5ldyBQcm9taXNlKGZ1bmN0aW9uIChhLCBiKSB7IHEucHVzaChbbiwgdiwgYSwgYl0pID4gMSB8fCByZXN1bWUobiwgdik7IH0pOyB9OyB9XHJcbiAgICBmdW5jdGlvbiByZXN1bWUobiwgdikgeyB0cnkgeyBzdGVwKGdbbl0odikpOyB9IGNhdGNoIChlKSB7IHNldHRsZShxWzBdWzNdLCBlKTsgfSB9XHJcbiAgICBmdW5jdGlvbiBzdGVwKHIpIHsgci52YWx1ZSBpbnN0YW5jZW9mIF9fYXdhaXQgPyBQcm9taXNlLnJlc29sdmUoci52YWx1ZS52KS50aGVuKGZ1bGZpbGwsIHJlamVjdCkgOiBzZXR0bGUocVswXVsyXSwgcik7IH1cclxuICAgIGZ1bmN0aW9uIGZ1bGZpbGwodmFsdWUpIHsgcmVzdW1lKFwibmV4dFwiLCB2YWx1ZSk7IH1cclxuICAgIGZ1bmN0aW9uIHJlamVjdCh2YWx1ZSkgeyByZXN1bWUoXCJ0aHJvd1wiLCB2YWx1ZSk7IH1cclxuICAgIGZ1bmN0aW9uIHNldHRsZShmLCB2KSB7IGlmIChmKHYpLCBxLnNoaWZ0KCksIHEubGVuZ3RoKSByZXN1bWUocVswXVswXSwgcVswXVsxXSk7IH1cclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fYXN5bmNEZWxlZ2F0b3Iobykge1xyXG4gICAgdmFyIGksIHA7XHJcbiAgICByZXR1cm4gaSA9IHt9LCB2ZXJiKFwibmV4dFwiKSwgdmVyYihcInRocm93XCIsIGZ1bmN0aW9uIChlKSB7IHRocm93IGU7IH0pLCB2ZXJiKFwicmV0dXJuXCIpLCBpW1N5bWJvbC5pdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpO1xyXG4gICAgZnVuY3Rpb24gdmVyYihuLCBmKSB7IGlbbl0gPSBvW25dID8gZnVuY3Rpb24gKHYpIHsgcmV0dXJuIChwID0gIXApID8geyB2YWx1ZTogX19hd2FpdChvW25dKHYpKSwgZG9uZTogbiA9PT0gXCJyZXR1cm5cIiB9IDogZiA/IGYodikgOiB2OyB9IDogZjsgfVxyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19hc3luY1ZhbHVlcyhvKSB7XHJcbiAgICBpZiAoIVN5bWJvbC5hc3luY0l0ZXJhdG9yKSB0aHJvdyBuZXcgVHlwZUVycm9yKFwiU3ltYm9sLmFzeW5jSXRlcmF0b3IgaXMgbm90IGRlZmluZWQuXCIpO1xyXG4gICAgdmFyIG0gPSBvW1N5bWJvbC5hc3luY0l0ZXJhdG9yXSwgaTtcclxuICAgIHJldHVybiBtID8gbS5jYWxsKG8pIDogKG8gPSB0eXBlb2YgX192YWx1ZXMgPT09IFwiZnVuY3Rpb25cIiA/IF9fdmFsdWVzKG8pIDogb1tTeW1ib2wuaXRlcmF0b3JdKCksIGkgPSB7fSwgdmVyYihcIm5leHRcIiksIHZlcmIoXCJ0aHJvd1wiKSwgdmVyYihcInJldHVyblwiKSwgaVtTeW1ib2wuYXN5bmNJdGVyYXRvcl0gPSBmdW5jdGlvbiAoKSB7IHJldHVybiB0aGlzOyB9LCBpKTtcclxuICAgIGZ1bmN0aW9uIHZlcmIobikgeyBpW25dID0gb1tuXSAmJiBmdW5jdGlvbiAodikgeyByZXR1cm4gbmV3IFByb21pc2UoZnVuY3Rpb24gKHJlc29sdmUsIHJlamVjdCkgeyB2ID0gb1tuXSh2KSwgc2V0dGxlKHJlc29sdmUsIHJlamVjdCwgdi5kb25lLCB2LnZhbHVlKTsgfSk7IH07IH1cclxuICAgIGZ1bmN0aW9uIHNldHRsZShyZXNvbHZlLCByZWplY3QsIGQsIHYpIHsgUHJvbWlzZS5yZXNvbHZlKHYpLnRoZW4oZnVuY3Rpb24odikgeyByZXNvbHZlKHsgdmFsdWU6IHYsIGRvbmU6IGQgfSk7IH0sIHJlamVjdCk7IH1cclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fbWFrZVRlbXBsYXRlT2JqZWN0KGNvb2tlZCwgcmF3KSB7XHJcbiAgICBpZiAoT2JqZWN0LmRlZmluZVByb3BlcnR5KSB7IE9iamVjdC5kZWZpbmVQcm9wZXJ0eShjb29rZWQsIFwicmF3XCIsIHsgdmFsdWU6IHJhdyB9KTsgfSBlbHNlIHsgY29va2VkLnJhdyA9IHJhdzsgfVxyXG4gICAgcmV0dXJuIGNvb2tlZDtcclxufTtcclxuXHJcbnZhciBfX3NldE1vZHVsZURlZmF1bHQgPSBPYmplY3QuY3JlYXRlID8gKGZ1bmN0aW9uKG8sIHYpIHtcclxuICAgIE9iamVjdC5kZWZpbmVQcm9wZXJ0eShvLCBcImRlZmF1bHRcIiwgeyBlbnVtZXJhYmxlOiB0cnVlLCB2YWx1ZTogdiB9KTtcclxufSkgOiBmdW5jdGlvbihvLCB2KSB7XHJcbiAgICBvW1wiZGVmYXVsdFwiXSA9IHY7XHJcbn07XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19pbXBvcnRTdGFyKG1vZCkge1xyXG4gICAgaWYgKG1vZCAmJiBtb2QuX19lc01vZHVsZSkgcmV0dXJuIG1vZDtcclxuICAgIHZhciByZXN1bHQgPSB7fTtcclxuICAgIGlmIChtb2QgIT0gbnVsbCkgZm9yICh2YXIgayBpbiBtb2QpIGlmIChrICE9PSBcImRlZmF1bHRcIiAmJiBPYmplY3QucHJvdG90eXBlLmhhc093blByb3BlcnR5LmNhbGwobW9kLCBrKSkgX19jcmVhdGVCaW5kaW5nKHJlc3VsdCwgbW9kLCBrKTtcclxuICAgIF9fc2V0TW9kdWxlRGVmYXVsdChyZXN1bHQsIG1vZCk7XHJcbiAgICByZXR1cm4gcmVzdWx0O1xyXG59XHJcblxyXG5leHBvcnQgZnVuY3Rpb24gX19pbXBvcnREZWZhdWx0KG1vZCkge1xyXG4gICAgcmV0dXJuIChtb2QgJiYgbW9kLl9fZXNNb2R1bGUpID8gbW9kIDogeyBkZWZhdWx0OiBtb2QgfTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fY2xhc3NQcml2YXRlRmllbGRHZXQocmVjZWl2ZXIsIHN0YXRlLCBraW5kLCBmKSB7XHJcbiAgICBpZiAoa2luZCA9PT0gXCJhXCIgJiYgIWYpIHRocm93IG5ldyBUeXBlRXJyb3IoXCJQcml2YXRlIGFjY2Vzc29yIHdhcyBkZWZpbmVkIHdpdGhvdXQgYSBnZXR0ZXJcIik7XHJcbiAgICBpZiAodHlwZW9mIHN0YXRlID09PSBcImZ1bmN0aW9uXCIgPyByZWNlaXZlciAhPT0gc3RhdGUgfHwgIWYgOiAhc3RhdGUuaGFzKHJlY2VpdmVyKSkgdGhyb3cgbmV3IFR5cGVFcnJvcihcIkNhbm5vdCByZWFkIHByaXZhdGUgbWVtYmVyIGZyb20gYW4gb2JqZWN0IHdob3NlIGNsYXNzIGRpZCBub3QgZGVjbGFyZSBpdFwiKTtcclxuICAgIHJldHVybiBraW5kID09PSBcIm1cIiA/IGYgOiBraW5kID09PSBcImFcIiA/IGYuY2FsbChyZWNlaXZlcikgOiBmID8gZi52YWx1ZSA6IHN0YXRlLmdldChyZWNlaXZlcik7XHJcbn1cclxuXHJcbmV4cG9ydCBmdW5jdGlvbiBfX2NsYXNzUHJpdmF0ZUZpZWxkU2V0KHJlY2VpdmVyLCBzdGF0ZSwgdmFsdWUsIGtpbmQsIGYpIHtcclxuICAgIGlmIChraW5kID09PSBcIm1cIikgdGhyb3cgbmV3IFR5cGVFcnJvcihcIlByaXZhdGUgbWV0aG9kIGlzIG5vdCB3cml0YWJsZVwiKTtcclxuICAgIGlmIChraW5kID09PSBcImFcIiAmJiAhZikgdGhyb3cgbmV3IFR5cGVFcnJvcihcIlByaXZhdGUgYWNjZXNzb3Igd2FzIGRlZmluZWQgd2l0aG91dCBhIHNldHRlclwiKTtcclxuICAgIGlmICh0eXBlb2Ygc3RhdGUgPT09IFwiZnVuY3Rpb25cIiA/IHJlY2VpdmVyICE9PSBzdGF0ZSB8fCAhZiA6ICFzdGF0ZS5oYXMocmVjZWl2ZXIpKSB0aHJvdyBuZXcgVHlwZUVycm9yKFwiQ2Fubm90IHdyaXRlIHByaXZhdGUgbWVtYmVyIHRvIGFuIG9iamVjdCB3aG9zZSBjbGFzcyBkaWQgbm90IGRlY2xhcmUgaXRcIik7XHJcbiAgICByZXR1cm4gKGtpbmQgPT09IFwiYVwiID8gZi5jYWxsKHJlY2VpdmVyLCB2YWx1ZSkgOiBmID8gZi52YWx1ZSA9IHZhbHVlIDogc3RhdGUuc2V0KHJlY2VpdmVyLCB2YWx1ZSkpLCB2YWx1ZTtcclxufVxyXG5cclxuZXhwb3J0IGZ1bmN0aW9uIF9fY2xhc3NQcml2YXRlRmllbGRJbihzdGF0ZSwgcmVjZWl2ZXIpIHtcclxuICAgIGlmIChyZWNlaXZlciA9PT0gbnVsbCB8fCAodHlwZW9mIHJlY2VpdmVyICE9PSBcIm9iamVjdFwiICYmIHR5cGVvZiByZWNlaXZlciAhPT0gXCJmdW5jdGlvblwiKSkgdGhyb3cgbmV3IFR5cGVFcnJvcihcIkNhbm5vdCB1c2UgJ2luJyBvcGVyYXRvciBvbiBub24tb2JqZWN0XCIpO1xyXG4gICAgcmV0dXJuIHR5cGVvZiBzdGF0ZSA9PT0gXCJmdW5jdGlvblwiID8gcmVjZWl2ZXIgPT09IHN0YXRlIDogc3RhdGUuaGFzKHJlY2VpdmVyKTtcclxufVxyXG4iLCJpbXBvcnQgeyB3IGFzIHdyYXAsIHIgYXMgcmVwbGFjZVRyYXBzIH0gZnJvbSAnLi93cmFwLWlkYi12YWx1ZS5qcyc7XG5leHBvcnQgeyB1IGFzIHVud3JhcCwgdyBhcyB3cmFwIH0gZnJvbSAnLi93cmFwLWlkYi12YWx1ZS5qcyc7XG5cbi8qKlxuICogT3BlbiBhIGRhdGFiYXNlLlxuICpcbiAqIEBwYXJhbSBuYW1lIE5hbWUgb2YgdGhlIGRhdGFiYXNlLlxuICogQHBhcmFtIHZlcnNpb24gU2NoZW1hIHZlcnNpb24uXG4gKiBAcGFyYW0gY2FsbGJhY2tzIEFkZGl0aW9uYWwgY2FsbGJhY2tzLlxuICovXG5mdW5jdGlvbiBvcGVuREIobmFtZSwgdmVyc2lvbiwgeyBibG9ja2VkLCB1cGdyYWRlLCBibG9ja2luZywgdGVybWluYXRlZCB9ID0ge30pIHtcbiAgICBjb25zdCByZXF1ZXN0ID0gaW5kZXhlZERCLm9wZW4obmFtZSwgdmVyc2lvbik7XG4gICAgY29uc3Qgb3BlblByb21pc2UgPSB3cmFwKHJlcXVlc3QpO1xuICAgIGlmICh1cGdyYWRlKSB7XG4gICAgICAgIHJlcXVlc3QuYWRkRXZlbnRMaXN0ZW5lcigndXBncmFkZW5lZWRlZCcsIChldmVudCkgPT4ge1xuICAgICAgICAgICAgdXBncmFkZSh3cmFwKHJlcXVlc3QucmVzdWx0KSwgZXZlbnQub2xkVmVyc2lvbiwgZXZlbnQubmV3VmVyc2lvbiwgd3JhcChyZXF1ZXN0LnRyYW5zYWN0aW9uKSwgZXZlbnQpO1xuICAgICAgICB9KTtcbiAgICB9XG4gICAgaWYgKGJsb2NrZWQpIHtcbiAgICAgICAgcmVxdWVzdC5hZGRFdmVudExpc3RlbmVyKCdibG9ja2VkJywgKGV2ZW50KSA9PiBibG9ja2VkKFxuICAgICAgICAvLyBDYXN0aW5nIGR1ZSB0byBodHRwczovL2dpdGh1Yi5jb20vbWljcm9zb2Z0L1R5cGVTY3JpcHQtRE9NLWxpYi1nZW5lcmF0b3IvcHVsbC8xNDA1XG4gICAgICAgIGV2ZW50Lm9sZFZlcnNpb24sIGV2ZW50Lm5ld1ZlcnNpb24sIGV2ZW50KSk7XG4gICAgfVxuICAgIG9wZW5Qcm9taXNlXG4gICAgICAgIC50aGVuKChkYikgPT4ge1xuICAgICAgICBpZiAodGVybWluYXRlZClcbiAgICAgICAgICAgIGRiLmFkZEV2ZW50TGlzdGVuZXIoJ2Nsb3NlJywgKCkgPT4gdGVybWluYXRlZCgpKTtcbiAgICAgICAgaWYgKGJsb2NraW5nKSB7XG4gICAgICAgICAgICBkYi5hZGRFdmVudExpc3RlbmVyKCd2ZXJzaW9uY2hhbmdlJywgKGV2ZW50KSA9PiBibG9ja2luZyhldmVudC5vbGRWZXJzaW9uLCBldmVudC5uZXdWZXJzaW9uLCBldmVudCkpO1xuICAgICAgICB9XG4gICAgfSlcbiAgICAgICAgLmNhdGNoKCgpID0+IHsgfSk7XG4gICAgcmV0dXJuIG9wZW5Qcm9taXNlO1xufVxuLyoqXG4gKiBEZWxldGUgYSBkYXRhYmFzZS5cbiAqXG4gKiBAcGFyYW0gbmFtZSBOYW1lIG9mIHRoZSBkYXRhYmFzZS5cbiAqL1xuZnVuY3Rpb24gZGVsZXRlREIobmFtZSwgeyBibG9ja2VkIH0gPSB7fSkge1xuICAgIGNvbnN0IHJlcXVlc3QgPSBpbmRleGVkREIuZGVsZXRlRGF0YWJhc2UobmFtZSk7XG4gICAgaWYgKGJsb2NrZWQpIHtcbiAgICAgICAgcmVxdWVzdC5hZGRFdmVudExpc3RlbmVyKCdibG9ja2VkJywgKGV2ZW50KSA9PiBibG9ja2VkKFxuICAgICAgICAvLyBDYXN0aW5nIGR1ZSB0byBodHRwczovL2dpdGh1Yi5jb20vbWljcm9zb2Z0L1R5cGVTY3JpcHQtRE9NLWxpYi1nZW5lcmF0b3IvcHVsbC8xNDA1XG4gICAgICAgIGV2ZW50Lm9sZFZlcnNpb24sIGV2ZW50KSk7XG4gICAgfVxuICAgIHJldHVybiB3cmFwKHJlcXVlc3QpLnRoZW4oKCkgPT4gdW5kZWZpbmVkKTtcbn1cblxuY29uc3QgcmVhZE1ldGhvZHMgPSBbJ2dldCcsICdnZXRLZXknLCAnZ2V0QWxsJywgJ2dldEFsbEtleXMnLCAnY291bnQnXTtcbmNvbnN0IHdyaXRlTWV0aG9kcyA9IFsncHV0JywgJ2FkZCcsICdkZWxldGUnLCAnY2xlYXInXTtcbmNvbnN0IGNhY2hlZE1ldGhvZHMgPSBuZXcgTWFwKCk7XG5mdW5jdGlvbiBnZXRNZXRob2QodGFyZ2V0LCBwcm9wKSB7XG4gICAgaWYgKCEodGFyZ2V0IGluc3RhbmNlb2YgSURCRGF0YWJhc2UgJiZcbiAgICAgICAgIShwcm9wIGluIHRhcmdldCkgJiZcbiAgICAgICAgdHlwZW9mIHByb3AgPT09ICdzdHJpbmcnKSkge1xuICAgICAgICByZXR1cm47XG4gICAgfVxuICAgIGlmIChjYWNoZWRNZXRob2RzLmdldChwcm9wKSlcbiAgICAgICAgcmV0dXJuIGNhY2hlZE1ldGhvZHMuZ2V0KHByb3ApO1xuICAgIGNvbnN0IHRhcmdldEZ1bmNOYW1lID0gcHJvcC5yZXBsYWNlKC9Gcm9tSW5kZXgkLywgJycpO1xuICAgIGNvbnN0IHVzZUluZGV4ID0gcHJvcCAhPT0gdGFyZ2V0RnVuY05hbWU7XG4gICAgY29uc3QgaXNXcml0ZSA9IHdyaXRlTWV0aG9kcy5pbmNsdWRlcyh0YXJnZXRGdW5jTmFtZSk7XG4gICAgaWYgKFxuICAgIC8vIEJhaWwgaWYgdGhlIHRhcmdldCBkb2Vzbid0IGV4aXN0IG9uIHRoZSB0YXJnZXQuIEVnLCBnZXRBbGwgaXNuJ3QgaW4gRWRnZS5cbiAgICAhKHRhcmdldEZ1bmNOYW1lIGluICh1c2VJbmRleCA/IElEQkluZGV4IDogSURCT2JqZWN0U3RvcmUpLnByb3RvdHlwZSkgfHxcbiAgICAgICAgIShpc1dyaXRlIHx8IHJlYWRNZXRob2RzLmluY2x1ZGVzKHRhcmdldEZ1bmNOYW1lKSkpIHtcbiAgICAgICAgcmV0dXJuO1xuICAgIH1cbiAgICBjb25zdCBtZXRob2QgPSBhc3luYyBmdW5jdGlvbiAoc3RvcmVOYW1lLCAuLi5hcmdzKSB7XG4gICAgICAgIC8vIGlzV3JpdGUgPyAncmVhZHdyaXRlJyA6IHVuZGVmaW5lZCBnemlwcHMgYmV0dGVyLCBidXQgZmFpbHMgaW4gRWRnZSA6KFxuICAgICAgICBjb25zdCB0eCA9IHRoaXMudHJhbnNhY3Rpb24oc3RvcmVOYW1lLCBpc1dyaXRlID8gJ3JlYWR3cml0ZScgOiAncmVhZG9ubHknKTtcbiAgICAgICAgbGV0IHRhcmdldCA9IHR4LnN0b3JlO1xuICAgICAgICBpZiAodXNlSW5kZXgpXG4gICAgICAgICAgICB0YXJnZXQgPSB0YXJnZXQuaW5kZXgoYXJncy5zaGlmdCgpKTtcbiAgICAgICAgLy8gTXVzdCByZWplY3QgaWYgb3AgcmVqZWN0cy5cbiAgICAgICAgLy8gSWYgaXQncyBhIHdyaXRlIG9wZXJhdGlvbiwgbXVzdCByZWplY3QgaWYgdHguZG9uZSByZWplY3RzLlxuICAgICAgICAvLyBNdXN0IHJlamVjdCB3aXRoIG9wIHJlamVjdGlvbiBmaXJzdC5cbiAgICAgICAgLy8gTXVzdCByZXNvbHZlIHdpdGggb3AgdmFsdWUuXG4gICAgICAgIC8vIE11c3QgaGFuZGxlIGJvdGggcHJvbWlzZXMgKG5vIHVuaGFuZGxlZCByZWplY3Rpb25zKVxuICAgICAgICByZXR1cm4gKGF3YWl0IFByb21pc2UuYWxsKFtcbiAgICAgICAgICAgIHRhcmdldFt0YXJnZXRGdW5jTmFtZV0oLi4uYXJncyksXG4gICAgICAgICAgICBpc1dyaXRlICYmIHR4LmRvbmUsXG4gICAgICAgIF0pKVswXTtcbiAgICB9O1xuICAgIGNhY2hlZE1ldGhvZHMuc2V0KHByb3AsIG1ldGhvZCk7XG4gICAgcmV0dXJuIG1ldGhvZDtcbn1cbnJlcGxhY2VUcmFwcygob2xkVHJhcHMpID0+ICh7XG4gICAgLi4ub2xkVHJhcHMsXG4gICAgZ2V0OiAodGFyZ2V0LCBwcm9wLCByZWNlaXZlcikgPT4gZ2V0TWV0aG9kKHRhcmdldCwgcHJvcCkgfHwgb2xkVHJhcHMuZ2V0KHRhcmdldCwgcHJvcCwgcmVjZWl2ZXIpLFxuICAgIGhhczogKHRhcmdldCwgcHJvcCkgPT4gISFnZXRNZXRob2QodGFyZ2V0LCBwcm9wKSB8fCBvbGRUcmFwcy5oYXModGFyZ2V0LCBwcm9wKSxcbn0pKTtcblxuZXhwb3J0IHsgZGVsZXRlREIsIG9wZW5EQiB9O1xuIiwiY29uc3QgaW5zdGFuY2VPZkFueSA9IChvYmplY3QsIGNvbnN0cnVjdG9ycykgPT4gY29uc3RydWN0b3JzLnNvbWUoKGMpID0+IG9iamVjdCBpbnN0YW5jZW9mIGMpO1xuXG5sZXQgaWRiUHJveHlhYmxlVHlwZXM7XG5sZXQgY3Vyc29yQWR2YW5jZU1ldGhvZHM7XG4vLyBUaGlzIGlzIGEgZnVuY3Rpb24gdG8gcHJldmVudCBpdCB0aHJvd2luZyB1cCBpbiBub2RlIGVudmlyb25tZW50cy5cbmZ1bmN0aW9uIGdldElkYlByb3h5YWJsZVR5cGVzKCkge1xuICAgIHJldHVybiAoaWRiUHJveHlhYmxlVHlwZXMgfHxcbiAgICAgICAgKGlkYlByb3h5YWJsZVR5cGVzID0gW1xuICAgICAgICAgICAgSURCRGF0YWJhc2UsXG4gICAgICAgICAgICBJREJPYmplY3RTdG9yZSxcbiAgICAgICAgICAgIElEQkluZGV4LFxuICAgICAgICAgICAgSURCQ3Vyc29yLFxuICAgICAgICAgICAgSURCVHJhbnNhY3Rpb24sXG4gICAgICAgIF0pKTtcbn1cbi8vIFRoaXMgaXMgYSBmdW5jdGlvbiB0byBwcmV2ZW50IGl0IHRocm93aW5nIHVwIGluIG5vZGUgZW52aXJvbm1lbnRzLlxuZnVuY3Rpb24gZ2V0Q3Vyc29yQWR2YW5jZU1ldGhvZHMoKSB7XG4gICAgcmV0dXJuIChjdXJzb3JBZHZhbmNlTWV0aG9kcyB8fFxuICAgICAgICAoY3Vyc29yQWR2YW5jZU1ldGhvZHMgPSBbXG4gICAgICAgICAgICBJREJDdXJzb3IucHJvdG90eXBlLmFkdmFuY2UsXG4gICAgICAgICAgICBJREJDdXJzb3IucHJvdG90eXBlLmNvbnRpbnVlLFxuICAgICAgICAgICAgSURCQ3Vyc29yLnByb3RvdHlwZS5jb250aW51ZVByaW1hcnlLZXksXG4gICAgICAgIF0pKTtcbn1cbmNvbnN0IGN1cnNvclJlcXVlc3RNYXAgPSBuZXcgV2Vha01hcCgpO1xuY29uc3QgdHJhbnNhY3Rpb25Eb25lTWFwID0gbmV3IFdlYWtNYXAoKTtcbmNvbnN0IHRyYW5zYWN0aW9uU3RvcmVOYW1lc01hcCA9IG5ldyBXZWFrTWFwKCk7XG5jb25zdCB0cmFuc2Zvcm1DYWNoZSA9IG5ldyBXZWFrTWFwKCk7XG5jb25zdCByZXZlcnNlVHJhbnNmb3JtQ2FjaGUgPSBuZXcgV2Vha01hcCgpO1xuZnVuY3Rpb24gcHJvbWlzaWZ5UmVxdWVzdChyZXF1ZXN0KSB7XG4gICAgY29uc3QgcHJvbWlzZSA9IG5ldyBQcm9taXNlKChyZXNvbHZlLCByZWplY3QpID0+IHtcbiAgICAgICAgY29uc3QgdW5saXN0ZW4gPSAoKSA9PiB7XG4gICAgICAgICAgICByZXF1ZXN0LnJlbW92ZUV2ZW50TGlzdGVuZXIoJ3N1Y2Nlc3MnLCBzdWNjZXNzKTtcbiAgICAgICAgICAgIHJlcXVlc3QucmVtb3ZlRXZlbnRMaXN0ZW5lcignZXJyb3InLCBlcnJvcik7XG4gICAgICAgIH07XG4gICAgICAgIGNvbnN0IHN1Y2Nlc3MgPSAoKSA9PiB7XG4gICAgICAgICAgICByZXNvbHZlKHdyYXAocmVxdWVzdC5yZXN1bHQpKTtcbiAgICAgICAgICAgIHVubGlzdGVuKCk7XG4gICAgICAgIH07XG4gICAgICAgIGNvbnN0IGVycm9yID0gKCkgPT4ge1xuICAgICAgICAgICAgcmVqZWN0KHJlcXVlc3QuZXJyb3IpO1xuICAgICAgICAgICAgdW5saXN0ZW4oKTtcbiAgICAgICAgfTtcbiAgICAgICAgcmVxdWVzdC5hZGRFdmVudExpc3RlbmVyKCdzdWNjZXNzJywgc3VjY2Vzcyk7XG4gICAgICAgIHJlcXVlc3QuYWRkRXZlbnRMaXN0ZW5lcignZXJyb3InLCBlcnJvcik7XG4gICAgfSk7XG4gICAgcHJvbWlzZVxuICAgICAgICAudGhlbigodmFsdWUpID0+IHtcbiAgICAgICAgLy8gU2luY2UgY3Vyc29yaW5nIHJldXNlcyB0aGUgSURCUmVxdWVzdCAoKnNpZ2gqKSwgd2UgY2FjaGUgaXQgZm9yIGxhdGVyIHJldHJpZXZhbFxuICAgICAgICAvLyAoc2VlIHdyYXBGdW5jdGlvbikuXG4gICAgICAgIGlmICh2YWx1ZSBpbnN0YW5jZW9mIElEQkN1cnNvcikge1xuICAgICAgICAgICAgY3Vyc29yUmVxdWVzdE1hcC5zZXQodmFsdWUsIHJlcXVlc3QpO1xuICAgICAgICB9XG4gICAgICAgIC8vIENhdGNoaW5nIHRvIGF2b2lkIFwiVW5jYXVnaHQgUHJvbWlzZSBleGNlcHRpb25zXCJcbiAgICB9KVxuICAgICAgICAuY2F0Y2goKCkgPT4geyB9KTtcbiAgICAvLyBUaGlzIG1hcHBpbmcgZXhpc3RzIGluIHJldmVyc2VUcmFuc2Zvcm1DYWNoZSBidXQgZG9lc24ndCBkb2Vzbid0IGV4aXN0IGluIHRyYW5zZm9ybUNhY2hlLiBUaGlzXG4gICAgLy8gaXMgYmVjYXVzZSB3ZSBjcmVhdGUgbWFueSBwcm9taXNlcyBmcm9tIGEgc2luZ2xlIElEQlJlcXVlc3QuXG4gICAgcmV2ZXJzZVRyYW5zZm9ybUNhY2hlLnNldChwcm9taXNlLCByZXF1ZXN0KTtcbiAgICByZXR1cm4gcHJvbWlzZTtcbn1cbmZ1bmN0aW9uIGNhY2hlRG9uZVByb21pc2VGb3JUcmFuc2FjdGlvbih0eCkge1xuICAgIC8vIEVhcmx5IGJhaWwgaWYgd2UndmUgYWxyZWFkeSBjcmVhdGVkIGEgZG9uZSBwcm9taXNlIGZvciB0aGlzIHRyYW5zYWN0aW9uLlxuICAgIGlmICh0cmFuc2FjdGlvbkRvbmVNYXAuaGFzKHR4KSlcbiAgICAgICAgcmV0dXJuO1xuICAgIGNvbnN0IGRvbmUgPSBuZXcgUHJvbWlzZSgocmVzb2x2ZSwgcmVqZWN0KSA9PiB7XG4gICAgICAgIGNvbnN0IHVubGlzdGVuID0gKCkgPT4ge1xuICAgICAgICAgICAgdHgucmVtb3ZlRXZlbnRMaXN0ZW5lcignY29tcGxldGUnLCBjb21wbGV0ZSk7XG4gICAgICAgICAgICB0eC5yZW1vdmVFdmVudExpc3RlbmVyKCdlcnJvcicsIGVycm9yKTtcbiAgICAgICAgICAgIHR4LnJlbW92ZUV2ZW50TGlzdGVuZXIoJ2Fib3J0JywgZXJyb3IpO1xuICAgICAgICB9O1xuICAgICAgICBjb25zdCBjb21wbGV0ZSA9ICgpID0+IHtcbiAgICAgICAgICAgIHJlc29sdmUoKTtcbiAgICAgICAgICAgIHVubGlzdGVuKCk7XG4gICAgICAgIH07XG4gICAgICAgIGNvbnN0IGVycm9yID0gKCkgPT4ge1xuICAgICAgICAgICAgcmVqZWN0KHR4LmVycm9yIHx8IG5ldyBET01FeGNlcHRpb24oJ0Fib3J0RXJyb3InLCAnQWJvcnRFcnJvcicpKTtcbiAgICAgICAgICAgIHVubGlzdGVuKCk7XG4gICAgICAgIH07XG4gICAgICAgIHR4LmFkZEV2ZW50TGlzdGVuZXIoJ2NvbXBsZXRlJywgY29tcGxldGUpO1xuICAgICAgICB0eC5hZGRFdmVudExpc3RlbmVyKCdlcnJvcicsIGVycm9yKTtcbiAgICAgICAgdHguYWRkRXZlbnRMaXN0ZW5lcignYWJvcnQnLCBlcnJvcik7XG4gICAgfSk7XG4gICAgLy8gQ2FjaGUgaXQgZm9yIGxhdGVyIHJldHJpZXZhbC5cbiAgICB0cmFuc2FjdGlvbkRvbmVNYXAuc2V0KHR4LCBkb25lKTtcbn1cbmxldCBpZGJQcm94eVRyYXBzID0ge1xuICAgIGdldCh0YXJnZXQsIHByb3AsIHJlY2VpdmVyKSB7XG4gICAgICAgIGlmICh0YXJnZXQgaW5zdGFuY2VvZiBJREJUcmFuc2FjdGlvbikge1xuICAgICAgICAgICAgLy8gU3BlY2lhbCBoYW5kbGluZyBmb3IgdHJhbnNhY3Rpb24uZG9uZS5cbiAgICAgICAgICAgIGlmIChwcm9wID09PSAnZG9uZScpXG4gICAgICAgICAgICAgICAgcmV0dXJuIHRyYW5zYWN0aW9uRG9uZU1hcC5nZXQodGFyZ2V0KTtcbiAgICAgICAgICAgIC8vIFBvbHlmaWxsIGZvciBvYmplY3RTdG9yZU5hbWVzIGJlY2F1c2Ugb2YgRWRnZS5cbiAgICAgICAgICAgIGlmIChwcm9wID09PSAnb2JqZWN0U3RvcmVOYW1lcycpIHtcbiAgICAgICAgICAgICAgICByZXR1cm4gdGFyZ2V0Lm9iamVjdFN0b3JlTmFtZXMgfHwgdHJhbnNhY3Rpb25TdG9yZU5hbWVzTWFwLmdldCh0YXJnZXQpO1xuICAgICAgICAgICAgfVxuICAgICAgICAgICAgLy8gTWFrZSB0eC5zdG9yZSByZXR1cm4gdGhlIG9ubHkgc3RvcmUgaW4gdGhlIHRyYW5zYWN0aW9uLCBvciB1bmRlZmluZWQgaWYgdGhlcmUgYXJlIG1hbnkuXG4gICAgICAgICAgICBpZiAocHJvcCA9PT0gJ3N0b3JlJykge1xuICAgICAgICAgICAgICAgIHJldHVybiByZWNlaXZlci5vYmplY3RTdG9yZU5hbWVzWzFdXG4gICAgICAgICAgICAgICAgICAgID8gdW5kZWZpbmVkXG4gICAgICAgICAgICAgICAgICAgIDogcmVjZWl2ZXIub2JqZWN0U3RvcmUocmVjZWl2ZXIub2JqZWN0U3RvcmVOYW1lc1swXSk7XG4gICAgICAgICAgICB9XG4gICAgICAgIH1cbiAgICAgICAgLy8gRWxzZSB0cmFuc2Zvcm0gd2hhdGV2ZXIgd2UgZ2V0IGJhY2suXG4gICAgICAgIHJldHVybiB3cmFwKHRhcmdldFtwcm9wXSk7XG4gICAgfSxcbiAgICBzZXQodGFyZ2V0LCBwcm9wLCB2YWx1ZSkge1xuICAgICAgICB0YXJnZXRbcHJvcF0gPSB2YWx1ZTtcbiAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgfSxcbiAgICBoYXModGFyZ2V0LCBwcm9wKSB7XG4gICAgICAgIGlmICh0YXJnZXQgaW5zdGFuY2VvZiBJREJUcmFuc2FjdGlvbiAmJlxuICAgICAgICAgICAgKHByb3AgPT09ICdkb25lJyB8fCBwcm9wID09PSAnc3RvcmUnKSkge1xuICAgICAgICAgICAgcmV0dXJuIHRydWU7XG4gICAgICAgIH1cbiAgICAgICAgcmV0dXJuIHByb3AgaW4gdGFyZ2V0O1xuICAgIH0sXG59O1xuZnVuY3Rpb24gcmVwbGFjZVRyYXBzKGNhbGxiYWNrKSB7XG4gICAgaWRiUHJveHlUcmFwcyA9IGNhbGxiYWNrKGlkYlByb3h5VHJhcHMpO1xufVxuZnVuY3Rpb24gd3JhcEZ1bmN0aW9uKGZ1bmMpIHtcbiAgICAvLyBEdWUgdG8gZXhwZWN0ZWQgb2JqZWN0IGVxdWFsaXR5ICh3aGljaCBpcyBlbmZvcmNlZCBieSB0aGUgY2FjaGluZyBpbiBgd3JhcGApLCB3ZVxuICAgIC8vIG9ubHkgY3JlYXRlIG9uZSBuZXcgZnVuYyBwZXIgZnVuYy5cbiAgICAvLyBFZGdlIGRvZXNuJ3Qgc3VwcG9ydCBvYmplY3RTdG9yZU5hbWVzIChib29vKSwgc28gd2UgcG9seWZpbGwgaXQgaGVyZS5cbiAgICBpZiAoZnVuYyA9PT0gSURCRGF0YWJhc2UucHJvdG90eXBlLnRyYW5zYWN0aW9uICYmXG4gICAgICAgICEoJ29iamVjdFN0b3JlTmFtZXMnIGluIElEQlRyYW5zYWN0aW9uLnByb3RvdHlwZSkpIHtcbiAgICAgICAgcmV0dXJuIGZ1bmN0aW9uIChzdG9yZU5hbWVzLCAuLi5hcmdzKSB7XG4gICAgICAgICAgICBjb25zdCB0eCA9IGZ1bmMuY2FsbCh1bndyYXAodGhpcyksIHN0b3JlTmFtZXMsIC4uLmFyZ3MpO1xuICAgICAgICAgICAgdHJhbnNhY3Rpb25TdG9yZU5hbWVzTWFwLnNldCh0eCwgc3RvcmVOYW1lcy5zb3J0ID8gc3RvcmVOYW1lcy5zb3J0KCkgOiBbc3RvcmVOYW1lc10pO1xuICAgICAgICAgICAgcmV0dXJuIHdyYXAodHgpO1xuICAgICAgICB9O1xuICAgIH1cbiAgICAvLyBDdXJzb3IgbWV0aG9kcyBhcmUgc3BlY2lhbCwgYXMgdGhlIGJlaGF2aW91ciBpcyBhIGxpdHRsZSBtb3JlIGRpZmZlcmVudCB0byBzdGFuZGFyZCBJREIuIEluXG4gICAgLy8gSURCLCB5b3UgYWR2YW5jZSB0aGUgY3Vyc29yIGFuZCB3YWl0IGZvciBhIG5ldyAnc3VjY2Vzcycgb24gdGhlIElEQlJlcXVlc3QgdGhhdCBnYXZlIHlvdSB0aGVcbiAgICAvLyBjdXJzb3IuIEl0J3Mga2luZGEgbGlrZSBhIHByb21pc2UgdGhhdCBjYW4gcmVzb2x2ZSB3aXRoIG1hbnkgdmFsdWVzLiBUaGF0IGRvZXNuJ3QgbWFrZSBzZW5zZVxuICAgIC8vIHdpdGggcmVhbCBwcm9taXNlcywgc28gZWFjaCBhZHZhbmNlIG1ldGhvZHMgcmV0dXJucyBhIG5ldyBwcm9taXNlIGZvciB0aGUgY3Vyc29yIG9iamVjdCwgb3JcbiAgICAvLyB1bmRlZmluZWQgaWYgdGhlIGVuZCBvZiB0aGUgY3Vyc29yIGhhcyBiZWVuIHJlYWNoZWQuXG4gICAgaWYgKGdldEN1cnNvckFkdmFuY2VNZXRob2RzKCkuaW5jbHVkZXMoZnVuYykpIHtcbiAgICAgICAgcmV0dXJuIGZ1bmN0aW9uICguLi5hcmdzKSB7XG4gICAgICAgICAgICAvLyBDYWxsaW5nIHRoZSBvcmlnaW5hbCBmdW5jdGlvbiB3aXRoIHRoZSBwcm94eSBhcyAndGhpcycgY2F1c2VzIElMTEVHQUwgSU5WT0NBVElPTiwgc28gd2UgdXNlXG4gICAgICAgICAgICAvLyB0aGUgb3JpZ2luYWwgb2JqZWN0LlxuICAgICAgICAgICAgZnVuYy5hcHBseSh1bndyYXAodGhpcyksIGFyZ3MpO1xuICAgICAgICAgICAgcmV0dXJuIHdyYXAoY3Vyc29yUmVxdWVzdE1hcC5nZXQodGhpcykpO1xuICAgICAgICB9O1xuICAgIH1cbiAgICByZXR1cm4gZnVuY3Rpb24gKC4uLmFyZ3MpIHtcbiAgICAgICAgLy8gQ2FsbGluZyB0aGUgb3JpZ2luYWwgZnVuY3Rpb24gd2l0aCB0aGUgcHJveHkgYXMgJ3RoaXMnIGNhdXNlcyBJTExFR0FMIElOVk9DQVRJT04sIHNvIHdlIHVzZVxuICAgICAgICAvLyB0aGUgb3JpZ2luYWwgb2JqZWN0LlxuICAgICAgICByZXR1cm4gd3JhcChmdW5jLmFwcGx5KHVud3JhcCh0aGlzKSwgYXJncykpO1xuICAgIH07XG59XG5mdW5jdGlvbiB0cmFuc2Zvcm1DYWNoYWJsZVZhbHVlKHZhbHVlKSB7XG4gICAgaWYgKHR5cGVvZiB2YWx1ZSA9PT0gJ2Z1bmN0aW9uJylcbiAgICAgICAgcmV0dXJuIHdyYXBGdW5jdGlvbih2YWx1ZSk7XG4gICAgLy8gVGhpcyBkb2Vzbid0IHJldHVybiwgaXQganVzdCBjcmVhdGVzIGEgJ2RvbmUnIHByb21pc2UgZm9yIHRoZSB0cmFuc2FjdGlvbixcbiAgICAvLyB3aGljaCBpcyBsYXRlciByZXR1cm5lZCBmb3IgdHJhbnNhY3Rpb24uZG9uZSAoc2VlIGlkYk9iamVjdEhhbmRsZXIpLlxuICAgIGlmICh2YWx1ZSBpbnN0YW5jZW9mIElEQlRyYW5zYWN0aW9uKVxuICAgICAgICBjYWNoZURvbmVQcm9taXNlRm9yVHJhbnNhY3Rpb24odmFsdWUpO1xuICAgIGlmIChpbnN0YW5jZU9mQW55KHZhbHVlLCBnZXRJZGJQcm94eWFibGVUeXBlcygpKSlcbiAgICAgICAgcmV0dXJuIG5ldyBQcm94eSh2YWx1ZSwgaWRiUHJveHlUcmFwcyk7XG4gICAgLy8gUmV0dXJuIHRoZSBzYW1lIHZhbHVlIGJhY2sgaWYgd2UncmUgbm90IGdvaW5nIHRvIHRyYW5zZm9ybSBpdC5cbiAgICByZXR1cm4gdmFsdWU7XG59XG5mdW5jdGlvbiB3cmFwKHZhbHVlKSB7XG4gICAgLy8gV2Ugc29tZXRpbWVzIGdlbmVyYXRlIG11bHRpcGxlIHByb21pc2VzIGZyb20gYSBzaW5nbGUgSURCUmVxdWVzdCAoZWcgd2hlbiBjdXJzb3JpbmcpLCBiZWNhdXNlXG4gICAgLy8gSURCIGlzIHdlaXJkIGFuZCBhIHNpbmdsZSBJREJSZXF1ZXN0IGNhbiB5aWVsZCBtYW55IHJlc3BvbnNlcywgc28gdGhlc2UgY2FuJ3QgYmUgY2FjaGVkLlxuICAgIGlmICh2YWx1ZSBpbnN0YW5jZW9mIElEQlJlcXVlc3QpXG4gICAgICAgIHJldHVybiBwcm9taXNpZnlSZXF1ZXN0KHZhbHVlKTtcbiAgICAvLyBJZiB3ZSd2ZSBhbHJlYWR5IHRyYW5zZm9ybWVkIHRoaXMgdmFsdWUgYmVmb3JlLCByZXVzZSB0aGUgdHJhbnNmb3JtZWQgdmFsdWUuXG4gICAgLy8gVGhpcyBpcyBmYXN0ZXIsIGJ1dCBpdCBhbHNvIHByb3ZpZGVzIG9iamVjdCBlcXVhbGl0eS5cbiAgICBpZiAodHJhbnNmb3JtQ2FjaGUuaGFzKHZhbHVlKSlcbiAgICAgICAgcmV0dXJuIHRyYW5zZm9ybUNhY2hlLmdldCh2YWx1ZSk7XG4gICAgY29uc3QgbmV3VmFsdWUgPSB0cmFuc2Zvcm1DYWNoYWJsZVZhbHVlKHZhbHVlKTtcbiAgICAvLyBOb3QgYWxsIHR5cGVzIGFyZSB0cmFuc2Zvcm1lZC5cbiAgICAvLyBUaGVzZSBtYXkgYmUgcHJpbWl0aXZlIHR5cGVzLCBzbyB0aGV5IGNhbid0IGJlIFdlYWtNYXAga2V5cy5cbiAgICBpZiAobmV3VmFsdWUgIT09IHZhbHVlKSB7XG4gICAgICAgIHRyYW5zZm9ybUNhY2hlLnNldCh2YWx1ZSwgbmV3VmFsdWUpO1xuICAgICAgICByZXZlcnNlVHJhbnNmb3JtQ2FjaGUuc2V0KG5ld1ZhbHVlLCB2YWx1ZSk7XG4gICAgfVxuICAgIHJldHVybiBuZXdWYWx1ZTtcbn1cbmNvbnN0IHVud3JhcCA9ICh2YWx1ZSkgPT4gcmV2ZXJzZVRyYW5zZm9ybUNhY2hlLmdldCh2YWx1ZSk7XG5cbmV4cG9ydCB7IHJldmVyc2VUcmFuc2Zvcm1DYWNoZSBhcyBhLCBpbnN0YW5jZU9mQW55IGFzIGksIHJlcGxhY2VUcmFwcyBhcyByLCB1bndyYXAgYXMgdSwgd3JhcCBhcyB3IH07XG4iLCIvLyBUaGUgbW9kdWxlIGNhY2hlXG52YXIgX193ZWJwYWNrX21vZHVsZV9jYWNoZV9fID0ge307XG5cbi8vIFRoZSByZXF1aXJlIGZ1bmN0aW9uXG5mdW5jdGlvbiBfX3dlYnBhY2tfcmVxdWlyZV9fKG1vZHVsZUlkKSB7XG5cdC8vIENoZWNrIGlmIG1vZHVsZSBpcyBpbiBjYWNoZVxuXHR2YXIgY2FjaGVkTW9kdWxlID0gX193ZWJwYWNrX21vZHVsZV9jYWNoZV9fW21vZHVsZUlkXTtcblx0aWYgKGNhY2hlZE1vZHVsZSAhPT0gdW5kZWZpbmVkKSB7XG5cdFx0cmV0dXJuIGNhY2hlZE1vZHVsZS5leHBvcnRzO1xuXHR9XG5cdC8vIENyZWF0ZSBhIG5ldyBtb2R1bGUgKGFuZCBwdXQgaXQgaW50byB0aGUgY2FjaGUpXG5cdHZhciBtb2R1bGUgPSBfX3dlYnBhY2tfbW9kdWxlX2NhY2hlX19bbW9kdWxlSWRdID0ge1xuXHRcdC8vIG5vIG1vZHVsZS5pZCBuZWVkZWRcblx0XHQvLyBubyBtb2R1bGUubG9hZGVkIG5lZWRlZFxuXHRcdGV4cG9ydHM6IHt9XG5cdH07XG5cblx0Ly8gRXhlY3V0ZSB0aGUgbW9kdWxlIGZ1bmN0aW9uXG5cdF9fd2VicGFja19tb2R1bGVzX19bbW9kdWxlSWRdKG1vZHVsZSwgbW9kdWxlLmV4cG9ydHMsIF9fd2VicGFja19yZXF1aXJlX18pO1xuXG5cdC8vIFJldHVybiB0aGUgZXhwb3J0cyBvZiB0aGUgbW9kdWxlXG5cdHJldHVybiBtb2R1bGUuZXhwb3J0cztcbn1cblxuIiwiLy8gZGVmaW5lIGdldHRlciBmdW5jdGlvbnMgZm9yIGhhcm1vbnkgZXhwb3J0c1xuX193ZWJwYWNrX3JlcXVpcmVfXy5kID0gKGV4cG9ydHMsIGRlZmluaXRpb24pID0+IHtcblx0Zm9yKHZhciBrZXkgaW4gZGVmaW5pdGlvbikge1xuXHRcdGlmKF9fd2VicGFja19yZXF1aXJlX18ubyhkZWZpbml0aW9uLCBrZXkpICYmICFfX3dlYnBhY2tfcmVxdWlyZV9fLm8oZXhwb3J0cywga2V5KSkge1xuXHRcdFx0T2JqZWN0LmRlZmluZVByb3BlcnR5KGV4cG9ydHMsIGtleSwgeyBlbnVtZXJhYmxlOiB0cnVlLCBnZXQ6IGRlZmluaXRpb25ba2V5XSB9KTtcblx0XHR9XG5cdH1cbn07IiwiX193ZWJwYWNrX3JlcXVpcmVfXy5vID0gKG9iaiwgcHJvcCkgPT4gKE9iamVjdC5wcm90b3R5cGUuaGFzT3duUHJvcGVydHkuY2FsbChvYmosIHByb3ApKSIsIi8vIGRlZmluZSBfX2VzTW9kdWxlIG9uIGV4cG9ydHNcbl9fd2VicGFja19yZXF1aXJlX18uciA9IChleHBvcnRzKSA9PiB7XG5cdGlmKHR5cGVvZiBTeW1ib2wgIT09ICd1bmRlZmluZWQnICYmIFN5bWJvbC50b1N0cmluZ1RhZykge1xuXHRcdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShleHBvcnRzLCBTeW1ib2wudG9TdHJpbmdUYWcsIHsgdmFsdWU6ICdNb2R1bGUnIH0pO1xuXHR9XG5cdE9iamVjdC5kZWZpbmVQcm9wZXJ0eShleHBvcnRzLCAnX19lc01vZHVsZScsIHsgdmFsdWU6IHRydWUgfSk7XG59OyIsImltcG9ydCB7RGF0YWJhc2VDb250ZXh0fSBmcm9tIFwiLi9EYXRhYmFzZS9EYXRhYmFzZUNvbnRleHRcIjtcclxuXHJcbmV4cG9ydCB7fVxyXG5cclxuZGVjbGFyZSBnbG9iYWwge1xyXG4gICAgaW50ZXJmYWNlIFdpbmRvd3tcclxuICAgICAgICBpc09ubGluZSgpIDogYm9vbGVhbjtcclxuICAgICAgICBhcHBseVVybCh1cmw6c3RyaW5nKTtcclxuICAgIH1cclxufVxyXG5cclxuZXhwb3J0IG5hbWVzcGFjZSBJbmRleCB7XHJcblxyXG4gICAgaW1wb3J0IGluaXREYXRhYmFzZSA9IERhdGFiYXNlQ29udGV4dC5pbml0RGF0YWJhc2U7XHJcbiAgICBpbml0KClcclxuXHJcbiAgICBmdW5jdGlvbiBpbml0KCkge1xyXG4gICAgICAgIFxyXG4gICAgICAgIGNvbnNvbGUubG9nKFwiSW5pdCBJbmRleCBBcGlcIilcclxuICAgICAgICB3aW5kb3cuaXNPbmxpbmUgPSBpc09ubGluZTtcclxuICAgICAgICB3aW5kb3cuYXBwbHlVcmwgPSBhcHBseVVybDtcclxuICAgICAgICBcclxuICAgICAgICBpbml0RGF0YWJhc2UoKVxyXG4gICAgfVxyXG5cclxuICAgIGV4cG9ydCBmdW5jdGlvbiBhcHBseVVybCh1cmw6c3RyaW5nKSB7XHJcbiAgICAgICAgd2luZG93Lmhpc3RvcnkucHVzaFN0YXRlKFwibnVsbFwiLCBcIlwiLCB1cmwpO1xyXG4gICAgfVxyXG4gICAgXHJcbiAgICBleHBvcnQgZnVuY3Rpb24gaXNPbmxpbmUoKSB7XHJcbiAgICAgICAgcmV0dXJuIHdpbmRvdy5uYXZpZ2F0b3Iub25MaW5lO1xyXG4gICAgfVxyXG59Il0sIm5hbWVzIjpbXSwic291cmNlUm9vdCI6IiJ9