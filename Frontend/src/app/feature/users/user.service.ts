import { HttpClient } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  userId : BehaviorSubject<number> = new BehaviorSubject(0);
  registered : BehaviorSubject<string> = new BehaviorSubject("");
  
  constructor(private _HttpClient:HttpClient) {};

  registerData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/register`, value);
  };

  loginData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/login`, value, {withCredentials: true});
  };

  logoutConfirmation() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/logout`, null, {withCredentials: true});
  };

  refreshToken() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Token/refresh`, null);
  };

  getProfileData(userId : number) : Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Users/profile?id=${userId}`)
  }
};
