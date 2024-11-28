import { defineConfig } from 'vite'

export default defineConfig({
    server: {
        host: "0.0.0.0",
        port: 8080
    },
    root: "src/Client",
    publicDir: "assets",
    build: {
        // note: relative to src/Client
        outDir: "../../deploy-client/",
        emptyOutDir: true,
        sourcemap: true
    }
});
