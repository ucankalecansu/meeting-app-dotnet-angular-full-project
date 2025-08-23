import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',   // ✅ doğru dosya adı
})
export class RegisterComponent {
  form = { firstName: '', lastName: '', email: '', phone: '', passwordHash: '' };

  onSubmit() {
    console.log('Register data:', this.form);
    // TODO: backend'e gönder
  }
}
