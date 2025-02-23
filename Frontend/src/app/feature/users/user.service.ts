import { HttpClient } from '@angular/common/http';
import { Injectable, OnInit } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  userId : BehaviorSubject<number> = new BehaviorSubject(0);
  registered : BehaviorSubject<string> = new BehaviorSubject("");
  image : BehaviorSubject<string> = new BehaviorSubject("");
  userName : BehaviorSubject<string> = new BehaviorSubject("");


  constructor(
    private _HttpClient:HttpClient
  ) {};

  registerData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/register`, value, {withCredentials: true});
  };

  loginData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/login`, value, {withCredentials: true});
  };

  logoutConfirmation() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/logout`, {}, {withCredentials : true });
  };

  refreshToken() : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Token/refresh`, {}, {withCredentials : true });
  };

  getProfileData(userId : number) : Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Users/profile`, {withCredentials: true, params :
      {id : userId}
    });
  };

  getUserImage(userId : number) : Observable<any> {
    return this._HttpClient.get(`https://localhost/api/users/image`, { withCredentials : true , params : 
      {id : userId} , responseType : 'blob'
    });
  };
};
