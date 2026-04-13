import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';
import { resolve } from 'path';
import { fileURLToPath } from 'url';

const __dirname = fileURLToPath(new URL('.', import.meta.url));

export default defineConfig({
  plugins: [react()],
  build: {
    outDir: 'wwwroot/js',
    emptyOutDir: false,
    sourcemap: true,
    rollupOptions: {
      input: {
        keyboard:       resolve(__dirname, 'ts/keyboard.ts'),
        sounds:         resolve(__dirname, 'ts/sounds.ts'),
        tooltips:       resolve(__dirname, 'ts/tooltips.ts'),
        GalaxyRenderer: resolve(__dirname, 'ts/GalaxyRenderer.ts'),
        tacticalViewer: resolve(__dirname, 'ts/tacticalViewer.ts'),
        tutorial:       resolve(__dirname, 'ts/tutorial.ts'),
        // Theme test mockups temporarily excluded (corrupted, need manual repair)
        // 'lcars-test':    resolve(__dirname, 'ts/lcars-test.tsx'),
        // 'klingon-test':  resolve(__dirname, 'ts/klingon-test.tsx'),
        // 'borg-test':     resolve(__dirname, 'ts/borg-test.tsx'),
        // 'romulan-test':  resolve(__dirname, 'ts/romulan-test.tsx'),
      },
      output: {
        entryFileNames: '[name].js',
        chunkFileNames: 'chunks/[name].js',
        format: 'es',
      },
    },
  },
  resolve: {
    alias: { '@ts': resolve(__dirname, 'ts') },
  },
});
