import { UserConfig, defineConfig } from 'vite';
import vue from '@vitejs/plugin-vue';
import { spawn } from 'child_process';
import fs from 'fs';
import path from 'path';

const baseFolder =
    process.env.APPDATA !== undefined && process.env.APPDATA !== ''
        ? `${process.env.APPDATA}/ASP.NET/https`
        : `${process.env.HOME}/.aspnet/https`;

const certificateName = process.env.npm_package_name;
const certFilePath = path.join(baseFolder, `${certificateName}.pem`);
const keyFilePath = path.join(baseFolder, `${certificateName}.key`);

const cssPattern = /\.css$/;
const imagePattern = /\.(png|jpe?g|gif|svg|webp|avif)$/;

// Export Vite configuration
export default defineConfig(async ({ command, mode, isSsrBuild, isPreview }) => {
    // Ensure the certificate and key exist
    if (command === 'serve' && (!fs.existsSync(certFilePath) || !fs.existsSync(keyFilePath))) {
        // Wait for the certificate to be generated
        await new Promise<void>((resolve) => {
            spawn('dotnet', [
                'dev-certs',
                'https',
                '--export-path',
                certFilePath,
                '--format',
                'Pem',
                '--no-password',
            ], { stdio: 'inherit', })
                .on('exit', (code) => {
                    resolve();
                    if (code) {
                        process.exit(code);
                    }
                });
        });
    };

    // Define Vite configuration
    const config: UserConfig = {
        appType: 'custom',
        root: 'src',
        publicDir: '../public',
        plugins: [vue()],
        resolve: {
            alias: {
                vue: 'vue/dist/vue.esm-bundler.js',
                stream: 'stream-browserify'    // workaround for Plotly
            },
        },
        build: {
            manifest: 'vite-manifest.json',
            emptyOutDir: true,
            outDir: '../wwwroot',
            assetsDir: '',
            rollupOptions: {
                input: ['src/navbar-main.ts', 'src/careerlog-main.ts', 'src/records-main.ts', 'src/races-main.ts', 'src/css/styles.scss'],
                output: {
                    entryFileNames: 'js/[name].[hash].js',
                    chunkFileNames: 'js/[name]-chunk.js',
                    assetFileNames: (info) => {
                        if (info.name) {
                            if (cssPattern.test(info.name)) {
                                return 'css/[name][extname]';
                            }
                            if (imagePattern.test(info.name)) {
                                return 'images/[name][extname]';
                            }

                            return 'assets/[name][extname]';
                        } else {
                            return '[name][extname]';    // If the file name is not specified, save it to the output directory
                        }
                    },
                }
            },
        },
        server: {
            strictPort: true,
            https: {
                cert: certFilePath,
                key: keyFilePath
            }
        },
        optimizeDeps: {
            include: [],
            esbuildOptions: {
                define: {
                    global: 'globalThis'    // workaround for Plotly
                }
            }
        }
    }

    return config;
});
