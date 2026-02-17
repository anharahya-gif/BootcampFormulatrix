export interface User {
  id: string;
  email: string;
  userName: string;
  fullName: string;
  department?: string;
  phoneNumber?: string;
  avatarUrl?: string;
  role?: string;
  createdAt?: string;
}

export type UserReadDto = User;

export interface UserCreateDto {
  email: string;
  password?: string;
  fullName: string;
  department?: string;
  phoneNumber?: string;
  role: string;
}

export interface UserUpdateDto {
  fullName?: string;
  department?: string;
  phoneNumber?: string;
  avatarUrl?: string;
}

export interface ServiceResult<T> {
  success: boolean;
  data: T;
  message: string | null;
  statusCode: number;
  errors: string[];
}

export type DeletedItemType = 'Room' | 'Booking' | 'User';

export interface DeletedItem {
  id: string;
  name: string;
  type: DeletedItemType;
  deletedAt: string;
  deletedBy?: string;
}

export interface LoginDto {
  email: string;
  password?: string;
}

export interface RegisterDto {
  email: string;
  password?: string;
  fullName: string;
  department?: string;
  phoneNumber?: string;
}

export interface AuthResponseDto {
  success: boolean;
  token?: string;
  errorMessage?: string;
  userName?: string;
  email?: string;
  role?: string;
}

export interface RoomDto {
  id: string;
  name: string;
  capacity: number;
  location: string;
  hasProjector: boolean;
  createdAt?: string;
}

export interface RoomCreateDto {
  name: string;
  capacity: number;
  location: string;
  hasProjector: boolean;
}

export interface RoomUpdateDto {
  name: string;
  capacity: number;
  location: string;
  hasProjector: boolean;
}

export interface BookingDto {
  id: string;
  title: string;
  description?: string;
  roomId: string;
  roomName?: string;
  createdByUserId?: string;
  createdByUserName?: string;
  startTime: string;
  endTime: string;
  status: number;
  participantUserIds?: string[];
}

export interface BookingCreateDto {
  title: string;
  description?: string;
  roomId: string;
  startTime: string;
  endTime: string;
  participantUserIds: string[];
}

export const BookingStatus = {
  Pending: 0,
  Approved: 1,
  Rejected: 2,
  Cancelled: 3
} as const;

export type BookingStatus = typeof BookingStatus[keyof typeof BookingStatus];