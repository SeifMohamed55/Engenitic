import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private _HttpClient:HttpClient) {};

  registerData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/register`, value);
  };

  loginData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/login`, value);
  };

  logoutConfirmation() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/logout`, null);
  };

  refreshToken() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Token/refresh`, null);
  };

};
