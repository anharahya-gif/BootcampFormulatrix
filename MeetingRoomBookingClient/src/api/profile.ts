import api from './axios';
import type { UserReadDto, UserUpdateDto, ServiceResult } from '../types';

export const profileService = {
    getProfile: async (): Promise<UserReadDto> => {
        const response = await api.get<ServiceResult<UserReadDto>>('/Profile');
        return response.data.data;
    },

    updateProfile: async (data: UserUpdateDto): Promise<UserReadDto> => {
        const response = await api.put<ServiceResult<UserReadDto>>('/Profile', data);
        return response.data.data;
    }
};
