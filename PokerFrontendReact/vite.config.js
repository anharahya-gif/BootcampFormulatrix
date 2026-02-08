import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
        '/api': {
            target: 'http://localhost:5175', // Adjust if your backend port is different
            changeOrigin: true,
            secure: false
        },
        '/pokerHub': {
            target: 'http://localhost:5175',
            ws: true,
            changeOrigin: true,
            secure: false
        }
    }
  }
})
