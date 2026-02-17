import api from './axios';
import type { LoginDto, RegisterDto, AuthResponseDto, ServiceResult } from '../types';

export const authService = {
    login: async (data: LoginDto): Promise<AuthResponseDto> => {
        const response = await api.post<ServiceResult<AuthResponseDto>>('/Auth/login', data);
        return response.data.data; // Unwrap ServiceResult
    },

    register: async (data: RegisterDto): Promise<AuthResponseDto> => {
        const response = await api.post<ServiceResult<AuthResponseDto>>('/Auth/register', data);
        return response.data.data; // Unwrap ServiceResult
    },
};
