import {DatabaseContext} from "./Database/DatabaseContext";

export {}

declare global {
    interface Window {
        isOnline(): boolean;

        applyUrl(url: string);
    }
}

export namespace Index {

    import initDatabase = DatabaseContext.initDatabase;
    init()

    function init() {

        console.log("Init Index Api")
        window.isOnline = isOnline;
        window.applyUrl = applyUrl;

        initDatabase()
    }

    export function applyUrl(url: string) {
        window.history.pushState("null", "", url);
    }

    export function isOnline() {
        return window.navigator.onLine;
    }
}