import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private _HttpClient:HttpClient) {

  }

  registerData(value : any) : Observable<any> 
  {
    return this._HttpClient.post(`https://localhost/api/Authentication/register`, value)
  };

  loginData(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Authentication/login`, value);
  };
}
