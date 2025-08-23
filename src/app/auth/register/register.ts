import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
})
export class RegisterComponent {
  form = { firstName: '', lastName: '', email: '', phone: '', password: '' };

  constructor(private auth: AuthService, private router: Router) {}

  onSubmit(registerForm: NgForm) {
    if (registerForm.invalid) return;

    this.auth.register(this.form).subscribe({
      next: () => {
        alert('Kayıt başarılı! Şimdi giriş yapabilirsiniz.');
        this.router.navigate(['/login']);
      },
      error: (err) => {
        console.error(err);
        alert('Kayıt başarısız');
      },
    });
  }
}
