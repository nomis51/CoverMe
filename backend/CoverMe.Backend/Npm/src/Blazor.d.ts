interface BlazorRef {
    invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>;
}