import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';

import { Page } from '@nativescript/core';

import { SearchComponent } from '../../search/search.component';
import { FooterComponent } from '../../footer/footer.component';

import { PerfumeService } from '../../services/perfume.service';
import { PerfumeListService } from '../../services/perfumelist.service';
import { BrandService } from '../../services/brand.service';
import { GroupService } from '../../services/group.service';
import { CommonService } from '../../services/common.service';

import { PerfumeSearchResultDto } from '../../models/perfumesearchresult.dto';

@Component({
  standalone: true,
  selector: 'app-lists-add-perfumes',
  templateUrl: './listsaddperfumes.component.html',
  imports: [
    NativeScriptCommonModule,
    NativeScriptFormsModule,
    ReactiveFormsModule,
    FooterComponent
  ],
  schemas: [NO_ERRORS_SCHEMA]
})
export class ListsAddPerfumesComponent extends SearchComponent {

  listId!: number;
  listName: string | null = null;

  addingIds = new Set<number>();
  addedIds = new Set<number>();

  constructor(
    perfumeService: PerfumeService,
    brandService: BrandService,
    groupService: GroupService,
    router: Router,
    pageCore: Page,
    common: CommonService,
    private readonly listsService: PerfumeListService,
    private readonly route: ActivatedRoute
  ) {
    super(
      perfumeService,
      brandService,
      groupService,
      router,
      pageCore,
      common
    );
  }

  override ngOnInit(): void {
    this.listId = Number(this.route.snapshot.paramMap.get('listId'));
    if (!this.listId) {
      this.listName = 'List';
      return;
    }

    this.listsService.getList(this.listId).subscribe({
      next: l => this.listName = l.name,
      error: () => this.listName = 'List'
    });

    this.pageCore.on(Page.navigatedToEvent, () => {
      this.refreshAddedIds();
    });

    super.ngOnInit();
  }

  isAdding(item: PerfumeSearchResultDto): boolean {
    return this.addingIds.has(item.perfumeId);
  }

  isAdded(item: PerfumeSearchResultDto): boolean {
    return this.addedIds.has(item.perfumeId);
  }

  addToList(item: PerfumeSearchResultDto): void {
    const id = item.perfumeId;

    if (this.isAdded(item) || this.isAdding(item)) {
      return;
    }

    this.addingIds.add(id);

    this.listsService.addPerfumeToList(this.listId, id).subscribe({
      next: () => {
        this.addingIds.delete(id);
        this.addedIds.add(id);
      },
      error: err => {
        this.addingIds.delete(id);
      }
    });
  }

  private refreshAddedIds(): void {
    this.listsService.getListPerfumes(this.listId).subscribe({
      next: items => {
        this.addedIds = new Set(items.map(x => x.perfumeId));
      },
      error: () => {
        this.addedIds.clear();
      }
    });
  }
}
