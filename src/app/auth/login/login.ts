import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink], 
  templateUrl: './login.html',
})
export class LoginComponent {
  form = { email: '', passwordHash: '' };

  onSubmit(loginForm: NgForm) {
  if(loginForm.invalid) {
    return;
  }
  console.log('Login data:', this.form);
    // TODO: backend'e gönder
  }
}
