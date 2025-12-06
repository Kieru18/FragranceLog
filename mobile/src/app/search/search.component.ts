import { AfterViewChecked, Component, NO_ERRORS_SCHEMA, OnDestroy, OnInit } from '@angular/core';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { NativeScriptFormsModule } from '@nativescript/angular';
import { RouterModule, Router } from '@angular/router';
import { PerfumeService } from '../services/perfume.service';
import { BrandService } from '../services/brand.service';
import { GroupService } from '../services/group.service';
import { BehaviorSubject, Subject, takeUntil,
         debounceTime, distinctUntilChanged,
         switchMap, of, tap, catchError, 
         forkJoin} from 'rxjs';
import { Page } from '@nativescript/core';
import { PerfumeSearchResultDto } from '../models/perfumesearchresult.dto';
import { BrandDictionaryItemDto } from '../models/branddictionaryitem.dto';
import { GroupDictionaryItemDto } from '../models/groupdictionaryitem.dto';
import { PerfumeSearchRequestDto } from '../models/perfumesearchrequest.dto';
import { PerfumeSearchResponseDto } from '../models/perfumesearchresponse.dto';
import { CommonService } from '../services/common.service';
import { PerfumeSearchRow } from '../models/types';
import { ElementRef, ViewChild } from '@angular/core';
import { View } from '@nativescript/core';
import { Screen } from '@nativescript/core';


@Component({
  standalone: true,
  selector: 'app-search',
  templateUrl: './search.component.html',
  imports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    ReactiveFormsModule,
    RouterModule
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class SearchComponent implements OnInit, OnDestroy, AfterViewChecked {
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

  filtersReady = false;
  filtersLoading = false;

  private refresh$ = new BehaviorSubject<void>(undefined);
  private destroy$ = new Subject<void>();

  @ViewChild('filterSheet') filterSheetRef!: ElementRef<View>;
  @ViewChild('backdrop') backdropRef!: ElementRef<View>;

  constructor(
    private readonly perfumeService: PerfumeService,
    private readonly brandService: BrandService,
    private readonly groupService: GroupService,
    private readonly router: Router,
    private pageCore: Page,
    private common: CommonService
  ) {
    this.pageCore.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.preloadFilters();

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
            // groupIds: this.selectedGroupIds.length ? this.selectedGroupIds : undefined,
          };

          this.loading = true;

          return this.perfumeService.searchPerfumes(req).pipe(
            tap(() => (this.loading = false)),
            catchError(err => {
              console.error('Search error', err); // @DELETE ME
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

  ngAfterViewChecked(): void {
    if (!this.showFilters) return;
    if (!this.filterSheetRef || !this.backdropRef) return;

    const sheet = this.filterSheetRef.nativeElement;
    const backdrop = this.backdropRef.nativeElement;

    if ((sheet as any).__animated) return;
    (sheet as any).__animated = true;

    const height = Screen.mainScreen.heightDIPs;

    sheet.translateY = height;
    backdrop.opacity = 0;

    sheet.once('layoutChanged', () => {
      backdrop.animate({
        opacity: 0.6,
        duration: 240
      });

      sheet.animate({
        translate: { x: 0, y: 0 },
        duration: 240,
        curve: 'easeOut'
      });
    });
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

  toggleFilters(): void {
    this.showFilters = !this.showFilters;
    if (this.showFilters) {
      this.brandSearchText = '';
      this.groupSearchText = '';
    }
  }

  applyFilters(): void {
    this.closeFilters();
    this.resetAndSearch();
  }

  clearFilters(): void {
    this.selectedBrandId = null;
    this.selectedGroupIds = [];
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

  openFilters(): void {
    if (this.showFilters || !this.filtersReady) return;
    this.showFilters = true;
  }

  closeFilters(): void {
    if (!this.filterSheetRef || !this.backdropRef) {
      this.showFilters = false;
      return;
    }

    const sheet = this.filterSheetRef.nativeElement;
    const backdrop = this.backdropRef.nativeElement;

    const height = require('@nativescript/core').Screen.mainScreen.heightDIPs;

    sheet.animate({
      translate: { x: 0, y: height },
      duration: 180,
      curve: 'easeIn'
    });

    backdrop.animate({
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

    return `${item.rating.toFixed(1)} · ${item.ratingCount} reviews`;
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
}
