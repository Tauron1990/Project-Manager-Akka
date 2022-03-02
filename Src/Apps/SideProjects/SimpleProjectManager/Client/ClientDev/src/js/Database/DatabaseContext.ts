// noinspection JSUnusedGlobalSymbols

import { openDB, DBSchema, IDBPDatabase } from 'idb';

export namespace DatabaseContext {

    interface IDefaultDatabase extends DBSchema {
        timeout: {
            value: DataContainer;
            key: string;
        };
        data: {
            value: DataContainer;
            key: string;
        };
    }

    class DataContainer {
        id: string;
        data: JSON;

        constructor(id: string, data: JSON) {
            this.id = id;
            this.data = data;
        }
    }

    class StaticData {
        static database: IDBPDatabase<IDefaultDatabase> = null;
    }

    export class Result {
        sucess: boolean;
        message: string;

        constructor(msg?: string) {
            if (msg == undefined) {
                this.sucess = true;
            } else {
                this.sucess = false;
                this.message = msg;
            }
        }
    }

    async function openDataDatabase() {

        if (StaticData.database == null) {
            StaticData.database = await openDB<IDefaultDatabase>("StateData",
                1,
                {
                    upgrade(db) {
                        db.createObjectStore("timeout", { keyPath: "id" });
                        db.createObjectStore("data", { keyPath: "id" });
                    }
                });
            
        }

        return StaticData.database;
    }

    export async function saveData(id: string, data: JSON) {
            const db = await openDataDatabase();
            await db.put("data", new DataContainer(id, data));
    }
    
    export async function getAllTimeoutElements() {
        const db = await openDataDatabase();

        const dataList = await db.getAll("timeout");

        return dataList.map((v: DataContainer) => v.data);
    }

    export async function deleteElement(id:string, timeoutId:string) {
        const db = await openDataDatabase();
        const transaction = db.transaction(["timeout", "data"], "readwrite");

        const timeoutStore = transaction.objectStore(transaction.objectStoreNames[0]);
        const dataStore = transaction.objectStore(transaction.objectStoreNames[1]);

        const timeoutData = await timeoutStore.get(timeoutId);

        if (timeoutData !== undefined) {
            await timeoutStore.delete(timeoutId);
        }
        await dataStore.delete(id);

        await transaction.done;
    }

    export async function deleteTimeoutElement(id: string) {
        try {
            const db = await openDataDatabase();
            await db.delete("timeout", id);

            return new Result();
        } catch (e) {
            return new Result(e.toString());
        } 
    }

    export async function getTimeout(id:string){
        const db = await openDataDatabase();
        const entry = await db.get("timeout", id);
        if(entry === undefined){
            return "";
        }
        
        return entry.data;
    }
    
    export async function updateTimeout(id:string, data:JSON){
        const db = await openDataDatabase();
        await db.put("timeout", new DataContainer(id, data), id);
    }
    
    export async function getCacheEntry(id:string)
    {
        const db = await openDataDatabase();
        
        const result = await db.get("data", id);
    
        if(result === undefined) return "";
        
        return result.data;
    }
}