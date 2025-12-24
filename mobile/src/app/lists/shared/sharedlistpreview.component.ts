import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';

import { FooterComponent } from '../../footer/footer.component';
import { SharedListPreviewDto } from '../../models/sharedlistpreview.dto';
import { PerfumeListService } from '../../services/perfumelist.service';
import { SharedListService } from '../../services/sharedlist.service';
import { environment } from '../../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-shared-list-preview',
  templateUrl: './sharedlistpreview.component.html',
  imports: [NativeScriptCommonModule, FooterComponent],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SharedListPreviewComponent implements OnInit {
  loading = false;
  error: string | null = null;

  token!: string;
  data: SharedListPreviewDto | null = null;

  private readonly contentUrl = environment.contentUrl;

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly page: Page,
    private readonly listsService: PerfumeListService,
    private readonly sharedService: SharedListService
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.token = String(this.route.snapshot.paramMap.get('token'));
    if (!this.token) {
      this.error = 'Invalid share link.';
      return;
    }

    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.sharedService
      .getSharedListPreview(this.token)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: data => {
          this.data = data;
        },
        error: (_: HttpErrorResponse) => {
          this.error = 'This shared list is no longer available.';
        }
      });
  }

  getThumbSrc(path?: string | null): string {
    if (!path) {
      return '~/assets/images/perfume-placeholder.png';
    }
    return `${this.contentUrl}${path}`;
  }

  importList(): void {
    // v1: stub – backend endpoint already exists
    // next step will wire POST /shared-lists/{token}/import
    console.log('Import list:', this.token);
  }
}
