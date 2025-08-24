import {Injectable} from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import { AuthService } from '../auth/auth.service';

@Injectable({providedIn: 'root'})
export class UserService {
    private apiUrl = 'http://localhost:5001/api/users';

    constructor(private http: HttpClient, private auth: AuthService) {}

    private getAuthHeaders() {
        const token = this.auth.getToken();
        return {
          headers: new HttpHeaders({
            Authorization: `Bearer ${token}`
          })
        };
      }

    getUsers():Observable<any[]> {
        return this.http.get<any[]>(this.apiUrl,this.getAuthHeaders());
    }
}