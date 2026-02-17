import api from './axios';
import type { BookingDto, BookingCreateDto, ServiceResult } from '../types';
import { BookingStatus } from '../types';

export const bookingsService = {
    getMyBookings: async (): Promise<BookingDto[]> => {
        const response = await api.get<ServiceResult<BookingDto[]>>('/Bookings/my');
        return response.data.data;
    },

    getAll: async (): Promise<BookingDto[]> => {
        const response = await api.get<ServiceResult<BookingDto[]>>('/Bookings');
        return response.data.data;
    },

    getById: async (id: string): Promise<BookingDto> => {
        const response = await api.get<ServiceResult<BookingDto>>(`/Bookings/${id}`);
        return response.data.data;
    },

    create: async (data: BookingCreateDto): Promise<BookingDto> => {
        const response = await api.post<ServiceResult<BookingDto>>('/Bookings', data);
        return response.data.data;
    },

    cancel: async (id: string): Promise<BookingDto> => {
        const response = await api.patch<ServiceResult<BookingDto>>(`/Bookings/${id}/cancel`);
        return response.data.data;
    },

    updateStatus: async (id: string, status: BookingStatus): Promise<BookingDto> => {
        const response = await api.patch<ServiceResult<BookingDto>>(`/Bookings/${id}/status`, status, {
            headers: { 'Content-Type': 'application/json' }
        });
        return response.data.data;
    },
};
