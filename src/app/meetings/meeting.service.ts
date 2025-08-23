import {Injectable} from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';

@Injectable({providedIn: 'root'})
export class MeetingService {
    private apiUrl = 'http://localhost:5000/api/meetings';

    constructor(private http: HttpClient) {}

    getMeetings():Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl);
    }

    createMeeting(data:any):Observable<any> {
        return this.http.post<any>(this.apiUrl,data);
    }

    updateMeeting(id:number,data:any):Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/${id}`,data);
    }

    deleteMeeting(id:number):Observable<any> {
        return this.http.delete<any>(`${this.apiUrl}/${id}`);
    }

}
    
    

