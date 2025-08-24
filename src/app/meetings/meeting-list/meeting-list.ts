import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, NgForm } from '@angular/forms';
import { MeetingService } from '../meeting.service';
import { UserService } from '../../user/user.service';

@Component({
  selector: 'app-meeting-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './meeting-list.html',
  styleUrls: ['./meeting-list.scss']
})
export class MeetingListComponent implements OnInit {
  meetings: any[] = [];
  loading = false;

  // Pagination için gerekli değişkenler
  currentPage = 1;
  pageSize = 5;
  totalItems = 0;

  // kullanıcı listesi
  users: any[] = [];
  usersLoading = false;
  userFilter = '';                          // dropdown içinde arama
  userLabelMap: Record<string, string> = {}; // email -> "Ad Soyad (email)"

  // popup state
  showModal = false;
  editingId: number | null = null;

  // form modeli
  form = {
    title: '',
    description: '',
    startDate: '',
    endDate: '',
    participants: [] as string[],   // dropdown çoklu seçim (email[] )
    status: ''                      // 'active' | 'cancelled' | ''
  };

  isParticipantsOpen = false;

toggleParticipants() { 
  this.isParticipantsOpen = !this.isParticipantsOpen; 
  // Açıldığında kullanıcıları yükle
  if (this.isParticipantsOpen && this.users.length === 0) {
    this.fetchUsers();
  }
}

closeParticipants() { 
  this.isParticipantsOpen = false; 
}

  constructor(
    private meetingService: MeetingService,
    private userService: UserService,
  ) {}

  ngOnInit(): void {
    this.fetchMeetings();
  }

  // --- Meetings ---
  fetchMeetings() {
    this.loading = true;
    this.meetingService.getMeetings().subscribe({
      next: (data) => {
        this.meetings = data;
        this.totalItems = data.length;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        alert('Toplantılar yüklenemedi');
      }
    });
  }

  get paginatedMeetings(): any[] {
    const startItem = (this.currentPage - 1) * this.pageSize;
    const endItem = this.currentPage * this.pageSize;
    return this.meetings.slice(startItem, endItem);
  }

  get totalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  changePage(page: number): void {
    this.currentPage = page;
  }

  nextPage(): void {
    if (this.currentPage < this.totalPages) {
      this.currentPage++;
    }
  }

  prevPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
    }
  }

  deleteMeeting(id: number) {
    if (!confirm('Silmek istediğine emin misin?')) return;
    this.meetingService.cancelMeeting(id).subscribe({
      next: () => this.fetchMeetings(),
      error: () => alert('Silme hatası')
    });
  }

  // --- Users (dropdown için) ---
  fetchUsers() {
    this.usersLoading = true;
    this.userService.getUsers().subscribe({
      next: (data:any) => {
        this.users = data.data || [];
        // email -> "Ad Soyad (email)" map
        this.userLabelMap = {};
        for (const u of this.users) {
          this.userLabelMap[u.email] = `${u.firstName} ${u.lastName} (${u.email})`;
        }
        this.usersLoading = false;
      },
      error: () => {
        this.usersLoading = false;
        alert('Kullanıcılar yüklenemedi');
      }
    });
  }

  get filteredUsers() {
    const q = this.userFilter.toLowerCase().trim();
    if (!q) return this.users;
    return this.users.filter((u: any) =>
      (`${u.firstName} ${u.lastName} ${u.email}`).toLowerCase().includes(q)
    );
  }

  isSelected(email: string) {
    return (this.form.participants || []).includes(email);
  }

  toggleParticipant(email: string, checked: boolean) {
    const set = new Set(this.form.participants || []);
    if (checked) set.add(email); else set.delete(email);
    this.form.participants = Array.from(set);
  }

  mapParticipantsToLabels(p: string | string[] | null | undefined): string {
    if (!p) return '';
    const list = Array.isArray(p) ? p : String(p).split(',').map(x => x.trim()).filter(Boolean);
    return list.map(e => this.userLabelMap[e] || e).join(', ');
  }

  // --- Modal Aç/Kapat ---
  openCreate() {
    this.editingId = null;
    this.form = {
      title: '',
      description: '',
      startDate: '',
      endDate: '',
      participants: [],
      status: ''
    };
    this.userFilter = '';
    this.showModal = true;
    this.fetchUsers();
  }

  openEdit(meeting: any) {
    this.editingId = meeting.id;
    this.form = {
      title: meeting.title,
      description: meeting.description,
      startDate: meeting.startDate ? String(meeting.startDate).slice(0,16) : '',
      endDate: meeting.endDate ? String(meeting.endDate).slice(0,16) : '',
      participants: meeting.participants
        ? String(meeting.participants).split(',').map((x: string) => x.trim()).filter(Boolean)
        : [],
      status: meeting.status || ''
    };
    this.userFilter = '';
    this.showModal = true;
    this.fetchUsers();
  }

  closeModal() {
    this.showModal = false;
  }

  // --- Kaydet (Create/Update) ---
  save(formRef: NgForm) {
    if (formRef.invalid) return;

    // API string bekliyorsa join
    const payload = {
      ...this.form,
      participants: [(this.form.participants || []).join(',')]
    };

    if (this.editingId == null) {
      this.meetingService.createMeeting(payload).subscribe({
        next: () => {
          this.closeModal();
          this.fetchMeetings();
        },
        error: () => alert('Ekleme hatası')
      });
    } else {
      this.meetingService.updateMeeting(this.editingId, payload).subscribe({
        next: () => {
          this.closeModal();
          this.fetchMeetings();
        },
        error: () => alert('Güncelleme hatası')
      });
    }
  }
}
