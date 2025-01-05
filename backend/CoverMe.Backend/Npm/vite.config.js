import * as path from 'path';
import {defineConfig} from "vite";

export default defineConfig({
    appType: 'mpa',
    build: {
        target: 'esnext',
        outDir: '../wwwroot/js',
        emptyOutDir: true,
        lib: {
            formats: ["umd"],
            entry: path.resolve(__dirname, './src/index.ts'),
            name: "node",
            fileName: (format) => 'app.js',
        },
    }
});