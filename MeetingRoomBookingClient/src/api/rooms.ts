import api from './axios';
import type { RoomDto, RoomCreateDto, RoomUpdateDto } from '../types';

interface ServiceResult<T> {
    success: boolean;
    data: T;
    message: string | null;
    statusCode: number;
    errors: string[];
}

export const roomsService = {
    getAll: async (): Promise<RoomDto[]> => {
        const response = await api.get<ServiceResult<RoomDto[]>>('/Rooms');
        return response.data.data;
    },

    getById: async (id: string): Promise<RoomDto> => {
        const response = await api.get<ServiceResult<RoomDto>>(`/Rooms/${id}`);
        return response.data.data;
    },

    create: async (data: RoomCreateDto): Promise<RoomDto> => {
        const response = await api.post<ServiceResult<RoomDto>>('/Rooms', data);
        return response.data.data;
    },

    update: async (id: string, data: RoomUpdateDto): Promise<RoomDto> => {
        const response = await api.put<ServiceResult<RoomDto>>(`/Rooms/${id}`, data);
        return response.data.data;
    },

    delete: async (id: string): Promise<void> => {
        await api.delete(`/Rooms/${id}`);
    },
};
