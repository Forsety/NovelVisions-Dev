// src/services/auth.service.ts
import apiService from './api.service';
import { API_CONFIG } from '../config/api.config';
import { LoginRequest, RegisterRequest, AuthResult, User } from '../types';

class AuthService {
  async login(credentials: LoginRequest): Promise<AuthResult> {
    const result = await apiService.post<AuthResult>(
      API_CONFIG.ENDPOINTS.AUTH.LOGIN,
      credentials
    );

    if (result.succeeded && result.token && result.refreshToken) {
      apiService.setAuth(result.token, result.refreshToken, result.user);
    }

    return result;
  }

  async register(data: RegisterRequest): Promise<AuthResult> {
    const result = await apiService.post<AuthResult>(
      API_CONFIG.ENDPOINTS.AUTH.REGISTER,
      data
    );

    if (result.succeeded && result.token && result.refreshToken) {
      apiService.setAuth(result.token, result.refreshToken, result.user);
    }

    return result;
  }

  async getProfile(): Promise<User> {
    return apiService.get<User>(API_CONFIG.ENDPOINTS.AUTH.PROFILE);
  }

  async updateProfile(data: Partial<User>): Promise<User> {
    return apiService.put<User>(API_CONFIG.ENDPOINTS.AUTH.PROFILE, data);
  }

  async changePassword(currentPassword: string, newPassword: string): Promise<void> {
    await apiService.post(API_CONFIG.ENDPOINTS.AUTH.CHANGE_PASSWORD, {
      currentPassword,
      newPassword
    });
  }

  async forgotPassword(email: string): Promise<void> {
    await apiService.post(API_CONFIG.ENDPOINTS.AUTH.FORGOT_PASSWORD, { email });
  }

  logout(): void {
    apiService.clearAuth();
    window.location.href = '/';
  }

  isAuthenticated(): boolean {
    return apiService.isAuthenticated();
  }

  getCurrentUser(): User | null {
    return apiService.getUser();
  }
}

export const authService = new AuthService();
export default authService;