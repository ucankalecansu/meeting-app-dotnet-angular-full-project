import { Component } from '@angular/core';
import { FormsModule, NgForm } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import * as bcrypt from 'bcryptjs';

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
  const hashedPassword = bcrypt.hashSync(this.form.passwordHash, 10);
  const payload = { ...this.form, passwordHash: hashedPassword };

  console.log('Login data:', payload);
    // TODO: backend'e gönder
  }
}
