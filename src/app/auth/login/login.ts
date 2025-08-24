import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
})
export class LoginComponent {
  form = { email: '', password: '' };

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(loginForm: NgForm) {
    if (loginForm.invalid) return;

    this.auth.login(this.form).subscribe({
      next: (res: any) => {
        // backend token dönüyorsa sakla
        if (res.data.token || res.token ) {
          this.auth.saveToken(res.data.token ? res.data.token : res.token);
          this.router.navigate(['/meetings']); // meetings'e yönlendir
        } else {
          alert('Token alınamadı, login başarısız');
        }
      },
      error: (err) => {
        console.error(err);
        alert('Giriş başarısız');
      },
    });

    // this.router.navigate(['/meetings']); // meetings'e yönlendir

  }
}
