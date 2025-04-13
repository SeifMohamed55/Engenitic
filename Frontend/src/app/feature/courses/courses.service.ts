import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class CoursesService {
  constructor(private _HttpClient: HttpClient) {}

  currentSearchQuery = '';

  isSearchActive = new BehaviorSubject<boolean>(false);
  private searchResults = new BehaviorSubject<any>(null);
  currentSearchResults = this.searchResults.asObservable();

  updateSearchResults(results: any) {
    this.searchResults.next(results);
    this.isSearchActive.next(true);
  }

  clearSearchResults() {
    this.currentSearchQuery = '';
    this.searchResults.next(null);
    this.isSearchActive.next(false);
  }

  searchForCourseCollection(
    courseTitle: string,
    index: number = 1
  ): Observable<any> {
    this.currentSearchQuery = courseTitle;
    return this._HttpClient.get(`https://localhost/api/Courses/search`, {
      params: {
        search: courseTitle,
        index: index.toString(),
      },
    });
  }

  // Add this method to handle search pagination
  searchCourses(query: string, page: number): Observable<any> {
    return this.searchForCourseCollection(query, page);
  }

  getEnrolledCourses(index: number, userId: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Student/courses`, {
      params: {
        index,
        id: userId,
      },
    });
  }

  enrollCourseHandler(courseId: number, studentId: number): Observable<any> {
    return this._HttpClient.post(`https://localhost/api/student/enroll`, {
      courseId,
      studentId,
    });
  }

  getCreatedCourses(
    collectionId: number,
    instructorId: number
  ): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Instructor/courses`, {
      params: {
        index: collectionId,
        instructorId,
      },
    });
  }

  coursesOffered(collectionNumber: number): Observable<any> {
    return this._HttpClient.get(
      `https://localhost/api/Courses/${collectionNumber}`
    );
  }

  getCourseDetails(courseId: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Courses/id/${courseId}`);
  }

  deleteCourse(courseId: number, instructorId: number): Observable<any> {
    return this._HttpClient.delete(
      `https://localhost/api/Instructor/deleteCourse`,
      {
        body: {
          instructorId,
          courseId,
        },
      }
    );
  }

  editCourse(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Instructor/editCourse`,
      value
    );
  }

  addCourse(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/Instructor/addCourse`,
      value
    );
  }

  getCurrentStage(studentId: number, enrollmentId: number): Observable<any> {
    return this._HttpClient.get(
      `https://localhost/api/Student/enrollment/current-stage`,
      {
        params: {
          studentId,
          enrollmentId,
        },
      }
    );
  }

  getEnrollmentStage(
    studentId: number,
    enrollmentId: number,
    stage: number
  ): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/Student/enrollment`, {
      params: {
        studentId,
        enrollmentId,
        stage,
      },
    });
  }

  getQuizTitles(courseId: number): Observable<any> {
    return this._HttpClient.get(`https://localhost/api/courses/quizzes-title`, {
      params: {
        courseId,
      },
    });
  }

  submitQuiz(value: any): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/student/enrollment/attempt-quiz`,
      value
    );
  }
}
