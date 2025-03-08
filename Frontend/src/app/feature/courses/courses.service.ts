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

  getCreatedCourses(collectionId : number , instructorId : number) : Observable<any>{
    return this._HttpClient.get(`https://localhost/api/Instructor/courses`, {
      params : {
        index : collectionId,
        instructorId
      }
    })
  }

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

  deleteCourse(courseId : number, instructorId : number) : Observable<any> {
    return this._HttpClient.delete(`https://localhost/api/Instructor/deleteCourse`,  
      {
        body : {
          instructorId,
          courseId
        }
      }
    );
  };

  editCourse(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Instructor/editCourse`, value);
  };

  addCourse(value : any) : Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Instructor/addCourse`, value);
  };
}
