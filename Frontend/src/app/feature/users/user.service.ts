import { Injectable } from '@angular/core';
import { HttpClient, Observable } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  constructor(private _HttpClient:HttpClient) {

  }

  registerData(value : any) : Observable<any> 
  {
    this._HttpClient.post(`https://localhost/api/Authentication/register`, value)
  };

  
}
