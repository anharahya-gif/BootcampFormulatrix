import api from './axios';
import type { UserReadDto, UserCreateDto, ServiceResult } from '../types';

export const usersService = {
    getAll: async (): Promise<UserReadDto[]> => {
        const response = await api.get<ServiceResult<UserReadDto[]>>('/Users');
        return response.data.data;
    },

    create: async (data: UserCreateDto): Promise<UserReadDto> => {
        const response = await api.post<ServiceResult<UserReadDto>>('/Users', data);
        return response.data.data;
    },

    updateRole: async (id: string, role: string): Promise<boolean> => {
        const response = await api.post<ServiceResult<boolean>>(`/Users/${id}/role`, JSON.stringify(role), {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data.data;
    },

    delete: async (id: string): Promise<boolean> => {
        const response = await api.delete<ServiceResult<boolean>>(`/Users/${id}`);
        return response.data.data;
    }
};
