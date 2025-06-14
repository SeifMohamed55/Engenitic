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
    return this._HttpClient.get(`/api/Courses/search`, {
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
    return this._HttpClient.get(`/api/Student/courses`, {
      params: {
        index,
        id: userId,
      },
    });
  }

  enrollCourseHandler(courseId: number, studentId: number): Observable<any> {
    return this._HttpClient.post(`/api/student/enroll`, {
      courseId,
      studentId,
    });
  }

  getCreatedCourses(
    collectionId: number,
    instructorId: number
  ): Observable<any> {
    return this._HttpClient.get(`/api/Instructor/courses`, {
      params: {
        index: collectionId,
        instructorId,
      },
    });
  }

  coursesOffered(collectionNumber: number): Observable<any> {
    return this._HttpClient.get(
      `/api/Courses/${collectionNumber}`
    );
  }

  getCourseDetails(courseId: number): Observable<any> {
    return this._HttpClient.get(`/api/Courses/id/${courseId}`);
  }

  deleteCourse(courseId: number, instructorId: number): Observable<any> {
    return this._HttpClient.delete(
      `/api/Instructor/deleteCourse`,
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
      `/api/Instructor/editCourse`,
      value
    );
  }

  editCourseImage(value: any): Observable<any> {
    return this._HttpClient.post(
      `/api/Instructor/editCourseImage`,
      value
    );
  }

  getCourseWithQuizzes(courseId: number): Observable<any> {
    return this._HttpClient.get(
      `/api/Instructor/course-with-quizes`,
      {
        params: {
          courseId,
        },
      }
    );
  }

  addCourse(value: any): Observable<any> {
    return this._HttpClient.post(
      `/api/Instructor/addCourse`,
      value
    );
  }

  getCurrentStage(studentId: number, enrollmentId: number): Observable<any> {
    return this._HttpClient.get(
      `/api/Student/enrollment/current-stage`,
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
    return this._HttpClient.get(`/api/Student/enrollment`, {
      params: {
        studentId,
        enrollmentId,
        stage,
      },
    });
  }

  getQuizTitles(courseId: number): Observable<any> {
    return this._HttpClient.get(`/api/courses/quizzes-title`, {
      params: {
        courseId,
      },
    });
  }

  submitQuiz(value: any): Observable<any> {
    return this._HttpClient.post(
      `/api/student/enrollment/attempt-quiz`,
      value
    );
  }

  GetRandomCourses(): Observable<any> {
    return this._HttpClient.get(`/api/courses/random4`);
  }

  getCourseReviews(courseId: number, index: number): Observable<any> {
    return this._HttpClient.get(`/api/reviews`, {
      params: {
        courseId,
        index,
      },
    });
  }

  confirmEmailChange(
    userId: number,
    newEmail: string,
    token: string
  ): Observable<any> {
    console.log({ userId, newEmail, token });
    return this._HttpClient.post(
      `/api/Users/confirm-email-change`,
      {
        userId,
        newEmail,
        token,
      }
    );
  }

  addReview(value: {
    courseId: number;
    content: string;
    rating: number;
  }): Observable<any> {
    return this._HttpClient.post(`/api/reviews/add`, value);
  }

  editReview(value: {
    reviewId: number;
    content: string;
    rating: number;
  }): Observable<any> {
    return this._HttpClient.post(`/api/reviews/edit`, value);
  }

  deleteReview(reviewId: number): Observable<any> {
    return this._HttpClient.post(
      `/api/reviews/delete/${reviewId}`,
      {}
    );
  }
}
