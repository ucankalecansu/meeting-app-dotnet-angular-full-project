import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import * as bcrypt from 'bcryptjs';


@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',   // ✅ doğru dosya adı
})
export class RegisterComponent {
  form = { firstName: '', lastName: '', email: '', phone: '', passwordHash: '' };

  onSubmit() {
    const hashedPassword = bcrypt.hashSync(this.form.passwordHash, 10);
    const payload = { ...this.form, passwordHash: hashedPassword };

    console.log('Register data:', payload);
    // TODO: backend'e gönder
  }
}
