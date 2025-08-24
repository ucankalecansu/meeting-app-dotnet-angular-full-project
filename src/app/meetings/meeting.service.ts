import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Injectable({providedIn: 'root'})
export class MeetingService {
    private apiUrl = 'http://localhost:5001/api/meetings';

    constructor(private http: HttpClient, private auth: AuthService) {}

    private getAuthHeaders() {
        const token = this.auth.getToken();
        return {
          headers: new HttpHeaders({
            Authorization: `Bearer ${token}`
          })
        };
      }

    getMeetings():Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl,this.getAuthHeaders());
    }

    createMeeting(data:any):Observable<any> {
        return this.http.post<any>(this.apiUrl,data,this.getAuthHeaders());
    }

    updateMeeting(id:number,data:any):Observable<any> {
        return this.http.put<any>(`${this.apiUrl}/${id}`,data,this.getAuthHeaders());
    }

    deleteMeeting(id:number):Observable<any> {
        return this.http.delete<any>(`${this.apiUrl}/${id}`,this.getAuthHeaders());
    }
    cancelMeeting(id:number):Observable<any> {
        return this.http.post<any>(`${this.apiUrl}/${id}/cancel`,this.getAuthHeaders());
    }

}
    
    

