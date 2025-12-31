import { Component, NO_ERRORS_SCHEMA, OnInit, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { SharedListPreviewDto } from '../../models/sharedlistpreview.dto';
import { SharedListService } from '../../services/sharedlist.service';
import { environment } from '../../../environments/environment';
import { RouterExtensions } from '@nativescript/angular';

@Component({
  standalone: true,
  selector: 'app-shared-list-preview',
  templateUrl: './sharedlistpreview.component.html',
  imports: [NativeScriptCommonModule],
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
    private readonly page: Page,
    private readonly sharedService: SharedListService,
    private readonly routerExtensions: RouterExtensions,
    private readonly cdr: ChangeDetectorRef
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.token = String(this.route.snapshot.paramMap.get('token'));
    
    if (!this.token) {
      this.error = 'Invalid share link.';
      return;
    }

    setTimeout(() => {
      this.load();
    }, 0);
  }

  load(): void {
    this.loading = true;
    this.error = null;
    this.cdr.detectChanges();

    this.sharedService
      .getSharedListPreview(this.token)
      .pipe(finalize(() => {
        this.loading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: data => {
          this.data = {
            ...data,
            perfumes: data.perfumes ?? []
          };
          this.cdr.detectChanges();
        },
        error: (_: HttpErrorResponse) => {
          this.error = 'This shared list is no longer available.';
          this.cdr.detectChanges();
        }
      });
  }

  formatRating(item: any): string {
    if (item.avgRating == null || item.ratingCount === 0) {
      return 'No rating';
    }
    return `${item.avgRating.toFixed(2)} · ${item.ratingCount} reviews`;
  }

  getThumbSrc(item: any): string {
    if (!item?.imageUrl) {
      return '~/assets/images/perfume-placeholder.png';
    }
    return `${this.contentUrl}${item.imageUrl}`;
  }

  importList(): void {
    this.sharedService.importSharedList(this.token).subscribe({
      next: (newListId) => {
        this.routerExtensions.navigate(['/lists', newListId], {
          clearHistory: true
        });
      },
      error: () => {
        this.error = 'Failed to import list.';
        this.cdr.detectChanges();
      }
    });
  }

  reject(): void {
    this.routerExtensions.navigate(['/home'], {
      clearHistory: true
    });
  }
}
