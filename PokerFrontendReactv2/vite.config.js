import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 3000,
    proxy: {
      '/api': {
        target: 'http://localhost:5292', // Adjusted to match PokerAPIMPwDBv2 port
        changeOrigin: true,
        secure: false
      },
      '/pokerHub': {
        target: 'http://localhost:5292',
        ws: true,
        changeOrigin: true,
        secure: false
      }
    }
  }
})
