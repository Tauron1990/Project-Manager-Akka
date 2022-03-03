// noinspection JSUnusedGlobalSymbols
import { __awaiter, __generator } from "tslib";
import { openDB } from 'idb';
export var DatabaseContext;
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
                        return [4 /*yield*/, openDB("StateData", 1, {
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
                        return [4 /*yield*/, timeoutStore.delete(timeoutId)];
                    case 3:
                        _a.sent();
                        _a.label = 4;
                    case 4: return [4 /*yield*/, dataStore.delete(id)];
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
                        return [4 /*yield*/, db.delete("timeout", id)];
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
})(DatabaseContext || (DatabaseContext = {}));
//# sourceMappingURL=DatabaseContext.js.map