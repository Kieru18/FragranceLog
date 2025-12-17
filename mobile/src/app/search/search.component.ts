import { Component, NO_ERRORS_SCHEMA, OnDestroy, OnInit } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { RouterModule, Router } from '@angular/router';
import { PerfumeService } from '../services/perfume.service';
import { BrandService } from '../services/brand.service';
import { GroupService } from '../services/group.service';
import {
  BehaviorSubject,
  Subject,
  takeUntil,
  debounceTime,
  distinctUntilChanged,
  switchMap,
  of,
  tap,
  catchError,
  forkJoin
} from 'rxjs';
import { PerfumeSearchResultDto } from '../models/perfumesearchresult.dto';
import { BrandDictionaryItemDto } from '../models/branddictionaryitem.dto';
import { GroupDictionaryItemDto } from '../models/groupdictionaryitem.dto';
import { PerfumeSearchRequestDto } from '../models/perfumesearchrequest.dto';
import { PerfumeSearchResponseDto } from '../models/perfumesearchresponse.dto';
import { CommonService } from '../services/common.service';
import { PerfumeSearchRow } from '../models/types';
import { EventData, View, Screen, Page, Utils, PanGestureEventData } from '@nativescript/core';
import { GenderEnum } from '../enums/gender.enum';
import { FooterComponent } from '../footer/footer.component';
import { environment } from '~/environments/environment';

