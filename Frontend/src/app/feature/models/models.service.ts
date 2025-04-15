import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ModelsService {
  constructor(private _HttpClient: HttpClient) {}

  VqaModel(value: any): Observable<any> {
    return this._HttpClient.post(`https://localhost/api/Vqa/predict`, value);
  }

  grammarCorrection(sentence: string): Observable<any> {
    return this._HttpClient.post(
      `https://localhost/api/GrammarCorrection/correct`,
      { sentence }
    );
  }
}
