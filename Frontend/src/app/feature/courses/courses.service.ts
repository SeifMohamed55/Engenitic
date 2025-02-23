import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CoursesService {

  constructor(private _HttpClient:HttpClient) { }

  coursesOffered(collectionNumber : number) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Courses/dummy/${collectionNumber}`, {withCredentials: true});
  };

  getCourseDetails(courseId : number) : Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Courses/id/${courseId}`, {withCredentials: true})
  };

  searchForCourseCollection(courseTitle : string, index : number = 1) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Courses/search`, {withCredentials: true, params : 
      {search : courseTitle, index : index}
    });
  };
}
