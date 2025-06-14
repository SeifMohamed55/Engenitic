import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class ModelsService {
  constructor(private _HttpClient: HttpClient) {}

  VqaModel(value: any): Observable<any> {
    return this._HttpClient.post(`/api/Vqa/predict`, value);
  }

  grammarCorrection(sentence: string): Observable<any> {
    return this._HttpClient.post(
      `/api/GrammarCorrection/correct`,
      { sentence }
    );
  }

  textToSpeech(value: any): Observable<any> {
    return this._HttpClient.post(
      `/api/text-to-speech`,
      value,
      {
        responseType: 'blob',
      }
    );
  }
}
