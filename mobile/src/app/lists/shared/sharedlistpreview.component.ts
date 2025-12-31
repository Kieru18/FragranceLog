import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { NativeScriptCommonModule, ModalDialogParams, RouterExtensions } from '@nativescript/angular';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { SharedListPreviewDto } from '../../models/sharedlistpreview.dto';
import { SharedListService } from '../../services/sharedlist.service';
import { environment } from '../../../environments/environment';

@Component({
  standalone: true,
  selector: 'app-shared-list-preview',
  templateUrl: './sharedlistpreview.component.html',
  imports: [NativeScriptCommonModule],
  schemas: [NO_ERRORS_SCHEMA],
})
export class SharedListPreviewComponent implements OnInit {
  loading = false;
  error: string | null = null;

  token!: string;
  data: SharedListPreviewDto | null = null;

  private readonly contentUrl = environment.contentUrl;

  constructor(
    private params: ModalDialogParams,
    private sharedService: SharedListService,
    private routerExtensions: RouterExtensions
  ) {
    this.token = this.params.context?.token;
  }

  ngOnInit(): void {
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
          this.data = {
            ...data,
            perfumes: data.perfumes ?? []
          };
        },
        error: (_: HttpErrorResponse) => {
          this.error = 'This shared list is no longer available.';
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
        this.params.closeCallback();
        setTimeout(() => {
          this.routerExtensions.navigate(['/lists', newListId], {
            clearHistory: false,
            animated: true
          });
        }, 50);
      },
      error: () => {
        this.error = 'Failed to import list.';
      }
    });
  }

  reject(): void {
    this.params.closeCallback();
  }
}
