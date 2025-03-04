import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class CoursesService {

  constructor(private _HttpClient:HttpClient) { }

  getEnrolledCourses(index : number, userId : number) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Student/courses`, {
      params : {
        index,
        id : userId
      }
    });
  };

  coursesOffered(collectionNumber : number) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Courses/dummy/${collectionNumber}`);
  };

  getCourseDetails(courseId : number) : Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Courses/id/${courseId}`)
  };

  searchForCourseCollection(courseTitle : string, index : number = 1) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Courses/search`, { 
    params : 
      {
        search : courseTitle,
        index : index
      }
    });
  };
}
