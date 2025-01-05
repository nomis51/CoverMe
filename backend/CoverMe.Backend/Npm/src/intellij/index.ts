export const NAMESPACE = 'intellij';

interface ProjectSettings {
    projectRootPath: string;
    channelId: string;
}

export class Intellij {
    public static namespace = NAMESPACE;

    /**
     * Public functions
     */
    public getProjectSettings(): ProjectSettings | undefined {
        if (!window[NAMESPACE]) return;

        return {
            projectRootPath: window[NAMESPACE].PROJECT_ROOT_PATH as string,
            channelId: window[NAMESPACE].CHANNEL_ID as string
        };
    }
}