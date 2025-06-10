import { HttpClient } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { BehaviorSubject, Observable, of } from 'rxjs';
import { UserData } from '../../interfaces/admin/admin-data';

@Injectable({
  providedIn: 'root',
})
export class UserService {
  userId: BehaviorSubject<number> = new BehaviorSubject<number>(0);
  registered: BehaviorSubject<string> = new BehaviorSubject<string>('');
  image: BehaviorSubject<string> = new BehaviorSubject<string>('');
  userName: BehaviorSubject<string> = new BehaviorSubject<string>('');
  role: BehaviorSubject<string> = new BehaviorSubject<string>('');
  redirectReason: BehaviorSubject<string | null> = new BehaviorSubject<
    string | null
  >(null);

  constructor(private _HttpClient: HttpClient) {}

  registerData(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Authentication/register`,
      value
    );
  }

  loginData(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Authentication/login`,
      value
    );
  }

  logoutConfirmation(): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Authentication/logout`,
      {}
    );
  }

  refreshToken(): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Token/refresh`,
      {},
      {
        headers: {
          'Content-Type': 'application/json',
        },
      }
    );
  }

  getProfileData(userId: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Users/profile`, {
      params: { id: userId },
    });
  }

  getUserImage(userId: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Users/image`, {
      params: { id: userId },
      responseType: 'blob',
    });
  }

  updateEmail(userId: number, newEmail: string): Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Users/update-email`, {
      id: userId,
      newEmail,
    });
  }

  updateUserName(userId: number, newUserName: string): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Users/update-username`,
      {
        id: userId,
        newUserName,
      }
    );
  }

  updateImage(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/users/update-image`,
      value
    );
  }

  updatePassword(
    userId: number,
    oldPassword: string,
    newPassword: string
  ): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/users/update-password`,
      {
        id: userId,
        oldPassword,
        newPassword,
      }
    );
  }

  getAllUsers(index: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Admin/users`, {
      params: {
        index,
      },
    });
  }

  banUser(userId: number): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Admin/ban/${userId}`,
      {}
    );
  }
  unbanUser(userId: number): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Admin/unban/${userId}`,
      {}
    );
  }
}
