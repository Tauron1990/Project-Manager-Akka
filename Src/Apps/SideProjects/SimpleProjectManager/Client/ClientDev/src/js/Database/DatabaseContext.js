"use strict";
// noinspection JSUnusedGlobalSymbols
var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
var __generator = (this && this.__generator) || function (thisArg, body) {
    var _ = { label: 0, sent: function() { if (t[0] & 1) throw t[1]; return t[1]; }, trys: [], ops: [] }, f, y, t, g;
    return g = { next: verb(0), "throw": verb(1), "return": verb(2) }, typeof Symbol === "function" && (g[Symbol.iterator] = function() { return this; }), g;
    function verb(n) { return function (v) { return step([n, v]); }; }
    function step(op) {
        if (f) throw new TypeError("Generator is already executing.");
        while (_) try {
            if (f = 1, y && (t = op[0] & 2 ? y["return"] : op[0] ? y["throw"] || ((t = y["return"]) && t.call(y), 0) : y.next) && !(t = t.call(y, op[1])).done) return t;
            if (y = 0, t) op = [op[0] & 2, t.value];
            switch (op[0]) {
                case 0: case 1: t = op; break;
                case 4: _.label++; return { value: op[1], done: false };
                case 5: _.label++; y = op[1]; op = [0]; continue;
                case 7: op = _.ops.pop(); _.trys.pop(); continue;
                default:
                    if (!(t = _.trys, t = t.length > 0 && t[t.length - 1]) && (op[0] === 6 || op[0] === 2)) { _ = 0; continue; }
                    if (op[0] === 3 && (!t || (op[1] > t[0] && op[1] < t[3]))) { _.label = op[1]; break; }
                    if (op[0] === 6 && _.label < t[1]) { _.label = t[1]; t = op; break; }
                    if (t && _.label < t[2]) { _.label = t[2]; _.ops.push(op); break; }
                    if (t[2]) _.ops.pop();
                    _.trys.pop(); continue;
            }
            op = body.call(thisArg, _);
        } catch (e) { op = [6, e]; y = 0; } finally { f = t = 0; }
        if (op[0] & 5) throw op[1]; return { value: op[0] ? op[1] : void 0, done: true };
    }
};
exports.__esModule = true;
exports.DatabaseContext = void 0;
var idb_1 = require("idb");
var DatabaseContext;
(function (DatabaseContext) {
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
            if (msg == undefined) {
                this.sucess = true;
            }
            else {
                this.sucess = false;
                this.message = msg;
            }
        }
        return Result;
    }());
    DatabaseContext.Result = Result;
    function openDataDatabase() {
        return __awaiter(this, void 0, void 0, function () {
            var _a;
            return __generator(this, function (_b) {
                switch (_b.label) {
                    case 0:
                        if (!(StaticData.database == null)) return [3 /*break*/, 2];
                        _a = StaticData;
                        return [4 /*yield*/, (0, idb_1.openDB)("StateData", 1, {
                                upgrade: function (db) {
                                    db.createObjectStore("timeout", { keyPath: "id" });
                                    db.createObjectStore("data", { keyPath: "id" });
                                }
                            })];
                    case 1:
                        _a.database = _b.sent();
                        _b.label = 2;
                    case 2: return [2 /*return*/, StaticData.database];
                }
            });
        });
    }
    function saveData(id, data) {
        return __awaiter(this, void 0, void 0, function () {
            var db;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
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
        return __awaiter(this, void 0, void 0, function () {
            var db, dataList;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
                    case 1:
                        db = _a.sent();
                        return [4 /*yield*/, db.getAll("timeout")];
                    case 2:
                        dataList = _a.sent();
                        return [2 /*return*/, dataList.map(function (v) { return v.data; })];
                }
            });
        });
    }
    DatabaseContext.getAllTimeoutElements = getAllTimeoutElements;
    function deleteElement(id, timeoutId) {
        return __awaiter(this, void 0, void 0, function () {
            var db, transaction, timeoutStore, dataStore, timeoutData;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
                    case 1:
                        db = _a.sent();
                        transaction = db.transaction(["timeout", "data"], "readwrite");
                        timeoutStore = transaction.objectStore(transaction.objectStoreNames[0]);
                        dataStore = transaction.objectStore(transaction.objectStoreNames[1]);
                        return [4 /*yield*/, timeoutStore.get(timeoutId)];
                    case 2:
                        timeoutData = _a.sent();
                        if (!(timeoutData !== undefined)) return [3 /*break*/, 4];
                        return [4 /*yield*/, timeoutStore["delete"](timeoutId)];
                    case 3:
                        _a.sent();
                        _a.label = 4;
                    case 4: return [4 /*yield*/, dataStore["delete"](id)];
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
        return __awaiter(this, void 0, void 0, function () {
            var db, e_1;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0:
                        _a.trys.push([0, 3, , 4]);
                        return [4 /*yield*/, openDataDatabase()];
                    case 1:
                        db = _a.sent();
                        return [4 /*yield*/, db["delete"]("timeout", id)];
                    case 2:
                        _a.sent();
                        return [2 /*return*/, new Result()];
                    case 3:
                        e_1 = _a.sent();
                        return [2 /*return*/, new Result(e_1.toString())];
                    case 4: return [2 /*return*/];
                }
            });
        });
    }
    DatabaseContext.deleteTimeoutElement = deleteTimeoutElement;
    function getTimeout(id) {
        return __awaiter(this, void 0, void 0, function () {
            var db, entry;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
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
        return __awaiter(this, void 0, void 0, function () {
            var db;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
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
        return __awaiter(this, void 0, void 0, function () {
            var db, result;
            return __generator(this, function (_a) {
                switch (_a.label) {
                    case 0: return [4 /*yield*/, openDataDatabase()];
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
})(DatabaseContext = exports.DatabaseContext || (exports.DatabaseContext = {}));
