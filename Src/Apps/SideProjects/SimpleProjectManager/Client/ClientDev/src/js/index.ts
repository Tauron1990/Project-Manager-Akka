import {DatabaseContext} from "./Database/DatabaseContext";

export {}

declare global {
    interface Window{
        isOnline() : boolean;
    }
}

export namespace Index {

    import initDatabase = DatabaseContext.initDatabase;
    init()

    function init() {
        
        console.log("Init Index Api")
        window.isOnline = isOnline;
        
        initDatabase()
    }

    export function isOnline() {
        return window.navigator.onLine;
    }
}