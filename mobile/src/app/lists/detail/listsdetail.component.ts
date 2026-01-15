import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { Page } from '@nativescript/core';
import { finalize } from 'rxjs/operators';
import { HttpErrorResponse } from '@angular/common/http';
import { FooterComponent } from '../../footer/footer.component';
import { PerfumeListItemDto } from '../../models/perfumelistitem.dto';
import { PerfumeListService } from '../../services/perfumelist.service';
import { environment } from '../../../environments/environment';
import * as SocialShare from '@nativescript/social-share';


@Component({
  standalone: true,
  selector: 'app-lists-detail',
  templateUrl: './listsdetail.component.html',
  imports: [NativeScriptCommonModule, FooterComponent],
  schemas: [NO_ERRORS_SCHEMA]
})
export class ListsDetailComponent implements OnInit {
  loading = false;
  error: string | null = null;

  listId!: number;

  listName: string | null = null;

  items: PerfumeListItemDto[] = [];

  private readonly contentUrl = environment.contentUrl;
  private readonly redirectUrl = environment.redirectUrl;

  private removingIds = new Set<number>();

  constructor(
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    private readonly page: Page,
    private readonly listsService: PerfumeListService
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.listId = Number(this.route.snapshot.paramMap.get('id'));
    if (!this.listId) return;

    this.listsService.getList(this.listId).subscribe({
      next: l => this.listName = l.name,
      error: () => this.listName = 'List'
    });

    this.page.on(Page.navigatedToEvent, () => { this.loadSilent(); });

    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.listsService
      .getListPerfumes(this.listId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: data => {
          this.items = data ?? [];
        },
        error: (err: HttpErrorResponse) => {
          this.error = 'Failed to load list perfumes.';
        }
      });
  }

  loadSilent(): void {
    this.error = null;

    this.listsService
      .getListPerfumes(this.listId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: data => {
          this.items = data ?? [];
        },
        error: (err: HttpErrorResponse) => {
          this.error = 'Failed to load list perfumes.';
        }
      });
  }

  shareList(): void {
    if (!this.listId) return;

    this.listsService.shareList(this.listId).subscribe({
      next: dto => {
        const url = `${this.redirectUrl}/?token=${dto.shareToken}`;

        SocialShare.shareText(
          `Check out my FragranceLog perfume collection:\n${url}`,
          'Share perfume list'
        );
      },
      error: () => {
        this.error = 'Failed to share list.';
      }
    });
  }

  formatRating(item: PerfumeListItemDto): string {
    if (item.avgRating == null || item.ratingCount === 0) {
      return 'No rating';
    }

    return `${item.avgRating.toFixed(2)} · ${item.ratingCount} reviews`;
  }

  getThumbSrc(item: PerfumeListItemDto): string {
    const path = (item as any)?.imageUrl as string | undefined;
    if (!path) return '~/assets/images/perfume-placeholder.png';
    return `${this.contentUrl}${path}`;
  }

  onPerfumeTap(item: PerfumeListItemDto): void {
    if (!item?.perfumeId) return;

    this.router.navigate(['/perfume', item.perfumeId]);
  }

  removeFromList(item: PerfumeListItemDto): void {
    if (!item?.perfumeId) return;
    if (this.removingIds.has(item.perfumeId)) return;

    this.removingIds.add(item.perfumeId);

    const backup = [...this.items];
    this.items = this.items.filter(x => x.perfumeId !== item.perfumeId);

    this.listsService.removePerfumeFromList(this.listId, item.perfumeId).subscribe({
      next: () => {
        this.removingIds.delete(item.perfumeId);
      },
      error: (err: HttpErrorResponse) => {
        console.log(err);
        this.items = backup;
        this.removingIds.delete(item.perfumeId);
      }
    });
  }

  openAddMode(): void {
    this.router.navigate(['/lists', this.listId, 'add']);
  }
}
