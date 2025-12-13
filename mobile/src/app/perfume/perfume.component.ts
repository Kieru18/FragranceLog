import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { PerfumeService } from '../services/perfume.service';
import { PerfumeDetailsDto } from '../models/perfumedetails.dto';
import { NoteTypeEnum } from '../models/notetype.enum';
import { FooterComponent } from '../footer/footer.component';
import { GROUP_COLORS } from '../const/GROUP_COLORS'
import { environment } from '~/environments/environment';

@Component({
  standalone: true,
  selector: 'app-perfume',
  templateUrl: './perfume.component.html',
  imports: [
    NativeScriptCommonModule,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class PerfumeComponent implements OnInit {
  loading = true;
  details: PerfumeDetailsDto | null = null;

  private readonly baseUrl = `${environment.contentUrl}`;

  constructor(
    private route: ActivatedRoute,
    private perfumeService: PerfumeService,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (!id) return;

    this.perfumeService.getPerfumeDetails(id).subscribe({
      next: d => {
        this.details = d;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  get perfumeImageSrc(): string {
    const path = this.details?.imageUrl;

    if (!path) {
      return '~/assets/images/perfume-placeholder.png';
    }

    return `${this.baseUrl}${path}`;
  }

  onImageLoaded(e: any) {
    e.object.opacity = 0;
    e.object.animate({ opacity: 1, duration: 250 });
  }

  noteTypeLabel(type: NoteTypeEnum): string {
    switch (type) {
      case NoteTypeEnum.Top: return 'Top notes';
      case NoteTypeEnum.Middle: return 'Heart notes';
      case NoteTypeEnum.Base: return 'Base notes';
      default: return '';
    }
  }

  getGroupColor(name: string): string {
    return GROUP_COLORS[name] ?? '#444444';
  }
}
