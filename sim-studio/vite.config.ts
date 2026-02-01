import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/ws': {
        target: 'ws://localhost:5001',
        ws: true,
      },
      '/sessions': {
        target: 'http://localhost:5001',
      },
      '/health': {
        target: 'http://localhost:5001',
      },
      '/data': {
        target: 'http://localhost:5001',
      },
    },
  },
  build: {
    outDir: 'dist',
  },
});