@Component({
  standalone: true,
  selector: 'app-search',
  templateUrl: './search.component.html',
  imports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    ReactiveFormsModule,
    RouterModule,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SearchComponent implements OnInit, OnDestroy {
  loading = false;
  results: PerfumeSearchRow[] = [];
  totalCount = 0;
  errorMessage: string | null = null;

  private page = 1;
  private readonly pageSize = 25;
  hasMore = true;

  readonly searchControl = new FormControl<string>('', { nonNullable: true });
  selectedBrandId: number | null = null;
  selectedGroupIds: number[] = [];

  brands: BrandDictionaryItemDto[] = [];
  groups: GroupDictionaryItemDto[] = [];

  showFilters = false;
  brandSearchText = '';
  groupSearchText = '';
  selectedGender: GenderEnum | null = null;
  selectedMinRating: number | null = null;
  readonly GenderEnum = GenderEnum;

  filtersReady = false;
  filtersLoading = false;

  private refresh$ = new BehaviorSubject<void>(undefined);
  private destroy$ = new Subject<void>();

  private panStartY = 0;

  private sheetView!: View;
  private backdropView!: View;
  private readonly screenHeight = Screen.mainScreen.heightDIPs;

  private readonly baseUrl = `${environment.contentUrl}`;

  constructor(
    private readonly perfumeService: PerfumeService,
    private readonly brandService: BrandService,
    private readonly groupService: GroupService,
    private readonly router: Router,
    private pageCore: Page,
    private readonly common: CommonService
  ) {
    this.pageCore.actionBarHidden = true;
  }

  ngOnInit(): void {
    setTimeout(() => {
      this.preloadFilters();
    }, 500);

    this.searchControl.valueChanges
      .pipe(
        debounceTime(350),
        distinctUntilChanged(),
        takeUntil(this.destroy$)
      )
      .subscribe(() => {
        this.resetAndSearch();
      });

    this.refresh$
      .pipe(
        takeUntil(this.destroy$),
        debounceTime(50),
        switchMap(() => {
          const req: PerfumeSearchRequestDto = {
            query: this.buildQuery(),
            page: this.page,
            pageSize: this.pageSize,
            brandId: this.selectedBrandId,
            groupIds: this.selectedGroupIds.length ? this.selectedGroupIds : undefined,
            gender: this.selectedGender,
            minRating: this.selectedMinRating
          };

          this.loading = true;

          return this.perfumeService.searchPerfumes(req).pipe(
            tap(() => (this.loading = false)),
            catchError(err => {
              this.loading = false;
              this.errorMessage = `${err.Status} ${err.StatusText}`;
              return of<PerfumeSearchResponseDto>({
                totalCount: 0,
                page: this.page,
                pageSize: this.pageSize,
                items: []
              });
            })
          );
        })
      )
      .subscribe(res => {
        const cleaned = this.results.filter(r => r.type !== 'spinner');

        const newItems = res.items.map(i => ({
          type: 'item' as const,
          data: i
        }));

        this.results = this.page === 1
          ? newItems
          : [...cleaned, ...newItems];

        this.totalCount = res.totalCount;
        this.hasMore = res.items.length === this.pageSize;
        this.loading = false;
      });

    this.resetAndSearch();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  private preloadFilters(): void {
    if (this.filtersReady || this.filtersLoading) return;

    this.filtersLoading = true;

    forkJoin({
      brands: this.brandService.getBrandsDictionary(),
      groups: this.groupService.getGroupsDictionary()
    })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: ({ brands, groups }) => {
          this.brands = brands;
          this.groups = groups;
          this.filtersReady = true;
          this.filtersLoading = false;
        },
        error: () => {
          this.filtersLoading = false;
        }
      });
  }

  private buildQuery(): string | null {
    return (this.searchControl.value || '').trim();
  }

  private resetAndSearch(): void {
    this.page = 1;
    this.results = [];
    this.hasMore = true;
    this.errorMessage = null;
    this.refresh$.next();
  }

  onLoadMore(): void {
    if (!this.hasMore || this.loading) {
      return;
    }

    this.loading = true;

    this.results = [...this.results, { type: 'spinner' }];

    this.page++;
    this.refresh$.next();
  }

  onPerfumeTap(event: any): void {
    const index = event?.index as number;
    const item = this.results[index];
    if (!item || item.type === 'spinner') return;

    this.router.navigate(['/perfume', item.data.perfumeId]);
  }

  selectTemplate(item: PerfumeSearchRow): string {
    return item.type;
  }

  applyFilters(): void {
    this.closeFilters();
    this.resetAndSearch();
  }

  clearFilters(): void {
    this.selectedBrandId = null;
    this.selectedGroupIds = [];
    this.selectedGender = null;
    this.selectedMinRating = null;
    this.closeFilters();
    this.resetAndSearch();
  }

  get filteredBrands(): BrandDictionaryItemDto[] {
    const term = this.brandSearchText?.trim().toLowerCase();
    if (!term) return this.brands;
    return this.brands.filter(b =>
      b.name.toLowerCase().includes(term)
    );
  }

  selectBrand(brand: BrandDictionaryItemDto | null): void {
    this.selectedBrandId = brand ? brand.id : null;
  }

  selectGender(value: GenderEnum | null): void {
    this.selectedGender = value;
  }

  setMinRating(value: number | null): void {
    this.selectedMinRating = value;
  }

  isBrandSelected(brand: BrandDictionaryItemDto): boolean {
    return this.selectedBrandId === brand.id;
  }

  get filteredGroups(): GroupDictionaryItemDto[] {
    const term = this.groupSearchText?.trim().toLowerCase();
    if (!term) return this.groups;
    return this.groups.filter(g =>
      g.name.toLowerCase().includes(term)
    );
  }

  toggleGroup(group: GroupDictionaryItemDto): void {
    const idx = this.selectedGroupIds.indexOf(group.id);
    if (idx >= 0) {
      this.selectedGroupIds.splice(idx, 1);
      this.selectedGroupIds = [...this.selectedGroupIds];
    } else {
      this.selectedGroupIds = [...this.selectedGroupIds, group.id];
    }
  }

  onSheetPan(args: PanGestureEventData): void {
    const sheet = this.pageCore.getViewById<View>('filterSheet');
    if (!sheet) return;

    const screenHeight = Screen.mainScreen.heightDIPs;

    switch (args.state) {
      case 1:
        this.panStartY = sheet.translateY || 0;
        break;

      case 2:
        const newY = Math.max(0, this.panStartY + args.deltaY);
        sheet.translateY = newY;
        break;

      case 3:
      case 4:
        if (sheet.translateY > screenHeight * 0.3) {
          this.closeFilters();
        } else {
          sheet.animate({
            translate: { x: 0, y: 0 },
            duration: 200,
            curve: 'easeOut'
          });
        }
        break;
    }
  }

  onFilterSheetLoaded(args: EventData): void {
    this.sheetView = args.object as View;
    const parent = this.sheetView.parent as View;
    this.backdropView = parent.getViewById('backdrop') as View;

    this.sheetView.translateY = this.screenHeight;
    this.backdropView.opacity = 0;
  }

  openFilters(): void {
    if (this.showFilters || !this.filtersReady) return;
    Utils.dismissSoftInput();
    this.showFilters = true;

    this.sheetView.translateY = this.screenHeight;
    this.backdropView.opacity = 0;

    this.backdropView.animate({
      opacity: 0.6,
      duration: 240
    });

    this.sheetView.animate({
      translate: { x: 0, y: 0 },
      duration: 240,
      curve: 'easeOut'
    });
  }

  closeFilters(): void {
    if (!this.showFilters) return;
    Utils.dismissSoftInput();
    this.sheetView.animate({
      translate: { x: 0, y: this.screenHeight },
      duration: 180,
      curve: 'easeIn'
    });

    this.backdropView.animate({
      opacity: 0,
      duration: 150
    }).then(() => {
      this.showFilters = false;
    });
  }

  isGroupSelected(group: GroupDictionaryItemDto): boolean {
    return this.selectedGroupIds.includes(group.id);
  }

  formatRating(item: PerfumeSearchResultDto): string {
    if (item.rating == null || item.ratingCount === 0) {
      return 'No rating';
    }

    return `${item.rating.toFixed(2)} · ${item.ratingCount} reviews`;
  }

  get filtersSummary(): string {
    const parts: string[] = [];

    if (this.selectedBrandId != null) {
      const brand = this.brands.find(b => b.id === this.selectedBrandId);
      if (brand) parts.push(brand.name);
    }

    if (this.selectedGroupIds.length) {
      const names = this.groups
        .filter(g => this.selectedGroupIds.includes(g.id))
        .map(g => g.name);
      if (names.length) parts.push(`${names.length} groups`);
    }

    return parts.length ? parts.join(' · ') : 'No filters applied';
  }

  getThumbSrc(item: PerfumeSearchResultDto): string {
    const path = (item as any)?.imageUrl as string | undefined;
    if (!path) return '~/assets/images/perfume-placeholder.png';
    return `${this.baseUrl}${path}`;
  }

  onThumbLoaded(e: any) {
    e.object.opacity = 0;
    e.object.animate({ opacity: 1, duration: 200 });
  }
}
