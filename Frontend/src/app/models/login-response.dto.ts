import { User } from "./user.model";

export interface LoginResponseDto {
    token: string;
    user: User;
  }