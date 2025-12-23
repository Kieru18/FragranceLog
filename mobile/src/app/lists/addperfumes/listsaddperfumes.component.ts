import { Component, NO_ERRORS_SCHEMA } from '@angular/core';
import { NativeScriptCommonModule, NativeScriptFormsModule } from '@nativescript/angular';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { PerfumeService } from '../../services/perfume.service';
import { PerfumeListService } from '../../services/perfumelist.service';
import { BrandService } from '../../services/brand.service';
import { GroupService } from '../../services/group.service';
import { PerfumeSearchResultDto } from '../../models/perfumesearchresult.dto';
import { CommonService } from '../../services/common.service';
import { Page } from '@nativescript/core';
import { FooterComponent } from '../../footer/footer.component';
import { SearchComponent } from '../../search/search.component';
import { environment } from '../../../environments/environment';

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
  addingIds = new Set<number>();

  constructor(
    perfumeService: PerfumeService,
    brandService: BrandService,
    groupService: GroupService,
    router: Router,
    page: Page,
    common: CommonService,
    private readonly lists: PerfumeListService,
    private readonly route: ActivatedRoute
  ) {
    super(
      perfumeService,
      brandService,
      groupService,
      router,
      page,
      common
    );
  }

  override ngOnInit(): void {
    this.listId = Number(this.route.snapshot.paramMap.get('listId'));
    super.ngOnInit();
  }

  addToList(item: PerfumeSearchResultDto): void {
    if (this.addingIds.has(item.perfumeId)) return;

    this.addingIds.add(item.perfumeId);

    this.lists.addPerfumeToList(this.listId, item.perfumeId)
      .subscribe({
        next: () => {
          this.addingIds.delete(item.perfumeId);
        },
        error: () => {
          this.addingIds.delete(item.perfumeId);
        }
      });
  }

  isAdding(item: PerfumeSearchResultDto): boolean {
    return this.addingIds.has(item.perfumeId);
  }
}
