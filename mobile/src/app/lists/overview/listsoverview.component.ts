import { Component, NO_ERRORS_SCHEMA, OnInit } from '@angular/core';
import { NativeScriptCommonModule } from '@nativescript/angular';
import { RouterModule } from '@angular/router';
import { finalize } from 'rxjs/operators';
import { Page, Utils } from '@nativescript/core';
import { action, confirm, prompt } from '@nativescript/core/ui/dialogs';
import { HttpErrorResponse } from '@angular/common/http';
import { PerfumeListService } from '../../services/perfumelist.service';
import { PerfumeListOverviewDto } from '../../models/perfumelistoverview.dto';

type PreviewSlot = { path: string | null };

@Component({
  standalone: true,
  selector: 'app-lists-overview',
  templateUrl: './lists-overview.component.html',
  imports: [NativeScriptCommonModule, RouterModule],
  schemas: [NO_ERRORS_SCHEMA]
})
export class ListsOverviewComponent implements OnInit {
  loading = false;
  error: string | null = null;

  items: PerfumeListOverviewDto[] = [];

  constructor(
    private readonly lists: PerfumeListService,
    private page: Page
  ) {
    this.page.actionBarHidden = true;
  }

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.error = null;

    this.lists.getListsOverview()
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: (data: PerfumeListOverviewDto[]) => {
          this.items = (data ?? []).slice().sort((a, b) => {
            const sys = (a.isSystem === b.isSystem) ? 0 : (a.isSystem ? -1 : 1);
            if (sys !== 0) return sys;
            return (a.name ?? '').localeCompare(b.name ?? '');
          });
        },
        error: (err: HttpErrorResponse) => {
          console.log(err);
          this.error = 'Failed to load lists.';
        }
      });
  }

  async createList(): Promise<void> {
    Utils.dismissSoftInput();

    const res = await prompt({
      title: 'Create list',
      message: 'Enter a name for your new list',
      okButtonText: 'Create',
      cancelButtonText: 'Cancel',
      defaultText: '',
      inputType: 'text'
    });

    if (!res.result) return;

    const name = (res.text ?? '').trim();
    if (!name) return;

    this.loading = true;
    this.error = null;

    this.lists.createList(name)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.load(),
        error: (err: HttpErrorResponse) => {
          console.log(err);
          this.error = 'Failed to create list.';
        }
      });
  }

  async openListMenu(item: PerfumeListOverviewDto): Promise<void> {
    if (item.isSystem) return;

    const choice = await action({
      title: item.name,
      cancelButtonText: 'Cancel',
      actions: ['Rename', 'Delete']
    });

    if (choice === 'Rename') {
      await this.renameList(item);
      return;
    }

    if (choice === 'Delete') {
      await this.deleteList(item);
    }
  }

  private async renameList(item: PerfumeListOverviewDto): Promise<void> {
    Utils.dismissSoftInput();

    const res = await prompt({
      title: 'Rename list',
      message: 'Enter a new name',
      okButtonText: 'Save',
      cancelButtonText: 'Cancel',
      defaultText: item.name ?? '',
      inputType: 'text'
    });

    if (!res.result) return;

    const name = (res.text ?? '').trim();
    if (!name || name === item.name) return;

    this.loading = true;
    this.error = null;

    this.lists.renameList(item.perfumeListId, name)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.load(),
        error: (err: HttpErrorResponse) => {
          console.log(err);
          this.error = 'Failed to rename list.';
        }
      });
  }

  private async deleteList(item: PerfumeListOverviewDto): Promise<void> {
    const ok = await confirm({
      title: 'Delete list',
      message: `Delete "${item.name}"? This cannot be undone.`,
      okButtonText: 'Delete',
      cancelButtonText: 'Cancel'
    });

    if (!ok) return;

    this.loading = true;
    this.error = null;

    this.lists.deleteList(item.perfumeListId)
      .pipe(finalize(() => (this.loading = false)))
      .subscribe({
        next: () => this.load(),
        error: (err: HttpErrorResponse) => {
          console.log(err);
          this.error = 'Failed to delete list.';
        }
      });
  }

  previewSlots(item: PerfumeListOverviewDto): PreviewSlot[] {
    const imgs = (item.previewImages ?? []).filter(x => !!x);
    const slots: PreviewSlot[] = [];

    for (let i = 0; i < 4; i++) {
      slots.push({ path: imgs[i] ?? null });
    }

    return slots;
  }

  trackByListId(_: number, item: PerfumeListOverviewDto): number {
    return item.perfumeListId;
  }

  trackByIndex(i: number): number {
    return i;
  }
}
