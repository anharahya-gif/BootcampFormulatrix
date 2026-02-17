import api from './axios';
import type { DeletedItem, DeletedItemType, ServiceResult } from '../types';

export const recycleBinService = {
    getDeletedItems: async (): Promise<DeletedItem[]> => {
        const response = await api.get<ServiceResult<DeletedItem[]>>('/RecycleBin');
        return response.data.data;
    },

    restoreItem: async (id: string, type: DeletedItemType): Promise<boolean> => {
        const response = await api.post<ServiceResult<boolean>>(`/RecycleBin/${id}/restore?type=${type}`);
        return response.data.data;
    },

    hardDelete: async (id: string, type: DeletedItemType): Promise<boolean> => {
        const response = await api.delete<ServiceResult<boolean>>(`/RecycleBin/${id}?type=${type}`);
        return response.data.data;
    }
};
